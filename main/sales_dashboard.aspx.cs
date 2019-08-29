using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sales_dashboard : Root {
    private int CurrentUserID;

    protected void Page_Load(object sender, System.EventArgs e) {
        CurrentUserID = G.User.ID;

        // This is for debuging and hard coded only to allow Gord's login to access other users details
        if (CurrentUserID == 0 && G.User.UserName == "Gord Funk") {
            int intAltUser = Valid.getInteger("UserID", -1);
            if (intAltUser > -1)
                CurrentUserID = intAltUser;
        }
        showData();
    }

    private void showData() {
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Sale", "Sale", "95%", -1, "sale_update.aspx")));
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Campaign", "Campaign", "95%", 600, "../campaign/campaign_update.aspx")));
        //Load the current sales for this
        gvCurrent.DataSource = Sale.loadSalesForSalesPerson(CurrentUserID, G.CurrentPayPeriodStart, G.CurrentPayPeriodEnd);
        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent, true);

        if (gvCurrent.Rows.Count == 0) {
            gvCurrent.Visible = false;
            pNoDataCurrent.Visible = true;
        }

        //Load the pending sales for this agent
        gvFuture.DataSource = Sale.loadSalesForSalesPerson(CurrentUserID, G.CurrentPayPeriodEnd, DateTime.MaxValue);
        gvFuture.DataBind();
        HTML.formatGridView(ref gvFuture, true);
        if (gvFuture.Rows.Count == 0) {
            gvFuture.Visible = false;
            pNoFutureSales.Visible = true;
        }
        //Load the summary sales for this agent
        DataSet dsHistorical = Sale.loadYTDSalesForSalesPerson(CurrentUserID, Utility.getFinYearStart(DateTime.Now));
        DataRow drTotal = dsHistorical.Tables[0].NewRow();
        double dTotal = 0;
        foreach (DataRow dr in dsHistorical.Tables[0].Rows) {
            dTotal += Convert.ToDouble(dr["CommissionTotal"]);
        }
        drTotal["Month"] = "<b>Total</b>";
        drTotal["CommissionTotal"] = dTotal;
        dsHistorical.Tables[0].Rows.InsertAt(drTotal, 0);

        gvHistory.DataSource = dsHistorical;
        gvHistory.DataBind();
        HTML.formatGridView(ref gvHistory, true);

        loadCampaignActions();
        loadPrePaymentHistory();
    }

    private void loadCampaignActions() {
        DataSet dsCampaign = CampaignDB.loadCampaignActions(G.User.ID);
        DataView dvSpend = dsCampaign.Tables[1].DefaultView;
        DataView dvInvoiced = dsCampaign.Tables[2].DefaultView;
        DataView dvPaid = dsCampaign.Tables[3].DefaultView;
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
            if (String.IsNullOrEmpty(Convert.ToString(drC["ACTIONID"])))
                drC["ACTIONID"] = -1;
        }
        DataView dv = dsCampaign.Tables[0].DefaultView;
        dv.Sort = "ACTIONID DESC, STARTDATE DESC";
        gvCampaign.DataSource = dv;
        gvCampaign.DataBind();
        HTML.formatGridView(ref gvCampaign, true);
        if (gvCampaign.Rows.Count == 0) {
            gvCampaign.Visible = false;
            pCampaignNoData.Visible = true;
        }
    }

    private void loadPrePaymentHistory() {
        DateTime dtEnd = DateTime.Now.AddMonths(-2);
        dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, DateTime.DaysInMonth(dtEnd.Year, dtEnd.Month)); //Get last day of month
        DateTime dtStart = dtEnd.AddMonths(-3).AddDays(1);
        string szCampaignFilterSQL = String.Format(@" WHERE ADDRESS1 NOT LIKE '%PROMO,%' AND C.STARTDATE BETWEEN '{0}' AND '{1}'", Utility.formatDate(dtStart), Utility.formatDate(dtEnd));
        string szUserFilter = string.Format(@" AND U.ID = {0}", CurrentUserID);
        DataTable dtPre = CampaignDB.loadPrePaymentCampaignData(szCampaignFilterSQL, "", szUserFilter: szUserFilter);

        double dSpent = 0, dPaid = 0, dOutstanding = 0, dPrePaid = 0;
        int count = 0;
        foreach (DataRow dr in dtPre.Rows) {
            dSpent += DB.readDouble(dr["TOTALSPENT"], 0);
            dPaid += DB.readDouble(dr["TOTALPAID"], 0);
            dPrePaid += DB.readDouble(dr["TOTALPREPAID"], 0);
            dOutstanding += DB.readDouble(dr["TOTALOWING"], 0);
            count++;
        }

        if (count > 0) {
            DataRow newDR = dtPre.NewRow();
            newDR["TOTALSPENT"] = dSpent;
            newDR["TOTALPAID"] = dPaid;
            newDR["TOTALOWING"] = dOutstanding;
            newDR["% Prepaid"] = dSpent > 0 ? dPrePaid / dSpent : 0;
            newDR["ADDRESS"] = string.Format("<span style='width:100%; text-align:right; float:right;'>{0} properties, total :</span>", count);
            dtPre.Rows.Add(newDR);
        }

        gvPrePayment.DataSource = dtPre;
        gvPrePayment.DataBind();
        HTML.formatGridView(ref gvPrePayment, true);
    }

    protected string getSettlementDate(string szSettlementDate) {
        if (szSettlementDate == "" || Convert.ToDateTime(szSettlementDate) == DateTime.MinValue) {
            return "";
        }
        return Utility.formatDate(szSettlementDate);
    }

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "updateSale(" + szID + ")";
        }
    }

    protected void gvCampaign_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            string szActionID = ((DataRowView)e.Row.DataItem)["ACTIONID"].ToString();
            e.Row.Attributes["onclick"] = "updateCampaign(" + szID + ", " + szActionID + ")";
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
        showData();
    }

    public string getReportLink(string Month, string Year, string PayPeriodID) {
        if (Month.Contains("Total"))
            return Month;
        string szFile = Month.Substring(0, 3) + " " + Year.Substring(2, 2) + ".pdf";
        if (File.Exists(Path.Combine(G.User.getPDFDir(G.User.UserID), szFile))) {
            return String.Format("<a href='../PDF/{0}/{1}' target='_blank' title='Click to view PDF commission statement'>{2} {3}</a>", G.User.UserID, szFile, Month.Substring(0, 3), Year);
        } else {
            //Create a link to the report history
            return String.Format("<a href='../reports/commission_statement.aspx?szUserID={0}&szPayPeriod={1}'  target='_blank' title='Click to generate commission statement'>{2} {3}</a>", G.User.UserID, PayPeriodID, Month.Substring(0, 3), Year);
        }
    }
}