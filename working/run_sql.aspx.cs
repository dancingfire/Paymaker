using System;

public partial class run_sql : Root {

    protected void Page_Init(object sender, System.EventArgs e) {
        blnOuputUserID = true;
    }

    protected void Page_Load(object sender, System.EventArgs e) {

    }

    bool isSQLOK(string SQL) {
        return !SQL.Contains("Drop");
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        if (!Valid.getBoolean("GORD", false)) {
            return;
        }
        if (!isSQLOK(txtSQL.Text))
            return;
        try {
            Utility.bindGV(ref gvOutput, DB.runDataSet(txtSQL.Text));
        } catch (Exception ex) {
            txtSQL.Text = ex.Message;
        }
    }
}
