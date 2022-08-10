using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

/// <summary>
/// Payroll general tools including access management
/// </summary>
public class Payroll {

    public static Boolean CanAccess {
        get {
            return G.User.IsAdmin /* Admin */
                || HasTimesheet /* Payroll Cycle set */
                || IsPayrollSupervisor /* Supervises staff on the payroll system */;
        }
    }

    public static Boolean HasTimesheet {
        get {
            UserDetail oCurrentUser = G.UserInfo.getUser(G.User.UserID);
            return oCurrentUser.PayrollCycleID > 0;
        }
    }

    public static Boolean IsPayrollSupervisor {
        get {
            if (HttpContext.Current.Session["ISPAYROLLSUPERVISOR"] == null) {
                // Check if User has any staff that are on the payroll system
                bool IsSuper = DB.getScalar(string.Format(@"
                    SELECT COUNT(*) FROM DB_USER S JOIN DB_USER U ON U.SUPERVISORID = S.ID
                    WHERE S.ID = {0} AND U.PAYROLLCYCLEID > 0 AND U.ISACTIVE = 1 AND U.ISDELETED = 0", G.User.UserID), 0) > 0;
                HttpContext.Current.Session["ISPAYROLLSUPERVISOR"] = IsSuper;
                return IsSuper;
            } else {
                return Convert.ToBoolean(HttpContext.Current.Session["ISPAYROLLSUPERVISOR"]);
            }
        }
    }
    public static Boolean IsLeaveSupervisor {
        get {
            if (HttpContext.Current.Session["ISLEAVESUPERVISOR"] == null) {
                // Check if User has any staff that are on the payroll system
                bool IsSuper = DB.getScalar(string.Format(@"
                    SELECT COUNT(*) FROM DB_USER 
                    WHERE SUPERVISORID IN ({0}) AND ISACTIVE = 1 AND ISDELETED = 0", G.User.UserIDListWithDelegates), 0) > 0;
                HttpContext.Current.Session["ISLEAVESUPERVISOR"] = IsSuper;
                return IsSuper;
            } else {
                return Convert.ToBoolean(HttpContext.Current.Session["ISLEAVESUPERVISOR"]);
            }
        }
    }

    /// <summary>
    /// Submits timesheet form.  Used for both user signoff and supervisor sign off
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="TimesheetCycleID"></param>
    /// <param name="SupervisorSignOff"></param>
    /// <param name="ForceSubmit">Allows Supervisors / Admin to update and resubmit forms</param>
    /// <returns></returns>
    public static int submitTimesheet(int UserID, int TimesheetCycleID, bool SupervisorSignOff = false, bool ForceSubmit = false) {
        // SQL for staff sign off (this is the default) - will only find record if staff has not already signed off
        string szRangeCheck = @"
                -- Must not have staff signoff
                AND TS.USERSIGNOFFDATE IS NULL";

        // Item to be changed - user sign off
        string szChange = string.Format(@"USERSIGNOFFDATE = '{0}'", Utility.formatDate(DateTime.Now));

        // SQL for supervisor sign off - will only sign off if staff has already signed off
        if (SupervisorSignOff) {
            szRangeCheck = @"
                -- Must have staff signoff but not supervisor signoff
                AND TS.USERSIGNOFFDATE IS NOT NULL";

            // Item to be changed - supervisor sign off
            szChange = string.Format(@"SIGNEDOFFDATE = '{0}'", Utility.formatDate(DateTime.Now));
        }

        // Admin can make changes and re-approve
        if (ForceSubmit)
            szRangeCheck = "";

        string szSQL = String.Format(@"
            SELECT COUNT(*) FROM TIMESHEETENTRY TS
                JOIN DB_USER U ON U.ID = TS.USERID
                JOIN TIMESHEETCYCLE TSC ON TSC.ID = TS.TIMESHEETCYCLEID
            WHERE
                -- Single timesheet Cycle
                TSC.ID = {0}
		        -- For given user
                AND U.ID = {1}
                {2}", TimesheetCycleID, UserID, szRangeCheck);
        DataSet ds = DB.runDataSet(string.Format(@"
            -- Get count of record to be updated (should always be 14 records)
            {0}

            -- Update records
            UPDATE TIMESHEETENTRY SET {2} WHERE ID IN ({1})", szSQL, szSQL.Replace("COUNT(*)", "TS.ID"), szChange));

        if (Convert.ToInt32(ds.Tables[0].Rows[0][0]) != 14)
            throw new Exception("There were not 14 days contained within the time period - may indicate attempt to signoff record a second time");

        return UserID;
    }

    [Serializable()]
    public class TimeSheetCycleReferenceList {
        public Dictionary<int, TimeSheetCycleReference> dValues;

        public void Add(int val, TimeSheetCycleReference val1) {
            dValues.Add(val, val1);
        }

        public TimeSheetCycleReference this[int i] {
            get {
                return dValues[i];
            }
        }

        public TimeSheetCycleReferenceList() {
            dValues = new Dictionary<int, TimeSheetCycleReference>();
        }

        /// <summary>
        /// To be called if the object is updated
        /// </summary>
        public void forceReload(bool WriteToCache = true) {
            HttpContext.Current.Application["TIMESHEETCR"] = null;
        }
    }

    //Class to store data for Timesheet pay cycles
    [Serializable()]
    public class TimeSheetCycleReference {
        public CycleReference NormalCycle { get; private set; }
        public CycleReference PayAdvanceCycle { get; private set; }

        public TimeSheetCycleReference(CycleReference NormalCycle, CycleReference PayAdvanceCycle) {
            this.NormalCycle = NormalCycle;
            this.PayAdvanceCycle = PayAdvanceCycle;
        }
    }

    [Serializable()]
    public class CycleReference {
        public int CycleID { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public CycleReference(int CycleID, DateTime StartDate, DateTime EndDate) {
            this.CycleID = CycleID;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
        }
    }

    /// <summary>
    /// TimeSheet cycles are listed relative to current cycle.  Function makes a list of (up to) 4 previous cycles and 4 current cycles.
    /// </summary>
    /// <returns></returns>
    public static TimeSheetCycleReferenceList getTimeSheetCycleReferences() {
        TimeSheetCycleReferenceList dInfo = new TimeSheetCycleReferenceList();
        string szSQL = string.Format(@"
                -- 0. Normal Timesheet cycle
                SELECT * FROM TIMESHEETCYCLE WHERE ID =
					(SELECT MIN(ID) AS ID FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 1 AND COMPLETED = 0)

                -- 1. Pay in Advance Timesheet cycle
                SELECT * FROM TIMESHEETCYCLE WHERE ID =
					(SELECT MIN(ID) AS ID FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 2 AND COMPLETED = 0)");
        DateTime dtStartDateNormal = DateTime.MinValue, dtStartDatePayAdvance = DateTime.MinValue;

        // Get current cycle - all other cycles are calculated in reference to these
        using (DataSet ds1 = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds1.Tables[0].Rows)
                dtStartDateNormal = DB.readDate(dr["STARTDATE"]);

            foreach (DataRow dr in ds1.Tables[1].Rows)
                dtStartDatePayAdvance = DB.readDate(dr["STARTDATE"]);
        }

        // Get other cycles relative to current cycle
        for (int c = -4; c < 4; c++) {
            szSQL = string.Format(@"
                -- 0. Normal Timesheet cycle
                SELECT * FROM TIMESHEETCYCLE WHERE STARTDATE = '{0}' and CYCLETYPEID = 1

                -- 1. Pay in Advance Timesheet cycle
                SELECT * FROM TIMESHEETCYCLE WHERE STARTDATE = '{1}' and CYCLETYPEID = 2

                -- Cycle : {2} ",
                        Utility.formatDate(dtStartDateNormal.AddDays(c * 14)),
                        Utility.formatDate(dtStartDatePayAdvance.AddDays(c * 14)),
                        c);

            using (DataSet ds1 = DB.runDataSet(szSQL)) {
                CycleReference normal = null;
                CycleReference payAdvanced = null;
                foreach (DataRow dr in ds1.Tables[0].Rows)
                    normal = new CycleReference(DB.readInt(dr["ID"]), DB.readDate(dr["STARTDATE"]), DB.readDate(dr["ENDDATE"]));

                foreach (DataRow dr in ds1.Tables[1].Rows)
                    payAdvanced = new CycleReference(DB.readInt(dr["ID"]), DB.readDate(dr["STARTDATE"]), DB.readDate(dr["ENDDATE"]));

                if (normal != null && payAdvanced != null)
                    dInfo.Add(c, new TimeSheetCycleReference(normal, payAdvanced));
            }
        }

        return dInfo;
    }
}

/// <summary>
/// Represents a single user timesheet
/// </summary>
public class UserTimesheet {
    public int ID { get; private set; }
    private int TimesheetCycleID { get; set; }
    private int UserID { get; set; }
    public DataTable dtModifications = null;
    public List<UserTimesheetEntry> lEntries = new List<UserTimesheetEntry>();

    // Indicates if this record is editable
    public bool CanEdit { get; private set; }

    public DateTime UserSignOff {
        get { return lEntries[0].UserSignOff; }
    }

    public DateTime SupervisorSignOff {
        get { return lEntries[0].SupervisorSignOff; }
    }

    public UserTimesheet(int UserID, int TimesheetCycleID, Boolean blReadOnlyForm = false) {
        this.TimesheetCycleID = TimesheetCycleID;
        this.UserID = UserID;
        loadTimesheet(blReadOnlyForm);
    }

    private void loadTimesheet(Boolean blReadOnlyForm) {
        using (DataSet ds = DB.runDataSet(String.Format(@"
                SELECT TSE.* FROM TIMESHEETENTRY TSE
                WHERE TIMESHEETCYCLEID = {0} AND USERID = {1}

                SELECT COMPLETED FROM TIMESHEETCYCLE WHERE ID = {0};

                SELECT L.* , U.FIRSTNAME +  ' ' + U.LASTNAME AS LOGNAME
                FROM TIMESHEETENTRY TSE JOIN LOGV2 L on L.OBJECTID = TSE.ID AND L.TYPEID = 10
                JOIN DB_USER U ON L.USERID = U.ID
                WHERE TIMESHEETCYCLEID = {0} AND TSE.USERID = {1}

                ", TimesheetCycleID, UserID))) {
            if (ds.Tables[0].Rows.Count == 0) {
                createRowsForUser();
                loadTimesheet(blReadOnlyForm);
            }
            foreach (DataRow dr in ds.Tables[1].Rows) {
                CanEdit = !DB.readBool(dr["COMPLETED"]);
            }

            foreach (DataRow dr in ds.Tables[0].Rows) {
                if (CanEdit && DB.readDate(dr["USERSIGNOFFDATE"]) > DateTime.MinValue && G.User.ID == DB.readInt(dr["USERID"]))
                    CanEdit = false;

                lEntries.Add(new UserTimesheetEntry(dr, blReadOnlyForm || !CanEdit));
            }
            dtModifications = ds.Tables[2];
        }
    }

    /// <summary>
    /// Creates the rows for the user to fill in
    /// </summary>
    private void createRowsForUser() {
        TimesheetCycle oTC = new TimesheetCycle(TimesheetCycleID);
        DateTime dtCurr = oTC.StartDate;
        SQLInsertQueue oQ = new SQLInsertQueue("INSERT INTO TIMESHEETENTRY(USERID, TIMESHEETCYCLEID, ENTRYDATE) VALUES ");

        while (dtCurr <= oTC.EndDate) {
            oQ.addSQL(String.Format(@"({0}, {1}, '{2}')", UserID, TimesheetCycleID, Utility.formatDate(dtCurr)));
            dtCurr = dtCurr.AddDays(1);
        }
        oQ.flush();
        oQ = null;
    }

    public void updateDB() {
        foreach (UserTimesheetEntry oUTSE in lEntries)
            oUTSE.updateDB();
    }
}

/// <summary>
/// Represents a single timesheet entry
/// </summary>
public class UserTimesheetEntry {
    public int ID { get; private set; }
    public int UserID { get; private set; }
    private int TimesheetCycleID { get; set; }
    public DateTime EntryDate { get; set; }
    public double Actual { get; set; }
    public double AnnualLeave { get; set; }
    public double SickLeave { get; set; }
    public double RDOAcrued { get; set; }
    public double RDOTaken { get; set; }
    public string Comments { get; set; }
    public DateTime UserSignOff { get; set; }
    public DateTime SupervisorSignOff { get; set; }
    public bool blReadOnlyForm = false;

    public string RenderedEntryDate {
        get {
            if (EntryDate == DateTime.MinValue)
                return "<b>Totals</b>";
            return EntryDate.ToString("ddd dd-MMM-yy");
        }
    }

    public string ActualEntryField {
        get {
            return getInput(Actual, "Actual");
        }
    }

    public string AnnualLeaveEntryField {
        get {
            return getInput(AnnualLeave, "AnnualLeave");
        }
    }

    public string SickLeaveEntryField {
        get {
            return getInput(SickLeave, "SickLeave");
        }
    }

    public string RDOAcruedEntryField {
        get {
            return getInput(RDOAcrued, "RDOAcrued");
        }
    }

    public string RDOTakenEntryField {
        get {
            return getInput(RDOTaken, "RDOTaken");
        }
    }

    public string CommentsEntryField {
        get {
            // -1 indicates a blank row for totals at the bottom of the form
            if (ID == -1)
                return "<div id='txtComments_total' name='txtComments_total' class='form-control input-sm' style='width: 100%'/>";

            return String.Format(@"<input type='text' id='txtComments_{0}' name='txtComments_{0}' class='form-control input-sm ' style='width: 100%' value='{1}' {2} />",
                ID, Comments, !CanEdit ? "disabled='true'" : "");
        }
    }

    public UserTimesheetEntry() {
        ID = -1;
        UserID = G.User.ID;
        EntryDate = DateTime.MinValue;
        Actual = double.MinValue;
        AnnualLeave = double.MinValue;
        SickLeave = double.MinValue;
        RDOAcrued = double.MinValue;
        RDOTaken = double.MinValue;
        Comments = "";

        this.blReadOnlyForm = true;
    }

    public UserTimesheetEntry(DataRow dr, Boolean blReadOnlyForm = false) {
        ID = DB.readInt(dr["ID"]);
        UserID = DB.readInt(dr["USERID"]);
        EntryDate = DB.readDate(dr["ENTRYDATE"]);
        Actual = DB.readDouble(dr["ACTUAL"]);
        AnnualLeave = DB.readDouble(dr["ANNUALLEAVE"]);
        SickLeave = DB.readDouble(dr["SICKLEAVE"]);
        RDOAcrued = DB.readDouble(dr["RDOACRUED"]);
        RDOTaken = DB.readDouble(dr["RDOTAKEN"]);
        Comments = DB.readString(dr["COMMENTS"]);
        UserSignOff = DB.readDate(dr["USERSIGNOFFDATE"]);
        SupervisorSignOff = DB.readDate(dr["SIGNEDOFFDATE"]);

        this.blReadOnlyForm = blReadOnlyForm;
    }

    private bool CanEdit {
        get {
            if (G.User.RoleID == 1 && G.User.ID != UserID) // Admit can always edit other staff forms
                return true;

            if (G.User.ID == UserID)
                return UserSignOff <= DateTime.MinValue && !blReadOnlyForm;
            return true;
        }
    }

    private string getInput(double value, string FieldName) {
        string szValue = "";
        if (value != Double.MinValue && value != Double.MaxValue)
            szValue = Convert.ToString(value);

        string szID = Convert.ToString(ID);
        if (ID == -1)
            szID = "total";

        return String.Format(@"<input type='text' id='txt{0}_{1}' name='txt{0}_{1}' class='form-control input-sm timesheet-field'  style='width: 100px' value='{2}' {3} />",
                                        FieldName, szID, szValue, !CanEdit ? "disabled='true'" : "");
    }

    public void updateDB() {
        sqlUpdate oSQL = new sqlUpdate("TIMESHEETENTRY", "ID", ID);

        oSQL.add("ENTRYDATE", Utility.formatDate(EntryDate));
        oSQL.add("ACTUAL", Actual);
        oSQL.add("ANNUALLEAVE", AnnualLeave);
        oSQL.add("SICKLEAVE", SickLeave);
        oSQL.add("RDOACRUED", RDOAcrued);
        oSQL.add("RDOTAKEN", RDOTaken);
        oSQL.add("COMMENTS", Comments);
        if (UserSignOff > DateTime.MinValue)
            oSQL.add("USERSIGNOFFDATE", Utility.formatDateTime(UserSignOff));
        else
            oSQL.addNull("USERSIGNOFFDATE");
        if (SupervisorSignOff > DateTime.MinValue)
            oSQL.add("SIGNEDOFFDATE", Utility.formatDateTime(SupervisorSignOff));
        else
            oSQL.addNull("SIGNEDOFFDATE");

        DB.runDataSet(oSQL.createUpdateSQL());
    }
}

/// <summary>
/// COntains all functionality to do with a timesheet cycle
/// </summary>
public class TimesheetCycle {
    public int ID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    private TimesheetType Type { get; set; }

    public string Name {
        get { return string.Format("{0} {1} - {2}", Type.ToString(), Utility.formatDate(StartDate), Utility.formatDate(EndDate)); }
    }

    /// <summary>
    /// A timesheet cycle that will come from the DB
    /// </summary>
    /// <param name="ID"></param>
    public TimesheetCycle(int ID) {
        this.ID = ID;
        loadTimeSheetCycle();
    }

    /// <summary>
    /// Creates a new timesheet cycle for the given Type and dates
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="StartDate"></param>
    /// <param name="EndDate"></param>
    /// <returns></returns>
    public static TimesheetCycle createTimeSheetCycle(TimesheetType Type, DateTime StartDate, DateTime EndDate) {
        sqlUpdate oSQL = new sqlUpdate("TIMESHEETCYCLE", "ID", -1);
        oSQL.add("CYCLETYPEID", (int)Type);
        oSQL.add("STARTDATE", Utility.formatDate(StartDate));
        oSQL.add("ENDDATE", Utility.formatDate(EndDate));
        int ID = DB.getScalar(oSQL.createInsertSQL(), -1);
        return new TimesheetCycle(ID);
    }

    private void loadTimeSheetCycle() {
        using (DataSet ds = DB.runDataSet(String.Format(@"SELECT * FROM TIMESHEETCYCLE WHERE ID = {0}", ID))) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                StartDate = DB.readDate(dr["STARTDATE"]);
                EndDate = DB.readDate(dr["ENDDATE"]);
                Type = (TimesheetType)DB.readInt(dr["CYCLETYPEID"]);
            }
        }
    }

    /// <summary>
    /// Makes sure that we have time cycles being created into the future
    /// </summary>
    public static void checkTimesheetCycles() {
        string szSQL = string.Format(@"
                -- Normal pay cycle
                SELECT MAX(STARTDATE) AS STARTDATE, MAX(ENDDATE) AS ENDDATE, COUNT(*) AS CURRENT_CYCLES FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 1 AND COMPLETED = 0

                -- Paid in advance pay cycle
                SELECT MAX(STARTDATE) AS STARTDATE, MAX(ENDDATE) AS ENDDATE, COUNT(*) AS CURRENT_CYCLES FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 2 AND COMPLETED = 0");

        using (DataSet ds = DB.runDataSet(szSQL)) {
            // Check if there are four current 'Normal Pay' cycles.  If not create new one(s).
            foreach (DataRow dr in ds.Tables[0].Rows) {
                int intCount = DB.readInt(dr["CURRENT_CYCLES"]);
                if (intCount < 4) {
                    DateTime dNormalStart = DB.readDate(dr["STARTDATE"]);
                    DateTime dNormalEnd = DB.readDate(dr["ENDDATE"]);
                    for (int c = intCount; c < 4; c++) {
                        dNormalStart = dNormalStart.AddDays(14);
                        dNormalEnd = dNormalEnd.AddDays(14);
                        DB.runNonQuery(string.Format(@"
                            INSERT INTO TIMESHEETCYCLE (CYCLETYPEID, STARTDATE, ENDDATE) VALUES (1, '{0}', '{1}');", Utility.formatDate(dNormalStart), Utility.formatDate(dNormalEnd)));
                    }
                }
            }

            // Check if there are four current 'Pay in advance' cycles.  If not create new one(s).
            foreach (DataRow dr in ds.Tables[1].Rows) {
                int intCount = DB.readInt(dr["CURRENT_CYCLES"]);
                if (intCount < 4) {
                    DateTime dAdvancedStart = DB.readDate(dr["STARTDATE"]);
                    DateTime dAdvancedEnd = DB.readDate(dr["ENDDATE"]);
                    for (int c = intCount; c < 4; c++) {
                        dAdvancedStart = dAdvancedStart.AddDays(14);
                        dAdvancedEnd = dAdvancedEnd.AddDays(14);
                        DB.runNonQuery(string.Format(@"
                            INSERT INTO TIMESHEETCYCLE (CYCLETYPEID, STARTDATE, ENDDATE) VALUES (2, '{0}', '{1}');", Utility.formatDate(dAdvancedStart), Utility.formatDate(dAdvancedEnd)));
                    }
                }
            }
        }
    }

    private string outstandingTimesheetSQL(int SupervisorID = -1, bool TestMode = false) {
        string szSupervisor = "";
        if (SupervisorID == 0)
            return ""; //Default user isn't a supervisor so no email needs to be sent
        if (SupervisorID > -1) {
            szSupervisor = string.Format("U.SUPERVISORID = {0} AND ", SupervisorID);
        }
        string szNullDates = " TE.USERSIGNOFFDATE IS NULL AND ";
        if (TestMode)
            szNullDates = "";

        return string.Format(@"
                -- Check for outstanding staff signoff
                SELECT U.ID AS OUTSTANDING, U.EMAIL AS USER_EMAIL, ISNULL(SUP.EMAIL, '') AS SUPERVISOR_EMAIL, SUM(ISNULL(TE.ACTUAL, 0)) AS TOTALHOURS, 
                    CASE WHEN MIN(TE.USERSIGNOFFDATE) IS NULL THEN 'EMAIL' ELSE '' END AS EMAILFLAG
                FROM DB_USER U
                LEFT JOIN DB_USER SUP ON SUP.ID = U.SUPERVISORID 
                LEFT JOIN TIMESHEETENTRY TE ON TE.USERID = U.ID AND TE.TIMESHEETCYCLEID = {0}
                WHERE {1} {2}
	                 U.PAYROLLCYCLEID = (SELECT CYCLETYPEID FROM TIMESHEETCYCLE WHERE ID = {0}) AND U.ISACTIVE = 1  AND U.ISDELETED = 0
                GROUP BY U.ID, U.EMAIL, SUP.EMAIL

                -- Check for outstanding supervisor signoff
                SELECT U.ID AS OUTSTANDING, U.EMAIL AS USER_EMAIL, ISNULL(SUP.EMAIL, '') AS SUPERVISOR_EMAIL
                FROM DB_USER U
                LEFT JOIN DB_USER SUP ON SUP.ID = U.SUPERVISORID
                LEFT JOIN TIMESHEETENTRY TE ON TE.USERID = U.ID AND TE.TIMESHEETCYCLEID = {0}
                WHERE {1} TE.USERSIGNOFFDATE IS NOT NULL
	                AND U.SUPERVISORID IS NOT NULL AND TE.SIGNEDOFFDATE IS NULL
	                AND U.PAYROLLCYCLEID = (SELECT CYCLETYPEID FROM TIMESHEETCYCLE WHERE ID = {0}) AND U.ISACTIVE = 1  AND U.ISDELETED = 0
                GROUP BY U.ID, U.EMAIL, SUP.EMAIL", this.ID, szSupervisor, szNullDates);
    }

    /// <summary>
    /// Checks if all users in supervisor group have signed off on TimeSheetCycle
    /// If all have, sends email to inform supervisor
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="TimeSheetCycleID"></param>
    public void checkSupervisorGroupSignoff(int SupervisorID) {
        if (SupervisorID == 0)
            return; //No email required on default user
        using (DataSet ds = DB.runDataSet(outstandingTimesheetSQL(SupervisorID))) {
            if (ds.Tables[0].Rows.Count == 0) {
                int intSupervisorSignoffPending = ds.Tables[1].Rows.Count;
                UserDetail oSuper = G.UserInfo.getUser(SupervisorID);
                string szMsg = string.Format(@"
                    <p>Hi Team Leaders</p>
                    <p>Please log on and review your teams timesheets.</p>
                    <p>These need to be approved by Wednesday 2pm at the latest.</p>
                    <p>Thanks</p>");
                Email.sendMail(oSuper.Email, EmailSettings.SMTPServerUserName, "All staff timesheets submitted", szMsg);
            }
        }
    }

    /// <summary>
    /// Emails staff who have not yet submitted
    /// </summary>
    public void emailTimesheetStaff(bool TestOnly = false) {

        using (DataSet ds = DB.runDataSet(outstandingTimesheetSQL(TestMode: true))) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                string szEmail = DB.readString(dr["USER_EMAIL"]);

                if (string.IsNullOrWhiteSpace(szEmail))
                    continue;
                string szMsg = string.Format(@"
                    <p>Good morning</p>
                    <p>Your fortnightly timesheet is ready, please click <a href='https://{0}/'>here</a> and submit your hours.  Timesheets must be completed by Wednesday 12pm.</p>
                    <p>Please remember if you are going on annual leave to prefill out your timesheet so you don’t miss out on your pay.</p>
                    <p>Thanks</p>", G.BaseURL);
                if (!TestOnly) {
                    Email.sendMail(szEmail, EmailSettings.SMTPServerUserName, "Fortnightly timesheet", szMsg, "", LogObjectID: DB.readInt(dr["OUTSTANDING"]));
                } else {
                    if (DB.readString(dr["EMAILFLAG"]) == "EMAIL") {
                        HttpContext.Current.Response.Write(String.Format("Send an email to {0}<br/>", szEmail));
                    } else {
                        HttpContext.Current.Response.Write(String.Format("{0} entered. Total hours: {1}<br/>", szEmail, DB.readDouble(dr["TOTALHOURS"])));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Emails staff who have not yet submitted
    /// </summary>
    public void emailOutstandingStaffFollowUp() {
        using (DataSet ds = DB.runDataSet(outstandingTimesheetSQL())) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                string szEmail = DB.readString(dr["USER_EMAIL"]);
                if (string.IsNullOrWhiteSpace(szEmail))
                    continue;
                string szMsg = string.Format(@"
                    <p>Good morning</p>
                    <p>This is your final reminder to fill out your timesheet, please click <a href='https://{0}/'>here</a>.</p>
                    <p>Your timesheet needs to be completed by midday today.</p>
                    <p>Remember - policy is, NO timesheet NO Pay.</p>
                    <p>Thanks</p>", G.BaseURL);

                Email.sendMail(szEmail, EmailSettings.SMTPServerUserName, "Timesheet outstanding", szMsg, "", LogObjectID: DB.readInt(dr["OUTSTANDING"]));
            }
        }
    }

    /// <summary>
    /// Tests the auto emails by looking 28 days into the future to see whether we would be sending any emails...
    /// </summary>
    public static void testAutomatedEmails() {
        AppConfigAdmin oConfigAdmin = new AppConfigAdmin(G.szCnn);

        DateTime oDTLastSent = DateTime.Now.AddDays(-1);

        // checks for automated emails starting 1 day after last sent
        // Email 1. Please fill in your
        for (DateTime dtC = oDTLastSent.AddDays(1); dtC <= DateTime.Now.AddDays(28); dtC = dtC.AddDays(1)) {
            HttpContext.Current.Response.Write(Utility.formatDate(dtC) + "<br/>");
            string szSQL = string.Format(@"
                SELECT ID FROM TIMESHEETCYCLE
	                WHERE
		                -- Email all pay cycle staff on the last day of their pay cycle
		                ENDDATE = dateadd(day, 0, '{0}')
		               ", Utility.formatDate(dtC));
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    TimesheetCycle oTC = new TimesheetCycle(DB.readInt(dr["ID"]));
                    oTC.emailTimesheetStaff(true);
                }
            }
        }
    }

    /// <summary>
    /// Check for Timesheet Cycles that trigger emails to outstanding staff today
    /// </summary>
    public static void checkAutomatedEmails() {
        AppConfigAdmin oConfigAdmin = new AppConfigAdmin(G.szCnn);
        // Note: Default value is yesterday (if no value is held in config).
        oConfigAdmin.addConfig("AUTOEMAILSLASTSENT", Utility.formatDate(DateTime.Now.AddDays(-1)));
        oConfigAdmin.loadValuesFromDB();
        DateTime oDTLastSent = Convert.ToDateTime(oConfigAdmin.getValue("AUTOEMAILSLASTSENT"));

        // checks for automated emails starting 1 day after last sent
        // Email 1. Please fill in your
        for (DateTime dtC = oDTLastSent.AddDays(1); dtC <= DateTime.Now; dtC = dtC.AddDays(1)) {
            string szSQL = string.Format(@"
                SELECT ID FROM TIMESHEETCYCLE
	                WHERE
		                -- Email all pay cycle staff on the last day of their pay cycle
		                ENDDATE = dateadd(day, 0, '{0}')
		               ", Utility.formatDate(dtC));
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    TimesheetCycle oTC = new TimesheetCycle(DB.readInt(dr["ID"]));
                    oTC.emailTimesheetStaff();
                }
            }
        }

        // Email 2. Follow up
        for (DateTime dtC = oDTLastSent.AddDays(1); dtC <= DateTime.Now; dtC = dtC.AddDays(1)) {
            string szSQL = string.Format(@"
                SELECT ID FROM TIMESHEETCYCLE
	                WHERE
		                -- Email all pay cycle staff one day after end of pay sheet cycle
		                ENDDATE = dateadd(day, -1, '{0}')
		               ", Utility.formatDate(dtC));
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    TimesheetCycle oTC = new TimesheetCycle(DB.readInt(dr["ID"]));
                    oTC.emailOutstandingStaffFollowUp();
                }
            }
        }

        oConfigAdmin.updateConfigValueSQL("AUTOEMAILSLASTSENT", Utility.formatDate(DateTime.Now));
    }
}