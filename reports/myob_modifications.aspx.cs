using System;
using System.Data;
using System.IO;
using System.Web.UI;

namespace Paymaker {

    public partial class commission_statement_finalize : Root {
        private bool blnHasData = false;

        protected void Page_Load(object sender, System.EventArgs e) {
            if (!Page.IsPostBack) {
                loadFilters();
            }
        }

        protected void loadFilters() {
            string szSQL = string.Format("select ID, STARTDATE , ENDDATE, '' AS NAME from PAYPERIOD ORDER BY ID DESC");
            DataSet ds = DB.runDataSet(szSQL);

            foreach (DataRow dr in ds.Tables[0].Rows) {
                dr["NAME"] = Utility.formatDate(dr["StartDate"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
            }
            Utility.BindList(ref lstPayPeriod, ds, "ID", "NAME");
        }

        protected DataSet loadData(int CompanyID = -1) {
            string szFilter = "";
            if (CompanyID != -1)
                szFilter = " AND  L_OFF.COMPANYID = " + CompanyID;
            DateTime dtStart = DateTime.MaxValue;
            DateTime dtEnd = DateTime.MaxValue;
            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(lstPayPeriod.SelectedValue));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }
            string szSQL = string.Format(@"
                SELECT '' as JOURNALNUMBER,  '' AS TXDATE, '' as MEMO, 'P' AS GST, '0' AS INCLUSIVE, '' AS ACCOUNTNUMBER, '' AS DEBITEXGST, '' AS DEBITINCGST,
                    '' AS CREDITEXGST, '' AS CREDITINCGST, '' as JOB, 'N-T' as TAXCODE, '' as STOP,
                   U.CREDITGLCODE AS USERCREDITGLCODE, U.DEBITGLCODE AS USERDEBITGLCODE, USS.CALCULATEDAMOUNT as AMOUNT, l_OFF.JOBCODE, S.SALEDATE, S.ID,
                   USS.recordstatus AS USS_STATUS, USS.ID AS USERSALESPITID
                FROM SALE S
                JOIN SALESPLIT SS ON S.ID =SS.SALEID
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID
                JOIN DB_USER U  ON USS.USERID = U.ID
                JOIN LIST L_COMM ON L_COMM.ID = SS.COMMISSIONTYPEID
                JOIN LIST L_OFF ON L_OFF.ID = USS.OFFICEID
                JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID
                WHERE USS.recordstatus IN (-1,  1)  AND S.SALEDATE BETWEEN '{1} 00:00:00' AND '{2} 23:59:59'
                {0}
                ORDER BY S.SALEDATE, SS.ID, USS.ID ", szFilter, Utility.formatDate(dtStart), Utility.formatDate(dtEnd));
            return DB.runDataSet(szSQL);
        }

        private DataSet getCompanyList() {
            string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);
            return DB.runDataSet(szSQL);
        }

        protected void btnFinalize_Click(object sender, EventArgs e) {
            DataSet dsCompany = getCompanyList();
            foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
                string szFileName = Utility.formatDate(DateTime.Now) + "-MYOB-Commission-Exception-" + drCompany["NAME"] + ".csv";
                processCompany(Convert.ToInt32(drCompany["ID"]), szFileName);
                szFileName = Utility.formatDate(DateTime.Now) + "-MYOB-Commission-Exception-ADVANCED-" + drCompany["NAME"] + ".csv";              
                processCompanyAdvanced(Convert.ToInt32(drCompany["ID"]), szFileName);
            }
        }

        private void processCompany(int CompanyID, string FileName) {
            DataSet ds = loadData(CompanyID);
            if (ds.Tables[0].Rows.Count == 0)
                return;
          
            blnHasData = true;

            DataTable dtNew = ds.Tables[0].Clone();
            string szJournalNumber = "999999";
            foreach (DataRow tx in ds.Tables[0].Rows) {
                dtNew.ImportRow(tx);
                DataRow rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                bool blnReverse = Convert.ToInt32(rCurr["USS_STATUS"]) == 1;
                //Debit
                rCurr["JOURNALNUMBER"] = szJournalNumber;
                rCurr["TXDATE"] = Utility.formatDateForMYOBExport(rCurr["SALEDATE"].ToString());
                rCurr["ACCOUNTNUMBER"] = rCurr["USERDEBITGLCODE"];
                if (blnReverse) {
                    rCurr["CREDITEXGST"] = rCurr["AMOUNT"];
                    rCurr["CREDITINCGST"] = rCurr["AMOUNT"];
                } else {
                    rCurr["DEBITEXGST"] = rCurr["AMOUNT"];
                    rCurr["DEBITINCGST"] = rCurr["AMOUNT"];
                }
                rCurr["JOB"] = rCurr["JOBCODE"];

                //Do the same for the inverse of this transaction
                //Credit
                dtNew.ImportRow(tx);
                rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                rCurr["JOURNALNUMBER"] = szJournalNumber;
                rCurr["TXDATE"] = Utility.formatDateForMYOBExport(rCurr["SALEDATE"].ToString());
                rCurr["ACCOUNTNUMBER"] = rCurr["USERCREDITGLCODE"];
                if (blnReverse) {
                    rCurr["DEBITEXGST"] = rCurr["AMOUNT"];
                    rCurr["DEBITINCGST"] = rCurr["AMOUNT"];
                } else {
                    rCurr["CREDITEXGST"] = rCurr["AMOUNT"];
                    rCurr["CREDITINCGST"] = rCurr["AMOUNT"];
                }
                rCurr["JOB"] = rCurr["JOBCODE"];
                if (blnReverse) {
                    string szSQL = "UPDATE USERSALESPLIT SET RECORDSTATUS = 2 WHERE ID = " + rCurr["USERSALESPITID"].ToString();
                    DB.runNonQuery(szSQL);
                }
            }

            dtNew.AcceptChanges();
            //Remove the datacolumns that are beyond the STOP in dtNew
            int intSTOPColumnOrdinal = 0;
            foreach (DataColumn dc in dtNew.Columns) {
                if (dc.ColumnName == "STOP") {
                    intSTOPColumnOrdinal = dc.Ordinal;
                }
            }
            while (dtNew.Columns.Count > intSTOPColumnOrdinal) {
                dtNew.Columns.RemoveAt(intSTOPColumnOrdinal);
            }
            string szPath = Path.Combine(G.Settings.MYOBDir, FileName);
            if (File.Exists(szPath))
                File.Delete(szPath);
            StreamWriter output = new StreamWriter(szPath);
            output.Write(CsvWriter.WriteToString(dtNew, true, false));
            output.Flush();
            output.Close();
            oLinks.InnerHtml += string.Format("<a href='../admin/myob_doc.aspx?file={0}' target='_blank'>{1}</a> <br/>", Server.UrlEncode(FileName), FileName);

            if (!blnHasData) {
                G.Notifications.addPageNotification(PageNotificationType.Warning, "No changes recorded", "There are no changed recorded for the seleected period.");
                G.Notifications.showPageNotification();
            }
        }

        private void processCompanyAdvanced(int CompanyID, string FileName) {
            DataSet ds = loadDataAdvanced(CompanyID);
            if (ds.Tables[0].Rows.Count == 0)
                return;
            if (File.Exists(FileName))
                File.Delete(FileName);
            blnHasData = true;

            DataTable dtNew = ds.Tables[0].Clone();
            foreach (DataRow tx in ds.Tables[0].Rows) {
                dtNew.ImportRow(tx);
                DataRow rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                bool blnReverse = Convert.ToInt32(rCurr["USS_STATUS"]) == 1;
                //Debit

                rCurr["Ref. Number"] = Utility.formatDateForMYOBExport(rCurr["SALEDATE"].ToString());
                rCurr["ACCOUNT"] = rCurr["USERDEBITGLCODE"];
                if (blnReverse) {
                    rCurr["CREDIT AMOUNT"] = rCurr["AMOUNT"];
                } else {
                    rCurr["DEBIT AMOUNT"] = rCurr["AMOUNT"];
                }

                //Do the same for the inverse of this transaction
                //Credit
                dtNew.ImportRow(tx);
                rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                rCurr["REf. Number"] = Utility.formatDateForMYOBExport(rCurr["SALEDATE"].ToString());
                rCurr["ACCOUNT"] = rCurr["USERCREDITGLCODE"];
                if (blnReverse) {
                    rCurr["DEBIT AMOUNT"] = rCurr["AMOUNT"];
                } else {
                    rCurr["CREDIT AMOUNT"] = rCurr["AMOUNT"];
                }
                if (blnReverse) {
                    string szSQL = "UPDATE USERSALESPLIT SET RECORDSTATUS = 2 WHERE ID = " + rCurr["USERSALESPITID"].ToString();
                    DB.runNonQuery(szSQL);
                }
            }

            dtNew.AcceptChanges();
            //Remove the datacolumns that are beyond the STOP in dtNew
            int intSTOPColumnOrdinal = 0;
            foreach (DataColumn dc in dtNew.Columns) {
                if (dc.ColumnName == "STOP") {
                    intSTOPColumnOrdinal = dc.Ordinal;
                }
            }
            while (dtNew.Columns.Count > intSTOPColumnOrdinal) {
                dtNew.Columns.RemoveAt(intSTOPColumnOrdinal);
            }
            string szPath = Path.Combine(G.Settings.MYOBDir, FileName);

            if (File.Exists(szPath))
                File.Delete(szPath);

            StreamWriter output = new StreamWriter(szPath);
            output.Write(CsvWriter.WriteToString(dtNew, true, false));
            output.Flush();
            output.Close();
            oLinks.InnerHtml += string.Format("<a href='../admin/myob_doc.aspx?file={0}' target='_blank'>{1}</a> <br/>", Server.UrlEncode(FileName), FileName);

            if (!blnHasData) {
                G.Notifications.addPageNotification(PageNotificationType.Warning, "No changes recorded", "There are no changed recorded for the seleected period.");
                G.Notifications.showPageNotification();
            }
        }

        protected DataSet loadDataAdvanced(int CompanyID = -1) {
            string szFilter = "";
            if (CompanyID != -1)
                szFilter = " AND  L_OFF.COMPANYID = " + CompanyID;
            DateTime dtStart = DateTime.MaxValue;
            DateTime dtEnd = DateTime.MaxValue;
            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(lstPayPeriod.SelectedValue));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }
            string szSQL = string.Format(@"
                SELECT L_COMP.OFFICEMYOBBRANCH as Branch, '2-3000' AS Account, L_COMP.OFFICEMYOBCODE + '-' + u.INITIALSCODE  AS Subaccount,
                    '' as [Debit Amount], '' as [Credit Amount], '' as [Ref. Number], '' AS [Transaction Description], '' as STOP,
                    U.CREDITGLCODE AS USERCREDITGLCODE, U.DEBITGLCODE AS USERDEBITGLCODE, USS.CALCULATEDAMOUNT as AMOUNT, l_OFF.JOBCODE, S.SALEDATE, S.ID,
                   USS.recordstatus AS USS_STATUS, USS.ID AS USERSALESPITID
                FROM SALE S
                JOIN SALESPLIT SS ON S.ID =SS.SALEID
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID
                JOIN DB_USER U  ON USS.USERID = U.ID
                JOIN LIST L_COMM ON L_COMM.ID = SS.COMMISSIONTYPEID
                JOIN LIST L_OFF ON L_OFF.ID = USS.OFFICEID
                JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID
                WHERE USS.recordstatus IN (-1,  1)  AND S.SALEDATE BETWEEN '{1} 00:00:00' AND '{2} 23:59:59'
                {0}
                ORDER BY S.SALEDATE, SS.ID, USS.ID ", szFilter, Utility.formatDate(dtStart), Utility.formatDate(dtEnd));
            return DB.runDataSet(szSQL);
        }
    }
}