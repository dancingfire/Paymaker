using System;
using System.Data;

namespace Paymaker {

    public partial class reset_password : Root {
        protected DataSet dsSet;

        protected void Page_Init(object sender, System.EventArgs e) {
            blnUseSession = false;
            blnIsRoot = true;
            blnShowMenu = false;
            blnLoggedInAccessOnly = false;
            Session["blnUseArchive"] = "false";
        }

        protected void btnPasswordReset_Click(object sender, System.EventArgs e) {
            if (!String.IsNullOrEmpty(txtResetPassword.Text) && (!String.IsNullOrEmpty(txtConfirmPassword.Text))) {
                if (String.Equals(txtResetPassword.Text, txtConfirmPassword.Text)) {
                    string szToken = Valid.getText("t", VT.NoValidation);
                    int UserID = DB.getScalar(String.Format("SELECT ID FROM DB_USER WHERE UNIQUEID = '{0}' AND PASSWORDRESETTIMEOUT > GETDATE()", DB.escape(szToken)), -1);
                    string szErrors = UserLogin.updatePassword(UserID, txtResetPassword.Text);
                    if(szErrors != "") {
                        G.Notifications.addPageNotification(PageNotificationType.Error, "Error!", "There has been an issue resetting your password. Please ensure you meet the password guidelines", true);
                        G.Notifications.showPageNotification(true);
                    } if (UserID == -1) {
                        G.Notifications.addPageNotification(PageNotificationType.Error, "Error!", "Unable to update user password, you must update your password within 2 hours of recieving email and you can only reset password once.  Please request a new email from login page", true);
                        G.Notifications.showPageNotification(true);
                    } else {
                        G.Notifications.addPageNotification(PageNotificationType.Success, "Success!", "Password has been reset successfully " + string.Format(@"<a href='{0}/login.aspx'>Click here to login</a>", G.Settings.DomainName), true);
                        G.Notifications.showPageNotification(true);
                        dLogin.Visible = false;
                        DB.runNonQuery(string.Format("UPDATE DB_USER SET UNIQUEID = NEWID() WHERE ID = {0}", UserID));
                    }
                }
            }
        }
    }
}