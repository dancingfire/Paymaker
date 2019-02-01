using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class holiday_update : Root {
    protected int intItemID = -1;
 
    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        blnShowMenu = false;

        intItemID = Valid.getInteger("intItemID");
        hdItemID.Value = intItemID.ToString();
        hdItemID.Value = intItemID.ToString();
        btnDelete.Enabled = false;
        if (!IsPostBack) {
            if (intItemID != -1)
                loadItem();
        }
    }

    private void loadItem() {
        string szSQL = String.Format("Select * from PUBLICHOLIDAY where ID = {0}", intItemID);
        DataSet dsItem = DB.runDataSet(szSQL);
        DataRow dr = dsItem.Tables[0].Rows[0];

        txtName.Text = dr["Name"].ToString();
        txtHolidayDate.Text = DB.readDateString(dr["HOLIDAYDATE"]);
        btnDelete.Enabled = DB.readDate(dr["HOLIDAYDATE"]) > DateTime.Now;
    }



    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        string szSQL;
        sqlUpdate oSQL = new sqlUpdate("PUBLICHOLIDAY", "ID", intItemID);
        oSQL.add("NAME", txtName.Text);
        oSQL.add("HOLIDAYDATE", txtHolidayDate.Text);
       
        if (intItemID == -1)
            szSQL = oSQL.createInsertSQL();
        else
            szSQL = oSQL.createUpdateSQL();
        int intID = Convert.ToInt32(DB.getScalar(szSQL));
     
        sbStartJS.Append("parent.refreshPage();");
    }

    private void doClose() {
        if (Valid.getBoolean("IsPopup", false))
            sbStartJS.Append(" parent.closeList(); ");
        else
            Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
                DELETE FROM PUBLICHOLIDAY
                WHERE ID = {0}", intItemID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.refreshPage();");
    }
}