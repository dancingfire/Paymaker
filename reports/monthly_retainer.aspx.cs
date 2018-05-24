using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class monthly_retainer : Root {
        private DataSet dsData = null;
        private DataSet dsFutureData = null;
        private DataSet dsOtherIncome = null;
        private DataSet dsDeductions = null;
        private double dHeldOverAmount = 0;
        private int intPayPeriodID = -1;
        private bool blnPrint = false;
        private int intCurrUserID = -1;

        private string szReportTitle = "";

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            if (!Page.IsPostBack) {
                intPayPeriodID = Valid.getInteger("szPayPeriod", -1);
                DateTime dtStart = Convert.ToDateTime(DB.getScalar("SELECT STARTDATE FROM PAYPERIOD WHERE ID = " + intPayPeriodID));
                blnPrint = Valid.getBoolean("blnPrint", false);
                szReportTitle = "Retainer list for " + dtStart.ToString("MMMM yyyy");
                pPageHeader.InnerHtml = szReportTitle;
                string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
                string szFilter = "";
                if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                    szFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

                DataSet dsUsers = DB.runDataSet(String.Format(@"
                    SELECT U.ID, FIRSTNAME + ' ' + LASTNAME AS NAME, 0 AS INCLUDE, 0 as AMOUNT
                    FROM DB_USER U JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    WHERE U.ISACTIVE = 1 {0}
                    ORDER by FIRSTNAME, LASTNAME", szFilter));
                foreach (DataRow dr in dsUsers.Tables[0].Rows) {
                    intCurrUserID = Convert.ToInt32(dr["ID"]);
                    double dDistribAmount = getTotalDistribution();
                    if (dDistribAmount < G.Settings.RetainerThreshhold && dDistribAmount != 0) {
                        dr["AMOUNT"] = dDistribAmount;
                        dr["INCLUDE"] = "1";
                    }
                }
                DataView dv = dsUsers.Tables[0].DefaultView;
                dv.RowFilter = "INCLUDE = 1";
                gvTable.DataSource = dv;
                gvTable.DataBind();
                HTML.formatGridView(ref gvTable, false);

                if (blnPrint) {
                    sbEndJS.Append("printReport();");
                }
            }
        }

        protected double getTotalDistribution() {
            //till date
            dsData = Sale.loadCommissionStatementForPayPeriod(intCurrUserID, intPayPeriodID);
            dsFutureData = Sale.loadFutureCommissionStatement(intCurrUserID, intPayPeriodID);
            dsOtherIncome = UserTX.loadUserTx(intCurrUserID, intPayPeriodID, ListType.Income);
            dsDeductions = UserTX.loadUserTx(intCurrUserID, intPayPeriodID, ListType.Expense);
            int intPrevPayperiodID = intPayPeriodID - 1;
            if (intPrevPayperiodID == 14)
                intPrevPayperiodID = 13;

            string szSQL = string.Format(@"
                SELECT HELDOVERAMOUNT
                FROM USERPAYPERIOD WHERE USERID = {0} AND PAYPERIODID = {1}", intCurrUserID, intPrevPayperiodID);
            dHeldOverAmount = Convert.ToDouble(DB.getScalar(szSQL, "0"));

            ListTypes oListTypes = new ListTypes(ListType.Commission);
            double dTotalCumulative = 0;
            double dTotalCommission = 0;
            double dRowCumulative = 0;
            double dRowCommission = 0;
            foreach (ListObject oCommissionType in G.CommTypeInfo.CommissionTypeList) {
                DataView oDataView = dsData.Tables[0].DefaultView;
                oDataView.RowFilter = string.Format(@"COMMISSIONTYPEID = {0}", (int)oCommissionType.ID);
                if (oDataView.Count > 0) {
                    foreach (DataRowView oRowView in oDataView) {
                        dRowCumulative = 0;
                        dRowCommission = 0;
                        if (Double.TryParse(oRowView["ACTUALPAYMENT"].ToString(), out dRowCumulative))
                            dTotalCumulative += dRowCumulative;
                        if (Double.TryParse(oRowView["ACTUALPAYMENT"].ToString(), out dRowCommission))
                            dTotalCommission += dRowCommission;
                    }
                }
            }

            double dOtherIncomeTotalCumulative = 0;
            double dRowOtherIncomeCumulative = 0;
            foreach (DataRow oRow in dsOtherIncome.Tables[0].Rows) {
                if (Double.TryParse(oRow["AMOUNT"].ToString(), out dRowOtherIncomeCumulative))
                    dOtherIncomeTotalCumulative += dRowOtherIncomeCumulative;
            }
            //Deductions
            double dDeductionsTotalCumulative = 0;
            double dRowDeductionsCumulative = 0;
            if (dHeldOverAmount != 0) {
                DataRow dr = dsDeductions.Tables[0].NewRow();
                dr["NAME"] = "Heldover from previous statement";
                double dDBAmount = dHeldOverAmount;
                if (dDBAmount < 0)
                    dDBAmount = dDBAmount * -1;
                dr["AMOUNT"] = dDBAmount;
                dsDeductions.Tables[0].Rows.Add(dr);
            }
            foreach (DataRow oRow in dsDeductions.Tables[0].Rows) {
                if (Double.TryParse(oRow["AMOUNT"].ToString(), out dRowDeductionsCumulative))
                    dDeductionsTotalCumulative += dRowDeductionsCumulative;
            }

            double dTotalDistributionOfFunds = dTotalCumulative + dOtherIncomeTotalCumulative - dDeductionsTotalCumulative;

            return dTotalDistributionOfFunds;
        }
    }
}