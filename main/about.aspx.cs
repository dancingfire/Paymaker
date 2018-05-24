using System;

namespace Paymaker {

    /// <summary>
    /// Summary description for about.
    /// </summary>
    public partial class about : Root {
        protected System.Web.UI.WebControls.Button Button2;

        protected void Page_Load(object sender, System.EventArgs e) {
        }

        protected void btnReload_Click(object sender, EventArgs e) {
            G.PayPeriodInfo.forceReload();
            G.UserInfo.forceReload();
        }
    }
}