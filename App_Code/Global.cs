using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

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

    /// <summary>
    /// Pay period indo
    /// </summary>
    public static PayPeriodInformation PayPeriodInfo {
        get {
            return PayPeriodInformation.getInfo();
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
            if (HttpContext.Current.Session == null || HttpContext.Current.Session["blnEnableTrace"] == null)
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

                return HttpContext.Current.Session["USEREMAIL"].ToString();
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

        public static string UserName {
            get {
                checkSessionValue("USERNAME");
                return HttpContext.Current.Session["USERNAME"].ToString();
            }
            set {
                HttpContext.Current.Session["USERNAME"] = value;
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

        public static string SMTPServer {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["EmailServer"];
            }
        }

        public static string CampaignTrackOffice {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["CampaignTrackOffice"];
            }
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
                return checkSessionVar("SUPERANNUATIONMAX", 9.25);
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
/// A user record in the system
/// </summary>
public class UserDetail {

    /// <summary>
    /// This contains the users who are moving from Teams to be proper sales people and require special handling at year end
    /// </summary>
    public static List<int> SpecialYearEndUserIDs = new List<int>() { 153, 207, 163 };

    public int ID { get; set; }
    public int MentorID { get; set; }
    public int RoleID { get; set; }
    public int Salary { get; set; }
    public int OfficeID { get; set; }
    public int PayrollCycleID { get; set; }
    public int SupervisorID { get; set; }
    public string FirstName { get; set; }
    public string OfficeGLCode { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Initials { get; set; }
    public string GLSubAccount {
        get {
            return OfficeGLCode.Replace("BA", "FL").Replace("CA", "FL").Replace("DO", "FL") + "-" + Initials;
        }
    }

    public PayBand PaymentStructure {
        get;
        set;
    }

    /// <summary>
    /// Gets the payment structure based on the date of the property sale
    /// </summary>
    /// <param name="SaleDate"></param>
    /// <returns></returns>
    public PayBand getPaymentStructure(DateTime SaleDate) {
        PayBand oP = PaymentStructure;

        if (!UserDetail.SpecialYearEndUserIDs.Contains(ID)) {
            return oP;
        }

        //Last year they were in a team with a salary
        if (SaleDate < new DateTime(2015, 7, 1))
            return PayBand.JuniorTeamSalary;
        else
            return oP;
    }

    private Hashtable htYTDTotals = new Hashtable();
    public string CreditGL { get; set; }
    public string DebitGL { get; set; }
    public string OfficeJobCode { get; set; }

    public string Name { get { return LastName + ", " + FirstName + "(" + Initials + ")"; } }

    public string NameFLI { get { return FirstName + " " + LastName + "(" + Initials + ")"; } }

    public UserDetail(int ID, string Initials, string First, string Last, string Email, int RoleID, int OfficeID, string OfficeGLCode, int MentorID, int Salary, int PayrollCycleID, int SupervisorID, string CreditGLCode, string DebitGLCode, string JobCode) {
        this.ID = ID;
        this.Initials = Initials;
        this.FirstName = First;
        this.LastName = Last;
        this.Email = Email;
        this.RoleID = RoleID;
        this.MentorID = MentorID;
        this.OfficeID = OfficeID;
        this.Salary = Salary;
        this.OfficeGLCode = OfficeGLCode;
        this.CreditGL = CreditGLCode;
        this.DebitGL = DebitGLCode;
        this.OfficeJobCode = JobCode;
        this.PayrollCycleID = PayrollCycleID;
        this.SupervisorID = SupervisorID;

        if (MentorID <= 0 && Salary == 0) {
            PaymentStructure = PayBand.Normal;
        } else if (MentorID > 0) { //Belong to a team
            if (Salary == 0)
                PaymentStructure = PayBand.JuniorTeamRetainer;
            else
                PaymentStructure = PayBand.JuniorTeamSalary;
        } else { // No team
            if (Salary == 0)
                PaymentStructure = PayBand.JuniorNoTeamRetainer;
            else {
                PaymentStructure = PayBand.JuniorNoTeamSalary;
                if (Salary == 85000)
                    PaymentStructure = PayBand.SpecialCase85kBase;
            }
        }
    }

    /// <summary>
    /// Checks the passed in user ID list to ensure that it contains the special IDs
    /// </summary>
    /// <param name="UserIDList"></param>
    /// <returns></returns>
    public static string checkUserIDListForSpecialIDs(string UserIDList) {
        if (String.IsNullOrEmpty(UserIDList))
            return string.Join(",", UserDetail.SpecialYearEndUserIDs);

        foreach (int ID in UserDetail.SpecialYearEndUserIDs) {
            if (!Utility.InCommaSeparatedString(ID.ToString(), UserIDList))
                Utility.Append(ref UserIDList, ID.ToString(), ",");
        }
        return UserIDList;
    }

    /// <summary>
    /// Returns the YTD total for the selected up to but not including the current pay period month passed in
    /// </summary>
    /// <param name="Year"></param>
    /// <returns></returns>
    public double getYTDTotal(int PayPeriodID) {
        if (PayPeriodID == -1)
            PayPeriodID = G.CurrentPayPeriod;
        PayPeriod oP = G.PayPeriodInfo.getPayPeriod(PayPeriodID);
        DateTime dtStart = DateUtil.ThisFinYear(oP.StartDate).Start;
        return (double)DB.getScalar(String.Format(@"
                SELECT ISNULL(SUM(USS.GRAPHCOMMISSION), 0) AS GRAPHCOMMISSION
                FROM USERSALESPLIT USS JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID JOIN SALE S ON S.ID = SS.SALEID
                    AND S.STATUSID IN (1, 2)
                WHERE  SS.CALCULATEDAMOUNT > 0 AND USS.RECORDSTATUS < 1 AND S.SALEDATE BETWEEN '{0} 00:00' AND '{1} 23:59' AND USS.USERID = {2}
                UNION SELECT 0
                ORDER BY 1 DESC", Utility.formatDate(dtStart), Utility.formatDate(oP.EndDate), ID), 0);
    }

    /// <summary>
    /// Determines, based on the sale date, which YTD totals are meant to be used
    /// </summary>
    /// <param name="CurrYTD"></param>
    /// <param name="PrevYTD"></param>
    /// <param name="SaleDate"></param>
    /// <param name="PayPeriodID"></param>
    /// <returns></returns>
    public double getEffectiveYTDCommission(double CurrYTD, double PrevYearTotal, DateTime SaleDate, int PayPeriodID) {
        double dEffectiveCommission = CurrYTD;
        PayPeriod oP = G.PayPeriodInfo.getPayPeriod(PayPeriodID);
        //If the sales date is in the previous financial year, we have to use the values from the previous year.
        if (DateUtil.ThisFinYear(SaleDate).Start.Year != oP.FinYear) {
            dEffectiveCommission = PrevYearTotal;
        }
        return dEffectiveCommission;
    }

    /// <summary>
    /// Returns the previous YTD total
    /// </summary>
    /// <param name="Year"></param>
    /// <returns></returns>
    public double getPrevYTDTotal(int PayPeriodID) {
        if (PayPeriodID == -1)
            PayPeriodID = G.CurrentPayPeriod;
        PayPeriod oP = G.PayPeriodInfo.getPayPeriod(PayPeriodID);
        DateTime dtStart = DateUtil.ThisFinYear(oP.StartDate).Start.AddYears(-1);
        return (double)DB.getScalar(String.Format(@"
                SELECT ISNULL(SUM(USS.GRAPHCOMMISSION), 0) AS GRAPHCOMMISSION
                FROM USERSALESPLIT USS JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID JOIN SALE S ON S.ID = SS.SALEID
                    AND S.STATUSID IN (1, 2)
                WHERE  SS.CALCULATEDAMOUNT > 0 AND USS.RECORDSTATUS < 1 AND S.SALEDATE BETWEEN '{0} 00:00' AND '{1} 23:59' AND USS.USERID = {2}
                UNION SELECT 0
                ORDER BY 1 DESC", Utility.formatDate(dtStart), Utility.formatDate(dtStart.AddYears(1).AddMinutes(-1)), ID), 0);
    }

    /// <summary>
    /// Returns the YTD total for the selected up to but not including the month passed in
    /// </summary>
    /// <param name="Year"></param>
    /// <returns></returns>
    public double getYTDTotal(DateTime SaleDate) {
        DateTime dtStart = DateUtil.ThisFinYear(SaleDate).Start;
        return (double)DB.getScalar(String.Format(@"
                SELECT ISNULL(SUM(USS.GRAPHCOMMISSION), 0) AS GRAPHCOMMISSION
                FROM USERSALESPLIT USS JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID JOIN SALE S ON S.ID = SS.SALEID
                    AND S.STATUSID IN (1, 2)
                WHERE  SS.CALCULATEDAMOUNT > 0 AND USS.RECORDSTATUS < 1 AND S.SALEDATE BETWEEN '{0} 00:00' AND '{1} 23:59' AND USS.USERID = {2}
                UNION SELECT 0
                ORDER BY 1 DESC", Utility.formatDate(dtStart), Utility.formatDate(new DateTime(SaleDate.Year, SaleDate.Month, DateTime.DaysInMonth(SaleDate.Year, SaleDate.Month))), ID), 0);
    }

    /// <summary>
    /// Calculate the agent annual bonus based on the actual amounts that have been paid
    /// </summary>
    /// <param name="dYTDTotal">The YTD for the period in consideration (generally current date less two months)</param>
    /// <param name="dNewPeriodTotal">The total for the next pay periond in consideration (generally the previous pay period)</param>
    /// <returns></returns>
    public static double calcMentorBonus(double dYTDTotal, double dNewPeriodTotal, int UserID) {
        if (!G.Settings.CreateBonusRecords)
            return 0;
        if (dNewPeriodTotal <= 0)
            return 0;

        double dCommission = 0;
        double dTotalAlreadyPaid = dYTDTotal - dNewPeriodTotal;
        List<CommissionTier> lTiers = new List<CommissionTier>();
        CommissionTier oCTNew = new CommissionTier(1, 0, 100000, 0.2);
        dTotalAlreadyPaid = oCTNew.applyAlreadyPaid(dTotalAlreadyPaid);
        lTiers.Add(oCTNew);

        oCTNew = new CommissionTier(2, 100001, 200000, 0.15);
        dTotalAlreadyPaid = oCTNew.applyAlreadyPaid(dTotalAlreadyPaid);
        lTiers.Add(oCTNew);

        oCTNew = new CommissionTier(3, 200001, 300000, 0.1);
        dTotalAlreadyPaid = oCTNew.applyAlreadyPaid(dTotalAlreadyPaid);
        lTiers.Add(oCTNew);

        oCTNew = new CommissionTier(4, 3000001, 1000000, 0.5);
        dTotalAlreadyPaid = oCTNew.applyAlreadyPaid(dTotalAlreadyPaid);
        lTiers.Add(oCTNew);

        //Find out if we have filled up the lowest commission bonus band
        foreach (CommissionTier oCT in lTiers) {
            if (dNewPeriodTotal <= 0)
                break;
            if (oCT.dRemaining > 0) {
                if (oCT.dRemaining >= dNewPeriodTotal) { //We use up all the outstanding amount to be paid within this tier
                    dCommission += dNewPeriodTotal * oCT.dPercentage;
                    break;
                } else {
                    dCommission += oCT.dRemaining * oCT.dPercentage;
                    dNewPeriodTotal -= oCT.dRemaining;
                }
            }
        }
        return dCommission;
    }

    /// <summary>
    /// Calculate the agent bonus commission structure
    /// </summary>
    /// <param name="dYTDBonusAmountAlreadyPaid">The YTD total NOT including the current period </param>
    /// <param name="dNextPeriodTotal"></param>
    /// <returns></returns>
    public static double calcEOFYBonusScheme(double dYTDBonusAmountAlreadyPaid, DataTable dtSaleCurrentPeriod, int UserID) {
        if (!G.Settings.CreateBonusRecords)
            return 0;

        double dBonus = 0;

        List<CommissionTier> lTiers = Client.getCommissionTiers(dYTDBonusAmountAlreadyPaid, UserID);
        double dCurrSaleCommission = 0;
        //Loop through each row set the commission tier based on the running total
        foreach (DataRow dr in dtSaleCurrentPeriod.Rows) {
            dCurrSaleCommission = DB.readDouble(dr["ELIGIBLE_COMMISSION"]);
            int intUserID = DB.readInt(dr["USERID"]);
            int intSaleID = DB.readInt(dr["SALEID"]);
            int intCommissionTier = -1;
            double dCurrSaleBonus = 0;
            //Find out if we have filled up the lowest commission bonus band
            foreach (CommissionTier oCT in lTiers) {
                intCommissionTier = oCT.ID;
                if (dCurrSaleCommission <= 0)
                    break;
                if (oCT.dRemaining > 0) {
                    if (oCT.dRemaining >= dCurrSaleCommission) { //We use up all the outstanding amount to be paid within this tier
                        dCurrSaleBonus = dCurrSaleCommission * oCT.dPercentage;
                        dBonus += dCurrSaleBonus;
                        oCT.dRemaining -= dCurrSaleCommission;
                        break;
                    } else {    //We are only using up part of the total amount to be paid
                        dCurrSaleBonus += oCT.dRemaining * oCT.dPercentage; //Use up the total remaining
                        dBonus += dCurrSaleBonus;
                        dCurrSaleCommission = dCurrSaleCommission - oCT.dRemaining; //Remove the part we've now paid commission on
                        oCT.dRemaining = 0;
                    }
                }
            }
            DB.runNonQuery(String.Format(@"
                UPDATE USERSALESPLIT
                    SET COMMISSIONTIERID = {0}, EOFYBONUSCOMMISSION = {3}
                WHERE USERID = {1} AND SALESPLITID IN (SELECT ID FROM SALESPLIT WHERE SALEID = {2})
            ", intCommissionTier, intUserID, intSaleID, dCurrSaleBonus));
        }
        return dBonus;
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

public class UserInformation {
    private List<UserDetail> lUsers = new List<UserDetail>();
    private bool blnIsLoaded = false;

    private UserInformation() {
    }

    /// <summary>
    /// List of all the users
    /// </summary>
    public List<UserDetail> UserList {
        get {
            if (!blnIsLoaded)
                loadItems();
            return lUsers;
        }
    }

    public static UserInformation instance {
        get {
            if (HttpContext.Current.Application["UserList"] == null) {
                HttpContext.Current.Application.Lock();
                HttpContext.Current.Application["UserList"] = new UserInformation();
                HttpContext.Current.Application.UnLock();
            }
            return (UserInformation)HttpContext.Current.Application["UserList"];
        }
    }

    /// <summary>
    /// Call this function to get a handle to the class. The class will auto-instantiate itself
    /// </summary>
    /// <returns></returns>
    public static UserInformation getInfo() {
        return instance;
    }

    /// <summary>
    /// Returns the users's name
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public string getName(int UserID) {
        if (!blnIsLoaded)
            loadItems();
        UserDetail oU = getUser(UserID);
        if (oU != null)
            return oU.FirstName + ' ' + oU.LastName;
        else
            return "<not found>";
    }

    /// <summary>
    /// Returns the users's name
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public string getInitials(int UserID) {
        if (!blnIsLoaded)
            loadItems();
        UserDetail oU = getUser(UserID);
        if (oU != null)
            return oU.Initials;
        else
            return "";
    }

    /// <summary>
    /// Return a report type record
    /// </summary>
    /// <param name="ReportTypeID"></param>
    /// <returns></returns>
    public UserDetail getUser(int UserID) {
        if (!blnIsLoaded)
            loadItems();
        return lUsers.Find(i => i.ID == UserID);
    }

    /// <summary>
    /// Return a report type record
    /// </summary>
    /// <param name="ReportTypeID"></param>
    /// <returns></returns>
    public UserDetail getUser(string Initials) {
        if (!blnIsLoaded)
            loadItems();
        return lUsers.Find(i => i.Initials == Initials);
    }

    /// <summary>
    /// To be called if the object is updated
    /// </summary>
    public void forceReload(bool WriteToCache = true) {
        blnIsLoaded = false;
        lUsers.Clear();
        loadItems();
    }

    private void loadItems() {
        string szSQL = @"
            --User info
            SELECT U.ID, INITIALSCODE, FIRSTNAME, LASTNAME, EMAIL, ROLEID, ISNULL(TEAMID, -1) AS TEAM, SALARY, 
             U.CREDITGLCODE, U.DEBITGLCODE, L_OFF.JOBCODE, U.OFFICEID, U.PAYROLLCYCLEID, U.SUPERVISORID, L_OFF.OFFICEMYOBCODE
            FROM DB_USER U  JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID ORDER BY LASTNAME, FIRSTNAME";

        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                int intID = Convert.ToInt32(dr["ID"]);
                lUsers.Add(new UserDetail(intID, DB.readString(dr["INITIALSCODE"]), DB.readString(dr["FIRSTNAME"]), DB.readString(dr["LASTNAME"]), DB.readString(dr["EMAIL"]),
                    DB.readInt(dr["ROLEID"]), DB.readInt(dr["OFFICEID"]), DB.readString(dr["OFFICEMYOBCODE"]), DB.readInt(dr["TEAM"]), DB.readInt(dr["SALARY"]), DB.readInt(dr["PAYROLLCYCLEID"]),
                    DB.readInt(dr["SUPERVISORID"]), DB.readString(dr["CREDITGLCODE"]), DB.readString(dr["DEBITGLCODE"]), DB.readString(dr["JOBCODE"])));
            }
            blnIsLoaded = true;
        }
    }
    /// <summary>
    /// Loads the list from the object
    /// </summary>
    /// <param name="l"></param>
    /// <param name="IncludeSelect"></param>
    public void loadList(ref ListBox l, bool IncludeSelect = true) {
        if (blnIsLoaded == false || lUsers == null)
            loadItems();

        foreach (UserDetail b in lUsers) {
            l.Items.Add(new ListItem(b.NameFLI, b.ID.ToString()));
        }
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