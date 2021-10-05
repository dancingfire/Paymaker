using System;

public partial class email_test : Root {

    protected void Page_Load(object sender, EventArgs e) {
       
    }

  protected void btnSend_Click(object sender, EventArgs e) {
        try {
            Email.sendMail(txtTo.Text, EmailSettings.SMTPServerUserName, "Test email from CAPS", txtMsg.Text, DisplayName: "Test email name");
        }  catch (Exception e1) {
            Response.Write(e1.Message);
        }

    }
}