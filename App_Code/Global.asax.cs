using System;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Caching;
using Sentry;

namespace Paymaker {

    /// <summary>
    /// Summary description for Global.
    /// </summary>
    public class Global : System.Web.HttpApplication {
        private EventArgs e = null;
        private static CacheItemRemovedCallback OnCacheRemove = null;
        private IDisposable _sentry;
        
        public Global() {
        }

        protected void Application_Start(Object sender, EventArgs e) {
            AddTask("checkEmailQueue", 30);
          //  ComponentPro.Saml.SamlSettings.LogWriter = new ComponentPro.Saml.Diagnostics.FileLogWriter("c:\\home\\site\\wwwroot\\saml.log", ComponentPro.Saml.Diagnostics.LogLevel.Verbose, false);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            SentryOptions o = new SentryOptions();
            //o.AddAS();
            o.Debug = true;
            o.Dsn = ConfigurationManager.AppSettings["SentryDNS"];
            o.Environment = ConfigurationManager.AppSettings["Environment"];
            o.TracesSampleRate = 1.0;


            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["SentryDNS"])) {
                _sentry = SentrySdk.Init(o);
            };

        }

        private void AddTask(string name, int seconds) {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert(name, seconds, null, DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, OnCacheRemove);
        }

        public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r) {
            try {
                Email.EmailQueue.checkCache();
            } catch (Exception e) {
                ; //Ignore this - we will try again
            }
            AddTask(k, Convert.ToInt32(v));
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Diagnostics.Debug.WriteLine("Hit the error");
            var exception = Server.GetLastError();

            //trace.axd is ASP.NET's built-in diagnostics handler. We run with tracing disabled (see
            //web.config), so it throws by design whenever it's requested - almost always an automated
            //scan probing for the endpoint, not anything actionable, so don't bother alerting on it.
            if (Request.Path.EndsWith("/trace.axd", StringComparison.OrdinalIgnoreCase))
                return;

            //A 404 (missing file/page) is never an application bug worth alerting on - it's either a
            //stale internal/external link or one of the constant automated scans probing for well-known
            //CVE paths (SharePoint, WordPress, etc.) that simply don't exist in this app.
            HttpException httpException = exception as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404)
                return;

            SentrySdk.ConfigureScope(scope => {
                if (!String.IsNullOrEmpty(G.User.Email)) {
                    scope.User = new SentryUser {
                        Id = Convert.ToString(G.User.UserID),
                        Email = G.User.Email
                    };
                }
            });
            SentrySdk.CaptureException(exception);
        }

        protected void Session_End(Object sender, EventArgs e) {
            ;
        }

        protected void Application_End(Object sender, EventArgs e) {
            _sentry.Dispose();
        }
    }
}