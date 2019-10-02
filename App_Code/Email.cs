using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

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
    public static void sendMail(string To, string szFrom, string Subject, string HTMLBody, string szCC = "", string szBCC = "", Attachment IncludeFile = null, int LogObjectID = -1, EmailType Type = EmailType.General, string DisplayName = "") {
        List<UserDelegate> lDelegates = G.UserDelegateInfo.DelegateList;
        string szDelegatedEmailAddresses = "";

        MailMessage msg = new MailMessage();
        string szLog = String.Format(@"
            From: {0}
            To: {1}
            CC: {2}
            BCC: {3}
            Subject: {4}
            Body: {5}
            ", szFrom, To, szCC, szBCC, Subject, HTMLBody);
        EmailLog.addLog(Type, Subject, szFrom, To, szCC, HTMLBody, LogObjectID);
        msg.IsBodyHtml = true;

        //Perform that delegation first as this may clear out the To field
        To = separateEmailAddresses(To, lDelegates, ref szDelegatedEmailAddresses);
        if (!String.IsNullOrWhiteSpace(To)) {
            msg.To.Add(To);
        } else {
            msg.To.Add("do-not-reply@fletchers.net.au"); //We need an address here or the email will fail
        }

        szCC = separateEmailAddresses(szCC, lDelegates, ref szDelegatedEmailAddresses);
        if (!String.IsNullOrWhiteSpace(szCC))
            msg.CC.Add(szCC);

        //We do not delegate for the BCC recipients
        if (!String.IsNullOrWhiteSpace(szBCC))
            msg.Bcc.Add(separateEmailAddresses(szBCC, null, ref szDelegatedEmailAddresses));

        if (!String.IsNullOrWhiteSpace(szDelegatedEmailAddresses)) {
            msg.CC.Add(szDelegatedEmailAddresses.TrimEnd(','));
        }
        if (szFrom == "")
            szFrom = "do-not-reply@fletchers.net.au";
        msg.From = new MailAddress(szFrom, DisplayName);
        msg.Subject = Subject;

        HTMLBody = String.Format(@"<html><head>
            <style> </style>
            <body>
                {0}

                <br />
            </body></html>", HTMLBody);
        msg.Body = HTMLBody;
        if (IncludeFile != null) {
            msg.Attachments.Add(IncludeFile);
        }

        SmtpClient oSMTP = Email.getEmailServer();
        try {
            oSMTP.Send(msg);
        } catch {
            throw;
        } finally {
            oSMTP = null;
        }
    }

    public static string separateEmailAddresses(string Addresses) {
        string szAdd = Addresses.Replace(";", ",").Replace(Environment.NewLine, ",").Trim().Replace(" ", ",").Replace(",,", ",").TrimEnd(',');
        return szAdd;
    }


    public static string separateEmailAddresses(string Addresses, List<UserDelegate> lDelegateList, ref string DelegatedEmailAddresses) {
        string szAdd = Addresses.Replace(";", ",").Replace(Environment.NewLine, ",").Trim().Replace(" ", ",").Replace(",,", ",").Trim().TrimEnd(',');

        if (lDelegateList.Count == 0)
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
        if (!String.IsNullOrEmpty(EmailSettings.SMTPServerUserName)) {
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(EmailSettings.SMTPServerUserName, EmailSettings.SMTPServerPassword);
            oSMTP.Credentials = credentials;
        }
        return oSMTP;
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