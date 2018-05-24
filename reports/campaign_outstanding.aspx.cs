using System;
using System.Data;

public partial class campaign_outstanding : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
        showData();
    }

    private void showData() {
        string szCompanyFilter = Valid.getText("szCompanyID", "", VT.TextNormal);
        if (szCompanyFilter == "null")
            szCompanyFilter = "";
        if (!String.IsNullOrWhiteSpace(szCompanyFilter)) {
            szCompanyFilter = " AND L_OFFICE.COMPANYID IN (" + szCompanyFilter + ")";
        }
        //Create data structure to form final output.
        string szSQL = string.Format(@" SELECT USR.ID AS USERID, USR.FIRSTNAME + ' ' + USR.LASTNAME AS AGENT, L_OFFICE.NAME AS OFFICE, 0.0 as AMOUNT, 0 as USED
                FROM DB_USER USR JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                WHERE USR.ISACTIVE = 1 {0}
                ORDER BY LASTNAME, FIRSTNAME", szCompanyFilter);
        DataSet dsUsers = DB.runDataSet(szSQL);
        DataView dtUsers = dsUsers.Tables[0].DefaultView;
        double dTotal = 0;

        DataView dvCampaigns = CampaignDB.loadCampaignData(CampaignPageView.InvoiceAging);
        foreach (DataRowView drCurrCampaign in dvCampaigns) {
            if (String.IsNullOrEmpty(drCurrCampaign["AGENTID"].ToString()))
                continue;
            dtUsers.RowFilter = "USERID = " + drCurrCampaign["AGENTID"].ToString();
            foreach (DataRowView dv in dtUsers) {
                double dblTotalOwing = Convert.ToDouble(drCurrCampaign["TOTALOWING"]);
                if (dblTotalOwing > 0) {
                    dv["AMOUNT"] = Convert.ToDouble(dv["AMOUNT"]) + Convert.ToDouble(drCurrCampaign["TOTALOWING"]);
                    dTotal += Convert.ToDouble(drCurrCampaign["TOTALOWING"]);
                    dv["USED"] = 1;
                }
            }
        }

        lblTotal.Text = "Overall total: " + Utility.formatMoney(dTotal);
        dtUsers.RowFilter = "USED = 1";
        gvCurrent.DataSource = dtUsers;
        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent);
    }
}