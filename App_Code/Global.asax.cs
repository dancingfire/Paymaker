using System;

namespace Paymaker {

    /// <summary>
    /// Summary description for Global.
    /// </summary>
    public class Global : System.Web.HttpApplication {
        private EventArgs e = null;

        public Global() {
        }

        protected void Application_Start(Object sender, EventArgs e) {
        }

        protected void Session_Start(Object sender, EventArgs e) {
        }

        protected void Application_BeginRequest(Object sender, EventArgs e) {
        }

        protected void Application_EndRequest(Object sender, EventArgs e) {
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
        }

        protected void Application_Error(Object sender, EventArgs e) {
            // avoids single exception triggering error handlin twice.
            if (this.e == e)
                return;
            this.e = e;

            ExceptionHandler.LogException exc = new ExceptionHandler.LogException();
            exc.HandleException(Server.GetLastError().GetBaseException());
        }

        protected void Session_End(Object sender, EventArgs e) {
            Response.Redirect("login.aspx");
        }

        protected void Application_End(Object sender, EventArgs e) {
        }

        private void sqlConnection1_InfoMessage(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e) {
        }
    }
}