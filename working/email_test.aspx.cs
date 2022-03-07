using System;
using System.Net;
using System.Net.Mail;

public partial class email_test : Root {

    protected void Page_Load(object sender, EventArgs e) {
       
    }

  protected void btnSend_Click(object sender, EventArgs e) {
        try {
            if (chkInclude.Checked) {
                Attachment a = new Attachment(Server.MapPath("./SalesHistory.xls"));
                Email.sendMail(txtTo.Text, EmailSettings.SMTPServerUserName, "Test email from CAPS", txtMsg.Text, IncludeFile: a, DisplayName: "Test email name");
            } else {
                Email.sendMail(txtTo.Text, EmailSettings.SMTPServerUserName, "Test email from CAPS", txtMsg.Text, DisplayName: "Test email name");
            }
        }  catch (Exception e1) {
            Response.Write(e1.Message);
        }

    }

     SmtpClient getEmailServer() {
        SmtpClient oSMTP = new SmtpClient(EmailSettings.SMTPServer);
        oSMTP.Port = 25;
       // oSMTP.UseDefaultCredentials = true;
        if (!String.IsNullOrEmpty(EmailSettings.SMTPServerUserName)) {
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(EmailSettings.SMTPServerUserName, EmailSettings.SMTPServerPassword);
            oSMTP.Credentials = credentials;
        }
        return oSMTP;
    }
    protected void btnSendDirect_Click(object sender, EventArgs e) {
        try {
            MailMessage msg = new MailMessage();
            msg.IsBodyHtml = true;
            msg.Subject = "Test email from CAPS";
            msg.To.Add(txtTo.Text);
            msg.From  = new MailAddress(EmailSettings.SMTPServerFromEmail, "Test sender");
            SmtpClient oSMTP = getEmailServer();
            oSMTP.Send(msg);
        } catch (Exception e1) {
            Response.Write(e1.Message);
        }

    }
}