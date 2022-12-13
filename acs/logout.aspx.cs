using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;

public partial class logout : Page {

    protected void Page_Init(object sender, EventArgs e) {
    }

    protected void Page_Load(object sender, EventArgs e) {
		FormsAuthentication.SignOut();
		Session.Abandon();
		// clear authentication cookie
		HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
		cookie1.Expires = DateTime.Now.AddYears(-1);
		Response.Cookies.Add(cookie1);

		// clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
		SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
		HttpCookie cookie2 = new HttpCookie(sessionStateSection.CookieName, "");
		cookie2.Expires = DateTime.Now.AddYears(-1);
		Response.Cookies.Add(cookie2);
    }
}