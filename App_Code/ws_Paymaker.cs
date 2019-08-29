using System;
using System.Data;
using System.Text;
using System.Web.Script.Services;
using System.Web.Services;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class ws_Paymaker : System.Web.Services.WebService {
    public ws_Paymaker() : base() {
        /* SECURITY : DO NOT REMOVE
        * This code checks if user is logged in.  
        * This protects WebServices from being accessed by users not logged in. */
        int a = G.User.UserID;
    }

    [WebMethod(EnableSession = true)]
    public void updateActionReminderDate(string ReminderDate, int CampaignNoteID) {
        ReminderDate = Microsoft.JScript.GlobalObject.unescape(ReminderDate);
        string szSQL = String.Format(@"UPDATE CAMPAIGNNOTE SET REMINDER = '{0}' WHERE ID = {1}", ReminderDate, CampaignNoteID);
        if (ReminderDate == "NULL")
            szSQL = "UPDATE CAMPAIGNNOTE SET REMINDER = NULL WHERE ID = " + CampaignNoteID;
        DB.runNonQuery(szSQL);
    }

    [WebMethod(EnableSession = true)]
    public void checkTimesheetCycle() {
        TimesheetCycle.checkTimesheetCycles();
    }

    /// <summary>
    /// Marks the campaign notes as completed
    /// </summary>
    /// <param name="CampaignID"></param>
    [WebMethod(EnableSession = true)]
    public void markActionAsCompleted(int CampaignID) {
        string szSQL = String.Format(@"UPDATE CAMPAIGNNOTE SET ISCOMPLETED = 1 WHERE CAMPAIGNID = {0}", CampaignID);
        DB.runNonQuery(szSQL);
    }

    /// <summary>
    /// Marks the campaign and invoice as exported
    /// </summary>
    /// <param name="CampaignID"></param>
    [WebMethod(EnableSession = true)]
    public void markExportedToMYOB(int InvoiceID, int CampaignID) {
        string szSQL = String.Format(@"
            UPDATE CAMPAIGNINVOICE SET EXPORTID = -1
            WHERE ID = {0}", InvoiceID);
        DB.runNonQuery(szSQL);

        szSQL = String.Format(@"
            UPDATE CAMPAIGN SET ISEXPORTEDTOMYOB = 1
            WHERE ID = {0}", CampaignID);
        DB.runNonQuery(szSQL);
    }

    /// <summary>
    /// Run when supervisor submits staff timesheet
    /// </summary>
    /// <param name="UserID"></param>
    [WebMethod(EnableSession = true)]
    public int submitTimesheet(int UserID, int TimesheetCycleID, bool SupervisorSignOff = false) {
        return Payroll.submitTimesheet(UserID, TimesheetCycleID, SupervisorSignOff);
    }

    [WebMethod(EnableSession = true)]
    public void updateCampaignContributionStatus(int StatusID, int CampaignContributionID) {
        CampaignContribution.updateContributionStatus(CampaignContributionID, (ContributionStatus)StatusID);
    }

    [WebMethod(EnableSession = true)]
    public string getSplitHTML(int CommissionTypeID, int SaleSplitID, int UserSaleSplitCount) {
        //If the UserSplit is new send it with a -ve ID
        return HTML.userSplitHTML(CommissionTypeID, SaleSplitID, true, null, UserSaleSplitCount * -1);
    }

    [WebMethod(EnableSession = true)]
    public string test() {
        // This was used to test if these functions can be accessed without logging in
        return "yep";
    }

    [WebMethod(EnableSession = true)]
    public string getExpenseHTML(int SaleExpenseCount) {
        return SalesExpense.getHTML(false, SaleExpenseCount * -1);
    }

    [WebMethod(EnableSession = true)]
    public string getTemplateHTML(int TemplateID, int CampaignID, int CurrentUserID) {
        G.User.EmailGUID = new Guid().ToString();
        Template oT = new Template(TemplateID);
        oT.fillTemplate(CampaignID, CurrentUserID);
        return oT.FilledTemplate;
    }

    /// <summary>
    /// Returns the account amount, along with the GLcodes for the account and the user
    /// </summary>
    /// <param name="AccountID"></param>
    /// <param name="UserID"></param>
    /// <returns>BudgetAmount***GLCode***AccountJobCode***UserAccount***UserOfficeJobCode</returns>
    [WebMethod(EnableSession = true)]
    public string getUserGLSubAccount(int UserID) {
        UserDetail d = G.UserInfo.getUser(UserID);
        if (d == null)
            return "";
        return d.GLSubAccount;
    }

    /// <summary>
    /// Returns the account amount, along with the GLcodes for the account and the user
    /// </summary>
    /// <param name="AccountID"></param>
    /// <param name="UserID"></param>
    /// <returns>BudgetAmount***GLCode***AccountJobCode***UserAccount***UserOfficeJobCode</returns>
    [WebMethod(EnableSession = true)]
    public string getBudgetAmount(int AccountID, int UserID, bool blnIsExpense) {
        string szSQL = string.Format(@"
            SELECT SUM(FLETCHERCONTRIBTOTAL)
            FROM USERTX
            WHERE ACCOUNTID = {0} AND USERID = {1} AND TXDATE BETWEEN '{2} 00:00:00' AND  '{3} 23:59:59' AND ISDELETED = 0
            ", AccountID, UserID, Utility.formatDate(Utility.getFinYearStart(DateTime.Now)), Utility.formatDate(Utility.getFinYearEnd(DateTime.Now)));
        double dAmountAlreadyCommitted = DB.getScalar(szSQL, 0.0);

        string szAmount = "";

        string szCodes = "";
        if (blnIsExpense) {
            szSQL = "SELECT CREDITGLCODE + '***' + JOBCODE FROM LIST WHERE ID = " + AccountID;
            szCodes = DB.getScalar(szSQL, "");

            szSQL = "SELECT U.CREDITGLCODE + '***' + L.JOBCODE FROM LIST L JOIN DB_USER U ON U.OFFICEID = L.ID WHERE U.ID = " + UserID;
            szCodes += "***" + DB.getScalar(szSQL, "");
            szSQL = string.Format(@"SELECT AMOUNT FROM USERACCOUNT WHERE ACCOUNTID = {0} AND USERID = {1}", AccountID, UserID);
            double dTotalAmount = DB.getScalar(szSQL, 0.0);
            szAmount = Math.Round(dTotalAmount - dAmountAlreadyCommitted, 2).ToString();
        } else {
            szSQL = "SELECT DEBITGLCODE + '***' + JOBCODE FROM LIST WHERE ID = " + AccountID;
            szCodes = DB.getScalar(szSQL, "");

            szSQL = "SELECT U.CREDITGLCODE + '***' + L.JOBCODE FROM LIST L JOIN DB_USER U ON U.OFFICEID = L.ID WHERE U.ID = " + UserID;
            szCodes += "***" + DB.getScalar(szSQL, "");
        }

        return szAmount + "***" + szCodes;
    }

    /// <summary>
    /// Returns the last three months of TXs for the account and the user
    /// </summary>
    /// <param name="AccountID"></param>
    /// <param name="UserID"></param>
    /// <returns>HTML table</returns>
    [WebMethod(EnableSession = true)]
    public string getAccountHistory(int AccountID, int UserID) {
        StringBuilder oSB = new StringBuilder();
        using (DataSet ds = DB.runDataSet(String.Format(@"
            SELECT TX.*
            FROM USERTX TX
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN DB_USER U ON U.ID = TX.USERID AND TX.ISDELETED = 0
            WHERE TX.TXDATE BETWEEN DateAdd(d, -90, getdate()) AND getdate()
            AND TX.UserID = {0} and TX.ACCOUNTID = {1}
            ORDER BY TX.TXDATE DESC", UserID, AccountID))) {
            if (ds.Tables[0].Rows.Count == 0)
                return "There is no data for this account.";

            oSB.AppendFormat("<table><thead><th>Date</th><th>Amount</th><th>Comment</th></thead>");
            foreach (DataRow dr in ds.Tables[0].Rows) {
                oSB.AppendFormat(@"
                    <tr>
                        <td>{0}</td><td>{1}</td><td>{2}</td>
                    </tr>
                    ", DB.readDateString(dr["TXDATE"]), Utility.formatMoney(DB.readDouble(dr["AMOUNT"])), DB.readString(dr["COMMENT"]));
            }
            oSB.AppendFormat("</table>");
        }
        return oSB.ToString();
    }
}