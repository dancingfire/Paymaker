using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class AR_report : Root {
        private DateTime dtStartDate;
        private DateTime dtEndDate;

        private string szCompanyIDList = "";
        private GridViewHelper helper = null;
        protected void Page_Init(object sender, System.EventArgs e) {
          
        }

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            dtStartDate = Valid.getDate("szStartDate", DateTime.Now);
            dtEndDate = Valid.getDate("szEndDate", DateTime.MaxValue);
            szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

            bindData();
            if (Valid.getBoolean("blnPrint", false)) {
                sbEndJS.Append("printReport();");
            }
        }

        private void bindData() {
            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);
            DB.runNonQuery("UPDATE SALE SET SETTLEMENTDATE = null where SETTLEMENTDATE < 'Jan 1, 2000'");
            string szSQL = string.Format(@"

                SELECT MAX(S.SALEDATE) as SALEDATE, S.ENTITLEMENTDATE as ENTITLEMENTDATE,
                S.CODE, SUM(GRAPHCOMMISSION) AS CALCULATEDAMOUNT,  S.SETTLEMENTDATE, 
                ISNULL((SELECT SUM(SE.CALCULATEDAMOUNT) FROM SALEEXPENSE SE JOIN LIST L ON SE.EXPENSETYPEID = L.ID 
                        AND  (L.NAME like '%Incentive%' OR L.NAME like 'Gifts%' or L.NAME = 'Unauth Advertising' or L.NAME = 'Auctioneer Fee')
                WHERE SE.SALEID = S.ID), 0) as EXPENSES,  SUM(ACTUALPAYMENT) as NETCOMMISSION
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID = 1 AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID  AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USS.OFFICEID AND L_OFFICE.ISACTIVE = 1
                JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE SS.CALCULATEDAMOUNT > 0 AND S.ENTITLEMENTDATE BETWEEN '{0}' AND '{1} 23:59' {2}
                GROUP BY S.CODE, S.ID, S.ENTITLEMENTDATE, S.SETTLEMENTDATE
                ORDER BY S.ENTITLEMENTDATE
                ", Utility.formatDate(dtStartDate), Utility.formatDate(dtEndDate), szCompanyFilter);
            DataSet ds = DB.runDataSet(szSQL);
           
            helper = new GridViewHelper(gvTable, true);

            helper.RegisterSummary("CALCULATEDAMOUNT", SummaryOperation.Sum);

            helper.GeneralSummary += new FooterEvent(helper_GeneralSummary);
            gvTable.DataSource = ds;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable, true);
        }

        private void helper_GeneralSummary(GridViewRow row) {
            row.BackColor = Color.FromArgb(230, 213, 172);
            row.Font.Bold = true;
            foreach (TableCell c in row.Cells)
                c.HorizontalAlign = HorizontalAlign.Right;
        }
    }
}