using System;
using System.Web.UI;

public partial class campaign_settings : Root {
    private AppConfigAdmin oConfigAdmin = new AppConfigAdmin();

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            oConfigAdmin.addConfig("PREPAYMENTDAYS", "40");
            oConfigAdmin.addConfig("INVOICETOTALTHRESHOLD", "10000");

            if (!Page.IsPostBack) {
                loadSettings();
            }
        }
    }

    private void loadSettings() {
        oConfigAdmin.loadValuesFromDB();
        txtNumberOfDays.Text = oConfigAdmin.getValue("PREPAYMENTDAYS");
        txtInvoiceThreshold.Text = oConfigAdmin.getValue("INVOICETOTALTHRESHOLD");
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        oConfigAdmin.updateConfigValueSQL("PREPAYMENTDAYS", Utility.formatForDB(txtNumberOfDays.Text));
        oConfigAdmin.updateConfigValueSQL("INVOICETOTALTHRESHOLD", Utility.formatForDB(txtInvoiceThreshold.Text));
    }
}