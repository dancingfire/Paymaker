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
            SELECT U.ID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM USERDELEGATION UD JOIN DB_USER U ON UD.USERID = U.ID
            WHERE RECORDSTATUS = 1 AND DELEGATIONUSERID = {0} AND '{1}' BETWEEN UD.STARTDATE AND UD.ENDDATE
            ORDER BY U.FIRSTNAME + ' ' + U.LASTNAME;", G.User.OriginalUserID, Utility.formatDate(DateTime.Now));
        if (G.User.OriginalRoleID == UserRole.Admin) {
            szSQL = @"SELECT U.ID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM DB_USER U
            WHERE U.ISACTIVE = 1
            ORDER BY U.FIRSTNAME + ' ' + U.LASTNAME";
        }

        Utility.BindList(ref lstUser, DB.runDataSet(szSQL));
        lstUser.Items.Insert(0, new ListItem("Select the person", "-1"));

        if (G.User.OriginalUserID == G.User.UserID)
            btnLoginSelf.Visible = false;
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        UserLogin.performDelegateLogin(Convert.ToInt32(lstUser.SelectedValue));
        sbStartJS.Append("parent.closeLoginAs(); parent.location.href = '../main/admin_dashboard.aspx';");
    }

    protected void btnLoginSelf_Click(object sender, System.EventArgs e) {
        UserLogin.performDelegateLogin(G.User.OriginalUserID);
        sbStartJS.Append("parent.closeLoginAs();  parent.location.href = '../main/admin_dashboard.aspx';");
    }
}