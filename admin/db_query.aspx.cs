using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;

public partial class db_query : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
    }

    protected void btnRun_Click(object sender, EventArgs e) {
        gvResults.Visible = false;
        try {
            DataSet ds = DB.runDataSet(txtSQL.Text);
            gvResults.DataSource = ds;
            gvResults.DataBind();
            HTML.formatGridView(ref gvResults, true);
            gvResults.Visible = true;
        } catch (Exception e1) {
            G.Notifications.addPageNotification(PageNotificationType.Error, "SQL Error", e1.Message);
            G.Notifications.showPageNotification();
        }
    }

    protected void btnRunREOffice_Click(object sender, EventArgs e) {
        gvResults.Visible = false;
        try {
            string szCNN = ConfigurationManager.AppSettings["REOFFICE"];
            OdbcDataAdapter da = new OdbcDataAdapter(txtSQL.Text, szCNN);
            DataSet ds = new DataSet();
            da.Fill(ds, "emp");

            gvResults.DataSource = ds;
            gvResults.DataBind();
            HTML.formatGridView(ref gvResults, true);
            gvResults.Visible = true;
        } catch (Exception e1) {
            G.Notifications.addPageNotification(PageNotificationType.Error, "SQL Error", e1.Message);
            G.Notifications.showPageNotification();
        }
    }
}