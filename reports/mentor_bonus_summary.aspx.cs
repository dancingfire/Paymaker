using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class mentor_bonus_summary : Root {
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            this.Title = "Mentor Bonus summary report";

            if (!Page.IsPostBack) {
                Report oReport = getReport();
                rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                rViewer.ShowParameterPrompts = false;
                rViewer.ShowPageNavigationControls = true;
                rViewer.LocalReport.Refresh();
                rViewer.Visible = true;
            }
        }

        private Report getReport() {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();
            oFilter.OfficeIDList = Valid.getText("szOfficeID", "", VT.NoValidation);
            oFilter.UserIDList = Valid.getText("szUserID", "", VT.NoValidation);
            Report oR = new Mentor_Bonus_Summary(rViewer, oFilter);
            return oR;
        }
    }

    public class Mentor_Bonus_Summary : Report {
        private static string RFileName = "Mentor bonus summary";

        /// <summary>
        /// Constructor Method that receives the Viewer and the Filter and creates the report.
        /// </summary>
        /// <param name="Viewer"></param>
        /// <param name="oFilter"></param>
        public Mentor_Bonus_Summary(ReportViewer Viewer, ReportFilters oFilter) {
            oViewer = Viewer;
            this.oFilter = oFilter;
            ReportTitle = "Mentor bonus summary";
            initReport();
        }

        protected override void initReport() {
            oFilter.loadUserFilters();
            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(oFilter.PayPeriodID));
            oFilter.StartDate = DateUtil.ThisFinYear(oP.StartDate).Start;
            oFilter.EndDate = oP.EndDate;
            loadParameters();
            setupReport();
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
            if (oFilter.PayPeriodID == "")
                oFilter.PayPeriodID = G.CurrentPayPeriod.ToString();
            string szPayPeriodIDs = G.PayPeriodInfo.getPayPeriodsForYTD(Convert.ToInt32(oFilter.PayPeriodID));

            string szFilter = String.Format(" AND PAYPERIODID IN ({0})", szPayPeriodIDs);
            bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
            string szUserActive = " AND U.ISACTIVE = 1 ";
            if (blnIncludeInactive)
                szUserActive = "";

            string szSQL = string.Format(@"
               SELECT  ISNULL(SUM(CASE WHEN SALEID IS NULL THEN MB.SALARYBONUS ELSE 0 END), 0) AS SALARYTOTAL,
                    ISNULL(SUM(MB.COMMISSIONBONUS), 0) AS TEAMBONUS_TOTAL, UM.FIRSTNAME + ' ' + UM.LASTNAME AS SENIOURAGENT,
                    U.FIRSTNAME + ' ' + U.LASTNAME AS JUNIORAGENT,
                    DateName(month, P.STARTDATE) + ' ' + Cast(P.FINYEAR as varchar) AS MONTHDATE

                FROM MENTORBONUS MB JOIN DB_USER UM ON MB.MENTORUSERID = UM.ID
				JOIN PAYPERIOD P ON MB.PAYPERIODID = P.ID
				JOIN DB_USER U ON MB.USERID = U.ID
                WHERE MB.USERID != MB.MENTORUSERID {0} {1}
               GROUP BY UM.FIRSTNAME + ' ' + UM.LASTNAME, PAYPERIODID, P.STARTDATE, P.FinYear, U.FIRSTNAME + ' ' + U.LASTNAME
               ORDER BY FINYEAR, MONTH(P.STARTDATE), UM.FIRSTNAME + ' ' + UM.LASTNAME"
                , szFilter, szUserActive);
            DataSet ds = DB.runDataSet(szSQL);

            return ds;
        }
    }
}