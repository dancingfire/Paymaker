using BootstrapWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public enum DelegationType {
    EmailAndLogin = 0,
    LoginOnly = 1,
    EmailOnly = 2
}

public enum UserRole {
    Admin = 1,
    SalesManager = 2,
    SalesPerson = 3,
    Director = 4,
    InternalServices = 5,
    MarketingTeam = 6,
    SalesPA = 7
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
        if (!blnIsLoaded || lUsers == null)
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
    /// Returns the first active user record matching the email address
    /// </summary>
    /// <param name="ReportTypeID"></param>
    /// <returns></returns>
    public UserDetail getUserByEmail(string EmailAddress) {
        EmailAddress = EmailAddress.ToUpper().Trim();
        if (!blnIsLoaded)
            loadItems();
        return lUsers.Find(i => i.Email.ToUpper() == EmailAddress && i.IsActive);
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
            SELECT U.ID, U.ISACTIVE, INITIALSCODE, FIRSTNAME, LASTNAME, EMAIL, ROLEID, ISNULL(TEAMID, -1) AS TEAM, SALARY,
             U.CREDITGLCODE, U.DEBITGLCODE, L_OFF.JOBCODE, U.OFFICEID, U.PAYROLLCYCLEID, U.SUPERVISORID, L_OFF.OFFICEMYOBCODE,
            L_OFF.NAME AS OFFICENAME
            FROM DB_USER U  JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID ORDER BY LASTNAME, FIRSTNAME";

        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                int intID = Convert.ToInt32(dr["ID"]);
                lUsers.Add(new UserDetail(intID, DB.readBool(dr["ISACTIVE"]), DB.readString(dr["INITIALSCODE"]), DB.readString(dr["FIRSTNAME"]), DB.readString(dr["LASTNAME"]), DB.readString(dr["EMAIL"]),
                    DB.readInt(dr["ROLEID"]), DB.readInt(dr["OFFICEID"]), DB.readString(dr["OFFICENAME"]), DB.readString(dr["OFFICEMYOBCODE"]), DB.readInt(dr["TEAM"]), DB.readInt(dr["SALARY"]), DB.readInt(dr["PAYROLLCYCLEID"]),
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
    public void loadList(ref DropDownList l, bool IncludeSelect = true, bool IncludeInactive = false) {
        if (blnIsLoaded == false || lUsers == null)
            loadItems();
        if (IncludeSelect) {
            l.Items.Add(new ListItem("Select a user...", "-1"));
        }
        foreach (UserDetail b in lUsers) {
            if (!IncludeInactive && !b.IsActive)
                continue;

            l.Items.Add(new ListItem(b.NameIFLO, b.ID.ToString()));
        }
    }

    /// <summary>
    /// Loads the list from the object
    /// </summary>
    /// <param name="l"></param>
    /// <param name="IncludeSelect"></param>
    public void loadList(ref bwDropDownList l, bool IncludeSelect = true, bool IncludeInactive = false) {
        if (blnIsLoaded == false || lUsers == null)
            loadItems();
        if (IncludeSelect) {
            l.Items.Add(new ListItem("Select a user...", "-1"));
        }
        foreach (UserDetail b in lUsers) {
            if (!IncludeInactive && !b.IsActive)
                continue;

            l.Items.Add(new ListItem(b.NameIFLO, b.ID.ToString()));
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

public class UserDelegate {

    /// <summary>
    /// A user delegate is when one user permits another to login on their behalf or receive email for a limited period of time
    /// </summary>
    /// <param name="DBID"></param>
    public UserDelegate(int DBID) {
        this.DBID = DBID;
        if (this.DBID > 0)
            loadFromDB();
    }

    public int DBID { get; protected set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
/// <summary>
/// The person who setup the delegation - IE) THis pewrson is going on holidays
/// </summary>
    public int UserID { get; set; }

    /// <summary>
    /// The person responsible for the delegation
    /// </summary>
    public int DelegationUserID { get; set; }
    public bool SendEmailOrigUser { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DelegationType Type { get; set; }

    private void loadFromDB() {
        using (DataSet ds = DB.runDataSet("SELECT * from USERDELEGATION where id = " + DBID)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                StartDate = DB.readDate(dr["STARTDATE"]);
                EndDate = DB.readDate(dr["ENDDATE"]);
                UserID = DB.readInt(dr["USERID"]);
                DelegationUserID = DB.readInt(dr["DELEGATIONUSERID"]);
                SendEmailOrigUser = DB.readBool(dr["SENDTOORIGUSER"]);
                RecordStatus = (RecordStatus)DB.readInt(dr["RECORDSTATUS"]);
                Type = (DelegationType)DB.readInt(dr["DELEGATIONTYPEID"]);
            }
        }
    }

    public int updateToDB() {
        sqlUpdate oSQL = new sqlUpdate("USERDELEGATION", "ID", DBID);
        oSQL.add("STARTDATE", StartDate);
        oSQL.add("ENDDATE", EndDate);
        oSQL.add("USERID", UserID);
        oSQL.add("DELEGATIONUSERID", DelegationUserID);
        oSQL.add("SENDTOORIGUSER", SendEmailOrigUser);
        oSQL.add("RECORDSTATUS", (int)RecordStatus);
        oSQL.add("DELEGATIONTYPEID", (int)Type);
        string szSQL = "";
        if (DBID == -1)
            szSQL = oSQL.createInsertSQL();
        else
            szSQL = oSQL.createUpdateSQL();
        if (szSQL != "") {
            if (DBID < 0) {
                DBID = DB.getScalar(szSQL, -1);
            } else {
                DB.runNonQuery(szSQL);
            }
        }
        return DBID;
    }
}

public class UserDelegateInformation {
    private static readonly object _lock = new object();

    private List<UserDelegate> lDelegates = new List<UserDelegate>();
    private DateTime dtLoadDate = DateTime.MinValue;

    private UserDelegateInformation() {
    }

    /// <summary>
    /// Reload every four hours as the data contained is date sensitive.
    /// </summary>
    private bool DueForReload { get { return DateTime.Now.Subtract(dtLoadDate).TotalHours > 4; } }

    /// <summary>
    /// List of all the delegates
    /// </summary>
    public List<UserDelegate> DelegateList {
        get {
            if (DueForReload)
                loadItems();
            return lDelegates;
        }
    }

    public static UserDelegateInformation instance {
        get {
            if (HttpContext.Current.Application["DelegateList"] == null) {
                HttpContext.Current.Application.Lock();
                if (HttpContext.Current.Application["DelegateList"] == null)
                    HttpContext.Current.Application["DelegateList"] = new UserDelegateInformation();
                HttpContext.Current.Application.UnLock();
            }
            return (UserDelegateInformation)HttpContext.Current.Application["DelegateList"];
        }
    }

    /// <summary>
    /// Call this function to get a handle to the class. The class will auto-instantiate itself
    /// </summary>
    /// <returns></returns>
    public static UserDelegateInformation getInfo() {
        return instance;
    }

    /// <summary>
    /// To be called if the object is updated
    /// </summary>
    public void forceReload(bool WriteToCache = true) {
        dtLoadDate = DateTime.MinValue;
        lDelegates.Clear();
        loadItems();
    }

    private void loadItems() {
        string szSQL = String.Format(@"
          SELECT UD.*
            FROM USERDELEGATION UD
            WHERE RECORDSTATUS = 1 AND '{0}' BETWEEN STARTDATE AND ENDDATE
        ", Utility.formatDate(DateTime.Now));

        using (DataSet ds = DB.runDataSet(szSQL)) {
            lock (_lock) {
                if (!DueForReload)
                    return;

                lDelegates.Clear();

                foreach (DataRow dr in ds.Tables[0].Rows) {
                    int intID = Convert.ToInt32(dr["ID"]);
                    UserDelegate ud = new UserDelegate(intID);
                    ud.DelegationUserID = DB.readInt(dr["DELEGATIONUSERID"]);
                    ud.UserID = DB.readInt(dr["USERID"]);
                    ud.StartDate = DB.readDate(dr["STARTDATE"]);
                    ud.EndDate = DB.readDate(dr["ENDDATE"]);
                    ud.Type = (DelegationType)DB.readInt(dr["DELEGATIONTYPEID"]);
                    ud.SendEmailOrigUser = DB.readBool(dr["SENDTOORIGUSER"]);
                    lDelegates.Add(ud);
                }
                dtLoadDate = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Returns all the userIDs that this user is currently responsible for
    /// </summary>
    /// <param name="IncludeUser">Include the user ID in the list</param>
    /// <returns>A comma seperated list of IDs</returns>
    public string getIDsDelegatedToThisUser(int UserID, bool IncludeUser = true) {
        string szIDList = IncludeUser ? UserID.ToString() : "";
        foreach (UserDelegate d in G.UserDelegateInfo.DelegateList) {
            if (d.DelegationUserID == UserID) {
                Utility.Append(ref szIDList, d.UserID.ToString(), ",");
            }
        }
        return szIDList;
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
    public bool IsActive { get; set; }
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
            return Utility.fixGLOfficeCode(OfficeGLCode) + "-" + Initials;
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
    public string OfficeName { get; set; }
    public string OfficeJobCode { get; set; }

    public string Name { get { return LastName + ", " + FirstName + "(" + Initials + ")"; } }

    public string NameFLI { get { return FirstName + " " + LastName + "(" + Initials + ")"; } }
    public string NameIFLO { get { return Initials + " " + FirstName + " " + LastName + " (" + OfficeName.Substring(0, 2) + ")"; } }

    public UserDetail(int ID, bool IsActive, string Initials, string First, string Last, string Email, int RoleID, int OfficeID, string OfficeName, string OfficeGLCode, int MentorID, int Salary, int PayrollCycleID, int SupervisorID, string CreditGLCode, string DebitGLCode, string JobCode) {
        this.ID = ID;
        this.IsActive = IsActive;
        this.Initials = Initials;
        this.FirstName = First;
        this.LastName = Last;
        this.Email = Email;
        this.RoleID = RoleID;
        this.MentorID = MentorID;
        this.OfficeID = OfficeID;
        this.OfficeName = OfficeName;
        this.OfficeGLCode = OfficeGLCode;
        this.OfficeJobCode = JobCode;
        this.Salary = Salary;
        this.CreditGL = CreditGLCode;
        this.DebitGL = DebitGLCode;
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