using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

/// <summary>
/// Manages all auditing for the system
/// </summary>
public class ChangeTracker {
    private StringBuilder sbLog = new StringBuilder();
    protected int intObjectID = -1;
    protected int intChildObjectID = -1;
    protected DBLogType oType = DBLogType.SaleData;

    public int ChildObjectID {
        set { intChildObjectID = value; }
    }

    public ChangeTracker() {
    }

    /// <summary>
    /// Creates an instance of a change tracker with for the specified Log type and the object ID that is being added
    /// </summary>
    public ChangeTracker(DBLogType LogType, int ObjectID, int ChildObjectID = -1) {
        intObjectID = ObjectID;
        intChildObjectID = ChildObjectID;
        oType = LogType;
    }

    /// <summary>
    /// Returns whether any changes have been recorded. If change logging is fully implemented, this can be used to determine whether or not
    /// and update statement on the object needs to be run.
    /// </summary>
    public bool AreChangesRecorded {
        get {
            return !String.IsNullOrEmpty(sbLog.ToString());
        }
    }

    /// <summary>
    /// Adds a specific codified change of a property
    /// </summary>
    /// <param name="PropertyName"></param>
    /// <param name="OrigValue"></param>
    /// <param name="NewValue"></param>
    public void addChange(string PropertyName, string OrigValue, string NewValue) {
        sbLog.AppendFormat(@"{0} was changed from <i>{1}</i> to <i>{2}</i><br/>", PropertyName, OrigValue, NewValue);
    }

    /// <summary>
    /// Adds a specific codified change of a property
    /// </summary>
    /// <param name="PropertyName"></param>
    /// <param name="OrigValue"></param>
    /// <param name="NewValue"></param>
    public void addChange(string PropertyName, DateTime OrigValue, DateTime NewValue) {
        addChange(PropertyName, getDateValue(OrigValue), getDateValue(NewValue));
    }

    /// <summary>
    /// Adds a generic change log line
    /// </summary>
    /// <param name="Message"></param>
    public void addChange(string ChangeLogText) {
        sbLog.AppendFormat(@"{0}<br/>", ChangeLogText);
    }

    /// <summary>
    /// Adds a generic change log line at the beginning of the log text
    /// </summary>
    /// <param name="Message"></param>
    public void addChangeAtStart(string ChangeLogText) {
        sbLog.Insert(0, "<br/>").Insert(0, ChangeLogText);
    }

    /// <summary>
    /// Writes the change log to the DB
    /// </summary>
    public void writeToDB() {
        DBLog.addGenericRecord(oType, sbLog.ToString(), intObjectID, intChildObjectID);
        sbLog.Clear();
    }

    /// <summary>
    /// Returns the datetime nicely formatted, accounting for blanked out values.
    /// </summary>
    /// <param name="value"></param>
    public string getDateValue(DateTime value) {
        if (value == DateTime.MinValue)
            return "";
        else
            return value.ToString("MMM dd, yyyy hh:mm");
    }

    /// <summary>
    /// Retrieves the log entries for a specific object type and id
    /// </summary>
    /// <param name="oType"></param>
    /// <param name="ObjectID"></param>
    /// <returns></returns>
    public static string showHistoryLog(DBLogType oType, int ObjectID) {
        StringBuilder sbOutput = new StringBuilder();
        string szSQL = String.Format(@"
                    SELECT L.*, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
                    FROM LOG L JOIN MGT_USER U ON L.USERID = U.ID
                    WHERE TYPEID = {0} AND OBJECTID = {1} ORDER BY ID ", (int)oType, ObjectID);
        DataSet ds = DB.runDataSet(szSQL);

        foreach (DataRow dr in ds.Tables[0].Rows) {
            string szValue = dr["VALUE"].ToString();
            string szDate = Utility.formatDate(Convert.ToDateTime(dr["CHANGEDATE"])) + " " + Convert.ToDateTime(dr["CHANGEDATE"]).ToShortTimeString();
            string szChild = dr["CHILDOBJECTID"].ToString();
            sbOutput.Append("<b>");
            sbOutput.Append(szDate);
            sbOutput.Append(" - ");
            sbOutput.Append(dr["NAME"]);
            sbOutput.Append("</b><br/>");
            if (!string.IsNullOrWhiteSpace(szChild)) {
                sbOutput.Append(" Child ID ");
                sbOutput.Append(szChild);
                sbOutput.Append(": ");
            }
            sbOutput.Append(dr["VALUE"]);
            sbOutput.Append("<br/>");
        }
        if (String.IsNullOrWhiteSpace(sbOutput.ToString()))
            return "We don't have any history on this item, sorry.";
        return sbOutput.ToString();
    }
}

public class SalesExpenseChangeTracker : ChangeTracker {

    /// <summary>
    /// Creates an instance of a partner change tracker
    /// </summary>
    public SalesExpenseChangeTracker(int SaleID, int SalesExpenseID) {
        intObjectID = SaleID;
        intChildObjectID = SalesExpenseID;
        oType = DBLogType.SaleExpense;
    }
}

public class UserSalesSplitChangeTracker : ChangeTracker {

    /// <summary>
    /// Creates an instance of a partner change tracker
    /// </summary>
    public UserSalesSplitChangeTracker(int SaleID, int UserSaleSplitID) {
        intObjectID = SaleID;
        intChildObjectID = UserSaleSplitID;
        oType = DBLogType.UserSalesSplit;
    }
}

public class SalesSplitChangeTracker : ChangeTracker {

    /// <summary>
    /// Creates an instance of a partner change tracker
    /// </summary>
    public SalesSplitChangeTracker(int SaleSplitID, int SalesSplitID) {
        intObjectID = SaleSplitID;
        intChildObjectID = SalesSplitID;
        oType = DBLogType.SalesSplit;
    }
}

/*
public sealed class ProjectPaymentHistory : LogHistory {
    int intProjectID;
    Dictionary<int, string> dBankAccount = new Dictionary<int, string>();
    Dictionary<int, string> dCurrencyEquiv = new Dictionary<int, string>();
    Dictionary<int, string> dCurrency = new Dictionary<int, string>();
    Dictionary<int, string> dList = new Dictionary<int, string>();

    List<DBLogType> lstDeliveryDetail = new List<DBLogType>(){  DBLogType.FundingCycleApproved,
                                                                    DBLogType.FundingCycleRejected
                                                                };
    public ProjectPaymentHistory(int ProjectPaymentID = -1, int ProjectID = -1) {
        oType = DBLogType.Payment;
        intObjectID = ProjectPaymentID;
        intProjectID = ProjectID;

        getLogData();
    }

    public override void getLogData() {
        string szSQL = String.Format(@"
            --Log details
            SELECT L.*
            FROM LOG L JOIN PROJECT_PAYMENT PP ON L.OBJECTID = PP.ID AND L.TYPEID = {0} AND L.OBJECTID = {1};

            -- 1 Bank Account
            Select id, bank + ' - ' + account_name as name from partner_bank_account;

            -- 2 Currency equiv
            Select id, code + ' equiv of' as name from currency;

            -- 3 Currency
            Select id, code as name from currency;

            -- 4 Receipt method
            Select id, name from list;

        ", (int)oType, intObjectID);

        ds = DB.runDataSet(szSQL);
        dBankAccount = fillDictionary(ds.Tables[1]);
        dCurrencyEquiv = fillDictionary(ds.Tables[2]);
        dCurrency = fillDictionary(ds.Tables[3]);
        dList = fillDictionary(ds.Tables[4]);
    }

    private string TransitKey(DataRow dr) {
        return Convert.ToDateTime(dr["CHANGEDATE"]).ToString("yyyy/MM/dd hh:mm") + dr["NAME"].ToString();
    }

    public override string replaceProperties(string szValue) {
        if (szValue.Contains("Account"))
            szValue = replaceIDValues("Account", szValue, dBankAccount);

        //NOTE : Value 'Currency' conflicts with other values in list
        //    Two if statements search for Currency after a <br/> or Currency at the start
        //    of string to avoid conflicts
        if (szValue.Contains(">Currency"))
            szValue = replaceIDValues(">Currency", szValue, dCurrency);
        if (szValue.StartsWith("Currency"))
            szValue = replaceIDValues("Currency", szValue, dCurrency);

        // Below values contain the word 'Currency' - see note above re conflict
        if (szValue.Contains("Equivalent Currency"))
            szValue = replaceIDValues("Equivalent Currency", szValue, dCurrencyEquiv);
        if (szValue.Contains("Actual Currency"))
            szValue = replaceIDValues("Actual Currency", szValue, dCurrency);
        if (szValue.Contains("Received Currency"))
            szValue = replaceIDValues("Received Currency", szValue, dCurrency);

        if (szValue.Contains("Receipt type"))
            szValue = replaceIDValues("Receipt type", szValue, dList);

        return szValue;
    }
}
*/

public abstract class LogHistory {
    protected int intObjectID = -1;
    protected DBLogType oType = DBLogType.SaleData;
    protected DataSet ds = null;
    protected DataView dv = null;
    private string szInitialID = "-10";
    private string szChangedID = "-10";
    private int intEndIndex = 0;
    private int intStartIndex = 0;

    public LogHistory() {
    }

    public bool hasData {
        get {
            if (ds == null)
                return false;
            else
                return ds.Tables[0].Rows.Count > 0;
        }
    }

    public abstract void getLogData();

    /// <summary>
    /// Fill a hashtable with list data to swap log integers for
    /// </summary>
    /// <param name="ht"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    protected Hashtable fillHashtable(DataTable dt) {
        Hashtable ht = new Hashtable();
        foreach (DataRow dr in dt.Rows) {
            string szID = dr["ID"].ToString();
            if (!ht.Contains(szID))
                ht.Add(szID, dr["NAME"].ToString());
        }
        return ht;
    }

    /// <summary>
    /// Fill a dictionary with list data to swap log integers for
    /// </summary>
    /// <param name="ht"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    protected Dictionary<int, string> fillDictionary(DataTable dt) {
        Dictionary<int, string> dic = new Dictionary<int, string>();
        foreach (DataRow dr in dt.Rows) {
            int intID = Convert.ToInt32(dr["ID"]);
            if (!dic.ContainsKey(intID))
                dic.Add(intID, dr["NAME"].ToString());
        }
        return dic;
    }

    /// <summary>
    /// Replaces the ids with corresponding database values for this item
    /// </summary>
    /// <param name="szPropertyName"></param>
    /// <param name="szValue"></param>
    /// <param name="dicIDNames"></param>
    /// <returns></returns>
    protected string replaceIDValues(string szPropertyName, string szValue, Dictionary<int, string> dicIDNames, bool blnRecursiveReplace = false) {
        if (blnRecursiveReplace) {
            intEndIndex = 0;
            while (intEndIndex < szValue.Length) {
                string szReplaceItem = getReplaceItem(szPropertyName, szValue, blnRecursiveReplace);
                if (intStartIndex == -1)
                    break;
                string szReplacementItem = szReplaceItem.Replace(szInitialID, getIDValue(szInitialID, dicIDNames));
                szValue = szValue.Replace(szReplaceItem, szReplacementItem);
            }
        } else {
            string szReplaceItem = getReplaceItem(szPropertyName, szValue);
            if (intStartIndex == -1)
                return szValue;
            string szReplacementItem = szReplaceItem.Replace("<i>" + szInitialID + "</i>", "<i>" + getIDValue(szInitialID, dicIDNames) + "</i>").Replace("<i>" + szChangedID + "</i>", "<i>" + getIDValue(szChangedID, dicIDNames) + "</i>");
            szValue = szValue.Replace(szReplaceItem, szReplacementItem);
        }
        return szValue;
    }

    /// <summary>
    /// Get the name value for this specific ID
    /// </summary>
    /// <param name="szID"></param>
    /// <param name="dicIDNames"></param>
    /// <returns></returns>
    private string getIDValue(string szID, Dictionary<int, string> dicIDNames) {
        int intID = -1;
        if (Int32.TryParse(szID, out intID) && dicIDNames.ContainsKey(intID))
            return dicIDNames[intID].ToString();
        return "___";
    }

    /// <summary>
    /// Formats and displays the History of the object.
    /// </summary>
    /// <param name="blnShowItemInfo">Show a column with further information regarding the item that has been changed eg. Sampletest details</param>
    /// <returns></returns>
    public string showHistory(bool blnShowItemInfo = false) {
        StringBuilder sbOutput = new StringBuilder();
        sbOutput.Append(getTableHeader(blnShowItemInfo));
        dv = ds.Tables[0].DefaultView;
        foreach (DataRowView dr in dv) {
            string szValue = dr["VALUE"].ToString();

            string szDate = Utility.formatDate(Convert.ToDateTime(dr["CHANGEDATE"])) + " " + Convert.ToDateTime(dr["CHANGEDATE"]).ToShortTimeString();
            string szChild = dr["CHILDOBJECTID"].ToString();
            //update 'true to false' = unticked, 'false to true' = ticked
            szValue = szValue.Replace("changed from <i>True</i> to <i>False</i>", "<i>unticked</i>");
            szValue = szValue.Replace("changed from <i>False</i> to <i>True</i>", "<i>ticked</i>");
            szValue = szValue.Replace("<i>False</i>", "<i>unticked</i>");
            szValue = szValue.Replace("<i>True</i>", "<i>ticked</i>");
            szValue = szValue.Replace("<i></i>", "<i>__</i>");
            szValue = szValue.Replace("<i>" + int.MinValue.ToString() + "</i>", "<i>__</i>");
            szValue = replaceProperties(szValue);
        }
        sbOutput.Append("</tbody></table>");
        if (sbOutput.Length == 0)
            return "We don't have any history on this item, sorry.";
        return sbOutput.ToString();
    }

    private string getTableHeader(bool blnShowType) {
        //Extra header columns for extra detail
        string szTypeHeader = blnShowType ? "<th width='100px'>Type</th><th>Detail</th>" : "";
        //Specific id for sample history
        string szHeader = string.Format(@"<table id='{1}' class='history' width='100%' class='Normal' style='font-size: 0.9em' cellpadding='0' cellspacing='0'><thead>
                                    <tr class='ListHeader'><th width='140px'>Time</th><th width='100px'>Person</th>{0}<th>Change</th></tr></thead><tbody>", szTypeHeader, szInitialID);
        return szHeader;
    }

    private string getItemInfo(DataRow dr, string szValue) {
        DBLogType oLogType = (DBLogType)Convert.ToInt32(dr["TYPEID"]);
        switch (oLogType) {
            default:
                return string.Format("<td>{1}</td><td></td><td>{0}</td>", szValue, oLogType);
        }
    }

    /// <summary>
    /// Identifies which properties need replacement values
    /// </summary>
    /// <param name="szValue"></param>
    /// <returns></returns>
    public abstract string replaceProperties(string szValue);

    /// <summary>
    /// Find the item to replace and the IDs that need replacing
    /// </summary>
    /// <param name="szPropertyName"></param>
    /// <param name="szValue"></param>
    /// <returns></returns>
    private string getReplaceItem(string szPropertyName, string szValue, bool blnRecursiveReplace = false) {
        if (!blnRecursiveReplace)
            intEndIndex = 0;

        //Get start and end of the item we want to replace
        intStartIndex = szValue.IndexOf(szPropertyName, intEndIndex);
        if (intStartIndex < 0)
            return szValue;
        intEndIndex = szValue.IndexOf("<br/>", intStartIndex);
        if (intStartIndex == -1 || intEndIndex == -1)
            return szValue;

        //Get start and end of the initial ID
        int fromItemIndex = szValue.IndexOf("<i>", intStartIndex) + 3;
        int fromItemEndIndex = szValue.IndexOf("</i>", intStartIndex);
        if (fromItemIndex == -1 || fromItemEndIndex == -1)
            return szValue;

        //Get start and end of the changed ID
        int toItemIndex = szValue.IndexOf("<i>", fromItemIndex) + 3;
        int toItemEndIndex = szValue.IndexOf("</i>", toItemIndex);
        //Get the initial and changed ID numbers
        szInitialID = szValue.Substring(fromItemIndex, fromItemEndIndex - fromItemIndex);
        szChangedID = szValue.Substring(toItemIndex, toItemEndIndex - toItemIndex);
        //Get an item to replace and a replacement item
        return szValue.Substring(intStartIndex, intEndIndex - intStartIndex);
    }
}

public enum DBLogType {
    SaleData = 1,
    CampaignData = 2,
    SaleExpense = 3,
    SalesSplit = 4,
    UserSalesSplit = 5,
    CampaignAction = 6,
    CampaignProduct = 7,
    CampaignContributionSplit = 8,
    EOFYBonus = 9,
    PayrollModification = 10,
    Email = 11,
    EmailAutomation = 12
}

public enum EmailType {
    LeaveRequest = 0,
    Approval = 1,
    Rejection = 2,
    Reminder = 3,
    General = 4
}

public class EmailLog {

    /// <summary>
    ///
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Entry"></param>
    /// <param name="ObjectID"></param>
    /// <param name="ChildObjectID"></param>
    public static void addLog(EmailType Type, string Subject, string From, string To, string CC, string Body, int ObjectID) {
        sqlUpdate oSQL = new sqlUpdate("EMAILLOG", "ID", -1);
        oSQL.add("TYPEID", (int)Type);
        oSQL.add("SUBJECT", Subject);
        oSQL.add("SENTFROM", From);
        oSQL.add("SENTTO", To);
        oSQL.add("CC", CC);
        oSQL.add("Body", Body);
        oSQL.add("OBJECTID", ObjectID);
        DB.runNonQuery(oSQL.createInsertSQL());
    }
}

public class DBLog {
    public string szTableName = "";
    public int intUserID = 0;
    private StringBuilder sbLogItems = new StringBuilder();
    private DBLogType LogType = DBLogType.SaleData;
    private int intPrimaryKey = -1;

    public DBLog(string TableName, int PrimaryKey, DBLogType Type) {
        szTableName = TableName;
        intUserID = G.User.UserID;
        LogType = Type;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Entry"></param>
    /// <param name="ObjectID"></param>
    /// <param name="ChildObjectID"></param>
    public static void addGenericRecord(DBLogType Type, string Entry, int ObjectID, int ChildObjectID = -1) {
        addRecord(Type, Entry, ObjectID, ChildObjectID, G.User.UserID);
    }

    /// <summary>
    /// Adds data to the existing open log record
    /// </summary>
    /// <param name="LogRecord"></param>
    public void appendToLog(string LogRecord) {
        sbLogItems.Append(LogRecord + "<br/>");
    }

    public void writeLog() {
        if (sbLogItems.Length == 0)
            return;

        sqlUpdate oSQL = new sqlUpdate("LOGV2", "ID", -1);
        oSQL.add("TYPEID", (int)LogType);
        oSQL.add("USERID", intUserID);
        oSQL.add("OBJECTID", intPrimaryKey);
        oSQL.add("VALUE", sbLogItems.ToString());
        DB.runNonQuery(oSQL.createInsertSQL());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Entry"></param>
    /// <param name="ObjectID"></param>
    /// <param name="ChildObjectID"></param>
    /// <param name="UserID"></param>
    public static void addRecord(DBLogType Type, string Entry, int ObjectID, int ChildObjectID, int UserID = -1) {
        sqlUpdate oSQL = new sqlUpdate("LOGV2", "ID", -1);
        oSQL.add("TYPEID", (int)Type);
        oSQL.add("VALUE", Entry);
        oSQL.add("USERID", -1);

        if (ObjectID > -1)
            oSQL.add("OBJECTID", ObjectID);
        if (ChildObjectID > -1)
            oSQL.add("CHILDOBJECTID", ChildObjectID);

        DB.runNonQuery(oSQL.createInsertSQL());
        oSQL = null;
    }
}

/// <summary>
/// Defines the required functions to implement auditing properly in the application
/// </summary>
public abstract class AuditClass {
    protected ChangeTracker oCT;

    protected int setValue(string description, int oldVal, int newVal) {
        if (oldVal != newVal)
            oCT.addChange(description, oldVal.ToString(), newVal.ToString());
        return newVal;
    }

    protected double setValue(string description, double oldVal, double newVal) {
        if (oldVal != newVal)
            oCT.addChange(description, oldVal.ToString(), newVal.ToString());
        return newVal;
    }

    protected bool setValue(string description, bool oldVal, bool newVal) {
        if (oldVal != newVal)
            oCT.addChange(description, oldVal.ToString(), newVal.ToString());
        return newVal;
    }
}