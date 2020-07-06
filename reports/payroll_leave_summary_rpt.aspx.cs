using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class payroll_leave_summary_rpt : Root {
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

            Report oR = new Payroll_Leave_Summary(rViewer, oFilter, CycleRef);
            return oR;
        }
    }

    public class Payroll_Leave_Summary : Report {
        DataView dvFinal;
        private static string RFileName = "Payroll Leave Summary";
        private int CycleRef = 0;

        /// <summary>
        /// Constructor Method that receives the Viewer and the Filter and creates the report.
        /// </summary>
        /// <param name="Viewer"></param>
        /// <param name="oFilter"></param>
        public Payroll_Leave_Summary(ReportViewer Viewer, ReportFilters oFilter, int CycleRef) {
            this.CycleRef = CycleRef;
            if (!G.User.IsAdmin)
                throw new Exception("Non-Admin user attempting to access admin report - Payroll Summary");
            oViewer = Viewer;
            this.oFilter = oFilter;
            ReportTitle = "Payroll Leave Summary";
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
            // Get all data for current pay cycle
            DataSet ds = DB.runDataSet(string.Format(@"
                -- 0) Sick leave
                SELECT '' AL_DATES, '' SL_DATES, ENTRYDATE, U.ID
                FROM TIMESHEETENTRY TE JOIN DB_USER U ON U.ID = TE.USERID
                JOIN LIST O ON O.ID = U.OFFICEID
                JOIN TIMESHEETCYCLE TSC ON TSC.ID = TE.TIMESHEETCYCLEID
                WHERE TSC.ID in ({0},{1})
                    AND SICKLEAVE IS NOT NULL 
                ORDER BY TSC.CYCLETYPEID, LASTNAME;

                -- 1) Annual leave
                SELECT '' AL_DATES, '' SL_DATES, ENTRYDATE, U.ID
                FROM TIMESHEETENTRY TE JOIN DB_USER U ON U.ID = TE.USERID
                JOIN LIST O ON O.ID = U.OFFICEID
                JOIN TIMESHEETCYCLE TSC ON TSC.ID = TE.TIMESHEETCYCLEID
                WHERE TSC.ID in ({0},{1})
                    AND ANNUALLEAVE iS NOT NULL
                ORDER BY TSC.CYCLETYPEID, LASTNAME;

                --2) Final results
                SELECT '' AL_DATES, '' SL_DATES, SUM( SICKLEAVE) AS SICKLEAVE, SUM(ANNUALLEAVE) AS ANNUALLEAVE, U.ID, U.FIRSTNAME, U.LASTNAME , TSC.CYCLETYPEID
                FROM TIMESHEETENTRY TE JOIN DB_USER U ON U.ID = TE.USERID
                JOIN TIMESHEETCYCLE TSC ON TSC.ID = TE.TIMESHEETCYCLEID
                WHERE  TSC.ID in ({0},{1}) AND ( SICKLEAVE IS NOT NULL OR ANNUALLEAVE iS NOT NULL)
				GROUP BY U.ID, U.FIRSTNAME, U.LASTNAME, TSC.CYCLETYPEID
                ORDER BY TSC.CYCLETYPEID, U.ID, LASTNAME",
                    G.TimeSheetCycleReferences[CycleRef].NormalCycle.CycleID,
                    G.TimeSheetCycleReferences[CycleRef].PayAdvanceCycle.CycleID));
            
            dvFinal = ds.Tables[2].DefaultView;

            processTable(ds.Tables[0], "SL_DATES");
            processTable(ds.Tables[1], "AL_DATES");
            DataSet dsFinal = new DataSet("table");
            dvFinal.RowFilter = "";
            dsFinal.Tables.Add(dvFinal.ToTable());
            return dsFinal;
        }

        void processTable(DataTable Data, string DateColumn){
            int CurrUser = -1;
            List<DateInterval> lDates = new List<DateInterval>();
            DateInterval dCurr = new DateInterval(DateTime.Now);
            foreach (DataRow dr in Data.Rows) {
                DateTime EntryDate = Convert.ToDateTime(dr["ENTRYDATE"]);
                if (CurrUser != DB.readInt(dr["ID"])) {
                    addFinalRow(CurrUser, lDates, dvFinal, DateColumn);
                    CurrUser = DB.readInt(dr["ID"]);
                    dCurr = new DateInterval(EntryDate);
                    lDates.Add(dCurr);
                                            
                } else {
                    // Check to see if we are in a date run
                    if(dCurr.EndDate.AddDays(1) == EntryDate.Date) {
                        dCurr.EndDate = EntryDate;
                    } else {
                        dCurr = new DateInterval(EntryDate);
                        lDates.Add(dCurr);
                    }
                }
            }
            addFinalRow(CurrUser, lDates, dvFinal, DateColumn);
        }

       private void addFinalRow(int CurrUser, List<DateInterval> lDates, DataView dvFinal, string DateColumn) {
            if (CurrUser == -1)
                return;
            dvFinal.RowFilter = "ID = " + CurrUser;
            dvFinal[0][DateColumn] = String.Join(" ", lDates);
            lDates.Clear();
        }
    }

    public class DateInterval {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateInterval(DateTime dtStart) {
            StartDate = dtStart;
            EndDate = dtStart;
        }

        public override string ToString() {
            if (StartDate == EndDate)
                return StartDate.ToString("dd/M ");
            else
                return StartDate.ToString("dd/M-") + EndDate.ToString("dd/M "); ;
        }
    }
}