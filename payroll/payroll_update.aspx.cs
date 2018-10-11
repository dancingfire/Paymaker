using System;
using System.IO;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class payroll_update : Root {
    private Boolean blPopup = false;
    private int intUserID;
    private int intUserPayrollCycleID;
    private int intCycleRef;
    bool blnIsAdminView = false;

    protected void Page_Load(object sender, System.EventArgs e) {
        blPopup = false;
        intUserID = G.CurrentUserID;
       
        // Popup window can be accessed by admin / supervisor to view further details of the users timesheet
        if (Valid.getText("IsPopup", "").ToLower() == "true") {
            blnShowMenu = false;
            blPopup = true;
            dPageHeader.Visible = btnSignOff.Visible = btnUpdate.Visible = false;
            dContainer.Style.Add("width", "100%");
            intUserID = Valid.getInteger("UserID");
        }


        intCycleRef = Valid.getInteger("CycleID", 0);
        if (hdCycleRef.Value == "")
            hdCycleRef.Value = intCycleRef.ToString();

        intCycleRef = Convert.ToInt32(hdCycleRef.Value);

        if (G.UserInfo.getUser(intUserID).PayrollCycleID == 1)
            intUserPayrollCycleID = G.TimeSheetCycleReferences[intCycleRef].NormalCycle.CycleID;
        else
            intUserPayrollCycleID = G.TimeSheetCycleReferences[intCycleRef].PayAdvanceCycle.CycleID;
        
        loadData();
    }

    private void loadData() {
        lstCycle.Items.Clear();
        foreach (int c in G.TimeSheetCycleReferences.dValues.Keys) {
            DateTime dtStart = G.TimeSheetCycleReferences[c].NormalCycle.StartDate;
            DateTime dtEnd = G.TimeSheetCycleReferences[c].NormalCycle.EndDate;
            if (G.UserInfo.getUser(intUserID).PayrollCycleID == 2) {
                dtStart = G.TimeSheetCycleReferences[c].PayAdvanceCycle.StartDate;
                dtEnd = G.TimeSheetCycleReferences[c].PayAdvanceCycle.EndDate;
            }

            ListItem oLI = new ListItem(
                                string.Format("{0:d MMM} - {1:d MMM}", dtStart, dtEnd)
                                , c.ToString());
            if (c < 0)
                oLI.Attributes.Add("style", "color:red;");
            lstCycle.Items.Add(oLI);
        }

        lstCycle.SelectedValue = hdCycleRef.Value;

        dContainer.Visible = true;
        dOldRecords.Visible = false;
        btnSignOff.Visible = btnUpdate.Visible = true;
        if (intCycleRef > -1) {
            UserTimesheet oUT = new UserTimesheet(intUserID, intUserPayrollCycleID, blReadOnlyForm: blPopup);
            oUT.lEntries.Add(new UserTimesheetEntry());
            if (oUT.lEntries[0].UserID != G.CurrentUserID)
                blnIsAdminView = true;
            gvList.DataSource = oUT.lEntries;
            gvList.DataBind();

            gvModifications.DataSource = oUT.dtModifications;
            gvModifications.DataBind();
            HTML.formatGridView(ref gvModifications, true);
            gvModifications.Visible = oUT.dtModifications.Rows.Count > 0;
            
            HTML.formatGridView(ref gvList, true);

            // Check if user has a supervisor
            if (G.UserInfo.getUser(G.CurrentUserID).SupervisorID > 0)
                btnSignOff.Text = "Submit";

            btnUpdate.Enabled = btnSignOff.Enabled = true;
            lblMessage.Text = btnUpdate.ToolTip = btnSignOff.ToolTip = "";
            pMessage.Visible = false;

            // Once user submits time sheet it can no longer be updated.
            if (!oUT.CanEdit) {
                btnUpdate.Enabled = btnSignOff.Enabled = false;

                lblMessage.Text = btnUpdate.ToolTip = btnSignOff.ToolTip = "Timesheet is locked.  No further changes can be made";
                pMessage.Visible = true;
                btnSignOff.Visible = btnUpdate.Visible = false;
            }

            pAdmin.Visible = blnIsAdminView;

            // The supervisor approve override button is only visible when staff has signed off and the supervisor has not
            // NOTE: the Override button remains available to Supervisors / Admin even after supervisor approval to allow record changes to be made.
            btnApprove.Visible = oUT.UserSignOff > DateTime.MinValue && oUT.SupervisorSignOff == DateTime.MinValue;

            btnUpdate.Visible = btnSignOff.Visible = !pAdmin.Visible;
        } else {
            dContainer.Visible = false;
            dOldRecords.Visible = true;

            btnDownload.HRef = string.Format("../reports/manual_timesheet.aspx?UserID={0}&CycleID={1}", G.CurrentUserID, intUserPayrollCycleID);
        }
    }

    private double getValue(string Value, int ID) {
        string szValue = Valid.getText(string.Format("txt{0}_{1}", Value, ID), "");
        return szValue == "" ? double.MinValue : Convert.ToDouble(szValue);
    }

    double checkValue(double value) {
        if (value == Double.MinValue)
            return 0;
        return value;
    }
    private void getFormValues(UserTimesheet oUT, bool SignOff = false) {
        string szLog = "" ;
        foreach (UserTimesheetEntry oUTSE in oUT.lEntries) {
            szLog = "";
            double value = getValue("Actual", oUTSE.ID);
            if (oUTSE.Actual != value)
                szLog += String.Format(" {2} Actual changed from {0} to {1}. ", checkValue(oUTSE.Actual), checkValue(value), Utility.formatDate(oUTSE.EntryDate));
            oUTSE.Actual = value;

            value = getValue("AnnualLeave", oUTSE.ID);
            if (oUTSE.AnnualLeave != value)
                szLog += String.Format(" {2} Annual Leave changed from {0} to {1}. ", checkValue(oUTSE.AnnualLeave), checkValue(value), Utility.formatDate(oUTSE.EntryDate));
            oUTSE.AnnualLeave = value;

            value = getValue("SickLeave", oUTSE.ID);
            if (oUTSE.SickLeave != value)
                szLog += String.Format(" {2} Sick Leave changed from {0} to {1}. ", checkValue(oUTSE.SickLeave), checkValue(value), Utility.formatDate(oUTSE.EntryDate));
            oUTSE.SickLeave = value;
         
            value = getValue("RDOAcrued", oUTSE.ID);
            if (oUTSE.RDOAcrued != value)
                szLog += String.Format(" {2} RDO Acrued changed from {0} to {1}. ", checkValue(oUTSE.RDOAcrued), checkValue(value), Utility.formatDate(oUTSE.EntryDate));
            oUTSE.RDOAcrued = value;
          
            value = getValue("RDOTaken", oUTSE.ID);
            if (oUTSE.RDOTaken != value)
                szLog += String.Format(" {2} RDO Taken changed from {0} to {1}. ", checkValue(oUTSE.RDOTaken), checkValue(value), Utility.formatDate(oUTSE.EntryDate));
            oUTSE.RDOTaken = value;

            oUTSE.Comments = Valid.getText(string.Format("txtComments_{0}", oUTSE.ID), "");
            if (szLog != "" && blnIsAdminView) {
                szLog += "Comments: " + oUTSE.Comments;
                DBLog.addGenericRecord(DBLogType.PayrollModification, szLog, oUTSE.ID);
            }

            if (SignOff)
                oUTSE.UserSignOff = DateTime.Now;
        }
        
    }

    private void updateRecord(bool SignOff = false) {
        UserTimesheet oUT = new UserTimesheet(intUserID, intUserPayrollCycleID);
        getFormValues(oUT, SignOff);
        oUT.updateDB();
        DBLog.addGenericRecord(DBLogType.PayrollModification, String.Format("Paysheet updated - sign off: {0} {1}", SignOff, G.User.UserName), intUserPayrollCycleID, intUserID);
        // When user signs off, a check is run to see if they are the final user of their supervisor.
        // Supervisor is informed when all staff from a given pay cycle have submitted their forms.
        if (SignOff) {
            int intSupervisorID = G.UserInfo.getUser(G.CurrentUserID).SupervisorID;
            if (intSupervisorID > 0) {
                TimesheetCycle oTC = new TimesheetCycle(intUserPayrollCycleID);
                oTC.checkSupervisorGroupSignoff(intSupervisorID);
            }
        }
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        updateRecord();
        Response.Redirect("payroll_dashboard.aspx");
        // When the changes are saved, check to see if all users that belong to this user's supervisor (if they have one) are completed - if so, send an email to the supervisor letting them know that all changes are complete
    }

    protected void btnChange_Click(object sender, System.EventArgs e) {
        updateRecord();
        Payroll.submitTimesheet(intUserID, intUserPayrollCycleID, false, true);
        sendEmail();
        sbStartJS.Append("parent.refreshPage();");
    }

    protected void btnApprove_Click(object sender, System.EventArgs e) {
        Payroll.submitTimesheet(intUserID, intUserPayrollCycleID, true);
        sbStartJS.Append("parent.refreshPage();");
    }

    private Report getReport() {
        //Load the parameters from the page
        ReportFilters oFilter = new ReportFilters();
        Report oR = new Payroll_Timesheets(rViewer, oFilter, -1, intUserPayrollCycleID, intUserID);
        return oR;
    }

    private void sendEmail() {
        Report oReport = getReport();

        rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
        rViewer.ShowParameterPrompts = false;
        rViewer.ShowPageNavigationControls = true;

        byte[] bFile = rViewer.LocalReport.Render("PDF");
        string szEmail = G.UserInfo.getUser(intUserID).Email;

        string Msg = "Thank you for submitting your timesheet.  Please find a copy attached.";
        if (blnIsAdminView) {
            Msg = " Your timesheet has been changed by " + G.User.UserName;
        }
        Email.sendMail(szEmail, "do-not-reply@fletchers.net.au", "Timesheet submitted",
                Msg, szBCC: "payroll@fletchers.net.au", IncludeFile: new Attachment(new MemoryStream(bFile), "Timesheet.pdf"));
    }

    protected void btnSignOff_Click(object sender, System.EventArgs e) {
        updateRecord(SignOff: true);
        sendEmail();
        Response.Redirect("payroll_dashboard.aspx");
        // When the changes are saved, check to see if all users that belong to this user's supervisor (if they have one) are completed - if so, send an email to the supervisor letting them know that all changes are complete
    }
}