using System;
using System.Web.UI;

namespace Paymaker {

    public partial class top_advertising_detail : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        private bool blnGroupByMunicipality = false;

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

            blnPrint = Valid.getBoolean("blnPrint", false);

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
            oFilter.SuburbIDList = Valid.getText("szSuburbID", "", VT.NoValidation).Replace(",", "','");
            oFilter.StartDate = dtStart;
            oFilter.EndDate = dtEnd;
            oFilter.OfficeIDList = Valid.getText("szOfficeID", "", VT.NoValidation);
            oFilter.UserIDList = Valid.getText("szUserID", "", VT.NoValidation);
            oFilter.RoleID = Valid.getText("szRoleID", "", VT.TextNormal);
            oFilter.CompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
            oFilter.PayPeriodID = Valid.getText("szPayPeriod", "", VT.TextNormal);
            return new Top_Advertising_Detail(rViewer, oFilter);
        }
    }
}