using System;

public partial class run_sql : Root {

    protected void Page_Init(object sender, System.EventArgs e) {
        blnOuputUserID = true;
    }

    protected void Page_Load(object sender, System.EventArgs e) {
        throw new Exception("This is only a test - please ignor!! :-)");
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        DB.runNonQuery(txtSQL.Text);
    }
}