using System;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Template admin
/// </summary>
public partial class template_detail : Root {
    private int intSelID = 0;

    protected void Page_Load(object sender, EventArgs e) {
        oBody.Attributes.Add("onload", "doLoad();");
        oBody.Attributes.Add("onbeforeunload", "return checkPageDirty();");

        if (!Page.IsPostBack) {
            intSelID = Valid.getInteger("intTemplateID", 0);
            loadTemplateList();
        } else {
            intSelID = Convert.ToInt32(lstSelTemplateID.SelectedValue);
        }
    }

    protected void loadTemplateList() {
        lstSelTemplateID.Items.Clear();

        Utility.BindList(ref lstSelTemplateID, DB.runReader("SELECT ID, NAME FROM TEMPLATE ORDER BY NAME"), "ID", "NAME");
        lstSelTemplateID.Items.Insert(0, new ListItem("Select a template...", "-1"));
    }

    protected void loadTemplate(bool blnShowOnly) {
        pTemplateUpdate.Visible = !blnShowOnly;
        if (intSelID > -1) {
            Template oT = new Template(intSelID);
            txtName.Text = oT.Name;
            txtDescription.Text = oT.Description;
            btnUpdate.Text = "Update";
            txtContent.Text = oT.TemplateHTML;
            btnDelete.Visible = true;
            if (intSelID == G.Settings.SalesLetterTemplateID)
                btnDelete.Enabled = false;
        } else {
            txtName.Text = "";
            txtDescription.Text = "";
            btnUpdate.Text = "Insert";
            btnDelete.Visible = false;
        }
    }

    protected void doUpdate() {
        if (Page.IsValid) {
            sqlUpdate oSQL = new sqlUpdate("TEMPLATE", "ID", intSelID);
            oSQL.add("NAME", txtName.Text);
            oSQL.add("DESCRIPTION", txtDescription.Text);
            oSQL.add("CONTENT", Request.Form["txtContent"]);

            if (intSelID == -1) {
                DB.runNonQuery(oSQL.createInsertSQL());
            } else {
                DB.runNonQuery(oSQL.createUpdateSQL());
            }
        }
        pMaster.Visible = true;
        pTemplateUpdate.Visible = false;
    }

    protected void lstSelTemplateID_SelectedIndexChanged(object sender, EventArgs e) {
        if (lstSelTemplateID.SelectedValue != "-1") {
            pMaster.Visible = false;
            loadTemplate(false);
        }
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        doUpdate();
        loadTemplate(true);
        loadTemplateList();
        lstSelTemplateID.SelectedValue = "-1";
        pMaster.Visible = true;
    }

    protected void btnDelete_Click(object sender, EventArgs e) {
        DB.runNonQuery("DELETE FROM TEMPLATE WHERE ID = " + intSelID);
        intSelID = -1;
        loadTemplateList();
        pTemplateUpdate.Visible = false;

        pMaster.Visible = true;
    }

    protected void blnCancel_Click(object sender, EventArgs e) {
        lstSelTemplateID.SelectedValue = "-1";
        pTemplateUpdate.Visible = false;
        pMaster.Visible = true;
    }

    protected void btnInsert_Click(object sender, EventArgs e) {
        intSelID = -1;
        lstSelTemplateID.SelectedValue = "-1";
        loadTemplate(false);
        pMaster.Visible = false;
    }
}