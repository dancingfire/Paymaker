using System;
using System.Data;

namespace Paymaker {

    public partial class missing_sales : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;

            pPageHeader.InnerHtml = "Missing sales";
            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szSQL = @"
                SELECT S.ID, S.ADDRESS, S.CODE,
                S.SALEDATE, S.SALEPRICE, S.SETTLEMENTDATE, S.GROSSCOMMISSION, S.ENTITLEMENTDATE
                FROM SALE S

                WHERE ((S.PAYPERIODID IS NULL AND S.STATUSID = 2) OR S.PAYPERIODID NOT IN (SELECT ID FROM PAYPERIOD))
                ORDER BY SUBSTRING(S.ADDRESS, CHARINDEX(' ', S.ADDRESS) + 1, LEN(S.ADDRESS))";

            DataSet ds = DB.runDataSet(szSQL);
            gvTable.EmptyDataText = "There are no properties missing.";
            gvTable.DataSource = ds;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }
    }
}