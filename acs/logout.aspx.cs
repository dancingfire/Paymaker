using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;

public partial class logout : Page {

    protected void Page_Init(object sender, EventArgs e) {
    }

    protected void Page_Load(object sender, EventArgs e) {
        UserLogin.logout();
    }
}