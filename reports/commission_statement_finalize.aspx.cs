using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class commission_statement_finalize : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            if (!Page.IsPostBack)
                loadFilters();
        }

        protected void loadFilters() {
            string szSQL = string.Format("select ID, STARTDATE , ENDDATE, '' AS NAME from PAYPERIOD ORDER BY ID DESC");
            DataSet ds = DB.runDataSet(szSQL);

            foreach (DataRow dr in ds.Tables[0].Rows) {
                dr["NAME"] = Utility.formatDate(dr["StartDate"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
            }
            Utility.BindList(ref lstPayPeriod, ds, "ID", "NAME");

            Utility.BindList(ref lstCompany, DB.runDataSet(string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company)), "ID", "NAME");
            lstCompany.Items.Insert(0, new ListItem("All", ""));
        }

        protected void btnFinalize_Click(object sender, EventArgs e) {
            createRecords(true);
        }

        private void createRecords(bool UpdateDB) {
            string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);
            DataSet dsCompany = DB.runDataSet(szSQL);
            DateTime dtStart = DateTime.MaxValue;
            DateTime dtEnd = DateTime.MaxValue;
            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(lstPayPeriod.SelectedValue));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }
            int intMYOBExportID = 0;
            DataTable dtTotal = null;
            foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
                if (lstCompany.SelectedValue != "" && Convert.ToInt32(lstCompany.SelectedValue) != DB.readInt(drCompany["ID"]))
                    continue;
                string szFileName = Utility.formatDate(DateTime.Now) + "-MYOB-Commission-" + drCompany["NAME"] + ".csv";
                if (UpdateDB) {
                    intMYOBExportID = Utility.getMYOBExportID(ExportType.SalesCommission, szFileName);
                }
                string szPath = G.Settings.MYOBDir + szFileName;

                szSQL = string.Format(@"
                   -- 0) All normal commissions
                   SELECT '' as JOURNALNUMBER, '' AS TXDATE, '' as MEMO, 'P' AS GST, '0' AS INCLUSIVE, '' AS ACCOUNTNUMBER,
                        '' AS DEBITEXGST, '' AS DEBITINCGST,
                        '' AS CREDITEXGST, '' AS CREDITINCGST, '' as JOB, 'N-T' as TAXCODE, '' as STOP,
                        REPLACE(U.CREDITGLCODE, '-', '') AS USERCREDITGLCODE, REPLACE(U.DEBITGLCODE, '-', '') AS USERDEBITGLCODE, USS.ACTUALPAYMENT as AMOUNT, l_OFF.JOBCODE,
                        S.ID, 0 AS TOTALBONUS,  S.SALEDATE as ACTUALDATE
                    FROM SALE S
                    JOIN SALESPLIT SS ON S.ID = SS.SALEID AND SS.RECORDSTATUS = 0
                    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                    JOIN DB_USER U  ON USS.USERID = U.ID
                    JOIN LIST L_COMM ON L_COMM.ID = SS.COMMISSIONTYPEID
                    JOIN LIST L_OFF ON L_OFF.ID = USS.OFFICEID
                    JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID AND L_OFF.COMPANYID = {0}
                    AND S.SALEDATE BETWEEN '{1} 00:00:00' AND '{2} 23:59:59' AND S.STATUSID IN (1, 2) AND USS.ACTUALPAYMENT > 0
                    ORDER BY S.SALEDATE;

                ", drCompany["ID"], Utility.formatDate(dtStart), Utility.formatDate(dtEnd), oP.ID);

                DataSet ds = DB.runDataSet(szSQL);

                if (File.Exists(szPath))
                    File.Delete(szPath);
                DataTable dtNew = ds.Tables[0].Clone();
                if (dtTotal == null)
                    dtTotal = ds.Tables[0].Clone(); ;

                string szJournalNumber = "999999";
                string szFilter = lstRecords.SelectedValue;
                Response.Write("*" + szFilter + "*");
                if (szFilter == "") {
                    //Process the commissions
                    processTable(ds.Tables[0].DefaultView, dtNew, szJournalNumber, "COMM", intMYOBExportID, UpdateDB);
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
                if (UpdateDB) {
                    StreamWriter output = new StreamWriter(szPath);
                    output.Write(CsvWriter.WriteToString(dtNew, true, false));
                    output.Flush();
                    output.Close();
                    oLinks.InnerHtml += string.Format("<a href='../admin/myob_doc.aspx?file={0}' target='_blank'>{1}</a> <br/>", Server.UrlEncode(szFileName), szFileName);
                } else {
                    //Add these rows to the overalloutput so we can see the data
                    foreach (DataRow dr in dtNew.Rows) {
                        dtTotal.Rows.Add(dr.ItemArray);
                    }
                }
            }
            if (!UpdateDB) {
                gvData.DataSource = dtTotal;
                gvData.DataBind();
                gvData.Visible = true;
                HTML.formatGridView(ref gvData, true);
            }
        }

        private void processTable(DataView dt, DataTable dtNew, string szJournalNumber, string Type, int MYOBExportID, bool UpdateDB) {
            foreach (DataRowView tx in dt) {
                dtNew.ImportRow(tx.Row);
                DataRow rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                double dCreditExGST = 0;
                double dCreditIncGST = 0;
                double dDebitExGST = 0;
                double dDebitIncGST = 0;

                string szAccount = DB.readString(rCurr["USERDEBITGLCODE"]);
                string szJobCode = DB.readString(rCurr["JOBCODE"]);
                if (Type == "SALESBONUSSCHEME") {
                    dCreditExGST = DB.readDouble(rCurr["TOTALBONUS"]);
                    dCreditIncGST = DB.readDouble(rCurr["TOTALBONUS"]);
                } else if (Type == "JUNIORTOSENIOR") {
                    // Debit the juniors 2 account and credit the 5 account
                    dCreditExGST = DB.readDouble(rCurr["AMOUNT"]);
                    dCreditIncGST = DB.readDouble(rCurr["AMOUNT"]);
                } else {
                    dDebitExGST = DB.readDouble(rCurr["AMOUNT"]);
                    dDebitIncGST = DB.readDouble(rCurr["AMOUNT"]);
                }
                DateTime dtTX = DB.readDate(rCurr["ACTUALDATE"]);
                //Debit
                createDataRow(ref rCurr, szJournalNumber, dtTX, szAccount, dDebitExGST, dDebitIncGST, dCreditExGST, dCreditIncGST, szJobCode);

                //Do the same for the inverse of this transaction
                //Credit
                dtNew.ImportRow(tx.Row);
                rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                dCreditExGST = 0;
                dCreditIncGST = 0;
                dDebitExGST = 0;
                dDebitIncGST = 0;
                szAccount = DB.readString(rCurr["USERCREDITGLCODE"]);
                if (Type == "JUNIORTOSENIOR") {
                    dDebitExGST = DB.readDouble(rCurr["AMOUNT"]);
                    dDebitIncGST = DB.readDouble(rCurr["AMOUNT"]);
                } else if (Type == "SALESBONUSSCHEME") {
                    rCurr["ACCOUNTNUMBER"] = "2-8400";
                    dDebitExGST = DB.readDouble(rCurr["TOTALBONUS"]);
                    dDebitIncGST = DB.readDouble(rCurr["TOTALBONUS"]);
                } else {
                    dCreditExGST = DB.readDouble(rCurr["AMOUNT"]);
                    dCreditIncGST = DB.readDouble(rCurr["AMOUNT"]);
                }
                createDataRow(ref rCurr, szJournalNumber, dtTX, szAccount, dDebitExGST, dDebitIncGST, dCreditExGST, dCreditIncGST, szJobCode);

                if (UpdateDB) {
                    if (Type == "COMM") {
                        DB.runNonQuery(String.Format(@"UPDATE SALE SET MYOBEXPORTID = {0} WHERE ID = {1}", MYOBExportID, rCurr["ID"].ToString()));
                    } else if (Type == "JUNIORTOSENIOR") {
                        DB.runNonQuery(String.Format(@"UPDATE MENTORBONUS SET MYOBEXPORTID = {0} WHERE ID = {1}", MYOBExportID, rCurr["ID"].ToString()));
                    }
                }
            }
        }

        private void createDataRow(ref DataRow dr, string JournalNumber, DateTime TxDate, string AccountCode, double DebitExGST, double DebitIncGST, double CreditExGST, double CreditIncGST, string JobCode) {
            dr["TXDATE"] = Utility.formatDateForMYOBExport(TxDate);
            dr["ACCOUNTNUMBER"] = AccountCode;
            dr["DEBITEXGST"] = Utility.formatMoney(DebitExGST);
            dr["DEBITINCGST"] = Utility.formatMoney(DebitIncGST);
            dr["CREDITEXGST"] = Utility.formatMoney(CreditExGST);
            dr["CREDITINCGST"] = Utility.formatMoney(CreditIncGST);
            dr["JOB"] = JobCode;
        }

        protected void btnPreview_Click(object sender, EventArgs e) {
            createRecords(false);
        }
    }
}