namespace Paymaker {

    public partial class welcome : Root {
        protected void Page_Init(object sender, System.EventArgs e) {
\            blnIsRoot = true;
        }

        protected void Page_Load(object sender, System.EventArgs e) {
            dWelcome.InnerHtml = "Welcome  " + G.User.UserName;
        }
    }
}