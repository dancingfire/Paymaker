using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Paymaker {

    /// <summary>
    /// Manages the leave functions both for line staff and admin staff
    /// </summary>
    public partial class leave_manager_dashboard : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            // Check which pages user should have access to
            if (!Page.IsPostBack) {
                loadRequests();
            }
            ModalForms.createModalUpdate("Leave request", "60%", "500px", false, true);
        }

        private void loadRequests() {
            string szSupervisor = string.Format("AND U.SUPERVISORID = {0}", G.User.UserID);
            if (G.User.RoleID == 1) // Admin
                szSupervisor = "";

            string szSQL = string.Format(@"
                SELECT  LR.*, L.NAME AS LEAVETYPE, LS.NAME AS LEAVESTATUS, U.FIRSTNAME + ' ' + U.LASTNAME AS STAFFMEMBER
                FROM LEAVEREQUEST LR JOIN LIST L ON L.ID = LR.LEAVETYPEID
                JOIN LEAVESTATUS LS ON LS.ID = LR.LEAVESTATUSID
                JOIN DB_USER U ON LR.USERID = U.ID
                WHERE LR.ISDELETED = 0  {0}
                ORDER BY LR.ENTRYDATE DESC", szSupervisor);
            using (DataSet ds = DB.runDataSet(szSQL)) {
                Utility.bindGV(ref gvList, ds, true, true);
            }
        }

        protected void gvList_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e) {
            if (e.Row.RowType == DataControlRowType.DataRow) {
                string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
                e.Row.Attributes["onclick"] = String.Format("editRequest({0})", szID);
                if (((DataRowView)e.Row.DataItem)["LEAVESTATUS"].ToString() == "Requested") {
                    e.Row.CssClass += "  bold";
                }
            } else if (e.Row.RowType == DataControlRowType.EmptyDataRow) {
                gvList.BorderWidth = 0;
            }
        }
    }
}