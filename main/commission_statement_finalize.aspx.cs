using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class commission_statement_finalize : Root {
        DataTable dtTotal = null;
        PayPeriod oP = null;
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
            processExport(true);
        }

        private void processExport(bool UpdateDB) {
            string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);
            DataSet dsCompany = DB.runDataSet(szSQL);
           
            foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
                if (lstCompany.SelectedValue != "" && Convert.ToInt32(lstCompany.SelectedValue) != DB.readInt(drCompany["ID"]))
                    continue;
                string szFileName = Utility.formatDate(DateTime.Now) + "-MYOB-Commission-ADVANCED" + drCompany["NAME"] + ".csv";
                createRecordsAdvanced(UpdateDB, DB.readInt(drCompany["ID"]), szFileName);
            }
        }

        private void createRecordsAdvanced(bool UpdateDB, int CompanyID, string FileName) {
            int intMYOBExportID = 0;
            if (UpdateDB) {
                intMYOBExportID = Utility.getMYOBExportID(ExportType.SalesCommission, FileName);
            }
            DateTime dtStart = DateTime.MaxValue;
            DateTime dtEnd = DateTime.MaxValue;
            oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(lstPayPeriod.SelectedValue));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }
            string szPath = G.Settings.MYOBDir + FileName;

            string szSQL = string.Format(@"
                -- 0) All normal commissions
                SELECT  L_OFF.OFFICEMYOBBRANCH as Branch, '2-3000' AS Account, L_OFF.OFFICEMYOBCODE + '-' + u.INITIALSCODE  AS Subaccount,
                    '' as [Debit Amount], '' as [Credit Amount], '' as [Ref. Number], S.ADDRESS AS [Transaction Description], '' as STOP,
                   U.CREDITGLCODE AS USERCREDITGLCODE, U.DEBITGLCODE AS USERDEBITGLCODE, USS.ACTUALPAYMENT as COMMAMOUNT, l_OFF.JOBCODE,
                    S.ID,  S.SALEDATE as ACTUALDATE
                FROM SALE S
                JOIN SALESPLIT SS ON S.ID = SS.SALEID AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER U  ON USS.USERID = U.ID
                JOIN LIST L_COMM ON L_COMM.ID = SS.COMMISSIONTYPEID
                JOIN LIST L_OFF ON L_OFF.ID = USS.OFFICEID
                JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID AND L_OFF.COMPANYID = {0}
                    AND S.SALEDATE BETWEEN '{1} 00:00:00' AND '{2} 23:59:59' AND S.STATUSID IN (1, 2) AND USS.ACTUALPAYMENT > 0
                ORDER BY S.SALEDATE;

                ", CompanyID, Utility.formatDate(dtStart), Utility.formatDate(dtEnd), oP.ID);

            DataSet ds = DB.runDataSet(szSQL);

            if (File.Exists(szPath))
                File.Delete(szPath);
            DataTable dtNew = ds.Tables[0].Clone();
          
            string szFilter = lstRecords.SelectedValue;
            if (szFilter == "") {
                //Process the commissions
                processTableAdvanced(ds.Tables[0].DefaultView, dtNew, "COMM", intMYOBExportID, UpdateDB);
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
            dtNew.AcceptChanges();
            if (UpdateDB) {
                StreamWriter output = new StreamWriter(szPath);
                output.Write(CsvWriter.WriteToString(dtNew, true, false, false));
                output.Flush();
                output.Close();
                oLinks.InnerHtml += string.Format("<a href='../admin/myob_doc.aspx?file={0}' target='_blank'>{1}</a> <br/>", Server.UrlEncode(FileName), FileName);
            } else {
                if (dtTotal == null)
                    dtTotal = dtNew.Clone();
                //Add these rows to the overalloutput so we can see the data
                foreach (DataRow dr in dtNew.Rows) {
                    dtTotal.Rows.Add(dr.ItemArray);
                }
            }

            if (!UpdateDB) {
                gvData.DataSource = dtTotal;
                gvData.DataBind();
                gvData.Visible = true;
                HTML.formatGridView(ref gvData, true);
            }
        }

        private void processTableAdvanced(DataView dt, DataTable dtNew,  string Type, int MYOBExportID, bool UpdateDB) {
            foreach (DataRowView tx in dt) {
                dtNew.ImportRow(tx.Row);
                DataRow rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                double Amount = DB.readDouble(rCurr["COMMAMOUNT"]);

                string szAccount = DB.readString(rCurr["USERDEBITGLCODE"]);
                string szJobCode = DB.readString(rCurr["JOBCODE"]);
               
                DateTime dtTX = DB.readDate(rCurr["ACTUALDATE"]);
                
                //Debit
                createDataRowAdvanced(ref rCurr, dtTX, "5-1010", Amount, false);

                //Do the same for the inverse of this transaction
                //Credit
                dtNew.ImportRow(tx.Row);
                rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                szAccount = DB.readString(rCurr["USERCREDITGLCODE"]);
                createDataRowAdvanced(ref rCurr, dtTX, "2-3000", Amount, true);
               
                if (UpdateDB) {
                    DB.runNonQuery(String.Format(@"UPDATE SALE SET MYOBEXPORTID = {0} WHERE ID = {1}", MYOBExportID, rCurr["ID"].ToString())) ;
                }
            }
        }

        private void createDataRowAdvanced(ref DataRow dr, DateTime TxDate, string AccountCode, double Amount, bool IsCredit) {
            dr["REf. Number"] = Utility.formatDate(TxDate);
            dr["ACCOUNT"] = AccountCode;
            if (IsCredit) {
                dr["CREDIT AMOUNT"] = Utility.formatMoney(Amount);
            } else {
                dr["DEBIT AMOUNT"] = Utility.formatMoney(Amount);
            }
            dr["SUBACCOUNT"] = Utility.fixGLOfficeCode(Convert.ToString(dr["SUBACCOUNT"]));
            if (dr["SUBACCOUNT"].ToString().Contains("PMD")) {
                dr["ACCOUNT"] = "2-3035";
            }

        }


        protected void btnPreview_Click(object sender, EventArgs e) {
            processExport(false);
        }
    }
}