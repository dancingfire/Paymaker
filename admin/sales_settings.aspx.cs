using System;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sales_settings : Root {
    private AppConfigAdmin oConfigAdmin = new AppConfigAdmin();

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            oConfigAdmin.addConfig("RETAINERAMOUNT", "3041.67");
            oConfigAdmin.addConfig("CALCULATEBONUS", "FALSE");
            oConfigAdmin.addConfig("SUPERGLCODE", "");

            if (!Page.IsPostBack) {
                loadSettings();
                loadSuperHistory();
            }
        }
    }

    private void loadSettings() {
        oConfigAdmin.loadValuesFromDB();
        txtRetainerAmount.Text = oConfigAdmin.getValue("RETAINERAMOUNT");
        txtSuperGLCode.Text = oConfigAdmin.getValue("SUPERGLCODE");
        Utility.setListBoxItems(ref lstCalcBonus, oConfigAdmin.getValue("CALCULATEBONUS"));
    }

    private void loadSuperHistory() {
        bindHistory(gvSuperPercentageHistory, "SUPERANNUATIONPERCENTAGE");
        bindHistory(gvSuperMaxHistory, "SUPERANNUATIONMAX");
        txtNewSuperPercentageDate.Text = Utility.formatDate(DateTime.Now);
        txtNewSuperMaxDate.Text = Utility.formatDate(DateTime.Now);
    }

    private void bindHistory(GridView oGV, string SettingName) {
        string szSQL = String.Format(@"
            SELECT ID, VALUE, EFFECTIVEFROM
            FROM SUPERSETTINGSHISTORY
            WHERE SETTINGNAME = '{0}'
            ORDER BY EFFECTIVEFROM DESC", SettingName);

        oGV.DataSource = DB.runReader(szSQL);
        oGV.DataBind();
        HTML.formatGridView(ref oGV, true);
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        oConfigAdmin.updateConfigValueSQL("RETAINERAMOUNT", Utility.formatForDB(txtRetainerAmount.Text));
        oConfigAdmin.updateConfigValueSQL("CALCULATEBONUS", Utility.formatForDB(lstCalcBonus.SelectedValue));
        oConfigAdmin.updateConfigValueSQL("SUPERGLCODE", Utility.formatForDB(txtSuperGLCode.Text));
    }

    protected void btnAddSuperPercentage_Click(object sender, EventArgs e) {
        addSuperHistoryEntry("SUPERANNUATIONPERCENTAGE", txtNewSuperPercentageDate.Text, txtNewSuperPercentageValue.Text);
        loadSuperHistory();
    }

    protected void btnAddSuperMax_Click(object sender, EventArgs e) {
        addSuperHistoryEntry("SUPERANNUATIONMAX", txtNewSuperMaxDate.Text, txtNewSuperMaxValue.Text);
        loadSuperHistory();
    }

    private void addSuperHistoryEntry(string SettingName, string EffectiveDate, string Value) {
        sqlUpdate oSQL = new sqlUpdate("SUPERSETTINGSHISTORY", "ID", -1);
        oSQL.add("SETTINGNAME", SettingName);
        oSQL.add("EFFECTIVEFROM", EffectiveDate);
        oSQL.add("VALUE", Convert.ToDouble(Value));
        DB.runNonQuery(oSQL.createInsertSQL());
    }

    protected void gvSuperPercentageHistory_RowDeleting(object sender, GridViewDeleteEventArgs e) {
        deleteSuperHistoryEntry(gvSuperPercentageHistory, e.RowIndex);
    }

    protected void gvSuperMaxHistory_RowDeleting(object sender, GridViewDeleteEventArgs e) {
        deleteSuperHistoryEntry(gvSuperMaxHistory, e.RowIndex);
    }

    private void deleteSuperHistoryEntry(GridView oGV, int RowIndex) {
        int intID = Convert.ToInt32(oGV.DataKeys[RowIndex].Value);
        DB.runNonQuery(String.Format("DELETE FROM SUPERSETTINGSHISTORY WHERE ID = {0}", intID));
        loadSuperHistory();
    }

    protected void btnDeleteBonus_Click(object sender, EventArgs e) {
        DB.runNonQuery("DELETE FROM MENTORBONUS");
    }
}
