using System;
using System.Data;

public class UserTX {
    private int intID = 0;                              //DB Account ID
    private double dAmount = 0;                         //Amount user is contributing
    private double dTotalAmount = 0;                    //Total amount entered
    private double dFletcherContribAmount = 0;          //% or dollar value fletchers is contributing
    private double dFletcherContribTotal = 0;           //Actual value fletchers is contributing
    private AmountType AmountType = AmountType.Percent;
    private int intAccount = 0;
    private int intMYOBExportID = -1;
    private int intUserID = 0;
    private int intCategoryID = -1;
    private DateTime dtTX = DateTime.Now;
    private bool blnShowExGST = false;
    private string szComment = "";
    private string szDebitJobCode = "";
    private string szCreditJobCode = "";
    private string szDebitGLCode = "";
    private string szCreditGLCode = "";
    private bool blnLockCodes = false;

    // Amount user is contributing - this is the value we use for MYOB export
    public double UserAmount {
        get { return dAmount; }
    }

    //Total amount of the overall tx
    public double TotalAmount {
        get { return dTotalAmount; }
    }

    //Value that fletchers is controbuting (% or $)
    public double FletcherContribAmount {
        get { return dFletcherContribAmount; }
    }

    //Actual value that fletchers is contributing
    public double FletcherContribTotal {
        get { return dFletcherContribTotal; }
    }

    public AmountType FletcherAmountType {
        get { return AmountType; }
    }

    public bool ShowExGST {
        get { return blnShowExGST; }
    }

    public bool OverrideGLCodes {
        get { return blnLockCodes; }
    }

    public DateTime Date {
        get { return dtTX; }
    }

    /// <summary>
    /// THe DB ID of this account
    /// </summary>
    public int ID {
        get { return intID; }
    }

    public int UserID {
        get { return intUserID; }
    }

    public int AccountID {
        get { return intAccount; }
    }

    public int CategoryID {
        get { return intCategoryID; }
    }

    public int MYOBExportID {
        get { return intMYOBExportID; }
    }

    public string Comment {
        get { return szComment; }
    }

    public string DebitGLCode {
        get { return szDebitGLCode; }
    }

    public string CreditGLCode {
        get { return szCreditGLCode; }
    }

    public string DebitJobCode {
        get { return szDebitJobCode; }
    }

    public string CreditJobCode {
        get { return szCreditJobCode; }
    }

    /// <summary>
    /// Loads an object for the given transaction
    /// </summary>
    /// <param name="TxID"></param>
    public UserTX(int TxID) {
        intID = TxID;
        string szSQL = String.Format("SELECT  * FROM USERTX where ID = {0}", intID);

        DataSet dsSale = DB.runDataSet(szSQL);
        foreach (DataRow dr in dsSale.Tables[0].Rows) {
            dAmount = Convert.ToDouble(dr["AMOUNT"]);
            dFletcherContribAmount = Convert.ToDouble(dr["FLETCHERAMOUNT"]);
            dTotalAmount = Convert.ToDouble(dr["TOTALAMOUNT"]);
            dFletcherContribTotal = Convert.ToDouble(dr["FLETCHERCONTRIBTOTAL"]);
            AmountType = (AmountType)Convert.ToInt32(dr["AMOUNTTYPEID"]);
            intAccount = Convert.ToInt32(dr["ACCOUNTID"]);
            intUserID = Convert.ToInt32(dr["USERID"]);
            intCategoryID = DB.readInt((dr["TXCATEGORYID"]));
            szComment = Convert.ToString(dr["COMMENT"]);
            szCreditGLCode = Convert.ToString(dr["CREDITGLCODE"]);
            szDebitGLCode = Convert.ToString(dr["DEBITGLCODE"]);
            szDebitJobCode = Convert.ToString(dr["DEBITJOBCODE"]);
            szCreditJobCode = Convert.ToString(dr["CREDITJOBCODE"]);
            dtTX = Convert.ToDateTime(dr["TXDATE"]);
            blnShowExGST = Convert.ToBoolean(dr["SHOWEXGST"]);
            blnLockCodes = Convert.ToBoolean(dr["OVERRIDEGLCODES"]);
            if (dr["MYOBEXPORTID"] != System.DBNull.Value)
                intMYOBExportID = Convert.ToInt32(dr["MYOBEXPORTID"]);
        }
    }

    /// <summary>
    /// Loads all the transactions for a current user
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public static DataSet loadUserTx(int UserID, DateTime dtCutOffDate) {
        string szSQL = String.Format(@"
            SELECT tx.*,
                L.NAME AS ACCOUNT
            FROM USERTX TX JOIN LIST L ON L.ID = TX.ACCOUNTID
            WHERE TX.USERID = {0} AND L.TXDATE >= '{1}' AND TX.ISDELETED = 0
            ORDER BY TX.TXDATE ", UserID, Utility.formatDate(dtCutOffDate));

        return DB.runDataSet(szSQL);
    }

    /// <summary>
    /// Loads all the transactions
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public static DataSet loadCurrentTXs(DateTime dtStartDate, DateTime dtEndDate) {
        string szSQL = String.Format(@"
            SELECT TX.*, L.NAME AS ACCOUNT,LASTNAME + ', ' +  FIRSTNAME as [USER], C.NAME AS CATEGORY
            FROM USERTX TX
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN DB_USER U ON U.ID = TX.USERID AND TX.ISDELETED = 0
            LEFT JOIN LIST C ON C.ID = TX.TXCATEGORYID
            WHERE TX.TXDATE BETWEEN '{0} 00:00:00' AND '{1} 23:59:59'
            ORDER BY [USER], TX.TXDATE DESC"
            , Utility.formatDate(dtStartDate), Utility.formatDate(dtEndDate));

        return DB.runDataSet(szSQL);
    }

    /// <summary>
    /// Loads User Tx for a given payperiod
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="PayPeriod"></param>
    /// <returns></returns>
    public static DataSet loadUserTx(int UserID, int PayPeriod, ListType oListType) {
        string szSQL = string.Format(@"select STARTDATE, ENDDATE from PAYPERIOD WHERE ID = {0}", PayPeriod);
        DataSet ds = DB.runDataSet(szSQL);
        string szTxDate = string.Format(@"
            AND UTX.TXDATE BETWEEN '{0} 00:00:00' AND '{1} 23:59:59'", Utility.formatDate(Convert.ToDateTime(ds.Tables[0].Rows[0]["STARTDATE"])), Utility.formatDate(Convert.ToDateTime(ds.Tables[0].Rows[0]["ENDDATE"])));

        szSQL = String.Format(@"
            SELECT L_TXTYPE.NAME, UTX.AMOUNT, UTX.COMMENT, L_CAT.NAME AS CATEGORY
            FROM USERTX UTX
            JOIN LIST L_TXTYPE ON UTX.ACCOUNTID = L_TXTYPE.ID AND LISTTYPEID = {0}
            AND UTX.USERID = {1} AND UTX.ISDELETED = 0
            {2}
			LEFT JOIN LIST L_CAT ON UTX.TXCATEGORYID = L_CAT.ID", (int)oListType, UserID, szTxDate);
        return DB.runDataSet(szSQL);
    }
}

/// <summary>
/// Contains the information on the commission statment so that this cannot be changed for commission statements except in the current pay period
/// </summary>
public class UserPayPeriod {
    public double HeldOver { get; set; }
    public double PrevHeldOver { get; set; }
    public double Income { get; set; }
    public double CommissionIncome { get; set; }
    public double OtherIncome { get; set; }
    public double Pending { get; set; }
    public double EOFYBonus { get; set; }

    /// <summary>
    /// The distribution
    /// </summary>
    public double TotalDistributionOfFunds { get; set; }

    public double AnnualSBSTotal { get; set; }
    public double GraphCommissionTotal { get; set; }
    public double RetainerAmount { get; set; }
    public double DeductionsAmount { get; set; }
    public double SuperAmount { get; set; }

    /// <summary>
    /// Includes the full value of the deduction
    /// </summary>
    public double TotalDeductionAmount {
        get { return DeductionsAmount; }
    }

    /// <summary>
    /// The current income for the month - includes the retainer as income
    /// </summary>
    public double MonthlyIncomeWithRetainer {
        get {
            return this.Income + this.OtherIncome - this.DeductionsAmount + (2*this.RetainerAmount); //Need to add the retainer back in
        }
    }

    /// <summary>
    /// The current income for the month - the retainer counts as a deduction in this calculation
    /// </summary>
    public double MonthlyIncomeWithoutRetainer {
        get {
            return this.Income + this.OtherIncome - this.DeductionsAmount ; 
        }
    }
    public int UserID { get; set; }
    public int PayPeriodID { get; set; }
    public int DBID { get; set; }
    public DataSet dsData = null;
    public DataSet dsFutureData = null;
    public DataSet dsOtherIncome = null;
    public DataSet dsDeductions = null;

    /// <summary>
    /// We use the retainer only when the agents' pay will be less than their retainer amount
    /// New rule for Ringwood - always apply the retainer
    /// </summary>
    public bool UseRetainer {
        get {
            return this.RetainerAmount > 0 && ( this.RetainerAmount > this.MonthlyIncomeWithoutRetainer || G.Settings.IsRingwood);
        }
    }
    /// <summary>
    /// This will be true if the commission statement has written values to the DB
    /// </summary>
    public bool CommissionStatementDataExists { get; set; }

    public UserPayPeriod(int UserID, int PayPeriodID) {
        this.UserID = UserID;
        this.PayPeriodID = PayPeriodID;

        loadData();
    }

    /// <summary>
    /// We need to calculate the totals when this is the current PP or when we don't have data in the UserPayPeriod table
    /// </summary>
    /// <returns></returns>
    public bool canUpdateTotals {
        get { return G.CurrentPayPeriod == PayPeriodID || !CommissionStatementDataExists; }
    }

    private void loadData() {
        dsData = Sale.loadCommissionStatementForPayPeriod(UserID, PayPeriodID);
        dsFutureData = Sale.loadFutureCommissionStatement(UserID, PayPeriodID);
        dsOtherIncome = UserTX.loadUserTx(UserID, PayPeriodID, ListType.Income);
        dsDeductions = UserTX.loadUserTx(UserID, PayPeriodID, ListType.Expense);

        string szSQL = String.Format(@"
            SELECT *
            FROM USERPAYPERIOD
            WHERE USERID = {0} AND PAYPERIODID = {1}", UserID, PayPeriodID);
        DBID = -1;
        CommissionStatementDataExists = false;
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                CommissionStatementDataExists = dr["INCOME"] != System.DBNull.Value;
                DBID = DB.readInt(dr["ID"]);
                if (CommissionStatementDataExists && !canUpdateTotals) {
                    HeldOver = DB.readDouble(dr["HELDOVERAMOUNT"], 0);
                    Income = DB.readDouble(dr["INCOME"], 0);
                    OtherIncome = DB.readDouble(dr["OTHERINCOME"], 0);
                    SuperAmount = DB.readDouble(dr["SUPERPAID"], 0);
                    Pending = DB.readDouble(dr["PENDING"], 0);
                    EOFYBonus = DB.readDouble(dr["EOFYBonus"], 0);
                    TotalDistributionOfFunds = DB.readDouble(dr["DistributionOfFunds"], 0);
                }
            }
        }

        //Get the heldover amount
        int intPrevPayperiodID = PayPeriodID - 1;
        if (intPrevPayperiodID == 14)
            intPrevPayperiodID = 13;
        szSQL = string.Format(@"
            SELECT HELDOVERAMOUNT
            FROM USERPAYPERIOD WHERE USERID = {0} AND PAYPERIODID = {1}", UserID, intPrevPayperiodID);
        PrevHeldOver = DB.getScalar(szSQL, 0.0);
        getRetainerAmount();
        checkPreviousHeldOver();
    }

    /// <summary>
    /// Processes all the calculations for the commission statement - this should only be done for the current pay period as previous ones are read from the DB
    /// </summary>
    /// <param name="dsData"></param>
    /// <param name="dsFutureData"></param>
    public void calculateTotals() {
        if (!canUpdateTotals) {
            addBonusCommToOtherIncome();
            return;
        }
        Income = 0;
        CommissionIncome = 0;
        GraphCommissionTotal = 0;
        //Get the total commission and graph commission for this pay period
        foreach (DataRow dr in dsData.Tables[0].Rows) {
            Income += DB.readDouble(dr["ACTUALPAYMENT"], 0);
            GraphCommissionTotal += DB.readDouble(dr["GRAPHTOTAL"], 0);
        }
        CommissionIncome = Income;

        //Annual SBS Total
        EOFYBonus += calcEOFYBonusScheme();
        addBonusCommToOtherIncome();

        //Sum up the other income amount
        foreach (DataRow dr in dsOtherIncome.Tables[0].Rows) {
            OtherIncome += DB.readDouble(dr["AMOUNT"], 0);
        }

        DeductionsAmount = 0;
        //Add the deductions
        foreach (DataRow dr in dsDeductions.Tables[0].Rows) {
            DeductionsAmount += DB.readDouble(dr["AMOUNT"], 0);
        }

        if (G.Settings.ClientID == ClientID.HeadOffice) {
            TotalDistributionOfFunds = CommissionIncome + OtherIncome - DeductionsAmount;
        } else {
            TotalDistributionOfFunds = CommissionIncome + OtherIncome + EOFYBonus - DeductionsAmount;
        }

        foreach (DataRow dr in dsFutureData.Tables[0].Rows) {
            Pending += DB.readDouble(dr["ACTUALPAYMENT"], 0);
        }
        if (TotalDistributionOfFunds < 0) {
            HeldOver = TotalDistributionOfFunds;
        } else {
            HeldOver = 0;
        }
        updateDB();
    }

    private void addBonusCommToOtherIncome() {
        //We need to load this as this is part of the data on the report
        if (G.Settings.ClientID == ClientID.Eltham) {
            EOFYBonus = 0; //Eltham totals are included in the other income area - we need to load up the sales that have bonus commission
            string szSQL = String.Format(@"
                SELECT SUM(ISNULL(USS.EOFYBONUSCOMMISSION, 0)) as BONUS_CALCULATED, S.ADDRESS
                FROM USERSALESPLIT USS
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1   AND USS.RECORDSTATUS < 1
                JOIN SALE S ON SS.SALEID = S.ID
                JOIN DB_USER U ON U.ID = USS.USERID
                LEFT JOIN COMMISSIONTIER CT ON USS.COMMISSIONTIERID = CT.ID
                JOIN PAYPERIOD P On P.ID = S.PAYPERIODID
                WHERE  S.STATUSID = 2 AND S.PAYPERIODID IN ({0}) AND USS.USERID = {1}	AND CT.PERCENTAGE > 0
                GROUP BY S.ID, S.SALEDATE, S.ADDRESS
                ORDER BY S.SALEDATE, S.ADDRESS", PayPeriodID, UserID);
            using (DataSet ds = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    DataRow drNew = dsOtherIncome.Tables[0].NewRow();
                    drNew["NAME"] = DB.readString(dr["ADDRESS"]);
                    drNew["COMMENT"] = "Commission bonus (+)";
                    drNew["AMOUNT"] = dr["BONUS_CALCULATED"];
                    dsOtherIncome.Tables[0].Rows.Add(drNew);
                }
            }
            dsOtherIncome.Tables[0].AcceptChanges();
        }
    }

    private void getRetainerAmount() {
        foreach (DataRow dr in dsDeductions.Tables[0].Rows) {
            if (DB.readString(dr["NAME"]).Contains("Retainer") )
                RetainerAmount = DB.readDouble(dr["AMOUNT"], 0);
        }
    }

    private void checkPreviousHeldOver() {
        if (PrevHeldOver != 0) {
            DataRow dr = dsDeductions.Tables[0].NewRow();
            dr["NAME"] = "Heldover from previous statement";
            double dDBAmount = PrevHeldOver;
            if (dDBAmount < 0)
                dDBAmount = dDBAmount * -1;
            dr["AMOUNT"] = dDBAmount;
            dsDeductions.Tables[0].Rows.Add(dr);
        }
    }

    /// <summary>
    /// Calculate and write to the DB the EOFYBonusScheme amount
    /// </summary>
    /// <param name="dYTDTotal"></param>
    /// <param name="dCurrPeriodTotal"></param>
    /// <returns></returns>
    public double calcEOFYBonusScheme() {
        double dYTDTotal = 0;
        foreach (DataRow dr in dsData.Tables[6].Rows) { //Should only be a single row
            dYTDTotal = DB.readDouble(dr["ELIGIBLE_COMMISSION"], 0);
        }

        double dBonus = UserDetail.calcEOFYBonusScheme(dYTDTotal, dsData.Tables[7], this.UserID);
        //We need to write this to the DB for the GL records
        string szSQL = String.Format(@"
            SELECT ID, EOFYBONUS
            FROM USERPAYPERIOD
            WHERE PAYPERIODID = {0} AND USERID = {1} ", PayPeriodID, UserID);
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                if (dBonus != DB.readDouble(dr["EOFYBONUS"])) {
                    //  this.EOFYBonus = dCommission;
                    DB.runNonQuery(String.Format("UPDATE USERPAYPERIOD SET EOFYBONUS = {1} WHERE ID = {0}", DB.readInt(dr["ID"]), dBonus));
                    DBLog.addGenericRecord(DBLogType.EOFYBonus, "Changed value from " + DB.readMoneyString(dr["EOFYBONUS"]) + " to " + Utility.formatMoney(dBonus), PayPeriodID, UserID);
                }
            }
        }
        return dBonus;
    }

    /// <summary>
    /// Calculate the agent bonus commission structure
    /// </summary>
    /// <param name="dYTDTotal"></param>
    /// <param name="dCurrPeriodTotal"></param>
    /// <returns></returns>
    public double calcMentorBonus(double dYTDTotal, double dCurrPeriodTotal, int JuniorUserID) {
        double dCommission = UserDetail.calcMentorBonus(dYTDTotal, dCurrPeriodTotal, JuniorUserID);
        //We need to write this to the DB for the GL records
        string szSQL = String.Format(@"
                SELECT ID
                FROM MENTORBONUS
                WHERE PAYPERIODID = {0} AND SALEID IS NULL  AND MENTORUSERID = {1} AND USERID = {2}", PayPeriodID, UserID, JuniorUserID);
        int intDBID = DB.getScalar(szSQL, -1);

        if (dCommission > 0) {
            sqlUpdate oSQL = new sqlUpdate("MENTORBONUS", "ID", intDBID);
            oSQL.add("MENTORUSERID", UserID);
            oSQL.add("PAYPERIODID", PayPeriodID);
            oSQL.add("USERID", UserID);
            oSQL.add("AGENTBONUS", dCommission);
            if (intDBID == -1)
                DB.runNonQuery(oSQL.createInsertSQL());
            else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } else if (dCommission == 0 && intDBID > 0) {
            DB.runNonQuery("UPDATE MENTORBONUS SET AGENTBONUS = 0 WHERE ID = " + intDBID);
        }
        return dCommission;
    }

    public void updateDB() {
        sqlUpdate oSQL = new sqlUpdate("USERPAYPERIOD", "ID", DBID);

        oSQL.add("USERID", UserID);
        oSQL.add("PAYPERIODID", PayPeriodID);
        oSQL.add("HELDOVERAMOUNT", HeldOver);
        oSQL.add("INCOME", Income);
        oSQL.add("OTHERINCOME", OtherIncome);
        oSQL.add("PENDING", Pending);
        oSQL.add("SUPERPAID", SuperAmount);
        oSQL.add("EOFYBonus", EOFYBonus);
        oSQL.add("DistributionOfFunds", TotalDistributionOfFunds);
        if (DBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else {
            //Create a history record
            string szSQL = oSQL.createInsertSQL();
            szSQL = szSQL.Replace("USERPAYPERIOD", "USERPAYPERIODHISTORY");
            DB.runNonQuery(szSQL + "; UPDATE USERPAYPERIODHISTORY SET UPDATEUSERID = " + G.User.UserID);
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }
}