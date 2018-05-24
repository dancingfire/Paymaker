using System;
using System.Web.UI;

namespace Paymaker {

    /// <summary>
    /// Summary description for about.
    /// </summary>
    public partial class campaign_dashboard : Root {
        protected System.Web.UI.WebControls.Button Button2;

        protected void Page_Load(object sender, System.EventArgs e) {
            if (!Page.IsPostBack) {
            }
        }

        protected void btnLoad_Click(object sender, EventArgs e) {
            CampaignImport.performFullImport(true);
        }

        protected void btnRefreah_Click(object sender, EventArgs e) {
            CampaignImport.performFullImport(false);
        }
    }
}