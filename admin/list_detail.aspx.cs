using System.Data;
using System.Web.UI.WebControls;

public partial class list_detail : Root {
    private ListType oListType = ListType.Office;

    protected void Page_Load(object sender, System.EventArgs e) {
        oListType = (ListType)Valid.getInteger("intListTypeID");
        sbStartJS.Append("var intListTypeID = " + (int)oListType);
        if (!IsPostBack) {
            initPage();
            loadData();
        }
    }

    private void initPage() {
        switch (oListType) {
            case ListType.Commission:
                lblItemName.Text = "Commision items";
                lblInsertText.Text = "Add new commission item";
                break;

            case ListType.Company:
                lblItemName.Text = "Companies";
                lblInsertText.Text = "Add new company";
                break;

            case ListType.Expense:
                lblItemName.Text = "Expense categories";
                lblInsertText.Text = "Add new expense category";
                break;

            case ListType.Income:
                lblItemName.Text = "Income categories";
                lblInsertText.Text = "Add new income category";
                break;

            case ListType.Office:
                lblItemName.Text = "Offices";
                lblInsertText.Text = "Add new office";
                break;

            case ListType.OffTheTop:
                lblItemName.Text = "Off the top sales expenses";
                lblInsertText.Text = "Add new category";
                break;

            case ListType.CampaignGL:
                lblItemName.Text = "Campaign GL codes";
                lblInsertText.Text = "Add new GL code";
                break;

            case ListType.TXCategory:
                lblItemName.Text = "TX Categories";
                lblInsertText.Text = "Add new TX category";
                break;
        }
    }

    protected void loadData() {
        string szSQL = string.Format(@"
            SELECT *, case when ISACTIVE = 1 then 'Y' else 'N' END as status
            FROM LIST
            WHERE LISTTYPEID = {0}
            ORDER BY SEQUENCENO", (int)oListType);
        DataSet dsList = DB.runDataSet(szSQL);
        gvList.DataSource = dsList;
        gvList.DataBind();
        HTML.formatGridView(ref gvList, true);
    }

    protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();

            e.Row.Attributes["onclick"] = "viewItem(" + szID + ")";
        }
    }
}