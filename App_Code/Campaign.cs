using ServiceReference1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for Campaign
/// </summary>
public class Campaign {
    private DateTime dtStartDate = DateTime.MinValue;
    private DateTime dtLastImportedDate = DateTime.MinValue;
    private int intDBID = 0;
    private string szPropertyRef = "";
    private string szAddress1 = "";
    private string szAddress2 = "";
    private string szAgent = "";
    private string szOffice = "";
    private string szNotes = "";
    private string szOfficeID = "";
    private string szCampaignTrackID = "";
    private int intPostCode = 0;
    private int intAgentID = -1;
    private int intActionID = -1;
    private int intFocusAgentID = -1;
    private int intDistributionAgentID = -1;
    private bool blnCampaignProductsLoaded = false;
    private bool blnCampaignContributionsLoaded = false;
    private bool blnCampaignInvoicesLoaded = false;
    private bool blnCampaignActionsLoaded = false;
    private CampaignProductList lProducts = new CampaignProductList();
    private CampaignContributionList lContributions = new CampaignContributionList();
    private CampaignInvoiceList lInvoices = new CampaignInvoiceList();
    private CampaignActionList lActions = new CampaignActionList();
    private CampaignStatus oStatus = CampaignStatus.New;
    private double dTotalBudget = 0;
    private double dTotalCost = 0;
    private double dTotalActual = 0;
    private double dTotalVendor = 0;
    private double dTotalPaid = 0;
    private double dTotalInvoiced = 0;
    private double dTotalAgentCompany = 0;

    private bool blnIsReconciled = false;
    private bool blnCampaignDateUnderUserControl = false;
    private sqlUpdate oSQL = null;

    public Campaign(CampaignTrackProperty oProperty, string OfficeKey) {
        szPropertyRef = oProperty.PropertyRef;
        loadCampaign(oProperty, OfficeKey);
    }

    public Campaign(int CampaignID) {
        intDBID = CampaignID;
        oSQL = new sqlUpdate("CAMPAIGN", "ID", intDBID);
        loadCampaignFromDB();
    }

    public string Address {
        get { return szAddress1 + " " + szAddress2 + " " + intPostCode; }
    }

    /// <summary>
    /// The property reference as supplied by the external 3rd party (in this case Box & Dice
    /// </summary>
    public string PropertyRef {
        get { return szPropertyRef; }
    }

    public string CampaignTrackID {
        get { return szCampaignTrackID; }
    }

    public string Agent {
        get { return szAgent; }
    }

    public int AgentID {
        get { return intAgentID; }
    }

    public int FocusAgentID {
        get { return intFocusAgentID; }
        set {
            // if current value is same as new value or both current value and new value are less than zero (usually MinValue or -1)
            if (intFocusAgentID != value && (intFocusAgentID > 0 || value > 0)) {
                DBLog.addGenericRecord(DBLogType.CampaignAction, String.Format("FocusAgentID {0} <-> {1}", intFocusAgentID, value.ToString()), intDBID, intFocusAgentID);
                intFocusAgentID = value;
                if (value < 0)
                    oSQL.addNull("FOCUSAGENTID");
                else
                    oSQL.add("FOCUSAGENTID", intFocusAgentID);
            }
        }
    }

    public int DistributionAgentID {
        get { return intDistributionAgentID; }
        set {
            // if current value is same as new value or both current value and new value are less than zero (usually MinValue or -1)
            if (intDistributionAgentID != value && (intDistributionAgentID > 0 || value > 0)) {
                DBLog.addGenericRecord(DBLogType.CampaignAction, String.Format("DistributionAgentID {0} <-> {1}", intDistributionAgentID, value.ToString()), intDBID, intDistributionAgentID);
                intDistributionAgentID = value;
                if (value < 0)
                    oSQL.addNull("DISTRIBUTIONAGENTID");
                else
                    oSQL.add("DISTRIBUTIONAGENTID", intDistributionAgentID);
            }
        }
    }

    /// <summary>
    /// The currently active action ID for the campaign
    /// </summary>
    public int ActionID {
        get { return intActionID; }
        set {
            if (intActionID != value) {
                DBLog.addGenericRecord(DBLogType.CampaignAction, String.Format("ActionID {0} <-> {1}", intActionID, value.ToString()), intDBID, intActionID);
                intActionID = value;
                oSQL.add("ACTIONID", intActionID);
            }
        }
    }

    public int DBID {
        get { return intDBID; }
    }

    /// <summary>
    /// The office that the agent owning this campaign belongs to
    /// </summary>
    public string Office {
        get { return szOffice; }
    }

    public string Notes {
        get { return szNotes; }
        set {
            if (szNotes != value) {
                DBLog.addGenericRecord(DBLogType.CampaignData, String.Format("Notes {0} <-> {1}", szNotes, value.ToString()), intDBID);
                szNotes = value;
                oSQL.add("NOTE", szNotes);
            }
        }
    }

    /// <summary>
    /// The office ID from CampaignTrack - used to determine which web service key to use when refreshing a property
    /// </summary>
    public string OfficeID {
        get { return szOfficeID; }
    }

    public DateTime CampaignDate {
        get { return dtStartDate; }
    }

    public DateTime LastImportedDate {
        get { return dtLastImportedDate; }
    }

    public CampaignStatus Status {
        get { return oStatus; }
        set {
            if (oStatus != value) {
                DBLog.addGenericRecord(DBLogType.CampaignData, String.Format("Status {0} <-> {1}", oStatus.ToString(), value.ToString()), intDBID);

                oStatus = value;
                oSQL.add("STATUSID", (int)oStatus);
            }
        }
    }

    public CampaignProductList ProductList {
        get {
            loadCampaignProducts();
            return lProducts;
        }
    }

    public CampaignInvoiceList InvoiceList {
        get {
            loadCampaignInvoices();
            return lInvoices;
        }
    }

    public CampaignActionList ActionList {
        get {
            loadCampaignActions();
            return lActions;
        }
    }

    public double TotalBudget {
        get {
            loadCampaignContributions();
            return dTotalBudget;
        }
    }

    /// <summary>
    /// The total that has been paid, which is the AGENT amount and any contributions marked as received
    /// </summary>
    public double TotalPaid {
        get {
            loadCampaignContributions();
            return dTotalPaid;
        }
    }

    /// <summary>
    /// The total that has been paid by the agent or company. These will always be considered to have been paid
    /// </summary>
    public double TotalAgentCompany {
        get {
            loadCampaignContributions();
            return dTotalAgentCompany;
        }
    }

    /// <summary>
    /// THe sum of the product list
    /// </summary>
    private double TotalCost {
        get {
            loadCampaignProducts();
            return dTotalCost;
        }
    }

    /// <summary>
    /// THe sum of the actual product list
    /// </summary>
    private double TotalActual {
        get {
            loadCampaignProducts();
            return dTotalActual;
        }
    }

    /// <summary>
    /// Has the campaign start date been changed by the user - until the user modifies it, the system is free to update it
    /// </summary>
    private bool CampaignDateUnderUserControl {
        get {
            return blnCampaignDateUnderUserControl;
        }
        set { blnCampaignDateUnderUserControl = value; }
    }

    /// <summary>
    /// Determines the actual amount spent depending on whether or not the campaign has been reconciled
    /// </summary>
    public double ActualSpent {
        get {
            loadCampaignProducts();
            if (blnIsReconciled)
                return dTotalVendor;
            else
                return dTotalActual;
        }
    }

    /// <summary>
    /// THe sum of the actual product list
    /// </summary>
    public double TotalVendor {
        get {
            loadCampaignProducts();
            return dTotalVendor;
        }
    }

    /// <summary>
    /// The sum of the invoices entered against this campaign
    /// </summary>
    public double TotalInvoices {
        get {
            loadCampaignInvoices();
            return dTotalInvoiced;
        }
    }

    public double BudgetRemaining {
        get {
            loadCampaignProducts();
            return dTotalBudget + TotalAgentCompany - ActualSpent;
        }
    }

    public double AmountOwing {
        get {
            loadCampaignProducts();
            return ActualSpent - TotalPaid;
        }
    }

    public CampaignContributionList ContributionList {
        get {
            loadCampaignContributions();
            return lContributions;
        }
    }

    /// <summary>
    /// Updates any unwritten changes to the DB
    /// </summary>
    public void updateCampaign() {
        if (oSQL != null && oSQL.HasUpdates)
            DB.runNonQuery(oSQL.createUpdateSQL());
    }

    private void loadCampaignFromDB() {
        string szLoadSQL = String.Format(@"
            SELECT C.* , ISNULL(U.FIRSTNAME + ' ' + U.LASTNAME, 'Unknown') AS AGENT, ISNULL(L.NAME, '') AS OFFICE, ISNULL(AGENTID, -1) AS SAFEAGENTID
            FROM CAMPAIGN C
            LEFT JOIN DB_USER U ON U.ID = C.AGENTID
            LEFT JOIN LIST L ON U.OFFICEID = L.ID
            WHERE C.ID = {0}", intDBID);
        DataSet ds = DB.runDataSet(szLoadSQL);
        if (ds.Tables[0].Rows.Count == 0)
            return;

        DataRow dr = ds.Tables[0].Rows[0];
        intAgentID = Convert.ToInt32(dr["SAFEAGENTID"]);
        intFocusAgentID = DB.readInt(dr["FOCUSAGENTID"]);
        intDistributionAgentID = DB.readInt(dr["DISTRIBUTIONAGENTID"]);
        intPostCode = Convert.ToInt32(dr["POSTCODE"]);
        szAddress1 = Convert.ToString(dr["ADDRESS1"]);
        szAddress2 = Convert.ToString(dr["ADDRESS2"]);
        szAgent = Convert.ToString(dr["AGENT"]);
        szOffice = Convert.ToString(dr["OFFICE"]);
        szNotes = Convert.ToString(dr["NOTE"]);
        szOfficeID = Convert.ToString(dr["OFFICEID"]);
        if (System.DBNull.Value != dr["STARTDATE"])
            dtStartDate = Convert.ToDateTime(dr["STARTDATE"]);
        dtLastImportedDate = Convert.ToDateTime(dr["LASTIMPORTEDDATE"]);
        szPropertyRef = Convert.ToString(dr["CAMPAIGNNUMBER"]);
        szCampaignTrackID = Convert.ToString(dr["ORIGCAMPAIGNNUMBER"]);
        blnCampaignDateUnderUserControl = DB.readBool(dr["STARTDATEMODIFIED"]);
        oStatus = (CampaignStatus)Convert.ToInt32(dr["STATUSID"]);
        intDBID = Convert.ToInt32(dr["ID"]);
    }

    private void loadCampaign(CampaignTrackProperty oProperty, string OfficeKey) {
        string szLoadSQL = "SELECT * FROM CAMPAIGN WHERE CAMPAIGNNUMBER = " + szPropertyRef;
        DataSet ds = DB.runDataSet(szLoadSQL);
        bool blnCheckDifferences = false;
        if (ds.Tables[0].Rows.Count == 0) {
            //We need to import the data to our DB
            sqlUpdate oSQL = new sqlUpdate("CAMPAIGN", "ID", -1);

            oSQL.add("CAMPAIGNNUMBER", oProperty.PropertyRef);
            oSQL.add("ORIGCAMPAIGNNUMBER", getCampaignNumberFromPropertyName(oProperty.PropertyName));
            oSQL.add("ADDRESS1", oProperty.StreetAddress + " " + oProperty.Street);
            oSQL.add("ADDRESS2", oProperty.Suburb);
            oSQL.add("POSTCODE", oProperty.Postcode);
            oSQL.add("VENDORSURNAME", oProperty.VendorLastName);
            oSQL.add("STARTDATE", Utility.formatDate(DateTime.Now));
            oSQL.add("AGENTUSERREF", oProperty.AgentUserRef);
            oSQL.add("OFFICEID", Convert.ToInt32(OfficeKey));
            oSQL.add("NOTE", szNotes);
            oSQL.add("STARTDATEMODIFIED", blnCampaignDateUnderUserControl);
            oSQL.add("MYOBCARDID", getMYOBCardID(oProperty.StreetAddress, oProperty.Street));
            intAgentID = DB.getScalar("SELECT ID FROM DB_USER WHERE INITIALSCODE = '" + DB.escape(oProperty.AgentUserRef) + "'", -1);
            if (intAgentID > -1)
                oSQL.add("AGENTID", intAgentID);
            oSQL.add("LASTIMPORTEDDATE", Utility.formatDate(DateTime.Now));
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM CAMPAIGN", -1);
            ds = DB.runDataSet(szLoadSQL);
        } else {
            //Check to see if the details are still the same.
            intDBID = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"]);
            blnCheckDifferences = true;
        }
        loadCampaignFromDB();
        if (blnCheckDifferences)
            checkForUpdates(oProperty);
    }

    private string getMYOBCardID(string StreetAddress, string Street) {
        Street = Street.Trim();
        if (Street.Length >= 4)
            Street = Street.Substring(0, 4);
        string szFinal = "Fees-" + Street + StreetAddress.Trim();
        if (szFinal.Length > 14) {
            szFinal = szFinal.Substring(0, 14);
        }
        int intCount = DB.getScalar(String.Format("SELECT COUNT(id) from campaign where MYOBCARDID = '{0}'", DB.escape(szFinal)), 0);
        if (intCount > 0)
            szFinal = szFinal + intCount;
        return szFinal;
    }

    public void deleteCampaign() {
        string szSQL = String.Format(@"
               UPDATE CAMPAIGN SET ISDELETED = 1 WHERE ID = {0};", intDBID);
        DB.runNonQuery(szSQL);
    }

    protected string getCampaignNumberFromPropertyName(string Name) {
        if (String.IsNullOrEmpty(Name) || !Name.Contains("["))
            return "";
        Name = Name.Substring(Name.LastIndexOf('[') + 1, 7);
        return Name;
    }

    public void setCampaignDate(DateTime dt) {
        if (CampaignDate != dt) {
            DBLog.addGenericRecord(DBLogType.CampaignAction, String.Format("Campaign start date {0} <-> {1}", Utility.formatDate(CampaignDate), Utility.formatDate(dt)), intDBID);
            blnCampaignDateUnderUserControl = true;
            dtStartDate = dt;
            oSQL.add("STARTDATE", Utility.formatDate(dt));
            oSQL.add("STARTDATEMODIFIED", 1);
        }
    }

    /// <summary>
    /// Check across the fields that we use for differences.
    /// </summary>
    /// <param name="oProperty"></param>
    private void checkForUpdates(CampaignTrackProperty oProperty) {
        //Update the address
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGN", "ID", intDBID);
        bool blnNeedToUpdate = false;
        string szCTID = getCampaignNumberFromPropertyName(oProperty.PropertyName);

        if (szCampaignTrackID != szCTID) {
            oSQL.add("ORIGCAMPAIGNNUMBER", szCTID);
        }

        if (szAddress1 != oProperty.StreetAddress + " " + oProperty.Street) {
            oSQL.add("ADDRESS1", oProperty.StreetAddress + " " + oProperty.Street);
            blnNeedToUpdate = true;
        }
        if (szAddress2 != oProperty.Suburb) {
            oSQL.add("ADDRESS2", oProperty.Suburb);
            blnNeedToUpdate = true;
            szAddress2 = oProperty.Suburb;
        }
        int intOrigAgentID = intAgentID;
        intAgentID = DB.getScalar("SELECT ID FROM DB_USER WHERE INITIALSCODE = '" + DB.escape(oProperty.AgentUserRef) + "'", -1);
        if (intAgentID > -1 && intAgentID != intOrigAgentID) {
            oSQL.add("AGENTID", intAgentID);
            blnNeedToUpdate = true;
        }
        if (blnNeedToUpdate) {
            oSQL.add("MYOBCARDID", getMYOBCardID(oProperty.StreetAddress, oProperty.Street));
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }

    private void loadCampaignProducts() {
        if (blnCampaignProductsLoaded)
            return;

        dTotalCost = 0;
        dTotalActual = 0;
        dTotalVendor = 0;
        string szSQL = String.Format(@"
            SELECT CP.ID, CP.CAMPAIGNID, CP.PURCHASEDATE, CP.COSTPRICE, CP.ACTUALPRICE, CP.VENDORPRICE, CP.SUPPLIER, CP.INVOICENO, ISNULL(CP.DESCRIPTION, P.DESCRIPTION) AS DESCRIPTION, CP.PRODUCTID, CP.ITEMID, CP.ISDELETED
            FROM CAMPAIGNPRODUCT CP LEFT JOIN PRODUCT P ON CP.PRODUCTID = P.ID
            WHERE CP.CAMPAIGNID = {0}
            ORDER BY CP.PURCHASEDATE", intDBID);
        using (SqlDataReader dr = DB.runReader(szSQL)) {
            while (dr.Read()) {
                CampaignProduct oP = new CampaignProduct(dr);
                if (!oP.IsDeleted) {
                    dTotalCost += oP.CostPrice;
                    dTotalActual += oP.ActualPrice;
                    dTotalVendor += oP.VendorPrice;
                    if (oP.VendorPrice > 0)
                        blnIsReconciled = true;
                }
                lProducts.Add(oP);
            }
        }
        blnCampaignProductsLoaded = true;
    }

    private void loadCampaignActions() {
        if (blnCampaignActionsLoaded)
            return;

        string szSQL = String.Format(@"
            SELECT CN.ID, CN.CAMPAIGNID, CN.NOTEDATE, CN.CONTENT, DA.FIRSTNAME + ' ' + DA.LASTNAME AS AGENTNAME, US.FIRSTNAME + ' ' + US.LASTNAME AS SENDERNAME,
            CN.REMINDER, CN.REMINDERVALID, CN.ACTIONID, ISNULL(A.NAME, '') AS ACTION
            FROM CAMPAIGNNOTE CN JOIN DB_USER US ON US.ID = CN.USERID
            JOIN DB_USER DA ON DA.ID = CN.AGENTID
            LEFT JOIN ACTION A ON A.ID = CN.ACTIONID
            WHERE CN.CAMPAIGNID = {0}
            ORDER BY CN.NOTEDATE DESC", intDBID);
        using (SqlDataReader dr = DB.runReader(szSQL)) {
            while (dr.Read()) {
                CampaignAction oCA = new CampaignAction(dr);
                lActions.Add(oCA);
            }
        }
        blnCampaignActionsLoaded = true;
    }

    private void loadCampaignContributions() {
        if (blnCampaignContributionsLoaded)
            return;

        dTotalBudget = 0;
        dTotalPaid = 0;
        string szSQL = String.Format(@"
            SELECT CC.*
            FROM CAMPAIGNCONTRIBUTION CC
            WHERE CC.CAMPAIGNID = {0} AND CC.ISDELETED = 0
            ORDER BY CC.CONTRIBUTIONDATE ", intDBID);
        using (SqlDataReader dr = DB.runReader(szSQL)) {
            while (dr.Read()) {
                CampaignContribution oC = new CampaignContribution(dr);
                lContributions.Add(oC);
                if (oC.Type == ContributionType.Agent || oC.Type == ContributionType.Fletchers)
                    dTotalAgentCompany += oC.Amount;

                if (oC.Type == ContributionType.Vendor && !oC.IsPayment)
                    dTotalBudget += oC.Amount;
                else
                    dTotalPaid += oC.Amount;
            }
        }
        blnCampaignContributionsLoaded = true;
    }

    private void loadCampaignInvoices() {
        if (blnCampaignInvoicesLoaded)
            return;

        dTotalInvoiced = 0;

        using (DataSet ds = CampaignInvoice.loadCampaignInvoices(intDBID)) {
            DataView dvItems = ds.Tables[1].DefaultView;
            foreach (DataRow dr in ds.Tables[0].Rows) {
                CampaignInvoice oI = new CampaignInvoice(dr);
                lInvoices.Add(oI);
                dTotalInvoiced += oI.Amount;
                dvItems.RowFilter = "CAMPAIGNINVOICEID = " + oI.DBID;
                foreach (DataRowView drI in dvItems) {
                    oI.InvoiceItems.Add(new InvoiceProduct(Convert.ToInt32(drI["ID"]), oI.DBID, Convert.ToInt32(drI["CAMPAIGNPRODUCTID"]), drI["REFERENCEID"].ToString(), Convert.ToDouble(drI["AMOUNTINCGST"]), Convert.ToDouble(drI["AMOUNTEXGST"])));
                }
            }
        }
        blnCampaignInvoicesLoaded = true;
    }

    /// <summary>
    /// Returns the campaign contribution if is matches the DBID
    /// </summary>
    /// <param name="DBID"></param>
    /// <returns></returns>
    public CampaignContribution findContributionByDBID(int DBID) {
        loadCampaignContributions();
        return lContributions.Find(delegate (CampaignContribution t) { return t.DBID == DBID; });
    }

    /// <summary>
    /// Adds a contribution to the campaign and updates the DB with the new data - this can come from an imported payment or a user entered contribution
    /// </summary>
    /// <param name="dtDate"></param>
    /// <param name="Amount"></param>
    /// <param name="oType"></param>
    /// <returns></returns>
    public CampaignContribution addContribution(DateTime dtDate, double Amount, bool IsPayment, ContributionType oType = ContributionType.Vendor, string RefID = "", string ReceiptNumber = "") {
        CampaignContribution oC = new CampaignContribution(intDBID, dtDate, Amount, oType, RefID, ReceiptNumber, IsPayment);
        this.lContributions.Add(oC);
        return oC;
    }

    public int checkProduct(Item Product) {
        loadCampaignProducts();
        CampaignProduct oP = lProducts.Find(delegate (CampaignProduct t) { return t.CTItemID == Product.ItemID; });
        if (oP == null) { //We need to import the campaign item into our system
            oP = new CampaignProduct(Product, intDBID);
            lProducts.Add(oP);
        } else {
            oP.checkDelta(Product);
        }
        if (oP == null)
            return -1;
        return oP.DBID;
    }

    /// <summary>
    /// Add all payments that we don't already have entered
    /// </summary>
    /// <param name="oC"></param>
    public void checkContributions(Contribution oC) {
        loadCampaignContributions();
        //Agent
        if (oC.PaidBy == "Agent" || oC.PaidBy == "Company") {
            CampaignContribution oCC = lContributions.Find(delegate (CampaignContribution t) { return t.CTRef == oC.ContributionRef; });

            if (oCC == null) {
                ContributionType oType = ContributionType.Agent;
                if (oC.PaidBy.Contains("Company"))
                    oType = ContributionType.Fletchers;

                addContribution(oC.Date, Convert.ToDouble(oC.Amount), true, oType, oC.ContributionRef, oC.ReceiptNo);
            } else {
                oCC.checkDelta(oC);
            }
        } else if (oC.PaidBy == "Vendor" || oC.PaidBy == "Company Filler") {
            //Check to see if we have already recorded this payment
            CampaignContribution oCC = lContributions.Find(delegate (CampaignContribution t) { return t.CTRef == oC.ContributionRef; });
            if (oCC == null) {
                if (oC.Received) {
                    ContributionType oContribBy = ContributionType.Vendor;
                    //We only ever import the controbutions if they are payments
                    if (oC.PaidBy == "Company")
                        oContribBy = ContributionType.Fletchers;
                    addContribution(oC.Date, Convert.ToDouble(oC.Amount), true, oContribBy, oC.ContributionRef, oC.ReceiptNo);
                }
            } else {
                oCC.checkDelta(oC);
            }
        }
    }

    /// <summary>
    /// Add all invoices that we don't already have entered
    /// </summary>
    /// <param name="oC"></param>
    public void checkInvoices(Invoice oI, InvoiceItem[] arInvoiceItems) {
        loadCampaignInvoices();
        CampaignInvoice oCI = lInvoices.Find(delegate (CampaignInvoice t) { return t.CTRef == oI.TransactionID.ToString(); });
        if (oCI == null) {
            oCI = new CampaignInvoice(oI, this.DBID);
            lInvoices.Add(oCI);
        } else {
            oCI.checkDelta(oI);
        }
        foreach (InvoiceItem oII in arInvoiceItems) {
            InvoiceProduct oIP = oCI.InvoiceItems.Find(delegate (InvoiceProduct t) { return t.CTRef == oII.ItemID.ToString(); });
            if (oIP == null) {
                oIP = new InvoiceProduct(oII, oCI.DBID, this.DBID);
                oCI.InvoiceItems.Add(oIP);
            } else {
                oIP.checkDelta(oII);
            }
        }
        //Check to see if there are any contributions that have not been marked as checked by the import process - these have been deleted
        foreach (InvoiceProduct oIP in oCI.InvoiceItems) {
            if (!oIP.CheckedByImportProcess)
                oIP.delete();
        }
    }
}

#region CampaignImport

public class CampaignImport {
    private ServiceReference1.IntegrationSoapClient oWS = new IntegrationSoapClient("IntegrationSoap12");
    private int intErrorCode = 0;
    private string szWSKey = "";
    private string szWSOffice = "";
    private int intCurrentCount = 0;

    public CampaignImport(string WSKey, string OfficeID) {
        szWSKey = WSKey;
        szWSOffice = OfficeID;
    }

    public static void performFullImport(bool OnlyLoadNew = false, bool OnlyUpdatePropertyDetails = false) {
        String CampaignTrackKeys = System.Configuration.ConfigurationManager.AppSettings["CampaignTrackKeys"];
        string[] arKeys = CampaignTrackKeys.Split('|');
        for (int i = 0; i < arKeys.Length; i += 2) {
            CampaignImport oI = new CampaignImport(arKeys[i+1], arKeys[i]);
            oI.performImport(OnlyLoadNew, OnlyUpdatePropertyDetails);
            oI = null;
        }
        HttpContext.Current.Response.Flush();
    }

    public static bool refreshProperty(Campaign oC) {
        String CampaignTrackKeys = System.Configuration.ConfigurationManager.AppSettings["CampaignTrackKeys"];
        string[] arKeys = CampaignTrackKeys.Split('|');

        CampaignImport oI = new CampaignImport(arKeys[1], oC.OfficeID);
        bool blnSuccess = oI.refreshPropertyDetails(oC);
        oI = null;
        return blnSuccess;
    }

    public void performImport(bool OnlyLoadNew = false, bool OnlyUpdatePropertyDetails = false) {
        G.User.ImportCurrentRecord = -1;
        G.User.ImportTotal = -1;
        G.User.IsImportDone = false;
        CampaignTrackProperty[] oCT = oWS.CheckPropertyDetails(szWSKey, szWSOffice, ref intErrorCode);
        G.User.ImportTotal = oCT.Length;

        string szSQL = "";
        checkError();
        StringBuilder sbUpdateIDs = new StringBuilder();
        foreach (CampaignTrackProperty oP in oCT) {
            G.User.ImportCurrentRecord = ++intCurrentCount;
            HttpContext.Current.Response.Write("Importing " + intCurrentCount + " of " + oCT.Length + ". (" + oP.PropertyRef + ")<br/>");
            HttpContext.Current.Response.Flush();
            if (String.IsNullOrWhiteSpace(oP.PropertyRef))
                continue;
            if (OnlyLoadNew) {
                //Check to see if it already exists - if so, we skip it
                szSQL = "SELECT ID FROM CAMPAIGN WHERE CAMPAIGNNUMBER = " + oP.PropertyRef;
                if (DB.getScalar(szSQL, -1) > 0)
                    continue;
            }
            sbUpdateIDs.Append(oP.PropertyRef + ",");
            importProperty(oP, szWSOffice, false, OnlyUpdatePropertyDetails);
        }

        if (OnlyLoadNew) { //Don't need to update the rest of the properties
            G.User.IsImportDone = true;
            return;
        }

        //Mark all these properties as recently updated
        if (sbUpdateIDs.Length > 0) {
            DB.runNonQuery("UPDATE CAMPAIGN SET LASTIMPORTEDDATE = getdate() WHERE CAMPAIGNNUMBER IN(" + sbUpdateIDs.ToString() + "-1)");
        }

        sbUpdateIDs.Length = 0;
        szSQL = "SELECT ID, CAMPAIGNNUMBER FROM CAMPAIGN C WHERE C.LASTIMPORTEDDATE < DATEADD(mi, -190, getdate()) AND STATUSID != 100 AND ISDELETED = 0 ";
        DataSet ds = DB.runDataSet(szSQL);
        G.User.ImportTotal = ds.Tables[0].Rows.Count;
        intCurrentCount = 0;
        foreach (DataRow dr in ds.Tables[0].Rows) {
            CampaignTrackProperty oP = oWS.CheckPropertyDetailByPropertyRef(szWSKey, szWSOffice, dr["CAMPAIGNNUMBER"].ToString(), ref intErrorCode);

            checkError();
            G.User.ImportCurrentRecord = ++intCurrentCount;
            HttpContext.Current.Response.Write("Processing  " + intCurrentCount + " of " + ds.Tables[0].Rows.Count + " existing properties that need updating. (" + dr["CAMPAIGNNUMBER"].ToString() + ")<br/>");
            HttpContext.Current.Response.Flush();

            if (oP.PropertyRef == null) {
                //We cannot load this property from Campaigntrack so skip it
                continue;
            }
            sbUpdateIDs.Append(dr["CAMPAIGNNUMBER"].ToString() + ",");
            importProperty(oP, szWSOffice, false, OnlyUpdatePropertyDetails);
        }

        //Mark all these properties as recently updated
        if (sbUpdateIDs.Length > 0) {
            DB.runNonQuery("UPDATE CAMPAIGN SET LASTIMPORTEDDATE = getdate() WHERE CAMPAIGNNUMBER IN(" + sbUpdateIDs.ToString() + "-1)");
        }
        G.User.IsImportDone = true;
    }

    /// <summary>
    /// Refreshes the property based on the number
    /// </summary>
    /// <param name="PropertyNumber"></param>
    public bool refreshPropertyDetails(Campaign oC) {
        CampaignTrackProperty oP = oWS.CheckPropertyDetailByPropertyRef(szWSKey, szWSOffice, oC.PropertyRef, ref intErrorCode);
        if (oP.PropertyRef == null) {
            oC.deleteCampaign();
            return false;
        }

        importProperty(oP, szWSOffice, true);
        string szSQL = "UPDATE CAMPAIGN SET LASTIMPORTEDDATE = getdate() WHERE CAMPAIGNNUMBER = '" + DB.escape(oC.PropertyRef) + "'";
        DB.runNonQuery(szSQL);
        return true;
    }

    /// <summary>
    /// Imports the property so they are totally synced up
    /// </summary>
    /// <param name="oP"></param>
    public void importProperty(CampaignTrackProperty oP, string OfficeKey, bool IgnoreStatus = false, bool OnlyUpdatePropertyDetails = false) {
        //Load/import the campaign into the system
        Campaign oCampaign = new Campaign(oP, OfficeKey);
        if (oCampaign.Status == CampaignStatus.Completed && !IgnoreStatus) {
            //We want to check the contributions for all campaigns, regardless of status.
            Contribution[] arC1 = oWS.GetCampaignContributions(szWSKey, szWSOffice, oP.PropertyRef, ref intErrorCode);
            checkError();

            foreach (Contribution oC in arC1) {
                oCampaign.checkContributions(oC);
            }
            Item[] arCI1 = oWS.GetCampaignItems(szWSKey, szWSOffice, oP.PropertyRef, ref intErrorCode);
            foreach (Item oI in arCI1) {
                oCampaign.checkProduct(oI);
            }
            return; //We have no further work to do with completed campaigns
        }
        Item[] arCI = oWS.GetCampaignItems(szWSKey, szWSOffice, oP.PropertyRef, ref intErrorCode);
        checkError();
        if (OnlyUpdatePropertyDetails)
            return;

        foreach (Item oI in arCI) {
            oCampaign.checkProduct(oI);
        }
        Contribution[] arC = oWS.GetCampaignContributions(szWSKey, szWSOffice, oP.PropertyRef, ref intErrorCode);
        checkError();

        foreach (Contribution oC in arC) {
            oCampaign.checkContributions(oC);
        }

        //Check to see if there are any contributions that have not been marked as checked by the import process - these have been deleted
        foreach (CampaignContribution oCC in oCampaign.ContributionList) {
            if (!oCC.CheckedByImportProcess && oCC.Splits.Count == 0)
                oCC.delete();
        }
        Invoice[] arI = oWS.GetCampaignInvoices(szWSKey, szWSOffice, oP.PropertyRef, ref intErrorCode);
        checkError();

        foreach (Invoice oI in arI) {
            InvoiceItem[] arInvoiceItems = oWS.GetInvoiceItems(szWSKey, oI.TransactionID, ref intErrorCode);

            oCampaign.checkInvoices(oI, arInvoiceItems);
        }

        //Check to see if there are any invoices that have not been marked as checked by the import process - these have been deleted
        foreach (CampaignInvoice oI in oCampaign.InvoiceList) {
            if (!oI.CheckedByImportProcess)
                oI.delete();
        }
    }

    private void checkError() {
        if (intErrorCode < 0) {
            G.Notifications.addPageNotification(PageNotificationType.Error, "CampaignTrack Error", "Error ID: " + intErrorCode);
            G.Notifications.showPageNotification();
        }
    }
}

#endregion CampaignImport

#region CampaignProduct

public class CampaignProduct : IComparable<CampaignProduct> {
    private double dActualPrice = 0;
    private double dCostPrice = 0;
    private double dVendorPrice = 0;
    private string szDescription = "";
    private DateTime dtPurchaseDate = DateTime.MinValue;
    private int intUserID = -1;
    private int intDBID = -1;
    private int intProductID = -1;
    private int intCampaignID = -1;
    private string szSupplier = "";
    private string szReceiptNumber = "";
    private string szCTItemID = "";
    private bool blnIsDeleted = false;

    public CampaignProduct(SqlDataReader dr) {
        szDescription = Convert.ToString(dr["DESCRIPTION"]);
        dActualPrice = Convert.ToDouble(dr["ACTUALPRICE"]);
        dCostPrice = Convert.ToDouble(dr["COSTPRICE"]);
        dVendorPrice = Convert.ToDouble(dr["VENDORPRICE"]);
        if (dr["PURCHASEDATE"] != System.DBNull.Value)
            dtPurchaseDate = Convert.ToDateTime(dr["PURCHASEDATE"]);
        intDBID = Convert.ToInt32(dr["ID"]);
        intProductID = Convert.ToInt32(dr["PRODUCTID"]);
        intCampaignID = Convert.ToInt32(dr["CAMPAIGNID"]);
        szSupplier = Convert.ToString(dr["SUPPLIER"]);
        szReceiptNumber = Convert.ToString(dr["INVOICENO"]);
        szCTItemID = Convert.ToString(dr["ITEMID"]);
        intDBID = Convert.ToInt32(dr["ID"]);
        blnIsDeleted = Convert.ToBoolean(dr["ISDELETED"]);
    }

    public CampaignProduct(Item Product, int CampaignID) {
        szDescription = Product.Description;
        dActualPrice = Convert.ToDouble(Product.ActualPrice);
        dCostPrice = Convert.ToDouble(Product.CostPrice);
        dVendorPrice = Convert.ToDouble(Product.VendorPrice);
        dtPurchaseDate = Product.Date;
        intProductID = getProductID(Product);
        szSupplier = Product.Supplier;
        szReceiptNumber = Product.InvoiceNo;
        intCampaignID = CampaignID;
        szCTItemID = Product.ItemID;
        blnIsDeleted = Product.Deleted;
        updateToDB();
    }

    public string Description {
        get { return szDescription; }
    }

    //The invoice number received from the supplier
    public string InvoiceNo {
        get { return szReceiptNumber; }
    }

    //The unique ID from CT
    public string CTItemID {
        get { return szCTItemID; }
    }

    public int ProductID {
        get { return intProductID; }
    }

    public int UserID {
        get { return intUserID; }
    }

    public int DBID {
        get { return intDBID; }
    }

    public bool IsDeleted {
        get { return blnIsDeleted; }
    }

    public DateTime Date {
        get { return dtPurchaseDate; }
    }

    public double ActualPrice {
        get { return dActualPrice; }
    }

    /// <summary>
    /// The price that Fletchers ends up invoicing the client
    /// </summary>
    public double CostPrice {
        get { return dCostPrice; }
    }

    /// <summary>
    /// The price that Fletchers ends up invoicing the client
    /// </summary>
    public double VendorPrice {
        get { return dVendorPrice; }
    }

    private void updateToDB() {
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGNPRODUCT", "ID", intDBID);
        oSQL.add("CAMPAIGNID", intCampaignID);
        oSQL.add("PRODUCTID", intProductID);
        if (dtPurchaseDate != DateTime.MinValue)
            oSQL.add("PURCHASEDATE", Utility.formatDate(dtPurchaseDate));
        oSQL.add("ACTUALPRICE", dActualPrice);
        oSQL.add("COSTPRICE", dCostPrice);
        oSQL.add("VENDORPRICE", dVendorPrice);
        oSQL.add("DESCRIPTION", szDescription);
        oSQL.add("SUPPLIER", szSupplier);
        oSQL.add("INVOICENO", szReceiptNumber);
        oSQL.add("ITEMID", szCTItemID);
        if (blnIsDeleted)
            oSQL.add("ISDELETED", 1);
        else
            oSQL.add("ISDELETED", 0);

        if (intDBID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM CAMPAIGNPRODUCT", -1);
        } else {
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
        //Update the start date based on this ID
        if (!String.IsNullOrWhiteSpace(this.Description) && this.Description.ToUpper().StartsWith("MARKETING PRODUCTION")) {
            DB.runNonQuery(String.Format(@"
                UPDATE CAMPAIGN SET STARTDATE = '{0}' where ID = {1} AND STARTDATEMODIFIED = 0", Utility.formatDate(dtPurchaseDate), intCampaignID));
        }
    }

    public int CompareTo(CampaignProduct other) {
        return this.Description.CompareTo(other.Description);
    }

    public void checkDelta(Item Product) {
        bool blnUpdate = false;
        if (Math.Abs(Convert.ToDouble(Product.ActualPrice) - Convert.ToDouble(dActualPrice)) > 0.01) {
            DBLog.addGenericRecord(DBLogType.CampaignProduct, "Actual amount " + dActualPrice + " <-> " + Product.ActualPrice, intCampaignID, intDBID);
            this.dActualPrice = Convert.ToDouble(Product.ActualPrice);
            blnUpdate = true;
        }
        if (Math.Abs(Convert.ToDouble(Product.CostPrice) - Convert.ToDouble(dCostPrice)) > 0.01) {
            DBLog.addGenericRecord(DBLogType.CampaignProduct, "Cost price " + dCostPrice + " <-> " + Product.CostPrice, intCampaignID, intDBID);

            this.dCostPrice = Convert.ToDouble(Product.CostPrice);
            blnUpdate = true;
        }
        if (Math.Abs(Convert.ToDouble(Product.VendorPrice) - Convert.ToDouble(dVendorPrice)) > 0.01) {
            DBLog.addGenericRecord(DBLogType.CampaignProduct, "Vendor price " + dVendorPrice + " <-> " + Product.VendorPrice, intCampaignID, intDBID);

            this.dVendorPrice = Convert.ToDouble(Product.VendorPrice);
            blnUpdate = true;
        }
        if (Product.Date != dtPurchaseDate) {
            this.dtPurchaseDate = Product.Date;
            blnUpdate = true;
        }
        if (this.Description != Product.Description) {
            this.szDescription = Product.Description;
            blnUpdate = true;
        }

        int intTestProductID = CampaignProduct.getProductID(Product);
        if (this.intProductID != intTestProductID) {
            intProductID = intTestProductID;
            blnUpdate = true;
        }

        if (this.Date != Product.Date) {
            this.dtPurchaseDate = Product.Date;
            blnUpdate = true;
        }

        if (this.InvoiceNo != Product.InvoiceNo) {
            this.szReceiptNumber = Product.InvoiceNo;
            blnUpdate = true;
        }

        if (this.IsDeleted != Product.Deleted) {
            blnIsDeleted = Product.Deleted;
            blnUpdate = true;
        }
        if (!String.IsNullOrWhiteSpace(this.Description) && this.Description.ToUpper().StartsWith("MARKETING PRODUCTION"))
            blnUpdate = true;
        if (blnUpdate)
            updateToDB();
    }

    /// <summary>
    /// Find the product ID in this system and return it, creating the product if it doesn't exist
    /// </summary>
    /// <param name="Product"></param>
    /// <returns></returns>
    public static int getProductID(Item Product) {
        string szSQL = "SELECT ID, DESCRIPTION FROM PRODUCT WHERE PRODUCTID = " + Product.ProductID;
        int ProductID = -1;
        using (SqlDataReader dr = DB.runReader(szSQL)) {
            while (dr.Read()) {
                //Product exists so update description
                ProductID = Convert.ToInt32(dr["ID"]);
                if (dr["DESCRIPTION"].ToString() != Product.Description) {
                    szSQL = String.Format(@"UPDATE PRODUCT SET DESCRIPTION = '{0}' WHERE ID = {1}", DB.escape(Convert.ToString(dr["DESCRIPTION"])), ProductID);
                    DB.runNonQuery(szSQL);
                }
            }
            if (ProductID == -1) {
                sqlUpdate oSQL = new sqlUpdate("PRODUCT", "ID", -1);
                oSQL.add("PRODUCTID", Product.ProductID);
                oSQL.add("DESCRIPTION", Product.Description);
                oSQL.add("PRICE", Convert.ToDouble(Product.CostPrice));
                DB.runNonQuery(oSQL.createInsertSQL());
                ProductID = DB.getScalar("SELECT MAX(ID) FROM PRODUCT", -1);
            }
        }
        return ProductID;
    }

    public static int addProductID(int ProductID, string Description, decimal CostPrice) {
        string szSQL = "SELECT ID, DESCRIPTION FROM PRODUCT WHERE PRODUCTID = " + ProductID;
        int intProductID = DB.getScalar(szSQL, -1);

        if (intProductID == -1) {
            sqlUpdate oSQL = new sqlUpdate("PRODUCT", "ID", -1);
            oSQL.add("PRODUCTID", ProductID);
            oSQL.add("DESCRIPTION", Description);
            oSQL.add("PRICE", Convert.ToDouble(CostPrice));
            DB.runNonQuery(oSQL.createInsertSQL());
            intProductID = DB.getScalar("SELECT MAX(ID) FROM PRODUCT", -1);
        }
        return intProductID;
    }
}

public class CampaignProductList : List<CampaignProduct> {
}

#endregion CampaignProduct

#region CampaignAction

public class CampaignActionList : List<CampaignAction> {
}

public class CampaignAction : IComparable<CampaignAction> {
    private string szNote = "";
    private DateTime dtActionDate = DateTime.MinValue;
    private int intUserID = -1;
    private int intDBID = -1;
    private int intActionID = -1;
    private int intCampaignID = -1;
    private string szSender = "";
    private string szAgent = "";
    private string szAction = "";
    private DateTime dtReminder = DateTime.MinValue;
    private bool blnIsReminderValid = false;

    public CampaignAction(SqlDataReader dr) {
        szNote = Convert.ToString(dr["CONTENT"]);
        szAgent = Convert.ToString(dr["AGENTNAME"]);
        szSender = Convert.ToString(dr["SENDERNAME"]);
        dtActionDate = Convert.ToDateTime(dr["NOTEDATE"]);
        if (System.DBNull.Value != dr["REMINDER"]) {
            dtReminder = Convert.ToDateTime(dr["REMINDER"]);
            blnIsReminderValid = Convert.ToBoolean(dr["REMINDERVALID"]);
        }
        intDBID = Convert.ToInt32(dr["ID"]);
        intCampaignID = Convert.ToInt32(dr["CAMPAIGNID"]);
        intActionID = Convert.ToInt32(dr["ACTIONID"]);
        intDBID = Convert.ToInt32(dr["ID"]);
        szAction = Convert.ToString(dr["ACTION"]);
    }

    public string Note {
        get { return szNote; }
    }

    public string Action {
        get { return szAction; }
    }

    public string Sender {
        get { return szSender; }
    }

    public string Agent {
        get { return szAgent; }
    }

    public bool IsReminderValid {
        get { return blnIsReminderValid; }
    }

    public int UserID {
        get { return intUserID; }
    }

    public int ActionID {
        get { return intActionID; }
    }

    public int DBID {
        get { return intDBID; }
    }

    public DateTime Date {
        get { return dtActionDate; }
    }

    /// <summary>
    /// Returns the date of the reminder, on minDate if the value is not set. Check the isReminderValid to see if the reminder has been turned off
    /// </summary>
    public DateTime ReminderDate {
        get { return dtReminder; }
    }

    private void updateToDB() {
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGNNOTE", "ID", intDBID);
        oSQL.add("CAMPAIGNID", intCampaignID);
        oSQL.add("NOTEDATE", Utility.formatDate(dtActionDate));
        oSQL.add("CONTENT", szNote);

        if (intDBID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM CAMPAIGNNOTE", -1);
        } else {
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }

    public int CompareTo(CampaignAction other) {
        return this.dtActionDate.CompareTo(other.dtActionDate);
    }
}

#endregion CampaignAction

#region CampaignContribution

public class CampaignContribution : IComparable<CampaignContribution> {
    private double dAmount = 0;
    private double dAmountOrig = 0;
    private DateTime dtContributionDate = DateTime.MinValue;
    private DateTime dtContributionDateOrig = DateTime.MinValue;

    private int intUserID = -1;
    private int intDBID = -1;
    private int intCampaignID = -1;
    private string szCTRefID = "";
    private string szReceiptNumber = "";
    private ContributionType oType = ContributionType.Vendor;
    private ContributionSplitList lSplits = new ContributionSplitList();
    private bool blnIsPayment = false;
    private ContributionStatus oStatus = ContributionStatus.New;
    private bool blnCheckedByImport = false;

    public CampaignContribution(SqlDataReader dr) {
        dAmount = Convert.ToDouble(dr["AMOUNT"]);
        dAmountOrig = dAmount;
        if (dr["CONTRIBUTIONDATE"] != System.DBNull.Value) {
            dtContributionDate = Convert.ToDateTime(dr["CONTRIBUTIONDATE"]);
            dtContributionDateOrig = dtContributionDate;
        }
        intDBID = Convert.ToInt32(dr["ID"]);
        intCampaignID = Convert.ToInt32(dr["CAMPAIGNID"]);
        oType = (ContributionType)Convert.ToInt32(dr["CONTRIBUTIONTYPEID"]);
        oStatus = (ContributionStatus)Convert.ToInt32(dr["FINANCESTATUSID"]);
        szCTRefID = Convert.ToString(dr["REFERENCEID"]);
        szReceiptNumber = Convert.ToString(dr["RECEIPTNUMBER"]);
        blnIsPayment = Convert.ToBoolean(dr["ISPAYMENT"]);
        loadSplits();
    }

    /// <summary>
    /// Create a contribution to be updated into the DB. THe data is written to the DB as it is created so we get a DB ID
    /// </summary>
    /// <param name="CampaignID"></param>
    /// <param name="ContributionDate"></param>
    /// <param name="ContributedAmount"></param>
    public CampaignContribution(int CampaignID, DateTime ContributionDate, double ContributedAmount, ContributionType oType, string CTRefID, string ReceiptNumber, bool IsPayment) {
        this.dAmount = Convert.ToDouble(ContributedAmount);
        dtContributionDate = ContributionDate;
        intCampaignID = CampaignID;
        intUserID = G.User.ID;
        this.oType = oType;
        szReceiptNumber = ReceiptNumber;
        szCTRefID = CTRefID;
        blnIsPayment = IsPayment;
        blnCheckedByImport = true;
        updateToDB();
    }

    public int DBID {
        get { return intDBID; }
    }

    /// <summary>
    /// Updates the status on the current contribution
    /// </summary>
    /// <param name="ContributionID"></param>
    /// <param name="oStatus"></param>
    public static void updateContributionStatus(int ContributionID, ContributionStatus oStatus) {
        string szSQL = String.Format(@"
            UPDATE CAMPAIGNCONTRIBUTION
            SET FINANCESTATUSID = {0} WHERE ID = {1}", (int)oStatus, ContributionID);
        DB.runNonQuery(szSQL);
    }

    /// <summary>
    /// Is this contribution a payment?
    /// </summary>
    public bool IsPayment {
        get { return blnIsPayment; }
    }

    /// <summary>
    /// Set to true when we match this contribution to a matching one from CT. This is the only way we can determine if it has been deleted
    /// </summary>
    public bool CheckedByImportProcess {
        get { return blnCheckedByImport; }
    }

    /// <summary>
    /// Has this contribution been sent to finance, or been modified since its been sent to finance
    /// </summary>
    public ContributionStatus Status {
        get { return oStatus; }
    }

    public double Amount {
        get { return dAmount; }
        set { dAmount = value; }
    }

    public DateTime Date {
        get { return dtContributionDate; }
        set { dtContributionDate = value; }
    }

    /// <summary>
    /// CampaignTrack unique identifier
    /// </summary>
    public string CTRef {
        get { return szCTRefID; }
    }

    /// <summary>
    /// CampaignTrack invoice number
    /// </summary>
    public string ReceiptNumber {
        get { return szReceiptNumber; }
    }

    public ContributionType Type {
        get { return oType; }
    }

    public ContributionSplitList Splits {
        get {
            return lSplits;
        }
    }

    private void loadSplits() {
        string szSQL = String.Format(@"
                SELECT CCS.*, CC.CONTRIBUTIONDATE
                FROM CAMPAIGNCONTRIBUTIONSPLIT CCS JOIN CAMPAIGNCONTRIBUTION CC ON CC.ID = CCS.CAMPAIGNCONTRIBUTIONID
                WHERE CC.ID = {0} AND CCS.ISDELETED = 0", intDBID);
        using (SqlDataReader dr = DB.runReader(szSQL)) {
            while (dr.Read()) {
                ContributionSplit oSplit = new ContributionSplit(dr);
                lSplits.Add(oSplit);
            }
        }
    }

    public void addSplit(double SplitAmount, AmountType oType, double CalculatedAmount, PaymentOption oPaymentOption, DateTime ContributionDate, DateTime FixedDate, bool IsAuctionDate) {
        ContributionSplit oS = new ContributionSplit(intDBID, SplitAmount, oType, CalculatedAmount, oPaymentOption, ContributionDate, FixedDate, IsAuctionDate);
        lSplits.Add(oS);
    }

    public void updateSplit(int DBID, double SplitAmount, AmountType oType, double CalculatedAmount, PaymentOption oPaymentOption, DateTime ContributionDate, DateTime FixedDate, bool IsAuctionDate) {
        ContributionSplit oS = lSplits.Find(delegate (ContributionSplit t) { return t.DBID == DBID; });
        oS.CalculatedAmount = CalculatedAmount;
        oS.SplitAmount = SplitAmount;
        oS.PaymentOption = oPaymentOption;
        oS.SplitType = oType;
        oS.ContributionDate = ContributionDate;
        if (oPaymentOption == PaymentOption.FixedDate)
            oS.DueDate = FixedDate;
        else
            oS.updateDueDate(ContributionDate);
        oS.IsAuctionDate = IsAuctionDate;
        oS.updateToDB();
    }

    /// <summary>
    /// Soft deletes the contribution from the DB
    /// </summary>
    public void delete() {
        string szSQL = "UPDATE CAMPAIGNCONTRIBUTION SET ISDELETED = 1 WHERE ID = " + DBID;
        DB.runNonQuery(szSQL);
    }

    /// <summary>
    /// Delete the split from the DB and current contribution
    /// </summary>
    /// <param name="DBID"></param>
    public void deleteSplit(int DBID, int CampaignID) {
        DB.runNonQuery("UPDATE CAMPAIGNCONTRIBUTIONSPLIT SET ISDELETED = 1 WHERE ID = " + DBID);
        ContributionSplit oS = lSplits.Find(delegate (ContributionSplit t) { return t.DBID == DBID; });
        lSplits.Remove(oS);
        DBLog.addGenericRecord(DBLogType.CampaignContributionSplit, "Deleted", CampaignID, DBID);
    }

    public void checkDelta(Contribution oC) {
        bool blnUpdate = false;
        blnCheckedByImport = true;
        if (Math.Abs(Convert.ToDouble(oC.Amount) - dAmount) > 0.01) {
            this.dAmount = Convert.ToDouble(oC.Amount);
            blnUpdate = true;
        }
        if (!this.IsPayment && oC.Received) {
            blnIsPayment = true;
            blnUpdate = true;
        } else if (this.IsPayment && !oC.Received) {
            blnIsPayment = false;
            blnUpdate = true;
        }
        if (this.Date != oC.Date) {
            this.Date = oC.Date;
            blnUpdate = true;
        }

        if (szReceiptNumber != oC.ReceiptNo) {
            szReceiptNumber = oC.ReceiptNo;
            blnUpdate = true;
        }
        if (oC.PaidBy == "Vendor" || oC.PaidBy == "Company Filler") {
            if (oType != ContributionType.Vendor) {
                oType = ContributionType.Vendor;
                blnUpdate = true;
            }
        } else if (oC.PaidBy == "Company") {
            if (oType != ContributionType.Fletchers) {
                oType = ContributionType.Fletchers;
                blnUpdate = true;
            }
            if (!IsPayment) { //We always mark these as paid
                blnIsPayment = true;
                blnUpdate = true;
            }
        } else if (oC.PaidBy == "Agent") {
            if (oType != ContributionType.Agent) {
                oType = ContributionType.Agent;
                blnUpdate = true;
            }
            if (!IsPayment) { //We always mark these as paid
                blnIsPayment = true;
                blnUpdate = true;
            }
        }

        if (blnUpdate)
            updateToDB();
    }

    public void updateToDB() {
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGNCONTRIBUTION", "ID", intDBID);
        oSQL.add("CAMPAIGNID", intCampaignID);
        if (dtContributionDate != DateTime.MinValue)
            oSQL.add("CONTRIBUTIONDATE", Utility.formatDate(dtContributionDate));
        oSQL.add("AMOUNT", dAmount);
        oSQL.add("USERID", intUserID);
        oSQL.add("REFERENCEID", szCTRefID);
        oSQL.add("ReceiptNumber", szReceiptNumber);
        oSQL.add("CONTRIBUTIONTYPEID", (int)oType);
        if (blnIsPayment)
            oSQL.add("ISPAYMENT", 1);
        else
            oSQL.add("ISPAYMENT", 0);

        if (intDBID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM CAMPAIGNCONTRIBUTION", -1);
        } else {
            if (dAmount != dAmountOrig || dtContributionDate != dtContributionDateOrig) {
                DBLog oL = new DBLog("CAMPAIGNCONTRIBUTION", intDBID, DBLogType.CampaignData);
                if (dAmount != dAmountOrig)
                    oL.appendToLog(String.Format("Amount {0} <-> {1}", Utility.formatMoney(Convert.ToDouble(dAmountOrig)), Utility.formatMoney(Convert.ToDouble(dAmount))));
                if (dtContributionDate != dtContributionDateOrig)
                    oL.appendToLog(String.Format("Date {0} <-> {1}", Utility.formatDate(dtContributionDateOrig), Utility.formatDate(dtContributionDate)));
                oL.writeLog();
            }
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }

    public int CompareTo(CampaignContribution other) {
        return this.dAmount.CompareTo(other.dAmount);
    }
}

public class CampaignContributionList : List<CampaignContribution> {
}

public class ContributionSplit : IComparable<ContributionSplit> {
    private double dCalculatedAmount = 0;
    private double dSplitAmount = 0;
    private double dCalculatedAmountOrig = 0;
    private double dSplitAmountOrig = 0;
    private int intUserID = -1;
    private int intDBID = -1;
    private int intCampaignContributionID = -1;
    private DateTime dtDueDate = DateTime.MinValue;
    private DateTime dtDueDateOrig = DateTime.MinValue;
    private DateTime dtContributionDate = DateTime.MinValue;
    private PaymentOption oPaymentOption = PaymentOption.Days7;
    private AmountType oSplitAmountType = AmountType.Dollar;
    private PaymentOption oPaymentOptionOrig = PaymentOption.Days7;
    private AmountType oSplitAmountTypeOrig = AmountType.Dollar;
    private bool blnIsAuctionDate = false;

    public ContributionSplit(SqlDataReader dr) {
        dCalculatedAmount = Convert.ToDouble(dr["AMOUNT"]);
        dCalculatedAmountOrig = dCalculatedAmount;
        dSplitAmount = Convert.ToDouble(dr["SPLITAMOUNT"]);
        dSplitAmountOrig = dSplitAmount;
        intDBID = Convert.ToInt32(dr["ID"]);
        intUserID = Convert.ToInt32(dr["USERID"]);
        intCampaignContributionID = Convert.ToInt32(dr["CAMPAIGNCONTRIBUTIONID"]);
        dtDueDate = Convert.ToDateTime(dr["DUEDATE"]);
        dtDueDateOrig = dtDueDate;
        dtContributionDate = Convert.ToDateTime(dr["CONTRIBUTIONDATE"]);
        oPaymentOption = (PaymentOption)Convert.ToInt32(dr["PAYMENTOPTIONID"]);
        oPaymentOptionOrig = oPaymentOption;
        oSplitAmountType = (AmountType)Convert.ToInt32(dr["SPLITTYPEID"]);
        blnIsAuctionDate = Convert.ToBoolean(dr["ISAUCTIONDATE"]);
        oSplitAmountTypeOrig = oSplitAmountType;
    }

    /// <summary>
    /// Create a new contribution split to add to the DB. This is immediately updated into the DB
    /// </summary>
    /// <param name="CampaignContributionID"></param>
    /// <param name="Amount"></param>
    /// <param name="oType"></param>
    /// <param name="dSplitAmount"></param>
    /// <param name="oPaymentOption"></param>
    /// <param name="ContributionDate"></param>
    /// <param name="FixedDate"></param>
    public ContributionSplit(int CampaignContributionID, double dSplitAmount, AmountType oType, double CalculatedAmount, PaymentOption oPaymentOption, DateTime ContributionDate, DateTime FixedDate, bool IsAuctionDate) {
        dCalculatedAmount = CalculatedAmount;
        this.dSplitAmount = dSplitAmount;
        intCampaignContributionID = CampaignContributionID;
        this.oPaymentOption = oPaymentOption;
        oSplitAmountType = oType;
        blnIsAuctionDate = IsAuctionDate;
        if (oPaymentOption == global::PaymentOption.FixedDate)
            dtDueDate = FixedDate;
        else
            updateDueDate(ContributionDate);
        updateToDB();
    }

    /// <summary>
    /// Calculates the due date
    /// </summary>
    /// <param name="ContributionDate"></param>
    public void updateDueDate(DateTime ContributionDate) {
        switch (oPaymentOption) {
            case global::PaymentOption.Days7: dtDueDate = ContributionDate.AddDays(7); break;
            case global::PaymentOption.Days14: dtDueDate = ContributionDate.AddDays(14); break;
            case global::PaymentOption.Days28: dtDueDate = ContributionDate.AddDays(28); break;
        }
    }

    public int UserID {
        get { return intUserID; }
    }

    public int DBID {
        get { return intDBID; }
    }

    public double SplitAmount {
        get { return dSplitAmount; }
        set { dSplitAmount = value; }
    }

    /// <summary>
    /// The date that the contrirbution was made
    /// </summary>
    public DateTime ContributionDate {
        get { return dtContributionDate; }
        set { dtContributionDate = value; }
    }

    public DateTime DueDate {
        get { return dtDueDate; }
        set { dtDueDate = value; }
    }

    public double CalculatedAmount {
        get { return dCalculatedAmount; }
        set { dCalculatedAmount = value; }
    }

    public PaymentOption PaymentOption {
        get { return oPaymentOption; }
        set { oPaymentOption = value; }
    }

    public AmountType SplitType {
        get { return oSplitAmountType; }
        set { oSplitAmountType = value; }
    }

    /// <summary>
    /// True when the fixed date entered is the date of the auction
    /// </summary>
    public bool IsAuctionDate {
        get { return blnIsAuctionDate; }
        set { blnIsAuctionDate = value; }
    }

    /// <summary>
    /// Write the changes to the DB
    /// </summary>
    public void updateToDB() {
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGNCONTRIBUTIONSPLIT", "ID", intDBID);
        oSQL.add("CAMPAIGNCONTRIBUTIONID", intCampaignContributionID);
        oSQL.add("AMOUNT", dCalculatedAmount);
        oSQL.add("SPLITAMOUNT", dSplitAmount);
        if (dtDueDate < Convert.ToDateTime("Jan 1, 1900"))
            dtDueDate = DateTime.Now;

        oSQL.add("DUEDATE", Utility.formatDate(dtDueDate));
        oSQL.add("PAYMENTOPTIONID", (int)oPaymentOption);
        oSQL.add("USERID", G.User.ID);
        if (blnIsAuctionDate)
            oSQL.add("ISAUCTIONDATE", 1);
        else
            oSQL.add("ISAUCTIONDATE", 0);
        oSQL.add("SPLITTYPEID", (int)oSplitAmountType);

        if (intDBID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM CAMPAIGNCONTRIBUTIONSPLIT", -1);
        } else {
            DBLog oL = new DBLog("CAMPAIGNCONTRIBUTIONSPLIT", intDBID, DBLogType.CampaignData);

            //Check for the changes
            if (dSplitAmount != dSplitAmountOrig)
                oL.appendToLog(String.Format("Split amount {0} <-> {1}", Utility.formatMoney(dSplitAmountOrig), Utility.formatMoney(dSplitAmount)));
            if (oSplitAmountType != oSplitAmountTypeOrig)
                oL.appendToLog(String.Format("Split type {0} <-> {1}", oSplitAmountTypeOrig.ToString(), oSplitAmountType.ToString()));
            if (oPaymentOption != oPaymentOptionOrig)
                oL.appendToLog(String.Format("Amount {0} <-> {1}", oPaymentOptionOrig.ToString(), oPaymentOption.ToString()));
            if (dtDueDateOrig != dtDueDate)
                oL.appendToLog(String.Format("Due date {0} <-> {1}", Utility.formatDate(dtDueDateOrig), Utility.formatDate(dtDueDate)));
            oL.writeLog();
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }

    public int CompareTo(ContributionSplit other) {
        return this.dCalculatedAmount.CompareTo(other.dCalculatedAmount);
    }
}

public class ContributionSplitList : List<ContributionSplit> {
}

#endregion CampaignContribution

#region CampaignInvoice

public class CampaignInvoice : IComparable<CampaignInvoice> {
    private double dAmount = 0;
    private double dAmountOrig = 0;
    private DateTime dtInvoiceDate = DateTime.MinValue;
    private DateTime dtInvoiceDateOrig = DateTime.MinValue;
    private InvoiceProductList oItemList = new InvoiceProductList();
    private int intUserID = -1;
    private int intDBID = -1;
    private int intCampaignID = -1;
    private string szCTRefID = "";
    private string szInvoiceNumber = "";
    private bool blnCheckedByImport = false;

    public CampaignInvoice(DataRow dr) {
        dAmount = Convert.ToDouble(dr["AMOUNT"]);
        dAmountOrig = dAmount;
        dtInvoiceDate = Convert.ToDateTime(dr["INVOICEDATE"]);
        dtInvoiceDateOrig = dtInvoiceDate;

        intDBID = Convert.ToInt32(dr["ID"]);
        intCampaignID = Convert.ToInt32(dr["CAMPAIGNID"]);
        szCTRefID = Convert.ToString(dr["REFERENCEID"]);
        szInvoiceNumber = Convert.ToString(dr["INVOICENUMBER"]);
    }

    /// <summary>
    /// Loads the non-deleted invoices for the passed in campaign
    /// </summary>
    /// <param name="CampaignID"></param>
    /// <returns></returns>
    public static DataSet loadCampaignInvoices(int CampaignID) {
        string szSQL = String.Format(@"
            SELECT CI.*
            FROM CAMPAIGNINVOICE CI
            WHERE CI.CAMPAIGNID = {0} AND CI.ISDELETED = 0
            ORDER BY CI.INVOICEDATE;

            SELECT *
            FROM INVOICEPRODUCT IP JOIN CAMPAIGNINVOICE CI ON CI.ID = IP.CAMPAIGNINVOICEID
            WHERE CI.CAMPAIGNID = {0};
            ", CampaignID);
        return DB.runDataSet(szSQL);
    }

    /// <summary>
    /// Create an invoice from the Campaign Track record.The data is written to the DB as it is created so we get a DB ID
    /// </summary>
    /// <param name="CampaignID"></param>
    /// <param name="ContributionDate"></param>
    /// <param name="ContributedAmount"></param>
    public CampaignInvoice(Invoice oI, int CampaignID) {
        this.dAmount = Convert.ToDouble(oI.TotalIncGST);
        dtInvoiceDate = oI.InvoiceDate;
        intCampaignID = CampaignID;
        intUserID = G.User.ID;
        szInvoiceNumber = oI.InvoiceNo;
        szCTRefID = oI.TransactionID.ToString();
        blnCheckedByImport = true;
        updateToDB();
    }

    public int DBID {
        get { return intDBID; }
    }

    public double Amount {
        get { return dAmount; }
        set { dAmount = value; }
    }

    public DateTime Date {
        get { return dtInvoiceDate; }
        set { dtInvoiceDate = value; }
    }

    /// <summary>
    /// CampaignTrack unique identifier
    /// </summary>
    public string CTRef {
        get { return szCTRefID; }
    }

    /// <summary>
    /// The products that are on this invoice
    /// </summary>
    public InvoiceProductList InvoiceItems {
        get {
            return oItemList;
        }
    }

    /// <summary>
    /// CampaignTrack invoice number
    /// </summary>
    public string InvoiceNumber {
        get { return szInvoiceNumber; }
    }

    /// <summary>
    /// Set to true when we match this invoice to a matching one from CT. This is the only way we can determine if an invoice has been deleted
    /// </summary>
    public bool CheckedByImportProcess {
        get { return blnCheckedByImport; }
    }

    /// <summary>
    /// Soft deletes the invoice from the DB
    /// </summary>
    public void delete() {
        string szSQL = "UPDATE CAMPAIGNINVOICE SET ISDELETED = 1 WHERE ID = " + DBID;
        DB.runNonQuery(szSQL);
    }

    public void checkDelta(Invoice oI) {
        blnCheckedByImport = true;
        if (Math.Abs(Convert.ToDouble(oI.TotalIncGST) - dAmount) > 0.01) {
            this.dAmount = Convert.ToDouble(oI.TotalIncGST);
            updateToDB();
        }
        if (szInvoiceNumber != oI.InvoiceNo) {
            szInvoiceNumber = oI.InvoiceNo;
            updateToDB();
        }
    }

    public void updateToDB() {
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGNINVOICE", "ID", intDBID);
        oSQL.add("CAMPAIGNID", intCampaignID);
        if (dtInvoiceDate != DateTime.MinValue)
            oSQL.add("INVOICEDATE", Utility.formatDate(dtInvoiceDate));
        oSQL.add("AMOUNT", dAmount);
        oSQL.add("REFERENCEID", szCTRefID);
        oSQL.add("INVOICENUMBER", szInvoiceNumber);

        if (intDBID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM CAMPAIGNINVOICE", -1);
        } else {
            if (dAmount != dAmountOrig || dtInvoiceDate != dtInvoiceDateOrig) {
                oSQL.add("ISMODIFIED", 1);

                DBLog oL = new DBLog("CAMPAIGNINVOICE", intDBID, DBLogType.CampaignData);
                if (dAmount != dAmountOrig)
                    oL.appendToLog(String.Format("Amount {0} <-> {1}", Utility.formatMoney(Convert.ToDouble(dAmountOrig)), Utility.formatMoney(Convert.ToDouble(dAmount))));
                if (dtInvoiceDate != dtInvoiceDateOrig)
                    oL.appendToLog(String.Format("Date {0} <-> {1}", Utility.formatDate(dtInvoiceDateOrig), Utility.formatDate(dtInvoiceDate)));
                oL.writeLog();
            }
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }

    public int CompareTo(CampaignInvoice other) {
        return this.dAmount.CompareTo(other.dAmount);
    }
}

public class CampaignInvoiceList : List<CampaignInvoice> {
}

#endregion CampaignInvoice

#region InvoiceProduct

public class InvoiceProduct : IComparable<InvoiceProduct> {
    private double dAmountIncGST = 0;
    private double dAmountIncGSTOrig = 0;
    private double dAmountExGST = 0;
    private double dAmountExGSTOrig = 0;

    private int intDBID = -1;
    private int intCampaignInvoiceID = -1;
    private int intCampaignProductID = -1;
    private string szCTRefID = "";
    private bool blnCheckedByImport = false;
    private bool blnIsDeleted = false;

    public InvoiceProduct(int DBID, int CampaignInvoiceID, int CampaignProductID, string CTReferenceID, double AmountIncGST, double AmountExGST) {
        intDBID = DBID;
        dAmountIncGST = AmountIncGST;
        dAmountIncGSTOrig = dAmountIncGST;

        dAmountExGST = AmountExGST;
        dAmountExGSTOrig = AmountExGST;

        intCampaignInvoiceID = CampaignInvoiceID;
        intCampaignProductID = CampaignProductID;
        szCTRefID = CTReferenceID;
    }

    /// <summary>
    /// Create an invoice product record from the Campaign Track record.The data is written to the DB as it is created so we get a DB ID
    /// </summary>
    /// <param name="CampaignID"></param>
    /// <param name="ContributionDate"></param>
    /// <param name="ContributedAmount"></param>
    public InvoiceProduct(InvoiceItem oI, int CampaignInvoiceID, int CampaignID) {
        this.dAmountIncGST = Convert.ToDouble(oI.AmountIncGST);
        this.dAmountExGST = Convert.ToDouble(oI.AmountExGST);
        intCampaignInvoiceID = CampaignInvoiceID;

        intCampaignProductID = DB.getScalar(String.Format(@"
            SELECT CP.ID FROM CAMPAIGNPRODUCT CP JOIN PRODUCT P ON CP.PRODUCTID = P.ID
            WHERE CAMPAIGNID = {0} AND P.PRODUCTID = {1} AND CP.ISDELETED = 0", CampaignID, oI.ProductID), -1);

        if (intCampaignProductID == -1) {
            //Check for the product's existence.
            int intProductID = CampaignProduct.addProductID(oI.ProductID, oI.Description, oI.AmountIncGST);
            intCampaignProductID = DB.getScalar(String.Format(@"
                SELECT CP.ID FROM CAMPAIGNPRODUCT CP JOIN PRODUCT P ON CP.PRODUCTID = P.ID
                WHERE CAMPAIGNID = {0} AND P.ID = {1}", CampaignID, intProductID), -1);
        }
        szCTRefID = oI.ItemID;
        blnCheckedByImport = true;
        if (intCampaignProductID > -1)
            updateToDB();
    }

    public int DBID {
        get { return intDBID; }
    }

    public double AmountIncGST {
        get { return dAmountIncGST; }
        set { dAmountIncGST = value; }
    }

    public double AmountExGST {
        get { return dAmountExGST; }
        set { dAmountExGST = value; }
    }

    /// <summary>
    /// CampaignTrack unique identifier
    /// </summary>
    public string CTRef {
        get { return szCTRefID; }
    }

    public int CampaignInvoiceID {
        get { return intCampaignInvoiceID; }
    }

    public int CampaignProductID {
        get { return intCampaignProductID; }
    }

    public bool IsDeleted {
        get { return blnIsDeleted; }
    }

    /// <summary>
    /// Set to true when we match this invoice to a matching one from CT. This is the only way we can determine if an invoice item has been deleted
    /// </summary>
    public bool CheckedByImportProcess {
        get { return blnCheckedByImport; }
    }

    /// <summary>
    /// Soft deletes the invoice from the DB
    /// </summary>
    public void delete() {
        string szSQL = "UPDATE INVOICEPRODUCT SET ISDELETED = 1 WHERE ID = " + DBID;
        DB.runNonQuery(szSQL);
    }

    public void checkDelta(InvoiceItem oII) {
        if (blnCheckedByImport) {
            //This is the second time we are looking at this transaction, which means that we need to apply the offset amount from the previous one!
            this.dAmountExGST += Convert.ToDouble(oII.AmountExGST);
            this.dAmountIncGST += Convert.ToDouble(oII.AmountIncGST);
            updateToDB();
            return;
        }
        blnCheckedByImport = true;
        if (Math.Abs(Convert.ToDouble(oII.AmountIncGST) - dAmountIncGST) > 0.01) {
            this.dAmountIncGST = Convert.ToDouble(oII.AmountIncGST);
        }
        if (Math.Abs(Convert.ToDouble(oII.AmountExGST) - dAmountExGST) > 0.01) {
            this.dAmountExGST = Convert.ToDouble(oII.AmountExGST);
        }
        //We handle the delete in an odd manner since we don't get a flag from CT - all products are assumed to be good - then we delete all those that aren't looked at.
        blnIsDeleted = false;
        updateToDB();
    }

    public void updateToDB() {
        if (intCampaignProductID == -1)
            return; //Special case where we cannot match on the product ID, so we throw it away
        sqlUpdate oSQL = new sqlUpdate("INVOICEPRODUCT", "ID", intDBID);
        oSQL.add("CAMPAIGNINVOICEID", intCampaignInvoiceID);
        oSQL.add("AMOUNTINCGST", dAmountIncGST);
        oSQL.add("AMOUNTEXGST", dAmountExGST);
        oSQL.add("REFERENCEID", szCTRefID);
        if (blnIsDeleted)
            oSQL.add("ISDELETED", 1);
        else
            oSQL.add("ISDELETED", 0);

        oSQL.add("CAMPAIGNPRODUCTID", intCampaignProductID);

        if (intDBID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
            intDBID = DB.getScalar("SELECT MAX(ID) FROM INVOICEPRODUCT", -1);
        } else {
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
    }

    public int CompareTo(InvoiceProduct other) {
        return this.dAmountIncGST.CompareTo(other.dAmountIncGST);
    }
}

public class InvoiceProductList : List<InvoiceProduct> {
}

#endregion InvoiceProduct

public class CampaignDB {

    public static DataSet loadCampaignActions(int CurrUserID = -1) {
        string szCampaignFilterSQL = String.Format(@"
            WHERE C.ISDELETED = 0  AND C.STATUSID != 100 AND C.OFFICEID IN ({0}) ", G.Settings.CampaignTrackOffice);
        if (CurrUserID == -1) {
            //Make sure that there is a campaign note
            szCampaignFilterSQL += @" AND C.ID IN
                (SELECT CAMPAIGNID FROM CAMPAIGNNOTE )";
        }

        string szCampaignNoteFilter = "";
        if (CurrUserID != -1) {
            //Show all the campaigns for the current user
            szCampaignNoteFilter += String.Format(@"
                AND C.AGENTID = {0} AND C.STATUSID != 100
            ", CurrUserID);
        }

        string szSQL = String.Format(@"
            SELECT C.ID, 0.00 AS TOTALSPENT, 0.00 AS TOTALINVOICED, 0.00 AS TOTALPAID, 0.00 AS TOTALOWING, SUM(ISNULL(CC.AMOUNT, 0)) AS APPROVEDBUDGET, 0.00 AS AMOUNTLEFT, MAX(U.INITIALSCODE) AS AGENT,
                MAX(ADDRESS1) + ' ' + MAX(ADDRESS2) + ' <br/>VIC ' + MAX(CAST(POSTCODE AS VARCHAR)) AS ADDRESS, 0 AS PRODUCTSINVOICED, 0 AS PRODUCTCOUNT,
                MAX(C.ORIGCAMPAIGNNUMBER) AS CAMPAIGNNUMBER, MAX(C.STARTDATE) AS STARTDATE, '' AS MYOBCARDID, MAX(L_OFFICE.NAME) AS OFFICE,
                (SELECT TOP 1 A.NAME FROM CAMPAIGNNOTE CN_INNER1 JOIN ACTION A ON CN_INNER1.ACTIONID = A.ID AND CN_INNER1.CAMPAIGNID = C.ID AND ISCOMPLETED = 0 ORDER BY NOTEDATE DESC) AS ACTION,
                ISNULL((SELECT TOP 1 A.ID FROM CAMPAIGNNOTE CN_INNER1 JOIN ACTION A ON CN_INNER1.ACTIONID = A.ID AND CN_INNER1.CAMPAIGNID = C.ID AND ISCOMPLETED = 0 ORDER BY NOTEDATE DESC), -1) AS ACTIONID
            FROM CAMPAIGN C
            LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.CONTRIBUTIONTYPEID = {1} AND CC.RECEIPTNUMBER = '' AND CC.ISPAYMENT = 0
            LEFT JOIN DB_USER U ON C.AGENTID = U.ID
            LEFT JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
            {0} {2}
            GROUP BY C.ID
            ORDER BY STARTDATE DESC;

            SELECT C.ID, SUM(ISNULL(CP.VENDORPRICE, 0)) AS VENDORTOTAL,  SUM(ISNULL(CP.COSTPRICE, 0)) AS COSTTOTAL, SUM(ISNULL(CP.ACTUALPRICE, 0)) AS ACTUALTOTAL,
            SUM(CASE WHEN INVOICENO != '' THEN 1 ELSE 0 END) AS PRODUCTSINVOICED, COUNT(CP.ID) AS PRODUCTCOUNT, SUM(CASE WHEN P.EXCLUDEFROMINVOICE = 1 THEN 1 ELSE 0 END) AS PRODUCTSEXCLUDED
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND CP.ISDELETED = 0
            LEFT JOIN PRODUCT P ON P.ID = CP.PRODUCTID
            {0} AND CP.VENDORPRICE != 0
            GROUP BY C.ID;

            SELECT C.ID, SUM(ISNULL(CI.AMOUNT, 0)) AS TOTALINVOICED
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNINVOICE CI ON CI.CAMPAIGNID = C.ID
            {0} AND CI.ISDELETED = 0
            GROUP BY C.ID;

            SELECT C.ID, SUM(ISNULL(CC.AMOUNT, 0)) AS TOTALPAID
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.ISPAYMENT = 1
            {0}
            GROUP BY C.ID;

            ", szCampaignFilterSQL, (int)ContributionType.Vendor, szCampaignNoteFilter);
        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }

    /// <summary>
    /// Loads up the campaign information and sets all the values properly.
    /// </summary>
    /// <param name="PageMode"></param>
    /// <param name="CampaignFilter"></param>
    /// <param name="SearchText"></param>
    /// <returns></returns>
    public static DataView loadCampaignData(CampaignPageView PageMode, string CampaignFilter = "", string SearchText = "") {
        string szCampaignFilterSQL = String.Format(" WHERE C.ISDELETED = 0 AND C.OFFICEID IN({0}) ", G.Settings.CampaignTrackOffice);
        if (CampaignFilter != "")
            szCampaignFilterSQL += CampaignFilter;
        string szContributionFilterSQL = "";
        string szHavingClause = "";
        switch (PageMode) {
            case CampaignPageView.All:
                szCampaignFilterSQL += String.Format(" AND C.STATUSID != {0} ", (int)CampaignStatus.Completed); ;
                break;

            case CampaignPageView.Actioned:
                szCampaignFilterSQL += String.Format(@"
                    AND C.STATUSID != {0} AND C.ID IN (
                        SELECT CAMPAIGNID
                        FROM CAMPAIGNNOTE CN )
                    ", (int)CampaignStatus.Completed); ;
                break;

            case CampaignPageView.New:
                szHavingClause = " HAVING SUM(ISNULL(CC.AMOUNT, 0)) = 0 ";
                szCampaignFilterSQL += String.Format(" AND C.STATUSID != {0} ", (int)CampaignStatus.Completed);
                break;

            case CampaignPageView.Completed:
                szCampaignFilterSQL = String.Format(" WHERE (C.ADDRESS1 LIKE '%{0}%' OR C.ORIGCAMPAIGNNUMBER LIKE '%{0}%') ", DB.escape(SearchText));
                break;

            case CampaignPageView.ExceedingAuthority:
                szCampaignFilterSQL += String.Format(" AND C.STATUSID != {0} ", (int)CampaignStatus.Completed);
                break;

            case CampaignPageView.InvoiceDue:
                szCampaignFilterSQL += String.Format(" AND C.STATUSID != {0}  ", (int)CampaignStatus.Completed);
                break;

            case CampaignPageView.PartialInvoiced:
                szCampaignFilterSQL += String.Format(" AND C.STATUSID != {0} ", (int)CampaignStatus.Completed);
                break;

            case CampaignPageView.InvoiceChanged:
                szCampaignFilterSQL += " AND C.ID IN (SELECT CAMPAIGNID FROM CAMPAIGNINVOICE WHERE ISMODIFIED = 1)";
                break;

            case CampaignPageView.InvoiceAging:
                szCampaignFilterSQL += " AND C.ID IN (SELECT CAMPAIGNID FROM CAMPAIGNINVOICE WHERE INVOICEDATE <= getdate() AND ISDELETED = 0)";
                break;

            case CampaignPageView.DueForCollection:
                szContributionFilterSQL += String.Format(@" AND C.ID IN (
                    SELECT CC.CAMPAIGNID FROM CAMPAIGNCONTRIBUTIONSPLIT CCS JOIN CAMPAIGNCONTRIBUTION CC ON CCS.CAMPAIGNCONTRIBUTIONID = CC.ID
                    WHERE DUEDATE < DAteAdd(d, 7, getdate()) AND CC.CONTRIBUTIONTYPEID ={0}) AND CC.CONTRIBUTIONTYPEID = {0}", (int)ContributionType.Vendor);
                break;

            case CampaignPageView.Overdue:
                szContributionFilterSQL = String.Format(@"  AND C.ID IN (
                    SELECT CC.CAMPAIGNID FROM CAMPAIGNCONTRIBUTIONSPLIT CCS JOIN CAMPAIGNCONTRIBUTION CC ON CCS.CAMPAIGNCONTRIBUTIONID = CC.ID
                    WHERE DATEDIFF(d, DUEDATE, getdate()) > 30 AND CC.CONTRIBUTIONTYPEID = {0}) AND CC.CONTRIBUTIONTYPEID = {0}", (int)ContributionType.Vendor);
                break;
        }
        string szSQL = String.Format(@"
            --Load all campaigns
            SELECT C.ID, 0.00 AS TOTALSPENT, 0.00 AS TOTALINVOICED, 0.00 AS TOTALPAID, 0.00 AS TOTALOWING, SUM(ISNULL(CC.AMOUNT, 0)) AS APPROVEDBUDGET, 0.00 AS AMOUNTLEFT, MAX(U.INITIALSCODE) AS AGENT,
                MAX(ADDRESS1) + ' ' + MAX(ADDRESS2) + ' <br/>VIC ' + MAX(CAST(POSTCODE AS VARCHAR)) AS ADDRESS, 0 AS PRODUCTSINVOICED, 0 AS PRODUCTCOUNT, C.ISDELETED, 0 AS PRODUCTSEXCLUDED,
                MAX(C.ORIGCAMPAIGNNUMBER) AS CAMPAIGNNUMBER, MAX(C.STARTDATE) AS STARTDATE, '' AS MYOBCARDID, MAX(L_OFFICE.NAME) AS OFFICE, (SELECT COUNT(CN.ID) FROM CAMPAIGNNOTE CN WHERE CN.CAMPAIGNID = C.ID) AS NOTECOUNT,
                -1 AS CONTRIBUTIONSTATUS, MAX(U.ID) AS AGENTID, MAX(L_OFFICE.COMPANYID) AS COMPANYID, 0 AS DAYSOWING, MAX(C.STATUSID) AS STATUSID
            FROM CAMPAIGN C
            LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.CONTRIBUTIONTYPEID = {1} AND CC.RECEIPTNUMBER = '' AND CC.ISPAYMENT = 0
            LEFT JOIN DB_USER U ON C.AGENTID = U.ID
            LEFT JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
            {0} {2}
            GROUP BY C.ID, C.ISDELETED, C.STARTDATE
            {3}
            ORDER BY STARTDATE DESC;

            -- 1 Load up all the spending
            SELECT C.ID, SUM(ISNULL(CP.VENDORPRICE, 0)) AS VENDORTOTAL,  SUM(ISNULL(CP.COSTPRICE, 0)) AS COSTTOTAL, SUM(ISNULL(CP.ACTUALPRICE, 0)) AS ACTUALTOTAL, 0 AS OVERDUE,
            SUM(CASE WHEN INVOICENO != '' THEN 1 ELSE 0 END) AS PRODUCTSINVOICED, COUNT(CP.ID) AS PRODUCTCOUNT, SUM(CASE WHEN P.EXCLUDEFROMINVOICE = 1 THEN 1 ELSE 0 END) AS PRODUCTSEXCLUDED,
            MAX(C.STATUSID) AS CAMPAIGNSTATUS
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND CP.ISDELETED = 0
            LEFT JOIN PRODUCT P ON P.ID = CP.PRODUCTID
            {0} AND CP.VENDORPRICE != 0
            GROUP BY C.ID;

            -- 2 Load the campaign invoices
            SELECT C.ID, SUM(ISNULL(CI.AMOUNT, 0)) AS TOTALINVOICED, MIN(INVOICEDATE) AS INVOICEDATE
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNINVOICE CI ON CI.CAMPAIGNID = C.ID
            {0} AND CI.ISDELETED = 0
            GROUP BY C.ID;

            -- 3 Load the total paid
            SELECT C.ID, SUM(ISNULL(CC.AMOUNT, 0)) AS TOTALPAID
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND (CC.ISPAYMENT = 1 OR CC.CONTRIBUTIONTYPEID = 1)
            {0}
            GROUP BY C.ID;

            -- 4 Load contribution status
            SELECT C.ID, ISNULL(CC.FINANCESTATUSID, -1) AS STATUS, ISNULL(CC.AMOUNT, 0) AS AMOUNT, ISNULL(CC.ISPAYMENT, 0) AS ISPAYMENT
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.CONTRIBUTIONTYPEID IN (1, 2) AND CC.AMOUNT != 0
            {0} ;

            -- 5 Load all the authorities
            SELECT C.ID, CCS.DUEDATE, ISNULL(CCS.AMOUNT, 0) AS AMOUNT
            FROM CAMPAIGN C JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.CONTRIBUTIONTYPEID IN (0) AND CC.AMOUNT != 0
            JOIN CAMPAIGNCONTRIBUTIONSPLIT CCS ON CCS.CAMPAIGNCONTRIBUTIONID = CC.ID
            {0};
            ", szCampaignFilterSQL, (int)ContributionType.Vendor, szContributionFilterSQL, szHavingClause);
        DataSet dsCampaign = DB.runDataSet(szSQL);
        DataView dvSpend = dsCampaign.Tables[1].DefaultView;
        DataView dvInvoiced = dsCampaign.Tables[2].DefaultView;
        DataView dvPaid = dsCampaign.Tables[3].DefaultView;
        DataView dvContributionStatus = dsCampaign.Tables[4].DefaultView;
        DataView dvAuthorities = dsCampaign.Tables[5].DefaultView;
        foreach (DataRow drC in dsCampaign.Tables[0].Rows) {
            //Get the campaign product total from the product table
            dvSpend.RowFilter = "ID = " + drC["ID"].ToString();
            dvInvoiced.RowFilter = "ID = " + drC["ID"].ToString();
            dvPaid.RowFilter = "ID = " + drC["ID"].ToString();
            dvContributionStatus.RowFilter = "ID = " + drC["ID"].ToString();
            double dblApprovedBudget = 0;
            double dblTotalSpent = 0;
            foreach (DataRowView oRow in dvSpend) {
                // If this item has been reconciled, then use the vendor total, otherwise use the cost total
                if (Convert.ToDouble(oRow["VENDORTOTAL"]) == 0)
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["ACTUALTOTAL"]));
                else
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["VENDORTOTAL"]));
                drC["PRODUCTSINVOICED"] = Convert.ToInt32(oRow["PRODUCTSINVOICED"]);
                drC["PRODUCTSEXCLUDED"] = Convert.ToInt32(oRow["PRODUCTSEXCLUDED"]);
                drC["PRODUCTCOUNT"] = Convert.ToInt32(oRow["PRODUCTCOUNT"]);
                dblApprovedBudget = Convert.ToDouble(drC["APPROVEDBUDGET"]);
                dblTotalSpent = Convert.ToDouble(drC["TOTALSPENT"]);
            }

            foreach (DataRowView oRow in dvInvoiced) {
                drC["TOTALINVOICED"] = Utility.formatMoney(Convert.ToDouble(oRow["TOTALINVOICED"]));
            }

            double dblPaid = 0;

            foreach (DataRowView oRow in dvPaid) {
                dblPaid = Convert.ToDouble(oRow["TOTALPAID"]);
            }

            bool blnShowStatusIcon = false;
            foreach (DataRowView oRow in dvContributionStatus) {
                if (Convert.ToInt32(oRow["STATUS"]) == 0)
                    blnShowStatusIcon = true;
                if (Convert.ToBoolean(oRow["ISPAYMENT"]) == false)
                    dblPaid += Convert.ToDouble(oRow["AMOUNT"]); //Include the amounts paid by the company or agent unless this is marked as a payment, which would double apply the result
                dblApprovedBudget += Convert.ToDouble(oRow["AMOUNT"]);
            }
            if (blnShowStatusIcon)
                drC["CONTRIBUTIONSTATUS"] = 1;

            drC["TOTALPAID"] = Utility.formatMoney(dblPaid);
            drC["AMOUNTLEFT"] = Utility.formatMoney(dblApprovedBudget - dblTotalSpent);

            DB.runDebug(String.Format("--DEBUG  CN: {3} Total spend {0} Total Paid = {1} Totalowing {2}", Utility.formatMoney(dblTotalSpent), Utility.formatMoney(dblPaid), Utility.formatMoney(dblTotalSpent - dblPaid), drC["CAMPAIGNNUMBER"].ToString()));

            drC["TOTALOWING"] = Utility.formatMoney(dblTotalSpent - dblPaid);

            if (PageMode == CampaignPageView.DueForCollection || PageMode == CampaignPageView.Overdue || PageMode == CampaignPageView.InvoiceAging) {
                //We need to check to see if any of the authorities have been paid
                dvAuthorities.RowFilter = "ID = " + drC["ID"].ToString();
                double dblPaymentLeft = dblPaid;
                DateTime dtEfectiveDate = DateTime.MinValue;
                if (PageMode == CampaignPageView.DueForCollection || PageMode == CampaignPageView.Overdue) {
                    foreach (DataRowView oRow in dvAuthorities) {
                        double dblAuthorityAmount = Convert.ToDouble(oRow["AMOUNT"]);
                        dtEfectiveDate = Convert.ToDateTime(oRow["DUEDATE"]);
                        if (dblPaymentLeft - dblAuthorityAmount < 0)
                            break;
                        else
                            dblPaymentLeft -= dblAuthorityAmount;
                    }
                } else if (PageMode == CampaignPageView.InvoiceAging) {
                    //Get the days overdue strictly from the invoice itself - this may have to be split up to individual invoices for additional clarity
                    foreach (DataRowView oRow in dvInvoiced) {
                        double dblAuthorityAmount = Convert.ToDouble(oRow["TOTALINVOICED"]);
                        dtEfectiveDate = Convert.ToDateTime(oRow["INVOICEDATE"]);
                        if (dblPaymentLeft - dblAuthorityAmount < 0)
                            break;
                        else
                            dblPaymentLeft -= dblAuthorityAmount;
                    }
                }
                DateTime dtCompare = DateTime.Now;
                if (PageMode == CampaignPageView.DueForCollection)
                    dtCompare = dtCompare.AddDays(7);
                TimeSpan t = dtCompare.Subtract(dtEfectiveDate);
                drC["DAYSOWING"] = t.Days;
                if (PageMode == CampaignPageView.DueForCollection || PageMode == CampaignPageView.Overdue) {
                    if (dtEfectiveDate > dtCompare) { //The previous payment must've been paid off
                        drC.Delete();
                    }
                }
            }
        }

        DataView dvCampaigns = dsCampaign.Tables[0].DefaultView;
        switch (PageMode) {
            case CampaignPageView.ExceedingAuthority:
                dvCampaigns.RowFilter = "AMOUNTLEFT < 0";
                break;

            case CampaignPageView.PartialInvoiced:
                dvCampaigns.RowFilter = String.Format(@"(TOTALSPENT - TOTALPAID > {0} OR STARTDATE < '{1}') AND (TOTALSPENT - TOTALINVOICED > 0)", G.Settings.InvoiceTotalThreshold, Utility.formatDate(DateTime.Today.AddDays(-1 * G.Settings.PrePaymentNumberOfDays)));
                break;

            case CampaignPageView.InvoiceDue:
                //The total spend doesn't exceed the authority and there are no products with non-zero amounts left that do not have a supplier invoice tag on them
                dvCampaigns.RowFilter = String.Format(@"
                    PRODUCTCOUNT > 0  AND (PRODUCTCOUNT - PRODUCTSEXCLUDED - PRODUCTSINVOICED = 0) AND (TOTALSPENT - TOTALPAID > 0) AND (TOTALSPENT -TOTALINVOICED) > 0", G.Settings.InvoiceTotalThreshold, Utility.formatDate(DateTime.Today.AddDays(-1 * G.Settings.PrePaymentNumberOfDays)));
                break;

            case CampaignPageView.DueForCollection:

                //We may have payments that are due before we perform any invoicing
                dvCampaigns.RowFilter = "TOTALOWING > 0 OR (TOTALOWING = 0 AND TOTALINVOICED = 0 AND TOTALPAID = 0 AND STATUSID <> 100)";
                dvCampaigns.Sort = "NOTECOUNT ASC, STARTDATE DESC";
                break;

            case CampaignPageView.Overdue:
                //We may have payaments that are due before we perform any invoicing
                dvCampaigns.RowFilter = "TOTALOWING > 0 OR (TOTALOWING = 0 AND TOTALINVOICED = 0 AND TOTALPAID = 0 AND STATUSID <> 100)";
                break;
        }
        return dvCampaigns;
    }

    /// <summary>
    /// Loads the prepayment campaign information.
    /// </summary>
    /// <param name="szCampaignFilterSQL"></param>
    /// <returns></returns>
    public static DataTable loadPrePaymentCampaignData(string szCampaignFilterSQL, string CompanyIDList, bool ShowMonthTotals, String szCurrentPeriodFilter = null) {
        szCampaignFilterSQL += String.Format(" AND C.OFFICEID IN ({0}) ",  G.Settings.CampaignTrackOffice);
        string szCompanyFilter = "";

        // Creates the extra field [PERIOD] if szCurrentPeriodFilter has been given
        string szPeriodField = String.IsNullOrWhiteSpace(szCurrentPeriodFilter) ? "" : String.Format(@"
    				CASE WHEN C.STARTDATE {0}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,", szCurrentPeriodFilter);

        if (!String.IsNullOrWhiteSpace(CompanyIDList))
            szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", CompanyIDList);

        string szSQL = String.Format(@"
            -- 0
            SELECT C.ID, 0.00 AS TOTALSPENT, 0.00 AS TOTALINVOICED, 0.00 AS TOTALPAID, 0.00 AS TOTALOWING, 0.00 as TOTALPREPAID,
                0.00 AS [% PREPAID], C.AGENTID, U.INITIALSCODE AS AGENT, ISNULL(U.FIRSTNAME,'') + ', ' +  ISNULL(U.LASTNAME,'') as AGENTNAME,
                '' AS PROPERTYCOUNT, '' AS AGENTAVERAGE, 0 as AGENTAVERAGEPERCENT, 0 AS TOTALROW,
                ADDRESS1 + ' ' + ADDRESS2 AS ADDRESS, C.ORIGCAMPAIGNNUMBER AS CAMPAIGNNUMBER, C.STARTDATE AS LISTEDDATE, U.ROLEID,
                U.OFFICEID, ISNULL(L_OFFICE.NAME, '<unknown>') AS OFFICE, {2} C.AUCTIONDATE, U.SHOWONKPIREPORT
            FROM CAMPAIGN C
            LEFT JOIN DB_USER U ON C.AGENTID = U.ID
            LEFT JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
            {0}
            AND C.ISDELETED = 0 AND C.STARTDATE IS NOT NULL {1}
            ORDER BY  l_OFFICE.NAME, U.INITIALSCODE, STARTDATE DESC;

            -- 1 Total spend
            SELECT C.ID, SUM(ISNULL(CP.VENDORPRICE, 0)) AS VENDORTOTAL,  SUM(ISNULL(CP.COSTPRICE, 0)) AS COSTTOTAL,
                SUM(ISNULL(CP.ACTUALPRICE, 0)) AS ACTUALTOTAL
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND CP.ISDELETED = 0
            LEFT JOIN PRODUCT P ON P.ID = CP.PRODUCTID
            {0} AND C.ISDELETED = 0
            GROUP BY C.ID;

            -- 2 Payments
            SELECT C.ID, ISNULL(CC.AMOUNT, 0) AS TOTALPAID, CC.CONTRIBUTIONDATE, CC.CONTRIBUTIONTYPEID
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.ISPAYMENT = 1
            {0} AND C.ISDELETED = 0 AND CC.CONTRIBUTIONDATE IS NOT NULL;

            ", szCampaignFilterSQL, szCompanyFilter, szPeriodField);

        DataSet dsCampaign = DB.runDataSet(szSQL);
        System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
        //Process the dataset now
        DataView dvSpend = dsCampaign.Tables[1].DefaultView;
        DataView dvPaid = dsCampaign.Tables[2].DefaultView;
        double dblPaid = 0;
        double dblPrePaid = 0;
        int intOfficeCount = 0;
        int intAgentCount = 0;

        int intTotalCount = 0;
        double dAgentTotalPrePaid = 0;
        double dAgentTotalSpent = 0;
        double dAgentTotalPaid = 0;

        double dOfficePrePaid = 0;
        double dOfficeTotalSpent = 0;
        double dOfficeTotalPaid = 0;

        double dGrandTotalPrePaid = 0;
        double dGrandTotalSpent = 0;
        double dGrandTotalPaid = 0;
        string szCurrAgent = "START";
        string szCurrOffice = "START";
        int intCurrMonth = -1;
        int intMonthCount = 0;
        double dMonthTotalPrePaid = 0;
        double dMonthTotalSpent = 0;
        double dMonthTotalPaid = 0;

        DataTable dtFinal = dsCampaign.Tables[0].Clone();
        DataRow oPrevRow = null;
        foreach (DataRow drC in dsCampaign.Tables[0].Rows) {
            DateTime dtSaleDate = Convert.ToDateTime(drC["LISTEDDATE"]);
            if (szCurrAgent != drC["AGENT"].ToString()) {
                if (szCurrAgent != "START") {
                    DataRow drTotal = dtFinal.NewRow();
                    drTotal["AGENT"] = szCurrAgent;

                    drTotal["PROPERTYCOUNT"] = intAgentCount;
                    if (dAgentTotalSpent > 0) {
                        drTotal["AgentAverage"] = Convert.ToInt32((dAgentTotalPrePaid / dAgentTotalSpent) * 100) + "%";
                        drTotal["AgentAveragePercent"] = Convert.ToInt32((dAgentTotalPrePaid / dAgentTotalSpent) * 100);
                        drTotal["TOTALROW"] = 1;
                    }
                    drTotal["TOTALPAID"] = dAgentTotalPaid;
                    drTotal["TOTALSPENT"] = dAgentTotalSpent;
                    drTotal["TOTALOWING"] = Utility.formatMoney(dAgentTotalSpent - dAgentTotalPaid);

                    dtFinal.Rows.Add(drTotal);
                }
                szCurrAgent = drC["AGENT"].ToString();
                intAgentCount = 0;
                dAgentTotalSpent = 0;
                dAgentTotalPrePaid = 0;
                dAgentTotalPaid = 0;
            }

            if (szCurrOffice != drC["OFFICE"].ToString()) {
                if (szCurrOffice != "START") {
                    DataRow drFooter = dtFinal.NewRow();
                    drFooter["AGENT"] = szCurrOffice + " Totals";
                    drFooter["PROPERTYCOUNT"] = intOfficeCount;
                    drFooter["TOTALPAID"] = dOfficeTotalPaid;
                    drFooter["TOTALSPENT"] = dOfficeTotalSpent;

                    if (dOfficeTotalSpent > 0)
                        drFooter["AgentAverage"] = Convert.ToInt32((dOfficePrePaid / dOfficeTotalSpent) * 100) + "%";
                    dtFinal.Rows.Add(drFooter);
                    intOfficeCount = 0;
                    dOfficeTotalSpent = 0;
                    dOfficeTotalPaid = 0;
                    dOfficePrePaid = 0;
                }
                szCurrOffice = drC["OFFICE"].ToString();
            }

            if (intCurrMonth != dtSaleDate.Month) {
                if (intCurrMonth > -1 && ShowMonthTotals) {
                    oPrevRow["PROPERTYCOUNT"] = intMonthCount;
                    if (dMonthTotalSpent > 0)
                        oPrevRow["AgentAverage"] = mfi.GetAbbreviatedMonthName(intCurrMonth) + " " + Convert.ToInt32((dMonthTotalPrePaid / dMonthTotalSpent) * 100) + "%";
                }
                dMonthTotalPaid = 0;
                dMonthTotalPrePaid = 0;
                dMonthTotalSpent = 0;
                intMonthCount = 0;
                intCurrMonth = dtSaleDate.Month;
            }
            //Get the campaign product total from the product table
            dvSpend.RowFilter = "ID = " + drC["ID"].ToString();
            dvPaid.RowFilter = "ID = " + drC["ID"].ToString();
            double dTotalSpend = 0;
            foreach (DataRowView oRow in dvSpend) {
                // If this item has been reconciled, then use the vendor total, otherwise use the cost total
                if (Convert.ToDouble(oRow["VENDORTOTAL"]) == 0)
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["ACTUALTOTAL"]));
                else
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["VENDORTOTAL"]));
            }
            dTotalSpend = Convert.ToDouble(drC["TOTALSPENT"]);
            dAgentTotalSpent += dTotalSpend;
            dOfficeTotalSpent += dTotalSpend;
            dGrandTotalSpent += dTotalSpend;
            dMonthTotalSpent += dTotalSpend;
            dblPaid = 0;
            dblPrePaid = 0;

            foreach (DataRowView oRow in dvPaid) {
                dblPaid += Convert.ToDouble(oRow["TOTALPAID"]);
                DateTime dtPrePaidCutoff = Convert.ToDateTime(drC["LISTEDDATE"]).AddDays(G.Settings.PrePaymentNumberOfDays);

                if (Convert.ToDateTime(oRow["CONTRIBUTIONDATE"]) <= dtPrePaidCutoff && Convert.ToInt32(oRow["CONTRIBUTIONTYPEID"]) == (int)ContributionType.Vendor)
                    dblPrePaid += Convert.ToDouble(oRow["TOTALPAID"]);
            }
            dAgentTotalPrePaid += dblPrePaid;
            dOfficePrePaid += dblPrePaid;
            dGrandTotalPrePaid += dblPrePaid;
            dMonthTotalPrePaid += dblPrePaid;

            dAgentTotalPaid += dblPaid;
            dOfficeTotalPaid += dblPaid;
            dGrandTotalPaid += dblPaid;
            dMonthTotalPaid += dblPaid;

            if (dTotalSpend > 0)
                drC["% prepaid"] = dblPrePaid / dTotalSpend;
            drC["TOTALPAID"] = Utility.formatMoney(dblPaid);
            drC["TOTALPREPAID"] = Utility.formatMoney(dblPrePaid);

            drC["TOTALOWING"] = Utility.formatMoney(dTotalSpend - dblPaid);
            dtFinal.ImportRow(drC);
            oPrevRow = dtFinal.Rows[dtFinal.Rows.Count - 1];
            intMonthCount++;
            intAgentCount++;
            intOfficeCount++;
            intTotalCount++;
        }

        //Fill out the last agent/month details
        if (oPrevRow != null) {
            if (ShowMonthTotals) {
                oPrevRow["PROPERTYCOUNT"] = intMonthCount;
                if (dMonthTotalSpent > 0)
                    oPrevRow["AgentAverage"] = mfi.GetAbbreviatedMonthName(intCurrMonth) + " " + Convert.ToInt32((dMonthTotalPrePaid / dMonthTotalSpent) * 100) + "%";
            } else {
                DataRow drTotal = dtFinal.NewRow();
                drTotal["AGENT"] = szCurrAgent;

                drTotal["PROPERTYCOUNT"] = intAgentCount;
                if (dAgentTotalSpent > 0) {
                    drTotal["AgentAverage"] = Convert.ToInt32((dAgentTotalPrePaid / dAgentTotalSpent) * 100) + "%";
                    drTotal["AgentAveragePercent"] = Convert.ToInt32((dAgentTotalPrePaid / dAgentTotalSpent) * 100);
                    drTotal["TOTALROW"] = 1;
                }
                drTotal["TOTALPAID"] = dAgentTotalPaid;
                drTotal["TOTALSPENT"] = dAgentTotalSpent;
                drTotal["TOTALOWING"] = Utility.formatMoney(dAgentTotalSpent - dAgentTotalPaid);

                dtFinal.Rows.Add(drTotal);
            }
        }

        //Final office row
        DataRow drFinal = dtFinal.NewRow();
        drFinal["AGENT"] = "<b>" + szCurrOffice + " Totals</b>";
        drFinal["PROPERTYCOUNT"] = intOfficeCount;
        drFinal["TOTALSPENT"] = dOfficeTotalSpent;
        drFinal["TOTALPAID"] = dOfficeTotalPaid;

        if (dOfficeTotalSpent > 0)
            drFinal["AgentAverage"] = Convert.ToInt32((dOfficePrePaid / dOfficeTotalSpent) * 100) + "%";
        dtFinal.Rows.Add(drFinal);

        //Grand total row
        DataRow drFinal1 = dtFinal.NewRow();
        drFinal1["AGENT"] = "<b>Fletcher Totals</b>";
        drFinal1["PROPERTYCOUNT"] = intTotalCount;

        if (dGrandTotalSpent > 0)
            drFinal1["AgentAverage"] = Convert.ToInt32((dGrandTotalPrePaid / dGrandTotalSpent) * 100) + "%";

        drFinal1["TOTALSPENT"] = dGrandTotalSpent;
        drFinal1["TOTALPAID"] = dGrandTotalPaid;

        dtFinal.Rows.Add(drFinal1);

        dtFinal.AcceptChanges();

        Utility.dataTableToCSVFile(dtFinal);

        return dtFinal;
    }

    /// <summary>
    /// Loads the prepayment campaign information.
    /// </summary>
    /// <param name="szCampaignFilterSQL"></param>
    /// <returns></returns>
    public static DataTable loadPrePaymentCampaignData(string szCampaignFilterSQL, string CompanyIDList, String szCurrentPeriodFilter = null, string szUserFilter = null) {
        szCampaignFilterSQL += String.Format(" AND C.OFFICEID IN ({0})  ", G.Settings.CampaignTrackOffice);
        string szCompanyFilter = "";

        // Creates the extra field [PERIOD] if szCurrentPeriodFilter has been given
        string szPeriodField = String.IsNullOrWhiteSpace(szCurrentPeriodFilter) ? "" : String.Format(@"
    				CASE WHEN C.STARTDATE {0}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,", szCurrentPeriodFilter);

        if (!String.IsNullOrWhiteSpace(CompanyIDList)) {
            szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", CompanyIDList);
            CompanyIDList = "," + CompanyIDList.Replace(" ", "") + ",";
        }

        string szSQL = String.Format(@"
            -- 0
            SELECT C.ID, 0.00 AS TOTALSPENT, 0.00 AS TOTALINVOICED, 0.00 AS TOTALPAID, 0.00 AS TOTALOWING, 0.00 as TOTALPREPAID,
                0.00 AS [% PREPAID],
                -- User details taken from the follow possible locations trying top item first
                --     * CAPS user details linked with B&D data.
                --     * B&D user data.
                --     * CAPS user details linked with CT details.
                CASE WHEN BU.ID IS NULL THEN bak_U.ID
                     WHEN U.ID IS NULL THEN BU.ID*-1 -- B&D ID LISTED AS A NEGATIVE TO AVOID CONFLICT WITH CAPS IDs
                     ELSE U.ID END AS AGENTID,
                CASE WHEN BU.ID IS NULL THEN bak_U.INITIALSCODE
                     WHEN U.ID IS NULL THEN BU.INITIALS COLLATE Latin1_General_CI_AI
                     ELSE U.INITIALSCODE END AS AGENT,
                CASE WHEN BU.ID IS NULL THEN ISNULL(bak_U.FIRSTNAME,'') + ', ' + ISNULL(bak_U.LASTNAME,'')
                     WHEN U.ID IS NULL THEN ISNULL(BU.FIRSTNAME,'') + ', ' + ISNULL(BU.LASTNAME,'') COLLATE Latin1_General_CI_AI
                     ELSE ISNULL(U.FIRSTNAME,'') + ', ' + ISNULL(U.LASTNAME,'') END AS AGENTNAME,
                '' AS PROPERTYCOUNT, '' AS AGENTAVERAGE, 0 as AGENTAVERAGEPERCENT, 0 AS TOTALROW,
                ADDRESS1 + ' ' + ADDRESS2 AS ADDRESS, C.ORIGCAMPAIGNNUMBER AS CAMPAIGNNUMBER, C.STARTDATE AS LISTEDDATE, U.ROLEID,
                U.OFFICEID, ISNULL(L_OFFICE.NAME, '<unknown>') AS OFFICE, C.AUCTIONDATE, 0.0 AS TOTALSPLIT, 0.0 AS OFFICESPLIT, L_OFFICE.COMPANYID, {2}
                CASE
                    WHEN BU.ID IS NULL THEN 'CT'
                    WHEN U.ID IS NULL THEN 'BD'
                    ELSE 'OK' END AS SOURCE
            FROM CAMPAIGN C
            LEFT JOIN FLETCHERS_BOXDICEAPI.DBO.SALESLISTING SL ON SL.ID = C.BNDLISTINGID
            LEFT JOIN FLETCHERS_BOXDICEAPI.DBO.DB_USER BU ON BU.ID = SL.CONSULTANT1ID
            LEFT JOIN DB_USER U ON U.ID = (SELECT MIN(ID) FROM DB_USER U1 WHERE U1.ISACTIVE = 1 AND  BU.INITIALS = U1.INITIALSCODE COLLATE Latin1_General_CI_AS)
            LEFT JOIN DB_USER bak_U ON U.ID = C.AGENTID
            LEFT JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
            {0}
            AND C.ISDELETED = 0 AND C.STARTDATE IS NOT NULL AND U.SHOWONKPIREPORT = 1 {1} {3}
            UNION
            SELECT C.ID, 0.00 AS TOTALSPENT, 0.00 AS TOTALINVOICED, 0.00 AS TOTALPAID, 0.00 AS TOTALOWING, 0.00 as TOTALPREPAID,
                0.00 AS [% PREPAID], U.ID AS AGENTID, U.INITIALSCODE AS AGENT, ISNULL(U.FIRSTNAME,'') + ', ' +  ISNULL(U.LASTNAME,'') as AGENTNAME,
                '' AS PROPERTYCOUNT, '' AS AGENTAVERAGE, 0 as AGENTAVERAGEPERCENT, 0 AS TOTALROW,
                ADDRESS1 + ' ' + ADDRESS2 AS ADDRESS, C.ORIGCAMPAIGNNUMBER AS CAMPAIGNNUMBER, C.STARTDATE AS LISTEDDATE, U.ROLEID,
                U.OFFICEID, ISNULL(L_OFFICE.NAME, '<unknown>') AS OFFICE, C.AUCTIONDATE, 0.0 AS TOTALSPLIT, 0.0 AS OFFICESPLIT, L_OFFICE.COMPANYID, {2} 'OK' AS SOURCE
            FROM CAMPAIGN C
            LEFT JOIN FLETCHERS_BOXDICEAPI.DBO.SALESLISTING SL ON SL.ID = C.BNDLISTINGID
            LEFT JOIN FLETCHERS_BOXDICEAPI.DBO.DB_USER BU ON BU.ID = SL.CONSULTANT2ID
            LEFT JOIN DB_USER U ON U.ID = (SELECT MIN(ID) FROM DB_USER U1 WHERE U1.ISACTIVE = 1 AND BU.INITIALS = U1.INITIALSCODE COLLATE Latin1_General_CI_AS)
            LEFT JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
            {0}
            AND C.ISDELETED = 0 AND C.STARTDATE IS NOT NULL AND U.ID IS NOT NULL AND U.SHOWONKPIREPORT = 1 {1} {3}
            ORDER BY OFFICE, AGENT, STARTDATE DESC;

            -- 1 Total spend
            SELECT C.ID, SUM(ISNULL(CP.VENDORPRICE, 0)) AS VENDORTOTAL,  SUM(ISNULL(CP.COSTPRICE, 0)) AS COSTTOTAL,
                SUM(ISNULL(CP.ACTUALPRICE, 0)) AS ACTUALTOTAL
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND CP.ISDELETED = 0
            LEFT JOIN PRODUCT P ON P.ID = CP.PRODUCTID
            {0} AND C.ISDELETED = 0
            GROUP BY C.ID;

            -- 2 Payments
            SELECT C.ID, ISNULL(CC.AMOUNT, 0) AS TOTALPAID, CC.CONTRIBUTIONDATE, CC.CONTRIBUTIONTYPEID
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.ISPAYMENT = 1
            {0} AND C.ISDELETED = 0 AND CC.CONTRIBUTIONDATE IS NOT NULL;

            ", szCampaignFilterSQL, szCompanyFilter, szPeriodField, szUserFilter);

        DataSet dsCampaign = DB.runDataSet(szSQL);
        System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
        //Process the dataset now
        DataView dvSpend = dsCampaign.Tables[1].DefaultView;
        DataView dvPaid = dsCampaign.Tables[2].DefaultView;
        double dblPaid = 0;
        double dblPrePaid = 0;

        DataTable dtFinal = dsCampaign.Tables[0].Clone();

        int intOfficeID = -1;
        List<int> lCampaignOffice = null;
        List<int> lCampaignTotal = new List<int>();

        foreach (DataRow drC in dsCampaign.Tables[0].Rows) {
            if (DB.readInt(drC["OFFICEID"]) != intOfficeID) {
                intOfficeID = DB.readInt(drC["OFFICEID"]);
                lCampaignOffice = new List<int>();
            }

            DateTime dtSaleDate = Convert.ToDateTime(drC["LISTEDDATE"]);

            //Get the campaign product total from the product table
            dvSpend.RowFilter = "ID = " + drC["ID"].ToString();
            dvPaid.RowFilter = "ID = " + drC["ID"].ToString();
            double dTotalSpend = 0;
            foreach (DataRowView oRow in dvSpend) {
                // If this item has been reconciled, then use the vendor total, otherwise use the cost total
                if (Convert.ToDouble(oRow["VENDORTOTAL"]) == 0)
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["ACTUALTOTAL"]));
                else
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["VENDORTOTAL"]));
            }
            dTotalSpend = Convert.ToDouble(drC["TOTALSPENT"]);
            dblPaid = 0;
            dblPrePaid = 0;

            foreach (DataRowView oRow in dvPaid) {
                dblPaid += Convert.ToDouble(oRow["TOTALPAID"]);
                DateTime dtPrePaidCutoff = Convert.ToDateTime(drC["LISTEDDATE"]).AddDays(G.Settings.PrePaymentNumberOfDays);

                if (Convert.ToDateTime(oRow["CONTRIBUTIONDATE"]) <= dtPrePaidCutoff && Convert.ToInt32(oRow["CONTRIBUTIONTYPEID"]) == (int)ContributionType.Vendor)
                    dblPrePaid += Convert.ToDouble(oRow["TOTALPAID"]);
            }
            if (dTotalSpend > 0)
                drC["% prepaid"] = dblPrePaid / dTotalSpend;
            drC["TOTALPAID"] = Utility.formatMoney(dblPaid);
            drC["TOTALPREPAID"] = Utility.formatMoney(dblPrePaid);
            drC["TOTALOWING"] = Utility.formatMoney(dTotalSpend - dblPaid);

            // Check if value is a repeat in the office
            if (lCampaignOffice.Contains(DB.readInt(drC["ID"]))) {
                drC["OFFICESPLIT"] = 0f;
            } else {
                drC["OFFICESPLIT"] = 1f;
                lCampaignOffice.Add(DB.readInt(drC["ID"]));
            }

            // Check if the value is a repeat
            if (lCampaignTotal.Contains(DB.readInt(drC["ID"]))) {
                drC["TOTALSPLIT"] = 0f;
            } else {
                drC["TOTALSPLIT"] = 1f;
                lCampaignOffice.Add(DB.readInt(drC["ID"]));
            }
            dtFinal.ImportRow(drC);
        }

        dtFinal.AcceptChanges();

        Utility.dataTableToCSVFile(dtFinal);

        return dtFinal;
    }
}