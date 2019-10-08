using System.Web.UI;

namespace Paymaker {

    /// <summary>
    /// Manages the payroll functions both for line staff and admin staff
    /// </summary>
    public partial class payroll_dashboard : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            // Check which pages user should have access to
            dPayrollAdmin.Visible = G.User.IsAdmin || Payroll.IsPayrollSupervisor;
            dPDFFiles.Visible = G.User.IsAdmin;
            dPayrollManagement.Visible = Payroll.HasTimesheet;
            dCheck.Visible = G.User.IsAdmin;

            /*
             * 1) Need to create a scheduled task that runs each day - check to see whether the next payment cycle needs to be created - create the next one 3 days into the pay cycle
             *      search for checkTimesheetCycles
             * 2) Check whether email reminder needs to go out to people - goes on day of paysheet
             * 3) Also check whether paysheets are due that have not been filled in - send reminder email if this is the case
             */

            if (!Page.IsPostBack) {
            }
        }
    }
}