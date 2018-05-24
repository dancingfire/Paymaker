using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class farming_commission : Root {
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
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }

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
            Report oR = new FarmingCommission(rViewer, oFilter);
            return oR;
        }
    }
}

public class FarmingCommission : Report {
    private static string RFileName = "Farming commission";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public FarmingCommission(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Service Area commission";
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
        string szFilter = String.Format(" AND ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE)  BETWEEN '{0}' AND '{1}' ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList))
            szFilter += " AND USS_M.OFFICEID IN (" + oFilter.UserIDList + ")";

        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        string szUserActive = " AND USR_M.ISACTIVE = 1 ";
        if (blnIncludeInactive)
            szUserActive = "";

        string szSQL = string.Format(@"
                SELECT DISTINCT S.ID, L_O.DESCRIPTION AS LISTEROFFICE, USR_M.INITIALSCODE AS MENTOR, M_O.DESCRIPTION AS MENTOROFFICE, S.ADDRESS,
                S.SALEDATE, USS_M.GRAPHCOMMISSION, USS_M.ACTUALPAYMENT AS AMOUNT, S.ENTITLEMENTDATE
                FROM SALE S
                JOIN SALESPLIT SS_L ON S.ID = SS_L.SALEID AND SS_L.COMMISSIONTYPEID = 10                --Lister
                JOIN USERSALESPLIT USS_L ON USS_L.SALESPLITID = SS_L.ID AND USS_L.INCLUDEINKPI = 1
                JOIN DB_USER USR_L ON USS_L.USERID = USR_L.ID
                JOIN LIST L_O ON L_O.ID = USR_L.OFFICEID
                JOIN SALESPLIT SS_M ON S.ID = SS_M.SALEID AND SS_M.COMMISSIONTYPEID = 9                --Mentor
                JOIN USERSALESPLIT USS_M ON USS_M.SALESPLITID = SS_M.ID
                JOIN DB_USER USR_M ON USS_M.USERID = USR_M.ID
                JOIN LIST M_O ON M_O.ID = USR_M.OFFICEID
                WHERE USS_M.ACTUALPAYMENT > 0 {0} {1}
                ORDER BY USR_M.INITIALSCODE
                ;", szFilter, szUserActive);

        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}