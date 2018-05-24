using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;

namespace Paymaker {

    public partial class commission_statement : Root {
        private int intPayPeriodID = -1;
        private int intCurrUserID = -1;
        private string szCurrHTML = "";
        private string szCommissionSummaryHTML = "";
        private StringBuilder sbMainReport = new StringBuilder();
        private string szReportTitle = "";
        private PayPeriod oPayPeriod = null;
        private UserDetail oUser = null;
        private UserPayPeriod UserTotals = null;
        private bool blnCreatePDF = false;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            blnCreatePDF = Valid.getBoolean("blnPrint", false);
            if (!Page.IsPostBack) {
                initPage();

                int userCount = 0;
                using (DataSet dsUsers = loadUsers()) {
                    foreach (DataRow dr in dsUsers.Tables[0].Rows) {
                        intCurrUserID = DB.readInt(dr["ID"]);
                        oUser = G.UserInfo.getUser(intCurrUserID);
                        szCurrHTML = getCommissionReportHTML();
                        szCommissionSummaryHTML = getCommissionSummaryHTML();
                        userCount++;
                        if (userCount != 1 && !blnCreatePDF) {
                            addValue("[PAGEBREAK]", "page-break-before: always;");
                        } else {
                            addValue("[PAGEBREAK]", "");
                        }
                        loadAgentData();
                        createCommissionStatement();
                        if (blnCreatePDF)
                            createPDF(szCurrHTML);
                        sbMainReport.Append(szCurrHTML);
                    }
                }
                dReport.InnerHtml = sbMainReport.ToString();
            }
        }

        private void initPage() {
            G.UserInfo.forceReload();
            intPayPeriodID = Valid.getInteger("szPayPeriod", -1);
            oPayPeriod = G.PayPeriodInfo.getPayPeriod(intPayPeriodID);
            DateTime dtStart = oPayPeriod.StartDate;
            szReportTitle = "Commission statement for " + dtStart.ToString("MMMM yyyy");
            pPageHeader.InnerHtml = szReportTitle;
            calculateSalariedUserPayments();
        }

        private DataSet loadUsers() {
            string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
            string szFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szFilter = String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

            string szSQL = String.Format(@"
                    SELECT U.ID FROM DB_USER U
                    JOIN LIST l_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    WHERE U.ID IN ({0}) {1} AND U.ISPAID = 1
                    ORDER BY LASTNAME, FIRSTNAME", Valid.getText("szUserID", VT.List), szFilter);
            return DB.runDataSet(szSQL);
        }

        private string getCommissionPercentage(AmountType oAmountType, double Amount, string ListingTypeTotal) {
            if (Amount == 0)
                return "0%" + ListingTypeTotal;

            if (oAmountType == AmountType.Dollar)
                return Amount.ToString();
            return Amount.ToString() + "%" + ListingTypeTotal;
        }

        protected void loadAgentData() {
            addValue("[PERSONNAME]", "<strong>" + oUser.FirstName + ' ' + oUser.LastName + "</strong>");
            addValue("[PERSONCODE]", "<strong> Account: " + oUser.Initials + "</strong>");
            if (Valid.getText("RecalcTotals", "No").ToUpper() == "YES")
                DB.runNonQuery(String.Format(@"UPDATE USERPAYPERIOD SET INCOME = NULL WHERE USERID = {0} AND PAYPERIODID = {1}", intCurrUserID, intPayPeriodID));
            UserTotals = new UserPayPeriod(intCurrUserID, intPayPeriodID);
        }

        protected void createCommissionStatement() {
            StringBuilder sbHTML = new StringBuilder();
            DataView oDataView = UserTotals.dsData.Tables[0].DefaultView;
            UserTotals.calculateTotals();
            foreach (ListObject oCommissionType in G.CommTypeInfo.CommissionTypeList) {
                oDataView.RowFilter = string.Format(@"COMMISSIONTYPEID = {0}", (int)oCommissionType.ID);
                addCommissionHTML(sbHTML, oCommissionType, oDataView);
            }

            addValue("[TOTALCOMMISSION]", Utility.formatReportMoney(UserTotals.Income));
            addValue("[TRDATA]", sbHTML.ToString());
            addSummaryValue("[CommissionSummary_INCOME]", Utility.formatReportMoney(UserTotals.Income));

            //Other Income
            sbHTML.Clear();
            //Only get the HTML
            addOtherIncomeHTML(sbHTML);
            addJuniorBonusHTML(sbHTML);
            addValue("[OTHERINCOME]", sbHTML.ToString());
            addValue("[TOTALOTHERINCOME]", Utility.formatReportMoney(UserTotals.OtherIncome));

            addSummaryValue("[CommissionSummary_OTHERINCOME]", Utility.formatReportMoney(UserTotals.OtherIncome));
            addSummaryValue("[CommissionSummary_DEDUCTIONS]", Utility.formatReportMoney(UserTotals.TotalDeductionAmount));
            sbHTML.Clear();

            //Deductions
            addDeductionHTML(sbHTML);
            addValue("[TOTALDISTRIBUTIONOFFUNDS]", UserTotals.TotalDistributionOfFunds >= 0 ? Utility.formatReportMoney(UserTotals.TotalDistributionOfFunds) : "-" + Utility.formatReportMoney(-1 * UserTotals.TotalDistributionOfFunds));

            if (UserTotals.TotalDistributionOfFunds < 0) {
                addSummaryValue("[CommissionSummary_DISTRIBUTIONOFFUNDS]", "$0.00");
                addSummaryValue("[CommissionSummary_HELDOVER]", Utility.formatReportMoney(-1 * UserTotals.TotalDistributionOfFunds));
            } else {
                addSummaryValue("[CommissionSummary_DISTRIBUTIONOFFUNDS]", Utility.formatReportMoney(UserTotals.TotalDistributionOfFunds));
                addSummaryValue("[CommissionSummary_HELDOVER]", "$0.00");
            }

            addPendingHTML(sbHTML);
            addValue("[TRPENDING]", sbHTML.ToString());

            addSummaryValue("[CommissionSummary_PENDING]", Utility.formatReportMoney(UserTotals.Pending));
            addSummaryValue("[CommissionSummary_DATE]", "<strong>" + oUser.FirstName + ' ' + oUser.LastName + "</strong><br/>" + szReportTitle);
            addJuniorSalaryBreakdown();
            addSeniorBalanceBreakDown();
            addValue("[SUMMARYTABLE]", szCommissionSummaryHTML);
        }

        private void addSeniorBalanceBreakDown() {
            if (UserTotals.EOFYBonus == 0)
                addSummaryValue("[BONUSBREAKDOWN]", "");

            string szHTML = "";

            if (UserTotals.EOFYBonus > 0) {
                szHTML += String.Format(@"
                    <tr>
                        <td  style='width: 60%'>EOFY bonus scheme</td>
                        <td class='AlignRight' style='width: 40%'>{0}</td>
                    </tr>", Utility.formatReportMoney(UserTotals.EOFYBonus));
            }

            addSummaryValue("[BONUSBREAKDOWN]", "");
        }

        private void addJuniorSalaryBreakdown() {
            double dSalary = G.UserInfo.getUser(intCurrUserID).Salary / 12;

            if (dSalary == 0) {
                addSummaryValue("[SALARYBREAKDOWN]", "");
                return;
            }
            string szPaymentType = "Salary";
            double dPaymentAmount = dSalary;
            if (UserTotals.RetainerAmount > 0) {
                szPaymentType = "Retainer";
                dPaymentAmount = UserTotals.RetainerAmount;
            }

            double dTotal = UserTotals.CommissionIncome + dPaymentAmount + UserTotals.OtherIncome - UserTotals.DeductionsAmount - (oUser.Salary / 12);

            double dYTDCommission = oUser.getYTDTotal(intPayPeriodID);

            string szHTML = String.Format(@"
                <table id='SalarySummaryTable' style='width:30%; float: left; margin-left: 100px' class='CommissionTable'>
                    <tr>
                        <td colspan='2'><span class='ReportHeader'><br/>Income breakdown</span></td>
                    </tr>
                    <tr>
                        <td style='width: 70%'>Commission</td>
                        <td class='AlignRight' style='width: 30%'>{1}</td>
                    </tr>
                    <tr>
                        <td>{0}</td>
                        <td class='AlignRight' >{2}</td>
                    </tr>
                    <tr>
                        <td>Other income</td>
                        <td class='AlignRight' >{3}</td>
                    </tr>
                    {6}

                   <tr>
                        <td>Deductions</td>
                        <td class='AlignRight' >{4}</td>
                    </tr>
                    <tr>
                        <td>Total income</td>
                        <td class='AlignRight' >{5}</td>
                    </tr>
                    <tr>
                        <td>YTD Gross commission (as per graph)</td>
                        <td class='AlignRight' >{7} ({8})</td>
                    </tr> ", szPaymentType, Utility.formatReportMoney(UserTotals.Income), Utility.formatReportMoney(dPaymentAmount),
                           Utility.formatReportMoney(UserTotals.OtherIncome - oUser.Salary / 12), Utility.formatReportMoney(UserTotals.DeductionsAmount - UserTotals.RetainerAmount),
                      Utility.formatReportMoney(dTotal), "", Utility.formatReportMoney(dYTDCommission), Utility.formatReportMoney(UserTotals.GraphCommissionTotal));

            addSummaryValue("[SALARYBREAKDOWN]", szHTML + "</table>");
        }

        /// <summary>
        /// Process the data for all juniors on this seniors team. The totals have already been calculated so we can simply pull them across.
        ///
        /// </summary>
        /// <param name="sbHTML"></param>
        /// <returns></returns>
        private void addJuniorBonusHTML(StringBuilder sbHTML) {
            if (UserTotals.dsData.Tables[1].Rows.Count + UserTotals.dsData.Tables[2].Rows.Count == 0)
                return;

            //Output the initial commission payable based on deferred commissions that are payable to the seniors as well
            foreach (DataRow dr in UserTotals.dsData.Tables[5].Rows) {
                sbHTML.AppendFormat(@"
                    <tr>
                        <td colspan='2'>&nbsp;</td>
                        <td class='AlignLeft' colspan='2'>&nbsp;&nbsp;{0} - Initial commission</td>
                        <td class='AlignRight'>{1}</td>
                        <td>&nbsp;</td>
                    </tr>", DB.readString(dr["NAME"]), DB.readMoneyString(dr["COMMISSIONBONUS"], true));
            }
        }

        /// <summary>
        /// Process the data for all juniors on this seniors team. The totals have already been calculated so we can simply pull them across.
        ///
        /// </summary>
        /// <param name="sbHTML"></param>
        /// <returns></returns>
        private void addJuniorDeductionHTML(StringBuilder sbHTML) {
            //Jac wants to do this manually
            return;
        }

        private void addOtherIncomeHTML(StringBuilder sbHTML) {
            foreach (DataRow dr in UserTotals.dsOtherIncome.Tables[0].Rows) {
                sbHTML.AppendFormat(@"
                    <tr>
                       <td>&nbsp;</td>
                       <td>{0}</td>
                       <td class='AlignLeft' colspan='2'>{1}</td>
                       <td class='AlignRight'>{2}</td>
                       <td>&nbsp;</td>
                   </tr>", dr["NAME"].ToString(), dr["Comment"].ToString(), DB.readMoneyString(dr["AMOUNT"], true));
            }
        }

        private void addDeductionHTML(StringBuilder sbHTML) {
            double dTotal = 0.0;
            foreach (DataRow dr in UserTotals.dsDeductions.Tables[0].Rows) {
                string szCategory = dr["category"].ToString();
                sbHTML.AppendFormat(@"
                    <tr>
                        <td>&nbsp;</td>
                        <td>{0}</td>
                        <td class='AlignLeft' colspan='2'>{3}{1}</td>
                        <td class='AlignRight'>-{2}</td>
                        <td>&nbsp;</td>
                    </tr>", dr["NAME"].ToString(), dr["Comment"].ToString(), DB.readMoneyString(dr["AMOUNT"]),
                    string.IsNullOrWhiteSpace(szCategory) ? "" : string.Format("{0} - ", szCategory));
                dTotal += Convert.ToDouble(dr["AMOUNT"]);
            }
            addJuniorDeductionHTML(sbHTML);
            addValue("[TRDEDUCTIONS]", sbHTML.ToString());
            addValue("[TOTALDEDUCTIONS]", dTotal > 0 ? "-" + Utility.formatReportMoney(dTotal) : "");
        }

        private void addPendingHTML(StringBuilder sbHTML) {
            sbHTML.Clear();
            foreach (ListObject oCommissionType in G.CommTypeInfo.CommissionTypeList) {
                DataView oDataView = UserTotals.dsFutureData.Tables[0].DefaultView;
                oDataView.RowFilter = string.Format(@"COMMISSIONTYPEID = {0}", (int)oCommissionType.ID);
                if (oDataView.Count > 0) {
                    sbHTML.AppendFormat(@"<tr><td style='padding: 10px 0px 5px 0px;' colspan='6'><strong>{0}</strong></td></tr>", oCommissionType.Name);
                    foreach (DataRowView dr in oDataView) {
                        string szAmountType = getCommissionPercentage((AmountType)DB.readInt(dr["AMOUNTTYPEID"]), Convert.ToDouble(dr["Amount"]), " of " + DB.readString(dr["SS_AMOUNT"]));

                        sbHTML.AppendFormat(@"
                            <tr>
                                <td>{0}</td>
                                <td>{1}</td>
                                <td>{2}</td>
                                <td align='right'>{3}</td>
                                <td align='right'>{4}</td>
                                <td align='right'>{5}</td>
                            </tr>
                        ", DB.readString(dr["CODE"]).Replace("&", "&#38;"), DB.readString(dr["Address"]).Replace("&", "&#38;"), DB.readDateString(dr["ENTITLEMENTDATE"]), DB.readMoneyString(dr["GROSSCOMMISSION"]),
                         szAmountType, DB.readMoneyString(dr["ACTUALPAYMENT"], true));
                    }
                }
            }
            addValue("[FUTURETOTALCOMMISSION]", Utility.formatReportMoney(UserTotals.Pending));
        }

        private void addCommissionHTML(StringBuilder sbHTML, ListObject oCommissionType, DataView oDataView) {
            if (oDataView.Count > 0) {
                sbHTML.AppendFormat(@"<tr><td style='padding: 10px 0px 5px 0px;' colspan='6'><strong>{0}</strong></td></tr>", oCommissionType.Name);

                foreach (DataRowView dr in oDataView) {
                    string szAmountType = getCommissionPercentage((AmountType)DB.readInt(dr["AMOUNTTYPEID"]), Convert.ToDouble(dr["Amount"]), " of " + DB.readString(dr["SS_AMOUNT"]));
                    sbHTML.AppendFormat(@"
                            <tr>
                               <td>{0}</td>
                               <td>{1}</td>
                               <td>{2}</td>
                               <td class='AlignRight'>{3}</td>
                               <td class='AlignRight'>{4}</td>
                               <td class='AlignRight'>{5}</td>
                            </tr>", DB.readString(dr["CODE"]).Replace("&", "&#38;"), DB.readString(dr["Address"]).Replace("&", "&#38;"), DB.readDateString(dr["ENTITLEMENTDATE"]),
                                    DB.readMoneyString(dr["GRAPHTOTAL"], true), szAmountType, DB.readMoneyString(dr["ACTUALPAYMENT"], true));
                }
            }
        }

        private void addValue(string Tag, string Value) {
            szCurrHTML = szCurrHTML.Replace(Tag, Value);
        }

        private void addSummaryValue(string Tag, string Value) {
            szCommissionSummaryHTML = szCommissionSummaryHTML.Replace(Tag, Value);
        }

        /// <summary>
        /// Only create records for the current pay period
        /// </summary>
        /// <returns></returns>
        public bool canUpdateTotals {
            get { return G.CurrentPayPeriod == intPayPeriodID; }
        }

        /// <summary>
        /// Creates any salary records for salaried employees. This will only create them, not update
        /// </summary>
        private void createSalaryRecords() {
            if (!canUpdateTotals)
                return;

            int intSalaryAccountID = DB.getScalar("SELECT ID FROM LIST WHERE NAME = 'SALARY' AND LISTTYPEID = 4", 0);

            foreach (UserDetail uD in G.UserInfo.UserList) {
                if (uD.PaymentStructure == PayBand.JuniorTeamSalary || uD.PaymentStructure == PayBand.JuniorNoTeamSalary) {
                    int intTXID = DB.getScalar(String.Format(@"
                        SELECT ID FROM USERTX WHERE ACCOUNTID = {0} AND TXDATE = '{1}' AND USERID = {2}
                        ", intSalaryAccountID, Utility.formatDate(oPayPeriod.EndDate), uD.ID), 0);
                    if (intTXID > 0)
                        continue; //We don't update salary records once created so they can be modified if required
                    sqlUpdate oSQL = new sqlUpdate("USERTX", "ID", intTXID);
                    oSQL.add("AccountID", intSalaryAccountID);
                    oSQL.add("USERID", uD.ID);
                    oSQL.add("TXDATE", Utility.formatDate(oPayPeriod.EndDate));
                    oSQL.add("CREDITGLCODE", uD.CreditGL);
                    oSQL.add("CREDITJOBCODE", uD.OfficeJobCode);
                    oSQL.add("AMOUNTTYPEID", 1);
                    oSQL.add("DEBITGLCODE", uD.DebitGL);
                    oSQL.add("FLETCHERAMOUNT", uD.Salary / 12);
                    oSQL.add("AMOUNT", uD.Salary / 12);
                    oSQL.add("TOTALAMOUNT", uD.Salary / 12);
                    DB.runNonQuery(oSQL.createInsertSQL());
                }
            }
        }

        /// <summary>
        /// Check the salaried payments to ensure that we are handling the pay bands properly
        /// </summary>
        private void calculateSalariedUserPayments() {
            if (intPayPeriodID != G.CurrentPayPeriod && intPayPeriodID != (G.CurrentPayPeriod - 1))
                return; //Only do this when running for the current pay period or last month

            string szUserIDList = "";

            foreach (UserDetail uD in G.UserInfo.UserList) {
                if (uD.PaymentStructure == PayBand.JuniorTeamSalary || uD.PaymentStructure == PayBand.JuniorNoTeamSalary)
                    Utility.Append(ref szUserIDList, uD.ID.ToString(), ",");
            }

            //Special code to handle the three agents for the next few months - Albert, Todd and Jack
            if (!szUserIDList.Contains("153"))
                Utility.Append(ref szUserIDList, "153", ",");
            if (!szUserIDList.Contains("207"))
                Utility.Append(ref szUserIDList, "207", ",");
            if (!szUserIDList.Contains("163"))
                Utility.Append(ref szUserIDList, "163", ",");

            DB.runNonQuery("-- Recalculating for " + szUserIDList);
            if (szUserIDList == "")
                return;
            string szSQL = String.Format(@"
                SELECT USS.SALESPLITID, USS.ID AS USERSPLITID,  USS.USERID, USS.AMOUNT, USS.OFFICEID, S.SALEDATE,
                        USS.AMOUNTTYPEID, USS.ACTUALPAYMENT, USS.CALCULATEDAMOUNT, S.ID, USS.INCLUDEINKPI, SS.COMMISSIONTYPEID, USS.GRAPHCOMMISSION
                    FROM USERSALESPLIT USS
                    JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1   AND USS.RECORDSTATUS < 1
                    JOIN SALE S ON SS.SALEID = S.ID
                    JOIN DB_USER U ON U.ID = USS.USERID
                    WHERE S.PAYPERIODID IN ({0}) AND S.STATUSID = 2 AND U.ID in ({1})
                    ORDER BY USS.USERID, S.SALEDATE;

                    --Get all the 100000k sales bonuses that have been paid in this year
                    SELECT USERID FROM MENTORBONUS
                    WHERE USERID IN ({1}) AND SALEID IS NULL AND COMMISSIONBONUS > 0
                    AND PAYPERIODID IN ({2});
                    ", intPayPeriodID, szUserIDList, G.PayPeriodInfo.getPayPeriodsInCurrYear(G.CurrentPayPeriod));
            UserDetail oU = null;
            StringBuilder sbSQL = new StringBuilder();
            using (DataSet ds = DB.runDataSet(szSQL)) {
                int intCurrUserID = -1;
                double dUserTotalCommission = 0;
                double dPrevYearUserTotalCommission = 0;
                DataView dv = ds.Tables[1].DefaultView;
                bool blnSalaryCommBonusExists = false;
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    if (intCurrUserID != DB.readInt(dr["USERID"])) {
                        intCurrUserID = DB.readInt(dr["USERID"]);
                        oU = G.UserInfo.getUser(intCurrUserID);
                        dv.RowFilter = "USERID = " + intCurrUserID;
                        blnSalaryCommBonusExists = dv.Count == 1; //Reset the flag
                        dUserTotalCommission = oU.getYTDTotal(intPayPeriodID);
                        dPrevYearUserTotalCommission = oU.getPrevYTDTotal(intPayPeriodID);
                    }
                    //Recalculate the sales split
                    UserSalesSplit oUSS = new UserSalesSplit(DB.readInt(dr["SALESPLITID"]), DB.readInt(dr["USERSPLITID"]), intCurrUserID, Convert.ToDouble(dr["AMOUNT"]), (AmountType)Convert.ToInt32(dr["AMOUNTTYPEID"]), Convert.ToDouble(dr["CALCULATEDAMOUNT"]), Convert.ToDouble(dr["ACTUALPAYMENT"]), Convert.ToBoolean(dr["INCLUDEINKPI"]), DB.readInt(dr["COMMISSIONTYPEID"]), DB.readInt(dr["OFFICEID"]), DB.readDouble(dr["GRAPHCOMMISSION"]));
                    DateTime dtSaleDate = DB.readDate(dr["SALEDATE"]);
                    double dEffectiveCommission = oU.getEffectiveYTDCommission(dUserTotalCommission, dPrevYearUserTotalCommission, dtSaleDate, intPayPeriodID);

                    sbSQL.Append(oUSS.updateToDB(G.CurrentPayPeriod, dEffectiveCommission, dtSaleDate));
                    //checkInitialSalaryCommBonus(oU, intCurrUserID, dUserTotalCommission, ref blnSalaryCommBonusExists);
                    dUserTotalCommission += oUSS.CalculatedAmount;
                }
            }

            if (sbSQL.Length > 0)
                DB.runNonQuery(sbSQL.ToString());
        }

        private void checkInitialSalaryCommBonus(UserDetail oU, int intLastUserID, double dUserTotalCommission, ref bool SalaryCommBonusExists) {
            UserDetail oCurr = G.UserInfo.getUser(intLastUserID);
            //Check to see if we have already paid out the bonus
            if (SalaryCommBonusExists || oU.MentorID == -1)
                return;
            if (dUserTotalCommission >= (oCurr.Salary * 2.1)) { //Provide enough headroom to payout the bonus
                //Create and payout the record
                sqlUpdate oSQL = new sqlUpdate("MENTORBONUS", "ID", -1);
                oSQL.add("MENTORUSERID", oU.MentorID);
                oSQL.add("PAYPERIODID", intPayPeriodID);
                oSQL.add("USERID", oCurr.ID);
                oSQL.add("COMMISSIONBONUS", oCurr.Salary * 0.45 * 0.05);
                oSQL.add("SALARYBONUS", 0);
                oSQL.add("NOTES", String.Format("Payout of initial salary-based commission - 5% of 45% of salary ({0})", Utility.formatReportMoney(oCurr.Salary)));
                DB.runNonQuery(oSQL.createInsertSQL());
                SalaryCommBonusExists = true;
            }
        }

        protected string getCommissionSummaryHTML() {
            return @"
                <div id='Div20' style='padding: 5px; margin-top: 20px; width: 100%'>
                    <table id='CommissionSummaryTable' style='width:30%; float: left' class='CommissionTable'>
                        <tr>
                            <td colspan='2'>
                               <span class='ReportHeader'>[CommissionSummary_DATE]</span>
                            </td>
                        </tr>
                        <tr>
                            <td  style='width: 50%'>
                                Income
                            </td>
                            <td class='AlignRight' style='width: 50%'>
                                [CommissionSummary_INCOME]
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Other Income
                            </td>
                            <td class='AlignRight'>
                                [CommissionSummary_OTHERINCOME]
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Deductions
                            </td>
                            <td class='AlignRight'>
                                [CommissionSummary_DEDUCTIONS]
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Distribution of Funds
                            </td>
                            <td class='AlignRight'>
                                [CommissionSummary_DISTRIBUTIONOFFUNDS]
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Held over
                            </td>
                            <td class='AlignRight'>
                                [CommissionSummary_HELDOVER]
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Pending
                            </td>
                            <td class='AlignRight'>
                                [CommissionSummary_PENDING]
                            </td>
                        </tr>
                        <tr>
                            <td style='padding-bottom:15px'>
                            </td>
                            <td >
                            </td>
                        </tr>
                    </table>
                    [SALARYBREAKDOWN]
                    [BONUSBREAKDOWN]
                </div>";
        }

        private string getCommissionReportHTML() {
            return @"
                <div  style='[PAGEBREAK] width: 100%;'>
                    <table class='CommissionTable'>
                        <tr>
                            <td colspan='2' style='border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                <div id='oPersonName' >
                                    [PERSONNAME]
                                </div>
                            </td>
                            <td colspan='4' style='border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                <div id='oPersonCode' >
                                    [PERSONCODE]
                                </div>
                            </td>
                        </tr>
                        <tr class='bottomBorderItalics'>
                            <td style='width: 15%'>Property code</td>
                            <td style='width: 40%'>Property Address</td>
                            <td style='width: 12%'>Payable Date</td>
                            <td class='AlignRiht' style='width: 10%'>Gross Comm.</td>
                            <td class='AlignRight' style='width: 13%'>Comm. Intro.</td>
                            <td class='AlignRight' style='width: 10%'>Commission</td>
                        </tr>
                            <tr>
                                <td colspan='6'>&nbsp;
                                </td>
                            </tr>
                            <tr style='font-style: italic;'>
                                <td colspan='6' style='font-size: 14px; border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                    <b>Income</b>
                                </td>
                            </tr>
                            [TRDATA]
                            <tr class='topBorder'>
                                <td colspan='5' style='font-weight: bold; text-align: right'>
                                    Total Income
                                </td>
                                <td style='font-weight: bold'>
                                    <div id='divTotalCommission' class='AlignRight' >
                                        [TOTALCOMMISSION]
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td colspan='6'>
                                </td>
                            </tr>
                            <tr style='font-style: italic;'>
                                <td colspan='6' style='font-size: 14px; border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                    <b>Other Income</b>
                                </td>
                            </tr>
                            <tr>
                                <td colspan='6' style='padding: 5px 0px 3px 0px;'>
                                    &nbsp;
                                </td>
                            </tr>
                            [OTHERINCOME]
                            <tr class='bottomBorder'>
                                <td colspan='5' style='font-weight: bold; text-align: right; padding-top: 35px'>
                                </td>
                                <td style='vertical-align: text-bottom'>
                                </td>
                            </tr>
                            <tr class='topBorder'>
                                <td colspan='5' style='font-weight: bold; text-align: right'>
                                    Total Other Income
                                </td>
                                <td style='font-weight: bold'>
                                    <div id='divTotalOtherIncome' class='AlignRight CalcTotal' >
                                        [TOTALOTHERINCOME]
                                    </div>
                                </td>
                            </tr>
                            <tr style='font-style: italic;'>
                                <td colspan='6' style='font-size: 14px; border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                    <b>Deductions</b>
                                </td>
                            </tr>
                            <tr>
                                <td colspan='6' style='padding: 5px 0px 3px 0px;'>
                                    &nbsp;
                                </td>
                            </tr>
                            [TRDEDUCTIONS]
                            <tr class='topBorder'>
                                <td colspan='4' style='font-weight: bold; text-align: right'>
                                    Total Deductions
                                </td>
                                <td colspan='2' style='font-weight: bold' align='right'>
                                    <div id='divTotalDeductions' class='AlignRight CalcTotal' >
                                        [TOTALDEDUCTIONS]
                                    </div>
                                </td>
                            </tr>
                            <tr style='font-style: italic;'>
                                <td colspan='6' style='font-size: 14px; border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                    <b>Distribution of Funds</b>
                                </td>
                            </tr>
                            <tr class='bottomBorder'>
                                <td colspan='6' style='font-weight: bold; text-align: right; padding-top: 35px'>
                                    <div id='oPersonName2' >
                                        [PERSONNAME]
                                    </div>
                                </td>
                            </tr>
                            <tr class='topBorder'>
                                <td colspan='4' style='font-weight: bold; text-align: right'>
                                    Total Distribution of Funds
                                </td>

                                <td colspan='2' style='font-weight: bold' align='right'>
                                    <div id='divTotalDistributionOfFunds'  class='AlignRight calcTotal'>
                                        [TOTALDISTRIBUTIONOFFUNDS]
                                    </div>
                                </td>
                            </tr>
                            <tr style='font-style: italic;'>
                                <td colspan='6' style='font-size: 14px; border-bottom: 1px solid black; padding: 15px 0px 3px 0px;'>
                                    <b>Pending</b>
                                </td>
                            </tr>
                            [TRPENDING]
                            <tr class='topBorder'>
                                <td colspan='5' style='font-weight: bold; text-align: right'>
                                    Total Pending
                                </td>
                                <td style='font-weight: bold'>
                                    <div id='divFutureTotalCommission' class='AlignRight calcTotal' >
                                        [FUTURETOTALCOMMISSION]
                                    </div>
                                </td>
                            </tr>
                    </table>
                [SUMMARYTABLE]
                </div>
        ";
        }

        private void createPDF(string szHTML) {
            string szDir = G.User.getPDFDir(intCurrUserID);
            if (!Directory.Exists(szDir)) {
                Response.Write("Creating DIR: " + szDir);

                Directory.CreateDirectory(szDir);
            }
            Response.Write("Creating PDF to file: " + szDir);
            szHTML = @"
                    <html><head>
                    <style type='text/css'>
                            @media print {
                                .CommissionTable{
                                    width: 100%;
                                    font-family: Tahoma, Arial, Verdana;
                                    font-size: 9px;
                                    color: black;
                                    border-collapse: collapse;
                                }
                            }

                        .CommissionTable{
                            width: 99%;
                            font-family: Tahoma, Arial, Verdana;
                            font-size: 10px;
                            color: #333333;
                            border-collapse: collapse;
                        }
                            .CommissionTable td{
                                padding-left: 5px;
                                padding-right: 5px;
                            }
                            .bottomBorder td{
                                border-bottom: 1px solid black;
                            }
                            .bottomBorderItalics td{
                                border-bottom: 1px solid black;
                                padding-bottom: 5px;
                                font-style: italic;
                            }
                            .topBorder td{
                                border-top: 1px solid black;
                                padding-bottom: 5px;
                                padding-top: 5px;
                            }

                            .Header {
                            width: 750px;
                            }
                            body { font-size: 8pt; font-family: Verdana, Tahoma}
                        </style>
                    <link href='../main.css' rel='stylesheet' type='text/css' title='MainSS' /></head><body>" + szHTML + "</body></html>";

            MemoryStream msOutput = new MemoryStream();
            using (FileStream fs = new FileStream(Path.Combine(szDir, oPayPeriod.StartDate.ToString("MMM yy") + ".pdf"), FileMode.Create)) {
                TextReader reader = new StringReader(szHTML);

                // step 1: creation of a document-object
                using (Document document = new Document(PageSize.A4, 30, 30, 30, 30)) {
                    // step 2:
                    // we create a writer that listens to the document
                    // and directs a XML-stream to a file
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();
                    // step 3: we create a worker parse the document
                    //HTMLWorker worker = new HTMLWorker(document);
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, reader);
                }
            }
        }
    }
}