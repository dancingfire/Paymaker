using System;
using System.Data;
using System.Linq;

public partial class api_log : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        loadData();
    }

    protected void loadData() {
        string szSQL = "";
       

        szSQL = String.Format(@"
            SELECT TOP 10 *
            FROM bdAPILOG L  
            ORDER BY ID DESC
            ");

        DataSet dsList = DB.runDataSet(szSQL);
        gvLog.DataSource = dsList;
        gvLog.DataBind();
        HTML.formatGridView(ref gvLog, true);
    }
}