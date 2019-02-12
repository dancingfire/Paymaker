using System;
using System.Data;
using System.Linq;

public partial class logv2_detail : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
          
        }
    }

    protected void loadData() {
        string szSQL = "";
        string szWhere = "";
        if(txtSearch.Text != "") {
            szWhere = " WHERE L.VALUE LIKE '%" + DB.escape(txtSearch.Text) + "%' ";
        }
        szSQL = String.Format(@"
            SELECT TOP 500 *, U.FIRSTNAME + ' ' + U.LASTNAME AS PERSON
            FROM LOGV2 L JOIN DB_USER U ON L.USERID = U.ID      
            {0}
            ORDER BY CHANGEDATE DESC
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