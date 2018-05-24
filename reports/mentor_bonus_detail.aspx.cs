using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class mentor_bonus_detail : Root {
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            this.Title = "Mentor Bonus detail report";

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
            Report oR = new Mentor_Bonus_Detail(rViewer, oFilter);
            return oR;
        }
    }

    public class Mentor_Bonus_Detail : Report {
        private static string RFileName = "Mentor bonus detail";

        /// <summary>
        /// Constructor Method that receives the Viewer and the Filter and creates the report.
        /// </summary>
        /// <param name="Viewer"></param>
        /// <param name="oFilter"></param>
        public Mentor_Bonus_Detail(ReportViewer Viewer, ReportFilters oFilter) {
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
              SELECT SUM(USS.GRAPHCOMMISSION) AS GRAPHCOMMISSION, S.ADDRESS, P.StartDate AS MONTHDATE, U.FirstName + ' ' + U.LASTNAME AS JUNIORAGENT,
                MB.FIRSTNAME + ' ' + MB.LASTNAME AS SENIOURAGENT
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS < 1
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN PAYPERIOD P ON S.PAYPERIODID = P.ID
                JOIN DB_USER U ON U.ID = USS.USERID
                JOIN USERHISTORY UH ON U.ID = UH.USERID
                JOIN DB_USER MB  ON MB.ID = UH.TEAMID
                WHERE U.ID IN (SELECT USERID FROM USERHISTORY) {0} {1}
                GROUP BY S.ID, S.ADDRESS, U.FIRSTNAME + ' ' + U.LASTNAME, P.STARTDATE, MB.FIRSTNAME + ' ' + MB.LASTNAME
                ORDER BY MB.FIRSTNAME + ' ' + MB.LASTNAME, P.STARTDATE, S.ADDRESS
"
                , szFilter, szUserActive);
            DataSet ds = DB.runDataSet(szSQL);

            return ds;
        }
    }
}