using System;
using System.Collections.Generic;
using System.Data;

namespace PAYMAKER {

    public partial class admin_tasks : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            if (!IsPostBack) {
                Utility.bindAgentList(ref lstAgent);
            }
        }

        protected void btnUpdateGraphTotals_Click(object sender, EventArgs e) {
            string szSQL = @"
                SELECT ID
                FROM SALE S
                WHERE S.SALEDATE >= 'JUL 1, 2014'";
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    Sale oS = new Sale(DB.readInt(dr["ID"]));
                    oS.updateGraphTotals();
                }
            }
            G.Notifications.addPageNotification(PageNotificationType.Success, "Graphs totals updated", "The graph totals have been updated.");
            G.Notifications.showPageNotification();
        }
        

        protected void btnTestTimesheet_Click(object sender, EventArgs e) {
            string Msg = "Started correctly.";
            try {
                DBLog.addRecord(DBLogType.EmailAutomation, "Checking Timesheet emails", -1, -1);
                G.User.ID = -1;
                G.User.UserID = -1;
                G.User.RoleID = 1;
                TimesheetCycle.checkAutomatedEmails();
            } catch (Exception e1) {
                Msg = "ERROR: " + e1.Message;
            }

            G.Notifications.addPageNotification(PageNotificationType.Success, "Status", Msg);
            G.Notifications.showPageNotification();
        }


        protected void btnUpdateAgentActual_Click(object sender, EventArgs e) {
            string szPPID = G.PayPeriodInfo.getPayPeriodsForYTD(-1, Convert.ToInt32(lstAgentFinYear.SelectedValue));

            string szSQL = String.Format(@"
                SELECT ID, S.PAYPERIODID
                FROM SALE S
                WHERE S.PAYPERIODID IS NOT NULL AND S.PAYPERIODID IN ({1}) AND S.ID IN (
                    SELECT SS.SALEID
                    FROM USERSALESPLIT USS
                        JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                    WHERE USS.USERID = {0})
                ORDER BY S.PayPeriodID ", lstAgent.SelectedValue, szPPID);
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    Sale oS = new Sale(DB.readInt(dr["ID"]));

                    oS.updateActualPay(Convert.ToInt32(lstAgent.SelectedValue));
                    oS.updateGraphTotals();
                }
            }

            G.Notifications.addPageNotification(PageNotificationType.Success, "Actual pay updated", "The actual pay has been updated.");
            G.Notifications.showPageNotification();
        }

        protected void btnUpdateAnnualEOYB_Click(object sender, EventArgs e) {
            string szPPID = G.PayPeriodInfo.getPayPeriodsForYTD(-1, Convert.ToInt32(lstFinYear.SelectedValue));
            List<CommissionTier> lTiers = Client.getCommissionTiers(0, 0);
            string szSQL = String.Format(@"
                SELECT DISTINCT USS.USERID, SUM(USS.GRAPHCOMMISSION)
                FROM SALE S
                JOIN SALESPLIT SS ON S.ID = SS.SALEID AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                WHERE S.PAYPERIODID in ({0})
                   GROUP BY USS.USERID
                   HAVING SUM(USS.GRAPHCOMMISSION) > {1}", szPPID, lTiers[0].dUpperAmount);
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    foreach (string PayPeriodID in szPPID.Split(',')) {
                        UserPayPeriod uPP = new UserPayPeriod(Convert.ToInt32(lstAgent.SelectedValue), Convert.ToInt32(PayPeriodID));
                        uPP.calcEOFYBonusScheme();
                    }
                }
            }
            G.Notifications.addPageNotification(PageNotificationType.Success, "EOFY bonus calculated", "The EOFY bonus claculation has been updated.");
            G.Notifications.showPageNotification();
        }

        protected void btnReload_Click(object sender, EventArgs e) {
            G.CommTypeInfo.forceReload();
            G.CTInfo.forceReload();
            G.UserInfo.forceReload();
            G.PayPeriodInfo.forceReload();
            G.TimeSheetCycleReferences.forceReload();
            G.UserDelegateInfo.forceReload();
        }
    }
}