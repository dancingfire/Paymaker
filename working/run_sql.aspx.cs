using System;

public partial class run_sql : Root {

    protected void Page_Init(object sender, System.EventArgs e) {
        blnOuputUserID = true;
    }

    protected void Page_Load(object sender, System.EventArgs e) {
       
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        Utility.bindGV(ref gvOutput, DB.runDataSet(txtSQL.Text));
    }
}