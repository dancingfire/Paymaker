using System;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

public partial class request_update : Root {
    protected int intID = -1;
    LeaveRequest l = null;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
      
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        intID = Valid.getInteger("id", -1);
        hdTXID.Value = intID.ToString();
        if (!IsPostBack) {
            bindData();
            if (intID == -1)
                loadDefaults();
            else
                loadRequest();
            checkSupervisorSettings();
        }
    }

    private void bindData() {
       
        string szSQL = String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0} ORDER BY SEQUENCENO, NAME", (int)ListType.LeaveType);
        Utility.BindList(ref lstLeaveType, DB.runReader(szSQL), "ID", "NAME");
        lstLeaveType.Items.Insert(0, new ListItem("Select leave type...", ""));
    }

    void checkSupervisorSettings() {
        if (Payroll.IsPayrollSupervisor && intID > -1 && l.UserID != G.User.UserID) {
            if (l.LeaveStatus == LeaveRequestStatus.Requested) {
                btnApprove.Visible = btnReject.Visible = true;
            }
            btnDelete.Visible = btnUpdate.Visible = false;
            btnCancel.InnerHtml = "Close";
            hdReadOnly.Value = "true";
        }
    }

    private void loadDefaults() {
        txtStartDate.Text = Utility.formatDate(DateTime.Now);
        btnDelete.Visible = false;
    }

    private void loadRequest() {
        l = new LeaveRequest(intID);
        txtStartDate.Text = Utility.formatDate(l.StartDate);
        txtEndDate.Text = Utility.formatDate(l.EndDate);
        txtComments.Text = l.Comment;
        if(l.SupportingFile != "") {
            lExistingFile.Text = "<a href='view"
        }
        Utility.setListBoxItems(ref lstLeaveType, l.LeaveTypeID.ToString());
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        string szSQL;
        sqlUpdate oSQL = new sqlUpdate("LEAVEREQUEST", "ID", intID);
        oSQL.add("STARTDATE", txtStartDate.Text);
        oSQL.add("ENDDATE", txtEndDate.Text);
        oSQL.add("COMMENTS", txtComments.Text);
        oSQL.add("LEAVETYPEID", lstLeaveType.SelectedValue);
       
        if (intID == -1) {
            oSQL.add("USERID", G.User.ID);
            szSQL = oSQL.createInsertSQL();
        } else
            szSQL = oSQL.createUpdateSQL();

        DB.runNonQuery(szSQL);
        if(intID == -1) {
            intID = DB.getScalar("SELECT MAX(ID) FROM LEAVEREQUEST WHERE USERID = " + G.User.ID, -1);
        }
        if (FileUpload1.HasFile) {
            string FileName = "Evidence" + intID + "." + Path.GetExtension(FileUpload1.FileName);
            FileUpload1.SaveAs(Path.Combine(G.Settings.DataDir, FileName));
            DB.runNonQuery(String.Format("UPDATE LEAVEREQUEST SET SUPPORTINGFILE = '{0}' WHERE ID = {1}", DB.escape(FileName), intID));
        }
        sbStartJS.Append("parent.closeEditModal(true);");
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
            UPDATE LEAVEREQUEST
            SET ISDELETED = 1 WHERE ID = {0}", intID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.closeEditModal(true);");
    }

    protected void btnApprove_Click(object sender, EventArgs e) {
        string szSQL = string.Format(@"
            UPDATE LEAVEREQUEST
            SET LEAVESTATUSID = 1, MANAGERSIGNOFFDATE = getdate() WHERE ID = {0}", intID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.closeEditModal(true);");
      
    }

    protected void btnReject_Click(object sender, EventArgs e) {
        string szSQL = string.Format(@"
            UPDATE LEAVEREQUEST
            SET  LEAVESTATUSID = 2, MANAGERSIGNOFFDATE = getdate() WHERE ID = {0}", intID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.closeEditModal(true);");
    }
}