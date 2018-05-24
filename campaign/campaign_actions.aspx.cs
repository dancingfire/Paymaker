using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class campaign_actions : Root {
    private DataSet dsCampaign = null;
    private DataView dvSpend = null;
    private DataView dvInvoiced = null;
    private DataView dvPaid = null;

    protected void Page_Load(object sender, System.EventArgs e) {
        showData();
        Form.Controls.Add(HTML.createLabel(HTML.createModalIFrameHTML("Campaign", "Campaign update", "1160px", 600)));
    }

    private void showData() {
        dsCampaign = CampaignDB.loadCampaignActions();
        dvSpend = dsCampaign.Tables[1].DefaultView;
        dvInvoiced = dsCampaign.Tables[2].DefaultView;
        dvPaid = dsCampaign.Tables[3].DefaultView;

        foreach (DataRow drC in dsCampaign.Tables[0].Rows) {
            //Get the campaign product total from the product table
            dvSpend.RowFilter = "ID = " + drC["ID"].ToString();
            dvInvoiced.RowFilter = "ID = " + drC["ID"].ToString();
            dvPaid.RowFilter = "ID = " + drC["ID"].ToString();
            foreach (DataRowView oRow in dvSpend) {
                // If this item has been reconciled, then use the vendor total, otherwise use the cost total
                if (Convert.ToDouble(oRow["VENDORTOTAL"]) == 0)
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["ACTUALTOTAL"]));
                else
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["VENDORTOTAL"]));
                drC["PRODUCTSINVOICED"] = Convert.ToInt32(oRow["PRODUCTSINVOICED"]);
                drC["PRODUCTCOUNT"] = Convert.ToInt32(oRow["PRODUCTCOUNT"]);
                drC["AMOUNTLEFT"] = Utility.formatMoney(Convert.ToDouble(drC["APPROVEDBUDGET"]) - Convert.ToDouble(drC["TOTALSPENT"]));
            }

            foreach (DataRowView oRow in dvInvoiced) {
                drC["TOTALINVOICED"] = Utility.formatMoney(Convert.ToDouble(oRow["TOTALINVOICED"]));
            }

            foreach (DataRowView oRow in dvPaid) {
                drC["TOTALPAID"] = Utility.formatMoney(Convert.ToDouble(oRow["TOTALPAID"]));
            }
            drC["TOTALOWING"] = Utility.formatMoney(Convert.ToDouble(drC["TOTALINVOICED"]) - Convert.ToDouble(drC["TOTALPAID"]));
        }

        DataView dvCampaigns = dsCampaign.Tables[0].DefaultView;
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
}