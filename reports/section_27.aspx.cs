using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class section_27 : Root {
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szStartDate = Valid.getText("szStartDate", "", VT.TextNormal);
            string szEndDate = Valid.getText("szEndDate", "", VT.TextNormal);
            if (!String.IsNullOrEmpty(szStartDate)) {
                dtStart = DateTime.Parse(szStartDate);
                if (String.IsNullOrEmpty(szEndDate))
                    dtEnd = DateTime.Now;
                else
                    dtEnd = DateTime.Parse(szEndDate);
            }

            if (!Page.IsPostBack) {
                Report oReport = getReport("Section 27");
                rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                rViewer.ShowParameterPrompts = false;
                rViewer.ShowPageNavigationControls = true;
                rViewer.LocalReport.Refresh();
                rViewer.Visible = true;
            }
        }

        private Report getReport(string ReportName) {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();

            oFilter.StartDate = dtStart;
            oFilter.EndDate = dtEnd;
            oFilter.OfficeIDList = Valid.getText("szOfficeID", "", VT.NoValidation);
            Report oR = new Section27(rViewer, oFilter);
            return oR;
        }
    }

    public class Section27 : Report {
        private static string RFileName = "Missing Section 27";

        /// <summary>
        /// Constructor Method that receives the Viewer and the Filter and creates the report.
        /// </summary>
        /// <param name="Viewer"></param>
        /// <param name="oFilter"></param>
        public Section27(Microsoft.Reporting.WebForms.ReportViewer Viewer, ReportFilters oFilter) {
            oViewer = Viewer;
            this.oFilter = oFilter;
            ReportTitle = "Section 27 missing";
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
            string szFilter = String.Format(@"
                AND S.SALEDATE BETWEEN '{0}' AND '{1}'
            ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

            string szSQL = string.Format(@"
                SELECT S.ID, S.ADDRESS,  S.ENTITLEMENTDATE, MAX(USR.INITIALSCODE) AS SALESREP1, MIN(USR.INITIALSCODE) AS SALESREP2, '' AS SALESREP, MAX(S.SECTION27COMMENTS) AS COMMENTS
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID = 1 AND SS.COMMISSIONTYPEID = 10
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                WHERE SS.CALCULATEDAMOUNT > 0 AND S.ISSECTION27 = 0 AND S.STATUSID = 1 AND DATEDIFF(DAY, S.SALEDATE, S.ENTITLEMENTDATE) > 30 {0}
                GROUP BY S.ADDRESS, S.ID, S.ENTITLEMENTDATE
                ORDER BY S.ID, S.ADDRESS, S.ENTITLEMENTDATE

                ", szFilter);

            DataSet ds = DB.runDataSet(szSQL);
            foreach (DataRow dr in ds.Tables[0].Rows) {
                if (DB.readString(dr["SALESREP1"]) == DB.readString(dr["SALESREP2"]))
                    dr["SALESREP"] = dr["SALESREP1"];
                else
                    dr["SALESREP"] = dr["SALESREP1"] + "/" + dr["SALESREP2"];
            }
            return ds;
        }
    }
}