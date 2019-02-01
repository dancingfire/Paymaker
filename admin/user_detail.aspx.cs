using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class user_detail : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsTest;

    protected void Page_Load(object sender, System.EventArgs e) {
        loadData();
    }
    
    protected void loadData() {
        string szInactive = " AND U.ISACTIVE = 1 ";
        if (chkViewInactive.Checked)
            szInactive = "";
        string szSQL = string.Format(@"
            SELECT U.ID, U.INITIALSCODE + ' ' + U.FIRSTNAME + ' ' + U.LASTNAME AS NAME, R.NAME AS ROLE,
                CASE WHEN U.ISACTIVE = 0 THEN 'Inactive' ELSE 'Active' END AS STATUS, U.ISACTIVE, T.FIRSTNAME + ' ' + T.LASTNAME AS TEAM,
                LEFT(O.NAME, 2) AS OFFICE, CASE WHEN S.ID = 0 THEN '' ELSE S.FIRSTNAME + ' ' + S.LASTNAME END AS SUPERVISOR
            FROM DB_USER U
            JOIN ROLE R ON U.ROLEID = R.ID
            LEFT JOIN DB_USER T ON T.ID = U.TEAMID
            LEFT JOIN LIST O ON U.OFFICEID = O.ID
            LEFT JOIN DB_USER S ON U.SUPERVISORID = S.ID 
            
            WHERE U.ISDELETED = 0 {0}
            ORDER BY U.ISACTIVE DESC, U.INITIALSCODE", szInactive);
        dsTest = DB.runDataSet(szSQL);
        gvList.DataSource = dsTest;
        gvList.DataBind();

        HTML.formatGridView(ref gvList, true);
    }

    protected void chkIncludeInactive_CheckedChanged(object sender, EventArgs e) {
        loadData();
    }

    protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            bool blnIsactive = Convert.ToBoolean(((DataRowView)e.Row.DataItem)["ISACTIVE"]);

            e.Row.Attributes["onclick"] = "viewUser(" + szID + ")";
            if (!blnIsactive)
                e.Row.CssClass = e.Row.CssClass + " Inactive";
        }
    }
}