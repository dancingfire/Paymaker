using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Paymaker {

    public partial class expense_detail : Root {
        bool blnPrint = false;
         int startYear = 2000;
        int endYear = 3000;
    
        protected void Page_Load(object sender, System.EventArgs e) {
            hdOfficeID.Value = Valid.getText("szOfficeID", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);
            hdExpenseID.Value = Valid.getText("szExpenseID", "", VT.TextNormal);
            hdFY.Value = Valid.getText("szFY", "", VT.TextNormal);
            string szOfficeFilterNames = Valid.getText("szOfficeNames", "", VT.TextNormal);
            string szCompanyFilterNames = Valid.getText("szCompanyNames", "", VT.TextNormal);
            string szExpenseFilterNames = Valid.getText("szExpenseNames", "", VT.TextNormal);
            
            blnPrint = Valid.getBoolean("blnPrint", false);
            if (hdOfficeID.Value == "null")
                hdOfficeID.Value = "";
            if (hdCompanyID.Value == "null")
                hdCompanyID.Value = "";

            bindData();
            pPageHeader.InnerHtml = pPageHeader.InnerHtml + szExpenseFilterNames + " " + startYear + "/" + endYear;
            if (szOfficeFilterNames != "") {
                pPageHeader.InnerHtml += "<br/><span class='Normal'><strong>Office: </strong>" + szOfficeFilterNames + "</span>";
            }
            if (szCompanyFilterNames != "") {
                pPageHeader.InnerHtml += "<br/><span class='Normal'><strong>Company: </strong>" + szCompanyFilterNames + "</span>";
            }
            
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {

            string szFilter = getDateFilter();

            if (!String.IsNullOrWhiteSpace(hdOfficeID.Value))
                szFilter += String.Format(" AND L_OFFICE.ID IN ({0})", hdOfficeID.Value);
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szFilter += String.Format(" AND L_COMPANY.ID IN ({0})", hdCompanyID.Value);
            if (!String.IsNullOrWhiteSpace(hdExpenseID.Value))
                szFilter += String.Format(" AND TX.ACCOUNTID IN ({0})", hdExpenseID.Value);

            
            string szSQL = string.Format(@"
                SELECT TX.AMOUNT, TXDATE, TX.USERID
                FROM USERTX TX JOIN DB_USER USR ON USR.ID = TX.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID 
                WHERE {0} ;
                

                SELECT U.ID, LASTNAME + ', ' + FIRSTNAME AS AGENT
                FROM DB_USER U WHERE ISACTIVE = 1
                ORDER by LASTNAME, FIRSTNAME"
                , szFilter);

            DataSet ds = DB.runDataSet(szSQL);
            formatDataSet(ds);
        }

        protected void formatDataSet(DataSet ds) {
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
            dtUserData.Columns.Add(new DataColumn("TOTAL", System.Type.GetType("System.String")));

            DataRow oTotalRow = dtUserData.NewRow();
            DataView dvFiltered = ds.Tables[0].DefaultView;
            double dUserTotal = 0;
            foreach (DataRow drUser in ds.Tables[1].Rows) {
                intCurrUserID = Convert.ToInt32(drUser["ID"]);
                dvFiltered.RowFilter = "USERID =" + intCurrUserID;
                dUserTotal = 0;

                foreach(DataRowView dr in dvFiltered){
                    int intMonthID = Convert.ToDateTime(dr["TXDATE"]).Month;
                    string szInitialValue = drUser[intMonthID.ToString()].ToString();
                    double dValue = 0;
                    if(Utility.IsNumeric(szInitialValue))
                        dValue = Convert.ToDouble(szInitialValue);
                    dValue += Convert.ToDouble(dr["AMOUNT"]);
                    dUserTotal += Convert.ToDouble(dr["AMOUNT"]);
                    drUser[intMonthID.ToString()] = dValue;
                }

                drUser["TOTAL"] = Utility.formatMoney(dUserTotal);
            }
            addTotalRow(ds.Tables[1]);
            gvTable.DataSource = dtUserData;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }

        private void addTotalRow(DataTable dtUser){
            DataRow drTotal = dtUser.NewRow();

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
