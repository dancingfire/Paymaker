using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class agent_off_the_top : Root {
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
            Report oR = new Agent_OffTheTop(rViewer, oFilter);
            return oR;
        }
    }
}

public class Agent_OffTheTop : Report {
    private static string RFileName = "Agent off the top";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public Agent_OffTheTop(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Agent off the top";
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

        if (!String.IsNullOrWhiteSpace(oFilter.UserIDList))
            szFilter += " AND USS.USERID IN (" + oFilter.UserIDList + ")";

        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        string szUserActive = " AND USR.ISACTIVE = 1 ";
        if (blnIncludeInactive)
            szUserActive = "";

        if (!String.IsNullOrWhiteSpace(oFilter.OffTheTopCategoryIDList))
            szFilter += " AND SE.EXPENSETYPEID IN (" + oFilter.OffTheTopCategoryIDList + ") ";

        string szSQL = string.Format(@"
                SELECT DISTINCT SE.ID, SE.CALCULATEDAMOUNT AS AMOUNT, L.NAME AS EXPENSE, USR.FIRSTNAME + ' ' + USR.LASTNAME AS AGENTNAME, S.ADDRESS, S.SALEDATE
                FROM SALEEXPENSE SE JOIN LIST L ON L.ID = SE.EXPENSETYPEID
                JOIN SALE S ON SE.SALEID = S.ID
                JOIN SALESPLIT SS ON S.ID = SS.SALEID AND SS.COMMISSIONTYPEID = 10
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID --AND INCLUDEINKPI = 1
                JOIN DB_USER USR ON USS.USERID = USR.ID
                WHERE SE.CALCULATEDAMOUNT > 0  {0} {1}
                ORDER BY USR.FIRSTNAME + ' ' + USR.LASTNAME, L.NAME
                ;", szFilter, szUserActive);

        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}