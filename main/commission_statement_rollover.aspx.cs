using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class commission_statement_rollover : Root {
        private DataTable dtTotal = null;
        private PayPeriod oP = null;

        protected void Page_Load(object sender, System.EventArgs e) {
            if (!Page.IsPostBack)
                loadFilters();
        }

        protected void loadFilters() {
            string szSQL = string.Format("select ID, STARTDATE , ENDDATE, '' AS NAME from PAYPERIOD ORDER BY ID DESC");
            DataSet ds = DB.runDataSet(szSQL);

            foreach (DataRow dr in ds.Tables[0].Rows) {
                dr["NAME"] = Utility.formatDate(dr["StartDate"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
            }
            Utility.BindList(ref lstPayPeriod, ds, "ID", "NAME");
            lstPayPeriod.SelectedIndex = 1;
            loadPayPeriod();
        }

        protected void btnFinalize_Click(object sender, EventArgs e) {
            createRecords();
        }

        void createRecords() {
            DB.runNonQuery(String.Format(@"
            	INSERT INTO USERTX(ACCOUNTID, USERID, AMOUNT, FLETCHERAMOUNT, TXDATE, COMMENT, AMOUNTTYPEID, FLETCHERCONTRIBTOTAL, SHOWEXGST, TOTALAMOUNT, CREDITGLCODE, DEBITGLCODE, CREDITJOBCODE, DEBITJOBCODE, OVERRIDEGLCODES)
				SELECT 66, UPP.USERID, UPP.DISTRIBUTIONOFFUNDS, 0, DATEADD(month, 1, PP.ENDDATE), 'Carried forward balance', 1, 0, 1, UPP.DISTRIBUTIONOFFUNDS, '2-3035', O.OFFICEMYOBCODE + '-' + U.INITIALSCODE, O.OFFICEMYOBCODE + '-' + U.INITIALSCODE, O.OFFICEMYOBCODE + '-' + U.INITIALSCODE, 1
				FROM USERPAYPERIOD UPP JOIN DB_USER U ON UPP.USERID = U.ID JOIN PAYPERIOD PP ON PP.ID = UPP.PAYPERIODID JOIN LIST O ON U.OFFICEID = O.ID
				WHERE UPP.ID in ({0})", hdSelValues.Value));
            gvData.Visible = false;
            G.Notifications.addPageNotification(PageNotificationType.Success, "Records created", "The commission balance records have been created!!");
            G.Notifications.showPageNotification();
        }

        protected void btnPreview_Click(object sender, EventArgs e) {
            loadPayPeriod();
        }

        private void loadPayPeriod() {
            int PayPeriod = Convert.ToInt32(lstPayPeriod.SelectedValue);
            string szSQL = String.Format(@"
                select U.FIRSTNAME + ' ' + U.LASTNAME + ' (' + U.INITIALSCODE + ')' AS NAME, UPP.ID, UPP.DISTRIBUTIONOFFUNDS, '' As CHECKBOX
                from USERPAYPERIOD UPP
                JOIN DB_USER U on U.ID = UPP.USERID
                JOIN PAYPERIOD P ON UPP.PAYPERIODID = P.ID
                WHERE UPP.PAYPERIODID = {0} AND UPP.DISTRIBUTIONOFFUNDS != 0
                ORDER BY U.FIRSTNAME, U.LASTNAME
                ", PayPeriod);
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    dr["CHECKBOX"] = String.Format("<input type='checkbox' class='check' id='chkSel{0}' data-id='{0}' name='chkSel{0}' />", DB.readInt(dr["ID"]));
                }
                gvData.DataSource = ds;
                gvData.DataBind();
                gvData.Visible = btnFinalize.Visible = true;
                HTML.formatGridView(ref gvData, true);
            }
        }
    }
}