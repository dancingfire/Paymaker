using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class monthly_sales_detail : Root {
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
            Report oR = new MonthlySalesDetail(rViewer, oFilter);
            return oR;
        }
    }
}

public class MonthlySalesDetail : Report {
    private static string RFileName = "Monthly sales graph totals";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public MonthlySalesDetail(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Monthly sales detail";
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
        string szFilter = String.Format(" S.SALEDATE BETWEEN '{0}' AND '{1}'", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList))
            szFilter += String.Format(" AND S.OFFICEID IN ({0})", oFilter.OfficeIDList);
        if (!String.IsNullOrWhiteSpace(oFilter.CompanyID))
            szFilter += String.Format(" AND L_COMPANY.ID IN ({0})", oFilter.CompanyID);
        string szUserIDFilter = Valid.getText("szUserID", "", VT.List);
        if (G.CurrentUserRoleID != 1) //Filter for single user mode
            szFilter += " AND U1.ID IN (" + G.User.UserID + ") ";
        else if (!String.IsNullOrEmpty(oFilter.UserIDList)) {
            szFilter += " AND U1.ID IN (" + oFilter.UserIDList + ")";
        }

        string szSQL = string.Format(@"
                SELECT S.ID, S.ADDRESS,
                S.SALEDATE, S.SALEPRICE, S.SETTLEMENTDATE, S.GROSSCOMMISSION as TOTALCOMMISSION, S.CONJUNCTIONALCOMMISSION, USS.GRAPHCOMMISSION AS GRAPHTOTAL,
                    case WHEN S.PAYPERIODID IS NULL THEN 0 ELSE USS.ACTUALPAYMENT END AS ACTUALPAY,
                L_OFFICE.NAME, L_SALESPLIT.NAME AS COMMISSIONTYPE, SS.COMMISSIONTYPEID, USR.INITIALSCODE AS AGENTNAME
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS < 1
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = S.OFFICEID AND L_OFFICE.ISACTIVE = 1
                JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID
                JOIN  DB_USER U1 ON U1.ID = USR.TOPPERFORMERREPORTSETTINGS

                WHERE {0} AND SS.CALCULATEDAMOUNT > 0
                ORDER BY  ISNULL(S.PAYPERIODID, 1000), S.SALEDATE"
            , szFilter);

        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}