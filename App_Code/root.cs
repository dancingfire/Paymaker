using System;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Web.Security;
using System.Linq;

/// <summary>
/// Base page for the application. Contains code to:
/// </summary>
public class Root : System.Web.UI.Page, IDisposable {
    protected string szPageErrorMessage = "";
    protected string _szConfigurationErrorMessage = "";
    protected bool blnRestrictPageToAdmin = false;
    protected bool blnOuputUserID = false;
    protected bool blnUseSession = true;
    protected bool blnIsPageDirty = true;
    public bool blnIsRoot = false;
    public StringBuilder sbStartJS = new StringBuilder();
    public StringBuilder sbEndJS = new StringBuilder();
    public bool blnReadOnly = false;
    public bool blnShowMenu = true;

    // Set this to false for pages that are accessed without loggin in
    public bool blnLoggedInAccessOnly = true;

    // Unsure what this does
    public Root()
        : base() {
    }

    override protected void OnInit(EventArgs e) {
        // This stops browser from caching pages.
        // Clicking the back arrow on browser will result in reload of page
        // This increases security after logout
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        Response.Cache.SetNoStore();
        base.OnInit(e);
        G.oRoot = this;
    }

    override protected void OnLoad(EventArgs e) {
        /* SECURITY : DO NOT REMOVE
        * This code checks if user is logged in.  
        * This protects WebServices from being accessed by users not logged in. */
        int a;
        if (blnLoggedInAccessOnly)
            a = G.User.UserID;

        string szCurrScript = Request.ServerVariables["SCRIPT_NAME"];

        if (blnUseSession) {
            string  szPath = "..";

            if (blnIsRoot)
                szPath = "";
            string szTimeout = String.Format(@"
                var oSessionTimeout = window.setTimeout(""self.window.top.location='{0}/login.aspx?Timeout=true'"", {1});
                ", szPath, 240 * 60 * 1000 );
            ClientScript.RegisterStartupScript(this.GetType(), "Timeout", szTimeout, true);
        }

        base.OnLoad(e);

        if (Page.IsCallback) {
            blnIsPageDirty = Request.Form["HDIsPageDirty"].ToString() == "true";
        }
        if (blnOuputUserID) {
            sbEndJS.Append(string.Format(@"intUserID= {0};", G.User.ID));
        }

        // This sets authentication cookie to be secure (always encrypted)
        string authCookie = FormsAuthentication.FormsCookieName;
        if (Response.Cookies.AllKeys.Contains(authCookie)) {
            var httpCookie = Response.Cookies[authCookie];
            if (httpCookie != null)
                httpCookie.Secure = true;
        }
    }

    protected override void Render(HtmlTextWriter writer) {
        HTML.addJSLinks(this, blnIsRoot);

        if (blnRestrictPageToAdmin && G.User.RoleID != 2) {
            Response.Redirect("../welcome.aspx");
        }

        sbStartJS.AppendFormat(@"
            var blnReadOnly = {0};
            ", blnReadOnly.ToString().ToLower());

        if (szPageErrorMessage != "")
            ClientScript.RegisterStartupScript(this.GetType(), "Error", "alert('" + szPageErrorMessage + "');", true);
        if (sbEndJS.Length > 0)
            ClientScript.RegisterStartupScript(this.GetType(), "szEndJS", sbEndJS.ToString(), true);

        if (sbStartJS.Length > 0) {
            string szScript = "<script type='text/javascript'>" + sbStartJS.ToString() + "</script>";
            LiteralControl lblScript = new LiteralControl(szScript);
            Page.Header.Controls.Add(lblScript);
        }
        if (blnShowMenu) {
            ClientMenu oM = new ClientMenu();
            Form.Controls.AddAt(0, new LiteralControl(oM.createMenu()));
        }

        base.Render(writer);
    }

    public void addError(string szMessage) {
        if (szPageErrorMessage != "") {
            szPageErrorMessage = szPageErrorMessage + "\r\n" + szMessage;
        } else {
            szPageErrorMessage = szMessage;
        }
    }

    public bool blnDEBUG {
        get { return Convert.ToBoolean(Session["blnDEBUG"]); }
        set { Session["blnDEBUG"] = value; }
    }
}