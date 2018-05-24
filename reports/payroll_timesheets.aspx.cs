using System;
using System.IO;
using System.Web.UI;

namespace Paymaker {

    public partial class payroll_timesheets : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;

            if (!Page.IsPostBack) {
                for (int ReportType = 1; ReportType < 3; ReportType++) {
                    Report oReport = getReport(ReportType);

                    rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                    rViewer.ShowParameterPrompts = false;
                    rViewer.ShowPageNavigationControls = true;
                    rViewer.LocalReport.Refresh();
                    byte[] bFile = rViewer.LocalReport.Render("PDF");

                    Directory.CreateDirectory(G.Settings.FileDir + "PayrollTimeSheets");
                    File.WriteAllBytes(string.Format("{0}PayrollTimeSheets\\Timesheets_{1}_{2}.pdf", G.Settings.FileDir, ReportType, DateTime.Now.ToString("yyyyMMdd")), bFile);
                }

                // Mark time sheet cycles completed
                // ONLY the earliest current payroll cycles can be marked as completed
                DB.runNonQuery(@"
                    UPDATE TIMESHEETCYCLE SET COMPLETED = 1
                        WHERE ID IN
                            ((SELECT MIN(ID) FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 1 AND COMPLETED = 0),
                             (SELECT MIN(ID) FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 2 AND COMPLETED = 0))");

                G.TimeSheetCycleReferences = null;
            }

            // Create new cycles
            TimesheetCycle.checkTimesheetCycles();
            G.TimeSheetCycleReferences = Payroll.getTimeSheetCycleReferences();

            Response.Redirect("../Payroll/payroll_dashboard.aspx");
        }

        private Report getReport(int ReportType) {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();

            Report oR = new Payroll_Timesheets(rViewer, oFilter, ReportType);
            return oR;
        }
    }
}