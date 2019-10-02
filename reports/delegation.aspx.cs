using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class delegation : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsTest;

    protected void Page_Load(object sender, System.EventArgs e) {
        loadData();
    }
    
    protected void loadData() {
        string szSQL = string.Format(@"
            SELECT UD.*, U.FIRSTNAME + ' ' + U.LASTNAME + ' (' + U.INITIALSCODE + ')' AS NAME,
            D.FIRSTNAME + ' ' + D.LASTNAME + ' (' + D.INITIALSCODE + ')' AS DELEGATEDTO 
            FROM USERDELEGATION UD JOIN DB_USER D ON UD.DELEGATIONUSERID = D.ID
            JOIN DB_USER U ON UD.DELEGATIONUSERID = U.ID
            WHERE RECORDSTATUS = 1 AND UD.STARTDATE >= dateadd(year, -1, getdate())
            ORDER BY U.FIRSTNAME, U.LASTNAME;");
        dsTest = DB.runDataSet(szSQL);
        gvList.DataSource = dsTest;
        gvList.DataBind();

        HTML.formatGridView(ref gvList, true, true);
    }


    protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
           
            //e.Row.Attributes["onclick"] = "viewUser(" + szID + ")";
        }
    }
}