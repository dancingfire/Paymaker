using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class product_update : Root {
    protected int intItemID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        blnShowMenu = false;

        intItemID = Valid.getInteger("intItemID");
        hdItemID.Value = intItemID.ToString();
        if (!IsPostBack) {
            if (intItemID != -1)
                loadItem();
        }
    }

    private void loadItem() {
        string szSQL = String.Format("Select * from PRODUCT where ID = {0}", intItemID);
        DataSet dsItem = DB.runDataSet(szSQL);
        DataRow dr = dsItem.Tables[0].Rows[0];

        txtName.Text = dr["DESCRIPTION"].ToString();
        Utility.BindList(ref lstGLCode, DB.runDataSet(String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0} ORDER BY NAME", (int)(ListType.CampaignGL))), "ID", "NAME");
        lstGLCode.Items.Insert(0, new ListItem("Select a GL code...", "-1"));
        Utility.setListBoxItems(ref lstGLCode, dr["CREDITGLCode"].ToString());
        chkExcludeFromInvoicing.Checked = Convert.ToBoolean(dr["EXCLUDEFROMINVOICE"]);
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        sqlUpdate oSQL = new sqlUpdate("PRODUCT", "ID", intItemID);
        oSQL.add("DESCRIPTION", txtName.Text);
        oSQL.add("CREDITGLCODE", lstGLCode.SelectedValue);
        oSQL.add("EXCLUDEFROMINVOICE", chkExcludeFromInvoicing);

        DB.runNonQuery(oSQL.createUpdateSQL());
        sbEndJS.Append("parent.refreshPage();");
    }

    private void doClose() {
        Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
                DELETE FROM LIST
                WHERE ID = {0}", intItemID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.refreshPage();");
    }
}