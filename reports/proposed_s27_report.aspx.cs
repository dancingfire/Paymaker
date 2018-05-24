using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class proposed_s27_report : Root {
        private DateTime dtStartDate;
        private DateTime dtEndDate;

        private string szCompanyIDList = "";
        private GridViewHelper helper = null;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            dtStartDate = Valid.getDate("szStartDate", DateTime.MinValue);
            dtEndDate = Valid.getDate("szEndDate", DateTime.MinValue);
            szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

            bindData();
            if (Valid.getBoolean("blnPrint", false)) {
                sbEndJS.Append("printReport();");
            }
        }

        private void bindData() {
            string szWhereFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szWhereFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

            if (dtStartDate != DateTime.MinValue)
                szWhereFilter += " AND DATEADD(d, 30, S.SALEDATE) >= '" + Utility.formatDate(dtStartDate) + "' ";

            if (dtEndDate != DateTime.MinValue)
                szWhereFilter += " AND DATEADD(d, 30, S.SALEDATE) <= '" + Utility.formatDate(dtEndDate) + "' ";

            string szSQL = string.Format(@"

                SELECT MAX(S.SALEDATE) as SALEDATE, S.ENTITLEMENTDATE as ENTITLEMENTDATE,
                    S.CODE, SUM(USS.GRAPHCOMMISSION )AS CALCULATEDAMOUNT
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID = 1
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USS.OFFICEID AND L_OFFICE.ISACTIVE = 1
                JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE SS.CALCULATEDAMOUNT > 0 {0} AND S.ISSECTION27 = 1
                GROUP BY S.CODE, S.ID, S.ENTITLEMENTDATE
                ORDER BY S.ENTITLEMENTDATE
                ", szWhereFilter);
            DataSet ds = DB.runDataSet(szSQL);

            helper = new GridViewHelper(gvTable, true);

            helper.RegisterSummary("CALCULATEDAMOUNT", SummaryOperation.Sum);

            helper.GeneralSummary += new FooterEvent(helper_GeneralSummary);
            gvTable.DataSource = ds;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }

        private void helper_GeneralSummary(GridViewRow row) {
            row.BackColor = Color.FromArgb(230, 213, 172);
            row.Font.Bold = true;
            foreach (TableCell c in row.Cells)
                c.HorizontalAlign = HorizontalAlign.Right;
        }
    }
}