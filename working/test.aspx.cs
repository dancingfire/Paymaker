using System;

public partial class test : Root {

    protected void Page_Load(object sender, EventArgs e) {
        Response.Write(G.BaseURL);
    }

  protected void btnSend_Click(object sender, EventArgs e) {
        UserLogin.loginUserByID(661);

    }
}