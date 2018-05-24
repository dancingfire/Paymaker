using Microsoft.ApplicationBlocks.Data;
using System;
using System.Data;

/// <summary>
/// Summary description for Report.
/// </summary>
public partial class ErrorLogReport : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        doLoadData();
    }

    protected void doLoadData() {
        string szEventID = "";

        if (Request.QueryString["EvtId"] != null)
            if (Request.QueryString["EvtId"] != null)
                szEventID = Request.QueryString["EvtId"].ToString();
        string szSQL = "SELECT * FROM  RWASPERRORLOG WHERE 1=1";
        if (!chkShowAll.Checked)
            szSQL += " AND RECORDSTATUS = 0 ";
        if (szEventID != "")
            szSQL += "AND EventId = " + szEventID;
        szSQL += " ORDER BY EVENTID DESC";

        DataSet dr = SqlHelper.ExecuteDataset(DB.DBConn, CommandType.Text, szSQL);

        oList.DataSource = dr;
        oList.DataBind();
    }

    protected void btnMarkAsRead_Click(object sender, EventArgs e) {
        string szSQL = "UPDATE RWASPERRORLOG SET RECORDSTATUS = 1 WHERE RECORDSTATUS = 0";
        SqlHelper.ExecuteNonQuery(DB.DBConn, CommandType.Text, szSQL);
        doLoadData();
    }

    protected void chkShowAll_CheckedChanged(object sender, EventArgs e) {
        doLoadData();
    }

    protected string doFormatForm(string szValue) {
        if (szValue == "" || szValue == null)
            return szValue;
        string szReturn = "";
        string[] arValues = szValue.Split(Convert.ToChar("&"));
        foreach (string szVar in arValues) {
            szReturn += Server.UrlDecode(szVar) + "<br/>";
        }
        return szReturn;
    }

    protected void blnDeleteAll_Click(object sender, EventArgs e) {
        string szSQL = "truncate table RWASPERRORLOG ";
        SqlHelper.ExecuteNonQuery(DB.DBConn, CommandType.Text, szSQL);
        doLoadData();
    }
}