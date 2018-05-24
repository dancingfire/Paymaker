using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class campaign_detail : Root {
    private DataSet dsCampaign = null;
    private CampaignPageView PageMode = CampaignPageView.Completed;

    protected void Page_Load(object sender, System.EventArgs e) {
        PageMode = getPageMode();
        if (PageMode == CampaignPageView.Completed) {
            dSearch.Visible = true;
            lblHeader.Visible = false;
        } else {
            showData();
        }
        Form.Controls.Add(HTML.createLabel(HTML.createModalIFrameHTML("Campaign", "Campaign update", "1160px", 600)));
    }

    private CampaignPageView getPageMode() {
        string szPageType = Valid.getText("type", VT.NoValidation);
        switch (szPageType) {
            case "COMPLETED": return CampaignPageView.Completed;
            case "EXCEEDING":
                lblHeader.Text = "Campaigns exceding authority ";
                return CampaignPageView.ExceedingAuthority;

            case "ALL":
                lblHeader.Text = "All campaigns ";
                return CampaignPageView.All;

            case "NEW":
                lblHeader.Text = "New campaigns ";
                return CampaignPageView.New;

            case "INVOICEDUE":
                lblHeader.Text = "Campaigns due for invoicing";
                return CampaignPageView.InvoiceDue;

            case "INVOICEPARTIAL":
                lblHeader.Text = "Campaigns due for partial invoicing";
                return CampaignPageView.PartialInvoiced;

            case "INVOICECHANGED":
                lblHeader.Text = "Campaigns with modified invoices";
                return CampaignPageView.InvoiceChanged;

            case "MANUAL": return CampaignPageView.InvoiceChanged;
            case "DUEFORCOLLECTION":
                lblHeader.Text = "Campaigns due for collection";
                return CampaignPageView.DueForCollection;

            case "OVERDUE30DAYS":
                lblHeader.Text = "Campaigns overdue for 30+ days";
                return CampaignPageView.Overdue;

            case "ACTIONED":
                lblHeader.Text = "Campaigns requiring action ";
                return CampaignPageView.Actioned;
        }
        return CampaignPageView.All;
    }

    private void showData() {
        DataView dvCampaigns = CampaignDB.loadCampaignData(PageMode, "", txtSearch.Text);
        gvCurrent.DataSource = dvCampaigns;
        lblHeader.Text += " (" + dvCampaigns.Count + " campaigns found)";

        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent, true);
    }

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "updateCampaign(" + szID + ")";
        }
    }

    public string getIcons(int NoteCount, int CampaignStatus, bool IsDeleted, int CampaignID) {
        string szHTML = "";
        if (NoteCount > 0)
            szHTML = String.Format("<img src='../sys_images/notes.gif' align='middle'/>");
        if (CampaignStatus == 1)
            szHTML += "<img src='../sys_images/invoice.gif' title='This campaign has agent/company contributions' align='middle' />";
        if (IsDeleted)
            szHTML += String.Format(@"
                <img src='../sys_images/restore.gif' title='This campaign is currently marked as deleted - click to restore' align='middle' style='cursor: pointer' onclick='restoreCampaign({0});'/>
            ", CampaignID);
        return szHTML;
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
        showData();
    }

    protected void btnRestore_Click(object sender, EventArgs e) {
        string szSQL = "UPDATE CAMPAIGN SET ISDELETED = 0 WHERE ID = " + Valid.getInteger("hdRestoreCampaignID");
        DB.runNonQuery(szSQL);
        showData();
    }
}