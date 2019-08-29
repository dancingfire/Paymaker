using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Paymaker {

 
    public partial class staff_list : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            // Check which pages user should have access to
            if (!Page.IsPostBack) {
                loadStaff();
               
            }
        }

        private void loadStaff() {
            string szSQL = string.Format(@"
              select U.FIRSTNAME + ' ' + U.LASTNAME as Name, U.EMail, 
                CASE WHEN U.PAYROLLCYCLEID = 1 THEN 'NORMAL'  WHEN U.PAYROLLCYCLEID = 2 THEN 'Pay in advance' END AS CYCLE
                from DB_USER U
                WHERE U.PAYROLLCYCLEID > 0 AND U.ISDELETED = 0
                ORDER BY NAME");
            using (DataSet ds = DB.runDataSet(szSQL)) {
                Utility.bindGV(ref gvList, ds, true);
                HTML.formatGridView(ref gvList, true, true);
            }
        }
    }
}