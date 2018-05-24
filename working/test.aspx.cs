using System;

public partial class test : Root {

    protected void Page_Load(object sender, EventArgs e) {
       
    }

  protected void btnSend_Click(object sender, EventArgs e) {
        Email.sendMail(txtTo.Text, "do-not-reply@fletchers.net.au", "Test email from CAPS",
              txtMsg.Text, szBCC: "payroll@fletchers.net.au");


    }
}