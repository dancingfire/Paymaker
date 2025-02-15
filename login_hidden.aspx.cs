using Microsoft.ApplicationBlocks.Data;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Paymaker {

    public partial class login_hidden : Root {
        protected System.Data.SqlClient.SqlConnection cnn;
        private string szForgotUserNameEntered = string.Empty;

        protected void Page_Init(object sender, System.EventArgs e) {
            blnUseSession = false;
            blnIsRoot = true;
            blnShowMenu = false;
            blnLoggedInAccessOnly = false;
        }

        /// <summary>
        /// 1. Checks if the session is already timeout
        /// 2. Set the value of user login (Client, External or Internal)
        /// 3. Based on the above values we show and hide the lab locations.
        /// </summary>
        protected void Page_Load(object sender, System.EventArgs e) {
            // Check if a session already exists.
            // This ensures session is logged out when people log out or time out
            if (!IsPostBack && HttpContext.Current.Session != null)
                HttpContext.Current.Session.Abandon();
            //Check whether we are redirecting to the leave page
            G.User.IsLeave = Request.QueryString["LEAVE"] != null;
            
            if (Request.QueryString["Timeout"] != null) {
                lblTimeout.Visible = true;
            } else {
                lblTimeout.Visible = false;
            }
        }

        private void performLogin() {
            // If there is a login lock before the Login is performed, then Login not performed
            if (UserLogin.checkLoginLock(txtUserName.Text)) {
                G.Notifications.addPageNotification(PageNotificationType.Warning, "Too many login attempts", "This account has been locked for 1 minute. ", true);
                G.Notifications.showPageNotification(true);
                return;
            }
            bool UserLoginGood = UserLogin.loginUserByEmail(txtUserName.Text);
            
            if (UserLoginGood) {
                sbEndJS.AppendFormat("window.top.location.href = '{0}';", UserLogin.getStartPage());
                pPage.Visible = false;
            }            
        }

        protected void btnSubmit_Click(object sender, System.EventArgs e) {
            if (Page.IsValid) {
                performLogin();
            }
        }

        //Reset Password and send Email
        protected void btnReset_Click(object sender, System.EventArgs e) {
            szForgotUserNameEntered = Valid.getText("txtForgotUsername", "", VT.TextNormal);
            UserLogin.resetPassword(szForgotUserNameEntered);
        }
    }
}