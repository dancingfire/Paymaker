using System;
using System.Data;
using System.Text;

namespace Paymaker {

    public partial class agent_payroll_reconcilliation : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        private StringBuilder oSB = new StringBuilder();
        private string szPayPeriodIDs = "";

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            hdOfficeID.Value = Valid.getText("szOfficeID", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);
            blnPrint = Valid.getBoolean("blnPrint", false);
            if (hdOfficeID.Value == "null")
                hdOfficeID.Value = "";
            if (hdCompanyID.Value == "null")
                hdCompanyID.Value = "";

            string szStartDate = Valid.getText("szStartDate", "", VT.TextNormal);
            string szEndDate = Valid.getText("szEndDate", "", VT.TextNormal);
            if (!String.IsNullOrEmpty(szStartDate)) {
                dtStart = DateTime.Parse(szStartDate);
                if (String.IsNullOrEmpty(szEndDate))
                    dtEnd = DateTime.Now;
                else
                    dtEnd = DateTime.Parse(szEndDate);
            }

            // Get the pay periods that overlap with this report
            foreach (PayPeriod oP in G.PayPeriodInfo.payPeriodList) {
                if (oP.StartDate >= dtStart && oP.StartDate <= dtEnd)
                    Utility.Append(ref szPayPeriodIDs, oP.ID.ToString(), ",");
            }
            if (String.IsNullOrEmpty(szPayPeriodIDs)) {
                Response.Write("There are no pay periods in the date period selected. Please increase the date range so at least one pay period is included.");
                Response.End();
            }

            pPageHeader.InnerHtml = pPageHeader.InnerHtml + "<br/> " + Utility.formatDate(dtStart) + " - " + Utility.formatDate(dtEnd);
            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szFilter = " ";
            if (!String.IsNullOrWhiteSpace(hdOfficeID.Value))
                szFilter += String.Format(" AND L_OFFICE.ID IN ({0})", hdOfficeID.Value);
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", hdCompanyID.Value);
            double dIncomeTotal = 0;
            double dExpensesTotal = 0;

            string szUserFilter = Valid.getText("szUserID", VT.List);
            if (G.User.RoleID != 1) //Filter for single user mode
                szUserFilter = G.User.UserID.ToString();

            string szSQL = String.Format(@"
                    SELECT U.ID FROM DB_USER U
                    JOIN LIST l_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    WHERE U.ID IN ({0}) {1}
                    ORDER BY LASTNAME, FIRSTNAME", szUserFilter, szFilter);

            using (DataSet dsUsers = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in dsUsers.Tables[0].Rows) {
                    string szUserID = dr["ID"].ToString();
                    dIncomeTotal = 0;
                    dExpensesTotal = 0;
                    int intUserID = Convert.ToInt32(szUserID);

                    string szUserName = DB.getScalar("SELECT FIRSTNAME + ' ' + LASTNAME FROM DB_USER WHERE ID = " + intUserID, "");
                    oSB.AppendFormat(@"
                        <div class='AgentHeader' style='width: 500px'>{0}</div>", szUserName);

                    string szFinalFilter = szFilter + " AND USS.USERID = " + intUserID;
                    oSB.AppendFormat(@"
                    ", szUserID);

                    szSQL = String.Format(@"
                        --0) Monthyly retainer
                        SELECT SUM(AMOUNT) AS RETAINER,
                            CAST(DATEPART(year, TXDATE) AS VARCHAR) + ' ' + CAST(DATEPART(month, TXDATE) as VARCHAR) AS PERIOD
                        FROM USERTX WHERE ACCOUNTID IN (SELECT ID FROM LIST WHERE NAME = 'Monthly retainer')
                        AND TXDATE BETWEEN '{1}' AND '{2} 23:59' AND ISDELETED = 0 AND USERID = {0}
                        GROUP BY CAST(DATEPART(year, TXDATE) AS VARCHAR) + ' ' + CAST(DATEPART(month, TXDATE) as VARCHAR);

                        --1) Commissions
                        SELECT SUM(USS.ACTUALPAYMENT) AS AMOUNT, P.ID
                        FROM USERSALESPLIT USS
                        JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                        JOIN SALE S ON SS.SALEID = S.ID
                        JOIN PAYPERIOD P ON P.ID = S.PAYPERIODID
                        WHERE USS.USERID = {0} AND S.STATUSID = 2  AND P.ID IN ({3})
                        GROUP BY P.ID;

                        -- 2) Mentoring bonus paid to the senior if they are mentoring any juniors
                        SELECT  S.CODE, S.ADDRESS, S.ENTITLEMENTDATE, S.CONJUNCTIONALCOMMISSION AS CONJUNCTIONALCOMMISSION, S.GROSSCOMMISSION,
                            MB.COMMISSIONBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
                        FROM MENTORBONUS MB
                        JOIN SALE S ON MB.SALEID = S.ID
                        JOIN DB_USER U ON U.ID = MB.USERID
                        WHERE MB.MENTORUSERID = {0} AND S.PAYPERIODID IN ({3}) AND S.STATUSID = 2
                        ORDER BY MB.USERID, S.ADDRESS;

                        --3) Deductions
                        SELECT ISNULL(SUM(TX.AMOUNT), 0) AS AMOUNT, ISNULL(SUM(TX.FLETCHERCONTRIBTOTAL), 0) AS FLETCHERAMOUNT, LISTTYPEID,
                            L.NAME AS DESCRIPTION, ISNULL(MAX(UA.AMOUNT), 0) AS BUDGET
                        FROM USERTX TX JOIN DB_USER USR ON USR.ID = TX.USERID AND USR.ID= {0} AND TX.ISDELETED = 0
                        JOIN LIST L ON TX.ACCOUNTID = L.ID
                        LEFT JOIN USERACCOUNT UA ON UA.USERID = USR.ID AND UA.ACCOUNTID = TX.ACCOUNTID
                        WHERE  TX.TXDATE BETWEEN '{1}' AND '{2} 23:59' AND NAME != 'Monthly retainer'
                        GROUP BY L.LISTTYPEID, L.NAME
                        ORDER BY L.NAME;

                        -- 4) Salary portion paid to the senior if any of the juniors are on a salary
                        SELECT  MB.SALARYBONUS AS AMOUNT, MB.USERID, 'Salary paid - ' + U.FIRSTNAME + ' ' + U.LASTNAME AS DESCRIPTION,
                            S.FIRSTNAME + ' ' + S.LASTNAME AS SENIOR , 0 as BUDGET, 0 AS FLETCHERAMOUNT, 2 as LISTTYPEID
                        FROM MENTORBONUS MB JOIN DB_USER U ON MB.USERID = U.ID
                        JOIN DB_USER S ON S.ID  = MB.MENTORUSERID
                        WHERE MB.MENTORUSERID = {0}  AND MB.PAYPERIODID IN({3})  AND MB.SALEID IS NULL AND MB.SALARYBONUS > 0
                        ORDER BY U.FIRSTNAME, U.LASTNAME;

                        -- 5) Deductions paid to mentors if this is a junior
                        SELECT  'Mentor bonus - ' + MAX(U.FIRSTNAME) + ' ' + MAX(U.LASTNAME) AS DESCRIPTION, SUM( MB.COMMISSIONBONUS) AS AMOUNT,
                           0 AS FLETCHERAMOUNT,  0 as BUDGET, 1 as LISTTYPEID
                        FROM MENTORBONUS MB
                        JOIN SALE S ON MB.SALEID = S.ID
                        JOIN DB_USER U ON U.ID = MB.MENTORUSERID
                        WHERE MB.USERID = {0} AND S.PAYPERIODID IN ({3}) AND S.STATUSID = 2 AND MB.COMMISSIONBONUS > 0
                        GROUP BY MB.USERID

                        UNION --SalaryBonus
                        SELECT  'Salary bonus - ' + MAX(U.FIRSTNAME) + ' ' + MAX(U.LASTNAME), SUM(MB.SALARYBONUS), 0, 0, 1
                        FROM MENTORBONUS MB
                        JOIN DB_USER U ON U.ID = MB.MENTORUSERID
                        WHERE MB.USERID = {0} AND MB.PAYPERIODID IN ({3}) AND MB.SALARYBONUS > 0
                        GROUP BY MB.USERID

                        UNION --Initial commission bonus of 5%
                        SELECT  'Initial commission payment - ' +  MAX(U.FIRSTNAME) + ' ' + MAX(U.LASTNAME), SUM(MB.COMMISSIONBONUS), 0, 0, 1
                        FROM MENTORBONUS MB
                        JOIN DB_USER U ON U.ID = MB.MENTORUSERID
                        WHERE MB.USERID = {0} AND MB.PAYPERIODID IN ({3}) AND MB.SALEID IS NULL AND MB.COMMISSIONBONUS > 0
                        GROUP BY MB.USERID

                        ORDER BY 1;

                    ", intUserID, Utility.formatDate(dtStart), Utility.formatDate(dtEnd), szPayPeriodIDs);
                    double dCommissions = 0;
                    double dRetainer = 0;
                    double dTotalPayable = 0;
                    DataSet dsData = DB.runDataSet(szSQL);
                    oSB.AppendFormat(@"
                    <table width='500'>
                        <tr><td colspan='5'><div class='ReportHeader'>Income</div>  </td></tr>
                        <tr>
                            <td ><b>Period</b></td>
                            <td align='right'><b>Payable comm</b></td>
                            <td align='right'><b>Heldover</b></td>
                            <td align='right'><b>Retainer</b></td>
                            <td align='right'><b>Actual payable</b></td>
                        </tr>");
                    foreach (PayPeriod oP in G.PayPeriodInfo.payPeriodList) {
                        double dMonthRetainer = 0, dMonthComm = 0, dMonthHeldover = 0, dMonthPayable = 0;
                        if (oP.StartDate >= dtStart && oP.StartDate <= dtEnd) {
                            //Business rule is that we either get the commission OR the retainer, if one has been paid
                            DataTable dtRetainer = new DataView(dsData.Tables[0], "PERIOD = '" + oP.StartDate.ToString("yyyy M") + "'", "", DataViewRowState.OriginalRows).ToTable();
                            if (dtRetainer.Rows.Count > 0) {
                                dMonthRetainer = Convert.ToDouble(dtRetainer.Rows[0]["RETAINER"]);
                                dRetainer += dMonthRetainer;
                            }
                            int intPrevPayperiodID = oP.ID - 1;
                            if (intPrevPayperiodID == 14)
                                intPrevPayperiodID = 13;

                            szSQL = string.Format(@"
                            SELECT HELDOVERAMOUNT
                            FROM USERPAYPERIOD WHERE USERID = {0} AND PAYPERIODID = {1}", intUserID, intPrevPayperiodID);
                            dMonthHeldover = Convert.ToDouble(DB.getScalar(szSQL, "0"));

                            double dDeductions = getPayPeriodExpense(oP, intUserID);
                            double dOtherIncome = getPayPeriodIncome(oP, intUserID);

                            DataTable dtComm = new DataView(dsData.Tables[1], "ID = " + oP.ID, "", DataViewRowState.OriginalRows).ToTable();
                            if (dtComm.Rows.Count > 0) {
                                dMonthComm = Convert.ToDouble(dtComm.Rows[0]["AMOUNT"]);
                                dCommissions += dMonthComm;
                            }

                            dMonthPayable = dMonthComm + dOtherIncome + dMonthHeldover - dDeductions;
                            if (dMonthPayable < 0)
                                dMonthPayable = 0;
                            dTotalPayable += dMonthPayable;
                            oSB.AppendFormat(@"
                            <tr>
                                <td>{0}</td>
                                <td align='right'>{1}</td>
                                <td align='right'>{2}</td>
                                <td align='right'>{3}</td>
                                <td align='right'>{4}</td>
                            </tr>
                           ", oP.StartDate.ToString("MMM yyyy"), Utility.formatMoney(dMonthComm), Utility.formatMoney(dMonthHeldover), Utility.formatMoney(dMonthRetainer), Utility.formatMoney(dMonthPayable));
                        }
                    }

                    oSB.AppendFormat(@"
                            <tr>
                                <td><b>Total</b></td>
                                <td align='right'><b>{0}</b></td>
                                <td align='right'><b>{1}</b></td>
                                <td align='right'>&nbsp;</td>
                            <td align='right'><b>{2}</b></td>
                            </tr>
                            </table>
                ", Utility.formatMoney(dCommissions), Utility.formatMoney(dRetainer), Utility.formatMoney(dTotalPayable));

                    addAccounts(intUserID, dsData, ref dIncomeTotal, ref dExpensesTotal, dRetainer + dCommissions);
                    oSB.AppendFormat("</table>");
                }
                oDetails.InnerHtml = oSB.ToString();
            }
        }

        private double getPayPeriodExpense(PayPeriod oP, int UserID) {
            string szSQL = String.Format(@"
                SELECT SUM(TX.AMOUNT) AS AMOUNT
                FROM USERTX TX
                JOIN LIST L ON ACCOUNTID = L.ID AND L.LISTTYPEID = 2
                WHERE  TX.USERID = {0} AND TX.ISDELETED = 0 AND TX.TXDATE BETWEEN '{1}' AND '{2} 23:59' AND NAME != 'Monthly retainer'", UserID, Utility.formatDate(oP.StartDate), Utility.formatDate(oP.EndDate));
            return Convert.ToDouble(DB.getScalar(szSQL, "0"));
        }

        private double getPayPeriodIncome(PayPeriod oP, int UserID) {
            string szSQL = String.Format(@"
                SELECT SUM(TX.AMOUNT) AS AMOUNT
                FROM USERTX TX
                JOIN LIST L ON ACCOUNTID = L.ID AND L.LISTTYPEID = 4
                WHERE  TX.TXDATE BETWEEN '{1}' AND '{2} 23:59'AND TX.USERID = {0} AND TX.ISDELETED = 0 ", UserID, Utility.formatDate(oP.StartDate), Utility.formatDate(oP.EndDate));
            return Convert.ToDouble(DB.getScalar(szSQL, "0"));
        }

        private void addAccounts(int UserID, DataSet ds, ref double dIncomeTotal, ref double dExpenseTotal, double dIncome) {
            DataTable dtAccounts = ds.Tables[3];

            double dBudgetTotal = 0, dFletcherTotal = 0;
            oSB.AppendFormat(@"
                <table width='500'>
                <tr>
                    <td colspan='4'>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan='4'><b>Allowances</b></td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td align='right'><b>Amount</b></td>
                    <td align='right'><b>Fletcher <br/>contribution</b></td>
                    <td align='right'><b>Budget</b></td>
                </tr>");
            foreach (DataRow dr in dtAccounts.Rows) {
                if (Convert.ToInt32(dr["LISTTYPEID"]) == (int)ListType.Income) {
                    oSB.AppendFormat(@"
                        <tr>
                            <td>&nbsp;&nbsp;{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>&nbsp;</td>
                            <td align='right'>{2}</td>
                        </tr>", dr["DESCRIPTION"].ToString(), Utility.formatMoney(Convert.ToDouble(dr["AMOUNT"])), Utility.formatMoney(Convert.ToDouble(dr["BUDGET"])), 0);
                    dIncomeTotal += Convert.ToDouble(dr["AMOUNT"]);
                    dBudgetTotal += Convert.ToDouble(dr["BUDGET"]);
                }
            }
            oSB.AppendFormat(@"
                <tr>
                    <td><strong>Total allowances</strong></td>
                    <td align='right'><b>{0}</b></td>
                    <td align='right'><b>&nbsp;</b></td>
                    <td align='right'><b>{1}</b></td>
                </tr>
                <tr>
                    <td colspan='3'>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan='3'><strong>Deductions</strong></td>
                </tr>", Utility.formatMoney(dIncomeTotal), Utility.formatMoney(dBudgetTotal));

            dBudgetTotal = 0;
            for (int i = 3; i < 5; i++) {
                foreach (DataRow dr in ds.Tables[i].Rows) {
                    if (Convert.ToInt32(dr["LISTTYPEID"]) == (int)ListType.Expense) {
                        oSB.AppendFormat(@"
                        <tr>
                            <td>&nbsp;&nbsp;{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}</td>
                            <td align='right'>{3}</td>
                        </tr>", dr["DESCRIPTION"].ToString(), Utility.formatMoney(Convert.ToDouble(dr["AMOUNT"])), Utility.formatMoney(Convert.ToDouble(dr["FLETCHERAMOUNT"])), Utility.formatMoney(Convert.ToDouble(dr["BUDGET"])));
                        dExpenseTotal += Convert.ToDouble(dr["AMOUNT"]);
                        dFletcherTotal += Convert.ToDouble(dr["FLETCHERAMOUNT"]);
                        dBudgetTotal += Convert.ToDouble(dr["BUDGET"]);
                    }
                }
            }

            oSB.AppendFormat(@"
                <tr>
                    <td><strong>Total deductions</strong></td>
                    <td align='right'><strong>{0}</strong></td>
                    <td align='right'><strong>{1}</strong></td>
                    <td align='right'><strong>{2}</strong></td>
               </tr>
                <tr>
                    <td colspan='3'>&nbsp;</td>
                </tr>
                ", Utility.formatMoney(dExpenseTotal), Utility.formatMoney(dFletcherTotal), Utility.formatMoney(dBudgetTotal));
        }
    }
}