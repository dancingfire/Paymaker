using System;
using System.Data;

namespace Paymaker {

    public partial class expense_summary : Root {
        private bool blnPrint = false;
        private int startYear = 2000;
        private int endYear = 3000;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            hdExpenseID.Value = Valid.getText("szExpenseID", "", VT.TextNormal);
            hdFY.Value = Valid.getText("szFY", "", VT.TextNormal);
            string szOfficeFilterNames = Valid.getText("szOfficeNames", "", VT.TextNormal);
            string szCompanyFilterNames = Valid.getText("szCompanyNames", "", VT.TextNormal);
            string szExpenseFilterNames = Valid.getText("szExpenseNames", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);

            blnPrint = Valid.getBoolean("blnPrint", false);
            if (hdExpenseID.Value == "null")
                hdExpenseID.Value = "";

            bindData();
            if (String.IsNullOrWhiteSpace(szExpenseFilterNames))
                szExpenseFilterNames = "All expenses";
            pPageHeader.InnerHtml = szExpenseFilterNames + " " + startYear + "/" + endYear;

            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szFilter = getDateFilter();
            string szAccountFilter = "";
            if (!String.IsNullOrWhiteSpace(hdExpenseID.Value))
                szAccountFilter = String.Format(" AND TX.ACCOUNTID IN ({0})", hdExpenseID.Value);

            //Are we looking at an expense or income report
            bool blnIsExpenseReport = DB.getScalar("SELECT LISTTYPEID FROM LIST WHERE ID = " + hdExpenseID.Value, 2) == (int)ListType.Expense;

            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szCompanyFilter = String.Format(" AND L_OFFICE.COMPANYID IN ({0})", hdCompanyID.Value);

            string szSQL = string.Format(@"
                SELECT TX.AMOUNT, TX.FLETCHERCONTRIBTOTAL, TXDATE, TX.USERID
                FROM USERTX TX JOIN DB_USER USR ON USR.ID = TX.USERID AND USR.ID > 0 AND TX.ISDELETED = 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                 WHERE {0} {1} {2};

                --Expense account query
                SELECT U.ID, LASTNAME + ', ' + FIRSTNAME AS AGENT, TX.AMOUNT AS BUDGETAMOUNT
                FROM DB_USER U
                JOIN USERACCOUNT TX ON TX.USERID = U.ID {1}
                JOIN LIST L_OFFICE ON L_OFFICE.ID = U.OFFICEID
                WHERE U.ISACTIVE = 1  {2}
                ORDER by LASTNAME, FIRSTNAME;", szFilter, szAccountFilter, szCompanyFilter);

            DataSet ds = DB.runDataSet(szSQL);
            DataSet dsClone = ds.Copy();
            string szCol = "FLETCHERCONTRIBTOTAL";
            if (Valid.getText("szFletcherOrAgent", "").ToUpper() == "AGENT")
                szCol = "AMOUNT";
            gvFletchers.DataSource = formatDataSet(ds, szCol);
            gvFletchers.DataBind();
            HTML.formatGridView(ref gvFletchers);
            if (blnIsExpenseReport) {
                gvAgent.DataBind();
                HTML.formatGridView(ref gvAgent);
            }
            dAgentContribution.Visible = dFletchersHeader.Visible = blnIsExpenseReport;
        }

        protected DataView formatDataSet(DataSet ds, string SummingCol) {
            int intCurrUserID = -1;
            DataTable dtUserData = ds.Tables[1];

            dtUserData.Columns.Add(new DataColumn("0", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("1", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("2", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("3", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("4", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("5", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("6", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("7", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("8", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("9", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("10", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("11", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("12", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("Budget", System.Type.GetType("System.Double")));
            dtUserData.Columns.Add(new DataColumn("TOTAL", System.Type.GetType("System.Double")));

            DataRow oTotalRow = dtUserData.NewRow();
            DataView dvFiltered = ds.Tables[0].DefaultView;
            double dUserTotal = 0;
            foreach (DataRow drUser in ds.Tables[1].Rows) {
                intCurrUserID = Convert.ToInt32(drUser["ID"]);
                dvFiltered.RowFilter = "USERID =" + intCurrUserID;
                dUserTotal = 0;
                drUser["Budget"] = drUser["BudgetAmount"].ToString(); //Copy the budget amount across

                foreach (DataRowView dr in dvFiltered) {
                    int intMonthID = Convert.ToDateTime(dr["TXDATE"]).Month;
                    string szInitialValue = drUser[intMonthID.ToString()].ToString();
                    double dValue = 0;
                    if (Utility.IsNumeric(szInitialValue))
                        dValue = Convert.ToDouble(szInitialValue);
                    dValue += Convert.ToDouble(dr[SummingCol]);
                    dUserTotal += Convert.ToDouble(dr[SummingCol]);
                    drUser[intMonthID.ToString()] = dValue;
                }

                drUser["TOTAL"] = Utility.formatMoney(dUserTotal);
            }
            addTotalRow(ds.Tables[1]);
            DataView dvFinal = dtUserData.DefaultView;

            dvFinal.RowFilter = "TOTAL > 0 OR BUDGET > 0";
            return dvFinal;
        }

        private void addTotalRow(DataTable dtUser) {
            DataRow drTotal = dtUser.NewRow();
            drTotal["AGENT"] = "TOTAL";

            foreach (DataColumn oDC in dtUser.Columns) {
                double dTotal = 0;
                if (oDC.ColumnName != "AGENT") {
                    foreach (DataRow dr in dtUser.Rows) {
                        if (Utility.IsNumeric(dr[oDC.ColumnName].ToString())) {
                            dTotal += Convert.ToDouble(dr[oDC.ColumnName].ToString());
                        }
                    }
                    drTotal[oDC.ColumnName] = dTotal;
                }
            }
            dtUser.Rows.Add(drTotal);
        }

        private string getDateFilter() {
            startYear = Convert.ToInt32(hdFY.Value) - 1;
            endYear = Convert.ToInt32(hdFY.Value);
            return string.Format(@" TX.TXDATE > 'Jun 30, {0} 0:00' and TX.TXDATE < 'July 01, {1} 23:59'", startYear, endYear);
        }
    }
}