using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class action_update : Root {
    protected int intItemID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        intItemID = Valid.getInteger("intItemID");
        hdItemID.Value = intItemID.ToString();
        if (!IsPostBack) {
            loadItem();
        }
    }

    private void loadItem() {
        DataSet dsTemplate = DB.runDataSet("SELECT * FROM TEMPLATE ORDER BY NAME");
        Utility.BindList(ref lstTemplate, dsTemplate, "ID", "NAME");
        lstTemplate.Items.Insert(0, new ListItem("Select a template...", ""));

        if (intItemID > -1) {
            string szSQL = String.Format("Select *, ISNULL(TEMPLATEID, -1) AS SAFETEMPLATEID from ACTION where ID = {0}", intItemID);
            DataSet dsItem = DB.runDataSet(szSQL);
            DataRow dr = dsItem.Tables[0].Rows[0];
            txtName.Text = dr["NAME"].ToString();
            txtDays.Text = dr["DEFAULTREMINDERDAYS"].ToString();
            txtEmailSubject.Text = dr["EMAILSUBJECT"].ToString();
            chkActive.Checked = Convert.ToBoolean(dr["ISACTIVE"]);
            Utility.setListBoxItems(ref lstTemplate, Convert.ToString(dr["SAFETEMPLATEID"]));

            szSQL = "SELECT COUNT(*) FROM CAMPAIGNNOTE WHERE ACTIONID = " + intItemID;
            btnDelete.Visible = DB.getScalar(szSQL, -1) <= 0;
        }
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        sqlUpdate oSQL = new sqlUpdate("ACTION", "ID", intItemID);
        oSQL.add("NAME", txtName.Text);
        oSQL.add("ISACTIVE", chkActive);
        oSQL.add("EMAILSUBJECT", txtEmailSubject.Text);
        if (lstTemplate.SelectedValue == "-1")
            oSQL.addNull("TEMPLATEID");
        else
            oSQL.add("TEMPLATEID", lstTemplate.SelectedValue);
        if (Utility.IsNumeric(txtDays.Text))
            oSQL.add("DEFAULTREMINDERDAYS", Convert.ToInt32(txtDays.Text));
        else
            oSQL.add("DEFAULTREMINDERDAYS", 0);

        if (intItemID > -1) {
            DB.runNonQuery(oSQL.createUpdateSQL());
        } else {
            DB.runNonQuery(oSQL.createInsertSQL());
        }
        sbEndJS.Append("parent.refreshPage();");
    }

    private void closeWindow() {
        Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        closeWindow();
    }

    protected void btnDelete_Click(object sender, EventArgs e) {
        string szSQL = "DELETE FROM ACTION WHERE ID = " + intItemID;
        DB.runNonQuery(szSQL);
        sbEndJS.Append("parent.refreshPage();");
    }
}