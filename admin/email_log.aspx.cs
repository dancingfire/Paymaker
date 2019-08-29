using System;
using System.Data;
using System.Linq;

public partial class email_log : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        loadData();
    }

    protected void loadData() {
        string szSQL = "";
        string szWhere = "";
        if (txtSearch.Text != "") {
            szWhere = " AND L.BODY LIKE '%" + DB.escape(txtSearch.Text) + "%' ";
        }
        if (txtToFilter.Text != "") {
            szWhere = " AND L.SENTTO LIKE '%" + DB.escape(txtToFilter.Text) + "%' ";
        }
        szSQL = String.Format(@"
            SELECT TOP 500 *
            FROM EMAILLOG L  
            WHERE 1 = 1 
            {0}
            ORDER BY SENTDATE DESC
            ", szWhere);

        DataSet dsList = DB.runDataSet(szSQL);
        gvLog.DataSource = dsList;
        gvLog.DataBind();
        HTML.formatGridView(ref gvLog, true);
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
        loadData();
    }
}