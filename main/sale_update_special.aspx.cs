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
        if (oS.SettlementDate != DateTime.MinValue) {
            txtSettlementDate.Text = Utility.formatDate(oS.SettlementDate);
        }
        if (oS.EntitlementDate != DateTime.MinValue) {
            txtEntitlementDate.Text = Utility.formatDate(oS.EntitlementDate);
        }
        txtGrossCommission.Text = Utility.formatMoney(oS.GrossCommission);
        txtSalePrice.Text = Utility.formatMoney(oS.SalePrice);
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
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value), true, true);
        string LogEntry = "";
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
        if (oS.GrossCommission != Convert.ToDouble(txtGrossCommission.Text)) {
            oSQL.add("GROSSCOMMISSION", txtGrossCommission.Text);
            LogEntry += String.Format(" Gross commission changed from {0} to {1} <br/>", Utility.formatMoney(oS.SalePrice), Utility.formatMoney(Convert.ToDouble(txtSalePrice.Text)));
        }
        if (oS.ConjCommission != Convert.ToDouble(txtConjCommission.Text)) {
            oSQL.add("CONJUNCTIONALCOMMISSION", txtConjCommission.Text);
            LogEntry += String.Format(" Conj commission changed from {0} to {1} <br/>", Utility.formatMoney(oS.SalePrice), Utility.formatMoney(Convert.ToDouble(txtSalePrice.Text)));
        }
        if (txtSalePrice.Text == "")
            txtSalePrice.Text = "0";

        if (oS.SalePrice != Convert.ToDouble(txtSalePrice.Text)) {
            oSQL.add("SALEPRICE", txtSalePrice.Text);
            LogEntry += String.Format(" Sale price changed from {0} to {1} <br/>", Utility.formatMoney(oS.SalePrice), Utility.formatMoney(Convert.ToDouble(txtSalePrice.Text)));
        }

        oSQL.add("LOCKCOMMISSION", lstLocked.SelectedValue);
        if (lstPayPeriod.SelectedValue == "-1")
            oSQL.addNull("PAYPERIODID");
        else
            oSQL.add("PAYPERIODID", lstPayPeriod.SelectedValue);
      
        string szLogRecord = String.Format(@"
                    
                    Code: {0} modified<br/>
                    Status: {2} changed to {3}<br/>
                    Pay Period: {4} changed to {5}</br>
                    Sale date: {6} changed to {7}</br>
                    DB ID = {1} <br/>
                    {8}
                    ", oS.Code, hdSaleID.Value, oS.Status, lstStatus.SelectedItem.Text,
                    oS.PayPeriodID, lstPayPeriod.SelectedItem.Text,
                    oS.SaleDate, txtSalesDate.Text, LogEntry
                    );

        DBLog.addGenericRecord(DBLogType.SaleData, szLogRecord, Convert.ToInt32(hdSaleID.Value));
        DB.runNonQuery(oSQL.createUpdateSQL());
        sbEndJS.Append("parent.closeSale()");
    }
}