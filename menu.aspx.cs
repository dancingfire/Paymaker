namespace Paymaker {

    /// <summary>
    /// Summary description for main.
    /// </summary>
    public partial class menu : Root {

        protected void Page_Init(object sender, System.EventArgs e) {
            blnIsRoot = true;
        }

        protected void Page_Load(object sender, System.EventArgs e) {
            Response.Redirect("login.aspx");
        }
    }
}