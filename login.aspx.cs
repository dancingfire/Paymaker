using Microsoft.ApplicationBlocks.Data;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Paymaker {

    public partial class login : Root {
        protected System.Data.SqlClient.SqlConnection cnn;
        private string szForgotUserNameEntered = string.Empty;
        private bool blnLeave = false;

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
            blnLeave = Request.QueryString["LEAVE"] != null;

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
        
            int MatchingUserID = (int)SqlHelper.ExecuteScalar(DB.DBConn, CommandType.StoredProcedure,
                "getUserByLogin", new SqlParameter("@UserName", txtUserName.Text), new SqlParameter("@Password", txtPassword.Text));

            switch (MatchingUserID) {
                case -1:
                    UserLogin.writeLog(-1, txtUserName.Text, false);
                    Msg.Text = "That login could not be found. Please try again.";
                    break;

                default:
                    G.User.LoginCount = 0;
                    loginSuccess(MatchingUserID);
                    break;
            }
        }

        private void loginSuccess(int UserID) {
            UserLogin.loginUserByID(UserID);
            string szStartPage = "main/sales_dashboard.aspx";
            if (blnLeave) {
                szStartPage = "payroll/leave_manager_dashboard.aspx";
            } else if (G.User.IsAdmin) {
                szStartPage = "main/admin_dashboard.aspx";
            } else if (G.User.hasPermission(RolePermissionType.ViewCampaignModule)) {
                szStartPage = "campaign/campaign_dashboard.aspx";
            } else if (G.User.RoleID == 5) {
                szStartPage = "payroll/payroll_dashboard.aspx";
            }
            sbEndJS.AppendFormat("window.top.location.href = '{0}';", szStartPage);
            pPage.Visible = false;
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