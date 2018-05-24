using System;
using System.IO;
using System.Web.UI;

namespace Paymaker {

    public partial class payroll_timesheets_fix : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        string[][] vals = new string[4][] { // CycleID, ReportTypeID, FileDate
                            new string[3] { "5", "1", "20171221" },
                            new string[3] { "6", "1", "20180104" },
                            new string[3] { "7", "2", "20171221" },
                            new string[3] { "8", "2", "20180104" } };

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;

            if (!Page.IsPostBack) {
                foreach(string[] val in vals) {
                    Report oReport = getReport(Convert.ToInt32(val[1]), Convert.ToInt32(val[0]));

                    rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                    rViewer.ShowParameterPrompts = false;
                    rViewer.ShowPageNavigationControls = true;
                    rViewer.LocalReport.Refresh();
                    byte[] bFile = rViewer.LocalReport.Render("PDF");

                    Directory.CreateDirectory(G.Settings.FileDir + "PayrollTimeSheets");
                    File.WriteAllBytes(string.Format("{0}PayrollTimeSheets\\Timesheets_{1}_{2}.pdf", G.Settings.FileDir, val[1], val[2]), bFile);
                }
            }

            // Create new cycles
            TimesheetCycle.checkTimesheetCycles();
            G.TimeSheetCycleReferences = Payroll.getTimeSheetCycleReferences();

            Response.Redirect("../Payroll/payroll_dashboard.aspx");
        }

        private Report getReport(int ReportType, int TimeSheetCycleID) {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();

            Report oR = new Payroll_Timesheets(rViewer, oFilter, ReportType, TimeSheetCycleID);
            return oR;
        }
    }
}