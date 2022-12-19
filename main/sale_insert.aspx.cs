using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sale_insert : Root {
    private bool blnUserReadOnly = false;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        if (!Page.IsPostBack) {
            blnUserReadOnly = Valid.getBoolean("ReadOnly", false);
            hdSaleID.Value = "-1";
            initPage();
        }
    }

    private void initPage() {
        StringBuilder sbHTML = new StringBuilder();
         
        Utility.loadPayPeriodList(ref lstPayPeriod);
        lstPayPeriod.Items.Insert(0, new ListItem("Not set", "-1"));
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        sqlUpdate oSQL = new sqlUpdate("SALE", "ID", -1);
        
        oSQL.add("STATUSID", lstStatus.SelectedValue);
        oSQL.add("CODE", txtCode.Text);
        oSQL.add("ADDRESS", txtCode.Text);

        if (txtSalesDate.Text == "")
            oSQL.addNull("SALEDATE");
        else
            oSQL.add("SALEDATE", txtSalesDate.Text);
        if (txtEntitlementDate.Text == "")
            oSQL.addNull("ENTITLEMENTDATE");
        else
            oSQL.add("ENTITLEMENTDATE", txtEntitlementDate.Text);
        if (txtSettlementDate.Text == "")
            oSQL.addNull("SETTLEMENTDATE");
        else
            oSQL.add("SETTLEMENTDATE", txtSettlementDate.Text);

        oSQL.add("COMMENTS", txtComments.Text);
        oSQL.add("GROSSCOMMISSION", txtGrossCommission.Text);
        oSQL.add("CONJUNCTIONALCOMMISSION", txtConjCommission.Text);

        if (txtSalePrice.Text == "")
            txtSalePrice.Text = "0";
        oSQL.add("SALEPRICE", txtSalePrice.Text);
        oSQL.add("LOCKCOMMISSION", lstLocked.SelectedValue);
        if (lstPayPeriod.SelectedValue == "-1")
            oSQL.addNull("PAYPERIODID");
        else
            oSQL.add("PAYPERIODID", lstPayPeriod.SelectedValue);

        DB.runNonQuery(oSQL.createInsertSQL());
        sbEndJS.Append("parent.closeSale()");
    }
}