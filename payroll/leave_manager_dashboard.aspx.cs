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
            if (hdArchiveID.Value != "") {
                DB.runNonQuery("UPDATE LEAVEREQUEST SET ISARCHIVED = 1 WHERE ID  = " + hdArchiveID.Value);
                hdArchiveID.Value = "";
            } else if (hdUnArchiveID.Value != "") {
                DB.runNonQuery("UPDATE LEAVEREQUEST SET ISARCHIVED = 0 WHERE ID  = " + hdUnArchiveID.Value);
                hdUnArchiveID.Value = "";
            }

            loadRequests();
            ModalForms.createModalUpdate("Leave request", "60%", "500px", false, true);
        }

        private void loadRequests() {
            string szWhere = string.Format("AND U.SUPERVISORID IN ({0})", G.User.UserIDListWithDelegates);
            if (G.User.IsAdmin) // Admin
                szWhere = "";
            if(txtSearch.Text != "") {
                szWhere += String.Format(@" AND(U.FIRSTNAME LIKE '{0}%' or U.LASTNAME LIKE '{0}%' OR U.INITIALSCODE = '{0}') ", DB.escape(txtSearch.Text));
            }
            if (!chkViewArchived.Checked) {
                szWhere += "AND LR.ISARCHIVED = 0 ";
            } 
            string szSQL = string.Format(@"
                SELECT  LR.*, L.NAME AS LEAVETYPE, LS.NAME AS LEAVESTATUS, U.FIRSTNAME + ' ' + U.LASTNAME + ' (' + U.INITIALSCODE + ')' AS STAFFMEMBER, 
                    CASE WHEN HOURS = 0 THEN CAST(TOTALDAYS as VARCHAR) + ' Days' ELSE CAST(HOURS AS VARCHAR) + ' Hrs' END as DURATION
                FROM LEAVEREQUEST LR JOIN LIST L ON L.ID = LR.LEAVETYPEID
                JOIN LEAVESTATUS LS ON LS.ID = LR.LEAVESTATUSID
                JOIN DB_USER U ON LR.USERID = U.ID
                WHERE LR.ISDELETED = 0  {0}
                ORDER BY LR.ENTRYDATE DESC", szWhere);
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

        public string getArchiveButton(int ID, bool IsArchived, string Status) {
            if (Status != "Approved" && Status != "Rejected")
                return "";
            if (IsArchived) {
                return String.Format(@"<button type='submit'class='btn btn-secondary unarchive' data-id='{0}'>Un-archive</button>", ID);
            }            
            return String.Format(@"<button type='submit'class='btn btn-secondary archive' data-id='{0}'>Archive</button>", ID);
        }

        protected void btnSearch_Click(object sender, EventArgs e) {

        }
    }
}