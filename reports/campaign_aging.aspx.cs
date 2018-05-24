using System;
using System.Data;

public partial class campaign_aging : Root {
    private double dGrandTotal = 0;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
        showData();
    }

    private void showData() {
        lblHeader.Text = "Outstanding advertising as of " + Utility.formatDate(DateTime.Now);

        DataView dvCampaigns = CampaignDB.loadCampaignData(CampaignPageView.InvoiceAging);
        dvCampaigns.Sort = "AGENT";
        setFilter(ref dvCampaigns, "DAYSOWING < 30");
        lblCurrentTotal.Text = "Total <30: " + Utility.formatMoney(getTotal(dvCampaigns));
        gvCurrent.DataSource = dvCampaigns;
        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent);

        setFilter(ref dvCampaigns, "DAYSOWING > 30 AND DAYSOWING < 60");
        lbl30DaysTotal.Text = "Total 30 - 60: " + Utility.formatMoney(getTotal(dvCampaigns));
        gv30.DataSource = dvCampaigns;
        gv30.DataBind();
        HTML.formatGridView(ref gv30);

        setFilter(ref dvCampaigns, "DAYSOWING > 60 AND DAYSOWING < 90");
        lbl60DaysTotal.Text = "Total 60 - 90: " + Utility.formatMoney(getTotal(dvCampaigns));
        gv60.DataSource = dvCampaigns;
        gv60.DataBind();
        HTML.formatGridView(ref gv60);

        setFilter(ref dvCampaigns, "DAYSOWING > 90 AND DAYSOWING < 120");
        lbl90DaysTotal.Text = "Total 90 - 120: " + Utility.formatMoney(getTotal(dvCampaigns));
        gv90.DataSource = dvCampaigns;
        gv90.DataBind();
        HTML.formatGridView(ref gv90);

        setFilter(ref dvCampaigns, "DAYSOWING > 120 ");
        lbl120DaysTotal.Text = "Total 120+: " + Utility.formatMoney(getTotal(dvCampaigns));
        gv120.DataSource = dvCampaigns;
        gv120.DataBind();
        HTML.formatGridView(ref gv120);
        lblGrandTotal.Text = "Grand Total: " + Utility.formatMoney(dGrandTotal);
    }

    private void setFilter(ref DataView dv, string Filter) {
        string szCompanyFilter = Valid.getText("szCompanyID", "", VT.TextNormal);
        if (szCompanyFilter == "null")
            szCompanyFilter = "";

        dv.RowFilter = "(TOTALOWING > 0 OR (TOTALOWING = 0 AND TOTALINVOICED = 0 AND TOTALPAID = 0)) AND " + Filter;

        if (!String.IsNullOrWhiteSpace(szCompanyFilter))
            dv.RowFilter += " AND COMPANYID IN (" + szCompanyFilter + ")";
    }

    private double getTotal(DataView dv) {
        double dTotal = 0;

        foreach (DataRowView r in dv) {
            dTotal += Convert.ToDouble(r["TOTALOWING"]);
        }
        dGrandTotal += dTotal;
        return dTotal;
    }
}