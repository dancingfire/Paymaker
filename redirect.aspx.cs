using System;
using System.Web;
using System.Web.UI;

namespace Paymaker {

    public partial class redirect : Page {
        protected void Page_Init(object sender, System.EventArgs e) {
        }

        protected void Page_Load(object sender, System.EventArgs e) {

            if (HttpContext.Current.Session["USERID"] == null && G.Settings.Env != "dev") {
                
                if (!HttpContext.Current.User.Identity.IsAuthenticated) {
                    Response.Redirect("/acs/samllogin.aspx?post_login_redirect_url=/redirect.aspx");
                } else {
                    if (!UserLogin.loginUserByEmail(HttpContext.Current.User.Identity.Name)) {
                        Response.Write("Please contact your administrator to setup access to this application. We tried with the name: " + HttpContext.Current.User.Identity.Name + " Please ensure that theis record has a login name entered so the system will allow access.");
                        Response.End();
                    } else {
                        Response.Redirect("/acs/samllogin.aspx?post_login_redirect_url=/redirect.aspx");
                    }
                }
            }
            string szStartPage = UserLogin.getStartPage();
            Response.Write(String.Format("<script>window.top.location.href = '../{0}';</script>", szStartPage));
        }
    }
}