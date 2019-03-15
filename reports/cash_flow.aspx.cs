using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class cash_flow : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>sfs
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
            Report oR = new CashFlow(rViewer, oFilter);
            return oR;
        }
    }
}

public class CashFlow : Report {
    private static string RFileName = "Cash flow";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public CashFlow(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Cash flow";
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
        string szFilter = String.Format(" S.SALEDATE  BETWEEN '{0}' AND '{1}' ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList))
            szFilter += " AND USR.OFFICEID IN (" + oFilter.OfficeIDList + ")";

        string szSQL = string.Format(@"
                SELECT AVG(DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE)) AS AVGDAYS, 
                CAST(CAST(DATEPART(YEAR, S.SALEDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH, S.SALEDATE) AS VARCHAR), 2) AS INT) AS MONTH,
                L_OFFICE.NAME as OFFICE, AVG(S.GROSSCOMMISSION) AS AVGCOMM, COUNT(DISTINCT(S.ID)) AS SALECOUNT,
                COUNT (DISTINCT(CASE WHEN DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE) <= 30 then S.ID ELSE NULL END)) AS LESSTHAN30,
			    COUNT (DISTINCT(CASE WHEN DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE) > 30 AND DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE) <=60  then S.ID ELSE NULL END)) AS DAYS30TO60,              
			    COUNT (DISTINCT(CASE WHEN DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE) > 60 AND DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE) <=90 then S.ID ELSE NULL END)) AS DAYS60TO90, 
				COUNT (DISTINCT(CASE WHEN DATEDIFF(D, S.SALEDATE, S.ENTITLEMENTDATE) > 90 then S.ID ELSE NULL END)) AS GREATERTHAN90     
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS < 1
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                WHERE {0} AND ENTITLEMENTDATE IS NOT NULL
                AND S.STATUSID IN (0, 1, 2)
                GROUP BY L_OFFICE.NAME, CAST(CAST(DATEPART(YEAR, S.SALEDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH, S.SALEDATE) AS VARCHAR), 2) AS INT)
                ORDER BY L_OFFICE.NAME, CAST(CAST(DATEPART(YEAR, S.SALEDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH, S.SALEDATE) AS VARCHAR), 2) AS INT)
                ;", szFilter);

        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}