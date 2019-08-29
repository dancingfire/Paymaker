using System;
using System.Data;

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
}