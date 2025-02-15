using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Paymaker {

    /// <summary>
    /// Manages the leave functions both for line staff and admin staff
    /// </summary>
    public partial class leave_dashboard : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            // Check which pages user should have access to
            if (!Page.IsPostBack) {
                loadRequests();
                btnSuperviser.Visible = Payroll.IsLeaveSupervisor || G.User.IsAdmin;
            }
            ModalForms.createModalUpdate("Leave request", "60%", "500px", false, true);
        }

        private void loadRequests() {
            string szSQL = string.Format(@"
                SELECT  LR.*, L.NAME AS LEAVETYPE, LS.NAME AS LEAVESTATUS,
                    CASE WHEN HOURS = 0 THEN CAST(TOTALDAYS as VARCHAR) + ' Days' ELSE CAST(HOURS AS VARCHAR) + ' Hrs' END as DURATION
                FROM LEAVEREQUEST LR JOIN LIST L ON L.ID = LR.LEAVETYPEID
                JOIN LEAVESTATUS LS ON LS.ID = LR.LEAVESTATUSID
                WHERE LR.USERID = {0} AND ISDELETED = 0 
                ORDER BY LR.ENTRYDATE DESC", G.User.ID);
            using (DataSet ds = DB.runDataSet(szSQL)) {
                Utility.bindGV(ref gvList, ds, true);
            }
        }

        protected void gvList_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e) {
            if (e.Row.RowType == DataControlRowType.DataRow) {
                string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
                e.Row.Attributes["onclick"] = String.Format("editRequest({0})", szID);
                e.Row.CssClass += " trEdit";
            }
        }
    }
}