using System;
using System.Web.UI;

namespace Paymaker {

    public partial class top_advertising_average : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        private bool blnGroupByMunicipality = false;

        protected void Page_Load(object sender, EventArgs e) {
            blnShowMenu = false;
            string szStartDate = Valid.getText("szStartDate", "", VT.TextNormal);
            string szEndDate = Valid.getText("szEndDate", "", VT.TextNormal);

            blnGroupByMunicipality = Valid.getBoolean("blnGroupByMunicipality", false);

            if (!String.IsNullOrEmpty(szStartDate)) {
                //blnIsMultiMonth = true;
                dtStart = DateTime.Parse(szStartDate);
                if (!String.IsNullOrEmpty(szEndDate))
                    dtEnd = DateTime.Parse(szEndDate);
            }

            if (blnGroupByMunicipality) {
                this.Title = "KPI by municipality";
            } else {
                this.Title = "KPI by suburb";
            }

            blnPrint = Valid.getBoolean("blnPrint", false);

            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }

            if (!Page.IsPostBack) {
                Report oReport = getReport("KPI by company");

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
            oFilter.SuburbIDList = Valid.getText("szSuburbID", "", VT.NoValidation).Replace(",", "','");
            oFilter.StartDate = dtStart;
            oFilter.EndDate = dtEnd;
            oFilter.OfficeIDList = Valid.getText("szOfficeID", "", VT.NoValidation);
            oFilter.UserIDList = Valid.getText("szUserID", "", VT.NoValidation);
            oFilter.RoleID = Valid.getText("szRoleID", "", VT.TextNormal);
            oFilter.CompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
            oFilter.PayPeriodID = Valid.getText("szPayPeriod", "", VT.TextNormal);
            Report oR = new Top_Advertising_Average(rViewer, oFilter);
            return oR;
        }
    }
}