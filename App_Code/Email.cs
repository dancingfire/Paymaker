using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Dapper;

public class EmailSettings {

    /// <summary>
    /// The live email server
    /// </summary>
    public static string SMTPServer {
        get {
            return System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
        }
    }

    /// <summary>
    /// The live email server username
    /// </summary>
    public static string SMTPServerUserName {
        get {
            return System.Configuration.ConfigurationManager.AppSettings["SMTPServerUserName"];
        }
    }

    /// <summary>
    /// The live email server password
    /// </summary>
    public static string SMTPServerPassword {
        get {
            return System.Configuration.ConfigurationManager.AppSettings["SMTPServerPassword"];
        }
    }

    /// <summary>
    /// The live email server password
    /// </summary>
    public static bool SMTPServerSSL{
        get {
            return System.Configuration.ConfigurationManager.AppSettings["SMTPServerUseSSL"] == "TRUE";
        }
    }
    public static string SMTPServerFromEmail {
        get {
            return System.Configuration.ConfigurationManager.AppSettings["SMTPServerFromEmail"];
        }
    }
    
}

/// <summary>
/// Summary description for Email
/// </summary>
public class Email {

    /// <summary>
    /// Create and send an email
    /// </summary>
    /// <param name="To"></param>
    /// <param name="szFrom"></param>
    /// <param name="Subject"></param>
    /// <param name="HTMLBody"></param>
    /// <param name="szCC"></param>
    /// <param name="szBCC"></param>
    /// <param name="SendToUserID">In the case that emails are sent while there is no logged in user eg. Password reset, a user Id needs to be provided to log emails against</param>
    ///
    public static bool sendMail(string To, string szFrom, string Subject, string HTMLBody, string szCC = "", string szBCC = "", Attachment IncludeFile = null, int LogObjectID = -1, EmailType Type = EmailType.General, string DisplayName = "", bool FromQueue = false) {
        
        string szDelegatedEmailAddresses = "";
        MailMessage msg = new MailMessage();
        msg.IsBodyHtml = true;

        //Perform that delegation first as this may clear out the To field 
        if (!FromQueue) {
            List<UserDelegate> lDelegates = G.UserDelegateInfo.DelegateList;
            To = separateEmailAddresses(To, lDelegates, ref szDelegatedEmailAddresses);
            szCC = separateEmailAddresses(szCC, lDelegates, ref szDelegatedEmailAddresses);
        }
        if (!String.IsNullOrWhiteSpace(To)) {
            msg.To.Add(To);
        } else {
            msg.To.Add(EmailSettings.SMTPServerUserName); //We need an address here or the email will fail
        }

        if (!String.IsNullOrWhiteSpace(szCC))
            msg.CC.Add(szCC);

        //We do not delegate for the BCC recipients
        if (!String.IsNullOrWhiteSpace(szBCC))
            msg.Bcc.Add(separateEmailAddresses(szBCC, null, ref szDelegatedEmailAddresses));

        if (!String.IsNullOrWhiteSpace(szDelegatedEmailAddresses)) {
            msg.CC.Add(szDelegatedEmailAddresses.TrimEnd(','));
        }
        if (szFrom == "")
            szFrom = EmailSettings.SMTPServerFromEmail;
        msg.From = new MailAddress(szFrom, DisplayName);
        msg.Subject = Subject;
        if (!FromQueue) {
            HTMLBody = String.Format(@"
            <html><head>
            <style> </style>
            <body>
                {0}<br />
            </body></html>", HTMLBody);
        }
        msg.Body = HTMLBody;

        //Determine whether we send the email now or simply add it into the Queue
        if (!FromQueue) {
            EmailQueue.queueEmail(msg, Type, DisplayName, IncludeFile);
        } else {
            if (IncludeFile != null) {
                msg.Attachments.Add(IncludeFile);
            }
            SmtpClient oSMTP = Email.getEmailServer();
            try {
                oSMTP.Send(msg);
                string szLog = String.Format(@"
                    From: {0}
                    To: {1}
                    CC: {2}
                    BCC: {3}
                    Subject: {4}
                    Body: {5}
                    ", szFrom, To, szCC, szBCC, Subject, HTMLBody);
                EmailLog.addLog(Type, Subject, szFrom, To, szCC, HTMLBody, LogObjectID);
            } catch (Exception e){
                DB.runNonQuery("--" + DB.escape(e.Message));
                return false;
            } finally {
                oSMTP = null;
            }
        }
        return true;
    }



    public static string separateEmailAddresses(string Addresses) {
        string szAdd = Addresses.Replace(";", ",").Replace(Environment.NewLine, ",").Trim().Replace(" ", ",").Replace(",,", ",").TrimEnd(',');
        return szAdd;
    }

    public static string separateEmailAddresses(string Addresses, List<UserDelegate> lDelegateList, ref string DelegatedEmailAddresses) {
        string szAdd = Addresses.Replace(";", ",").Replace(Environment.NewLine, ",").Trim().Replace(" ", ",").Replace(",,", ",").Trim().TrimEnd(',');

        if (lDelegateList == null || lDelegateList.Count == 0)
            return szAdd;

        //For each email address we have, check to see if there are any current email delegations. If there are, replace the original
        string[] lAddresses = szAdd.Split(',');
        szAdd = "";
        foreach (string szEmail in lAddresses) {
            UserDetail oD = G.UserInfo.getUserByEmail(szEmail);
            if (oD == null) {
                Utility.Append(ref szAdd, szEmail, ",");
                continue;
            }

            var lDels = lDelegateList.FindAll(o => (o.Type == DelegationType.EmailAndLogin || o.Type == DelegationType.EmailOnly) && o.UserID == oD.ID);
            if (lDels.Count == 0)
                Utility.Append(ref szAdd, szEmail, ",");
            else {
                foreach (UserDelegate oUD in lDels) {
                    Utility.Append(ref szAdd, G.UserInfo.getUser(oUD.DelegationUserID).Email, ",");
                    if (oUD.SendEmailOrigUser) //Include the original emailer
                        Utility.Append(ref DelegatedEmailAddresses, szEmail, ",");
                }
            }
        }
        return szAdd.Trim().TrimEnd(',');
    }

    public static SmtpClient getEmailServer() {
        SmtpClient oSMTP = new SmtpClient(EmailSettings.SMTPServer);
        oSMTP.EnableSsl = EmailSettings.SMTPServerSSL;
        if (!String.IsNullOrEmpty(EmailSettings.SMTPServerUserName)) {
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(EmailSettings.SMTPServerUserName, EmailSettings.SMTPServerPassword);
            oSMTP.Credentials = credentials;
        }
        return oSMTP;
    }

    public static class EmailQueue {

        public static void queueEmail(MailMessage msg, EmailType Type, string DisplayName, Attachment IncludeFile) {
            sqlUpdate oSQL = new sqlUpdate("EMAILQUEUE", "ID", -1);
            oSQL.add("EmailType", (int)Type);
            oSQL.add("ToList", msg.To.ToString());
            oSQL.add("CCList", msg.CC.ToString());
            oSQL.add("BCCList", msg.Bcc.ToString());
            oSQL.add("Subject", msg.Subject);
            oSQL.add("Body", msg.Body);
            oSQL.add("DisplayName", DisplayName);
            if (IncludeFile != null) {
                oSQL.add("ATTACHMENT", IncludeFile.Name);
                EmailQueue.saveMailAttachment(IncludeFile);
            }
            DB.runNonQuery(oSQL.createInsertSQL());
        }

        /// <summary>
        /// Sends any emails in the email cache
        /// </summary>
        public static void checkCache() {
            List<EmailDB> lEmails = new List<EmailDB>();
            using (IDbConnection _db = new SqlConnection(G.szCnn)) {
                lEmails = _db.Query<EmailDB>("SELECT * From EMAILQUEUE where IsProcessing = 0;").ToList();
            }
            foreach (EmailDB oE in lEmails) {
                Attachment IncludeFile = null;
                if(!String.IsNullOrEmpty(oE.Attachment)) {
                    IncludeFile = new Attachment(getAttachmentFileName(oE.Attachment));
                }
                oE.startProcessing();
                if(Email.sendMail(oE.ToList, "", oE.Subject, oE.Body, oE.CCList, oE.BCCList, IncludeFile, -1, oE.EmailType, oE.DisplayName, true)) {
                    oE.markSent();
                } else {
                    oE.failProcessing();
                }
            }
        }

        private static void saveMailAttachment(Attachment attachment) {
            byte[] allBytes = new byte[attachment.ContentStream.Length];
            int bytesRead = attachment.ContentStream.Read(allBytes, 0, (int)attachment.ContentStream.Length);

            string destinationFile = getAttachmentFileName(attachment.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
            BinaryWriter writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            writer.Write(allBytes);
            writer.Close();
        }

        private static string getAttachmentFileName(string FileName) {
            if (String.IsNullOrEmpty(FileName))
                return "";
            return Path.Combine(G.Settings.DataDir, "Attachments", FileName);
        }

        public class EmailDB {
            public int ID { get; set; }
            public EmailType EmailType { get; set; }
            public string ToList { get; set; }
            public string CCList { get; set; }
            public string BCCList { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string DisplayName { get; set; }
            public string Attachment { get; set; }

            public void markSent() {
                DB.runNonQuery("DELETE FROM EMAILQUEUE where id = " + ID);
            }

            public void startProcessing() {
                DB.runNonQuery("UPDATE EMAILQUEUE set IsProcessing = 1 WHERE ID = " + ID);
            }
            public void failProcessing() {
                DB.runNonQuery("UPDATE EMAILQUEUE set IsProcessing = 0 WHERE ID = " + ID);
            }
        }
    }
   
}

public class RegexUtilities {
    private bool invalid = false;

    public bool IsValidEmail(string strIn) {
        invalid = false;
        if (String.IsNullOrEmpty(strIn))
            return false;

        // Use IdnMapping class to convert Unicode domain names.
        try {
            strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));
        } catch (RegexMatchTimeoutException) {
            return false;
        }

        if (invalid)
            return false;

        // Return true if strIn is in valid e-mail format.
        try {
            return Regex.IsMatch(strIn,
                  @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                  RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        } catch (RegexMatchTimeoutException) {
            return false;
        }
    }

    private string DomainMapper(Match match) {
        // IdnMapping class with default property values.
        IdnMapping idn = new IdnMapping();

        string domainName = match.Groups[2].Value;
        try {
            domainName = idn.GetAscii(domainName);
        } catch (ArgumentException) {
            invalid = true;
        }
        return match.Groups[1].Value + domainName;
    }
}