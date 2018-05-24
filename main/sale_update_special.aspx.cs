using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sale_update : Root {
    private bool blnUserReadOnly = false;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        if (!Page.IsPostBack) {
            blnUserReadOnly = Valid.getBoolean("ReadOnly", false);
            hdSaleID.Value = Valid.getInteger("intItemID").ToString();
            loadSale();
        }
    }

    private void loadSale() {
        StringBuilder sbHTML = new StringBuilder();
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value), true, true);
        txtCode.Text = oS.Code + " - " + oS.Address;
        txtComments.Text = oS.Comments;
        txtSalesDate.Text = oS.SaleDate;
        txtSettlementDate.Text = Utility.formatDate(oS.SettlementDate);
        txtEntitlementDate.Text = Utility.formatDate(oS.EntitlementDate);
        txtGrossCommission.Text = Utility.formatMoney(oS.GrossCommission);
        txtConjCommission.Text = Utility.formatMoney(oS.ConjCommission);
        Utility.setListBoxItems(ref lstStatus, oS.Status.ToString());
        Utility.loadPayPeriodList(ref lstPayPeriod);
        lstPayPeriod.Items.Insert(0, new ListItem("Not set", "-1"));
        Utility.setListBoxItems(ref lstPayPeriod, Convert.ToString(oS.PayPeriodID));
        int CurrLock = DB.getScalar("SELECT LOCKCOMMISSION FROM SALE WHERE ID = " + hdSaleID.Value, 0);
        Utility.setListBoxItems(ref lstLocked, CurrLock.ToString());
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        sqlUpdate oSQL = new sqlUpdate("SALE", "ID", Convert.ToInt32(hdSaleID.Value));
        oSQL.add("STATUSID", lstStatus.SelectedValue);
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

        oSQL.add("LOCKCOMMISSION", lstLocked.SelectedValue);
        if (lstPayPeriod.SelectedValue == "-1")
            oSQL.addNull("PAYPERIODID");
        else
            oSQL.add("PAYPERIODID", lstPayPeriod.SelectedValue);
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value));
        string szLogRecord = String.Format(@"Code: {0} modified<br/>
                    Status: {2} changed to {3}<br/>
                    Pay Period: {4} changed to {5}</br>
                    Sale date: {6} changed to {7}</br>
                    DB ID = {1}
                    ", oS.Code, hdSaleID.Value, oS.Status, lstStatus.SelectedItem.Text,
                    oS.PayPeriodID, lstPayPeriod.SelectedItem.Text,
                    oS.SaleDate, txtSalesDate.Text
                    );

        DBLog.addGenericRecord(DBLogType.SaleData, szLogRecord, Convert.ToInt32(hdSaleID.Value));
        DB.runNonQuery(oSQL.createUpdateSQL());
        sbEndJS.Append("parent.closeSale()");
    }
}