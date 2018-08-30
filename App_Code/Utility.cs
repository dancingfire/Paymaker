using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// What is the payment structure for the current agent
/// </summary>
public enum PayBand {
    ToBeDetermined = -1,
    JuniorNoTeamRetainer = 0,
    JuniorNoTeamSalary = 1,
    JuniorTeamRetainer = 2,
    JuniorTeamSalary = 3,
    Normal = 4,
    SpecialCase85kBase = 5
}

public enum ExportType {
    UserTx = 1,
    Campaign = 2,
    SalesCommission = 3
}

public enum TimesheetType {
    NotUsingTimeSheet = 0,
    InternalServices = 1,
    PaidInAdvance = 2
}

public enum CommissionType {
    Lead = 6,
    List = 10,
    Manage = 7,
    Sell = 8,
    ServiceArea = 9,
    Fletchers = 35,
    Mentoring = 89
}

public enum ListType {
    Office = 1,
    Expense = 2,
    Commission = 3,
    Income = 4,
    SaleStatus = 5,
    OffTheTop = 6,
    Company = 7,
    MYOBExport = 8,
    CampaignGL = 9,
    TXCategory = 10,
}

public enum AmountType {
    Dollar = 0,
    Percent = 1
}

public enum TrueFalse {
    False = 0,
    True = 1
}

public enum CampaignStatus {
    New = 0,
    InProgress = 1,
    Completed = 100
}

public enum CampaignPageView {
    Completed = 0,
    All = 1,
    New = 2,
    ExceedingAuthority = 3,
    PartialInvoiced = 4,
    InvoiceDue = 5,
    InvoiceChanged = 6,
    DueForCollection = 7,
    Overdue = 8,
    Actioned = 9,
    InvoiceAging = 10
}

public enum ContributionStatus {
    New = 0,
    SentToFinance = 1,
    Modified = 2
}

public enum ContributionType {
    Vendor = 0,
    Agent = 1,
    Fletchers = 2
}

public enum PaymentOption {
    Days7 = 0,
    Days14 = 1,
    Days28 = 2,
    AfterInvoicing = 3,
    FixedDate = 4
}

public enum UserSalesSplitStatus {
    Deleted = -1,
    Entered = 0,
    Modified = 1,
    MYOBREversed = 2
}

internal static class Extensions {

    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }
}

public class Utility {

    public Utility() {
    }

    public static bool isSet(object o) {
        return !(o == null);
    }

    /// <summary>
    /// To use for debuging - writes DataTable to a file
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="FileName"></param>
    public static void dataTableToCSVFile(DataTable dt, string FileName = @"C:\Temp\output.csv") {
        StringBuilder sb = new StringBuilder();

        IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                          Select(column => '"' + column.ColumnName.Replace(@"""", " ") + '"');
        sb.AppendLine(string.Join(",", columnNames));

        foreach (DataRow row in dt.Rows) {
            IEnumerable<string> fields = row.ItemArray.Select(field => '"' + field.ToString().Replace(@"""", " ") + '"');
            sb.AppendLine(string.Join(",", fields));
        }

        File.WriteAllText(FileName, sb.ToString());
    }

    /// <summary>
    /// Return the start of the financial year given a date
    /// </summary>
    /// <param name="dtCurrDate"></param>
    /// <returns></returns>
    public static DateTime getFinYearStart(DateTime dtCurrDate) {
        if (dtCurrDate.Month <= 6)
            return new DateTime(dtCurrDate.Year - 1, 7, 1);
        else
            return new DateTime(dtCurrDate.Year, 7, 1);
    }

    /// <summary>
    /// Return the end of the financial year given a date
    /// </summary>
    /// <param name="dtCurrDate"></param>
    /// <returns></returns>
    public static DateTime getFinYearEnd(DateTime dtCurrDate) {
        if (dtCurrDate.Month > 6)
            return new DateTime(dtCurrDate.Year + 1, 6, 30);
        else
            return new DateTime(dtCurrDate.Year, 6, 30);
    }

    public static void doWrite(string szText) {
        HttpContext.Current.Response.Write(szText);
        HttpContext.Current.Response.Flush();
    }

    /// <summary>
    /// Compares a passed in string value with a column in a datarow
    /// </summary>
    /// <param name="dr"></param>
    /// <param name="ColName"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static bool valueHasChanged(DataRow dr, string ColName, string Value) {
        if (dr == null)
            return true;

        if (System.DBNull.Value == dr[ColName] && Value == null)
            return false;
        if (Value == null)
            Value = "";
        return Convert.ToString(dr[ColName]).Trim() != Value.Trim();
    }

    /// <summary>
    /// Compares a passed in string value with a column in a datarow
    /// </summary>
    /// <param name="dr"></param>
    /// <param name="ColName"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static bool valueHasChanged(DataRow dr, string ColName, bool Value) {
        if (dr == null)
            return true;
        if (System.DBNull.Value == dr[ColName] && Value == null)
            return false;

        return Convert.ToBoolean(dr[ColName]) != Value;
    }

    /// <summary>
    /// Returns a MYOB export ID to be used in the export
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Name"></param>
    /// <returns></returns>
    public static int getMYOBExportID(ExportType Type, string Name) {
        sqlUpdate oSQL = new sqlUpdate("MYOBEXPORT", "ID", -1);

        oSQL.add("NAME", Name);
        oSQL.add("EXPORTTYPEID", (int)Type);
        oSQL.add("UPDATETIME", Utility.formatDate(System.DateTime.Now));
        DB.runNonQuery(oSQL.createInsertSQL());
        return DB.getScalar("SELECT MAX(ID) FROM MYOBEXPORT", -1);
    }

    /// <summary>
    /// Returns the end of the last mont
    /// </summary>
    /// <returns></returns>
    public static DateTime getEndOfLastMonth() {
        DateTime now = DateTime.Now;

        DateTime lastDayLastMonth = new DateTime(now.Year, now.Month, 1);
        return lastDayLastMonth.AddDays(-1);
    }

    /// <summary>
    /// Preferred Escape script to be used in client side JS.
    ///
    /// </summary>
    /// <param name="szInput"></param>
    /// <returns></returns>
    public static string EscapeJS(string szInput) {
        return Microsoft.JScript.GlobalObject.escape(szInput);
    }

    public static void LogSQL(string szSQL, bool blnIsDebug, int intBatchID, string oCnn) {
        if (blnIsDebug) {
            szSQL = "INSERT INTO DEBUG(SQL, BATCHID) VALUES('" + szSQL.Replace("'", "''") + "', " + intBatchID.ToString() + ")";
            SqlHelper.ExecuteNonQuery(oCnn, CommandType.Text, szSQL);
        }
    }

    public static string getSelListboxItems(ListBox oList) {
        string szIDs = "";
        foreach (ListItem oLI in oList.Items) {
            if (oLI.Selected) {
                if (szIDs != "")
                    szIDs += ",";
                szIDs += oLI.Value;
            }
        }
        return szIDs;
    }

    public static void ShowValidationError() {
        doPrintError("Invalid input");
    }

    // Date - 1900-01-01
    public static DateTime MinDBDate = new DateTime(599266080000000000);

    public static bool isDateTime(string szValue) {
        DateTime dtTest;
        bool isDateTime = DateTime.TryParse(szValue, out dtTest);
        return isDateTime && dtTest > MinDBDate;
    }

    public static string isDouble(string value) {
        double validDouble;
        if (Double.TryParse(value, out validDouble))
            return value;
        else
            return "0.00";
    }

    public static string nl2br(string szIn) {
        if (szIn == null)
            return "";
        else
            return szIn.Replace(Environment.NewLine, "<br>");
    }

    /// <summary>
    /// Checks if a given value is in a comma-separated list of values
    ///
    /// </summary>
    /// <param name="ValueToFind"></param>
    /// <param name="CommaSeparatedString"></param>
    public static bool InCommaSeparatedString(string ValueToFind, string CommaSeparatedString) {
        CommaSeparatedString = "," + CommaSeparatedString + ",";
        return CommaSeparatedString.IndexOf("," + ValueToFind + ",") > -1;
    }

    public static void setListBoxItems(ref ListBox oList, string szListOfIDs) {
        if (szListOfIDs == "")
            return;
        Utility.RemoveWhitespace(szListOfIDs);
        szListOfIDs = "," + szListOfIDs + ",";

        foreach (ListItem oLI in oList.Items) {
            oLI.Selected = szListOfIDs.IndexOf("," + oLI.Value + ",") > -1;
        }
    }

    public static void setListBoxItemsError(ref DropDownList oList, string ListValue) {
        Utility.RemoveWhitespace(ListValue);
        if (ListValue == "" || ListValue == "null" || ListValue == "NULL")
            return;
        oList.ClearSelection();
        bool blnFound = false;
        foreach (ListItem oLI in oList.Items) {
            oLI.Selected = oLI.Value == ListValue;
            if (oLI.Selected) {
                blnFound = true;
                break; //Can only select one item.
            }
        }
        if (!blnFound) {
            ListItem oItem = new ListItem("Err : " + ListValue, ListValue);
            oList.Items.Add(oItem);
            setListBoxItemsError(ref oList, ListValue);
        }
    }

    public static void setListBoxItems(ref DropDownList oList, string ListValue) {
        //if (G.CurrentUserID == 0)
        //    setListBoxItemsError(ref oList, ListValue);
        //else
        Utility.RemoveWhitespace(ListValue);
        if (ListValue == "" || ListValue == "null" || ListValue == "NULL")
            return;
        oList.ClearSelection();
        foreach (ListItem oLI in oList.Items) {
            oLI.Selected = oLI.Value == ListValue;
            if (oLI.Selected)
                break; //Can only select one item.
        }
    }

    public static void setListBoxItems(ref DropDownList oList, string ListText, bool blnText) {
        Utility.RemoveWhitespace(ListText);
        if (ListText == "" || ListText == "null" || ListText == "NULL")
            return;
        oList.ClearSelection();
        foreach (ListItem oLI in oList.Items) {
            oLI.Selected = oLI.Text == ListText;
            if (oLI.Selected)
                break; //Can only select one item.
        }
    }

    public static void WriteLine(string szText) {
        HttpContext.Current.Response.Write(szText + "<br/>");
        HttpContext.Current.Response.Flush();
    }

    /// <summary>
    /// Is true if the user comes from an internal Paymaker IP address.
    /// </summary>
    /// <returns></returns>
    public static bool isUserLocalToPaymaker() {
        string szLocalAddr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        return szLocalAddr.IndexOf("172.16") > -1;
    }

    public static void loadPayPeriodList(ref DropDownList oList) {
        string szSQL = "SELECT ID, DATENAME(MM, STARTDATE) AS NAME FROM PAYPERIOD ORDER BY ENDDATE DESC";

        oList.DataSource = DB.runReader(szSQL);
        oList.DataValueField = "ID";
        oList.DataTextField = "NAME";
        oList.DataBind();
        setListBoxItems(ref oList, G.CurrentPayPeriod.ToString());
        oList.Items.Insert(0, new ListItem("All..", ""));
    }

    public static void BindList(ref ListBox oList, ref SqlDataReader oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
        oData.Close();
        oData = null;
    }

    public static void BindList(ref ListBox oList, DataSet oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
    }

    public static void BindList(ref DropDownList oList, DataSet oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
    }

    public static void bindAgentList(ref DropDownList oList, bool ShowInActive = false) {
        var data = G.UserInfo.UserList.OrderBy(o => o.LastName).ThenBy(o => o.FirstName); ;
        oList.DataSource = data;
        oList.DataValueField = "ID";
        oList.DataTextField = "NAME";
        oList.DataBind();
    }

    public static void BindList(ref DropDownList oList, ref DataSet oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
    }

    public static void BindList(ref DropDownList oList, ArrayList oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
    }

    /// <summary>
    /// Use this one when you need to use the dataset after you have loaded up the list.
    /// </summary>
    /// <param name="oList"></param>
    /// <param name="oData"></param>
    /// <param name="szIDCol"></param>
    /// <param name="szValueCol"></param>
    public static void BindList(ref ListBox oList, ref DataSet oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
    }

    public static void BindList(ref ListBox oList, DataTable oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
    }

    public static void BindList(ref ListBox oList, SqlDataReader oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
        oData.Close();
        oData = null;
    }

    public static void BindList(ref DropDownList oList, SqlDataReader oData, string szIDCol, string szValueCol) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
        oData.Close();
        oData = null;
    }

    public static void BindList(ref DropDownList oList, ref DataSet oData, string szIDCol, string szValueCol, bool NullDataset) {
        oList.DataSource = oData;
        oList.DataValueField = szIDCol;
        oList.DataTextField = szValueCol;
        oList.DataBind();
        if (NullDataset)
            oData = null;
    }

    public static void doPrintError(string szError) {
        HttpContext.Current.Response.Write(szError);
        HttpContext.Current.Response.End();
    }

    public static void doPrintMessage(string szMessage) {
        HttpContext.Current.Response.Write(szMessage);
        HttpContext.Current.Response.Flush();
    }

    public static string RemoveWhitespace(string szInput) {
        if (szInput == null)
            return szInput;
        Regex r = new Regex(@"\s+");         // remove all whitespace
        return r.Replace(szInput, "");
    }

    public static string Escape(string szInput) {
        szInput = szInput.Replace("'", "\x27");    // JScript encode apostrophes
        szInput = szInput.Replace("'", "\x27");    // JScript encode apostrophes
        szInput = szInput.Replace("\"", "\x22");   // JScript encode double-quotes
        szInput = HttpUtility.HtmlEncode(szInput);  // encode chars special to html
        return szInput;
    }

    public static string getDate() {
        return DateTime.Today.ToString("MMM dd, yyyy");
    }

    public static void doShowError(string szMessage) {
        string szhtml = "<div style='width: 400px; padding: 20px; margin-top: 30px; font-size: 12pt; background: silver; border: solid 2px #615257; color: red'>";
        szhtml += szMessage + "</div>";
        HttpContext.Current.Response.Write(szhtml);
        HttpContext.Current.Response.Flush();
        HttpContext.Current.Response.End();
    }

    /// <summary>
    /// USe this in the app where you can't afford to see commas
    /// </summary>
    /// <param name="dAmount"></param>
    /// <returns></returns>
    public static string formatMoney(double dAmount) {
        return Math.Round(dAmount, 2).ToString("#0.00");
    }

    /// <summary>
    /// Use this in the reports wher eyou want nicely formatted money where you can't afford to see commas
    /// </summary>
    /// <param name="dAmount"></param>
    /// <returns></returns>
    public static string formatReportMoney(double dAmount) {
        return Math.Round(dAmount, 2).ToString("C2");
    }

    public static string formatMoneyShort(double dAmount) {
        return Math.Round(dAmount).ToString("#0");
    }

    public static string formatDate(DateTime dtDate) {
        return dtDate.ToString("MMM dd, yyyy");
    }

    public static string formatDateForMYOBExport(string szDate) {
        if (!string.IsNullOrEmpty(szDate.Trim())) {
            return Convert.ToDateTime(szDate).ToString("dd'/'MM'/'yyyy");
        }
        return "";
    }

    public static string formatDateForMYOBExport(DateTime Date) {
        if (Date != null && Date != DateTime.MinValue) {
            return Date.ToString("dd'/'MM'/'yyyy");
        }
        return "";
    }

    public static string formatDate(string szDate) {
        if (szDate == null || szDate.Trim() == "")
            return "";
        else
            try {
                DateTime dtVal = Convert.ToDateTime(szDate);
                if (dtVal == DateTime.MinValue)
                    return "";
                return dtVal.ToString("MMM dd, yyyy");
            } catch {
                HttpContext.Current.Response.Write(szDate);
                return "";
            }
    }

    public static string formatDateTime(DateTime dtDate) {
        if (dtDate == DateTime.MinValue)
            return "";
        else
            return dtDate.ToString("MMM dd, yyyy HH:mm");
    }

    public static DateTime checkDBDate(object oDate) {
        if (oDate == System.DBNull.Value)
            return DateTime.MinValue;
        else
            return Convert.ToDateTime(oDate);
    }

    public static string getWebRootPath() {
        string szServer = HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToString();
        string szHTTPs = HttpContext.Current.Request.ServerVariables["HTTPS"].ToString();

        string szReturn = "http://";
        if (szHTTPs.ToLower() == "on")
            szReturn = "https://";
        szReturn += szServer;
        string szVirtualDir = ConfigurationSettings.AppSettings["VirtualDirectory"];
        szReturn += szVirtualDir;
        return szReturn;
    }

    /// <summary>
    /// Format a string variable so it's safe to use in a SQL statement
    /// - replace single apostrophe with double apostrophe
    /// - remove leading and trailing space chars
    /// </summary>
    /// <param name="szValue"></param>
    /// <returns></returns>
    public static string formatForDB(string szValue) {
        if (szValue == null)
            return "";
        szValue = szValue.Trim();
        szValue = szValue.Replace("'", "''");
        return szValue;
    }

    /// <summary>
    /// Format a string variable so it's safe to use in a SQL statement
    /// - replace single apostrophe with double apostrophe
    /// - remove leading and trailing space chars
    /// - trims the string to the MaxLength fit for DB
    /// </summary>
    /// <param name="szValue"></param>
    /// <param name="intMaxLength"></param>
    /// <returns></returns>
    public static string formatforDB(string szValue, int intMaxLength) {
        szValue = formatForDB(szValue);
        if (szValue.Length > intMaxLength)
            szValue = szValue.Substring(0, intMaxLength);
        return szValue;
    }

    public static bool findIDInList(ref ListBox oList, string szValue) {
        bool blnFound = false;
        foreach (ListItem oItem in oList.Items) {
            if (oItem.Value == szValue)
                blnFound = true;
        }
        return blnFound;
    }

    public static void setUpControls(Control oCMain) {
        foreach (Control oC in oCMain.Controls) {
            if (oC is ListBox) {
                ListBox oL = (ListBox)oC;
                oL.Attributes["onchange"] += "dirtyPage(this);";
            } else if (oC is DropDownList) {
                DropDownList oD = (DropDownList)oC;
                oD.Attributes["onchange"] += "dirtyPage(this);";
            } else if (oC is CheckBox) {
                CheckBox oCB = (CheckBox)oC;
                oCB.Attributes["onClick"] += "dirtyPage(this);";
            } else if (oC is TextBox) {
                TextBox oT = (TextBox)oC;
                oT.Attributes["onchange"] += "dirtyPage(this);";
            } else if (oC is FileUpload) {
                FileUpload oFU = (FileUpload)oC;
                oFU.Attributes["onchange"] += "dirtyPage(this);";
            }
            if (oC.HasControls())
                setUpControls(oC);
        }
    }

    /// <summary>
    /// Returns a split by string instead of a char array
    /// </summary>
    /// <param name="testString"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string[] SplitByString(string testString, string split) {
        int offset = 0;
        int index = 0;
        int[] offsets = new int[testString.Length + 1];

        while (index < testString.Length) {
            int indexOf = testString.IndexOf(split, index);
            if (indexOf != -1) {
                offsets[offset++] = indexOf;
                index = (indexOf + split.Length);
            } else {
                index = testString.Length;
            }
        }

        string[] final = new string[offset + 1];
        if (offset == 0) {
            final[0] = testString;
        } else {
            offset--;
            final[0] = testString.Substring(0, offsets[0]);
            for (int i = 0; i < offset; i++) {
                final[i + 1] = testString.Substring(offsets[i] + split.Length, offsets[i + 1] - offsets[i] - split.Length);
            }
            final[offset + 1] = testString.Substring(offsets[offset] + split.Length);
        }
        return final;
    }

    public static string drawCustomTextArea(int intWidth, int intHeight, string szValue, string szAppRoot) {
        string szHTML = "";
        string szUnit = "%";
        string szParams = "expand, ordered-list, unordered-list, separator, draw-layout-table, image, hyperlink, undo, source ";

        szHTML = String.Format(@"
            <object type='application/x-xstandard' id='oXHTML' width='{0}{1}' height='{2}'
            codebase='{3}/XStandard/download/XStandard.cab#Version=2,0,0,0'>
        	    <param name='Value' value=""{5}"" />
        	    <param name='CSS' value='{3}/XStandard/Xstandard.css?1'/>
        	    <param name='Styles' value='{3}/XStandard/styles.xml?3' />
        	    <param name='base' value='{3}/XStandard' />
                <param name='ToolbarWysiwyg' value='{4}' />
        	    <param name='EnablePasteMarkup' value='yes' />
                <param name='ExpandHeight' value='75%' />
                <param name='ExpandWidth' value='75%' />
                <param name='EnableCache' value='yes' />
                <param name='EnableTimestamp' value='no' />
                <param name='Options' value='10' />
                <param name='ClassImageFloatLeft' value='ImgLeft' />
                <param name='ClassImageFloatRight' value='ImgRight' />
            </object>
            <input type=hidden name='szHTML'/>
        ", intWidth, szUnit, intHeight, szAppRoot, szParams, HttpContext.Current.Server.HtmlEncode(szValue));
        return szHTML;
    }

    /// <summary>
    /// Appends the value to the passes in string, utilizing the split character if the string has data in it.
    /// </summary>
    /// <param name="Original"></param>
    /// <param name="AppendText"></param>
    /// <param name="SplitText"></param>
    public static void Append(ref string Original, string AppendText, string SplitText) {
        if (Original == "")
            Original = AppendText;
        else
            Original = Original + SplitText + AppendText;
    }

    public static string Append(string szOriginal, string szValue, string szSplitChar) {
        if (szOriginal == "")
            return szValue;
        else
            return szOriginal + szSplitChar + szValue;
    }

    public static string ControlToString(WebControl oControl) {
        StringBuilder sb = new StringBuilder();
        using (StringWriter sw = new StringWriter(sb)) {
            using (HtmlTextWriter textWriter = new HtmlTextWriter(sw)) {
                oControl.RenderControl(textWriter);
            }
        }
        return sb.ToString();
    }

    public static DataSet RemoveDuplicates(DataSet ds, DataColumn[] keyColumns) {
        DataTable tbl = ds.Tables[0];
        int rowNdx = 0;
        while (rowNdx < tbl.Rows.Count - 1) {
            DataRow[] dups = FindDups(tbl, rowNdx, keyColumns);
            if (dups.Length > 0) {
                foreach (DataRow dup in dups) {
                    tbl.Rows.Remove(dup);
                }
            } else {
                rowNdx++;
            }
        }
        return ds;
    }

    private static DataRow[] FindDups(DataTable tbl, int sourceNdx, DataColumn[] keyColumns) {
        ArrayList retVal = new ArrayList();
        DataRow sourceRow = tbl.Rows[sourceNdx];
        for (int i = sourceNdx + 1; i < tbl.Rows.Count; i++) {
            DataRow targetRow = tbl.Rows[i];
            if (IsDup(sourceRow, targetRow, keyColumns)) {
                retVal.Add(targetRow);
            }
        }
        return (DataRow[])retVal.ToArray(typeof(DataRow));
    }

    private static bool IsDup(DataRow sourceRow, DataRow targetRow, DataColumn[] keyColumns) {
        bool retVal = true;
        foreach (DataColumn column in keyColumns) {
            retVal = retVal && sourceRow[column].Equals(targetRow[column]);
            if (!retVal) break;
        }
        return retVal;
    }

    public static bool IsInteger(string inputData) {
        int intOut = 0;
        return int.TryParse(inputData, out intOut);
    }

    public static bool IsNumeric(string inputData) {
        double dOut = 0;
        return Double.TryParse(inputData, out dOut);
    }

    /// <summary>
    /// Adds value to arrayList and checks if it already exists
    /// </summary>
    /// <param name="alList">ArrayList by ref</param>
    /// <param name="szValue">Object oValue (pass string, int or objects)</param>
    public static void AddToArrayList(ref ArrayList alList, object oValue) {
        if (!alList.Contains(oValue)) {
            alList.Add(oValue);
        }
    }

    /// <summary>
    /// Accepts a arraylist and returns a CSV string (e.g. 2,3,5,8)
    /// </summary>
    /// <param name="arList">ArrayList</param>
    /// <returns>Comma separated string</returns>
    public static string ArrayListToString(ArrayList arList) {
        string szValue = "";
        if (arList != null)
            foreach (string szList in arList) {
                Utility.Append(ref szValue, szList, ",");
            }
        return szValue;
    }

    public static void echo(string szEcho) {
        HttpContext.Current.Response.Write(szEcho);
        HttpContext.Current.Response.Flush();
    }

    public static void die(string szEcho) {
        echo(szEcho);
        HttpContext.Current.Response.End();
    }
}

#region DateRange

//Contains a range of dates
public class DateRange {
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    /// <summary>
    /// The range formatted into a string
    /// </summary>
    public string DateRangeString {
        get {
            return Utility.formatDate(Start) + ":" + Utility.formatDate(End);
        }
    }
}

public static class DateUtil {

    public static int QuarterNumber(this DateTime dateTime) {
        return Convert.ToInt32((dateTime.Month - 1) / 3) + 1;
    }

    public static DateRange ThisYear(DateTime date) {
        DateRange range = new DateRange();

        range.Start = new DateTime(date.Year, 1, 1);
        range.End = range.Start.AddYears(1).AddSeconds(-1);

        return range;
    }

    public static DateRange ThisFinYear(DateTime date) {
        DateRange range = new DateRange();
        int intYear = date.Year;
        if (date.Month <= 6)
            intYear--;
        range.Start = new DateTime(intYear, 7, 1);
        range.End = range.Start.AddYears(1).AddSeconds(-1);

        return range;
    }

    public static DateRange LastYear(DateTime date) {
        DateRange range = new DateRange();
        int intYear = date.Year - 1;
        if (date.Month <= 6)
            intYear--;
        range.Start = new DateTime(intYear, 1, 1);
        range.End = range.Start.AddYears(1).AddSeconds(-1);

        return range;
    }

    public static DateRange LastFinYear(DateTime date) {
        DateRange range = new DateRange();
        int intYear = date.Year - 1;
        if (date.Month <= 6)
            intYear--;
        range.Start = new DateTime(intYear, 7, 1);
        range.End = range.Start.AddYears(1).AddSeconds(-1);

        return range;
    }

    public static DateRange ThisMonth(DateTime date) {
        DateRange range = new DateRange();

        range.Start = new DateTime(date.Year, date.Month, 1);
        range.End = range.Start.AddMonths(1).AddSeconds(-1);

        return range;
    }

    public static DateRange LastQuarter(DateTime date) {
        DateRange range = new DateRange();
        int currentQuarter = QuarterNumber(date) - 1;
        int intYear = date.Year;
        if (currentQuarter == 0) {
            intYear -= 1;
            currentQuarter = 4;
        }
        range.Start = new DateTime(intYear, 3 * currentQuarter - 2, 1);
        range.End = range.Start.AddMonths(3).AddSeconds(-1);
        return range;
    }

    public static DateRange ThisQuarter(DateTime date) {
        DateRange range = new DateRange();
        int currentQuarter = QuarterNumber(date);
        range.Start = new DateTime(date.Year, 3 * currentQuarter - 2, 1);
        range.End = range.Start.AddMonths(3).AddSeconds(-1);
        return range;
    }

    public static DateRange LastMonth(DateTime date) {
        DateRange range = new DateRange();

        range.Start = (new DateTime(date.Year, date.Month, 1)).AddMonths(-1);
        range.End = range.Start.AddMonths(1).AddSeconds(-1);

        return range;
    }

    public static DateRange ThisWeek(DateTime date) {
        DateRange range = new DateRange();

        range.Start = date.Date.AddDays(-(int)date.DayOfWeek);
        range.End = range.Start.AddDays(7).AddSeconds(-1);

        return range;
    }

    public static DateRange LastWeek(DateTime date) {
        DateRange range = ThisWeek(date);

        range.Start = range.Start.AddDays(-7);
        range.End = range.End.AddDays(-7);

        return range;
    }
}

#endregion DateRange

/// <summary>
/// Allows the comparison of file entries by their modified date
/// </summary>
public class CompareFileInfoDateModified : IComparer {

    public CompareFileInfoDateModified() {
    }

    public int Compare(object f1, object f2) {
        return DateTime.Compare(((FileInfo)f2).LastWriteTime, ((FileInfo)f1).LastWriteTime);
    }
}

public class ExtendedHashTable : Hashtable {

    public void checkDupAndAdd(object key, object value) {
        if (!Contains(key))
            Add(key, value);
    }
}

public class CsvWriter {

    public static string WriteToString(DataTable table, bool header, bool quoteall) {
        StringWriter writer = new StringWriter();
        WriteToStream(writer, table, header, quoteall);
        return writer.ToString();
    }

    public static void WriteToStream(TextWriter stream, DataTable table, bool header, bool quoteall) {
        if (header) {
            for (int i = 0; i < table.Columns.Count; i++) {
                WriteItem(stream, table.Columns[i].Caption, quoteall);
                if (i < table.Columns.Count - 1)
                    stream.Write(',');
                else
                    stream.Write(Environment.NewLine);
            }
        }
        int RowCount = 0;
        foreach (DataRow row in table.Rows) {
            RowCount++;
            for (int i = 0; i < table.Columns.Count; i++) {
                WriteItem(stream, row[i], quoteall);
                if (i < table.Columns.Count - 1)
                    stream.Write(',');
                else
                    stream.Write(Environment.NewLine);
            }
            if(RowCount % 2 == 0)
                stream.Write(Environment.NewLine); //MYOB needs transactions split with a newline
        }
    }

    private static void WriteItem(TextWriter stream, object item, bool quoteall) {
        if (item == null)
            return;
        string s = item.ToString();
        if (quoteall || s.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)
            stream.Write("\"" + s.Replace("\"", "\"\"") + "\"");
        else
            stream.Write(s);
    }
}

public class PayPeriod : IComparable<PayPeriod> {
    public int ID = 0;
    public DateTime StartDate = DateTime.MinValue;
    public DateTime EndDate = DateTime.MaxValue;
    public int FinYear = 0;

    public PayPeriod(int ID, DateTime StartDate, DateTime EndDate) {
        this.ID = ID;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        FinYear = DateUtil.ThisFinYear(EndDate).Start.Year;
    }

    #region IComparable<Relationship> Members

    public int CompareTo(PayPeriod other) {
        return this.StartDate.CompareTo(other.StartDate);
    }

    public static Comparison<PayPeriod> IDComparison =
      delegate (PayPeriod p1, PayPeriod p2) {
          return p1.ID.CompareTo(p2.ID);
      };

    #endregion IComparable<Relationship> Members
}

public enum APISource {
    BoxDice = 0,
    CampaignTrack = 1,
    RPData = 2
}

public class APILog {
    public int intDBID = 0;
    public DateTime RunDate = DateTime.MinValue;
    public APISource oSource = APISource.BoxDice;

    public APILog(int DBID, DateTime StartDate) {
        intDBID = DBID;
        this.RunDate = StartDate;
    }

    /// <summary>
    /// Adds a new log item
    /// </summary>
    /// <param name="oSource"></param>
    /// <param name="Message"></param>
    public static void addLog(APISource oSource, string Message) {
        sqlUpdate oSQL = new sqlUpdate("APILOG", "ID", -1);
        oSQL.add("APISOURCEID", (int)oSource);
        oSQL.add("LOG", Message.Replace("'", "''").Trim());
        oSQL.add("TIMESTAMP", Utility.formatDateTime(DateTime.Now));
        DB.runNonQuery(oSQL.createInsertSQL(), DB.BoxDiceDBConn);
    }
}

#region AppConfigAdmin

/// <summary>
/// Manages the updating of application configuration varibales for the admin pages
/// </summary>
public class AppConfigAdmin {
    public OrderedDictionary htSettings = new OrderedDictionary();
    private bool blnLoadedFromDB = false;
    private String ConnectionString = null;

    public AppConfigAdmin(String ConnectionString = null) {
        this.ConnectionString = ConnectionString;
    }

    /// <summary>
    /// Return the HTML to display the help icon
    /// </summary>
    /// <param name="HelpName"></param>
    public static string getHelpIcon(string HelpName) {
        return "<img src='../sys_images/help.gif' align='right' ondblclick='copyHelp(\"" + HelpName + "\");' onmouseover='showHelp(\"" + HelpName + "\")' onmouseout='parent.doHideHelp()' style='float: right'>";
    }

    /// <summary>
    /// Adds a config value to the list. This must be done to load the values from the DB
    /// </summary>
    /// <param name="ConfigName"></param>
    /// <param name="DefaultValue"></param>
    /// <param name="Label">Can be used in scenarios where loading the values dynamically is preferred. Otherwise this can be left empty</param>
    public void addConfig(string ConfigName, string DefaultValue, string Label = "") {
        if (!blnKeyExists(ConfigName))
            htSettings[ConfigName] = new AppConfigValue(ConfigName, "", DefaultValue, Label);
    }

    private bool blnKeyExists(string szKey) {
        return htSettings[szKey] != null;
    }

    /// <summary>
    /// Returns the value from the config settings
    /// </summary>
    /// <param name="ConfigName"></param>
    /// <returns></returns>
    public string getValue(string ConfigName) {
        if (!blnLoadedFromDB) {
            ;// "You have not read the values from the database. Call  oConfigAdmin.loadValuesFromDB() before reading values.";
        }
        if (blnKeyExists(ConfigName)) {
            AppConfigValue oV = ((AppConfigValue)htSettings[ConfigName]);
            return oV.ActualValue;
        }
        return "";
    }

    /// <summary>
    /// Sets the value of the config setting
    /// </summary>
    /// <param name="ConfigName"></param>
    /// <param name="Value"></param>
    public void setValue(string ConfigName, string Value) {
        if (blnKeyExists(ConfigName)) {
            ((AppConfigValue)htSettings[ConfigName]).ActualValue = Value;
        }
    }

    /// <summary>
    /// Loads the values of the items in the htSettings list from the DB.
    /// If the value is blank or doesn't exist in the DB, the actual value is set to the default value
    /// </summary>
    public void loadValuesFromDB() {
        blnLoadedFromDB = true;
        foreach (string key in htSettings.Keys) {
            AppConfigValue oCV = (AppConfigValue)htSettings[key];
            string szSQL = String.Format("SELECT VALUE FROM CONFIG WHERE NAME = '{0}'", oCV.Name);

            oCV.ActualValue = DB.getScalar(szSQL, oCV.DefaultValue, ConnectionString);
        }
    }

    /// <summary>
    /// Updates the DB and the Session table
    /// </summary>
    /// <param name="ConfigName"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    public void updateConfigValueSQL(string ConfigName, string Value) {
        string szSQL = "SELECT COUNT(ID) FROM CONFIG WHERE NAME = '" + ConfigName + "' AND USERID = 0";
        if ((int)DB.getScalar(szSQL, -1, ConnectionString) <= 0) {
            szSQL = String.Format(@"
                INSERT INTO CONFIG(NAME, VALUE, USERID)
                VALUES('{0}', '{1}', 0)", ConfigName, Utility.formatForDB(Value));
            if (ConnectionString == null) {
                szSQL = String.Format(@"
                INSERT INTO CONFIG(NAME, VALUE, USERID, LABLOCATION)
                VALUES('{0}', '{1}', 0, 0)", ConfigName, Utility.formatForDB(Value));
            }
        } else {
            szSQL = String.Format(@"
		        UPDATE CONFIG
                SET VALUE = '{1}' WHERE
                NAME = '{0}' AND USERID = 0
                ", ConfigName, Utility.formatForDB(Value));
        }
        setValue(ConfigName, Value);
        HttpContext.Current.Session[ConfigName] = Value;
        DB.runNonQuery(szSQL, ConnectionString);
    }
}

#endregion AppConfigAdmin

#region AppConfigValue

/// <summary>
/// A data class that stores a single application config value
/// </summary>
public class AppConfigValue {
    private string szName = "";
    private string szActualValue = "";
    private string szDefaultValue = "";
    private string szLabel = "";

    public string Name {
        get { return szName; }
        set { szName = value; }
    }

    public string ActualValue {
        get { return szActualValue; }
        set { szActualValue = value; }
    }

    public string DefaultValue {
        get { return szDefaultValue; }
        set { szDefaultValue = value; }
    }

    public string Label {
        get { return szLabel; }
        set { szLabel = value; }
    }

    public AppConfigValue(string ConfigName, string ActualValue, string DefaultValue, string Label) {
        szName = ConfigName;
        szActualValue = ActualValue;
        szDefaultValue = DefaultValue;
        szLabel = Label;
    }
}

#endregion AppConfigValue