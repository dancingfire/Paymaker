using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for Sale
/// </summary>
public class Sale {
    private int intSaleID = 0;
    private double dAmount = 0;
    private string szCode = "";
    private string szAddress = "";
    private string szComments = "";
    private string szSection27Comments = "";
    private int intStatusID = 0;
    private double dExpenseAmount = 0;
    private double dConjCommission = 0;
    private double dGrossCommission = 0;
    private double dAdvertisingSpend = 0;
    private double dSalePrice = 0;
    public DateTime EntitlementDate { get; set; }
    public DateTime SettlementDate { get; set; }

    private string szSaleDate = "";
    private string szWithdrawnDate = "";
    private string szAuctionDate = "";
    private string szSuburb = "";
    private int intPayPeriodID = -1;
    private bool blnExportedToMYOB = false;
    private bool blnIsSection27 = false;
    private bool blnIgnoreSaleBonus = false;
    public List<SalesSplit> lSaleSplits = new List<SalesSplit>();
    public List<SalesExpense> lSaleExpenses = new List<SalesExpense>();
    private int intBnDSaleID = Int32.MinValue;

    public double ExpenseAmount {
        get { return dExpenseAmount; }
    }

    /// <summary>
    /// Commission that has to paid to another office
    /// </summary>
    public double ConjCommission {
        get { return dConjCommission; }
        set { dConjCommission = value; }
    }

    public double GrossCommission {
        get { return dGrossCommission; }
        set { dGrossCommission = value; }
    }

    /// <summary>
    /// The amount of money spent on this property for advertising
    /// </summary>
    public double AdvertisingSpend {
        get { return dAdvertisingSpend; }
        //set { dAdvertisingSpend = value; }
    }

    /// <summary>
    /// The amount of money spent on this property for advertising
    /// </summary>
    public double SalePrice {
        get { return dSalePrice; }
    }

    public int SaleID {
        get { return intSaleID; }
    }

    public int Status {
        get { return intStatusID; }
    }

    /// <summary>
    /// The pay period or -1 if the pay period has not been set.
    /// </summary>
    public int PayPeriodID {
        get { return intPayPeriodID; }
    }

    public string Code {
        get { return szCode; }
    }

    public string Comments {
        get { return szComments; }
    }

    public string Section27Comments {
        get { return szSection27Comments; }
    }

    public string Address {
        get { return szAddress; }
    }

    public bool IsCompleted {
        get { return intStatusID > 0; }
    }

    public bool IsFinalized {
        get { return intStatusID == 2; }
    }

    public bool ExportedToMYOB {
        get { return blnExportedToMYOB; }
    }

    /// <summary>
    /// Has the section 27 been received?
    /// </summary>
    public bool IsSection27 {
        get { return blnIsSection27; }
        set { blnIsSection27 = value; }
    }

    /// <summary>
    /// Should we apply the junior to senior sales bonus
    /// </summary>
    public bool IgnoreSaleBonus {
        get { return blnIgnoreSaleBonus; }
        set { blnIgnoreSaleBonus = value; }
    }

    public string SaleDate {
        get { return szSaleDate; }
    }

    public string Suburb {
        get { return szSuburb; }
    }

    /// <summary>
    /// When this sale was withdrawn as an auction
    /// </summary>
    public string WithdrawnDate {
        get { return szWithdrawnDate; }
    }

    public string AuctionDate {
        get { return szAuctionDate; }
    }

    /// <summary>
    /// Box&Dice SaleID
    /// </summary>
    public int BnDSaleID {
        get { return intBnDSaleID; }
    }

    /// <summary>
    /// Loads an object for the given sale
    /// </summary>
    /// <param name="SalesID"></param>
    public Sale(int SalesID, bool ForceCommissionTypeLoad = false, bool CreateIfNotInDB = false) {
        string szSQL = String.Format("SELECT  * FROM SALE where ID = {0}", SalesID);
        intSaleID = SalesID;
        DataSet dsSale = DB.runDataSet(szSQL);
        foreach (DataRow dr in dsSale.Tables[0].Rows) {
            szCode = dr["CODE"].ToString();
            szAddress = dr["Address"].ToString();
            szComments = dr["COMMENTS"].ToString();
            szSection27Comments = dr["SECTION27COMMENTS"].ToString();
            szSuburb = dr["SUBURB"].ToString();
            dConjCommission = Convert.ToDouble(dr["CONJUNCTIONALCOMMISSION"]);
            dGrossCommission = Convert.ToDouble(dr["GROSSCOMMISSION"]);
            dAdvertisingSpend = Convert.ToDouble(dr["ADVERTISINGSPEND"]);
            dSalePrice = Convert.ToDouble(dr["SALEPRICE"]);
            intStatusID = Convert.ToInt32(dr["STATUSID"]);
            szSaleDate = Utility.formatDate(dr["SALEDATE"].ToString());
            if (System.DBNull.Value != dr["AUCTIONDATE"])
                szAuctionDate = Utility.formatDate(dr["AUCTIONDATE"].ToString());
            if (System.DBNull.Value != dr["FALLENTHROUGHDATE"])
                szWithdrawnDate = Utility.formatDate(dr["FALLENTHROUGHDATE"].ToString());
            if (System.DBNull.Value != dr["ENTITLEMENTDATE"])
                EntitlementDate = DB.readDate(dr["ENTITLEMENTDATE"]);
            if (System.DBNull.Value != dr["SETTLEMENTDATE"])
                SettlementDate = DB.readDate(dr["SETTLEMENTDATE"]);

            if (dr["PAYPERIODID"] != System.DBNull.Value)
                intPayPeriodID = Convert.ToInt32(dr["PAYPERIODID"]);
            blnExportedToMYOB = Convert.ToInt32(dr["MYOBEXPORTID"]) > 0;
            G.User.IsCurrentSaleExportedToMYOB = blnExportedToMYOB;
            blnIsSection27 = Convert.ToBoolean(dr["ISSECTION27"]);
            blnIgnoreSaleBonus = Convert.ToBoolean(dr["IGNORESALEBONUS"]);
            intBnDSaleID = DB.readInt(dr["BNDSALEID"]);
        }
        if (intSaleID > -1 || (intSaleID == -1 && CreateIfNotInDB)) {
            loadSaleExpenses(IsCompleted);
            loadSaleSplits(ForceCommissionTypeLoad);
        }
    }

    /// <summary>
    /// Performs the port processing that might be required on this sale
    /// </summary>
    public void performPostProcessing() {
        updateGraphTotals();
        calculateSalesCommissionBonus();
    }

    /// <summary>
    /// Finalizes the sale and completes the associated transactions
    /// </summary>
    public void finalizeSale() {
        sqlUpdate oSQL = new sqlUpdate("SALE", "ID", intSaleID);
        oSQL.add("STATUSID", 2);
        if (intPayPeriodID == -1) {
            oSQL.add("PAYPERIODID", G.CurrentPayPeriod);
            intPayPeriodID = G.CurrentPayPeriod;
        }
        DB.runNonQuery(oSQL.createUpdateSQL());
        performPostProcessing();
    }

    /// <summary>
    /// Updates the graph totals for this sale
    /// </summary>
    public void updateGraphTotals() {
        bool blnHasFarm = lSaleSplits.Exists(o => o.CommissionTypeID == (int)CommissionType.ServiceArea && o.CalculatedAmount > 0);
        bool blnHasMentor = lSaleSplits.Exists(o => o.CommissionTypeID == (int)CommissionType.Mentoring && o.CalculatedAmount > 0);
        double dFarm = 0;
        double dLead = 0;
        double dList = 0;
        double dManage = 0;
        double dSell = 0;
        double dMentor = 0;

        Client.getGraphCommisionPercentage(ref dFarm, ref dLead, ref dList, ref dManage, ref dSell, ref dMentor, blnHasFarm, blnHasMentor);
        DB.runNonQuery(String.Format(@"
            --Update graph commission
            UPDATE USS
            SET GRAPHCOMMISSION =
                ISNULL(
                    CASE
                    WHEN COMMISSIONTYPEID = 9 -- Service Area
                        THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * {1} * (S.GROSSCOMMISSION - CONJUNCTIONALCOMMISSION)
	 	            WHEN COMMISSIONTYPEID = 89 --MENTOR
                        THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * {2} * (S.GROSSCOMMISSION - CONJUNCTIONALCOMMISSION)
		            WHEN COMMISSIONTYPEID = 6  --LEAD
                        THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * {3} * (S.GROSSCOMMISSION - CONJUNCTIONALCOMMISSION)
	 	            WHEN COMMISSIONTYPEID = 10  --LIST
                        THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * {4} * (S.GROSSCOMMISSION - CONJUNCTIONALCOMMISSION)
	 	            WHEN COMMISSIONTYPEID = 7  --MANAGE
                        THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * {5} * (S.GROSSCOMMISSION - CONJUNCTIONALCOMMISSION)
	 	            WHEN COMMISSIONTYPEID = 8  --SELL
                        THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * {6} * (S.GROSSCOMMISSION - CONJUNCTIONALCOMMISSION)
	        END, 0)  FROM SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID
	        JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID
	        WHERE SS.CALCULATEDAMOUNT > 0 AND S.ID = {0};
            ", SaleID, dFarm, dMentor, dLead, dList, dManage, dSell));
    }

    /// <summary>
    /// Calculate and sales bonuses based on a junior agent belonging to a TEAM
    /// </summary>
    private void calculateSalesCommissionBonus() {
        DB.runNonQuery("DELETE FROM MENTORBONUS WHERE SALEID = " + intSaleID);
        if (blnIgnoreSaleBonus || intPayPeriodID == -1)
            return;
        string szUpdateSQL = "";

        //Check to see if anyone in this sale belongs to a team
        foreach (SalesSplit oS in lSaleSplits) {
            foreach (UserSalesSplit uSS in oS.lUserSplits) {
                UserDetail oD = G.UserInfo.getUser(uSS.UserID);
                if (oD == null || oD.MentorID <= 0)
                    continue;
                if (uSS.ActualPayment == 0)
                    continue;
                string szDesc = Convert.ToString((CommissionType)uSS.CommissionTypeID) + ": 5% of " + uSS.ActualPayment;
                szUpdateSQL += String.Format(@"
                    INSERT INTO MENTORBONUS(MENTORUSERID, USERID, SALEID, COMMISSIONBONUS, SALARYBONUS, PAYPERIODID, NOTES)
                    VALUES({0}, {1}, {2}, {3}, 0, {4}, '{5}')", oD.MentorID, oD.ID, intSaleID, 0.05 * uSS.ActualPayment, intPayPeriodID, DB.escape(szDesc));
            }
        }
        if (szUpdateSQL != "")
            DB.runNonQuery(szUpdateSQL);
    }

    /// <summary>
    /// Updates the actual pay on this property for the agent - this function will not generate MYOB records that have changed as a result of this update
    /// </summary>
    /// <param name="AgentID"></param>
    public void updateActualPay(int AgentID) {
        UserDetail oU = G.UserInfo.getUser(AgentID);
        if (oU == null)
            return;
        double YTDTotal = oU.getYTDTotal(this.PayPeriodID);
        string szSQL = "";
        foreach (SalesSplit oS in lSaleSplits) {
            foreach (UserSalesSplit uSS in oS.lUserSplits) {
                if (uSS.UserID == AgentID) {
                    uSS.calculateActualPay(YTDTotal);
                    Utility.Append(ref szSQL, String.Format("UPDATE USERSALESPLIT SET ACTUALPAYMENT = {0} WHERE ID = {1}", uSS.ActualPayment, uSS.ID), ";");
                }
            }
        }
        if (szSQL != "")
            DB.runNonQuery(szSQL);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="SaleID"></param>
    /// <param name="SaleSplitID"></param>
    /// <param name="CommissionTypeID"></param>
    /// <param name="Amount"></param>
    /// <param name="oAmountType"></param>
    /// <param name="CalculatedAmount"></param>
    /// <returns></returns>
    public string updateSaleSplit(int SaleSplitID, int CommissionTypeID, double Amount, AmountType oAmountType, double CalculatedAmount) {
        SalesSplit oSS = lSaleSplits.Find(t => t.ID == SaleSplitID);
        if (oSS == null) {
            oSS = new SalesSplit(SaleID, -1, CommissionTypeID, Amount, oAmountType, CalculatedAmount, "");
            lSaleSplits.Add(oSS);
        } else {
            oSS.CommissionTypeID = CommissionTypeID;
            oSS.Amount = Amount;
            oSS.AmountType = oAmountType;
            oSS.CalculatedAmount = CalculatedAmount;
        }

        return oSS.writeToDB();
    }

    public string updateUserSaleSplit(int SaleSplitID, int UserSaleSplitID, int UserID, int CommissionTypeID, double Amount, AmountType oAmountType, double CalculatedAmount, double ActualPayment, bool IncludeInKPI, int OfficeID) {
        SalesSplit oSS = lSaleSplits.Find(t => t.ID == SaleSplitID);
        if (oSS == null) {
            //reload the sale splits
            lSaleSplits.Clear();
            loadSaleSplits(false);
            oSS = lSaleSplits.Find(t => t.ID == SaleSplitID);
        }

        //Match by split ID, or grab the fletchers commission split for the one that is the Fletchers row
        UserSalesSplit oUSS = oSS.lUserSplits.Find(t => (t.ID == UserSaleSplitID || oSS.CommissionTypeID == 35));
        //TODO - setup the DB to marks rows as deleted instead of updating them if the property is marked as completed
        if (oUSS == null) {
            if (Amount == 0 || CalculatedAmount == 0) {
                return ""; //We don't want to update an empty record
            }
            oUSS = new UserSalesSplit(SaleSplitID, -1, UserID, Amount, oAmountType, CalculatedAmount, ActualPayment, IncludeInKPI, oSS.CommissionTypeID, OfficeID);
            oSS.lUserSplits.Add(oUSS);
        } else {
            oUSS.UserID = UserID;
            oUSS.Amount = Amount;
            oUSS.oAmountType = oAmountType;
            oUSS.CalculatedAmount = CalculatedAmount;
            oUSS.IncludeInKPI = IncludeInKPI;
            oUSS.CommissionTypeID = oSS.CommissionTypeID;
            oUSS.OfficeID = OfficeID;
        }
        double dYTDTotal = 0;
        UserDetail oU = G.UserInfo.getUser(oUSS.UserID);
        if (oU != null)
            dYTDTotal = oU.getYTDTotal(this.PayPeriodID);
        return oUSS.updateToDB(this.PayPeriodID, dYTDTotal);
    }

    /// <summary>
    /// Updates the sales expense with the new information
    /// </summary>
    /// <param name="SaleID"></param>
    /// <param name="ExpenseTypeID"></param>
    /// <param name="Amount"></param>
    /// <param name="oAmountType"></param>
    /// <param name="CalculatedAmount"></param>
    /// <returns></returns>
    public string updateSaleExpense(int ExpenseID, int ExpenseTypeID, double Amount, AmountType oAmountType, double CalculatedAmount) {
        SalesExpense oSE = lSaleExpenses.Find(t => t.ID == ExpenseID);
        if (oSE == null) {
            oSE = new SalesExpense(-1, SaleID, ExpenseTypeID, Amount, oAmountType, CalculatedAmount);
            lSaleExpenses.Add(oSE);
        }
        oSE.ExpenseTypeID = ExpenseTypeID;
        oSE.Amount = Amount;
        oSE.CalculatedAmount = CalculatedAmount;
        oSE.oAmountType = oAmountType;
        return oSE.updateToDB();
    }

    public void deleteSalesExpense(int SalesExpenseID) {
        SalesExpense oSE = lSaleExpenses.Find(t => t.ID == SalesExpenseID);
        if (oSE != null)
            oSE.delete();
    }

    /// <summary>
    /// Used to check for various null / invalid date values.  including DateTime.MinVal & 1900-01-01
    /// </summary>
    private static DateTime dtDateOutOfRangeMin = Convert.ToDateTime("1900-01-02");

    /// <summary>
    /// Produces RECode for records imported from B&D
    /// </summary>
    /// <param name="Address"></param>
    /// <returns></returns>
    public static string CreateCode(String Address) {
        if (String.IsNullOrEmpty(Address))
            return "";

        Match match1 = Regex.Match(Address, "[0-9]");

        if (match1.Index > 1)
            Address = Address.Substring(match1.Index);

        Match match2 = Regex.Match(Address, " [A-Za-z'`]{2}");
        return String.Format("{1}{0}",
            Address.Substring(0, match2.Index).Replace(" ", "").ToUpper(),
            Address.Substring(match2.Index + 1).Replace("'", "").Replace("`", "").Substring(0, 4).ToUpper().TrimEnd(' '));
    }

    /// <summary>
    /// Processes Imports from B&D
    /// </summary>
    public static void processBDImports(int MinTimeStamp = 0, int BnDSalesID = -1) {
        string szFilterRecords = string.Format("WHERE P.TIMESTAMP > {0} AND SV.SOLDDATE >= '{1}'", MinTimeStamp, Utility.formatDate(G.TransitionToBnDDate));
        if (BnDSalesID > -1) {
            szFilterRecords += string.Format("AND SL.ID = {0}", BnDSalesID);
        }

        String szSQL = String.Format(@"
                    -- 0. BnD records to be imported
                    SELECT SL.ID AS BnDSALEID, *,
						(SELECT SUM(AMOUNT) FROM SALESVOUCHERDEDUCTION WHERE (REASON = 'External Conjunctional' OR REASON = 'Referral/Conjunctional' OR REASON like '%Conjunctional%') AND SALESVOUCHERID = SV.ID) AS CONJUNCTIONAL,
	                    SUBSTRING ( -- Calculate purchaser suburb(s) in a comma seperated list if required
		                    (SELECT ', ' + SUBURB FROM PROPERTY PUR_P
			                    JOIN CONTACT C ON C.RESIDENTIALPROPERTYID = PUR_P.ID
			                    JOIN CONTACTACTIVITY CA ON CA.CONTACTID = C.ID
			                    JOIN CONTACTACTIVITYTYPE CT ON CT.ID = CA.CONTACTACTIVITYTYPEID
		                    WHERE CT.NAME IN ('PURCHASER', 'PURCHASED') AND CA.SALESLISTINGID = SL.ID
		                    GROUP BY SUBURB
		                    FOR XML PATH(''))
		                    , 3, 100
	                    ) AS PURCHASERSUBURB
                    FROM PROPERTY P
	                    JOIN SALESLISTING SL ON SL.PROPERTYID = P.ID
                    	JOIN SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
					{1}

                    -- 1. BnD records already imported
                    SELECT ID, BnDSALEID, AUCTIONDATE, LOCKCOMMISSION FROM {0}.dbo.SALE
                    WHERE BnDSALEID IN (
                        SELECT SL.ID FROM PROPERTY P
	                        JOIN SALESLISTING SL ON SL.PROPERTYID = P.ID
                    	    JOIN SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
					    {1}
                    )", Client.DBName, szFilterRecords);
        using (DataSet ds = DB.runDataSet(szSQL, DB.BoxDiceDBConn)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                //Check it the Sale is already imported
                DataRow dr_SALES = ds.Tables[1].Rows.Count == 0 ? null : ds.Tables[1].Rows.OfType<DataRow>().FirstOrDefault(x => DB.readInt(x["BnDSALEID"]) == DB.readInt(dr["BnDSALEID"]));
                int intDbID = dr_SALES == null ? Int32.MinValue : DB.readInt(dr_SALES["ID"]);

                Sale oS = new Sale(intDbID, false, false);
                bool blnDataChanged = false;
                sqlUpdate oSQL = new sqlUpdate("SALE", "ID", intDbID);

                oSQL.add("BnDSALEID", DB.readInt(dr["BnDSALEID"]));

                oSQL.add("ADDRESS", dr["ADDRESS"].ToString());
                oSQL.add("CODE", CreateCode(dr["ADDRESS"].ToString()));

                // Will always contain a date given search criteria
                DateTime dtSale = DB.readDate(dr["SOLDDATE"].ToString());
                oSQL.add("SALEDATE", Utility.formatDate(dtSale));

                // Purchaser Suburb only added when listing is first loaded
                if (intDbID == Int32.MinValue)
                    oSQL.add("PURCHASERSUBURB", DB.readString(dr["PURCHASERSUBURB"]));

                DateTime dtActualSettlement = Valid.checkDate(dr["ACTUALSETTLEMENT"].ToString(), DateTime.MinValue);
                if (dtActualSettlement < dtDateOutOfRangeMin) {
                    oSQL.addNull("SETTLEMENTDATE");
                } else {
                    oSQL.add("SETTLEMENTDATE", Utility.formatDate(dtActualSettlement));
                }

                // Entitlement date is drawn from :
                //     Comm Drawn/ Received
                //     Actual Settlement date
                //     Expected Settlement date
                // Entitlement date takes the first valid date from this list ie. Comm Drawn, then if that is not set Actual Settlement etc.
                DateTime dtCommDrawn = Valid.checkDate(dr["COMMISSIONDRAWNDATE"].ToString(), DateTime.MinValue);
                DateTime dtExpectSettlement = Valid.checkDate(dr["EXPECTEDSETTLEMENT"].ToString(), DateTime.MinValue);

                if (dtCommDrawn > dtDateOutOfRangeMin)
                    oSQL.add("ENTITLEMENTDATE", Utility.formatDate(dtCommDrawn));
                else if (dtActualSettlement > dtDateOutOfRangeMin)
                    oSQL.add("ENTITLEMENTDATE", Utility.formatDate(dtActualSettlement));
                else if (dtExpectSettlement > dtDateOutOfRangeMin)
                    oSQL.add("ENTITLEMENTDATE", Utility.formatDate(dtExpectSettlement));
                else
                    oSQL.addNull("ENTITLEMENTDATE");

                DateTime dtListed = Valid.checkDate(dr["LISTEDDATE"].ToString(), DateTime.MinValue);
                if (dtListed < dtDateOutOfRangeMin)
                    oSQL.addNull("LISTEDDATE");
                else
                    oSQL.add("LISTEDDATE", Utility.formatDate(dtListed));

                // Auction date is only taken from B&D if the value is empty.
                // This is because it is an editable field and needs to keep edited values
                DateTime dtAuction = DateTime.MinValue;
                if (String.IsNullOrEmpty(oS.AuctionDate) || Convert.ToDateTime(oS.AuctionDate) < dtDateOutOfRangeMin || intDbID < 0) {
                    dtAuction = Valid.checkDate(dr["AUCTIONAT"].ToString(), DateTime.MinValue);
                    if (dtAuction < dtDateOutOfRangeMin)
                        oSQL.addNull("AUCTIONDATE");
                    else
                        oSQL.add("AUCTIONDATE", Utility.formatDate(dtAuction));
                }

                // isAuction is true if Auction and Sale date are the same day - allows for differences in time
                oSQL.add("ISAUCTION", false); //dtAuction.Date == dtSale.Date);

                DateTime dtFallThrough = Valid.checkDate(dr["WITHDRAWNON"].ToString(), DateTime.MinValue);
                if (dtFallThrough < dtDateOutOfRangeMin)
                    oSQL.addNull("FALLENTHROUGHDATE");
                else
                    oSQL.add("FALLENTHROUGHDATE", Utility.formatDate(dtFallThrough));

                if (Valid.isNumeric(dr["SALEPRICE"].ToString())) {
                    oSQL.add("SALEPRICE", dr["SALEPRICE"].ToString());
                } else
                    oSQL.add("SALEPRICE", 0);

                oSQL.add("SUBURB", Convert.ToString(dr["SUBURB"]));

                // LockCommission flag is currently set through manual SQL queries or when commission changed manually on special update page.
                if (dr_SALES == null || !DB.readBool(dr_SALES["LOCKCOMMISSION"])) {
                    oSQL.add("CONJUNCTIONALCOMMISSION", dr["CONJUNCTIONAL"] != System.DBNull.Value ? Math.Round(DB.readDouble(dr["CONJUNCTIONAL"]) / 1.1, 2) : 0);
                    Double dbGrossCommission = dr["GROSSCOMMISSION"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["GROSSCOMMISSION"]);
                    oSQL.add("GROSSCOMMISSION", dbGrossCommission);
                }

                if (intDbID == int.MinValue) {
                    DB.runNonQuery(oSQL.createInsertSQL());
                    //intDbID = DB.getScalar("SELECT MAX(ID) FROM SALE", 0);
                } else {
                    // CHeck to see if we are updating a current or future record and flag it as such
                    if (blnDataChanged && dtActualSettlement != DateTime.MinValue && dtActualSettlement >= G.CurrentPayPeriodStart)
                        oSQL.add("ISSOURCEMODIFIED", 1);

                    DB.runNonQuery(oSQL.createUpdateSQL());
                }
            }
        }

        DB.runNonQuery(string.Format(@"
            -- temp fix for IsAuction not calculating correctly under blimps
            update sale set ISAUCTION = 1 where SALEDATE = AUCTIONDATE and SALEDATE > '2016-07-01'"));

        checkWithdrawnSales();
    }

    /// <summary>
    /// Checks to see if any sales have been withdrawn, and sets the gross commission to 0 as a result
    /// </summary>
    private static void checkWithdrawnSales() {
        string szSQL = String.Format(@"
                UPDATE   {0}.dbo.SALE
                    SET GROSSCOMMISSION = 0
                WHERE BNDSALEID in (
                    SELECT ID
                    FROM SALESLISTING
                    WHERE (WITHDRAWNON IS NOT NULL AND STATUS = 'listing_cancelled')
				);", Client.DBName);
        DB.runNonQuery(szSQL, DB.BoxDiceDBConn);
    }

    /// <summary>
    /// Loads all the records where a salesperson has a part of the splits
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public static DataSet loadSalesForSalesPerson(int UserID, DateTime dtStartDate, DateTime dtEndDate, string LocationFilter = "") {
        string szLocationFilter = "";
        if (LocationFilter != "")
            szLocationFilter = String.Format(" AND (CODE LIKE '%{0}%' or S.ADDRESS LIKE '%{0}%' )", Utility.formatForDB(LocationFilter));

        string szSQL = String.Format(@"
            -- Sales
            SELECT S.ID, S.ENTITLEMENTDATE, MIN(S.CODE) AS CODE, S.ADDRESS, MIN(S.SALEDATE) AS SALEDATE,  MIN(S.SETTLEMENTDATE) AS SETTLEMENTDATE, min(S.SALEPRICE) AS SALEPRICE, SUM(USS.CALCULATEDAMOUNT) AS COMMISSIONTOTAL
            FROM SALE S
            JOIN SALESPLIT SS ON S.ID = SS.SALEID AND SS.RECORDSTATUS = 0
            JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
            WHERE USS.USERID = {0} AND S.ENTITLEMENTDATE BETWEEN '{1} 00:00:00' AND '{2} 23:59:59' {3}
            GROUP BY S.ID, S.ENTITLEMENTDATE, S.ADDRESS
            ORDER BY S.ENTITLEMENTDATE ", UserID, Utility.formatDate(dtStartDate), Utility.formatDate(dtEndDate), szLocationFilter);

        return DB.runDataSet(szSQL);
    }

    /// <summary>
    /// Loads all the records where a salesperson has a part of the splits
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public static DataSet loadYTDSalesForSalesPerson(int UserID, DateTime StartDate) {
        string szSQL = String.Format(@"
            -- YTD
            SELECT P.ID AS PAYPERIODID, DATENAME(mm, P.STARTDATE) AS MONTH, DATEPART(yyyy,P.STARTDATE) as YEAR, SUM(ISNULL(USS.CALCULATEDAMOUNT, 0)) AS COMMISSIONTOTAL,  SUM(ISNULL(USS.GRAPHCOMMISSION, 0)) AS GRAPHCOMMISSION
            FROM PAYPERIOD P
            LEFT JOIN SALE S ON S.PAYPERIODID = P.ID
            LEFT JOIN SALESPLIT SS ON S.ID = SS.SALEID AND SS.RECORDSTATUS = 0
            LEFT JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.USERID = {0} AND USS.RECORDSTATUS < 1
            WHERE P.STARTDATE >= '{1}' AND S.STATUSID = 2
            GROUP BY  DATEPART(mm, P.STARTDATE), DATEPART(yyyy,P.STARTDATE), DATENAME(mm, P.STARTDATE), P.ID
            ORDER BY DATEPART(yyyy,P.STARTDATE), DATEPART(mm, P.STARTDATE)", UserID, Utility.formatDate(StartDate));
        return DB.runDataSet(szSQL);
    }

    /// <summary>
    /// Loads the commission based values for an agent for this passed in PayPeriod. Also includes the data to calculate the bonus payable to this agent
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="PayPeriod"></param>
    /// <returns></returns>
    public static DataSet loadCommissionStatementForPayPeriod(int UserID, int PayPeriod) {
        string szIDs = G.PayPeriodInfo.getPayPeriodsForYTD(PayPeriod);
        string szSQL = String.Format(@"
            --0) Commission paid to this agent
            SELECT '' AS COMMISSIONNAME, SS.COMMISSIONTYPEID, S.CODE, S.ADDRESS, S.ENTITLEMENTDATE,
                S.CONJUNCTIONALCOMMISSION AS CONJUNCTIONALCOMMISSION, S.GROSSCOMMISSION,
            USS.AMOUNT AS AMOUNT, USS.AMOUNTTYPEID, USS.ACTUALPAYMENT, USS.CALCULATEDAMOUNT AS GRAPHTOTAL,
                CASE WHEN SS.AMOUNTTYPEID = 1 THEN CAST(SS.AMOUNT as varchar) + '%' ELSE '$'+ CAST(SS.AMOUNT as varchar) END AS SS_AMOUNT, USS.COMMISSIONTIERID,
            (SELECT SUM(CALCULATEDAMOUNT) FROM SALEEXPENSE WHERE SALEID = S.ID) AS OTTEXP, SS.AMOUNT AS TOTALCOMMPERCENTAGE

            FROM USERSALESPLIT USS
            JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1   AND USS.RECORDSTATUS < 1
            JOIN SALE S ON SS.SALEID = S.ID
            WHERE USS.USERID = {0}
            AND S.PAYPERIODID = {1} AND S.STATUSID = 2;

            -- 1) Mentoring bonus paid to the senior if they are mentoring any juniors
            SELECT  S.CODE, S.ADDRESS, S.ENTITLEMENTDATE, S.CONJUNCTIONALCOMMISSION AS CONJUNCTIONALCOMMISSION, S.GROSSCOMMISSION,
                MB.COMMISSIONBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM MENTORBONUS MB
            JOIN SALE S ON MB.SALEID = S.ID
            JOIN DB_USER U ON U.ID = MB.USERID
            WHERE MB.MENTORUSERID = {0} AND S.PAYPERIODID = {1} AND S.STATUSID = 2
            ORDER BY MB.USERID, S.ADDRESS;

            -- 2) Salary portion paid to the senior if any of the juniors are on a salary
            SELECT  MB.SALARYBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME, S.FIRSTNAME + ' ' + S.LASTNAME AS SENIOR
            FROM MENTORBONUS MB JOIN DB_USER U ON MB.USERID = U.ID
            JOIN DB_USER S ON S.ID  = MB.MENTORUSERID
            WHERE MB.MENTORUSERID = {0}  AND MB.PAYPERIODID = {1} AND MB.SALEID IS NULL AND MB.SALARYBONUS > 0
            ORDER BY U.FIRSTNAME, U.LASTNAME;

            -- 3) Deductions paid to mentors if this is a junior
            SELECT  S.CODE, S.ADDRESS, MB.COMMISSIONBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM MENTORBONUS MB
            JOIN SALE S ON MB.SALEID = S.ID
            JOIN DB_USER U ON U.ID = MB.MENTORUSERID
            WHERE MB.USERID = {0} AND S.PAYPERIODID = {1} AND S.STATUSID = 2 AND MB.COMMISSIONBONUS > 0
            UNION --SalaryBonus
            SELECT  '', 'Salary bonus', MB.SALARYBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM MENTORBONUS MB
            JOIN DB_USER U ON U.ID = MB.MENTORUSERID
            WHERE MB.USERID = {0} AND MB.PAYPERIODID = {1} AND MB.SALARYBONUS > 0
            UNION --Initial commission bonus of 5%
            SELECT  '', 'Initial commission payment', MB.COMMISSIONBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM MENTORBONUS MB
            JOIN DB_USER U ON U.ID = MB.MENTORUSERID
            WHERE MB.USERID = {0} AND MB.PAYPERIODID = {1} AND MB.SALEID IS NULL AND MB.COMMISSIONBONUS > 0
            ORDER BY 1;

            -- 4) Total commission for all juniors who are  being mentored by this senior - YTD
            SELECT SUM(USS.GRAPHCOMMISSION) AS YTD_TOTAL, USS.USERID,  U.FIRSTNAME + ' ' + U.LASTNAME AS NAME,
            SUM(CASE WHEN(PAYPERIODID = {1}) then USS.GRAPHCOMMISSION ELSE 0 END) as PAYPERIOD_TOTAL
            FROM USERSALESPLIT USS
            JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1   AND USS.RECORDSTATUS < 1
            JOIN SALE S ON SS.SALEID = S.ID
            JOIN DB_USER U ON U.ID = USS.USERID
            WHERE U.TEAMID = {0} AND S.PAYPERIODID IN ({2}) AND S.STATUSID = 2
            GROUP BY USS.USERID,  U.FIRSTNAME + ' ' + U.LASTNAME;

            -- 5) Initial commission paid for salaried juniors
            SELECT  MB.COMMISSIONBONUS, MB.USERID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME
            FROM MENTORBONUS MB JOIN DB_USER U ON MB.USERID = U.ID
             WHERE MB.MENTORUSERID = {0} AND MB.PAYPERIODID = {1} AND MB.SALEID IS NULL AND MB.COMMISSIONBONUS > 0
            ORDER BY U.FIRSTNAME, U.LASTNAME;

            -- 6) Users total graph commission for the year to determine SBS amounts
            SELECT  SUM(CASE WHEN(PAYPERIODID < {1} ) then USS.GRAPHCOMMISSION ELSE 0 END) as ELIGIBLE_COMMISSION,
            SUM(CASE WHEN(PAYPERIODID = {1} ) then USS.GRAPHCOMMISSION ELSE 0 END) as PAYPERIOD_TOTAL
            FROM USERSALESPLIT USS
            JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
            JOIN SALE S ON SS.SALEID = S.ID
             WHERE USS.USERID = {0} AND S.PAYPERIODID IN ({2}) AND S.STATUSID = 2
            GROUP BY USS.USERID;

            --7) All sales for the current pay period so we can determine the appropriate SBS amounts
            SELECT S.ID AS SALEID, SUM(USS.GRAPHCOMMISSION) AS ELIGIBLE_COMMISSION, {0} AS USERID
            FROM USERSALESPLIT USS
            JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
            JOIN SALE S ON SS.SALEID = S.ID
             WHERE USS.USERID = {0} AND S.PAYPERIODID IN ({1}) AND S.STATUSID = 2
            GROUP BY S.ID, S.SALEDATE, S.ADDRESS
            ORDER BY S.SALEDATE, S.ADDRESS

           ", UserID, PayPeriod, szIDs, Client.EOFYBonusMonthDelay);

        return DB.runDataSet(szSQL);
    }

    /// <summary>
    /// Loads commission data beyond the pay period - used in Commission Statement report
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="PayPeriod"></param>
    /// <returns></returns>
    public static DataSet loadFutureCommissionStatement(int UserID, int PayPeriod) {
        string szPayPeriodDateFilter = Utility.formatDate(G.CurrentPayPeriodEnd);
        PayPeriod oP = G.PayPeriodInfo.getPayPeriod(PayPeriod);
        if (oP != null) {
            szPayPeriodDateFilter = Utility.formatDate(oP.EndDate);
        }

        string szSQL = String.Format(@"
            SELECT '' AS COMMISSIONNAME, SS.COMMISSIONTYPEID, S.CODE, S.ADDRESS, S.ENTITLEMENTDATE, USS.INCLUDEINKPI, S.SALEDATE,
                S.CONJUNCTIONALCOMMISSION AS CONJUNCTIONALCOMMISSION, S.GROSSCOMMISSION, SS.ID AS SALESPLITID,
                USS.AMOUNT AS AMOUNT, USS.AMOUNTTYPEID, USS.ACTUALPAYMENT, USS.ID AS USERSPLITID, USS.CALCULATEDAMOUNT,
                CASE WHEN SS.AMOUNTTYPEID = 1 THEN CAST(SS.AMOUNT as varchar) + '%' ELSE '$'+ CAST(SS.AMOUNT as varchar) END AS SS_AMOUNT,
                USS.OFFICEID, (SELECT SUM(CALCULATEDAMOUNT) FROM SALEEXPENSE WHERE SALEID = S.ID) AS OTTEXP,  SS.AMOUNT AS TOTALCOMMPERCENTAGE

            FROM USERSALESPLIT USS
            JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
            JOIN SALE S ON SS.SALEID = S.ID
            where USS.USERID = {0} AND S.STATUSID != 3 AND S.SALEDATE IS NOT NULL
            AND (S.PAYPERIODID IS NULL OR (S.PAYPERIODID != {2} AND S.ENTITLEMENTDATE > '{1}'))
            ", UserID, szPayPeriodDateFilter, PayPeriod);
        DataSet ds = DB.runDataSet(szSQL);
        UserDetail oU = G.UserInfo.getUser(UserID);
        if (oU.PaymentStructure == PayBand.JuniorTeamSalary || oU.PaymentStructure == PayBand.JuniorNoTeamSalary || UserDetail.SpecialYearEndUserIDs.Contains(UserID)) {
            DB.runNonQuery("-- Payment structure for " + oU.Initials + " " + oU.PaymentStructure.ToString());
            recalculateActualPayments(ref ds, oU, PayPeriod);
        }
        return ds;
    }

    private static void recalculateActualPayments(ref DataSet ds, UserDetail oUser, int PayPeriod) {
        double dYTDTotal = oUser.getYTDTotal(PayPeriod);
        double dPrevYTDTotal = oUser.getPrevYTDTotal(PayPeriod);

        foreach (DataRow dr in ds.Tables[0].Rows) {
            DateTime dtSaleDate = DB.readDate(dr["SALEDATE"]);
            //Recalculate the sales split
            UserSalesSplit oUSS = new UserSalesSplit(DB.readInt(dr["SALESPLITID"]), DB.readInt(dr["USERSPLITID"]), oUser.ID, Convert.ToDouble(dr["AMOUNT"]), (AmountType)Convert.ToInt32(dr["AMOUNTTYPEID"]), Convert.ToDouble(dr["CALCULATEDAMOUNT"]), Convert.ToDouble(dr["ACTUALPAYMENT"]), Convert.ToBoolean(dr["INCLUDEINKPI"]), DB.readInt(dr["COMMISSIONTYPEID"]), DB.readInt(dr["OFFICEID"]));
            double dEffectiveCommission = oUser.getEffectiveYTDCommission(dYTDTotal, dPrevYTDTotal, dtSaleDate, PayPeriod);

            oUSS.calculateActualPay(dEffectiveCommission, oUser.getPaymentStructure(dtSaleDate));
            dr["ACTUALPAYMENT"] = oUSS.ActualPayment;
        }
    }

    /// <summary>
    /// Loads all the records where a salesperson has a part of the splits
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public static DataSet loadFutureSalesForSalesPerson(int UserID, DateTime dtStartDate, DateTime dtEndDate, string LocationFilter = "") {
        string szLocationFilter = "";
        if (LocationFilter != "")
            szLocationFilter = String.Format(" AND CODE LIKE '%{0}%' or S.ADDRESS LIKE '%{0}%' ", Utility.formatForDB(LocationFilter));
        string szSQL = String.Format(@"
            SELECT S.ID, S.ENTITLEMENTDATE, S.ADDRESS, SUM(USS.ACTUALPAYMENT) AS AMOUNT
            FROM SALE S JOIN SALESPLIT SS ON S.ID = SS.SALEID
            JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID  AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
            WHERE USS.USERID = {0} AND S.ENTITLEMENTDATE BETWEEN '{1} 00:00:00' AND '{2} 23:59:59' {3} AND S.SALEDATE IS NOT NULL
            GROUP BY S.ID, S.ENTITLEMENTDATE, S.ADDRESS
            ORDER BY S.ENTITLEMENTDATE ", UserID, Utility.formatDate(dtStartDate), Utility.formatDate(dtEndDate), szLocationFilter);

        return DB.runDataSet(szSQL);
    }

    private void loadSaleSplits(bool LoadAllCommissionTypes) {
        bool blnSplitInsert = DB.getScalar(string.Format(@"select COUNT (ID) from SALESPLIT where SALEID = {0}", intSaleID), 0) == 0;
        string szSQL = String.Format(@"
            SELECT SS.ID AS SPLITID, SS.AMOUNT AS SPLITAMOUNT, SS.AMOUNTTYPEID AS SPLITAMOUNTTYPE, SS.CALCULATEDAMOUNT as SPLITCALCULATEDAMOUNT, SS.COMMISSIONTYPEID,
            ISNULL(USS.ID, -1) AS USERSPLITID, ISNULL(USS.AMOUNT, 0) as USERSPLITAMOUNT, ISNULL(USS.AMOUNTTYPEID, 0) AS USERAMOUNTTYPE, ISNULL(USS.CALCULATEDAMOUNT, 0) as USERCALCULATEDAMOUNT,
            ISNULL(USS.USERID, -1) as USERID, ISNULL(USS.INCLUDEINKPI, 0) AS INCLUDEINKPI, ISNULL(USS.ACTUALPAYMENT, 0) as USERACTUALPAYMENT, ISNULL(USS.OFFICEID, -1) AS OFFICEID
            FROM SALESPLIT SS
            LEFT JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
            LEFT JOIN DB_USER U on U.ID = USS.USERID
            WHERE SS.SALEID = {0}
            ORDER BY U.LASTNAME, U.FIRSTNAME", intSaleID);
        DataSet dsData = DB.runDataSet(szSQL);

        ListTypes oListTypes = new ListTypes(ListType.Commission);

        foreach (ListObject oCommissionType in G.CommTypeInfo.CommissionTypeList) {
            SalesSplit oSS = null;
            DataView oDataView = dsData.Tables[0].DefaultView;
            oDataView.RowFilter = string.Format(@"COMMISSIONTYPEID = {0}", (int)oCommissionType.ID);
            if (oDataView.Count == 0) {
                if (blnSplitInsert) {
                    double calcAmount = Convert.ToDouble(oCommissionType.Amount);
                    if (oCommissionType.AmountType == AmountType.Percent) {
                        calcAmount = Convert.ToDouble(((Convert.ToDouble(oCommissionType.Amount) * dAmount) / 100).ToString("0.00"));
                    }
                    oSS = new SalesSplit(intSaleID, -1, oCommissionType.ID, Convert.ToDouble(oCommissionType.Amount), oCommissionType.AmountType, calcAmount, oCommissionType.Name);
                } else {
                    //Show the blank Row
                    oSS = new SalesSplit(intSaleID, -1, oCommissionType.ID, Convert.ToDouble("0.00"), AmountType.Dollar, Convert.ToDouble("0.00"), oCommissionType.Name);
                }
            } else {
                foreach (DataRowView dr in oDataView) {
                    if (oSS == null)
                        oSS = new SalesSplit(intSaleID, Convert.ToInt32(dr["SPLITID"]), oCommissionType.ID, Convert.ToDouble(dr["SPLITAMOUNT"]), (AmountType)Convert.ToInt32(dr["SPLITAMOUNTTYPE"]), Convert.ToDouble(dr["SPLITCALCULATEDAMOUNT"]), oCommissionType.Name);
                    oSS.addUserSplit(new UserSalesSplit(Convert.ToInt32(dr["SPLITID"]), Convert.ToInt32(dr["USERSPLITID"]), Convert.ToInt32(dr["USERID"]),
                        Convert.ToDouble(dr["USERSPLITAMOUNT"]), (AmountType)Convert.ToInt32(dr["USERAMOUNTTYPE"]),
                        Convert.ToDouble(dr["USERCALCULATEDAMOUNT"]), Convert.ToDouble(dr["USERACTUALPAYMENT"]), Convert.ToBoolean(dr["INCLUDEINKPI"]),
                        oCommissionType.ID, Convert.ToInt32(dr["OFFICEID"])));
                }
            }
            lSaleSplits.Add(oSS);
        }
    }

    protected void loadSaleExpenses(bool blnSaleIsComplete) {
        string szSQL = String.Format(@"
            SELECT SE.ID, SE.EXPENSETYPEID, SE.AMOUNT, SE.AMOUNTTYPEID, SE.CALCULATEDAMOUNT
            FROM SALEEXPENSE SE
            WHERE SE.SALEID = {0}", intSaleID);
        DataSet dsData = DB.runDataSet(szSQL);
        if (dsData.Tables[0].Rows.Count == 0 && !blnSaleIsComplete) {
            insertDefaultSaleExpense();
            dsData = DB.runDataSet(szSQL);
        }
        foreach (DataRow oRow in dsData.Tables[0].Rows) {
            dExpenseAmount += Convert.ToDouble(oRow["CALCULATEDAMOUNT"]);
            lSaleExpenses.Add(new SalesExpense(Convert.ToInt32(oRow["ID"]), intSaleID, Convert.ToInt32(oRow["EXPENSETYPEID"]), Convert.ToDouble(oRow["AMOUNT"]), (AmountType)Convert.ToInt32(oRow["AMOUNTTYPEID"]), Convert.ToDouble(oRow["CALCULATEDAMOUNT"])));
        }
    }

    private void insertDefaultSaleExpense() {
        string intExpenseTypeID = "";
        string szAmount = "0";
        AmountType oAmountType = AmountType.Percent;
        Double dCalculatedAmount = 0;

        string szSQL = string.Format(@"
            SELECT ID AS EXPENSETYPEID, AMOUNT, AMOUNTTYPEID
            FROM LIST WHERE LISTTYPEID = {0} AND NAME = '6% Incentive' AND ISACTIVE = {1}", (int)ListType.OffTheTop, (int)TrueFalse.True);
        DataSet ds = DB.runDataSet(szSQL);

        foreach (DataRow oRow in ds.Tables[0].Rows) {
            intExpenseTypeID = oRow["EXPENSETYPEID"].ToString();
            szAmount = oRow["AMOUNT"].ToString();
            oAmountType = (AmountType)Convert.ToInt32(oRow["AMOUNTTYPEID"]);
        }
        if (intExpenseTypeID != "") {
            sqlUpdate oSQL = new sqlUpdate("SALEEXPENSE", "ID", -1);
            oSQL.add("SALEID", intSaleID);
            oSQL.add("EXPENSETYPEID", intExpenseTypeID);
            oSQL.add("AMOUNT", Utility.isDouble(szAmount));
            oSQL.add("AMOUNTTYPEID", (int)oAmountType);
            if (oAmountType == AmountType.Dollar)
                oSQL.add("CALCULATEDAMOUNT", Utility.isDouble(szAmount));
            else {
                dCalculatedAmount = ((dGrossCommission) * Convert.ToDouble(szAmount)) / 100;
                oSQL.add("CALCULATEDAMOUNT", dCalculatedAmount.ToString());
            }
            DB.runNonQuery(oSQL.createInsertSQL());
        }
    }
}

/// <summary>
/// Contains the details of a commission split for a salce
/// </summary>
public class SalesSplit : AuditClass {
    private int intID = 0;
    private int intCommissionTypeID = 0;
    private double dAmount = 0;
    private AmountType oType = 0;
    private double dCalculatedAmount = 0;
    public List<UserSalesSplit> lUserSplits = new List<UserSalesSplit>();
    private string szCommissionName = "";
    private int intSaleID = -1;

    public int ID {
        get { return intID; }
    }

    public string CommissionName {
        get { return szCommissionName; }
    }

    public int CommissionTypeID {
        get { return intCommissionTypeID; }
        set { intCommissionTypeID = setValue("Commission type", intCommissionTypeID, value); }
    }

    public int SalesSplitID {
        get { return intID; }
    }

    public double Amount {
        get { return dAmount; }
        set { dAmount = setValue("Amount", dAmount, value); }
    }

    public double CalculatedAmount {
        get { return dCalculatedAmount; }
        set { dCalculatedAmount = setValue("Calculated amount", dCalculatedAmount, value); }
    }

    public AmountType AmountType {
        get {
            return oType;
        }
        set { oType = (AmountType)setValue("Amount type", (int)oType, (int)value); }
    }

    public SalesSplit(int SaleID, int DBID, int CommissionTypeID, double Amount, AmountType Type, double CalculatedAmount, string CommissionName) {
        intSaleID = SaleID;
        intID = DBID;
        dAmount = Amount;
        oType = Type;
        dCalculatedAmount = CalculatedAmount;
        intCommissionTypeID = CommissionTypeID;
        szCommissionName = CommissionName;
        oCT = new SalesSplitChangeTracker(intSaleID, intID);
    }

    public void addUserSplit(UserSalesSplit UserSplitDetails) {
        lUserSplits.Add(UserSplitDetails);
    }

    public string writeToDB() {
        if (!oCT.AreChangesRecorded && intID > 0)
            return "";

        sqlUpdate oSQL = new sqlUpdate("SALESPLIT", "ID", intID);
        oSQL.add("SALEID", intSaleID);
        oSQL.add("COMMISSIONTYPEID", CommissionTypeID);
        oSQL.add("AMOUNT", dAmount);
        oSQL.add("AMOUNTTYPEID", (int)AmountType);
        oSQL.add("CALCULATEDAMOUNT", dCalculatedAmount);
        string szSQL = "";
        if (intID == -1) {
            szSQL += oSQL.createInsertSQL();
            oCT.addChange("Created");
        } else {
            szSQL += oSQL.createUpdateSQL();
        }
        oCT.writeToDB();
        return szSQL;
    }

    public void delete() {
        DB.runNonQuery(String.Format(@"
            DELETE FROM USERSALESPLIT WHERE SALESPLITID = {0};
            DELETE FROM SALESPLIT WHERE ID = {0}", intID));
    }
}

/// <summary>
/// Contains the details of a Expenses for a sale
/// </summary>
public class SalesExpense : AuditClass {
    private int intID = 0;
    private double dAmount = 0;
    private AmountType oType = 0;
    private double dCalculatedAmount = 0;
    private int intExpenseTypeID = -1;
    private int intSaleID = -1;

    public int ID {
        get { return intID; }
    }

    public int SaleID {
        get { return intSaleID; }
        set { intSaleID = value; }
    }

    public int ExpenseTypeID {
        get { return intExpenseTypeID; }
        set { intExpenseTypeID = setValue("Expense type", intExpenseTypeID, value); }
    }

    public double Amount {
        get { return dAmount; }
        set { dAmount = setValue("Amount", dAmount, value); }
    }

    public double CalculatedAmount {
        get { return dCalculatedAmount; }
        set { dCalculatedAmount = setValue("Calculated amount", dCalculatedAmount, value); }
    }

    public AmountType oAmountType {
        get {
            return oType;
        }
        set { oType = (AmountType)setValue("Amount type", (int)oType, (int)value); }
    }

    /// <summary>
    /// Creates a sales expense record
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="ExpenseTypeID"></param>
    /// <param name="Amount"></param>
    /// <param name="Type"></param>
    /// <param name="CalculatedAmount"></param>
    public SalesExpense(int ID, int SaleID, int ExpenseTypeID, double Amount, AmountType Type, double CalculatedAmount) {
        intID = ID;
        intSaleID = SaleID;
        intExpenseTypeID = ExpenseTypeID;
        dAmount = Amount;
        oType = Type;
        dCalculatedAmount = CalculatedAmount;
        oCT = new SalesExpenseChangeTracker(intSaleID, intID);
    }

    /// <summary>
    /// Updates the valus in place in the DB
    /// </summary>
    /// <returns></returns>
    public string updateToDB() {
        if (!oCT.AreChangesRecorded && intID > 0)
            return "";

        sqlUpdate oSQL = new sqlUpdate("SALEEXPENSE", "ID", intID);
        oSQL.add("SALEID", SaleID);
        oSQL.add("EXPENSETYPEID", ExpenseTypeID);
        oSQL.add("AMOUNT", dAmount);
        oSQL.add("AMOUNTTYPEID", (int)oAmountType);
        oSQL.add("CALCULATEDAMOUNT", dCalculatedAmount);
        string szSQL = "";
        if (intID == -1) {
            szSQL += oSQL.createInsertSQL();
            oCT.addChange("Created");
        } else
            szSQL += oSQL.createUpdateSQL();

        oCT.writeToDB();
        return szSQL;
    }

    public void delete() {
        DB.runNonQuery("DELETE FROM SALEEXPENSE WHERE ID = " + intID);
    }

    /// <summary>
    /// Gets the HTML for a row of sales expense
    /// </summary>
    /// <param name="blnAddButton"></param>
    /// <param name="SaleExpenseID"></param>
    /// <param name="oSE"></param>
    /// <returns></returns>
    public static string getHTML(bool blnAddButton, int SaleExpenseID, SalesExpense oSE = null) {
        string szIDTag = "";
        szIDTag = "SE_" + SaleExpenseID;
        int intID = -1;
        if (oSE != null)
            intID = oSE.intID;
        DropDownList ddlCategory = HTML.createAmountTypeListBox("lstCategory_" + szIDTag, "Entry JQSaleExpenseCategory", "");

        string szSQL = string.Format(@"
            SELECT ID, NAME
            FROM LIST WHERE LISTTYPEID = {0} AND ISACTIVE = {1}", (int)ListType.OffTheTop, (int)TrueFalse.True);
        DataSet dsSalesExpense = DB.runDataSet(szSQL);
        Utility.BindList(ref ddlCategory, DB.runDataSet(szSQL), "ID", "NAME");
        ddlCategory.Items.Insert(0, new ListItem("Select...", "-1"));

        if (oSE != null) {
            Utility.setListBoxItems(ref ddlCategory, oSE.ExpenseTypeID.ToString());
        }
        ddlCategory.Attributes.Add("onchange", "return checkExpenseCategory(this)");
        DropDownList ddlAmountType = HTML.createAmountTypeListBox("lstAmountType_" + szIDTag, "Entry JQSaleExpenseAmountType", "width: 45px");
        ddlAmountType.Attributes.Add("onchange", "amountTypeChange(this);");

        Utility.setListBoxItems(ref ddlAmountType, ((int)AmountType.Percent).ToString());
        string szValidationType = "percent";
        string szAmount = "0";
        string szCalcAmount = "0";
        if (oSE != null && oSE.ID > -1) {
            Utility.setListBoxItems(ref ddlAmountType, ((int)oSE.oAmountType).ToString());
            if (oSE.oAmountType == AmountType.Dollar)
                szValidationType = "numbersOnly";
            szAmount = oSE.Amount.ToString();
            szCalcAmount = oSE.CalculatedAmount.ToString("0.00");
        }

        string szHTMLAmount = string.Format(@"
           <input id='{0}' name='{0}' value='{1}' class='Entry JQSaleExpenseAmount {2}' style='width:80px;' onfocus='highlightTextOnFocus(this)' onblur='saleSplitAmountChange(this)'/>
            <input type='hidden' id='{3}' name='{3}' value='{4}' />
            "
          , "txtAmount_" + szIDTag, szAmount, szValidationType, "hdExpenseDBID_" + szIDTag, intID);
        string szHTMLCalcAmount = string.Format(@"
           <span id='{0}' class='JQExpenseSplit CalcTotal', style='width:80px; text-align:right'>{1}</span>"
            , "txtCalculatedAmount_" + szIDTag, szCalcAmount);
        string szHTMLButton = string.Format(@"
            <span id='spSaleExpenseDeleteButton' style='padding-left:10px; padding-top:3px'>
                <input name='btnDelete_{0}' id='btnDelete_{0}' title='Delete expense' style='' onclick='deleteSaleExpense(this, {1}); return false;'  type='image' src='../sys_images/delete.gif' border='0'/>
            </span>", szIDTag, intID);
        if (SaleExpenseID < 0) {
            return string.Format(@" {0} {1} {2} {3} {4} ^^***^^{5} ", Utility.ControlToString(ddlCategory), szHTMLAmount, Utility.ControlToString(ddlAmountType), szHTMLCalcAmount, szHTMLButton, szIDTag);
        }
        return string.Format(@"<li id='li_{5}'> {0} {1} {2} {3} {4} </li>", Utility.ControlToString(ddlCategory), szHTMLAmount, Utility.ControlToString(ddlAmountType), szHTMLCalcAmount, szHTMLButton, szIDTag);
    }
}

public class ListObject {
    protected int intID; //Commission Type
    protected string szName;
    protected string szAmount;
    protected AmountType oAType = AmountType.Dollar;

    public ListObject(int intID, string szName, string szAmount, AmountType oAType) {
        this.intID = intID;
        this.szName = szName;
        this.szAmount = szAmount;
        this.oAType = oAType;
    }

    public int ID {
        get { return intID; }
    }

    public string Name {
        get { return szName; }
    }

    public string Amount {
        get { return szAmount; }
    }

    public AmountType AmountType {
        get { return oAType; }
    }
}

public class ListTypes {
    private ListType oType;
    private List<ListObject> lListType = new List<ListObject>();

    public ListTypes(ListType oType) {
        this.oType = oType;
        load();
    }

    public List<ListObject> ListObjectList {
        get { return lListType; }
        set { lListType = value; }
    }

    public void load() {
        string szSQL = String.Format(@"
            SELECT ID, NAME, AMOUNT, AMOUNTTYPEID
            FROM LIST WHERE LISTTYPEID = {0}
            ORDER BY SEQUENCENO, NAME ", (int)oType);
        DataSet ds = DB.runDataSet(szSQL);
        foreach (DataRow oRow in ds.Tables[0].Rows) {
            lListType.Add(new ListObject(Convert.ToInt32(oRow["ID"]), oRow["NAME"].ToString(), oRow["AMOUNT"].ToString(), (AmountType)Convert.ToInt32(oRow["AMOUNTTYPEID"])));
        }
    }
}

/// <summary>
/// Contains the details of a single user split amount
/// </summary>
public class UserSalesSplit : AuditClass {
    private int intID = 0;
    private int intUserID = 0;
    private double dAmount = 0;
    private AmountType oType = AmountType.Percent;
    private double dCalculatedAmount = 0;
    private double dActualPayment = 0;
    private double dGraphCommission = Double.MinValue;
    private ArrayList arUserSplits = new ArrayList();
    private bool blnIncludeInKPI = false;
    private int intSaleSplitID = -1;
    private int intCommissionTypeID = -1;
    private string szPaymentNotes = "";
    private int intOfficeID = 0;

    public int ID {
        get { return intID; }
    }

    public int UserID {
        get { return intUserID; }
        set { intUserID = setValue("User ID", intUserID, value); }
    }

    public int OfficeID {
        get { return intOfficeID; }
        set { intOfficeID = setValue("Office ID", intOfficeID, value); }
    }

    public int SaleSplitID {
        get { return intSaleSplitID; }
    }

    /// <summary>
    /// Include this user in the KPI splits - only value when the commission type is Lead
    /// </summary>
    public bool IncludeInKPI {
        get { return blnIncludeInKPI; }
        set { blnIncludeInKPI = setValue("IncludeInKPI", blnIncludeInKPI, value); }
    }

    public double Amount {
        get { return dAmount; }
        set { dAmount = setValue("Amount", dAmount, value); }
    }

    public int CommissionTypeID {
        get { return intCommissionTypeID; }
        set { intCommissionTypeID = setValue("Commission type", intCommissionTypeID, value); }
    }

    //The amount directed to the agent for graph purposes
    public double CalculatedAmount {
        get { return dCalculatedAmount; }
        set { dCalculatedAmount = setValue("Calculated amount", dCalculatedAmount, value); }
    }

    /// <summary>
    /// The actual amount that will be paid based on the payment schedule of the agent
    /// </summary>
    public double ActualPayment {
        get { return dActualPayment; }
        set { dActualPayment = setValue("Actual payment", dActualPayment, value); }
    }

    public AmountType oAmountType {
        get { return oType; }
        set { oType = (AmountType)setValue("Amount type", (int)oType, (int)value); }
    }

    public UserSalesSplit(int SaleSplitID, int ID, int UserID, double Amount, AmountType Type, double CalculatedAmount, double ActualPayment, bool IncludeInKPI, int CommissionTypeID, int OfficeID, double GraphCommission = Double.MinValue) {
        intSaleSplitID = SaleSplitID;
        intID = ID;
        intUserID = UserID;
        dAmount = Amount;
        oType = Type;
        dCalculatedAmount = CalculatedAmount;
        dActualPayment = ActualPayment;
        blnIncludeInKPI = IncludeInKPI;
        intCommissionTypeID = CommissionTypeID;
        intOfficeID = OfficeID;
        if (GraphCommission != Double.MinValue)
            dGraphCommission = GraphCommission;
        oCT = new UserSalesSplitChangeTracker(intSaleSplitID, intID);
    }

    /// <summary>
    /// Creates the update SQL as well as calculates the actual payed for salaried agents
    /// </summary>
    /// <param name="SalePayPeriod"></param>
    /// <param name="dYTDCommission"></param>
    /// <returns></returns>
    public string updateToDB(int SalePayPeriod, double dYTDCommission, DateTime? SaleDate = null) {
        if (SaleDate != null)
            calculateActualPay(dYTDCommission, G.UserInfo.getUser(intUserID).getPaymentStructure((DateTime)SaleDate));
        else
            calculateActualPay(dYTDCommission);
        if (!oCT.AreChangesRecorded && intID > 0)
            return "";

        if (intUserID == -1) {
            if (intID > 0) {
                delete();
            }
            return "";
        }
        if (Amount == 0 || CalculatedAmount == 0) {
            delete();
            return "";
        }
        sqlUpdate oSQL = new sqlUpdate("USERSALESPLIT", "ID", intID);
        oSQL.add("SALESPLITID", intSaleSplitID);
        oSQL.add("USERID", intUserID);
        oSQL.add("AMOUNT", dAmount);
        oSQL.add("AMOUNTTYPEID", (int)oAmountType);
        oSQL.add("CALCULATEDAMOUNT", dCalculatedAmount);
        oSQL.add("ACTUALPAYMENT", dActualPayment);
        oSQL.add("OFFICEID", intOfficeID);
        if (dGraphCommission != Double.MinValue)
            oSQL.add("GRAPHCOMMISSION", dGraphCommission);

        if (szPaymentNotes != "")
            oSQL.add("PAYMENTNOTES", szPaymentNotes);

        string szIncludeInKPI = "0";
        if (blnIncludeInKPI)
            szIncludeInKPI = "1";
        oSQL.add("INCLUDEINKPI", szIncludeInKPI);
        string szSQL = "";
        if (intID == -1) {
            szSQL += oSQL.createInsertSQL();
            oCT.addChange("Created");
        } else {
            if (G.User.IsCurrentSaleExportedToMYOB) {
                oSQL.add("RECORDSTATUS", -1);
                //We want to capture a diff of this scenario, so we are maintaining the record in the DB
                szSQL += oSQL.createInsertSQL();
                delete();
            } else {
                szSQL += oSQL.createUpdateSQL();
            }
        }

        oCT.writeToDB();
        return szSQL;
    }

    /// <summary>
    /// Updates the commission with the payment if the user is on a salary
    /// </summary>
    /// <param name="dYTDCommission"></param>
    public void calculateActualPay(double dYTDCommission, PayBand EffectivePaymentStructure = PayBand.ToBeDetermined) {
        UserDetail oU = G.UserInfo.getUser(intUserID);
        if (oU == null)
            return;
        if (EffectivePaymentStructure == PayBand.ToBeDetermined)
            EffectivePaymentStructure = oU.PaymentStructure;

        DB.runNonQuery("-- Calculating actual pay - YTD: " + dYTDCommission + " for user " + G.UserInfo.getName(intUserID));
        if (dActualPayment != dCalculatedAmount) {
            ActualPayment = dCalculatedAmount;
        }
        if (oU == null || EffectivePaymentStructure == PayBand.Normal)
            return;
        if (EffectivePaymentStructure == PayBand.JuniorNoTeamRetainer || EffectivePaymentStructure == PayBand.JuniorTeamRetainer) {
            dActualPayment = dCalculatedAmount;
        } else if (EffectivePaymentStructure == PayBand.JuniorNoTeamSalary || EffectivePaymentStructure == PayBand.JuniorTeamSalary || EffectivePaymentStructure == PayBand.SpecialCase85kBase) {
            double dCommissionPercentage = 1;
            //Calculate the actual percentage paid based on their salary structure
            if (EffectivePaymentStructure == PayBand.JuniorTeamSalary || EffectivePaymentStructure == PayBand.JuniorNoTeamSalary) {
                if (dYTDCommission < 100000)
                    dCommissionPercentage = 0; //0 - 100000 no commission
                else if (dYTDCommission >= 100000 && dYTDCommission < 150000)
                    dCommissionPercentage = 0.35 / 0.45; //We want approx 77% of the calculated amount
                szPaymentNotes = "YTD: " + Utility.formatMoney(dYTDCommission) + " Comm: " + dCommissionPercentage.ToString("N2");
            } else if (EffectivePaymentStructure == PayBand.SpecialCase85kBase) {
                if (dYTDCommission < 165000)
                    dCommissionPercentage = 0; //0 - 165000 no commission
                else
                    dCommissionPercentage = 0.45;
            }

            switch ((CommissionType)CommissionTypeID) {
                case CommissionType.Lead:
                    ActualPayment = dCalculatedAmount * (dCommissionPercentage);
                    break;

                case CommissionType.List:
                    ActualPayment = dCalculatedAmount * (dCommissionPercentage);
                    break;

                case CommissionType.Manage:
                    ActualPayment = dCalculatedAmount * (dCommissionPercentage);
                    break;

                case CommissionType.Sell:
                    ActualPayment = dCalculatedAmount * (dCommissionPercentage);
                    break;

                case CommissionType.ServiceArea:
                    ActualPayment = 0;
                    break;

                case CommissionType.Mentoring:
                    ActualPayment = 0;
                    break;
            }
        }
    }

    private void delete() {
        if (G.User.IsCurrentSaleExportedToMYOB) {
            DB.runNonQuery("UPDATE USERSALESPLIT SET RECORDSTATUS = 1 WHERE ID = " + intID);
        } else {
            DB.runNonQuery("DELETE FROM USERSALESPLIT WHERE ID = " + intID);
        }
    }
}