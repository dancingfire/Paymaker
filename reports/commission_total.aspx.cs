using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class commission_total : Root {
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
            Report oR = new CommissionTotal(rViewer, oFilter);
            return oR;
        }
    }
}

public class CommissionTotal : Report {
    private static string RFileName = "Commission total";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public CommissionTotal(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Mentoring commission";
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
        string szFilter = String.Format(" AND P.STARTDATE  BETWEEN '{0}' AND '{1}' ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList))
            szFilter += " AND U.OFFICEID IN (" + oFilter.OfficeIDList + ")";

        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        string szUserActive = " AND U.ISACTIVE = 1 ";
        if (blnIncludeInactive)
            szUserActive = "";

        string szSQL = string.Format(@"
               SELECT  MAX(U.LASTNAME) + ',' +  MAX(U.FIRSTNAME) AS EMPLOYEE, SUM(INCOME) AS SALARY, MAX(C.NAME) AS COMPANY,  MAX(L.OFFICEMYOBCODE) AS DEPT
				, SUM(UPP.SUPERPAID) AS SUPER
                from USERPAYPERIOD UPP JOIN DB_USER U ON UPP.USERID = U.ID
                join LIST l on U.OFFICEID = l.ID 
                JOIN LIST C ON L.COMPANYID = C.ID
                JOIN PAYPERIOD P On UPP.PAYPERIODID = P.ID
                WHERE U.ISPAID = 1 {0} {1}
                GROUP BY C.NAME, U.ID
				ORDER BY C.NAME, MAX(U.LASTNAME), MAX(U.FIRSTNAME)
                ;", szFilter, szUserActive);

        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}