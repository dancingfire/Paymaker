using System;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Functions  that are used throughout the application
/// </summary>
///

public enum VT {
    TextNormal = 0,
    NoValidation = 2,
    Integer = 3,
    AlphaNumericNoPunctuation = 4,
    List = 5
}

public enum RequestType {
    Post = 0,
    Get = 1
}

public class Valid {

    public static bool isSet(object o) {
        return !(o == null);
    }

    /// <summary>
    /// Returns true if the number is numeric (will accept decimal places)
    /// </summary>
    /// <param name="szValue"></param>
    /// <returns></returns>
    public static bool isNumeric(string szValue) {
        float fTest = 0;
        return float.TryParse(szValue, out fTest);
    }

    /// <summary>
    /// Returns true if the string is a valid date time
    /// </summary>
    /// <param name="szValue"></param>
    /// <returns></returns>
    public static bool isDateTime(string szValue) {
        DateTime dtTest;
        return DateTime.TryParse(szValue, out dtTest);
    }

    public static string ifNull(object o, string szDefault) {
        if (o == null)
            return szDefault;
        else
            return Convert.ToString(o);
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <param name="AllowBlank">If this is true then the default will be used if the value is not set</param>
    /// <returns></returns>
    private static int getInteger(string VariableName, int Default, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError("Value not an integer: " + szPostedValue);
        }

        return validateInteger(szPostedValue);
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static int getInteger(string VariableName) {
        return getInteger(VariableName, -1, false);
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static int getInteger(string VariableName, int Default) {
        return getInteger(VariableName, Default, true);
    }

    public static bool isInteger(string szValue) {
        int intTest = 0;
        return int.TryParse(szValue, out intTest);
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static long getLong(string VariableName) {
        return getLong(VariableName, -1, false);
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <param name="AllowBlank">If this is true then the default will be used if the value is not set</param>
    /// <returns></returns>
    private static long getLong(string VariableName, long Default, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError("Value not an integer: " + szPostedValue);
        }

        return validateLong(szPostedValue);
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <param name="AllowBlank">If this is true then the default will be used if the value is not set</param>
    /// <returns></returns>
    public static double getMoney(string VariableName, double Default, bool AllowBlank = true) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError(VariableName + " must have a value.");
        }
        if (isNumeric(szPostedValue))
            return Convert.ToDouble(szPostedValue);
        else {
            showValidationError("Value not an valid number: " + szPostedValue + " Form field:" + VariableName);
            return 0.0;
        }
    }

    /// <summary>
    /// Checks first the querystring and then the form for the variable
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static long getLong(string VariableName, long Default) {
        return getLong(VariableName, Default, true);
    }

    public static bool isLong(string szValue) {
        long intTest = 0;
        return long.TryParse(szValue, out intTest);
    }

    /// <summary>
    /// Gets the list value from the posted date.
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <param name="AllowBlank"></param>
    /// <returns></returns>
    private static string getList(string VariableName, string Default, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError("List is in incorrect format:" + szPostedValue);
        }

        validateList(szPostedValue);
        return szPostedValue;
    }

    /// <summary>
    /// Gets the list value from the posted one. Will return the default if its not set
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static string getList(string VariableName, string Default) {
        return getList(VariableName, Default, true);
    }

    /// <summary>
    /// Gets the list value from the posted one. The list value is required
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static string getList(string VariableName) {
        return getList(VariableName, "", false);
    }

    /// <summary>
    /// Makes sure this is a valid date.  This should only be used to validate data from the
    /// client, cause it will shut down the page if it fails.
    /// </summary>
    /// <param name="DateValue"></param>
    /// <param name="oFilter"></param>
    /// <returns></returns>
    private static string getTime(string VariableName, string Default, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError("Time is in incorrect format:" + szPostedValue);
        }

        Match m = Regex.Match(szPostedValue, "[\\w:]+");
        if (!m.Success) {
            showValidationError("Time error:" + szPostedValue);
        }

        return szPostedValue; ;
    }

    /// <summary>
    /// Gets the time value from the posted one. If not set the default value will be returned.
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static string getTime(string VariableName, string Default) {
        return getTime(VariableName, Default, true);
    }

    /// <summary>
    /// Gets the time value from the posted one. The value is required
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns></returns>
    public static string getTime(string VariableName) {
        return getTime(VariableName, "", false);
    }

    /// <summary>
    /// Makes sure this is a valid GUID.  This should only be used to validate data from the
    /// client, cause it will shut down the page if it fails.
    /// </summary>
    /// <param name="DateValue"></param>
    /// <param name="oFilter"></param>
    /// <returns></returns>
    private static string getGUID(string VariableName, string Default, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError("GUID cannot be blank:" + szPostedValue);
        }
        if (!isValidGUID(szPostedValue)) {
            showValidationError("GUID is in an incorrect format:" + szPostedValue);
        }

        return szPostedValue;
    }

    /// <summary>
    /// Gets the time value from the posted one. If not set the default value will be returned.
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static string getGUID(string VariableName, string Default) {
        return getGUID(VariableName, Default, true);
    }

    /// <summary>
    /// Gets the GUID value from the posted one. The value is required
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns></returns>
    public static string getGUID(string VariableName) {
        return getGUID(VariableName, "", false);
    }

    /// <summary>
    /// Verifies that the string is 36 chars long and has only the apporiate charcters
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static bool isValidGUID(string Value) {
        Value = Value.Trim();
        Match m = Regex.Match(Value, "^([\\d\\-\\w]{36})$");
        return (m.Success);
    }

    /// <summary>
    /// Makes sure this is a valid date.  This should only be used to validate data from the
    /// client, cause it will shut down the page if it fails.
    /// </summary>
    /// <param name="DateValue"></param>
    /// <param name="oFilter"></param>
    /// <returns></returns>
    private static DateTime getDate(string VariableName, DateTime dtDefault, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return dtDefault;
            else
                showValidationError("Date is in incorrect format:" + szPostedValue);
        }

        if (!Utility.isDateTime(szPostedValue)) {
            showValidationError("Date field error. Value attempted: " + szPostedValue);
        }
        return Convert.ToDateTime(szPostedValue);
    }

    /// <summary>
    /// Checks a date and returns it if valid.
    /// </summary>
    /// <param name="TestDate"></param>
    /// <param name="dtDefault"></param>
    /// <param name="AllowBlank"></param>
    /// <returns></returns>
    public static DateTime checkDate(string TestDate, DateTime dtDefault, bool AllowBlank = true) {
        string szPostedValue = TestDate;
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return dtDefault;
            else
                showValidationError("Date is in incorrect format:" + szPostedValue);
        }

        if (!Utility.isDateTime(szPostedValue)) {
            showValidationError("Date field error. Value attempted: " + szPostedValue);
        }
        return Convert.ToDateTime(szPostedValue);
    }

    /// <summary>
    /// Gets the date value from the posted one. If not set the default value will be returned.
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <returns></returns>
    public static DateTime getDate(string VariableName, DateTime Default) {
        return getDate(VariableName, Default, true);
    }

    /// <summary>
    /// Gets the date value from the posted one. The list value is required
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns></returns>
    public static DateTime getDate(string VariableName) {
        return getDate(VariableName, DateTime.MaxValue, false);
    }

    /// <summary>
    /// Gets the posted variable value
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns>Returns null if neither the form not querystring contains the value</returns>
    private static string getPostedValue(string VariableName) {
        HttpContext oCtx = HttpContext.Current;

        string szPostedValue = null;
        //First get the type of the request that we are dealing with
        if (Utility.isSet(oCtx.Request.QueryString[VariableName])) {
            szPostedValue = oCtx.Request.QueryString[VariableName];
        } else if (Utility.isSet(oCtx.Request.Form[VariableName])) {
            szPostedValue = oCtx.Request.Form[VariableName];
        }
        return szPostedValue;
    }

    /// <summary>
    /// Makes sure this is a valid text entry for the specified type.  This should only be used to validate data from the
    /// client, cause it will shut down the page if it fails.
    /// </summary>
    /// <param name="DateValue"></param>
    /// <param name="oFilter"></param>
    /// <returns></returns>
    private static string getText(string VariableName, string Default, VT ValidationType, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return Default;
            else
                showValidationError("Text is in incorrect format:" + szPostedValue);
        }

        return validateText(szPostedValue, ValidationType);
    }

    /// <summary>
    /// Gets the text value from the posted data. If not set the default value will be returned.
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <param name="ValidationType"></param>
    /// <returns></returns>
    public static string getText(string VariableName, string Default, VT ValidationType) {
        return getText(VariableName, Default, ValidationType, true);
    }

    /// <summary>
    /// Gets the text value from the posted data. If not set the default value will be returned.
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="Default"></param>
    /// <param name="ValidationType"></param>
    /// <returns></returns>
    public static string getText(string VariableName, string Default) {
        return getText(VariableName, Default, VT.NoValidation, true);
    }

    /// <summary>
    /// Gets the text value from the posted data. The value is required
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns></returns>
    public static string getText(string VariableName, VT ValidationType) {
        return getText(VariableName, "", ValidationType, false);
    }

    /// <summary>
    /// Gets the checked from the posted one. Returns
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns></returns>
    public static bool getCheck(string VariableName) {
        string szPostedValue = getPostedValue(VariableName);
        return !String.IsNullOrEmpty(szPostedValue);
    }

    /// <summary>
    /// Gets the boolean value from the posted one. Returns a default value if the posted value is not set
    /// </summary>
    /// <param name="VariableName"></param>
    /// <returns></returns>
    public static bool getBoolean(string VariableName, bool Default) {
        string szPostedValue = getPostedValue(VariableName);
        HttpContext oCtx = HttpContext.Current;
        bool blnValueIsBlank = (null == szPostedValue);

        if (blnValueIsBlank)
            return Default;
        return szPostedValue.ToLower() == "true" || szPostedValue == "1";
    }

    /// <summary>
    /// Validates the text for the passed in type
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="oValidationType"></param>
    /// <returns></returns>
    static private string validateText(string Value, VT oValidationType) {
        string szTestValue = Value.ToLower();
        switch (oValidationType) {
            case VT.TextNormal:
                /*if (szTestValue.IndexOf("%3cscript%3") > -1 || szTestValue.IndexOf("&lt;script&gt;") > -1 || szTestValue.IndexOf("<script>") > -1 || szTestValue.IndexOf("<img ") > -1 || szTestValue.IndexOf(" src=") > -1) {
                    showValidationError("TextNormal error: Possible XSS attack:" + Utility.EscapeHTML(szTestValue));
                }*/
                break;

            case VT.AlphaNumericNoPunctuation:
                Match m = Regex.Match(Value, "[\\w]+");
                if (!m.Success) {
                    showValidationError("TextNumbersNoPunctuation error:" + Value);
                }
                break;
        }
        return Value;
    }

    /// <summary>
    /// Validates that the text is an integer. This function will stop the response if the number is not an integer
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    static private int validateInteger(string Value) {
        if (!Valid.isInteger(Value)) {
            showValidationError("Validation failed for numeric data: " + Value);
        }

        return Convert.ToInt32(Value);
    }

    /// <summary>
    /// Validates that the text is an long. This function will stop the response if the number is not an long
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    static private long validateLong(string Value) {
        if (!Valid.isLong(Value)) {
            showValidationError("Validation failed for numeric data: " + Value);
        }

        return Convert.ToInt64(Value);
    }

    /// <summary>
    /// Validates that the text is an comma seperated list. This function will stop the response if the number is not a proper list
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    static private void validateList(string Value) {
        Value = Utility.RemoveWhitespace(Value);
        Match m = Regex.Match(Value, "^(([-]?[\\d])+[,]?)+$");
        if (!m.Success) {
            showValidationError("ID list error:" + Value);
        }

        return;
    }

    /// <summary>
    /// Returns a default value is the original value is empty
    /// </summary>
    /// <param name="szVariable"></param>
    /// <param name="szType"></param>
    /// <param name="szDefault"></param>
    /// <returns></returns>
    public static string isBlank(string OrigValue, string szDefault) {
        if (OrigValue == "")
            OrigValue = szDefault;
        return OrigValue;
    }

    public static object isBlank(string szValue) {
        if (szValue != null && szValue.Trim() != "")
            return szValue;
        else
            return null;
    }

    /// <summary>
    /// The error message that prints out when there's been a validation error.
    /// This means that either there is no javascript on the client's machine or the app is actually under attack.
    /// </summary>
    public static void showValidationError() {
        showValidationError("Page.IsValid check failed.");
    }

    /// <summary>
    /// The error message that prints out when there's been a validation error.
    /// This means that either there is no javascript on the client's machine or the app is actually under attack.
    /// </summary>
    public static void showValidationError(string Details) {
        string szCurrScript = HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"];

        HttpContext ctx = HttpContext.Current;
        string szEmailHeader = "<html><head>";
        string szEmailBody = String.Empty;
        //' set the css for emails
        szEmailHeader += "<style type=\"text/css\"> ";
        szEmailHeader += "<!--.RWEmailText { font-family: verdana; font-zise: 10px} --> ";
        szEmailHeader += "</style>";

        szEmailHeader += "</head>";
        szEmailHeader += "<body>";

        string szReferrer = String.Empty;
        if (ctx.Request.ServerVariables["HTTP_REFERER"] != null) {
            szReferrer = ctx.Request.ServerVariables["HTTP_REFERER"].ToString();
        }
        string szRemoteAddr = String.Empty;
        if (ctx.Request.ServerVariables["REMOTE_ADDR"] != null) {
            szRemoteAddr = ctx.Request.ServerVariables["REMOTE_ADDR"].ToString();
        }
        string szForm = (ctx.Request.Form != null) ? ctx.Request.Form.ToString() : String.Empty;
        szForm = szForm.Replace("?", "<br/>");
        szForm = szForm.Replace("&", "<br/>");
        szForm = HttpContext.Current.Server.UrlDecode(szForm);
        string logDateTime = DateTime.Now.ToString();
        string szQuery = (ctx.Request.QueryString != null) ? ctx.Request.QueryString.ToString() : String.Empty;
        szQuery = szQuery.Replace("?", "<br/>");
        szQuery = szQuery.Replace("&", "<br/>");

        szEmailBody = "A validation error has occured. ";

        szEmailBody += String.Format(@"
                <table><tr valign='top'>
                    <td width='20%'><b>Details: </b></td><td width='80%'>{0}</td></tr>
                        <tr valign='top'><td><b>LogDateTime: </b></td><td>{1}</td></tr>
                        <tr valign='top'><td><b>Page: </b></td><td>{6}</td></tr>
                        <tr valign='top'><td><b>FORM: </b></td><td>{2}</td></tr>
                        <tr valign='top'><td><b>QUERYSTRING: </b></td><td>{3}</td></tr>
                        <tr valign='top'><td><b>REFERER: </b></td><td>{4}</td></tr>
                        <tr valign='top'><td><b>IP ADDRESS: </b></td><td>{5}</td></tr>
                    ", Details, logDateTime, Utility.nl2br(szForm), szQuery, szReferrer, szRemoteAddr, szCurrScript);

        szEmailBody += "</table><br/><br/>";
    }

    public static DateTime checkNullDate(object oDate) {
        if (oDate == System.DBNull.Value)
            return DateTime.MinValue;
        else
            return Convert.ToDateTime(oDate);
    }

    /// <summary>
    /// Validates whether the client's response is a correct email address
    /// </summary>
    /// <param name="VariableName"></param>
    /// <param name="AllowBlank"></param>
    /// <returns></returns>
    public static string getEmail(string VariableName, bool AllowBlank) {
        string szPostedValue = getPostedValue(VariableName);
        bool blnValueIsBlank = String.IsNullOrEmpty(szPostedValue);

        // Check for the blank case
        if (blnValueIsBlank) {
            if (AllowBlank)
                return "";
            else
                showValidationError("Email address is incorrect:" + szPostedValue);
        }
        if (!isValidEmail(szPostedValue, 4)) {
            showValidationError("Email address is incorrect:" + szPostedValue);
        }
        return szPostedValue;
    }

    /// <summary>
    /// returns whether the email address is valid or not
    /// </summary>
    /// <param name="szEmail"></param>
    /// <param name="MinLength"></param>
    /// <returns></returns>
    public static bool isValidEmail(string szEmail, int MinLength) {
        bool blnIsValid = true;
        if (szEmail == null)
            return false;
        szEmail = szEmail.Trim();
        if (MinLength == 0 && szEmail.Length == 0)
            return true;
        else {
            string[] arrEmail;
            if (szEmail.IndexOf(",") > -1)
                arrEmail = szEmail.Split(',');
            else
                arrEmail = szEmail.Split(';');
            Regex re = new Regex(@"^(([^<>()[\]\\.,;:\s@\""]+"
                                + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                                + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                                + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                                + @"[a-zA-Z]{2,}))$");
            foreach (string szEmailAddress in arrEmail) {
                if (!re.IsMatch(szEmailAddress)) {
                    blnIsValid = false;
                    break;
                }
            }
        }
        return blnIsValid;
    }
}