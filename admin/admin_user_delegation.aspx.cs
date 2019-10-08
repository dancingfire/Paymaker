using Paymaker;
using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class admin_user_delegation : Root {

    protected void Page_Init(object sender, System.EventArgs e) {
        blnShowMenu = false;
    }

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            loadPage();
        }
    }
    private void loadPage() {
        if (!G.User.IsAdmin )
            throw new Exception("Non-Admin user attempting to access admin delegation");

        G.UserInfo.loadList(ref lstAwayUser, true);
        G.UserInfo.loadList(ref lstDelegatedToUser, true);

        //Load what I've delegated
        string szSQL = String.Format(@"
            SELECT UD.*, U_DEL.FIRSTNAME + ' ' + U_DEL.LASTNAME + ' (' + U_DEL.INITIALSCODE + ')' AS DELEGATEDNAME ,
                U_AWAY.FIRSTNAME + ' ' + U_AWAY.LASTNAME + ' (' + U_AWAY.INITIALSCODE + ')' AS AWAYNAME 
            FROM USERDELEGATION UD JOIN DB_USER U_DEL ON UD.DELEGATIONUSERID = U_DEL.ID 
            JOIN DB_USER U_AWAY ON UD.USERID = U_AWAY.ID 
            WHERE RECORDSTATUS = 1 AND UD.ENDDATE >= getdate()
            ORDER BY UD.ENDDATE DESC");

        using (DataSet ds = DB.runDataSet(szSQL)) {
            gvList.DataSource = ds;
            gvList.DataBind();
            HTML.formatGridView(ref gvList, true);
        }

        string szDelegations = "";
        foreach (UserDelegate d in G.UserDelegateInfo.DelegateList) {
            if(d.DelegationUserID == G.User.UserID) {
                string szInfo = String.Format("{0} has delegated to you from {1} to {2}", G.UserInfo.getName(d.UserID), Utility.formatDate(d.StartDate), Utility.formatDate(d.EndDate));
                Utility.Append(ref szDelegations, szInfo, "<br/>");
            }
        }
        if(szDelegations == "") {
            lblCurrDelegations.Visible = false;
        } else {
            lblCurrDelegations.Text = szDelegations; 
        }

    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        UserDelegate ud = new UserDelegate(Convert.ToInt32(hdDBID.Value));
        ud.StartDate = Convert.ToDateTime(txtStartDate.Text);
        ud.EndDate = Convert.ToDateTime(txtEndDate.Text);
        ud.UserID = Convert.ToInt32(lstAwayUser.SelectedValue);
        ud.DelegationUserID = Convert.ToInt32(lstDelegatedToUser.SelectedValue);
        ud.Type = DelegationType.EmailAndLogin;
        ud.SendEmailOrigUser = Convert.ToInt32(lstReceiveEmails.SelectedValue) == 1;
        ud.RecordStatus = RecordStatus.Active;
        ud.updateToDB();
        G.UserDelegateInfo.forceReload();
        loadPage();
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
                UPDATE USERDELEGATION SET RECORDSTATUS = 2
                WHERE ID = {0}", hdDBID.Value);
        DB.runNonQuery(szSQL);
        DBLog.addGenericRecord(DBLogType.User, "Delegation was deleted", G.User.UserID, Convert.ToInt32(hdDBID.Value));
        G.UserDelegateInfo.forceReload();
        loadPage();
    }

    public string getDelImage(int ID) {
        return "<img src='../sys_images/delete.gif' height='16' onclick=' confirmDelete(" + ID + ");' /> ";
    }

    protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            DataRowView v = (DataRowView)e.Row.DataItem;

            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = String.Format(@"updateDelegation({0}, {1}, {2}, {3}, '{4}', '{5}', {6});",
                DB.readInt(v["ID"]), DB.readInt(v["DELEGATIONTYPEID"]), DB.readInt(v["USERID"]), DB.readInt(v["DELEGATIONUSERID"]), DB.readDateToString(v["STARTDATE"]), 
                DB.readDateToString(v["EndDATE"]), DB.readInt(v["SENDTOORIGUSER"]));
        }
    }
}