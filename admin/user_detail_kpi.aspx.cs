using System;
using System.Data;

public partial class user_detail_kpi : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsTest;

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            rrList.DataSource = getData(); ;
            rrList.DataBind();
        }
    }

    protected DataSet getData() {
        string szSQL = string.Format(@"
            SELECT U.ID, U.INITIALSCODE + ' ' + U.FIRSTNAME + ' ' + U.LASTNAME AS NAME, R.NAME AS ROLE,
                PROFILEVIDEODATE, SHOWONKPIREPORT
            FROM DB_USER U
            JOIN ROLE R ON U.ROLEID = R.ID
            WHERE U.ISDELETED = 0 AND U.ISACTIVE  = 1
            ORDER BY U.ISACTIVE DESC, U.INITIALSCODE");
        return DB.runDataSet(szSQL);
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        using (DataSet ds = getData()) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                int ID = DB.readInt(dr["ID"]);
                DateTime dtVideo = Valid.getDate("txtVIDEO_" + ID, DateTime.MinValue);
                bool blnShowKPI = Valid.getCheck("chkSHOWONREPORT_" + ID);

                DB.runNonQuery(String.Format("UPDATE DB_USER SET PROFILEVIDEODATE = {0}, SHOWONKPIREPORT = {1} WHERE ID = {2}",
                    dtVideo == DateTime.MinValue ? "null" : "'" + Utility.formatDate(dtVideo) + "'", blnShowKPI ? "1" : "0", ID));
            }
        }
        rrList.DataSource = getData(); ;
        rrList.DataBind();
    }
}