using System;
using System.Collections.Generic;
using System.Data;

namespace Paymaker {

    /// <summary>
    /// Summary description for about.
    /// </summary>
    public partial class import : Root {
        protected System.Web.UI.WebControls.Button Button2;

        protected void Page_Load(object sender, System.EventArgs e) {
            bool blnRun = Valid.getBoolean("blnRunAuto", false);
            if (blnRun) {
                importAll();
                processSales();
            }
        }

        protected void btnOffice_Click(object sender, EventArgs e) {
            iOffice.importLatest();
        }

        protected void btnTestPing_Click(object sender, EventArgs e) {
            TestPing.importLatest();
        }

        protected void btnConsultants_Click(object sender, EventArgs e) {
            Team.importLatest();
            iConsultant.importLatest();
        }

        protected void btnPropertyType_Click(object sender, EventArgs e) {
            iPropertyType.importLatest();
        }

        protected void btnExpense_Click(object sender, EventArgs e) {
            iSalesListingExpenses.importLatest();
        }

        protected void btnContactActivityType_Click(object sender, EventArgs e) {
            iContactActivityType.importLatest();
        }

        protected void btnContactActivity_Click(object sender, EventArgs e) {
            iContactActivity.importLatest();
        }

        protected void btnPropertyCategory_Click(object sender, EventArgs e) {
            iPropertyCategory.importLatest();
        }

        protected void btnProperty_Click(object sender, EventArgs e) {
            iProperty.importLatest();
        }

        protected void btnContactCategory_Click(object sender, EventArgs e) {
            iContactCategory.importLatest();
        }

        protected void btnContactCategoryType_Click(object sender, EventArgs e) {
            iContactCategoryType.importLatest();
        }

        protected void btnSalesListing_Click(object sender, EventArgs e) {
            SalesListing.importLatest();
        }

        protected void btnSalesVouchers_Click(object sender, EventArgs e) {
            iSalesVouchers.importLatest();
        }

        protected void btnDeductions_Click(object sender, EventArgs e) {
            iSalesVoucherDeductions.importLatest();
        }

        protected void btnCommission_Click(object sender, EventArgs e) {
            iSalesVouchersCommissions.importLatest();
        }

        protected void btnContacts_Click(object sender, EventArgs e) {
            iContact.importLatest();
        }

        protected void btnTasks_Click(object sender, EventArgs e) {
            iTask.importLatest();
        }

        protected void btnListingSource_Click(object sender, EventArgs e) {
            iListingSource.importLatest();
        }

        protected void btnTest_Click(object sender, EventArgs e) {
            importAll();
        }

        private void importAll() {
            BlimpsHelper.runFullImport();
        }

        protected void btnProcess_Click(object sender, EventArgs e) {
            processSales();
        }

        private void processSales() {
            APILog.addLog(APISource.BoxDice, "Processing sale data");
            DB.runNonQuery("UPDATE COMMISSION SET EFFECTIVEUSERID = (SELECT REPORTUSERID FROM DB_USER WHERE ID = COMMISSION.CONSULTANTID)", DB.BoxDiceDBConn);

            string szSQL = @"
                SELECT SV.ID AS SALESVOUCHERID, U.OFFICEID, U.ISOFFICE, C.ID AS COMMISSIONID, C.CONSULTANTID, C.EFFECTIVEUSERID, SL.ID AS SALESLISTINGID,
                    ISCOMPANY, C.ROLEID, INTRODUCEDAMOUNT, INTRODUCEDPERCENT, PROPERTYAMOUNT, PROPERTYPERCENT, SPLITAMOUNT, SPLITPERCENT, U.INITIALS, LS.NAME AS LISTINGSOURCE
                FROM SALESLISTING SL
                JOIN SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
                JOIN COMMISSION C ON C.SALESVOUCHERID = SV.ID
                JOIN DB_USER U ON C.EFFECTIVEUSERID = U.ID
                JOIN LISTINGSOURCE LS ON LS.ID = SL.LISTINGSOURCEID
                JOIN PROPERTY P ON P.ID = SL.PROPERTYID
                ORDER BY SV.ID";
            Sale oS = null;
            DataTable dtSale = null;
            using (DataSet ds = DB.runDataSet(szSQL, DB.BoxDiceDBConn)) {
                int intCurrSalesVoucherID = 0;
                dtSale = ds.Tables[0].Clone();
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    if (intCurrSalesVoucherID != Convert.ToInt32(dr["SALESVOUCHERID"])) {
                        if (intCurrSalesVoucherID > 0)
                            oS = new Sale(intCurrSalesVoucherID, ref dtSale);
                        intCurrSalesVoucherID = Convert.ToInt32(dr["SALESVOUCHERID"]);
                        dtSale.Clear();
                    }
                    dtSale.ImportRow(dr);
                }
                if (dtSale.Rows.Count > 0)
                    oS = new Sale(intCurrSalesVoucherID, ref dtSale);
            }

            //Update the deduction totals
            DB.runNonQuery("UPDATE SALESVOUCHER SET INTERNALCONJUNCTIONAL = ISNULL((SELECT SUM(AMOUNT) FROM SALESVOUCHERDEDUCTION SVD WHERE SVD.SALESVOUCHERID = SALESVOUCHER.ID AND REASON LIKE 'Internal Conj%'), 0)", DB.BoxDiceDBConn);
            DB.runNonQuery("UPDATE SALESVOUCHER SET EXTERNALCONJUNCTIONAL = ISNULL((SELECT SUM(AMOUNT) FROM SALESVOUCHERDEDUCTION SVD WHERE SVD.SALESVOUCHERID = SALESVOUCHER.ID AND (REASON = 'External Conjunctional' or OR REASON = 'Referral/Conjunctional')), 0)", DB.BoxDiceDBConn);
        }
    }

    /// <summary>
    /// Represents a single closed sale
    ///
    /// Used to split the sale into reportable data
    /// </summary>
    public class Sale {
        private List<Commission> lComm = new List<Commission>();
        private int intSalesVoucherID = 0;
        private string szVendor = "";
        private string szPurchaser = "";
        private int intSalesListingID = 0;
        private int intDistinctOfficeCount = 0;
        private List<int> lOfficeID = new List<int>();
        private string szListingSource = "";

        public Sale(int SalesVoucherID, ref DataTable dt) {
            intSalesVoucherID = SalesVoucherID;
            intSalesListingID = Convert.ToInt32(dt.Rows[0]["SALESLISTINGID"]);
            processSale(ref dt);
        }

        private void processSale(ref DataTable dt) {
            foreach (DataRow dr in dt.Rows) {
                szListingSource = DB.readString(dr["LISTINGSOURCE"]);
                lComm.Add(new Commission(Convert.ToInt32(dr["COMMISSIONID"]), Convert.ToInt32(dr["CONSULTANTID"]), Convert.ToInt32(dr["OFFICEID"]),
                        Convert.ToBoolean(dr["ISOFFICE"]), Convert.ToInt32(dr["EFFECTIVEUSERID"]), Convert.ToBoolean(dr["ISCOMPANY"]), Convert.ToString(dr["INITIALS"]),
                        (RoleType)Convert.ToInt32(dr["ROLEID"]), Convert.ToDouble(dr["INTRODUCEDAMOUNT"]), Convert.ToDouble(dr["INTRODUCEDPERCENT"]),
                        Convert.ToDouble(dr["PROPERTYAMOUNT"]), Convert.ToDouble(dr["PROPERTYPERCENT"]), Convert.ToDouble(dr["SPLITAMOUNT"]), Convert.ToDouble(dr["SPLITPERCENT"])));
            }

            processVendor();
        }

        private string getListInitials(RoleType oType) {
            Commission oC = lComm.Find(s => s.Type == oType);
            if (oC == null)
                return "";
            else
                return oC.InitialsShared;
        }

        private void processVendor() {
            string szSQL = String.Format(@"
                SELECT ISNULL(JOBTITLE, '') AS JOBTITLE, LASTNAME, FIRSTNAME, CONTACTACTIVITYTYPEID
                FROM CONTACT C JOIN CONTACTACTIVITY CA ON CA.CONTACTID = C.ID WHERE CA.SALESLISTINGID = {0}", intSalesListingID);
            using (DataSet ds = DB.runDataSet(szSQL, DB.BoxDiceDBConn)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    string szName = dr["JOBTITLE"].ToString();
                    if (String.IsNullOrEmpty(szName))
                        szName = dr["FIRSTNAME"].ToString() + " " + dr["LASTNAME"].ToString();
                    int intTypeID = Convert.ToInt32(dr["CONTACTACTIVITYTYPEID"]);
                    if (intTypeID == 7 || intTypeID == 9) {
                        if (!szPurchaser.Contains(szName)) {
                            Utility.Append(ref szPurchaser, szName, ", ");
                        }
                    } else {
                        if (!szVendor.Contains(szName)) {
                            Utility.Append(ref szVendor, szName, ", ");
                        }
                    }
                }
                szSQL = String.Format("UPDATE SALESLISTING SET VENDOR = '{0}', PURCHASER = '{1}' WHERE ID = {2}", DB.escape(szVendor), DB.escape(szPurchaser), intSalesListingID);
                DB.runNonQuery(szSQL, DB.BoxDiceDBConn);
            }
        }

        /// <summary>
        /// Searches for B&D records to match to CAPS record.  Only to be used for transition
        /// </summary>
        public static void findBnDSaleRecords() {
            DB.runNonQuery(String.Format(@"
                    UPDATE {0}.DBO.SALE SET BNDSALEID =
		                    ( SELECT MAX(SL.ID) FROM PROPERTY P JOIN SALESLISTING SL ON SL.PROPERTYID = P.ID
			                    JOIN SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
			                    WHERE CAST(P.ADDRESS + ', Vic, ' + P.POSTCODE COLLATE SQL_LATIN1_GENERAL_CP1_CI_AS AS VARCHAR(MAX)) = CAST({0}.DBO.SALE.ADDRESS COLLATE SQL_LATIN1_GENERAL_CP1_CI_AS AS VARCHAR(MAX))
			                    AND SV.SOLDDATE = {0}.DBO.SALE.SALEDATE
			                    AND SV.SOLDDATE IS NOT NULL)
                       WHERE BNDSALEID IS NULL", Client.DBName), DB.BoxDiceDBConn);
        }
    }

    /// <summary>
    /// The role of the sales agent in the commission split
    /// </summary>
    public enum RoleType {
        List = 1,
        Manage = 2,
        Sell = 3
    }

    public class Commission {
        private int intConsultantID = 0;
        private int intDBID = 0;
        private int intOfficeID = 0;
        private string szInitials = "", szSharedInitials = "";
        private RoleType oRoleTypeID = RoleType.List;
        private double dIntroducedAmount = 0, dIntroducedPercent = 0, dPropertyAmount = 0, dPropertyPercent = 0, dSplitAmount = 0, dSplitPercent = 0;
        private int intNumberShared = 0;
        private double dCalculatedCount = 0;
        private bool blnIsOffice = false;
        private bool blnIsCompanyListing = false;
        private int intEffectiveUserID = 0;
        private bool blnUsedInCalculation = false;

        /// <summary>
        /// Number of commission splits of this type
        /// </summary>
        public int NumberShared {
            get { return intNumberShared; }
            set { intNumberShared = value; }
        }

        /// <summary>
        /// Database ID
        /// </summary>
        public int DBID {
            get { return intDBID; }
        }

        /// <summary>
        /// Office ID
        /// </summary>
        public int OfficeID {
            get { return intOfficeID; }
        }

        /// <summary>
        /// Does this commission have an introduced amount > 0?
        /// </summary>
        public bool HasValidIntroducedAmount {
            get { return dIntroducedAmount > 0; }
        }

        /// <summary>
        /// Should this user be treated as a company office
        /// </summary>
        public bool IsOffice {
            get { return blnIsOffice; }
        }

        /// <summary>
        /// Should this sale be treated as a company listing
        /// </summary>
        public bool IsCompanyListing {
            get { return blnIsCompanyListing; }
            set { blnIsCompanyListing = value; }
        }

        /// <summary>
        /// Used when calculating whether we have already factored this one into the sales calculations
        /// </summary>
        public bool UsedInCalculation {
            get { return blnUsedInCalculation; }
            set { blnUsedInCalculation = value; }
        }

        /// <summary>
        /// Is this person a sales assistant
        /// </summary>
        public int EffectiveUserID {
            get { return intEffectiveUserID; }
            set { intEffectiveUserID = value; }
        }

        /// <summary>
        /// The effective count for this person after applying all the rules
        /// </summary>
        public double CalculatedCount {
            get { return dCalculatedCount; }
            set { dCalculatedCount = value; }
        }

        /// <summary>
        /// The consultant user ID
        /// </summary>
        public int AgentID {
            get { return intConsultantID; }
        }

        /// <summary>
        /// The type of role the agent has (List, Manage, Sell)
        /// </summary>
        public RoleType Type {
            get { return oRoleTypeID; }
        }

        public string Initials {
            get {
                if (intConsultantID == intEffectiveUserID)
                    return szInitials;
                else
                    return G.UserInfo.getInitials(intConsultantID);
            }
        }

        public string InitialsShared {
            get { return szSharedInitials; }
            set { szSharedInitials = value; }
        }

        public Commission(int DBID, int ConsultantID, int OfficeID, bool IsOffice, int EffectiveUserID, bool IsCompany, string Initials, RoleType RoleID, double IntroducedAmount, double IntroducedPercent, double PropertyAmount, double PropertyPercent, double SplitAmount, double SplitPercent) {
            intDBID = DBID;
            intConsultantID = ConsultantID;
            intOfficeID = OfficeID;
            blnIsOffice = IsOffice;
            blnIsCompanyListing = IsCompany;
            intEffectiveUserID = EffectiveUserID;
            szInitials = Initials;
            oRoleTypeID = (RoleType)RoleID;
            dIntroducedAmount = IntroducedAmount;
            dIntroducedPercent = IntroducedPercent;
            dPropertyAmount = PropertyAmount;
            dPropertyPercent = PropertyPercent;
            dSplitAmount = SplitAmount;
            dSplitPercent = SplitPercent;
            dCalculatedCount = 1;
        }
    }
}