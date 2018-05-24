using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class eofy_bonus_detail : Root {
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
                if (!String.IsNullOrEmpty(szEndDate))
                    dtEnd = DateTime.Parse(szEndDate);
            }

            this.Title = "SBS summary report";

            if (!Page.IsPostBack) {
                Report oReport = getReport("SBS summary");
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
            oFilter.UserIDList = Valid.getText("szUserID", "", VT.NoValidation);
            Report oR = new EOFY_Bonus_Detail(rViewer, oFilter);
            return oR;
        }
    }

    public class EOFY_Bonus_Detail : Report {
        private static string RFileName = "EOFY bonus detail";

        /// <summary>
        /// Constructor Method that receives the Viewer and the Filter and creates the report.
        /// </summary>
        /// <param name="Viewer"></param>
        /// <param name="oFilter"></param>
        public EOFY_Bonus_Detail(ReportViewer Viewer, ReportFilters oFilter) {
            oViewer = Viewer;
            this.oFilter = oFilter;
            ReportTitle = "EOFY bonus detail";
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

            bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
            string szUserActive = " AND U.ISACTIVE = 1 ";
            if (blnIncludeInactive)
                szUserActive = "";

            string szSQL = string.Format(@"
                SELECT SUM(USS.GRAPHCOMMISSION) AS PAYPERIOD_COMMISSION, MAX(U.INITIALSCODE) AS INITIALSCODE, MIN(P.STARTDATE) as PAYPERIODDATE,
                    ISNULL(MAX(UPP.EOFYBONUS), 0) AS PAYSLIP_BONUS, S.PAYPERIODID, USS.USERID, SUM(ISNULL(USS.EOFYBONUSCOMMISSION, 0)) as CALCULATED_BONUS
                    FROM USERSALESPLIT USS
                    JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1   AND USS.RECORDSTATUS < 1
                    JOIN SALE S ON SS.SALEID = S.ID
                    JOIN DB_USER U ON U.ID = USS.USERID
                    LEFT JOIN COMMISSIONTIER CT ON USS.COMMISSIONTIERID = CT.ID
                    LEFT JOIN USERPAYPERIOD UPP ON UPP.USERID = U.ID  AND UPP.PAYPERIODID = S.PayPeriodID
                    JOIN PAYPERIOD P On P.ID = S.PAYPERIODID
                WHERE  S.STATUSID = 2 AND S.PAYPERIODID IN ({0})  {1}
                GROUP BY USS.USERID, S.PAYPERIODID
                HAVING SUM(ISNULL(USS.EOFYBONUSCOMMISSION, 0)) > 0
                ORDER BY MAX(U.INITIALSCODE) DESC, PAYPERIODID"
                , szPayPeriodIDs, szUserActive);
            DataSet ds = DB.runDataSet(szSQL);
            return ds;
        }
    }
}