using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using BootstrapWrapper;

/// <summary>
/// The global object that we can access throughout the app...
/// </summary>
public class G {

    private G() {
    }

    ///<summary>Sets the date to transition Sales to import from BnD</summary>
    public static DateTime TransitionToBnDDate { get { return dtTransition; } }

    private static DateTime dtTransition = new DateTime(2016, 07, 01);

    ///<summary>Sets actual switch date when import button first calls B&D - this code can be removed post transition</summary>
    public static DateTime SystemToBnDDate { get { return dtSwitch; } }

    private static DateTime dtSwitch = new DateTime(2016, 07, 12);

    // Page Parameters used when the root page has not been instantiated
    public static G GlobalObject {
        get {
            if (null == HttpContext.Current.Application["GlobalConstants"])
                HttpContext.Current.Application["GlobalConstants"] = new G();
            return (G)HttpContext.Current.Application["GlobalConstants"];
        }
    }

    /// <summary>
    /// The notification manager class
    /// </summary>
    public static PageNotificationManager Notifications {
        get {
            return PageNotificationManager.PageNotificationInfo;
        }
    }

    public static Root oRoot {
        get { return G.GlobalObject._Root; }
        set { G.GlobalObject._Root = value; }
    }

    private Root _Root = null;

    /// <summary>
    /// User information
    /// </summary>
    public static UserInformation UserInfo {
        get {
            return UserInformation.getInfo();
        }
    }

    public static string BaseURL {
        get { return HttpContext.Current.Request.Url.Host; }
    }
  
    /// <summary>
    /// Pay period indo
    /// </summary>
    public static PayPeriodInformation PayPeriodInfo {
        get {
            return PayPeriodInformation.getInfo();
        }
    }

    /// <summary>
    /// Delegate information
    /// </summary>
    public static UserDelegateInformation UserDelegateInfo {
        get {
            return UserDelegateInformation.getInfo();
        }
    }

    /// <summary>
    ///Commission split information
    /// </summary>
    public static CommissionTypeInformation CommTypeInfo {
        get {
            return CommissionTypeInformation.getInfo();
        }
    }

    /// <summary>
    ///Commission tier information
    /// </summary>
    public static CommissionTierInformation CTInfo {
        get {
            return CommissionTierInformation.getInfo();
        }
    }

    public static string szCnn {
        get {
            string szCnn = ConfigurationManager.AppSettings["DB"];
            return szCnn;
        }
    }

    /// <summary>
    /// The currently active pay period
    /// </summary>
    public static int CurrentPayPeriod {
        get {
            checkSessionValue("PAYPERIOD");
            return Convert.ToInt32(HttpContext.Current.Session["PAYPERIOD"]);
        }
        set { HttpContext.Current.Session["PAYPERIOD"] = value; }
    }

    public static Payroll.TimeSheetCycleReferenceList TimeSheetCycleReferences {
        get {
            //
            if (HttpContext.Current.Application["TIMESHEETCR"] == null)
                HttpContext.Current.Application["TIMESHEETCR"] = Payroll.getTimeSheetCycleReferences();

            return (Payroll.TimeSheetCycleReferenceList)HttpContext.Current.Application["TIMESHEETCR"];
        }
        set { HttpContext.Current.Application["TIMESHEETCR"] = value; }
    }

    /// <summary>
    /// Run with additional diagnostic messages
    /// </summary>
    public static bool DebugMode {
        get { return false; }
    }

    /// <summary>
    /// The starting date for the current pay period
    /// </summary>
    public static DateTime CurrentPayPeriodStart {
        get {
            checkSessionValue("PAYPERIODSTARTDATE");
            return Convert.ToDateTime(HttpContext.Current.Session["PAYPERIODSTARTDATE"]);
        }
        set { HttpContext.Current.Session["PAYPERIODSTARTDATE"] = value; }
    }

    /// <summary>
    /// The ending date for the current pay period
    /// </summary>
    public static DateTime CurrentPayPeriodEnd {
        get {
            checkSessionValue("PAYPERIODENDDATE");
            return Convert.ToDateTime(HttpContext.Current.Session["PAYPERIODENDDATE"]);
        }
        set { HttpContext.Current.Session["PAYPERIODENDDATE"] = value; }
    }

    private static void checkSessionValue(string szValue) {
        if (HttpContext.Current.Session == null || HttpContext.Current.Session[szValue] == null) {
            HttpContext.Current.Response.Redirect("~/login.aspx?Timeout=true&sv=" + szValue);
        }
    }

    public static int checkSessionValue(string szValue, bool RedirectToLoginOnNull, int Default = Int32.MinValue) {
        if (HttpContext.Current.Session[szValue] == null) {
            if (RedirectToLoginOnNull) {
                try {
                    HttpContext.Current.Response.Redirect("~/login.aspx?Timeout=true&sv=" + szValue);
                } catch { }
            } else {
                lock (HttpContext.Current.Session) {
                    if (HttpContext.Current.Session[szValue] != null)
                        return Convert.ToInt32(HttpContext.Current.Session[szValue]);

                    HttpContext.Current.Session[szValue] = Default;
                }
            }
            return Default;
        }

        return Convert.ToInt32(HttpContext.Current.Session[szValue]);
    }

    public static DateTime checkSessionValue(string szValue, bool RedirectToLoginOnNull, DateTime Default) {
        if (HttpContext.Current.Session[szValue] == null) {
            if (RedirectToLoginOnNull)
                HttpContext.Current.Response.Redirect("~/login.aspx?Timeout=true&sv=" + szValue);
            else {
                lock (HttpContext.Current.Session) {
                    if (HttpContext.Current.Session[szValue] != null)
                        return Convert.ToDateTime(HttpContext.Current.Session[szValue]);

                    HttpContext.Current.Session[szValue] = Default;
                }
            }
            return Default;
        } else {
            return Convert.ToDateTime(HttpContext.Current.Session[szValue]);
        }
    }

    public static string checkSessionValue(string szValue, bool RedirectToLoginOnNull, string Default = "") {
        if (HttpContext.Current.Session[szValue] == null) {
            if (RedirectToLoginOnNull)
                HttpContext.Current.Response.Redirect("~/login.aspx?Timeout=true&sv=" + szValue);
            else {
                lock (HttpContext.Current.Session) {
                    if (HttpContext.Current.Session[szValue] != null)
                        return Convert.ToString(HttpContext.Current.Session[szValue]);

                    HttpContext.Current.Session[szValue] = Default;
                }
            }
            return Default;
        }

        return Convert.ToString(HttpContext.Current.Session[szValue]);
    }
    private static bool checkSessionValue(string szValue, bool RedirectToLoginOnNull, bool Default = false) {
        if (HttpContext.Current.Session[szValue] == null) {
            if (RedirectToLoginOnNull)
                HttpContext.Current.Response.Redirect("~/login.aspx?Timeout=true&sv=" + szValue);
            else {
                lock (HttpContext.Current.Session) {
                    if (HttpContext.Current.Session[szValue] != null)
                        return Convert.ToBoolean(HttpContext.Current.Session[szValue]);

                    HttpContext.Current.Session[szValue] = Default;
                }
            }
            return Default;
        } else {
            return Convert.ToBoolean(HttpContext.Current.Session[szValue]);
        }
    }

    public static string CurrentUserPermissions {
        get {
            if (HttpContext.Current.Session["SESSIONROLEPERMISSIONS"] != null)
                return HttpContext.Current.Session["SESSIONROLEPERMISSIONS"].ToString();
            return "";
        }
        set { HttpContext.Current.Session["SESSIONROLEPERMISSIONS"] = value; }
    }

    public static string CurrentPage {
        get { return HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString(); }
    }

    /// <summary>
    /// Appends the SQL trace variable to all queries set to the DB
    /// </summary>
    public static bool EnableSQLTrace {
        get {
            if (HttpContext.Current == null || HttpContext.Current.Session == null || HttpContext.Current.Session["blnEnableTrace"] == null)
                return false;
            return HttpContext.Current.Session["blnEnableTrace"].ToString().ToUpper() == "TRUE";
        }
        set {
            HttpContext.Current.Session["blnEnableTrace"] = value.ToString();
        }
    }

    /// <summary>
    /// Utility classes
    /// </summary>
    public static class User {

        public static int ID {
            get {
                checkSessionValue("USERID");
                return Convert.ToInt32(HttpContext.Current.Session["USERID"]);
            }
            set { HttpContext.Current.Session["USERID"] = value; }
        }

        public static string Email {
            get {
                checkSessionValue("USEREMAIL");

                return Convert.ToString(HttpContext.Current.Session["USEREMAIL"]);
            }
            set {
                //Check for emails that are multiple and strip out the second one

                string szEmail = value;
                if (szEmail.Contains(";")) {
                    szEmail = szEmail.Substring(0, szEmail.IndexOf(';'));
                }
                HttpContext.Current.Session["USEREMAIL"] = szEmail;
            }
        }

        /// <summary>
        /// Payroll type 0 - none, 1 - Normal, 2 - Paid in advance
        /// </summary>
        public static int PayrollTypeID {
            get {
                checkSessionValue("PAYROLLTYPEID");
                return Convert.ToInt32(HttpContext.Current.Session["PAYROLLTYPEID"]);
            }
            set { HttpContext.Current.Session["PAYROLLTYPEID"] = value; }
        }

        /// <summary>
        /// Returns whether the current user can perform this action
        /// </summary>
        /// <param name="oPerm"></param>
        /// <returns></returns>
        public static bool hasPermission(RolePermissionType oPerm) {
            return Utility.InCommaSeparatedString(((int)oPerm).ToString(), G.CurrentUserPermissions);
        }


        /// <summary>
        /// Is this person an admin user
        /// </summary>
        public static bool IsAdmin {
            get {
                return G.User.OriginalRoleID == UserRole.Admin;
            }
        }
        // Checks if user has access to Campaign pages
        // Implemented in user because multiple pages need to check this
        public static bool hasCampaignAccess {
            get {
                return G.User.hasPermission(RolePermissionType.ViewCampaignModule) || G.User.RoleID == 6;
            }
        }

        public static bool IsImportDone {
            get {
                if (HttpContext.Current.Application["IMPORTCONTROL"] == null)
                    return false;
                else
                    return Convert.ToBoolean(HttpContext.Current.Application["IMPORTCONTROL"]);
            }
            set {
                HttpContext.Current.Application["IMPORTCONTROL"] = value;
            }
        }

        /// <summary>
        /// The list of user IDs that are delegated to this person - includes their own ID
        /// </summary>
        public static string UserIDListWithDelegates {
            get {
                
                if (HttpContext.Current.Session["USERIDLISTWITHDELEGATES"] == null) {
                    HttpContext.Current.Session["USERIDLISTWITHDELEGATES"] = G.UserDelegateInfo.getIDsDelegatedToThisUser(G.User.OriginalUserID, true);
                    return UserIDListWithDelegates;
                } else {
                    return Convert.ToString(HttpContext.Current.Session["USERIDLISTWITHDELEGATES"]);
                }
            }
        }
        /// <summary>
        /// The number of times that this user has tried to log in
        /// </summary>
        public static int LoginCount {
            get {
                if (HttpContext.Current.Session["LOGINCOUNT"] == null)
                    return 0;
                return Convert.ToInt32(HttpContext.Current.Session["LOGINCOUNT"]);
            }
            set { HttpContext.Current.Session["LOGINCOUNT"] = value; }
        }

        /// <summary>
        /// Will be true in the sale processing if the current sale has  been exported to MYOB
        /// </summary>
        public static bool IsCurrentSaleExportedToMYOB {
            get {
                if (HttpContext.Current.Application["CURRENTSALEEXPORTED"] == null)
                    return false;
                else
                    return Convert.ToBoolean(HttpContext.Current.Application["CURRENTSALEEXPORTED"]);
            }
            set {
                HttpContext.Current.Application["CURRENTSALEEXPORTED"] = value;
            }
        }

        public static int ImportTotal {
            get {
                if (HttpContext.Current.Application["IMPORTCOUNTTOTAL"] == null)
                    return -1;
                else
                    return Convert.ToInt32(HttpContext.Current.Application["IMPORTCOUNTTOTAL"]);
            }

            set {
                HttpContext.Current.Application["IMPORTCOUNTTOTAL"] = value;
            }
        }

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        public static bool IsLoggedIn {
            get {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["USERID"] == null) {
                    return false;
                }
                return true;
            }
        }

        public static int UserID {
            get {
                checkSessionValue("USERID");
                return Convert.ToInt32(HttpContext.Current.Session["USERID"]);
            }
            set { HttpContext.Current.Session["USERID"] = value; }
        }

        public static int RoleID {
            get {
                checkSessionValue("ROLEID");
                return Convert.ToInt32(HttpContext.Current.Session["ROLEID"]);
            }
            set { HttpContext.Current.Session["ROLEID"] = value; }
        }

        /// <summary>
        /// A generated GUID that is used to link an emails that is sent out
        /// </summary>
        public static string EmailGUID {
            get {
                if (HttpContext.Current.Session["USEREMAILGUID"] == null)
                    return new Guid().ToString();
                else
                    return Convert.ToString(HttpContext.Current.Session["USEREMAILGUID"]);
            }

            set {
                HttpContext.Current.Session["USEREMAILGUID"] = value;
            }
        }

        /// <summary>
        /// The path to this users' PDF reports
        /// </summary>
        public static string getPDFDir(int UserID) {
            return HttpContext.Current.Server.MapPath("../PDF/" + UserID + "/");
        }

        public static int ImportCurrentRecord {
            get {
                if (HttpContext.Current.Application["IMPORTCOUNTCURRENT"] == null)
                    return -1;
                else
                    return Convert.ToInt32(HttpContext.Current.Application["IMPORTCOUNTCURRENT"]);
            }

            set {
                HttpContext.Current.Application["IMPORTCOUNTCURRENT"] = value;
            }
        }

        /// <summary>
        /// The original role of the person who is logged in - this will stay the same even if the user delegated to someone else
        /// </summary>
        public static UserRole OriginalRoleID {
            get {
                checkSessionValue("ORIGINALROLEID", true, Int32.MinValue);
                return (UserRole)Convert.ToInt32(HttpContext.Current.Session["ORIGINALROLEID"]);
            }
            set { HttpContext.Current.Session["ORIGINALROLEID"] = value; }
        }


        public static string Name {
            get {
                checkSessionValue("USERNAME", false, "");
                return HttpContext.Current.Session["USERNAME"].ToString();
            }
            set {
                HttpContext.Current.Session["USERNAME"] = value;
            }
        }

        public static string UserName {
            get {
                checkSessionValue("USERNAME");
                return HttpContext.Current.Session["USERNAME"].ToString();
            }
            set {
                HttpContext.Current.Session["USERNAME"] = value;
            }
        }

        /// <summary>
        /// The original login of the person who is logged in - this will stay the same even if the user delegated to someone else
        /// </summary>
        public static int OriginalUserID {
            get {
                checkSessionValue("ORIGINALUSERID", false, -1);
                return Convert.ToInt32(HttpContext.Current.Session["ORIGINALUSERID"]);
            }
            set { HttpContext.Current.Session["ORIGINALUSERID"] = value; }
        }

        /// <summary>
        /// The original login of the person who is logged in - this will stay the same even if the user delegated to someone else
        /// </summary>
        public static string OriginalUserName {
            get {
                checkSessionValue("ORIGINALUSERID");
                return G.UserInfo.getName(Convert.ToInt32(HttpContext.Current.Session["ORIGINALUSERID"]));
            }
        }


        public static int AdminPAForThisUser {
            get {
                checkSessionValue("ADMINPAFORTHISUSER");
                return Convert.ToInt32(HttpContext.Current.Session["ADMINPAFORTHISUSER"]);
            }
            set {
                HttpContext.Current.Session["ADMINPAFORTHISUSER"] = value;
            }
        }
    }

    public static class Settings {

        public static string DataDir {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["AppDataPath"];
            }
        }

        public static string MYOBDir {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["AppMYOBPath"];
            }
        }

        public static string DomainName {
            get { return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath; }
        }

        public static string FileDir {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["AppFilePath"];
            }
        }

        public static int SalesLetterTemplateID {
            get {
                return Convert.ToSByte(System.Configuration.ConfigurationManager.AppSettings["SalesTemplateID"]);
            }
        }

        /// <summary>
        /// The clientID from the web config and run through the ClientID enum
        /// </summary>
        public static ClientID ClientID {
            get {
                return (ClientID)Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ClientID"]);
            }
        }

        public static string CampaignTrackOffice {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["CampaignTrackOffice"];
            }
        }
        public static bool IsRingwood {
            get { return HttpContext.Current.Request.Url.Host == "oecommission.fletchers.net.au"; }
        }

        /// <summary>
        /// Loads up the config values for the application
        /// </summary>
        public static void loadConfigValues() {
            string szSQL = @"
                SELECT ID, NAME, VALUE
                FROM CONFIG
                WHERE USERID = 0";
            SqlDataReader dr = DB.runReader(szSQL);
            while (dr.Read()) {
                HttpContext.Current.Session[dr["NAME"].ToString()] = dr["VALUE"].ToString();
            }
            dr.Close();
            dr = null;
        }

        /// <summary>
        /// The number of days at which we will generate an invoice for the campaign as a 'long-running' campaign
        /// </summary>
        public static int PrePaymentNumberOfDays {
            get {
                return checkSessionVar("PREPAYMENTDAYS", 45);
            }
        }

        /// <summary>
        /// The email address for people who don't have a supervisor email set
        /// </summary>
        public static string CatchAllEmail {
            get {
                return checkSessionVar("LEAVECATCHALLEMAIL", "jacqui.litvik@fletchers.net.au");
            }
        }

        /// <summary>
        /// The amount that will be paid out if there is not enough income on the month
        /// </summary>
        public static double RetainerThreshhold {
            get {
                return checkSessionVar("RETAINERAMOUNT", 3041.67);
            }
        }

        /// <summary>
        /// The percentage that comes off the agent's pay for superannuation
        /// </summary>
        public static double SuperannuationPercentage {
            get {
                return checkSessionVar("SUPERANNUATIONPERCENTAGE", 9.25);
            }
        }

        /// <summary>
        /// The maximum annual amount that can be contributed to super
        /// </summary>
        public static double SuperannuationMaxContribution {
            get {
                return checkSessionVar("SUPERANNUATIONMAX", 1807.85);
            }
        }

        /// <summary>
        /// TheSuper GL account code
        /// </summary>
        public static string SuperGLCode {
            get {
                return checkSessionVar("SUPERGLCODE", "SUPER");
            }
        }

        /// <summary>
        /// People permitted to test the leave system
        /// </summary>
        public static string LeaveTestingUsers {
            get {
                return checkSessionVar("PERMITTEDLEAVETESTERS", "0,178,497");
            }
        }

        /// <summary>
        /// Is the creation of bonus records turned on
        /// </summary>
        public static bool CreateBonusRecords {
            get {
                return checkSessionVar("CALCULATEBONUS", "FALSE") == "TRUE";
            }
        }

        /// <summary>
        /// The outstanding invoice total at which we will generate an invoice
        /// </summary>
        public static int InvoiceTotalThreshold {
            get {
                return checkSessionVar("INVOICETOTALTHRESHOLD", 10000);
            }
        }
    }

    private static bool checkSessionVar(string VarName, bool blnDefault) {
        bool blnMustConfirm = false;
        if (!Boolean.TryParse(Convert.ToString(HttpContext.Current.Session[VarName]), out blnMustConfirm))
            HttpContext.Current.Session[VarName] = blnDefault;
        return Convert.ToBoolean(HttpContext.Current.Session[VarName]);
    }

    private static int checkSessionVar(string VarName, int Default) {
        if (String.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session[VarName])))
            HttpContext.Current.Session[VarName] = Default;
        return Convert.ToInt32(HttpContext.Current.Session[VarName]);
    }

    private static double checkSessionVar(string VarName, double Default) {
        if (String.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session[VarName])))
            HttpContext.Current.Session[VarName] = Default;
        return Convert.ToDouble(HttpContext.Current.Session[VarName]);
    }

    private static string checkSessionVar(string VarName, string szDefault) {
        if (String.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session[VarName])))
            HttpContext.Current.Session[VarName] = szDefault;
        return HttpContext.Current.Session[VarName].ToString();
    }
}


/// <summary>
/// A commission level
/// </summary>
public class CommissionTier {
    public double dLowerAmount = 0;
    public double dUpperAmount = 0;
    public double dPercentage = 0;
    public double dRemaining = 0;
    public int ID = 0;

    public CommissionTier(int ID, double LowerBound, double UpperBound, double PercentPaid) {
        this.ID = ID;
        dLowerAmount = LowerBound;
        dUpperAmount = UpperBound;
        dPercentage = PercentPaid;
        dRemaining = dUpperAmount - dLowerAmount;
    }

    /// <summary>
    /// Removes the amount already allocated from the Remaining amount and returns the difference that still needs to be accounted for, if any
    /// </summary>
    /// <param name="AmountPaid"></param>
    /// <returns></returns>
    public double applyAlreadyPaid(double AmountPaid) {
        if (AmountPaid <= dRemaining) {
            dRemaining = dRemaining - AmountPaid;
            return 0;
        } else {
            double dDiff = AmountPaid - dRemaining;
            dRemaining = 0;
            return dDiff;
        }
    }
}

public class CommissionTypeInformation {
    private List<ListObject> lItems = new List<ListObject>();
    private bool blnIsLoaded = false;

    private CommissionTypeInformation() {
    }

    /// <summary>
    /// List of all the users
    /// </summary>
    public List<ListObject> CommissionTypeList {
        get {
            if (!blnIsLoaded)
                loadItems();
            return lItems;
        }
    }

    public static CommissionTypeInformation instance {
        get {
            if (HttpContext.Current.Application["CommissionTypeList"] == null) {
                HttpContext.Current.Application.Lock();
                HttpContext.Current.Application["CommissionTypeList"] = new CommissionTypeInformation();
                HttpContext.Current.Application.UnLock();
            }
            return (CommissionTypeInformation)HttpContext.Current.Application["CommissionTypeList"];
        }
    }

    /// <summary>
    /// Call this function to get a handle to the class. The class will auto-instantiate itself
    /// </summary>
    /// <returns></returns>
    public static CommissionTypeInformation getInfo() {
        return instance;
    }

    private void loadItems() {
        lItems = new ListTypes(ListType.Commission).ListObjectList;
        blnIsLoaded = true;
    }

    public void forceReload() {
        blnIsLoaded = false;
        loadItems();
    }
}

public class CommissionTierInformation {
    private List<CommissionTier> lItems = new List<CommissionTier>();
    private bool blnIsLoaded = false;

    private CommissionTierInformation() {
    }

    /// <summary>
    /// List of all the tiers
    /// </summary>
    public List<CommissionTier> CommissionTiers {
        get {
            if (!blnIsLoaded)
                loadItems();
            return lItems;
        }
    }

    public static CommissionTierInformation instance {
        get {
            if (HttpContext.Current.Application["CommissionTierInformation"] == null) {
                HttpContext.Current.Application.Lock();
                HttpContext.Current.Application["CommissionTierInformation"] = new CommissionTierInformation();
                HttpContext.Current.Application.UnLock();
            }
            return (CommissionTierInformation)HttpContext.Current.Application["CommissionTierInformation"];
        }
    }

    /// <summary>
    /// Call this function to get a handle to the class. The class will auto-instantiate itself
    /// </summary>
    /// <returns></returns>
    public static CommissionTierInformation getInfo() {
        return instance;
    }

    /// <summary>
    /// Gets the commission tiers filled in with the user's specific data
    /// </summary>
    /// <param name="dYTDCommAlreadyProcessed"></param>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public List<CommissionTier> getCommissionTiersForUser(double dYTDCommAlreadyProcessed, int UserID) {
        loadItems();
        List<CommissionTier> lReturn = new List<CommissionTier>();
        if (G.Settings.ClientID == ClientID.Eltham && UserID == 216) {
            //We have a custom commission tier for Prue at Eltham - a single tier of 10% at 600k
            CommissionTier oTNew = new CommissionTier(-1, 0, 599999, 0);
            dYTDCommAlreadyProcessed = oTNew.applyAlreadyPaid(dYTDCommAlreadyProcessed);
            lReturn.Add(oTNew);
            oTNew = new CommissionTier(-2, 599999, 9999999, 0.1);
            dYTDCommAlreadyProcessed = oTNew.applyAlreadyPaid(dYTDCommAlreadyProcessed);
            lReturn.Add(oTNew);
        } else {
            foreach (CommissionTier oT in lItems) {
                CommissionTier oTNew = new CommissionTier(oT.ID, oT.dLowerAmount, oT.dUpperAmount, oT.dPercentage);
                dYTDCommAlreadyProcessed = oTNew.applyAlreadyPaid(dYTDCommAlreadyProcessed);
                lReturn.Add(oTNew);
            }
        }
        return lReturn;
    }

    private void loadItems() {
        if (blnIsLoaded && lItems.Count > 0)
            return;

        using (DataSet ds = DB.runDataSet("SELECT * FROM COMMISSIONTIER WHERE ID >= 0")) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                CommissionTier oCT = new CommissionTier(DB.readInt(dr["ID"]), DB.readDouble(dr["RANGESTART"]), DB.readDouble(dr["RANGEEND"]), DB.readDouble(dr["PERCENTAGE"]));
                lItems.Add(oCT);
            }
        }
        blnIsLoaded = true;
    }

    public void forceReload() {
        blnIsLoaded = false;
        loadItems();
    }
}


public class PayPeriodCollection : List<PayPeriod> {
}

public class PayPeriodInformation {
    private PayPeriodCollection lPayPeriods = new PayPeriodCollection();
    private bool blnIsLoaded = false;

    private PayPeriodInformation() {
    }

    /// <summary>
    /// List of all the pay periods
    /// </summary>
    public PayPeriodCollection payPeriodList {
        get {
            if (!blnIsLoaded || lPayPeriods.Count == 0)
                loadItems();
            return lPayPeriods;
        }
    }

    public static PayPeriodInformation instance {
        get {
            if (HttpContext.Current.Application["PayPeriodInformation"] == null) {
                HttpContext.Current.Application.Lock();
                HttpContext.Current.Application["PayPeriodInformation"] = new PayPeriodInformation();
                HttpContext.Current.Application.UnLock();
            }
            return (PayPeriodInformation)HttpContext.Current.Application["PayPeriodInformation"];
        }
    }

    /// <summary>
    /// Call this function to get a handle to the class. The class will auto-instantiate itself
    /// </summary>
    /// <returns></returns>
    public static PayPeriodInformation getInfo() {
        return instance;
    }

    /// <summary>
    /// Returns all the payperiods in the current financial year
    /// </summary>
    /// <param name="PayPeriod"></param>
    /// <returns></returns>
    public string getPayPeriodsInCurrYear(int PayPeriod) {
        int FinYear = DateUtil.ThisFinYear(G.PayPeriodInfo.getPayPeriod(PayPeriod).StartDate).Start.Year;
        string szIDs = "";
        foreach (PayPeriod oP in payPeriodList) {
            if (oP.FinYear == FinYear)
                Utility.Append(ref szIDs, oP.ID.ToString(), ", ");
        }
        return szIDs;
    }

    /// <summary>
    /// Returns all the payperiods in the current financial year
    /// </summary>
    /// <param name="PayPeriod"></param>
    /// <returns></returns>
    public string getPayPeriodsForYTD(int PayPeriod = -1, int FinYear = -1) {
        if (PayPeriod != -1)
            FinYear = DateUtil.ThisFinYear(G.PayPeriodInfo.getPayPeriod(PayPeriod).EndDate).Start.Year;
        string szIDs = "";
        foreach (PayPeriod oP in payPeriodList) {
            if (oP.FinYear == FinYear && (oP.ID <= PayPeriod || PayPeriod == -1))
                Utility.Append(ref szIDs, oP.ID.ToString(), ", ");
        }

        return szIDs;
    }

    /// <summary>
    /// Return a report type record
    /// </summary>
    /// <param name="ReportTypeID"></param>
    /// <returns></returns>
    public PayPeriod getPayPeriod(int PayPeriodID) {
        if (!blnIsLoaded)
            loadItems();
        if (lPayPeriods == null || lPayPeriods.Count == 0) {
            forceReload();
        }

        return lPayPeriods.Find(i => i.ID == PayPeriodID);
    }

    /// <summary>
    /// To be called if the object is updated
    /// </summary>
    public void forceReload(bool WriteToCache = true) {
        blnIsLoaded = false;
        lPayPeriods.Clear();
        loadItems();
    }

    private void loadItems() {
        string szSQL = "SELECT * FROM PAYPERIOD ";
        DataSet ds = DB.runDataSet(szSQL);
        foreach (DataRow dr in ds.Tables[0].Rows) {
            lPayPeriods.Add(new PayPeriod(Convert.ToInt32(dr["ID"]), Convert.ToDateTime(dr["STARTDATE"]), Convert.ToDateTime(dr["ENDDATE"])));
        }
        blnIsLoaded = true;
    }
}