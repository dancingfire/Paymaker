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
        if (!isSQLOK(txtSQL.Text))
            return;
        Utility.bindGV(ref gvOutput, DB.runDataSet(txtSQL.Text));
    }

    protected void btnUpdateBD_Click(object sender, EventArgs e) {
        if (!isSQLOK(txtSQL.Text))
            return;
        Utility.bindGV(ref gvOutput, DB.runDataSet(txtSQL.Text, DB.BoxDiceDBConn));
    }
}