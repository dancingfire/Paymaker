using System;
using System.Web.UI;

public partial class leave_settings : Root {
    private AppConfigAdmin oConfigAdmin = new AppConfigAdmin();

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            oConfigAdmin.addConfig("LEAVECATCHALLEMAIL", "jacqui.litvik@fletchers.net.au");

            if (!Page.IsPostBack) {
                loadSettings();
            }
        }
    }

    private void loadSettings() {
        oConfigAdmin.loadValuesFromDB();
        txtCatchallEmail.Text = oConfigAdmin.getValue("LEAVECATCHALLEMAIL");
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        oConfigAdmin.updateConfigValueSQL("LEAVECATCHALLEMAIL", Utility.formatForDB(txtCatchallEmail.Text));
    }
}