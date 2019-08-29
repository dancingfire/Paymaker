using System;
using System.Web.UI;

public partial class leave_settings : Root {
    private AppConfigAdmin oConfigAdmin = new AppConfigAdmin();

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            oConfigAdmin.addConfig("LEAVECATCHALLEMAIL", "jacqui.litvik@fletchers.net.au");
            oConfigAdmin.addConfig("PERMITTEDLEAVETESTERS", "");

            if (!Page.IsPostBack) {
                loadSettings();
            }
        }
    }

    private void loadSettings() {
        oConfigAdmin.loadValuesFromDB();
        txtCatchallEmail.Text = oConfigAdmin.getValue("LEAVECATCHALLEMAIL");
        G.UserInfo.loadList(ref lstPermittedUsers);
        Utility.setListBoxItems(ref lstPermittedUsers, oConfigAdmin.getValue("PERMITTEDLEAVETESTERS"));
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        oConfigAdmin.updateConfigValueSQL("LEAVECATCHALLEMAIL", Utility.formatForDB(txtCatchallEmail.Text));
        oConfigAdmin.updateConfigValueSQL("PERMITTEDLEAVETESTERS", hdLeaveTesters.Value);

    }
}