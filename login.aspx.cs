
using System.Text;
using System.Web;
using System.Web.UI;

namespace Paymaker {

    public partial class login : Page {

        /// <summary>
        /// Note - this is a flase login page - only used for redirecting to the leave request page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
      
        protected void Page_Load(object sender, System.EventArgs e) {
            //Check whether we are redirecting to the leave page
            G.User.IsLeave = Request.QueryString["LEAVE"] != null;

            if (!HttpContext.Current.User.Identity.IsAuthenticated) {
                Response.Redirect("/acs/samllogin.aspx?post_login_redirect_url=payroll/leave_manager_dashboard.aspx.aspx");
            } else {
                //Oupput the javascript to redirect to the leave page if the user is already logged in
                StringBuilder sbEndJS = new StringBuilder();
                sbEndJS.AppendFormat("<script>window.top.location.href = '{0}';", UserLogin.getStartPage());
                sbEndJS.Append("</script>");
                Response.Write(sbEndJS.ToString());
            }
        }
    }
}