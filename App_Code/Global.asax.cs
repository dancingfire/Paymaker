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
            ComponentPro.Saml.SamlSettings.LogWriter = new ComponentPro.Saml.Diagnostics.FileLogWriter("c:\\home\\site\\wwwroot\\saml.log", ComponentPro.Saml.Diagnostics.LogLevel.Verbose, false);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["SentryDNS"])) {
                _sentry = SentrySdk.Init(o => {
                    o.Debug = true;
                    o.Dsn = ConfigurationManager.AppSettings["SentryDNS"];
                    o.Environment = ConfigurationManager.AppSettings["Environment"];
                });

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
            System.Diagnostics.Debug.WriteLine("Hit the error");
            var exception = Server.GetLastError();
            SentrySdk.ConfigureScope(scope => {
                if (!String.IsNullOrEmpty(G.User.Email)) {
                    scope.User = new Sentry.User {
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

        private void sqlConnection1_InfoMessage(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e) {
        }
    }
}