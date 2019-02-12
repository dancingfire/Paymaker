using Microsoft.ApplicationBlocks.Data;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;


/// <summary>
/// Summary description for DB.
/// </summary>
public class DB {

    /// <summary>
    /// Trims and adds the quotes to the string.
    /// </summary>
    /// <param name="szSQL"></param>
    /// <returns></returns>
    public static string escape(string szSQL, bool TrimText = true) {
        if (szSQL == null)
            return "";
        if (TrimText)
            return szSQL.Replace("'", "''").Trim();
        else
            return szSQL.Replace("'", "''");
    }

    /// <summary>
    /// Redirets the user to a page that lets them know that what they were doing has failed
    /// </summary>
    /// <param name="blnFailed"></param>
    private static void checkForDBFailure(bool blnFailed) {
        if (blnFailed) {
            HttpContext.Current.Response.Redirect("../db_timeout.aspx");
            HttpContext.Current.Response.End();
        }
    }

    /// <summary>
    /// The connection to the DB based off the web config client
    /// </summary>
    public static string DBConn {
        get {
            string szCnn = ConfigurationManager.AppSettings["DB"];
            return szCnn.Replace("DBNAME", Client.DBName);
        }
    }

    /// <summary>
    /// The connection to the DB based off the web config client
    /// </summary>
    public static string BoxDiceDBConn {
        get {
            string szCnn = ConfigurationManager.AppSettings["DB"];
            return szCnn.Replace("DBNAME", "Fletchers_BoxDiceAPI").Replace("Paymaker", "Fletchers_BoxDiceAPI");
        }
    }

    /// <summary>
    /// Reads a value from a field protecting us from the null value (returns empty string in that case)
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static string readValue(object Value) {
        if (Value == System.DBNull.Value)
            return String.Empty;
        else
            return Convert.ToString(Value);
    }

    /// <summary>
    /// Reads a value from a field protecting us from the null value (returns Double.MinValue)
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static double readDouble(object Value, double NullValue = Double.MinValue) {
        if (Value == System.DBNull.Value)
            return NullValue;
        else
            return Convert.ToDouble(Value);
    }

    /// Returns either a valid string of DateVale
    /// </summary>
    /// <param name="DateValue"></param>
    /// <returns></returns>
    public static string readDateString(object DateValue) {
        if (DateValue == null)
            return "";

        DateTime dt = DB.readDate(DateValue);
        if (dt == DateTime.MinValue)
            return "";

        return Utility.formatDate(dt);
    }

    /// Returns either a valid string of DateVale
    /// </summary>
    /// <param name="DateValue"></param>
    /// <returns></returns>
    public static string readMoneyString(object MoneyValue, bool FormatForReport = false) {
        if (!Utility.IsNumeric(Convert.ToString(MoneyValue)))
            return "";

        Double dt = DB.readDouble(MoneyValue);
        if (dt == Double.MinValue)
            return "";
        if (FormatForReport)
            return Utility.formatReportMoney(dt);

        return Utility.formatMoney(dt);
    }

    /// <summary>
    /// Returns either a valid datetime or mindate if the date is null
    /// </summary>
    /// <param name="DateValue"></param>
    /// <returns></returns>
    public static DateTime readDate(object DateValue) {
        if (System.DBNull.Value == DateValue)
            return DateTime.MinValue;
        else
            return Convert.ToDateTime(DateValue);
    }

    /// <summary>
    /// Returns either a valid int or Int32.MinInt if the value is null
    /// </summary>
    /// <param name="DateValue"></param>
    /// <returns></returns>
    public static int readInt(object Value) {
        if (System.DBNull.Value == Value)
            return Int32.MinValue;
        else
            return Convert.ToInt32(Value);
    }

    /// <summary>
    /// Returns a boolean - null = false
    /// </summary>
    /// <param name="DateValue"></param>
    /// <returns></returns>
    public static bool readBool(object Value) {
        if (System.DBNull.Value == Value)
            return false;
        else
            return Convert.ToBoolean(Value);
    }

    /// <summary>
    /// Returns either a valid string or a blank value
    /// </summary>
    /// <param name="DateValue"></param>
    /// <returns></returns>
    public static string readString(object Value) {
        if (System.DBNull.Value == Value)
            return "";
        else
            return Convert.ToString(Value);
    }

    /// <summary>
    /// Runs a SQL statement and returns an SQLRecordSet.
    /// Also logs the SQL to the Session Debug string
    /// </summary>
    /// <param name="ConnectionInfo"></param>
    /// <param name="SQL"></param>
    /// <returns></returns>

    public static SqlDataReader runReader(string SQL) {
        int intDBAttempt = 1;
        int AllowedAttempts = 2; //Default it should run atleast twice before generating error
        bool blnDBFailed = true;
        SqlDataReader oDr = null;
        if (G.EnableSQLTrace)
            SQL += "--SQLTRACE";
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            oDr = runReaderTx(SQL, out blnDBFailed);
        }
        checkForDBFailure(blnDBFailed);
        return oDr;
    }

    public static SqlDataReader runReader(string SQL, int AllowedAttempts) {
        int intDBAttempt = 1;
        bool blnDBFailed = true;
        SqlDataReader oDr = null;
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            oDr = runReaderTx(SQL, out blnDBFailed);
        }
        checkForDBFailure(blnDBFailed);
        return oDr;
    }

    private static SqlDataReader runReaderTx(string SQL, out bool blnFail) {
        SqlConnection oSqlCnn = new SqlConnection(DBConn);
        oSqlCnn.Open();
        if (G.EnableSQLTrace)
            SQL += "--SQLTRACE";
        SqlTransaction myTrans = oSqlCnn.BeginTransaction();
        SqlCommand myCommand = new SqlCommand(SQL, oSqlCnn, myTrans);
        myCommand.CommandTimeout = 500;
        SqlDataReader oDr = null;
        try {
            oDr = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
            HttpContext.Current.Session["DEBUGSQL"] = "";
            blnFail = false;
        } catch (Exception ex) {
            blnFail = true;
            HttpContext.Current.Session["DEBUGSQL"] = SQL + ex.ToString();
            oSqlCnn.Close();
            if (ex.Message.IndexOf("Timeout expired.") == -1) {
                throw;
            }
        }
        return oDr;
    }

    /// <summary>
    /// Runs a SQL statement and returns an DataSet.
    /// Also logs the SQL to the Session Debug string
    /// </summary>
    /// <param name="ConnectionInfo"></param>
    /// <param name="SQL"></param>
    /// <returns></returns>
    ///

    public static DataSet runDataSet(string SQL, string ConnectionInfo = "") {
        int intDBAttempt = 1;
        int AllowedAttempts = 2; //Default it should run atleast twice before generating error
        bool blnDBFailed = true;
        DataSet oDs = new DataSet();
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            oDs = runDataSetTx(SQL, out blnDBFailed, ConnectionInfo);
        }
        checkForDBFailure(blnDBFailed);
        return oDs;
    }

    public static DataSet runDataSet(string SQL, int AllowedAttempts) {
        int intDBAttempt = 1;
        bool blnDBFailed = true;
        DataSet oDs = new DataSet();
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            oDs = runDataSetTx(SQL, out blnDBFailed);
        }
        checkForDBFailure(blnDBFailed);
        return oDs;
    }

    private static DataSet runDataSetTx(string SQL, out bool blnFail, string ConnectionInfo = "") {
        if (String.IsNullOrEmpty(ConnectionInfo))
            ConnectionInfo = DBConn;
        SqlConnection oSqlCnn = new SqlConnection(ConnectionInfo);
        oSqlCnn.Open();
        if (G.EnableSQLTrace)
            SQL += "--SQLTRACE";
        SqlTransaction myTrans = oSqlCnn.BeginTransaction();
        SqlCommand myCommand = new SqlCommand(SQL, oSqlCnn, myTrans);
        myCommand.CommandTimeout = 500;

        SqlDataAdapter myDA = new SqlDataAdapter();
        myDA.SelectCommand = myCommand;
        DataSet oDs = new DataSet();
        try {
            myDA.Fill(oDs);
            HttpContext.Current.Session["DEBUGSQL"] = "";
            myTrans.Commit();
            blnFail = false;
        } catch (Exception ex) {
            if (ex.Message.ToString().Substring(0, 16) == "Timeout expired.") {
                blnFail = false;
                HttpContext.Current.Session["DEBUGSQL"] = SQL + ex.ToString();
                myTrans.Rollback();
            } else {
                blnFail = true;
                HttpContext.Current.Session["DEBUGSQL"] = SQL + ex.ToString();
                myTrans.Rollback();
                if (ex.Message.IndexOf("Timeout expired.") == -1) {
                    throw;
                }
            }
        } finally {
            myCommand.Dispose();
            oSqlCnn.Close();
            oSqlCnn.Dispose();
        }
        return oDs;
    }

    public static DataSet runLongDataSet(string SQL) {
        int intDBAttempt = 1;
        int AllowedAttempts = 2; //Default it should run atleast twice before generating error
        bool blnDBFailed = true;
        DataSet oDs = new DataSet();
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            oDs = runDataSetTx(SQL, out blnDBFailed);
        }
        checkForDBFailure(blnDBFailed);
        return oDs;
    }

    public static DataSet runLongDataSet(string SQL, int AllowedAttempts) {
        int intDBAttempt = 1;
        bool blnDBFailed = true;
        DataSet oDs = new DataSet();
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            oDs = runDataSetTx(SQL, out blnDBFailed);
        }
        checkForDBFailure(blnDBFailed);
        return oDs;
    }

    private static DataSet runLongDataSetTx(string SQL, out bool blnFail) {
        SqlConnection oSqlCnn = new SqlConnection(DBConn);
        oSqlCnn.Open();
        SqlTransaction myTrans = oSqlCnn.BeginTransaction();
        SqlCommand myCommand = new SqlCommand(SQL, oSqlCnn, myTrans);
        myCommand.CommandTimeout = 500;
        SqlDataAdapter myDA = new SqlDataAdapter();
        myDA.SelectCommand = myCommand;
        DataSet oDs = new DataSet();
        try {
            myDA.Fill(oDs);
            HttpContext.Current.Session["DEBUGSQL"] = "";
            myTrans.Commit();
            blnFail = false;
        } catch (Exception ex) {
            blnFail = true;
            HttpContext.Current.Session["DEBUGSQL"] = SQL + ex.ToString();
            myTrans.Rollback();
            if (ex.Message.IndexOf("Timeout expired.") == -1) {
                throw;
            }
        } finally {
            myCommand.Dispose();
            oSqlCnn.Close();
            oSqlCnn.Dispose();
        }
        return oDs;
    }

    /// <summary>
    /// Runs the statement if we are in debug mode
    /// </summary>
    /// <param name="SQL"></param>
    public static void runDebug(string SQL) {
        if (G.DebugMode)
            DB.runNonQuery(SQL);
    }

    /// <summary>
    /// Runs a SQL statement.
    /// Also logs the SQL to the Session Debug string
    /// May 24 - GF:    Added long timeout to this function.
    /// </summary>
    /// <param name="ConnectionInfo"></param>
    /// <param name="SQL"></param>
    /// <returns></returns>

    #region ExecuteNonQuery

    public static void runNonQuery(string SQL, string connectionString = null) {
        if (String.IsNullOrEmpty(connectionString ))
            connectionString = DBConn;

        int intDBAttempt = 1;
        int AllowedAttempts = 2; //Default it should run atleast twice before generating error
        bool blnDBFailed = true;
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            runNonQueryTx(SQL, out blnDBFailed, connectionString);
        }
        checkForDBFailure(blnDBFailed);
    }

    public static void runNonQueryRAID(string SQL) {
        bool blnDBFailed = true;
        SqlConnection oSqlCnn = new SqlConnection(DBConn);
        oSqlCnn.Open();
        oSqlCnn.ChangeDatabase("RAID");
        SqlCommand myCommand = new SqlCommand(SQL, oSqlCnn);
        myCommand.CommandTimeout = 300;
        try {
            myCommand.ExecuteNonQuery();
            blnDBFailed = false;
            HttpContext.Current.Session["DEBUGSQL"] = "";
        } catch (Exception ex) {
        } finally {
            myCommand.Dispose();
            oSqlCnn.Close();
            oSqlCnn.Dispose();
        }
        checkForDBFailure(blnDBFailed);
    }

    public static void runNonQuery(string SQL, int AllowedAttempts) {
        int intDBAttempt = 1;
        bool blnDBFailed = true;
        while (blnDBFailed && intDBAttempt <= AllowedAttempts) {
            intDBAttempt++;
            runNonQueryTx(SQL, out blnDBFailed);
        }
        checkForDBFailure(blnDBFailed);
    }

    /// <summary>
    /// Allows the very long run times that are required for the import process.
    /// </summary>
    /// <param name="SQL"></param>
    /// <param name="blnFail"></param>
    public static void runImportNonQuery(string SQL, out bool blnFail) {
        SqlConnection oSqlCnn = new SqlConnection(DBConn);
        oSqlCnn.Open();
        SqlTransaction myTrans = oSqlCnn.BeginTransaction();
        SqlCommand myCommand = new SqlCommand(SQL, oSqlCnn, myTrans);
        myCommand.CommandTimeout = 3000;
        try {
            myCommand.ExecuteNonQuery();
            HttpContext.Current.Session["DEBUGSQL"] = "";
            myTrans.Commit();
            blnFail = false;
        } catch (Exception ex) {
            blnFail = true;
            HttpContext.Current.Session["DEBUGSQL"] = SQL + ex.ToString();
            myTrans.Rollback();
            if (ex.Message.IndexOf("Timeout expired.") == -1) {
                throw;
            }
        } finally {
            myCommand.Dispose();
            oSqlCnn.Close();
            oSqlCnn.Dispose();
        }
    }

    private static void runNonQueryTx(string SQL, out bool blnFail, String connectionString = null) {
        if (connectionString == null)
            connectionString = DBConn;
        SqlConnection oSqlCnn = new SqlConnection(connectionString);
        oSqlCnn.Open();
        if (SQL.IndexOf("UPDATE BATCH ") > -1)
            SQL = SQL.Replace("UPDATE BATCH", "UPDATE BATCH WITH (ROWLOCK)");
        if (G.EnableSQLTrace)
            SQL += "--SQLTRACE";
        SqlTransaction myTrans = oSqlCnn.BeginTransaction();
        SqlCommand myCommand = new SqlCommand(SQL, oSqlCnn, myTrans);
        myCommand.CommandTimeout = 300;
        try {
            myCommand.ExecuteNonQuery();
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session["DEBUGSQL"] = "";
            myTrans.Commit();
            blnFail = false;
        } catch (Exception ex) {
            blnFail = true;
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session["DEBUGSQL"] = SQL + ex.ToString();
            myTrans.Rollback();
            if (ex.Message.IndexOf("Timeout expired.") == -1) {
                throw;
            }
        } finally {
            myCommand.Dispose();
            oSqlCnn.Close();
            oSqlCnn.Dispose();
        }
    }

    public static void emptyQueue(StringBuilder sbSQL) {
        if (sbSQL.Length > 0)
            DB.runNonQuery(sbSQL.ToString());
    }

    #endregion ExecuteNonQuery

    /// <summary>
    /// Sets the Session SQL Debug variable to the passed in statement.
    /// </summary>
    /// <param name="SQL"></param>
    public static void setDebugSQL(string SQL) {
        HttpContext.Current.Session["DEBUGSQL"] = SQL;
    }

    /// <summary>
    /// Clears the session debug variable.
    /// </summary>
    public static void clearDebugSQL() {
        HttpContext.Current.Session["DEBUGSQL"] = "";
    }

    public static int getScalar(string szSQL, int intDefault, String connectionString = null) {
        if (connectionString == null)
            connectionString = DBConn;

        if (G.EnableSQLTrace)
            szSQL += "--SQLTRACE";
        object oScalar = SqlHelper.ExecuteScalar(connectionString, CommandType.Text, szSQL);
        if (oScalar == DBNull.Value || oScalar == null)
            return intDefault;
        else
            return Convert.ToInt32(oScalar);
    }

    public static double getScalar(string szSQL, double Default, String connectionString = null) {
        if (connectionString == null)
            connectionString = DBConn;

        if (G.EnableSQLTrace)
            szSQL += "--SQLTRACE";
        object oScalar = SqlHelper.ExecuteScalar(connectionString, CommandType.Text, szSQL);
        if (oScalar == DBNull.Value || oScalar == null)
            return Default;
        else
            return Convert.ToDouble(oScalar);
    }

    public static string getScalar(string szSQL, string szDefault, String connectionString = null) {
        if (connectionString == null)
            connectionString = DBConn;

        if (G.EnableSQLTrace)
            szSQL += "--SQLTRACE";
        object oScalar = SqlHelper.ExecuteScalar(connectionString, CommandType.Text, szSQL);
        if (oScalar == DBNull.Value || oScalar == null)
            return szDefault;
        else
            return Convert.ToString(oScalar);
    }

    public static object getScalar(string szSQL) {
        SqlConnection oSqlCnn = new SqlConnection(DBConn);
        oSqlCnn.Open();
        if (G.EnableSQLTrace)
            szSQL += "--SQLTRACE";
        SqlCommand myCommand = new SqlCommand(szSQL, oSqlCnn);
        myCommand.CommandTimeout = 500;
        object oScalar = myCommand.ExecuteScalar();
        oSqlCnn.Close();
        oSqlCnn.Dispose();
        return oScalar;
    }

    public class Config {

        public static string getValue(string Key) {
            return (string)getScalar(String.Format("SELECT VALUE FROM CONFIG WHERE NAME = '{0}'", Key));
        }

        public static string getValue(string Key, int UserID) {
            string szSQL = String.Format("SELECT ISNULL(VALUE, '') from CONFIG WHERE USERID = {0} AND NAME = '{1}'", UserID, Key);
            return (string)getScalar(szSQL);
        }
    }

    public class Paymaker_User {

        public static SqlDataReader loadList(string RoleList, bool IncludeAll, int CurrentValue = -1, bool CommissionOnly = false) {
            string szSQL = "SELECT ID, INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME AS NAME, 1 AS SORTORDER FROM DB_USER  ";

            string szFilter = " WHERE (ISACTIVE = 1 AND ID > 0 AND ISDELETED = 0 ";
            if (RoleList != "")
                szFilter += " AND  ROLEID IN (" + RoleList + ")";
            if (CommissionOnly)
                szFilter += " AND ISPAID = 1";
            szFilter += ")";
            if (CurrentValue != -1)
                szFilter += " OR ID = " + CurrentValue;
            szSQL += szFilter;
           
            if (IncludeAll)
                szSQL += "UNION SELECT -1, 'Select...' AS NAME, 0 AS SORTORDER ";
            szSQL += "ORDER BY SORTORDER, INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME";
            return SqlHelper.ExecuteReader(DBConn, CommandType.Text, szSQL);
        }
    }

    public class Office {

        /// <summary>
        /// Loads the office list ordered by company and office, ignoring the test office that contains the non-Fletchers users.
        /// </summary>
        /// <returns></returns>
        public static DataSet loadOfficeList(string CompanyIDList = "") {
            string szFilter = "";
            if (!String.IsNullOrWhiteSpace(CompanyIDList))
                szFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", CompanyIDList);

            string szSQL = szSQL = string.Format(@"
                SELECT L_OFFICE.ID, L_OFFICE.NAME, L_OFFICE.DESCRIPTION
                FROM LIST L_OFFICE
                JOIN LIST L_COMPANY ON L_OFFICE.COMPANYID = L_COMPANY.ID
                WHERE L_OFFICE.LISTTYPEID = {0} AND L_OFFICE.ISACTIVE = 1 AND L_OFFICE.NAME != 'Test'
                {1}
                ORDER BY L_COMPANY.NAME, L_OFFICE.NAME", (int)ListType.Office, szFilter);
            return DB.runDataSet(szSQL);
        }
    }
}

public static class DebugInfo {

    public static void clearDebugInfo(bool IsDebug) {
        if (IsDebug)
            DB.runNonQuery("TRUNCATE TABLE DEBUG");
    }

    public static void writeDebugInfo(bool IsDebug, string szMSG) {
        if (IsDebug)
            DB.runNonQuery("INSERT INTO DEBUG(SQL) VALUES('" + DB.escape(szMSG) + "')");
    }
}

/// <summary>
/// Allows SQL insert statements to be run in batches for efficiency
/// You have to pass in the Insert statment first and then append the value segments
/// This will create insert statements of the format :
/// Insert into X(X, Y, Z) VALUES (1, 2, 3), (1, 2, 3), (1, 2, 3)
/// </summary>
public class SQLInsertQueue {
    private StringBuilder oSB = new StringBuilder();
    private int QueueMax = 100;
    private string szInsertString = "";
    private int intSQLCount = 0;
    private string Connection;

    public SQLInsertQueue(string InsertString, String Connection = null) {
        QueueMax = 999 / InsertString.Split(',').Length; //SQL restiction on number of hardcoded field values

        szInsertString = InsertString;
        this.Connection = Connection;
    }

    /// <summary>
    /// This should batch up queries to send together.
    /// </summary>
    /// <param name="szSQL"></param>
    public void addSQL(string szSQL) {
        if (intSQLCount > 0)
            oSB.Append(", " + szSQL);
        else
            oSB.Append(szSQL);

        intSQLCount += 1;
        if (intSQLCount > QueueMax) {
            DB.runNonQuery(szInsertString + oSB.ToString(), Connection);
            intSQLCount = 0;
            oSB.Length = 0;
        }
    }

    /// <summary>
    /// Runs any remaining SQL and empties the list
    /// </summary>
    public void flush() {
        if (oSB.Length > 0) {
            DB.runNonQuery(szInsertString + oSB.ToString(), Connection);
            oSB.Length = 0;
            intSQLCount = 0;
        }
    }
}

/// <summary>
/// Allows SQL statements to be run in batches for efficiency
/// </summary>
public class SQLQueue {
    private string szGlobalUpdateSQL = "";
    private int QueueMax = 100;

    private int intSQLCount = 0;

    public SQLQueue(int MaxStatements) {
        QueueMax = MaxStatements;
    }

    /// <summary>
    /// This should batch up queries to send together.
    /// </summary>
    /// <param name="szSQL"></param>
    public void addSQL(string szSQL) {
        szGlobalUpdateSQL += szSQL;
        intSQLCount += 1;
        if (intSQLCount > 100) {
            DB.runNonQuery(szGlobalUpdateSQL);
            intSQLCount = 0;
            szGlobalUpdateSQL = "";
        }
    }

    /// <summary>
    /// Runs any remaining SQL and empties the list
    /// </summary>
    public void flush() {
        if (szGlobalUpdateSQL != "") {
            DB.runNonQuery(szGlobalUpdateSQL);
            szGlobalUpdateSQL = "";
            intSQLCount = 0;
        }
    }
}