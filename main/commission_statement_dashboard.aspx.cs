using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class commission_statement_dashboard : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack) {
            Utility.BindList(ref lstAgent, DB.runDataSet(@"
                SELECT ID, LASTNAME + ', ' + FIRSTNAME AS NAME FROM DB_USER WHERE ISACTIVE = 1 AND ISDELETED = 0 ORDER BY LASTNAME, FIRSTNAME"), "ID", "NAME");

            lstAgent.Items.Insert(0, new ListItem("Select an agent...", "-1"));
        }
    }

    private void showData() {
        //Load the summary sales for this agent
        DataSet dsHistorical = Sale.loadYTDSalesForSalesPerson(Convert.ToInt32(lstAgent.SelectedValue), DateTime.Parse("May 1, 2015"));

        gvHistory.DataSource = dsHistorical;
        gvHistory.DataBind();
        HTML.formatGridView(ref gvHistory, true);
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
    }

    public string getReportLink(string Month, string Year, string PayPeriodID) {
        string szFile = Month.Substring(0, 3) + " " + Year.Substring(2, 2) + ".pdf";
        if (File.Exists(Path.Combine(HttpContext.Current.Server.MapPath("../PDF/" + lstAgent.SelectedValue), szFile))) {
            return String.Format("<a href='../PDF/{0}/{1}' target='_blank' title='Click to view PDF commission statement'>{2} {3}</a>", Convert.ToInt32(lstAgent.SelectedValue), szFile, Month.Substring(0, 3), Year);
        }
        return Month;
    }

    protected void lstAgent_SelectedIndexChanged(object sender, EventArgs e) {
        showData();
    }
}