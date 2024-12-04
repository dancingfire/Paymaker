using Sentry;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for RolePermissionType
/// </summary>
///
public enum RolePermissionType {

    // ---- Role permission constants ----
    // 1. Users
    ViewPermissionButton = 0,

    UpdatePreviousPayPeriod = 75,   //user can update details like Date Started and Completed
    ViewCampaignModule = 80,        //View the campaign module and related functionality
    ViewCommissionModule = 90,      // View the commission module and related functionality
    DeleteLeaveRequests = 95,
    ReportExpenseSummary = 100
}

public enum RolePermissionGroupType {
    SalePermissions = 1,
    UserPermissions = 2,
    ApplicationPermissions = 3,
    Reports = 4,
    Leave = 5
}

public class RolePermissions {
    private RolePermission[] arRolePermissions = null;
    private SortedList slRolePermissions = new SortedList();
    private bool blnPermissionsLoaded = false;

    /// <summary>
    /// Array of role permissions, sorted by Role Permission Group
    /// </summary>
    public RolePermission[] PermissionList {
        get { return arRolePermissions; }
    }

    private RolePermissions() {
    }

    public static RolePermissions getRolePermissions() {
        RolePermissions oReturn = new RolePermissions();
        oReturn.loadRolePermissions();
        return oReturn;
    }

    /// <summary>
    /// Returns whether the passed in permissions can perform the permitted role.
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Permissions"></param>
    /// <returns></returns>
    public static bool isPermitted(RolePermissionType Type, string Permissions) {
        return Utility.InCommaSeparatedString(((int)Type).ToString(), Permissions);
    }

    private void loadRolePermissions() {
        if (blnPermissionsLoaded)
            return;

        slRolePermissions.Add(slRolePermissions.Count, new RolePermission(RolePermissionType.UpdatePreviousPayPeriod, "Update previous pay period", "Permits a person to update sales that belong to past pay periods", true, RolePermissionGroupType.SalePermissions));
        slRolePermissions.Add(slRolePermissions.Count, new RolePermission(RolePermissionType.ViewPermissionButton, "Update user permissions", "Permits a person to update other users' permissions", true, RolePermissionGroupType.UserPermissions));
        slRolePermissions.Add(slRolePermissions.Count, new RolePermission(RolePermissionType.ViewCampaignModule, "View campaigns", "Show the campaign functionality", true, RolePermissionGroupType.ApplicationPermissions));
        slRolePermissions.Add(slRolePermissions.Count, new RolePermission(RolePermissionType.ViewCommissionModule, "View commissions", "Show the commission functionality", true, RolePermissionGroupType.ApplicationPermissions));
        slRolePermissions.Add(slRolePermissions.Count, new RolePermission(RolePermissionType.DeleteLeaveRequests, "Delete leave requests", "Delete leave requests", true, RolePermissionGroupType.Leave));
        slRolePermissions.Add(slRolePermissions.Count, new RolePermission(RolePermissionType.ReportExpenseSummary, "Expense summary for promotion expenses", "View this report", true, RolePermissionGroupType.Reports));
        arRolePermissions = new RolePermission[slRolePermissions.Count];
        foreach (DictionaryEntry oItem in slRolePermissions) {
            arRolePermissions[Convert.ToInt32(oItem.Key)] = (RolePermission)oItem.Value;
        }

        blnPermissionsLoaded = true;
    }

    public static string getRolePermissionGroupName(RolePermissionGroupType oGroupType) {
        switch (oGroupType) {
            case RolePermissionGroupType.SalePermissions:
                return "Sale permissions";
            case RolePermissionGroupType.ApplicationPermissions:
                return "Application modules";
            case RolePermissionGroupType.UserPermissions:
                return "User permissions";
            case RolePermissionGroupType.Reports:
                return "Report visibility";
            case RolePermissionGroupType.Leave:
                return "Leave module";
        }
        return "group name not found";
    }
}

public class RolePermission {
    private RolePermissionType oType = RolePermissionType.UpdatePreviousPayPeriod;
    private string szLabel = "";
    private string szHelp = "";
    private bool blnIsActive = false;
    private RolePermissionGroupType oGroup = RolePermissionGroupType.SalePermissions;

    public RolePermissionType PermissionType {
        get { return oType; }
    }

    public int PermissionTypeAsInt {
        get { return (int)oType; }
    }

    public string Label {
        get { return szLabel; }
    }

    public string Help {
        get { return szHelp; }
    }

    public bool IsActive {
        get { return blnIsActive; }
    }

    public RolePermissionGroupType PermissionGroup {
        get { return oGroup; }
    }

    public RolePermission(RolePermissionType oRolePermissionType, string Label, string Help, bool IsActive, RolePermissionGroupType oRolePermissionGroup) {
        this.oType = oRolePermissionType;
        this.szLabel = Label;
        this.szHelp = Help;
        this.blnIsActive = IsActive;
        this.oGroup = oRolePermissionGroup;
    }
}

public class UserLogin {
    public static string resetPasswordEmailFrom = EmailSettings.SMTPServerUserName;

    /// <summary>
    /// Validates the password passed in based on the rules. If the password is valid, the function will return an empty string, else the errors that
    /// are the cause of the issue.
    ///
    /// </summary>
    /// <param name="Pwd"></param>
    /// <returns></returns>
    public static string validatePassword(int UserID, string Pwd) {
        string szError = "";

        if (Pwd.Length < 8) {
            szError += "Error";
        }

        int PatternMatch = 0;
        if (Regex.IsMatch(Pwd, "[A-Z]"))
            PatternMatch++;
        if (Regex.IsMatch(Pwd, "[a-z]"))
            PatternMatch++;
        if (Regex.IsMatch(Pwd, "[\\d\\W]"))
            PatternMatch++;

        if (PatternMatch < 3) {
            szError += @"Error";
        }

        return szError;
    }

    /// <summary>
    /// Returns whether there are consecutive characters (1234, abc) at least three in a row
    /// </summary>
    /// <param name="Pwd"></param>
    /// <returns></returns>
    private static bool validateConsecutiveSeq(String Pwd) {
        char[] epinCharArray = Pwd.ToCharArray();
        int asciiCode = 0;
        bool isConSeq = false;
        int previousAsciiCode = 0;
        int numSeqcount = 0;

        for (int i = 0; i < epinCharArray.Length; i++) {
            asciiCode = epinCharArray[i];
            if ((previousAsciiCode + 1) == asciiCode) {
                numSeqcount++;
                if (numSeqcount >= 2) {
                    isConSeq = true;
                    break;
                }
            } else {
                numSeqcount = 0;
            }
            previousAsciiCode = asciiCode;
        }
        return isConSeq;
    }

    /// <summary>
    /// Updates the password - if there are any validation errors the return string will contain the errors. A success is indicated by an empty string
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="NewPwd"></param>
    public static string updatePassword(int UserID, string NewPwd) {
        string szErrors = validatePassword(UserID, NewPwd);

        if (szErrors != "")
            return szErrors;

        string szUpdateTokenValue = "";

        string szSQL = String.Format(@"
                UPDATE DB_USER
                SET PASSWORD = convert(varbinary(255), PWDENCRYPT('{1}')),
                UNIQUEID = newid()
                WHERE ID = {0} ", UserID, DB.escape(NewPwd), DB.escape(szUpdateTokenValue));
        DB.runNonQuery(szSQL);
        return "";
    }

    /// <summary>
    /// Logout out of the forms authentication
    /// </summary>
    public static void logout() {
        FormsAuthentication.SignOut();
        HttpContext.Current.Session.Abandon();
        // clear authentication cookie
        HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
        cookie1.Expires = DateTime.Now.AddYears(-1);
        HttpContext.Current.Response.Cookies.Add(cookie1);

        SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
        HttpCookie cookie2 = new HttpCookie(sessionStateSection.CookieName, "");
        cookie2.Expires = DateTime.Now.AddYears(-1);
        HttpContext.Current.Response.Cookies.Add(cookie2);
       // HttpContext.Current.Response.Redirect(String.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/logout", G.Settings.SAML.SSOTenant));
    }
    /// <summary>
    /// Resets the login for users
    /// </summary>
    /// <param name="UserName"></param>
    public static bool resetPassword(string UserName) {
        if (!String.IsNullOrEmpty(UserName)) {
            if (!String.IsNullOrEmpty(UserName)) {
                string szSQL = string.Format(@"
                    SELECT *
                    FROM [dbo].[DB_USER] DU
                    WHERE DU.EMAIL = '{0}' AND ISDELETED = 0  AND ISACTIVE = 1", DB.escape(UserName)
                );
                DataSet ds = DB.runDataSet(szSQL);
                if (ds.Tables[0].Rows.Count == 0) {
                    return false;
                } else {
                    DataRow dr = ds.Tables[0].Rows[0];
                    // Set timeout for password
                    DB.runNonQuery(string.Format(@"
                        UPDATE DB_USER SET PASSWORDRESETTIMEOUT = DATEADD(HOUR, 2, GETDATE()) WHERE ID = {0}", DB.readInt(dr["ID"])));
                    UserLogin.prepareEmail(DB.readString(dr["UNIQUEID"]), DB.readString(dr["EMAIL"]));
                    return true;
                }
            }
        }
        return false;
    }

    private static int checkLoginCount(string IP, string LoginName, int lastNumberOfMinutes) {
        // Check number of times user has failed attempt to login in given timeframe
        // Note successful login resets count to 0.
        int CountUser = DB.getScalar(String.Format(@"
            SELECT COUNT(*)
            FROM LOGINLOG WHERE
                USERNAME = '{0}'
                AND ID > (SELECT MAX(ID) FROM LOGINLOG WHERE USERNAME = '{0}' AND ISSUCCESS = 1)
                AND LOGINDATE > DATEADD(MINUTE, -{1}, GETDATE())
            ", DB.escape(LoginName), lastNumberOfMinutes), 0);

        // Check number of times IP has failed attempt to login in given timeframe
        // Note successful login resets count to 0.
        int CountIP = DB.getScalar(String.Format(@"
            SELECT COUNT(*)
            FROM LOGINLOG WHERE
                IPADDRESS = '{0}'
                AND ID > (SELECT MAX(ID) FROM LOGINLOG WHERE IPADDRESS = '{0}' AND ISSUCCESS = 1)
                AND LOGINDATE > DATEADD(MINUTE, -{1}, GETDATE())
            ", DB.escape(IP), lastNumberOfMinutes), 0);

        // Returns the higher of the two
        if (CountIP > CountUser)
            return CountIP;
        return CountUser;
    }

    ///<summary>
    ///Log Attempt to the Log History Table
    ///</summary>
    ///<param name = ""></param>
    public static bool checkLoginLock(string LoginName) {
        string IP = getIPAddress();
        if(LoginName == "") {
            return false;
        }
        // Count of failed attempts in last 1 minute (if zero - then no lock in place)
        if (checkLoginCount(IP, LoginName, 1) == 0)
            return false;

        // Count of failed attempts in last 1 minutes, if more than
        // 4 then user is locked out for 1 minute after a failed attempt.
        if (checkLoginCount(IP, LoginName, 1) > 4)
            return true;

        return false;
    }

    /// <summary>
    /// Clears config setup for user.  This is done on delegation to allow correct values to be loaded.
    /// </summary>
    private static void clearUserConfig() {
        HttpContext.Current.Session["DASHBOARDPOSITION"] = null;
        HttpContext.Current.Session["SHOWRECENT"] = null;
    }

    public static void performDelegateLogin(int UserID) {
        UserLogin.loginUserByID(UserID);
        clearUserConfig();
        G.Settings.loadConfigValues();
    }

    public static bool loginUserByEmail(string Email) {
        int UserID = DB.getScalar(String.Format("SELECT ID FROM DB_USER WHERE (EMAIL = '{0}' OR Login = '{0}') AND ISDELETED = 0 AND ISACTIVE = 1 ORDER BY ROLEID", DB.escape(Email)), -1);
        if (UserID < 0) {
            return false;
        }
        UserLogin.loginUserByID(UserID);
        clearUserConfig();
        G.Settings.loadConfigValues();
        return true;
    }

    /// <summary>
    /// Initial page of the application
    /// </summary>
    /// <returns></returns>
    public static string getStartPage() {
        string szStartPage = "main/sales_dashboard.aspx";
        if (G.User.IsLeave) {
            szStartPage = "payroll/leave_manager_dashboard.aspx";
            G.User.IsLeave = false;
        } else if (G.User.IsAdmin) {
            szStartPage = "main/admin_dashboard.aspx";
        } else if (G.User.hasPermission(RolePermissionType.ViewCampaignModule)) {
            szStartPage = "campaign/campaign_dashboard.aspx";
        } else if (G.User.RoleID == 5) {
            szStartPage = "payroll/payroll_dashboard.aspx";
        }
        return szStartPage;
    }
    /// <summary>
    /// Logs in the user by the passed in ID - this can be used as an admin login or the final step on the login page
    /// </summary>
    /// <param name="intResult"></param>
    public static void loginUserByID(int UserID) {
        G.User.ID = UserID;
        string szSQL = "SELECT * FROM DB_USER WHERE ID = " + UserID.ToString();

        using (SqlDataReader dr = DB.runReader(szSQL)) {
            if (dr.HasRows) {
                dr.Read();
                UserLogin.writeLog(UserID, Convert.ToString(dr["EMAIL"]), true);
                HttpContext.Current.Session["LOGIN"] = Convert.ToString(dr["EMAIL"]);
                G.User.UserName = Convert.ToString(dr["FirstName"] + " " + dr["LastName"]);
                G.User.AdminPAForThisUser = DB.readInt(dr["ADMINPAFORUSERID"]);
                G.User.Email = Convert.ToString(dr["EMAIL"]);
                G.User.RoleID = Convert.ToInt32(dr["ROLEID"]);
                G.User.OriginalRoleID = (UserRole)Convert.ToInt32(dr["ROLEID"]);
                if (G.User.OriginalUserID == -1) {
                    G.User.OriginalUserID = Convert.ToInt32(dr["ID"]);
                }
                G.User.OriginalUserID = Convert.ToInt32(dr["ID"]);
                G.User.Name = DB.readString(dr["FIRSTNAME"]);
                G.User.PayrollTypeID = Convert.ToInt32(dr["PAYROLLCYCLEID"]);
                HttpContext.Current.Session["ISLEAVESUPERVISOR"] = null;
                HttpContext.Current.Session["USERIDLISTWITHDELEGATES"] = null;
                if (String.IsNullOrEmpty(dr["PERMISSIONS"].ToString()))
                    G.CurrentUserPermissions = "";
                else
                    G.CurrentUserPermissions = dr["PERMISSIONS"].ToString();
                FormsAuthentication.SetAuthCookie(HttpContext.Current.Session["LOGIN"].ToString(), false);

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
        }
    }

    ///<summary>
    ///Log Attempt to the Log History Table
    ///</summary>
    ///<param name = ""></param>
    public static void writeLog(int UserID, string LoginName, bool IsSuccess) {
        string szSQLInsert = string.Format(@"INSERT INTO LOGINLOG(USERID,USERNAME,ISSUCCESS,IPADDRESS)
                                                 VALUES({0},'{1}',{2},'{3}')", UserID, DB.escape(LoginName), IsSuccess ? 1 : 0, DB.escape(getIPAddress()));
        DB.runNonQuery(szSQLInsert);
    }

    protected static string getIPAddress() {
        System.Web.HttpContext context = System.Web.HttpContext.Current;
        string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        if (!string.IsNullOrEmpty(ipAddress)) {
            string[] addresses = ipAddress.Split(',');
            if (addresses.Length != 0) {
                return addresses[0];
            }
        }

        return context.Request.ServerVariables["REMOTE_ADDR"];
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="Token"></param>
    /// <param name="ForPsswdUserEmail"></param>
    /// <param name="UserID">For logging purposes UserID must be provided</param>
    private static void prepareEmail(string Token, string ForPsswdUserEmail) {
        string To = ForPsswdUserEmail;

        string strMailContent = string.Format(@"
             <html>
                 <body>
                     <p> You have requested to reset your password.</p>
                 <p> Please click  <a href='{1}/reset_password.aspx?t={0}'>here</a> to reset your password</p>
                 </body>
             </html>", Token, G.Settings.DomainName);

        try {
            Email.sendMail(To, EmailSettings.SMTPServerUserName, "Reset password", strMailContent, UserID: 0);
            G.Notifications.addPageNotification(PageNotificationType.Success, "Email sent", "The email with reset link has been sent", true);
            G.Notifications.showPageNotification(true);
        } catch {
            throw;
        }
    }
}