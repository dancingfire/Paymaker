using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for test_detail.
/// </summary>
public partial class user_settings_detail : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsSettings;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        if (!IsPostBack) {
            loadSettings();
        }
    }

    private void loadSettings() {
        string szSQL = "SELECT * from CONFIG WHERE USERID =" + G.User.ID.ToString();
        SqlDataReader drSettings = DB.runReader(szSQL);
        while (drSettings.Read()) {
            if (drSettings["NAME"].ToString() == "CANCELSAMPLEENTRYPRINTING")
                lstCancelEntryPrinting.SelectedValue = drSettings["VALUE"].ToString();
            else if (drSettings["NAME"].ToString() == "PROMPTBEFOREPRINTINGLABELS")
                lstPromptBeforePrintingLabels.SelectedValue = drSettings["VALUE"].ToString();
            if (drSettings["NAME"].ToString() == "SIGNATURE") {
                txtSignature.Text = drSettings["VALUE"].ToString();
            }
            if (drSettings["NAME"].ToString() == "STYLESHEET") {
                lstLookAndFeel.SelectedValue = drSettings["VALUE"].ToString();
            }
        }
    }

    private void doUpdateConfig(string szName, string szValue) {
        string szSQL = "SELECT * FROM CONFIG WHERE NAME = '" + szName + "' and USERID = " + G.User.ID.ToString();
        SqlDataReader drSettings = DB.runReader(szSQL);
        if (drSettings.HasRows)
            szSQL = "update config set value = '" + szValue + "' WHERE NAME = '" + szName + "' and USERID = " + G.User.ID.ToString();
        else
            szSQL = "INSERT INTO CONFIG(NAME, VALUE, USERID) VALUES('" + szName + "', '" + szValue + "', " + G.User.ID.ToString() + ")";
        DB.runNonQuery(szSQL);
        Session[szName] = szValue;
    }

    protected void btnClose_Click(object sender, System.EventArgs e) {
        Response.Redirect("../welcome.aspx");
    }

    protected void btnInsert_Click(object sender, System.EventArgs e) {
        doUpdateConfig("CANCELSAMPLEENTRYPRINTING", lstCancelEntryPrinting.SelectedValue);
        doUpdateConfig("PROMPTBEFOREPRINTINGLABELS", lstPromptBeforePrintingLabels.SelectedValue);
        doUpdateConfig("STYLESHEET", lstLookAndFeel.SelectedValue);
        doUpdateConfig("SIGNATURE", txtSignature.Text);
    }
}