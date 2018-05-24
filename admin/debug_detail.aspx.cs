using System.Data;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for test_detail.
/// </summary>
public partial class debug_detail : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsTest;

    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        if (!Page.IsPostBack)
            doBindData();
    }

    private void doBindData() {
        try {
            string szSQL = "SELECT * from DEBUG ";
            dsTest = DB.runDataSet(szSQL);
            this.dlList.DataSource = dsTest;
            this.dlList.DataBind();
        } catch (System.Exception eLoad) {
            this.Response.Write(eLoad.Message);
        }
    }

    protected void btnInsert_Click(object sender, System.EventArgs e) {
        string szSQL = "DELETE from DEBUG";
        DB.runNonQuery(szSQL);
    }

    protected void btnReload_Click(object sender, System.EventArgs e) {
        doBindData();
    }
}