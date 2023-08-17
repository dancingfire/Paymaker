using System.Data;

public partial class apilog_detail : Root
{
   
    protected void Page_Load(object sender, System.EventArgs e) {
        initPage();
    }

    private void initPage() {
        DataSet ds = DB.runDataSet(@"
               SELECT TOP 250 *
                 FROM bdAPILOG A
                 ORDER BY ID DESC
                 ");
        gvList.DataSource = ds;
        gvList.DataBind();
        HTML.formatGridView(ref gvList, true);
    }
}