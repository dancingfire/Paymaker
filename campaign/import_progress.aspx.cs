using System.Web.UI;

public partial class import_progress : Page {

    protected void Page_Load(object sender, System.EventArgs e) {
        getImportCount(-1);
    }

    public void getImportCount(int ImportID) {
        if (G.User.IsImportDone) {
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Close", "window.close()", true);
        } else {
            if (G.User.ImportTotal > -1)
                oProgress.InnerHtml = "Now processing " + G.User.ImportCurrentRecord + " of " + G.User.ImportTotal + " properties.";
        }
    }
}