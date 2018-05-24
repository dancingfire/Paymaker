using System.Data;
using System.Web.UI.WebControls;

public partial class action_detail : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            lblItemName.Text = "Campaign actions";
            loadData();
        }
    }

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "viewItem(" + szID + ")";
        }
    }

    protected void loadData() {
        string szSQL = @"
            SELECT ID, NAME
            FROM ACTION
            ORDER BY NAME ";
        DataSet dsList = DB.runDataSet(szSQL);
        dlList.DataSource = dsList;
        dlList.DataBind();
    }
}