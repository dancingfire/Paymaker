using System;

public partial class campaign_import : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        Session["USERID"] = 0;
        if (Valid.getText("debug", "false", VT.TextNormal) != "true")
            CampaignImport.performFullImport(false, chkUpdateOnly.Checked);
    }

    protected void btnImport_Click(object sender, EventArgs e) {
        CampaignImport.performFullImport(false, chkUpdateOnly.Checked);
    }
}