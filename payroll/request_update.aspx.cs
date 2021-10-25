using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

public partial class request_update : Root {
    protected int intID = -1;
    private LeaveRequest l = null;

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
        string szSQL = String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0} AND ISACTIVE = 1 ORDER BY SEQUENCENO, NAME", (int)ListType.LeaveType);
        Utility.bindListBW( lstLeaveType, DB.runDataSet(szSQL), "ID", "NAME");
        lstLeaveType.Items.Insert(0, new ListItem("Select leave type...", ""));

        using (DataSet ds = DB.runDataSet("select * from PUBLICHOLIDAY WHERE HOLIDAYDATE > DateAdd(d, -100, getdate())")) {
            string szJS = "";
            foreach (DataRow dr in ds.Tables[0].Rows) {
                Utility.Append(ref szJS, "'" + DB.readDate(dr["HOLIDAYDATE"]).ToString("MMM dd, yyyy") + "'", ",");
            }
            sbStartJS.AppendFormat(@"
                arHolidayDates = [{0}];
            ", szJS);
        }
    }

    private void checkSupervisorSettings() {
        if (intID == -1) {
            dHistory.Visible = false;
            return;
        }

        if (Payroll.IsLeaveSupervisor && intID > -1 && l.RequestUserID != G.User.UserID) {
            if (l.LeaveStatus == LeaveRequestStatus.Requested || l.LeaveStatus == LeaveRequestStatus.DiscussionRequired || l.LeaveStatus == LeaveRequestStatus.ChangeRequest) {
                btnApprove.Visible = btnDiscussion.Visible = true;
            }
            if(l.LeaveStatus == LeaveRequestStatus.DiscussionRequired) {
                btnReject.Visible = true;
            }
            btnDelete.Visible = G.User.IsAdmin || l.StartDate < DateTime.Now;
            btnUpdate.Visible = false;
            btnCancel.InnerHtml = "Close";
            hdReadOnly.Value = "true";
            txtComments.ReadOnly = true;
            FileUpload1.Visible = false;
            pApprovalPanel.Visible = true;
        } else {
           
            txtManagerComments.Text = l.ManagerComments;
            pApprovalPanel.Visible = true;
            txtManagerComments.ReadOnly = true;
            if (l.StartDate < DateTime.Now.AddDays(-100)) {
                //Readonly after the start date
                hdReadOnly.Value = "true";
            }
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
        txtManagerComments.Text = l.ManagerComments;
        txtHours.Text = l.TotalHours.ToString();
        if(l.TotalHours > 0) {
            lstPartial.SelectedValue = "PARTIAL";
        }
        if (l.SupportingFile != "") {
            lExistingFile.Text = string.Format("<a href='view_doc.aspx?file={0}' target='_blank'>View file</a> <br/>", Server.UrlEncode(l.SupportingFile), l.SupportingFile);
        }
        Utility.setListBoxItems(lstLeaveType, l.LeaveTypeID.ToString());
        if (l.LeaveStatus == LeaveRequestStatus.Rejected || (l.LeaveStatus == LeaveRequestStatus.Approved && l.StartDate < DateTime.Now.AddDays(-100))) {
            txtComments.ReadOnly = true;
            btnUpdate.Visible = false;
            btnDelete.Visible = G.User.IsAdmin; //Admin can see the delete button;
            btnDiscussion.Visible = false;
        } else if (l.LeaveStatus == LeaveRequestStatus.Approved || l.LeaveStatus == LeaveRequestStatus.ChangeRequest) {
            btnDelete.Visible = G.User.IsAdmin; //Admin can see the delete button
            btnUpdate.Text = "Resend approval request";
        }
        loadHistory();
    }

    void loadHistory() {
        using(DataSet ds = DB.runDataSet(String.Format(@"
            SELECT TYPEID, '' As ACTION, SENTDATE, ''AS COMMENTS
            FROM EMAILLOG L 
            WHERE OBJECTID = {0} AND TYPEID IN (0, 1, 2, 3, 5,  6)
            ORDER BY SENTDATE 
        ", intID))) {
            foreach(DataRow dr in ds.Tables[0].Rows) {
                EmailType Type = (EmailType)DB.readInt(dr["TYPEID"]);
                if (Type == EmailType.LeaveRequest) {
                    dr["ACTION"] = "Initial request";
                } else if ( Type == EmailType.ChangeRequest) {
                    dr["ACTION"] = "Change request";
                } else if (Type == EmailType.Approval) {
                    dr["Action"] = "Approved";
                    dr["Comments"] = l.ManagerComments;
                } else if (Type == EmailType.DiscussionRequired) {
                    dr["Action"] = "Discussion required";
                    dr["Comments"] = l.ManagerComments;
                } else if (Type == EmailType.Rejection) {
                    dr["Action"] = "Rejected";
                    dr["Comments"] = l.ManagerComments;
                } else if(Type == EmailType.Reminder) {
                    dr["Action"] = "Reminder sent";
                }
            }
            Utility.bindGV(ref gvHistory, ds, false);

        }
    }
    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        l = new LeaveRequest(intID);
        l.StartDate = Valid.getDate("txtStartDate");
        l.EndDate = Valid.getDate("txtEndDate");
        l.Comment = Valid.getText("txtComments", "");
        l.LeaveTypeID = Valid.getInteger("lstLeaveType");
        l.TotalDays = Valid.getInteger("txtTotalDays");
        l.TotalHours = Valid.getInteger("txtHours", 0);
        l.updateDB();
        l.addFile(FileUpload1);
        sbStartJS.Append("parent.closeEditModal(true);");
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        l = new LeaveRequest(intID);
        l.delete();
        sbStartJS.Append("parent.closeEditModal(true);");
    }

    protected void btnApprove_Click(object sender, EventArgs e) {
        l = new LeaveRequest(intID);
        l.managerUpdate(txtManagerComments.Text, LeaveRequestStatus.Approved);
        sbStartJS.Append("parent.closeEditModal(true);");
    }

    protected void btnDiscussion_Click(object sender, EventArgs e) {
        l = new LeaveRequest(intID);
        l.managerUpdate(txtManagerComments.Text, LeaveRequestStatus.DiscussionRequired);
        sbStartJS.Append("parent.closeEditModal(true);");
    }

    protected void btnReject_Click(object sender, EventArgs e) {
        l = new LeaveRequest(intID);
        l.managerUpdate(txtManagerComments.Text, LeaveRequestStatus.Rejected);
        sbStartJS.Append("parent.closeEditModal(true);");
    }
}