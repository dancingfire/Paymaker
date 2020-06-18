using System;
using System.Web.UI;

namespace Paymaker {

    public partial class payroll_summary_rpt : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        int CycleRef = 0;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            CycleRef = Valid.getInteger("CycleRef", 0);

            if (!Page.IsPostBack) {
                Report oReport = getReport();

                rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                rViewer.ShowParameterPrompts = false;
                rViewer.ShowPageNavigationControls = true;
                //rViewer.LocalReport.Refresh();

                byte[] bFile = rViewer.LocalReport.Render("EXCELOPENXML");

                Response.ClearHeaders();
                //Changed from text/html to /xml
                Response.ContentType = "text/xls";
                Response.OutputStream.Write(bFile, 0, bFile.Length);
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename=payroll_summary.xlsx"));
                Response.End();
            }
        }

        private Report getReport() {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();

            Report oR = new Payroll_Summary(rViewer, oFilter, CycleRef);
            return oR;
        }
    }
}