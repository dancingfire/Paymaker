using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class campaign_prepayment_n : Root {
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
            Report oR = new CampaignPrepayment(rViewer, oFilter);
            return oR;
        }
    }
}

public class CampaignPrepayment : Report {
    private static string RFileName = "Campaign prepayment";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public CampaignPrepayment(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Campaign prepayment";
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
        string szCampaignFilterSQL = string.Format(@" WHERE ADDRESS1 NOT LIKE '%PROMO,%' AND C.STARTDATE BETWEEN '{0}' AND '{1}' ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        DataTable dt = CampaignDB.loadPrePaymentCampaignData(szCampaignFilterSQL, oFilter.CompanyID);

        // Wrap datatable in DataSet.
        DataSet ds = new DataSet();
        ds.Tables.Add(dt);

        return ds;
    }
}