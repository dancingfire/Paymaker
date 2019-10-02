using Paymaker;
using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class user_delegation : Root {

    protected void Page_Init(object sender, System.EventArgs e) {
        blnShowMenu = false;
    }

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            loadPage();
        }
    }
    private void loadPage() {
        G.UserInfo.loadList(ref lstUser, true);

        //Load what I've delegated
        string szSQL = String.Format(@"
            SELECT UD.*, U.FIRSTNAME + ' ' + U.LASTNAME + ' (' + U.INITIALSCODE + ')' AS NAME 
            FROM USERDELEGATION UD JOIN DB_USER U ON UD.DELEGATIONUSERID = U.ID
            WHERE RECORDSTATUS = 1 AND USERID = {0} AND UD.ENDDATE >= getdate()
            ORDER BY U.FIRSTNAME, U.LASTNAME;", G.User.UserID);

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
        ud.UserID = G.User.UserID;
        ud.DelegationUserID = Convert.ToInt32(lstUser.SelectedValue);
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
            e.Row.Attributes["onclick"] = String.Format(@"updateDelegation({0}, {1}, {2}, '{3}', '{4}', {5});",
                DB.readInt(v["ID"]), DB.readInt(v["DELEGATIONTYPEID"]), DB.readInt(v["DELEGATIONUSERID"]), DB.readDateToString(v["STARTDATE"]), DB.readDateToString(v["EndDATE"]), DB.readInt(v["SENDTOORIGUSER"]));
        }
    }
}