using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class templated_tx : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack) {
        }
        showData();
    }

    private void showData() {
        string szSQL = @"
            SELECT TX.*, L.NAME AS ACCOUNT,LASTNAME + ', ' +  FIRSTNAME as [USER], C.NAME AS CATEGORY, '' AS ACTION
            FROM USERTX TX
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN DB_USER U ON U.ID = TX.USERID AND TX.ISDELETED = 0
            LEFT JOIN LIST C ON C.ID = TX.TXCATEGORYID
            WHERE TX.ISTEMPLATE = 1 AND TX.ISDELETED = 0
            ORDER BY [USER], TX.TXDATE DESC";

        using( DataSet ds = DB.runDataSet(szSQL)){
            foreach(DataRow dr in ds.Tables[0].Rows) {
                dr["ACTION"] = String.Format("<a id='tag{0}' href='javascript: removeTemplate({0})'>Remove</a>", dr["ID"]);
            }
            gvTXs.DataSource = ds;
            gvTXs.DataBind();
            HTML.formatGridView(ref gvTXs, true);
        }

       
    }

  
    protected void btnRefresh_Click(object sender, EventArgs e) {
    }
}