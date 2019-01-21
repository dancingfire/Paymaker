using System;
using System.Web.UI;

public partial class sales_settings : Root {
    private AppConfigAdmin oConfigAdmin = new AppConfigAdmin();

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            oConfigAdmin.addConfig("RETAINERAMOUNT", "3041.67");
            oConfigAdmin.addConfig("SUPERANNUATIONPERCENTAGE", "9.25");
            oConfigAdmin.addConfig("SUPERANNUATIONMAX", "25000");
            oConfigAdmin.addConfig("CALCULATEBONUS", "FALSE");
            oConfigAdmin.addConfig("SUPERGLACCOUNT", "");

            if (!Page.IsPostBack) {
                loadSettings();
            }
        }
    }

    private void loadSettings() {
        oConfigAdmin.loadValuesFromDB();
        txtRetainerAmount.Text = oConfigAdmin.getValue("RETAINERAMOUNT");
        txtSuperPercentage.Text = oConfigAdmin.getValue("SUPERANNUATIONPERCENTAGE");
        txtSuperMax.Text = oConfigAdmin.getValue("SUPERANNUATIONMAX");
        txtSuperGLCode.Text = oConfigAdmin.getValue("SUPERGLCODE");
        Utility.setListBoxItems(ref lstCalcBonus, oConfigAdmin.getValue("CALCULATEBONUS"));
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        oConfigAdmin.updateConfigValueSQL("RETAINERAMOUNT", Utility.formatForDB(txtRetainerAmount.Text));
        oConfigAdmin.updateConfigValueSQL("SUPERANNUATIONPERCENTAGE", Utility.formatForDB(txtSuperPercentage.Text));
        oConfigAdmin.updateConfigValueSQL("SUPERANNUATIONMAX", Utility.formatForDB(txtSuperMax.Text));
        oConfigAdmin.updateConfigValueSQL("CALCULATEBONUS", Utility.formatForDB(lstCalcBonus.SelectedValue));
        oConfigAdmin.updateConfigValueSQL("SUPERGLCODE", Utility.formatForDB(txtSuperGLCode.Text));
    }

    protected void btnDeleteBonus_Click(object sender, EventArgs e) {
        DB.runNonQuery("DELETE FROM MENTORBONUS");
    }
}