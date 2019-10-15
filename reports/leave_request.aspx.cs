using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class leave_request : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            dtStart = Valid.getDate("txtStartDate");
            dtEnd = Valid.getDate("txtEndDate");

            if (!Page.IsPostBack) {
                loadDates();
            }
        }

        protected void btnViewReport_Click(object sender, EventArgs e) {
            Report oReport = getReport();

            rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            rViewer.ShowParameterPrompts = false;
            rViewer.ShowPageNavigationControls = true;
            rViewer.LocalReport.Refresh();
        }

        protected void btnExportExcel_Click(object sender, EventArgs e) {
            Report oReport = getReport();

            rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            rViewer.ShowParameterPrompts = false;
            rViewer.ShowPageNavigationControls = true;
            //rViewer.LocalReport.Refresh();

            byte[] bFile = rViewer.LocalReport.Render("EXCEL");

            Response.ClearHeaders();
            //Changed from text/html to /xml
            Response.ContentType = "text/xls";
            Response.OutputStream.Write(bFile, 0, bFile.Length);
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename=leave_requests.xls"));
            Response.End();
        }

        private void loadDates() {
            txtStartDate.Text = Utility.formatDate(DateTime.Now.AddDays(-14));
            txtEndDate.Text = Utility.formatDate(DateTime.Now);
        }

        private Report getReport() {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();

            Report oR = new LeaveRequest(rViewer, oFilter, dtStart, dtEnd, chkViewArchived.Checked);
            return oR;
        }
    }
}

public class LeaveRequest : Report {
    private static string RFileName = "Leave requests";
    private DateTime dtStart;
    private DateTime dtEnd;
    private bool IncludeArchived = false;
    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public LeaveRequest(ReportViewer Viewer, ReportFilters oFilter, DateTime StartDate, DateTime EndDate, bool IncludeArchived = false) {
        this.dtStart = StartDate;
        this.dtEnd = EndDate;
        oFilter.StartDate = StartDate;
        oFilter.EndDate = EndDate;
        this.IncludeArchived = IncludeArchived;
        if (G.User.RoleID != 1)
            throw new Exception("Non-Admin user attempting to access admin report - leave request");
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Leave request";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szArchive = " AND ISARCHIVED = 0";
        if (IncludeArchived)
            szArchive = "";
        string szSQL = string.Format(@"
                SELECT  LR.*, L.NAME AS LEAVETYPE, LS.NAME AS STATUS, U.FIRSTNAME + ' ' + U.LASTNAME AS STAFF,
                U.FIRSTNAME + ' ' + U.LASTNAME AS MANAGER,  CASE WHEN HOURS = 0 THEN CAST(TOTALDAYS as VARCHAR) + ' Days' ELSE CAST(HOURS AS VARCHAR) + ' Hrs' END as DURATION
                FROM LEAVEREQUEST LR JOIN LIST L ON L.ID = LR.LEAVETYPEID
                JOIN LEAVESTATUS LS ON LS.ID = LR.LEAVESTATUSID
                JOIN DB_USER U ON LR.USERID = U.ID
                JOIN DB_USER M ON U.SUPERVISORID = M.ID
                WHERE LR.ISDELETED = 0   AND LR.STARTDATE BETWEEN '{0}' AND '{1}' {2}
                ORDER BY LR.STARTDATE ", Utility.formatDate(dtStart), Utility.formatDate(dtEnd), szArchive);
        return DB.runDataSet(szSQL);
    }
}