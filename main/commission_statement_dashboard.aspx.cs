using System;
using System.Data;
using System.IO;
using System.Web.UI;

public partial class commission_statement_dashboard : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack) {
            G.UserInfo.loadCommissionList(lstAgent);
        }
    }

    private void showData() {
        //Load the summary sales for this agent
        DataSet dsHistorical = Sale.loadYTDSalesForSalesPerson(Convert.ToInt32(lstAgent.SelectedValue), DateTime.Parse("May 1, 2015"), "DESC");

        gvHistory.DataSource = dsHistorical;
        gvHistory.DataBind();
        HTML.formatGridView(ref gvHistory, true);
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
    }

    public string getReportLink(string Month, string Year, string PayPeriodID) {
        string szFile = Month.Substring(0, 3) + " " + Year.Substring(2, 2) + ".pdf";
        string szDir = G.User.getPDFDir(Convert.ToInt32(lstAgent.SelectedValue));

        if (File.Exists(Path.Combine(szDir, szFile))) {
            return String.Format("<a href='../PDF/{0}/{1}' target='_blank' title='Click to view PDF commission statement'>{2} {3}</a>", lstAgent.SelectedValue, szFile, Month.Substring(0, 3), Year);
        }
        return Month.Substring(0, 3) + " " + Year;
    }

    protected void lstAgent_SelectedIndexChanged(object sender, EventArgs e) {
        showData();
    }
}