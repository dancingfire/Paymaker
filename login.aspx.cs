using Microsoft.ApplicationBlocks.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI;
using System.Web;

namespace Paymaker {

    public partial class Paymaker_Form : Root {
        protected System.Data.SqlClient.SqlConnection cnn;
        private string szForgotUserNameEntered = string.Empty;
        bool blnLeave = false;
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

            if (UserLogin.checkLoginLock(txtUserName.Text)) {
                G.Notifications.addPageNotification(PageNotificationType.Warning, "Too many login attempts", "This account has been locked for 1 minute. ", true);
                G.Notifications.showPageNotification(true);
                sbEndJS.Append("$('#pPage').hide();");
            }
        }

        private void performLogin() {
            // If there is a login lock before the Login is performed, then Login not performed
            if (UserLogin.checkLoginLock(txtUserName.Text))
                return;

            int intResult = (int)SqlHelper.ExecuteScalar(DB.DBConn, CommandType.StoredProcedure,
                "getUserByLogin", new SqlParameter("@UserName", txtUserName.Text), new SqlParameter("@Password", txtPassword.Text));

            switch (intResult) {
                case -1:
                    UserLogin.writeLog(-1, txtUserName.Text, false);
                    Msg.Text = "That login could not be found. Please try again.";

                    if (UserLogin.checkLoginLock(txtUserName.Text)) {
                        G.Notifications.addPageNotification(PageNotificationType.Warning, "Too many login attempts", "This account has been locked for 1 minute. ", true);
                        G.Notifications.showPageNotification(true);
                        sbEndJS.Append("$('#pPage').hide();");
                    }
                    break;

                default:
                    G.User.LoginCount = 0;
                    loginSuccess(intResult);
                    break;
            }
        }

        private void loginSuccess(int intResult) {
            G.User.ID = intResult;
            string szSQL = "SELECT * FROM DB_USER WHERE ID = " + intResult.ToString();

            SqlDataReader dr = DB.runReader(szSQL);
            if (dr.HasRows) {
                dr.Read();
                UserLogin.writeLog(intResult, Convert.ToString(dr["LOGIN"]), true);
                Session["LOGIN"] = Convert.ToString(dr["LOGIN"]);
                G.User.UserName = Convert.ToString(dr["FirstName"] + " " + dr["LastName"]);
                G.User.AdminPAForThisUser = DB.readInt(dr["ADMINPAFORUSERID"]);
                G.User.Email = Convert.ToString(dr["EMAIL"]);
                G.User.RoleID = Convert.ToInt32(dr["ROLEID"]);
                G.User.PayrollTypeID = Convert.ToInt32(dr["PAYROLLCYCLEID"]);
                if (String.IsNullOrEmpty(dr["PERMISSIONS"].ToString()))
                    G.CurrentUserPermissions = "";
                else
                    G.CurrentUserPermissions = dr["PERMISSIONS"].ToString();
                FormsAuthentication.SetAuthCookie(Session["LOGIN"].ToString(), false);
                string szStartPage = "main/sales_dashboard.aspx";
                if (blnLeave) {
                    szStartPage = "payroll/leave_manager_dashboard.aspx";
                } else  if (G.User.RoleID == 1) {
                    szStartPage = "main/admin_dashboard.aspx";
                } else if (G.User.hasPermission(RolePermissionType.ViewCampaignModule)) {
                    szStartPage = "campaign/campaign_dashboard.aspx";
                } else if (G.User.RoleID == 5) {
                    szStartPage = "payroll/payroll_dashboard.aspx";
                }
                sbEndJS.AppendFormat("window.top.location.href = '{0}';", szStartPage);

                Application.UnLock();
                pPage.Visible = false;
                //Clear the supervisor variable - it is set as required
                HttpContext.Current.Session["ISPAYROLLSUPERVISOR"] = null;
                // Load up the session vars
                szSQL = "SELECT TOP 1 * FROM PAYPERIOD ORDER BY ID DESC";
                SqlDataReader drSettings = DB.runReader(szSQL);
                while (drSettings.Read()) {
                    G.CurrentPayPeriod = Convert.ToInt32(drSettings["ID"]);
                    G.CurrentPayPeriodStart = Convert.ToDateTime(drSettings["STARTDATE"]);
                    G.CurrentPayPeriodEnd = Convert.ToDateTime(drSettings["ENDDATE"]);
                }

                G.Settings.loadConfigValues();
                drSettings.Close();
                drSettings = null;
            }
            dr.Close();
            dr = null;
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