﻿using System;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;

public enum LeaveRequestStatus {
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    DiscussionRequired = 3,
    ChangeRequest = 4
}

/// <summary>
/// Summary description for LeaveRequest
/// </summary>
public class LeaveRequest {
    public int intID { get; set; }
    public int RequestUserID { get; set; }
    public int LeaveTypeID { get; set; }
    public LeaveRequestStatus LeaveStatus { get; set; }
    public DateTime EnteredDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public int TotalHours { get; set; }
    public string Comment { get; set; }
    public string ManagerComments { get; set; }
    public string SupportingFile { get; set; }
    public string LeaveType { get; set; }

    /// <summary>
    /// Loads an object for the given transaction
    /// </summary>
    /// <param name="TxID"></param>
    public LeaveRequest(int DBID) {
        intID = DBID;
        if (intID > -1) {
            string szSQL = String.Format("SELECT  LR.*, L.NAME AS LEAVETYPE FROM LEAVEREQUEST LR JOIN LIST L ON LR.LEAVETYPEID = L.ID where LR.ID = {0}", intID);

            DataSet dsSale = DB.runDataSet(szSQL);
            foreach (DataRow dr in dsSale.Tables[0].Rows) {
                LeaveTypeID = Convert.ToInt32(dr["LEAVETYPEID"]);
                RequestUserID = Convert.ToInt32(dr["USERID"]);
                LeaveStatus = (LeaveRequestStatus)Convert.ToInt32(dr["LEAVESTATUSID"]);
                TotalDays = Convert.ToInt32(dr["TotalDays"]);
                TotalHours = Convert.ToInt32(dr["Hours"]);
                Comment = Convert.ToString(dr["COMMENTS"]);
                EnteredDate = DB.readDate(dr["ENTRYDATE"]);
                StartDate = DB.readDate(dr["STARTDATE"]);
                EndDate = DB.readDate(dr["ENDDATE"]);
                SupportingFile = DB.readString(dr["SUPPORTINGFILE"]);
                ManagerComments = DB.readString(dr["MANAGERCOMMENTS"]);
                LeaveType = DB.readString(dr["LEAVETYPE"]);
            }
        }
    }

    public int updateDB() {
        string szSQL = "";
        LeaveRequest lOld = null;
        sqlUpdate oSQL = new sqlUpdate("LEAVEREQUEST", "ID", intID);

        //Check the old status in case we need to show the earlier request in the email
        if (this.LeaveStatus == LeaveRequestStatus.Approved || this.LeaveStatus == LeaveRequestStatus.ChangeRequest) {
            lOld = new LeaveRequest(this.intID);
            oSQL.add("LEAVESTATUSID", (int)LeaveRequestStatus.ChangeRequest);
        }
        oSQL.add("STARTDATE", Utility.formatDate(StartDate));
        oSQL.add("ENDDATE", Utility.formatDate(EndDate));
        oSQL.add("COMMENTS", Comment);
        oSQL.add("LEAVETYPEID", LeaveTypeID);
        oSQL.add("TOTALDAYS", TotalDays);
        oSQL.add("HOURS", TotalHours);

        if (intID == -1) {
            oSQL.add("USERID", G.User.ID);
            RequestUserID = G.User.ID;
            szSQL = oSQL.createInsertSQL();
        } else
            szSQL = oSQL.createUpdateSQL();

        DB.runNonQuery(szSQL);
        if (intID == -1) {
            intID = DB.getScalar("SELECT MAX(ID) FROM LEAVEREQUEST WHERE USERID = " + G.User.ID, -1);
        }

        sendEmailToManager(lOld);
        return intID;
    }

    public void addFile(FileUpload FileUpload1) {
        string DestDir = Path.Combine(G.Settings.DataDir, "LeaveData");
        if (!Directory.Exists(DestDir))
            Directory.CreateDirectory(DestDir);
        if (FileUpload1.HasFile) {
            string FileName = "Evidence" + intID + Path.GetExtension(FileUpload1.FileName);
            FileUpload1.SaveAs(Path.Combine(DestDir, FileName));
            DB.runNonQuery(String.Format("UPDATE LEAVEREQUEST SET SUPPORTINGFILE = '{0}' WHERE ID = {1}", DB.escape(FileName), intID));
        }
    }

    public void managerUpdate(string Comment, LeaveRequestStatus Status) {
        string SignOffDate = ", MANAGERSIGNOFFDATE = getdate() ";
        if (Status == LeaveRequestStatus.DiscussionRequired)
            SignOffDate = "";
        string szSQL = string.Format(@"
            UPDATE LEAVEREQUEST
            SET LEAVESTATUSID = {1},  MANAGERCOMMENTS = '{2}' {3}
            WHERE ID = {0}", intID, (int)Status, DB.escape(Comment),SignOffDate);
        DB.runNonQuery(szSQL);
        sendEmailToStaff(Status, Comment);
    }


    private void sendEmailToStaff(LeaveRequestStatus Status, string ManagerComment) {
        //Find the manager of this user
        UserDetail u = G.UserInfo.getUser(RequestUserID);
        UserDetail m = G.UserInfo.getUser(u.SupervisorID);
        string Subject = "Leave request approved";
        if (Status == LeaveRequestStatus.Rejected) {
            Subject = "Leave request rejected";
        } else  if (Status == LeaveRequestStatus.DiscussionRequired) {
            Subject = "Leave request - discussion required";
        }
        string szEmail = "The following leave request has been approved. <br/><br/>";
        string szLeaveDetails = String.Format(@"
            
                Type: {1} <br/>
                Start: {2}<br/>
                End: {3}<br/>
                Comments: {4} <br/><br/>
            ", u.Name, this.LeaveType, Utility.formatDate(StartDate), Utility.formatDate(EndDate), Utility.nl2br(Comment));
        EmailType EType = EmailType.Approval;
        if (Status == LeaveRequestStatus.Approved) {
            szEmail = "The following leave request has been approved. <br/><br/>" + szLeaveDetails;
            if (ManagerComment != "") {
                szEmail += "The following comments were left: <br/><br/>" + Utility.nl2br(ManagerComment);
            }
        } else if (Status == LeaveRequestStatus.Rejected) {
            EType = EmailType.Rejection;
            szEmail = "The following leave request has been rejected. <br/><br/>" + szLeaveDetails;
            szEmail += "The following comment was left: <br/><br/>" + Utility.nl2br(ManagerComment);
        } else {
            szEmail = "Further discussion is required for this request. <br/><br/>" + szLeaveDetails;
            EType = EmailType.DiscussionRequired;
            szEmail += "The following clarification is requested: <br/><br/>" + Utility.nl2br(ManagerComment);
        }

        Email.sendMail(u.Email, EmailSettings.SMTPServerFromEmail, Subject, szEmail, LogObjectID: intID, Type: EType);
    }

    public void delete() {
        string szSQL = string.Format(@"
            UPDATE LEAVEREQUEST
            SET ISDELETED = 1 WHERE ID = {0}", intID);
        DB.runNonQuery(szSQL);

        //If this is a user deleting the request send a message to their team leader
        if(G.User.OriginalUserID == this.RequestUserID) {
            sendDeleteNotification();
        }
    }

    private void sendDeleteNotification() {
        UserDetail u = G.UserInfo.getUser(RequestUserID);
        UserDetail m = G.UserInfo.getUser(u.SupervisorID);
        string szTo = G.Settings.CatchAllEmail;

        if (m != null && !String.IsNullOrWhiteSpace(m.Email)) {
            szTo = m.Email;
        }
        string szEmail = String.Format(@"
            {0} has deleted the following leave request:<br/><br/>
                Type: {1} <br/>
                Start: {2}<br/>
                End: {3}<br/>
                Comments: {4} <br/><br/> ", 
                u.Name, this.LeaveType, Utility.formatDate(StartDate), Utility.formatDate(EndDate), Utility.nl2br(Comment));

        string szSubject = " Leave request cancellation";
        EmailType typeOfRequest = EmailType.Cancellation;
        Email.sendMail(szTo, EmailSettings.SMTPServerFromEmail, szSubject, szEmail, LogObjectID: intID, Type: typeOfRequest, DisplayName: "Leave Request cancellation");
    }

    /// <summary>
    /// lOld can contain the previous leave request details if this is an alteration to an existing approved leave request
    /// </summary>
    /// <param name="lOld"></param>
    private void sendEmailToManager(LeaveRequest lOld = null) {
        //Find the manager of this user
        UserDetail u = G.UserInfo.getUser(RequestUserID);
        UserDetail m = G.UserInfo.getUser(u.SupervisorID);
        if (u.SupervisorID == 0)
            m = null;

        string szTo = G.Settings.CatchAllEmail;

        if (m != null && !String.IsNullOrWhiteSpace(m.Email)) {
            szTo = m.Email;
        }

        string szEmail = String.Format(@"
            {0} has requested leave for the following period:<br/><br/>
                Type: {1} <br/>
                Start: {2}<br/>
                End: {3}<br/>
                Comments: {4} <br/><br/>

               Please <a href='https://{5}/login.aspx?LEAVE=true'>login to CAPS </a> to respond to this request.
            ", u.Name, this.LeaveType, Utility.formatDate(StartDate), Utility.formatDate(EndDate), Utility.nl2br(Comment),
                G.BaseURL);

        string szSubject = "  Leave request";
        EmailType typeOfRequest = EmailType.LeaveRequest;
        if(lOld != null) {
            szSubject = "Updated leave request";
            szEmail += String.Format(@"<p> The original request details are below: </p>
                Type: {0} <br/>
                Start: {1}<br/>
                End: {2}<br/>
                Comments: {3} <br/><br/>", lOld.LeaveType, Utility.formatDate(lOld.StartDate), Utility.formatDate(lOld.EndDate), Utility.nl2br(lOld.Comment));
            typeOfRequest = EmailType.ChangeRequest;
        }
        Email.sendMail(szTo, EmailSettings.SMTPServerFromEmail, szSubject, szEmail, LogObjectID: intID, Type: typeOfRequest, DisplayName: "Leave Request for Actioning");
    }

    public void sendReminderEmailToManager() {

        //Find the manager of this user
        UserDetail u = G.UserInfo.getUser(RequestUserID);
        UserDetail m = G.UserInfo.getUser(u.SupervisorID);
        string szTo = G.Settings.CatchAllEmail;

        if (m != null && !String.IsNullOrWhiteSpace(m.Email)) {
            szTo = m.Email;
        }

        string szEmail = String.Format(@"
            <strong>This is a reminder email.</strong><br/><br/>
            {0} has requested leave for the following period:<br/><br/>
                Type: {1} <br/>
                Start: {2}<br/>
                End: {3}</br/>
                Comments: {4} <br/><br/>

               Please <a href='https://{0}?LEAVE=true'>login to CAPS </a> to respond to this request.
            ", u.Name, this.LeaveType, Utility.formatDate(StartDate), Utility.formatDate(EndDate), Utility.nl2br(Comment),
            G.BaseURL);

        Email.sendMail(szTo, "", "Leave request reminder", szEmail, LogObjectID: intID, Type: EmailType.Reminder);
    }
}


/// <summary>
/// Manages the reminder email process - we send out a reminder every three days
/// </summary>
public static class LeaveReminders {

    public static void checkReminders() {
        string szSQL = @"    
            SELECT LR.*
            FROM LEAVEREQUEST LR JOIN LIST L ON L.ID = LR.LEAVETYPEID
            JOIN LEAVESTATUS LS ON LS.ID = LR.LEAVESTATUSID
            JOIN DB_USER U ON LR.USERID = U.ID
			LEFT JOIN  EMAILLOG EL ON EL.ObjectID = LR.ID AND EL.TYPEID = 3
            WHERE LR.ISDELETED = 0   AND MANAGERSIGNOFFDATE IS NULL
                AND DATEDIFF(d, getdate(), entrydate) % 3 = 0 AND Convert(Date, ENTRYDATE) < Convert(Date, getDate())
				AND EL.ID IS NULL
                ORDER BY LR.ENTRYDATE
";

        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                LeaveRequest l = new LeaveRequest(DB.readInt(dr["ID"]));
                l.sendReminderEmailToManager();
            }
        }
    }
}
