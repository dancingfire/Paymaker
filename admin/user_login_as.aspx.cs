using Paymaker;
using System;
using System.Web.UI.WebControls;

public partial class user_login_as : Root {

    protected void Page_Init(object sender, System.EventArgs e) {
        blnShowMenu = false;
    }

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            loadPage();
        }
    }

    private void loadPage() {
        string szSQL = String.Format(@"
            SELECT U.ID, U.NAME
            FROM USER_DELEGATION UD JOIN DB_USER U ON UD.USER_ID = U.ID
            WHERE RECORDSTATUS = 1 AND DELEGATION_USER_ID = {0} AND '{1}' BETWEEN UD.START_DATE AND UD.END_DATE
            ORDER BY U.NAME;", G.User.OriginalUserID, Utility.formatDate(DateTime.Now));
        if (G.User.OriginalRoleID == UserRole.Admin) {
            szSQL = @"SELECT U.ID, U.NAME
            FROM DB_USER U
            WHERE U.ISACTIVE = 1
            ORDER BY U.NAME";
        }

        Utility.BindList(ref lstUser, DB.runDataSet(szSQL));
        lstUser.Items.Insert(0, new ListItem("Select the person", "-1"));

        if (G.User.OriginalUserID == G.User.UserID)
            btnLoginSelf.Visible = false;
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        UserLogin.performDelegateLogin(Convert.ToInt32(lstUser.SelectedValue));
        sbStartJS.Append("parent.closeLoginAs(); parent.location.href = '../projects/dashboard.aspx';");
    }

    protected void btnLoginSelf_Click(object sender, System.EventArgs e) {
        UserLogin.performDelegateLogin(G.User.OriginalUserID);
        sbStartJS.Append("parent.closeLoginAs();  parent.location.href = '../projects/dashboard.aspx';");
    }
}