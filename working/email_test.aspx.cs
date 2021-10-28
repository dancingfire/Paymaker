using System;
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
}