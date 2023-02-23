using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace ExceptionHandler {

    public class LogException {

        // The email list that the errors will be sent to. If this is empty, the errors will not be logged.
        private string szEmails = "gord.funk@gmail.com, fletcherserrorreporting@gmail.com";

        private string szFrom = "payroll@fletchers.net.au";
        private string szSubject = "Fletcher Paymaker error report";
        private string szSMTPServer = "";
        private string szUserName = "";
        private string szUserEmail = "";
        private string szDetailURL = "";
        private string szReferrer;
        private string szMessage;
        private string szForm;
        private string szQuery;
        private string szDebugSQL;

        private HttpContext ctx = null;

        public LogException() {
            // Set a few defaults
            ctx = HttpContext.Current;
        }

        public void HandleException(Exception ex) {
            string szEmailExtraInfo = ""; // use this variable to gather extra data about the exception. if set it will be included in the error email to RR techs

            if (WebConfigurationManager.AppSettings["LogErrors"] == "false")
                return;
            string[] arMessage = {  "0x80072746.", "Invalid viewstate"};
            foreach (string szMessage in arMessage) {
                if (ex.Message.Contains(szMessage))
                    return; // An error that doesn't affect the app.
            }
            saveToDB(ex);

            if (szEmails.Length > 0) {
                sendEmail(ex, szEmailExtraInfo);
            }
        }

        private void sendEmail(Exception ex, string ExtraInfo) {
            SmtpClient oSMTP = Email.getEmailServer();

            string szEmailHeader = "<html><head>";
            string szEmailBody = String.Empty;

            //' set the css for emails
            szEmailHeader += "<style type=\"text/css\"> ";
            szEmailHeader += "<!--.RWEmailText { font-family: verdana; font-zise: 10px} --> ";
            szEmailHeader += "</style>";

            szEmailHeader += "</head>";
            szEmailHeader += "<body>";

            szReferrer = String.Empty;
            if (ctx.Request.ServerVariables["HTTP_REFERER"] != null) {
                szReferrer = ctx.Request.ServerVariables["HTTP_REFERER"].ToString();
            }
            string szRemoteAddr = String.Empty;
            if (ctx.Request.ServerVariables["REMOTE_ADDR"] != null) {
                szRemoteAddr = ctx.Request.ServerVariables["REMOTE_ADDR"].ToString();
            }
            string logDateTime = DateTime.Now.ToString();
            szMessage = ex.Message;
            if (!String.IsNullOrEmpty(ExtraInfo))
                szMessage += "<br/>Extra info: " + ExtraInfo;

            szEmailBody = "The details of the error are as follows:<br/><br/>";

            szEmailBody += "<table><tr valign='top'><td width='20%'><b>SOURCE: </b></td><td width='80%'>" + ex.Source +
                        "</td></tr><tr valign='top'><td><b>LogDateTime: </b></td><td>" + logDateTime +
                        "</td></tr><tr valign='top'><td><b>MESSAGE: </b></td><td>" + Utility.nl2br(szMessage) +
                        "</td></tr><tr valign='top'><td><b>FORM: </b></td><td>" + Utility.nl2br(szForm) +
                        "</td></tr><tr valign='top'><td><b>QUERYSTRING: </b></td><td>" + szQuery +
                        "</td></tr><tr valign='top'><td><b>TARGETSITE: </b></td><td>" + Utility.nl2br(Convert.ToString(ex.TargetSite)) +
                         "</td></tr><tr valign='top'><td><b>STACKTRACE: </b></td><td>" + Utility.nl2br(ex.StackTrace) +
                        "</td></tr><tr valign='top'><td><b>REFERER: </b></td><td>" + szReferrer;

            if (HttpContext.Current.Session != null && HttpContext.Current.Session["USEREMAIL"] != null)
                szEmailBody += String.Format(@"</td></tr><tr valign='top'><td><b>User email: </b></td><td>{0}</td></tr>", HttpContext.Current.Session["USEREMAIL"].ToString());

            szEmailBody += "</td></tr><tr valign='top'><td><b>Browser (from server-side): </b></td><td>" + HttpContext.Current.Request.Browser.Type;
            szEmailBody += "</td></tr><tr valign='top'><td><b>IP address: </b></td><td>" + szRemoteAddr + "</td></tr></table>";

            szEmailBody += "<br/><br/><a href='" + szDetailURL + "'>View error log</a>";
            MailMessage msg = new MailMessage();
            msg.IsBodyHtml = true;
            msg.To.Add(szEmails.Replace(';', ','));

            msg.From = new MailAddress(EmailSettings.SMTPServerUserName);
            msg.Subject = szSubject;
            msg.Body = szEmailHeader + szEmailBody + "</body></html>";
            try {
                oSMTP.Send(msg);
            } catch (Exception excm) {
                Debug.WriteLine(excm.Message);
                throw;
            } finally {
                oSMTP = null;
            }
        }

        private void saveToDB(Exception ex) {
            sqlUpdate oSQL = new sqlUpdate("RWASPERRORLOG", "EVENTID", -1);
            string szSource = Convert.ToString(ex.Source);
            oSQL.add("SOURCE", szSource);
            oSQL.add("MESSAGE", szMessage);
            oSQL.add("FORM", szForm);
          
            oSQL.add("QUERYSTRING", szQuery);
            string szTargetSite = Convert.ToString(ex.TargetSite);
            oSQL.add("TARGETSITE", szTargetSite);
            oSQL.add("STACKTRACE", Convert.ToString(ex.StackTrace));
            oSQL.add("REFERER", szReferrer);
            if (HttpContext.Current.Session != null && HttpContext.Current.Session["USERID"] != null)
                oSQL.add("USERID", Convert.ToInt32(HttpContext.Current.Session["USERID"]));
            DB.runNonQuery(oSQL.createInsertSQL());
        }
    }
}

/// <summary>
/// An exception type that halts execution and passes the information to the user regardless of whether they are a RR user
/// </summary>
public class InformationException : Exception {

    public InformationException(string Message)
        : base(Message) {
        this.Data.Add("ExceptionType", "InformationException");
    }

    public InformationException(Exception ex) : base(ex.Message, ex.InnerException) {
    }
}