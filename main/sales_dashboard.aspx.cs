using Sentry.Protocol;
using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sales_dashboard : Root {
    private int CurrentUserID;

    protected void Page_Load(object sender, System.EventArgs e) {
        CurrentUserID = G.User.ID;
        if (!Page.IsPostBack) {
            Utility.loadPayPeriodList(ref lstPayPeriod);
            lstPayPeriod.SelectedValue = G.CurrentPayPeriod.ToString();
        } 
        showData();
    }

    private void showData() {
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Sale", "Sale", "95%", -1, "sale_update.aspx")));
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Campaign", "Campaign", "95%", 600, "../campaign/campaign_update.aspx")));
        PayPeriod oPP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(lstPayPeriod.SelectedValue));
        //Load the current sales for this
        gvCurrent.DataSource = Sale.loadSalesForSalesPerson(CurrentUserID, oPP.StartDate, oPP.EndDate);
        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent, true);
        pNoDataCurrent.Visible = pNoFutureSales.Visible = false;
        if (gvCurrent.Rows.Count == 0) {
            gvCurrent.Visible = false;
            pNoDataCurrent.Visible = true;
        } 
          
        //Load the pending sales for this agent
        gvFuture.DataSource = Sale.loadSalesForSalesPerson(CurrentUserID, oPP.EndDate, DateTime.MaxValue);
        dPendingHeader.InnerHtml = "Pending Commissions - After " + Utility.formatDate(oPP.EndDate);
        gvFuture.DataBind();
        HTML.formatGridView(ref gvFuture, true);
        if (gvFuture.Rows.Count == 0) {
            gvFuture.Visible = false;
            pNoFutureSales.Visible = true;
        }

      
        //Load the summary sales for this agent
        DataSet dsHistorical = Sale.loadYTDSalesForSalesPerson(CurrentUserID, Utility.getFinYearStart(oPP.EndDate));
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

        loadPendingPayments(oPP);
    }

    void loadPendingPayments( PayPeriod oPP) {
        //Load the pending refund transactions for this agent
        string szSQL = String.Format(@"
            SELECT T.AMOUNT, T.TxDate, A.NAME AS ACCOUNT, T.COMMENT
            FROM USERTX T JOIN LIST A ON A.id = T.ACCOUNTID 
            WHERE TXDATE > '{1}' AND USERID = {0} ", CurrentUserID, Utility.formatDate(oPP.EndDate));
        gvPayments.DataSource = DB.runDataSet(szSQL);
        dvPayments.InnerHtml = "Pending payments - After " + Utility.formatDate(oPP.EndDate);
        gvPayments.DataBind();
        HTML.formatGridView(ref gvPayments, true);
        if (gvPayments.Rows.Count == 0) {
            gvPayments.Visible = false;
            pNoPayments.Visible = true;
        }
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
            return String.Format("<a href='../reports/commission_statement_new.aspx?szUserID={0}&szPayPeriod={1}'  target='_blank' title='Click to generate commission statement'>{2} {3}</a>", G.User.UserID, PayPeriodID, Month.Substring(0, 3), Year);
        }
    }

    protected void lstPayPeriod_SelectedIndexChanged(object sender, EventArgs e) {
        
    }
}