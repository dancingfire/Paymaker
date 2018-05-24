using System.Data;
using System.Web.UI.WebControls;

public partial class product_detail : Root {
    private ListType oListType = ListType.Office;

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            lblItemName.Text = "Campaign products";
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
        string szFilter = "";
        if (Valid.getText("NOTSET", "", VT.TextNormal) == "true") {
            szFilter = " WHERE L_CREDIT.ID IS NULL";
            gvCurrent.EmptyDataText = "All products now have GL codes. <a href='../campaign/myob_export.aspx'>return to export.</a>";
        }

        string szSQL = string.Format(@"
            SELECT P.ID,  P.DESCRIPTION AS NAME, ISNULL(L_CREDIT.NAME, 'Not set') AS CREDITGLCODE, CASE EXCLUDEFROMINVOICE WHEN 0 THEN 'No' ELSE 'Yes' END AS EXCLUDEFROMINVOICE
            FROM PRODUCT P
            LEFT JOIN LIST L_CREDIT ON P.CREDITGLCODE = L_CREDIT.ID {1}
            ORDER BY P.DESCRIPTION ", (int)oListType, szFilter);
        DataSet dsList = DB.runDataSet(szSQL);
        gvCurrent.DataSource = dsList;
        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent, true);
    }
}