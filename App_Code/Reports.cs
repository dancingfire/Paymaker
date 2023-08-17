using Microsoft.Reporting.WebForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// Contains the filters used to create the reports
/// </summary>
public class ReportFilters {
    private DateTime dtStart = DateTime.MaxValue;
    private DateTime dtEnd = DateTime.MaxValue;
    private string szUserList = "";
    private string szFY = "";
    private string szRoleID = "";
    private string szCompanyID = "";
    private string szPayPeriodID = "";
    
    public DateTime StartDate {
        get {
            if (dtStart == null || dtStart == DateTime.MinValue || dtStart == DateTime.MaxValue)
                dtStart = DateUtil.ThisFinYear(DateTime.Now).Start;
            return dtStart;
        }
        set { dtStart = value; }
    }

    public DateTime EndDate {
        get {
            if (dtEnd == null || dtEnd == DateTime.MinValue || dtEnd == DateTime.MaxValue)
                dtEnd = DateUtil.ThisFinYear(DateTime.Now).End;
            return dtEnd;
        }
        set { dtEnd = value; }
    }

    public string OfficeIDList { get; set; }
    public string OfficeNameList { get; set; }

    public string UserIDList {
        get { return szUserList; }
        set {
            if (value != "null")
                szUserList = value;
        }
    }

    public string UserNameList {
        get;
        set;
    }

    public string CompanyNames { get; set; }
    public string SuburbIDList { get; set; }
    public string ExpenseCategoryIDList { get; set; }
    public string OffTheTopCategoryIDList { get; set; }
    public bool ExcludeReferral { get; set; }
    public string FinancialYear {
        get { return szFY; }
        set {
            if (value != "")
                szFY = value;
        }
    }

    /// <summary>
    /// The number of months in the date range (only valid for monthly and quarterly reports)
    /// </summary>
    public int MonthsinPeriod{ get; set; }
    
    //REturns a single financial year
    public int getSingleFinancialYear() {
        if (szFY == "")
            return -1;
        else
            return Convert.ToInt32(szFY);
    }

    public string RoleID {
        get { return szRoleID; }
        set {
            if (value != "")
                szRoleID = value;
        }
    }

    public string CompanyID {
        get { return szCompanyID; }
        set {
            if (value != "")
                szCompanyID = value;
        }
    }

    public string PayPeriodID {
        get { return szPayPeriodID; }
        set {
            if (value != "")
                szPayPeriodID = value;
        }
    }

    /// <summary>
    /// Gets the start date in a universal format - if there's no date entered it will be the start of the curr financial year
    /// </summary>
    /// <returns></returns>
    public string getDBSafeStartDate() {
        String szStartDate = StartDate.Year + "-" + StartDate.Month.ToString().PadLeft(2, '0') + "-" + StartDate.Day.ToString().PadLeft(2, '0') + "T00:00:00";

        return szStartDate;
    }

    /// <summary>
    /// Gets the end date in a universal format - if there's no date entered it will be the end of the curr financial year
    /// </summary>
    /// <returns></returns>
    public string getDBSafeEndDate() {
        String szEndDate = EndDate.Year + "-" + EndDate.Month.ToString().PadLeft(2, '0') + "-" + EndDate.Day.ToString().PadLeft(2, '0') + "T23:59:59";
        return szEndDate;
    }

    /// <summary>
    /// loads the user entered values
    /// </summary>
    public void loadUserFilters() {
        ExpenseCategoryIDList = Valid.getText("szExpenseID", "", VT.TextNormal);
        if (ExpenseCategoryIDList == "null" || ExpenseCategoryIDList == "-1")
            ExpenseCategoryIDList = "";

        OffTheTopCategoryIDList = Valid.getText("szOffTheTopID", "", VT.TextNormal);
        if (OffTheTopCategoryIDList == "null" || OffTheTopCategoryIDList == "-1")
            OffTheTopCategoryIDList = "";

        szCompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
        CompanyNames = Valid.getText("szCompanyNames", "", VT.TextNormal);
        if (CompanyNames == "null")
            CompanyNames = "";

        OfficeIDList = Valid.getText("szOfficeID", "", VT.TextNormal);
      
        string szStartDate = Valid.getText("szStartDate", "", VT.TextNormal);
        string szEndDate = Valid.getText("szEndDate", "", VT.TextNormal);

        //Check if this is a monthly or quarterly report - override normal filter processing
        string szMonth = Valid.getText("szMonth", "", VT.TextNormal);
        string szQuarter = Valid.getText("szQuarter", "", VT.TextNormal);

        if (szQuarter != "") {
            string[] arDate = Utility.SplitByString(szQuarter.Replace("Between  '", ""), "' AND '");
            szStartDate = arDate[0];
            szEndDate = arDate[1].Replace("'", "");
            MonthsinPeriod = 3;
        }

        if (szMonth != "") {
            string[] arDate = Utility.SplitByString(szMonth.Replace("Between  '", ""), "' AND '");
            szStartDate = arDate[0];
            szEndDate = arDate[1].Replace("'", "");
            MonthsinPeriod = 1;
        }
        ExcludeReferral = Valid.getText("blnExcludeReferral", "") == "true";

        if (!String.IsNullOrEmpty(szStartDate)) {
            dtStart = DateTime.Parse(szStartDate);
            if (!String.IsNullOrEmpty(szEndDate))
                dtEnd = DateTime.Parse(szEndDate);
        }

        UserIDList = Valid.getText("szUserID", "", VT.List);
        if (UserIDList == "null")
            UserIDList = "";

        szPayPeriodID = Valid.getText("szPayPeriod", "", VT.List);
        if (szPayPeriodID == "null")
            szPayPeriodID = "";

        szFY = Valid.getText("szFinYear", "", VT.List);
        if (szFY == "null")
            szFY = "";
    }
}

/// <summary>
/// Helper functions for functionality shared between reports
/// </summary>
public static class ReportHelper {

    /// <summary>
    /// Process the sales count based on the data - Assumes the existence of OFFICECOUNT, LISTINGOFFICEID, OFFICEID
    /// </summary>
    /// <param name="?"></param>
    public static double processSalesCount(DataTable dt, string FinalValueColumn) {
        double dTotal = 0;
        foreach (DataRow dr in dt.Rows) {
            int intOfficeCount = Convert.ToInt32(dr["DISTINCTOFFICECOUNT"]);
            int intListingOffice = Convert.ToInt32(dr["LISTINGOFFICEID"]);
            double dSplitValue = 1.0 / intOfficeCount;
            if (intOfficeCount == 3 && intListingOffice == Convert.ToInt32(dr["OFFICEID"]))
                dSplitValue = 0.34;
            dTotal += dSplitValue;
            dr[FinalValueColumn] = dSplitValue;
        }
        return dTotal;
    }

    /// <summary>
    /// Updates the recordset with the correct listing months and auction dates
    /// </summary>
    /// <param name="ds"></param>
    /// <returns></returns>
    public static void processExtraData(ref DataSet ds, bool IsSuburbReport) {
        Hashtable htIDList = new Hashtable();

        string szIntegrityCol = "AGENTID";
        if (IsSuburbReport)
            szIntegrityCol = "SUBURB";
        Hashtable htIntegrityIDList = getIntegrityIDValues(ds, szIntegrityCol);
        Hashtable htValidMonthGroups = getIntegrityIDValues(ds, "MONTHGROUP");

        Boolean IsOfficeAgentReport = ds.Tables[0].Columns.Contains("PERIOD");

        //Add the listing data
        foreach (DataRow dr in ds.Tables[1].Rows) {
            DataRow dNew = ds.Tables[0].NewRow();
            string szIntegrityCheckID = Convert.ToString(dr[szIntegrityCol]);
            if (!htIntegrityIDList.ContainsKey(szIntegrityCheckID))
                continue; //We aren't reporting any data with this user
            string szMonthGroup = dr["MONTHGROUP"].ToString();
            if (!htValidMonthGroups.ContainsKey(szMonthGroup))
                continue; //The listing data isn't in the range of the report

            copyData(ref dNew, dr);
            dNew["LISTINGCOUNT"] = 1;

            if (IsOfficeAgentReport)
                dNew["PERIOD"] = dr["PERIOD"];

            ds.Tables[0].Rows.Add(dNew);

            if (!IsSuburbReport) {
                //Check to see if we have already counted this property as sold
                if (htIDList.ContainsKey(Convert.ToInt32(dr["ID"]))) {
                    dNew["ISSOLDCOMPANY"] = 0;
                    dNew["COMPANYSALEPRICE"] = 0;
                } else {
                    htIDList.Add(Convert.ToInt32(dr["ID"]), Convert.ToInt32(dr["ID"]));
                }
            }
        }
        //Add the auction data
        foreach (DataRow dr in ds.Tables[2].Rows) {
            string szIntegrityCheckID = Convert.ToString(dr[szIntegrityCol]);
            if (!htIntegrityIDList.ContainsKey(szIntegrityCheckID))
                continue; //We aren't reporting any data with this user
            string szMonthGroup = dr["MONTHGROUP"].ToString();
            if (!htValidMonthGroups.ContainsKey(szMonthGroup))
                continue; //The listing data isn't in the range of the report

            DataRow dNew = ds.Tables[0].NewRow();
            copyData(ref dNew, dr);
            dNew["AUCTIONCOUNT"] = 1;
            dNew["TOTALAUCTIONSOLD"] = dr["SOLDAUCTION"];

            if (IsOfficeAgentReport)
                dNew["PERIOD"] = dr["PERIOD"];

            ds.Tables[0].Rows.Add(dNew);
        }
        ds.Tables[0].AcceptChanges();
        foreach (DataRow dr in ds.Tables[0].Rows) {
            if (System.DBNull.Value != dr["PRIVATESALE"])
                dr["PRIVATESALEWITHOUTAUCTION"] = Convert.ToInt32(dr["PRIVATESALE"]);

            if (IsOfficeAgentReport && Convert.ToString(dr["PERIOD"]) == "Current") {
                dr["ISSOLD_current"] = dr["ISSOLD"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["ISSOLD"]);
                dr["COMMISSIONEARNED_current"] = dr["COMMISSIONEARNED"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["COMMISSIONEARNED"]);
                dr["COMMISSION_PERCENTAGE_current"] = dr["COMMISSION_PERCENTAGE"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["COMMISSION_PERCENTAGE"]);
                dr["PRIVATESALE_current"] = dr["PRIVATESALE"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["PRIVATESALE"]);
                dr["AUCTIONCOUNT_current"] = dr["AUCTIONCOUNT"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["AUCTIONCOUNT"]);
                dr["SALEPRICE_current"] = dr["SALEPRICE"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["SALEPRICE"]);
                dr["TOTALAUCTIONSOLD_current"] = dr["TOTALAUCTIONSOLD"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["TOTALAUCTIONSOLD"]);
                dr["PRIVATESALEWITHOUTAUCTION_current"] = dr["PRIVATESALEWITHOUTAUCTION"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["PRIVATESALEWITHOUTAUCTION"]);
                dr["DAYS_TO_PRIVATE_SELL_current"] = dr["DAYS_TO_PRIVATE_SELL"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["DAYS_TO_PRIVATE_SELL"]);
                dr["SOLDATAUCTION_current"] = dr["SOLDATAUCTION"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["SOLDATAUCTION"]);
                dr["DAYS_TO_AUCTION_SELL_current"] = dr["DAYS_TO_AUCTION_SELL"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["DAYS_TO_AUCTION_SELL"]);
                dr["ISWITHDRAWN_current"] = dr["ISWITHDRAWN"] == System.DBNull.Value ? 0.0 : Convert.ToDouble(dr["ISWITHDRAWN"]);
                dr["LISTEDDATE_current"] = dr["LISTEDDATE"];
            }
        }
    }

    private static void copyData(ref DataRow dNew, DataRow drSource) {
        dNew["OFFICEID"] = drSource["OFFICEID"];
        dNew["AGENTID"] = drSource["AGENTID"];
        dNew["AGENTNAME"] = drSource["AGENTNAME"];
        dNew["MONTHGROUP"] = drSource["MONTHGROUP"];
        dNew["MONTHSORT"] = drSource["MONTHSORT"];
        dNew["SUBURB"] = drSource["SUBURB"];
        dNew["OFFICENAME"] = drSource["OFFICENAME"];
        dNew["DAYS_PASSED_IN_AUCTION"] = drSource["DAYS_PASSED_IN_AUCTION"];
        dNew["PASSEDINAUCTION"] = drSource["PASSEDINAUCTION"];
        dNew["AUCTIONCOUNT"] = 0;
        dNew["TOTALAUCTIONSOLD"] = 0;
        dNew["PRIVATESALE"] = 0;
        dNew["DAYS_TO_PRIVATE_SELL"] = 0;
        dNew["LISTINGCOUNT"] = 0;
        dNew["PRIVATESALE"] = 0;
        dNew["AUCTIONSALE"] = 0;
        dNew["FINYEAR"] = 1;
    }

    /// <summary>
    /// Returns the distinct values in a column in the dataset
    /// </summary>
    /// <param name="ds"></param>
    /// <param name="IntegrityColName"></param>
    /// <returns></returns>
    private static Hashtable getIntegrityIDValues(DataSet ds, string IntegrityColName) {
        Hashtable ht = new Hashtable();
        foreach (DataRow dr in ds.Tables[0].Rows) {
            if (ht.ContainsKey(Convert.ToString(dr[IntegrityColName])))
                continue;
            ht.Add(Convert.ToString(dr[IntegrityColName]), Convert.ToString(dr[IntegrityColName]));
        }
        return ht;
    }

    /// <summary>
    /// Copies the specified datatable
    /// </summary>
    /// <param name="ds"></param>
    /// <param name="TableToCopy"></param>
    public static void copyDataTable(ref DataSet ds, int TableToCopy) {
        DataTable DataTable2 = new DataTable();
        DataTable2 = ds.Tables[TableToCopy].Clone();
        DataTable2.TableName = "DataTable" + ds.Tables.Count + 1;
        foreach (DataRow dr in ds.Tables[TableToCopy].Rows) {
            DataTable2.ImportRow(dr);
        }
        ds.Tables.Add(DataTable2);
    }
}

#region KPI

public class KPI_Company : Report {
    private static string RFileName = "KPI by Company";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public KPI_Company(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "KPI by company";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szFilter = String.Format(" ((S.SALEDATE BETWEEN '{0}' AND '{1}') OR (S.SALEDATE IS NULL AND S.FALLENTHROUGHDATE BETWEEN '{0}' AND '{1}'))", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        string szUserFilter = "";
        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList)) {
            szUserFilter += (" AND USR.OFFICEID IN (" + oFilter.OfficeIDList + ")");
        }
        string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
        if (!String.IsNullOrWhiteSpace(szCompanyIDList)) {
            szUserFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);
        }

        if (!String.IsNullOrWhiteSpace(szCompanyIDList)) {
            szUserFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);
        }

        if (!String.IsNullOrWhiteSpace(oFilter.UserIDList)) {
            szUserFilter += String.Format(" AND USR.ID IN ({0})", oFilter.UserIDList);
        }
        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        if (!blnIncludeInactive)
            szUserFilter += " AND USR.ISACTIVE = 1 ";

        string szSQL = string.Format(@"
                SELECT DISTINCT
				    CONVERT(VARCHAR(7), ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 120) AS MONTHSORT,
					USS.USERID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                    SUBSTRING (CONVERT(VARCHAR, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 100),1,3) +'-'+ SUBSTRING (CONVERT(VARCHAR, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 100),8,4 ) AS MONTHGROUP,
                 	USR.OFFICEID, '' AS SUBURB,
                    CASE
                        WHEN S.FALLENTHROUGHDATE IS NOT NULL OR AUCTIONDATE IS NOT NULL THEN 0
			            ELSE 1
		            END AS PRIVATESALE,
                    CAST(ISAUCTION as INT) AS ISAUCTION,
		            CAST(ISAUCTION AS INT) AS AUCTIONSALE,
                    CASE
			         WHEN S.FALLENTHROUGHDATE IS NOT NULL OR AUCTIONDATE IS NOT NULL THEN 0
			            ELSE  DATEDIFF(DAY, S.LISTEDDATE, S.SALEDATE)
		            END AS DAYS_TO_PRIVATE_SELL,
                    0 AS DAYS_PASSED_IN_AUCTION,
                    0 AS PASSEDINAUCTION,
					ISNULL(L_OFFICE.NAME,'') as OFFICENAME,
                    S.ID,
		            CASE
			            WHEN S.SALEPRICE > 0 THEN S.GROSSCOMMISSION/S.SALEPRICE
			            ELSE 0
		            END as COMMISSION_PERCENTAGE,
		            ISNULL(S.GROSSCOMMISSION,0) AS COMMISSIONEARNED,
		            ISNULL(S.GROSSCOMMISSION,0) AS COMMISSION_DOLLARS,
		            CASE
			            WHEN S.SALEPRICE > 0 THEN S.ADVERTISINGSPEND/S.SALEPRICE
			            ELSE 0
		            END AS ADVERTISING_PERCENTAGE,
		            ISNULL(S.ADVERTISINGSPEND,0) AS ADVERTISING_DOLLARS,
		            ISNULL(S.SALEPRICE,0) AS SALEPRICE,
		            ISNULL(S.SALEPRICE,0) AS COMPANYSALEPRICE,
		            S.SALEDATE,
                    SUBSTRING(CONVERT(VARCHAR(11), LISTEDDATE, 113), 4, 8) AS LISTINGMONTHGROUP,
		            S.LISTEDDATE,
		            S.FALLENTHROUGHDATE as WITHDRAWNDATE,
                    CASE WHEN S.FALLENTHROUGHDATE IS NULL THEN 0 ELSE 1 END AS ISWITHDRAWN,
                    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLD,
				    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLDCOMPANY,
		            S.AUCTIONDATE,
                    CASE WHEN S.AUCTIONDATE > S.SALEDATE THEN 1 ELSE 0 END AS SOLDBEFORE,
                    0 AS LISTINGCOUNT, 0 AS AUCTIONCOUNT, 0 AS TOTALAUCTIONSOLD, 1 AS FINYEAR,
                    0 AS PRIVATESALEWITHOUTAUCTION
                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                            JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                            JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                            JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                            JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                            JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE {0} {1}
                    AND USS.INCLUDEINKPI = 1
                ORDER BY OFFICENAME, MONTHSORT, AGENTNAME;

                SELECT DISTINCT  S.ID,
					USS.USERID AS AGENTID,  ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME, '' AS SUBURB,
                    USR.OFFICEID, L_OFFICE.NAME AS OFFICENAME,
					SUBSTRING (CONVERT(VARCHAR, ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 100),1,3) +'-'+
                        SUBSTRING (CONVERT(VARCHAR, ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 100),8,4 ) AS MONTHGROUP,
                    CONVERT(VARCHAR(7), ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 120) AS MONTHSORT,
                    0 AS DAYS_PASSED_IN_AUCTION, 0 AS PASSEDINAUCTION

                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                        JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                        JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                        JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                        JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                        JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE {2} {1}
                    AND USS.INCLUDEINKPI = 1;

                -- 2 Auction count data
                SELECT DISTINCT   S.ID, S.Address, AUCTIONDATE, SALEDATE,  USS.USERID AS AGENTID, USR.OFFICEID, L_OFFICE.NAME AS OFFICENAME,
                SUBSTRING (CONVERT(VARCHAR, S.AUCTIONDATE, 100),1,3) +'-'+ SUBSTRING (CONVERT(VARCHAR, S.AUCTIONDATE, 100),8,4 ) AS MONTHGROUP,
                CONVERT(VARCHAR(7), S.AUCTIONDATE, 120) AS MONTHSORT, ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                CASE WHEN SALEDATE <= AUCTIONDATE THEN 1 ELSE 0 END AS SOLDAUCTION, '' AS SUBURB,
                CASE
                        WHEN DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE) > 1
                        THEN
	                       DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE)
	                    ELSE 0
                    END AS DAYS_PASSED_IN_AUCTION,
                CASE WHEN DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE) <= 1 THEN 0 ELSE 1 END AS PASSEDINAUCTION
                FROM
                SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                    JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE {3} {1} AND S.FALLENTHROUGHDATE IS  NULL AND AUCTIONDATE IS NOT NULL
                AND USS.INCLUDEINKPI = 1
                "
            , szFilter, szUserFilter, szFilter.Replace("S.SALEDATE", "S.LISTEDDATE"), szFilter.Replace("S.SALEDATE", "S.AUCTIONDATE"));
        DataSet ds = DB.runDataSet(szSQL);
        ReportHelper.processExtraData(ref ds, false);
        return ds;
    }
}

public class KPI_Agent : Report {
    private static string RFileName = "KPI Agent";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public KPI_Agent(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "KPI Agent Detail";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szFilter = String.Format(" ((S.SALEDATE BETWEEN '{0}' AND '{1}') OR (S.SALEDATE IS NULL AND S.FALLENTHROUGHDATE BETWEEN '{0}' AND '{1}'))", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        string szUserIDFilter = Valid.getText("szUserID", "", VT.List);
        if (szUserIDFilter == "null")
            szUserIDFilter = "";
        if (!String.IsNullOrWhiteSpace(szUserIDFilter)) {
            szFilter += (" AND USS.USERID IN (" + szUserIDFilter + ")");
        }

        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        string szUserActive = " AND USR.ISACTIVE = 1 ";
        if (blnIncludeInactive)
            szUserActive = "";

        string szSQL = string.Format(@"
                SELECT DISTINCT
					USS.USERID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' + ISNULL(USR.LASTNAME,'') as AGENTNAME,
					USR.OFFICEID,
					ISNULL(L_OFFICE.NAME,'') as OFFICENAME,
					S.ID,
					ISNULL(S.ADDRESS,'') AS ADDRESS,
					ISNULL(S.SUBURB,'') AS SUBURB,
					S.LISTEDDATE,
					CASE
						WHEN S.ISAUCTION = 0 THEN 'Private Sale'
						WHEN S.ISAUCTION = 1 THEN
							CASE
								WHEN S.AUCTIONDATE > S.SALEDATE
								THEN 'Before Auction'
								WHEN S.AUCTIONDATE < S.SALEDATE
								THEN 'Sold After Auction'
								ELSE 'Auction'
							END
						ELSE 'ERROR'
					END AS METHODOFSALE,
                    case WHEN S.ISAUCTION = 0 AND S.AUCTIONDATE IS NOT NULL AND S.AUCTIONDATE <= S.SALEDATE THEN S.AUCTIONDATE ELSE NULL END AS PASSEDINDATE,
					S.AUCTIONDATE as PASSEDINDATE,
					S.AUCTIONDATE,
					S.FALLENTHROUGHDATE as WITHDRAWNDATE,
                    CASE WHEN S.FALLENTHROUGHDATE IS NULL THEN 0 ELSE 1 END AS ISFALLENTHROUGH,
                    CASE WHEN MONTH(LISTEDDATE) = MONTH(SALEDATE) THEN 1 ELSE 0 END AS LISTINGCOUNT,
                    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLD,
					S.SALEDATE as SOLDDATE,
					S.SALEPRICE,
					S.PURCHASERSUBURB as SUBURBBUYERFROM,
					ISNULL(S.GROSSCOMMISSION,0) as COMMISSIONEARNED,
					ISNULL(S.ADVERTISINGSPEND,0) as MARKETINGEXPENDED
				FROM
					SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
						JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID  AND USS.RECORDSTATUS < 1
						JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
						JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
						JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
						JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID
                WHERE
                        {0} {1}
                     AND USS.INCLUDEINKPI = 1
                ORDER BY OFFICENAME, AGENTNAME
                "
            , szFilter, szUserActive);
        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}

public class KPI_Suburb : Report {
    private static string RFileName = "KPI by Suburb";

    private bool GroupByMunicipality {
        get;
        set;
    }

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public KPI_Suburb(ReportViewer Viewer, ReportFilters oFilter, bool GroupByMunicipality = false) {
        oViewer = Viewer;
        this.GroupByMunicipality = GroupByMunicipality;
        this.oFilter = oFilter;
        if (GroupByMunicipality) {
            ReportTitle = "KPI by municipality";
        } else {
            ReportTitle = "KPI by suburb";
        }
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szFilter = String.Format(" ((S.SALEDATE BETWEEN '{0}' AND '{1}') OR (S.SALEDATE IS NULL AND S.FALLENTHROUGHDATE BETWEEN '{0}' AND '{1}'))", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        string szUserFilter = "";
        string strGroup = "ISNULL(S.SUBURB,'') ";

        if (GroupByMunicipality) {
            strGroup = "ISNULL(M.NAME,'<no match>') ";
        }

        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList)) {
            szUserFilter += " AND USR.OFFICEID IN (" + oFilter.OfficeIDList + ")";
        }
        string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
        if (!String.IsNullOrWhiteSpace(szCompanyIDList)) {
            szUserFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);
        }
        if (!String.IsNullOrWhiteSpace(oFilter.SuburbIDList)) {
            szUserFilter += String.Format(" AND S.SUBURB IN ('{0}')", oFilter.SuburbIDList);
        }
        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        if (!blnIncludeInactive)
            szUserFilter += " AND USR.ISACTIVE = 1 ";

        string szSQL = string.Format(@"
                SELECT DISTINCT
					USS.USERID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                    SUBSTRING (CONVERT(VARCHAR, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 100),1,3) +'-'+ SUBSTRING (CONVERT(VARCHAR, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 100),8,4 ) AS MONTHGROUP,
                    CONVERT(VARCHAR(7), ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 120) AS MONTHSORT,
					USR.OFFICEID,
					ISNULL(L_OFFICE.NAME,'') as OFFICENAME,
					{0} AS SUBURB,
                    S.ID,
		            CASE
			            WHEN S.SALEPRICE > 0 THEN S.GROSSCOMMISSION/S.SALEPRICE
			            ELSE 0
		            END as COMMISSION_PERCENTAGE,
		            ISNULL(S.GROSSCOMMISSION,0) AS COMMISSIONEARNED,
		            CASE
			            WHEN S.SALEPRICE > 0 THEN S.ADVERTISINGSPEND/S.SALEPRICE
			            ELSE 0
		            END AS ADVERTISING_PERCENTAGE,
		            ISNULL(S.ADVERTISINGSPEND,0) AS ADVERTISING_DOLLARS,
		            ISNULL(S.SALEPRICE,0) AS SALEPRICE,
		            ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE) AS SALEDATE,
                    S.LISTEDDATE,
		            S.FALLENTHROUGHDATE as WITHDRAWNDATE,
                    0 AS PASSEDINAUCTION,

		            CASE
			            WHEN S.ISAUCTION = 0 THEN 0
			            WHEN S.ISAUCTION = 1 THEN 1
                        ELSE 0
                    END AS ISAUCTION,
		            CAST(S.ISAUCTION AS INT) AS AUCTIONSALE,
                    CASE WHEN S.AUCTIONDATE > S.SALEDATE THEN 1 ELSE 0 END AS SOLDBEFORE,
		            CASE
                        WHEN S.FALLENTHROUGHDATE IS NOT NULL OR AUCTIONDATE IS NOT NULL THEN 0
			            ELSE 1
		            END AS PRIVATESALE,
		            S.AUCTIONDATE,
                    0 AS DAYS_PASSED_IN_AUCTION,
		            CASE
			            WHEN S.ISAUCTION = 0 THEN DATEDIFF(DAY, S.LISTEDDATE, S.SALEDATE)
			            ELSE 0
		            END AS DAYS_TO_PRIVATE_SELL,
                    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLD,
				    0 AS LISTINGCOUNT,
                    CASE WHEN S.FALLENTHROUGHDATE IS NULL THEN 0 ELSE 1 END AS ISWITHDRAWN,
                    0 AS AUCTIONCOUNT,
                    0 AS TOTALAUCTIONSOLD,
                    1 AS FINYEAR,
                    0 AS PRIVATESALEWITHOUTAUCTION
                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID  AND SS.RECORDSTATUS = 0
                        JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                        JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                        JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                        JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                        JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                        LEFT JOIN SUBURB SUB ON SUB.NAME = S.SUBURB
						LEFT JOIN MUNICIPALITY M ON M.ID = SUB.MUNICIPALITYID
                WHERE
                        {1} {2}
                    AND USS.INCLUDEINKPI = 1;

                SELECT DISTINCT {0} AS SUBURB,  S.ID, USS.USERID AS AGENTID,  ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                    USR.OFFICEID, L_OFFICE.NAME AS OFFICENAME,
					SUBSTRING (CONVERT(VARCHAR, ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 100),1,3) +'-'
                        + SUBSTRING (CONVERT(VARCHAR,ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 100),8,4 ) AS MONTHGROUP,
                    CONVERT(VARCHAR(7), ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 120) AS MONTHSORT,
                    0 AS DAYS_PASSED_IN_AUCTION, 0 AS PASSEDINAUCTION

                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                        JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                        JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                        JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                        JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                        JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                        LEFT JOIN SUBURB SUB ON SUB.NAME = S.SUBURB
						LEFT JOIN MUNICIPALITY M ON M.ID = SUB.MUNICIPALITYID
                WHERE {3} {2} AND USS.INCLUDEINKPI = 1 ;

                SELECT DISTINCT {0} AS SUBURB, S.ID, S.Address, AUCTIONDATE, SALEDATE,  USS.USERID AS AGENTID, USR.OFFICEID, L_OFFICE.NAME AS OFFICENAME,
                SUBSTRING (CONVERT(VARCHAR, S.AUCTIONDATE, 100),1,3) +'-'+ SUBSTRING (CONVERT(VARCHAR, S.AUCTIONDATE, 100),8,4 ) AS MONTHGROUP,
                CONVERT(VARCHAR(7), S.AUCTIONDATE, 120) AS MONTHSORT, ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                CASE WHEN SALEDATE <= AUCTIONDATE THEN 1 ELSE 0 END AS SOLDAUCTION,
                CASE
                        WHEN DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE) > 1
                        THEN
	                       DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE)
	                    ELSE 0
                    END AS DAYS_PASSED_IN_AUCTION,
                CASE WHEN DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE) <= 1 THEN 0 ELSE 1 END AS PASSEDINAUCTION
                FROM
                SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                    JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                    LEFT JOIN SUBURB SUB ON SUB.NAME = S.SUBURB
						 LEFT JOIN MUNICIPALITY M ON M.ID = SUB.MUNICIPALITYID
                WHERE {4} {2} AND S.FALLENTHROUGHDATE IS  NULL AND AUCTIONDATE IS NOT NULL  AND USS.INCLUDEINKPI = 1

                "
            , strGroup, szFilter, szUserFilter, szFilter.Replace("S.SALEDATE", "S.LISTEDDATE"),
            szFilter.Replace("S.SALEDATE", "S.AUCTIONDATE"));
        DataSet ds = DB.runDataSet(szSQL);
        ReportHelper.processExtraData(ref ds, true);
        return ds;
    }
}

public class KPI_Office_agents_auction_detail : KPI_Office_agents {
    private static string RFileName = "KPI Office agents - Auction details";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public KPI_Office_agents_auction_detail(ReportViewer Viewer, ReportFilters oFilter) : base(Viewer, oFilter) {
        ReportTitle = "KPI by Agent (auction details)";
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }
}

public class KPI_Office_agents : Report {
    private static string RFileName = "KPI Office agents";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public KPI_Office_agents(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "KPI by Agent";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    /// <summary>
    /// If dtStart / dtFinish is a single period gets the start date of the 'previous' equivalent lenth period.
    /// </summary>
    /// <param name="dtStart"></param>
    /// <param name="dtFinish"></param>
    /// <returns></returns>
    private DateTime getPreviousPeriodStart(DateTime dtStart, DateTime dtFinish) {
        // Start to End dates represent whole months eg. 15 Jun to 14 Jul or 1 Jun to 30 Jun
        Boolean blWholeMonths = dtStart.Day == dtFinish.AddDays(1).Day;

        // calculate by number of days
        if (!blWholeMonths)
            return (dtStart.Date - (dtFinish.Date - dtStart.Date)).AddDays(-1);

        // calculate number of months timespan
        int MonthCount = (dtFinish.Year - dtStart.Year) * 12 + dtFinish.Month - dtStart.Month + 1;

        return dtStart.AddMonths(-MonthCount);
    }

    private String szCurrentPeriodFilter;
    private String szPreviousAndCurrentPeriodFilter;

    public override DataSet getData() {
        szCurrentPeriodFilter = String.Format("BETWEEN '{0}' AND '{1}'", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        DateTime dtPreviousPeriodEnd = oFilter.StartDate.AddSeconds(-1);

        string szPreviousPeriodEnd = dtPreviousPeriodEnd.Year + "-" + dtPreviousPeriodEnd.Month.ToString().PadLeft(2, '0') + "-" + dtPreviousPeriodEnd.Day.ToString().PadLeft(2, '0') + "T23:59:59";

        // Report takes in previous period - StartDate Changed to take in two equal periods of time (previous period & current period)
        oFilter.StartDate = getPreviousPeriodStart(oFilter.StartDate, oFilter.EndDate);

        szPreviousAndCurrentPeriodFilter = String.Format("BETWEEN '{0}' AND '{1}'", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        string szFilter = String.Format(" ((S.SALEDATE BETWEEN '{0}' AND '{1}') OR (S.SALEDATE IS NULL AND S.FALLENTHROUGHDATE BETWEEN '{0}' AND '{1}'))", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        string szYTDFilter = String.Format(" (S_.SALEDATE BETWEEN '{0}' AND '{1}')", Utility.formatDate(Utility.getFinYearStart(oFilter.EndDate)), Utility.formatDate(Utility.getFinYearEnd(oFilter.EndDate)));
        string szUserFilter = "";
        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList)) {
            szUserFilter += (" AND USR.OFFICEID IN (" + oFilter.OfficeIDList + ")");
        }
        string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
        if (!String.IsNullOrWhiteSpace(szCompanyIDList)) {
            szUserFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);
        }

        if (!String.IsNullOrWhiteSpace(oFilter.UserIDList)) {
            szUserFilter += String.Format(" AND USR.ID IN ({0})", oFilter.UserIDList);
        }
        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        if (!blnIncludeInactive)
            szUserFilter += " AND USR.ISACTIVE = 1 ";

        string szSQL = string.Format(@"
                SELECT DISTINCT
				    CONVERT(VARCHAR(7), ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 120) AS MONTHSORT,
					USS.USERID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                    S.ID AS SALEID, S.ADDRESS AS SALEADDRESS,
                    SUBSTRING (CONVERT(VARCHAR, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 100),1,3) +'-'+ SUBSTRING (CONVERT(VARCHAR, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE), 100),8,4 ) AS MONTHGROUP,
                 	USR.OFFICEID, '' AS SUBURB,
                    CASE
			            WHEN S.FALLENTHROUGHDATE IS NOT NULL OR S.ISAUCTION = 1 OR S.AUCTIONDATE IS NOT NULL OR S.LISTEDDATE IS NULL OR S.SALEDATE IS NULL THEN 0
			            ELSE 1
		            END AS PRIVATESALE,
                    CASE
			            WHEN S.FALLENTHROUGHDATE IS NOT NULL OR S.ISAUCTION = 1 OR S.AUCTIONDATE IS NOT NULL OR S.LISTEDDATE IS NULL OR S.SALEDATE IS NULL THEN 0
			            ELSE  DATEDIFF(DAY, S.LISTEDDATE, S.SALEDATE)
		            END AS DAYS_TO_PRIVATE_SELL,
                    CASE
                        WHEN S.FALLENTHROUGHDATE IS NOT NULL OR ISAUCTION = 0 OR S.LISTEDDATE IS NULL OR S.SALEDATE IS NULL THEN 0
			            ELSE 1
		            END AS SOLDATAUCTION,
					CASE
						WHEN S.FALLENTHROUGHDATE IS NOT NULL OR ISAUCTION = 0 OR S.LISTEDDATE IS NULL OR S.SALEDATE IS NULL THEN 0
			            ELSE DATEDIFF(DAY, ISNULL(C.STARTDATE, S.LISTEDDATE), S.SALEDATE)
		            END AS DAYS_TO_AUCTION_SELL,
                    CAST(ISAUCTION as INT) AS ISAUCTION,
		            CAST(ISAUCTION AS INT) AS AUCTIONSALE,
                    0 AS DAYS_PASSED_IN_AUCTION,
                    0 AS PASSEDINAUCTION,
					ISNULL(L_OFFICE.NAME,'') as OFFICENAME,
                    S.ID,
		            CASE
			            WHEN S.SALEPRICE > 0 THEN S.GROSSCOMMISSION/S.SALEPRICE
			            ELSE 0
		            END as COMMISSION_PERCENTAGE,
		            ISNULL(S.GROSSCOMMISSION,0) AS COMMISSIONEARNED,
		            CASE
			            WHEN S.SALEPRICE > 0 THEN S.ADVERTISINGSPEND/S.SALEPRICE
			            ELSE 0
		            END AS ADVERTISING_PERCENTAGE,
		            ISNULL(S.ADVERTISINGSPEND,0) AS ADVERTISING_DOLLARS,
		            ISNULL(S.SALEPRICE,0) AS SALEPRICE,
		            ISNULL(S.SALEPRICE,0) AS COMPANYSALEPRICE,
		            S.SALEDATE,
                    SUBSTRING(CONVERT(VARCHAR(11), LISTEDDATE, 113), 4, 8) AS LISTINGMONTHGROUP,
		            S.LISTEDDATE,
		            S.FALLENTHROUGHDATE as WITHDRAWNDATE,
                    CASE WHEN S.FALLENTHROUGHDATE IS NULL THEN 0 ELSE 1 END AS ISWITHDRAWN,
                    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLD,
				    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLDCOMPANY,
		            S.AUCTIONDATE,
                    CASE WHEN S.AUCTIONDATE > S.SALEDATE THEN 1 ELSE 0 END AS SOLDBEFORE,
                    0 AS LISTINGCOUNT, 0 AS AUCTIONCOUNT, 0 AS TOTALAUCTIONSOLD, 1 AS FINYEAR,
                    0 AS PRIVATESALEWITHOUTAUCTION,
    				CASE WHEN ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE) {4}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,
    				0 AS ISSOLD_current,
    				0.0 AS COMMISSIONEARNED_current,
    				0.0 AS COMMISSION_PERCENTAGE_current,
                    0 AS PRIVATESALE_current,
                    0 AS AUCTIONCOUNT_current,
                    0.0 AS SALEPRICE_current,
                    0 AS TOTALAUCTIONSOLD_current,
                    0 AS PRIVATESALEWITHOUTAUCTION_current,
                    0 AS DAYS_TO_PRIVATE_SELL_current,
                    0 AS SOLDATAUCTION_current,
                    0 AS DAYS_TO_AUCTION_SELL_current,
                    0 AS ISWITHDRAWN_current,
                    CAST('1800-01-01' AS date) AS LISTEDDATE_current
                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                            JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                            JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                            JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                            JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                            JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                            LEFT Join CAMPAIGN C on S.BnDSALEID = C.BnDLISTINGID
                WHERE {0} {1}
                    AND USS.INCLUDEINKPI = 1 AND USR.SHOWONKPIREPORT = 1

                -- 1
                SELECT DISTINCT  S.ID,
					USS.USERID AS AGENTID,  ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME, '' AS SUBURB,
                    USR.OFFICEID, L_OFFICE.NAME AS OFFICENAME,
					SUBSTRING (CONVERT(VARCHAR, ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 100),1,3) +'-'+
                        SUBSTRING (CONVERT(VARCHAR, ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 100),8,4 ) AS MONTHGROUP,
                    CONVERT(VARCHAR(7), ISNULL(S.LISTEDDATE, S.FALLENTHROUGHDATE), 120) AS MONTHSORT,
                    0 AS DAYS_PASSED_IN_AUCTION, 0 AS PASSEDINAUCTION,
    				CASE WHEN ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE) {4}
						THEN 'Current' ELSE 'Previous' END AS PERIOD
                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                        JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                        JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                        JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                        JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                        JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE {2} {1}
                    AND USS.INCLUDEINKPI = 1;

                -- 2 Auction count data
                SELECT DISTINCT   S.ID, S.Address, AUCTIONDATE, SALEDATE,  USS.USERID AS AGENTID, USR.OFFICEID, L_OFFICE.NAME AS OFFICENAME,
                SUBSTRING (CONVERT(VARCHAR, S.AUCTIONDATE, 100),1,3) +'-'+ SUBSTRING (CONVERT(VARCHAR, S.AUCTIONDATE, 100),8,4 ) AS MONTHGROUP,
                CONVERT(VARCHAR(7), S.AUCTIONDATE, 120) AS MONTHSORT, ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                CASE WHEN SALEDATE <= AUCTIONDATE THEN 1 ELSE 0 END AS SOLDAUCTION, '' AS SUBURB,
                CASE
                        WHEN DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE) > 1
                        THEN
	                       DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE)
	                    ELSE 0
                    END AS DAYS_PASSED_IN_AUCTION,
                CASE WHEN DATEDIFF(DAY, S.AUCTIONDATE, S.SALEDATE) <= 1 THEN 0 ELSE 1 END AS PASSEDINAUCTION,
    			CASE WHEN ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE) {4}
					THEN 'Current' ELSE 'Previous' END AS PERIOD
                FROM
                SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                    JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE {3} {1} AND S.FALLENTHROUGHDATE IS  NULL AND AUCTIONDATE IS NOT NULL
                AND USS.INCLUDEINKPI = 1 AND USR.SHOWONKPIREPORT = 1;

                -- 3 B&D Data Listing count
                SELECT
				    CONVERT(VARCHAR(7), LISTEDDATE, 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					CASE WHEN LISTEDDATE {4}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,
					CASE WHEN LISTEDDATE {6} THEN 1 ELSE 0 END AS LISTING_COUNT,
					CASE WHEN LISTEDDATE {4} THEN 1 ELSE 0 END AS LISTING_COUNT_current
				FROM bdSALESLISTING SL
					JOIN bdPROPERTY P ON P.ID = SL.PROPERTYID
					LEFT JOIN bdCONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
					JOIN bdCONSULTANT U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1 AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE (LISTEDDATE {6} OR STARTDATE {6}) {1} AND USR.SHOWONKPIREPORT = 1
				GROUP BY SL.CONSULTANT1ID, USR.ID, USR.OFFICEID, USR.FIRSTNAME, USR.LASTNAME, SL.ID, P.ADDRESS,STATUS, LISTEDDATE;

                -- 4. B&D Data Appraisal count
				WITH APPRAISALS AS (
				  SELECT
						SL.ID AS LISTINGID,
						ISNULL(MIN(CA_APPR.STARTDATE),SL.LISTEDDATE) AS DATE,
						MAX(T.ACTIONON) AS FOLLOWUPDATE,
						CASE
							WHEN ISNULL(MIN(CA_APPR.STARTDATE), LISTEDDATE) {4} THEN 'Current'
							WHEN ISNULL(MIN(CA_APPR.STARTDATE), LISTEDDATE) {6} THEN 'Previous'
							ELSE NULL END AS PERIOD,
						MIN(USR.ID) AS AGENTID,
						MIN(LS.NAME) AS LISTING_SOURCE,
						MIN(SL.PROPERTYID) AS PROPERTYID
					FROM bdSALESLISTING SL
						LEFT JOIN bdLISTINGSOURCE LS ON LS.ID = SL.LISTINGSOURCEID
						JOIN bdPROPERTY P ON P.ID = SL.PROPERTYID
						LEFT JOIN bdCONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
						JOIN bdCONSULTANT U ON U.ID = SL.CONSULTANT1ID
						JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
						LEFT JOIN bdTASK T ON T.PROPERTYID = SL.PROPERTYID
					WHERE (
						SL.ID IN (
							-- NOTE: There is a potential for multiple Appraisal dates in B&D - the earliest is the required date
							-- Find all Sales Listings where the first appraisal date is in the given range
							SELECT SL1.ID
							FROM bdSALESLISTING SL1
								JOIN bdCONTACTACTIVITY CA ON CA.SALESLISTINGID = SL1.ID AND CA.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
							GROUP BY SL1.ID
							HAVING MIN(CA.STARTDATE) {6}
						) OR
						LISTEDDATE {6}
					)
					GROUP BY SL.ID, SL.LISTEDDATE)

				SELECT
					CONVERT(VARCHAR(7), DATE, 120) AS MONTHSORT,
					MIN(AGENTID) AS AGENTID,
                    ISNULL(MIN(USR.FIRSTNAME),'') + ', ' +  ISNULL(MIN(USR.LASTNAME),'') as AGENTNAME,
					MIN(USR.OFFICEID) AS OFFICEID, PERIOD,
                    1 AS APPRAISAL_COUNT,
					CASE WHEN PERIOD = 'Current'
						THEN 1 ELSE 0 END AS APPRAISAL_COUNT_current,
                    -- Follow up count looks for any tasks with actionon date later than appraisal.
                    CASE WHEN DATE < MAX(T.ACTIONON)
                        THEN 1 ELSE 0 END AS APPRAISAL_FOLLOWUP_COUNT,
                    CASE WHEN PERIOD = 'Current' AND DATE < MAX(T.ACTIONON)
                        THEN 1 ELSE 0 END AS APPRAISAL_FOLLOWUP_COUNT_current,
					CASE WHEN LISTING_SOURCE LIKE 'C - %'
                        THEN 1 ELSE 0 END AS COMPANY_APPRAISAL_COUNT,
					CASE WHEN PERIOD = 'Current' AND LISTING_SOURCE LIKE 'C - %'
                        THEN 1 ELSE 0 END AS COMPANY_APPRAISAL_COUNT_current,
					CASE WHEN LISTING_SOURCE LIKE 'P - %'
                        THEN 1 ELSE 0 END AS PERSONAL_APPRAISAL_COUNT,
					CASE WHEN PERIOD = 'Current' AND LISTING_SOURCE LIKE 'P - %'
                        THEN 1 ELSE 0 END AS PERSONAL_APPRAISAL_COUNT_current
				 FROM APPRAISALS A
					JOIN DB_USER USR ON A.AGENTID = USR.ID
					JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                    LEFT JOIN bdTASK T ON T.PROPERTYID = A.PROPERTYID
				WHERE PERIOD IS NOT NULL {1}
				GROUP BY A.LISTINGID, A.DATE, A.PERIOD, LISTING_SOURCE;

                -- 5. B&D Data Withdrawn count
				SELECT
					CONVERT(VARCHAR(7), ISNULL(WITHDRAWNON,SOLDDATE), 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,
					CASE WHEN WITHDRAWNON {6} THEN 1 ELSE 0 END AS WITHDRAWNCOUNT,
					CASE WHEN WITHDRAWNON {4} THEN 1 ELSE 0 END AS WITHDRAWNCOUNT_current,
					CASE WHEN WITHDRAWNON IS NULL AND SOLDDATE {6} THEN 1 ELSE 0 END AS SOLDCOUNT,
					CASE WHEN WITHDRAWNON IS NULL AND SOLDDATE {4} THEN 1 ELSE 0 END AS SOLDCOUNT_current
				FROM bdSALESLISTING SL
				  JOIN bdSALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
					JOIN bdCONSULTANT U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE ((WITHDRAWNON IS NULL AND SOLDDATE {6}) OR WITHDRAWNON {6}) {1} AND USR.SHOWONKPIREPORT = 1;

                -- 6. Extended Campaign Track data
				SELECT
					CONVERT(VARCHAR(7), ISNULL(WITHDRAWNON,SOLDDATE), 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,
				    CASE WHEN C.ADDRESS1 IS NOT NULL THEN 1 ELSE 0 END AS CAMPAIGNLISTING_COUNT,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						AND C.ADDRESS1 IS NOT NULL THEN 1 ELSE 0 END AS CAMPAIGNLISTING_COUNT_current,
					CASE WHEN (SELECT COUNT(*) FROM CAMPAIGNPRODUCT C0 WHERE SUPPLIER = 'CT - VISUAL DOMAIN' AND C0.CAMPAIGNID = C.ID) > 0
					THEN 1 ELSE 0 END AS HASVIDEO,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						AND (SELECT COUNT(*) FROM CAMPAIGNPRODUCT C1 WHERE SUPPLIER = 'CT - VISUAL DOMAIN' AND C1.CAMPAIGNID = C.ID) > 0
						THEN 1 ELSE 0 END AS HASVIDEO_current,
					CASE WHEN (
                        SELECT COUNT(*) FROM CAMPAIGNPRODUCT WHERE
	                            (DESCRIPTION LIKE 'THE WEEKLY REVIEW:%' OR
	                            DESCRIPTION LIKE 'THE WEEKLY REVIEW EASTERN:%' OR
	                            DESCRIPTION LIKE 'THE WEEKLY REVIEW HDV:%' OR
	                            DESCRIPTION LIKE 'THE WEEKLY REVIEW MELBOURNE TIMES:%' OR
	                            DESCRIPTION LIKE 'MAROONDAH LEADER%' OR
	                            DESCRIPTION LIKE 'MONASH LEADER%') AND CAMPAIGNID = C.ID) > 0
					THEN 1 ELSE 0 END AS HASGLOSSY,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						AND (
                        SELECT COUNT(*) FROM CAMPAIGNPRODUCT WHERE
	                            (DESCRIPTION LIKE 'THE WEEKLY REVIEW:%' OR
	                            DESCRIPTION LIKE 'THE WEEKLY REVIEW EASTERN:%' OR
	                            DESCRIPTION LIKE 'THE WEEKLY REVIEW HDV:%' OR
	                            DESCRIPTION LIKE 'THE WEEKLY REVIEW MELBOURNE TIMES:%' OR
	                            DESCRIPTION LIKE 'MAROONDAH LEADER%' OR
	                            DESCRIPTION LIKE 'MONASH LEADER%') AND CAMPAIGNID = C.ID) > 0
						THEN 1 ELSE 0 END AS HASGLOSSY_current,
					CASE WHEN (SELECT COUNT(*) FROM CAMPAIGNPRODUCT C0 WHERE DESCRIPTION LIKE 'PLACES MAGAZINE%' AND C0.CAMPAIGNID = C.ID) > 0
					THEN 1 ELSE 0 END AS HASPLACEMAG,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						AND (SELECT COUNT(*) FROM CAMPAIGNPRODUCT C1 WHERE DESCRIPTION LIKE 'PLACES MAGAZINE%' AND C1.CAMPAIGNID = C.ID) > 0
						THEN 1 ELSE 0 END AS HASPLACEMAG_current
				FROM bdSALESLISTING SL
				  JOIN bdSALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
				  LEFT JOIN CAMPAIGN C ON SL.ID = CAMPAIGNNUMBER - 1595100000
					JOIN bdCONSULTANT U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE ((WITHDRAWNON IS NULL AND SOLDDATE {6}) OR WITHDRAWNON {6}) {1} AND USR.SHOWONKPIREPORT = 1;

                -- 7. Extended Campaign Track data
				SELECT
					CONVERT(VARCHAR(7), ISNULL(WITHDRAWNON,SOLDDATE), 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					CASE WHEN ISNULL(WITHDRAWNON,SOLDDATE) {4}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,
					CP.DESCRIPTION, 0.0 AS GLOSSY_AMOUNT, 0.0 AS GLOSSY_AMOUNT_current
				FROM bdSALESLISTING SL
				  JOIN bdSALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
				  LEFT JOIN CAMPAIGN C ON SL.ID = CAMPAIGNNUMBER - 1595100000
					JOIN bdCONSULTANT U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
					JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND
	                           (CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW:%' OR
	                            CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW EASTERN:%' OR
	                            CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW HDV:%' OR
	                            CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW MELBOURNE TIMES:%' OR
	                            CP.DESCRIPTION LIKE 'MAROONDAH LEADER%' OR
	                            CP.DESCRIPTION LIKE 'MONASH LEADER%')
				WHERE ((WITHDRAWNON IS NULL AND SOLDDATE {6}) OR WITHDRAWNON {6}) {1} AND USR.SHOWONKPIREPORT = 1;

                ", szFilter, szUserFilter, szFilter.Replace("S.SALEDATE", "S.LISTEDDATE"), szFilter.Replace("S.SALEDATE", "S.AUCTIONDATE"),
                szCurrentPeriodFilter, szYTDFilter, szPreviousAndCurrentPeriodFilter);
        DataSet ds = DB.runDataSet(szSQL);
        ReportHelper.processExtraData(ref ds, false);
        integrateCampaignData(ref ds);

        // Each table from 3 onward are integrated into Table[0]
        // NOTE: for report all queries need the basic details : MONTHSORT,AGENTID,AGENTNAME,OFFICEID
        integrateBnDData(ref ds, ds.Tables[3]);
        integrateBnDData(ref ds, ds.Tables[4]);
        integrateBnDData(ref ds, ds.Tables[5]);
        integrateBnDData(ref ds, ds.Tables[6]);

        // Process 7 then integrate ***
        processGlossyAmounts(ref ds, 7);

        integrateBnDData(ref ds, ds.Tables[7]);

        // Get list of all users found in report
        var lUsers = ds.Tables[0].AsEnumerable().Select(n => DB.readInt(n["AGENTID"])).Distinct();
        string szUserIDs = string.Join(",", lUsers);

        szSQL = string.Format(@"
				-- 0. Financial Year values
                SELECT
					CONVERT(VARCHAR(7), '{2}', 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					'Current' AS PERIOD,
                    (SELECT SUM(USS_.GRAPHCOMMISSION)
                        FROM USERSALESPLIT USS_
                        JOIN SALESPLIT SS_ ON USS_.SALESPLITID = SS_.ID AND SS_.RECORDSTATUS < 1 AND USS_.RECORDSTATUS < 1
                        JOIN SALE S_ ON SS_.SALEID = S_.ID
                        JOIN DB_USER U_ ON USS_.USERID = U_.ID
                        WHERE U_.TOPPERFORMERREPORTSETTINGS = USR.ID AND S_.STATUSID IN (1, 2) AND SS_.CALCULATEDAMOUNT > 0 AND U_.ISACTIVE = 1
                        AND {0}) AS YTD_ACTUALAMOUNT,
                    USR.SALESTARGET AS FY_SALESTARGET
				FROM DB_USER USR
				WHERE USR.ID IN ({1})

				-- 1. Agent Contact counts
                SELECT
					CONVERT(VARCHAR(7), '{2}', 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					CASE WHEN MONTHDATE {3}
						THEN 'Current' ELSE 'Previous' END AS PERIOD,
                    CASE WHEN MONTHDATE {3}
                        THEN CONTACTCOUNT END AS CONTACTCOUNT_CURRENT,
                    CASE WHEN MONTHDATE {3}
                        THEN BUYERCOUNT END AS BUYERCOUNT_CURRENT,
                    CONTACTCOUNT, BUYERCOUNT
				FROM DB_USER USR
                    JOIN AGENTCONTACTCOUNT ACC ON ACC.USERID = USR.ID
                            AND (MONTHDATE = DATEADD(m, DATEDIFF(m, 0, '{2}'), 0) OR MONTHDATE = DATEADD(m, DATEDIFF(m, 0, '{4}'), 0))
				WHERE USR.ID IN ({1})

				-- 2. Has profile video for period
                SELECT
					CONVERT(VARCHAR(7), '{2}', 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					'Current' AS PERIOD,
                    CASE WHEN USR.PROFILEVIDEODATE IS NULL THEN 'No'
                         WHEN USR.PROFILEVIDEODATE < '{2}' THEN 'Yes'
                         ELSE 'No' END AS HASPROFILEVIDEO
				FROM DB_USER USR
				WHERE USR.ID IN ({1})
                UNION
                SELECT
					CONVERT(VARCHAR(7), '{2}', 120) AS MONTHSORT,
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
				    USR.OFFICEID,
					'Previous' AS PERIOD,
                    CASE WHEN USR.PROFILEVIDEODATE IS NULL THEN 'No'
                         WHEN USR.PROFILEVIDEODATE < '{4}' THEN 'Yes'
                         ELSE 'No' END AS HASPROFILEVIDEO
				FROM DB_USER USR
				WHERE USR.ID IN ({1});

            -- 3 Listing Sources (Same temporary view is used for query -- 4)
				WITH APPRAISALS AS (
				  SELECT
						SL.ID AS LISTINGID,
						ISNULL(MIN(CA_APPR.STARTDATE),SL.LISTEDDATE) AS DATE,
						MAX(T.ACTIONON) AS FOLLOWUPDATE,
						CASE
							WHEN ISNULL(MIN(CA_APPR.STARTDATE), LISTEDDATE) {3} THEN 'Current'
							WHEN ISNULL(MIN(CA_APPR.STARTDATE), LISTEDDATE) {5} THEN 'Previous'
							ELSE NULL END AS PERIOD,
						MIN(USR.ID) AS AGENTID,
						MIN(LS.NAME) AS LISTING_SOURCE,
						MIN(SL.PROPERTYID) AS PROPERTYID
					FROM bdSALESLISTING SL
						LEFT JOIN bdLISTINGSOURCE LS ON LS.ID = SL.LISTINGSOURCEID
						JOIN bdPROPERTY P ON P.ID = SL.PROPERTYID
						LEFT JOIN bdCONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
						JOIN bdCONSULTANT U ON U.ID = SL.CONSULTANT1ID
						JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
						LEFT JOIN bdTASK T ON T.PROPERTYID = SL.PROPERTYID
					WHERE (
						SL.ID IN (
							-- NOTE: There is a potential for multiple Appraisal dates in B&D - the earliest is the required date
							-- Find all Sales Listings where the first appraisal date is in the given range
							SELECT SL1.ID
							FROM bdSALESLISTING SL1
								JOIN bdCONTACTACTIVITY CA ON CA.SALESLISTINGID = SL1.ID AND CA.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
							GROUP BY SL1.ID
							HAVING MIN(CA.STARTDATE) {5}
						) OR
						LISTEDDATE {5}
					)
					GROUP BY SL.ID, SL.LISTEDDATE)

                SELECT
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
					USR.OFFICEID, A.PERIOD,
                    CASE WHEN ROW_NUMBER() OVER(PARTITION BY USR.ID ORDER BY COUNT(*) DESC) > 5
                       THEN 'Other' ELSE ISNULL(LISTING_SOURCE, 'Unknown') END AS SOURCE_NAME,
                    COUNT(*) AS SOURCE_COUNT
				  FROM DB_USER USR LEFT JOIN APPRAISALS A ON A.AGENTID = USR.ID AND PERIOD = 'Current'
					JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE USR.ID IN ({1})
				GROUP BY PERIOD, USR.ID, USR.OFFICEID, USR.FIRSTNAME, USR.LASTNAME, LISTING_SOURCE
				ORDER BY USR.ID, COUNT(*) DESC
                ", szYTDFilter, szUserIDs, oFilter.getDBSafeEndDate(), szCurrentPeriodFilter, szPreviousPeriodEnd,
                szPreviousAndCurrentPeriodFilter);

        DataSet ds1 = DB.runDataSet(szSQL);
        integrateBnDData(ref ds, ds1.Tables[0]);
        integrateBnDData(ref ds, ds1.Tables[1]);
        integrateBnDData(ref ds, ds1.Tables[2]);
        integrateBnDData(ref ds, ds1.Tables[3]);

        // This is for debugging purposes - all data used in the report can be viewed in it's final state in this saved file.
        Utility.dataTableToCSVFile(ds.Tables[0], Path.Combine(System.IO.Path.GetTempPath(), "final.csv"));

        return ds;
    }

    // Convert the below values from textual amount to decimal value
    // Lowest values must be listed first, all text must be upper case.
    private Dictionary<string, float> dTextConverstion = new Dictionary<string, float>() {
        { @"1/4", 0.25f},
        { "QUARTER", 0.25f},
        { @"1/2", 0.5f},
        { "HALF", 0.5f},
        { "FULL", 1.0f},
        { "DPS", 2f}, // Double Page Spread
    };

    // Glossies contain textual descriptions of the size of glossy advertising.
    // Amounts such as 1/2, 1/4 etc need to be converted to decimal value
    private void processGlossyAmounts(ref DataSet ds, int TableNumber) {
        foreach (DataRow dr in ds.Tables[TableNumber].Rows) {
            string szDescription = DB.readString(dr["DESCRIPTION"]).ToUpper();

            // Search through all values
            foreach (string szKey in dTextConverstion.Keys) {
                if (szDescription.Contains(szKey)) {
                    dr["GLOSSY_AMOUNT"] = dTextConverstion[szKey];
                    if (DB.readString(dr["PERIOD"]) == "Current")
                        dr["GLOSSY_AMOUNT_current"] = dr["GLOSSY_AMOUNT"];

                    // If the description has upgrade, continue to look for higher values.
                    // EG. Full Page Select Suburb (Winter Deal Upgrade from 1/2 Page)
                    if (!szDescription.Contains("UPGRADE"))
                        break;
                }
            }
        }
    }

    // List of Campaign values requiring a VALUE_current
    private string[] CampaignCreateCurrentValue = new string[] { "TOTALPREPAID", "TOTALSPENT" };

    // inserts data from CampaignDB object into DataSet table[0]
    private void integrateCampaignData(ref DataSet ds) {
        string szCampaignFilterSQL = String.Format(@" WHERE ADDRESS1 NOT LIKE '%PROMO,%' AND C.STARTDATE {0}", szPreviousAndCurrentPeriodFilter);
        string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

        DataTable dtOfficeAgents = ds.Tables[0];

        DataTable dtCampaign = CampaignDB.loadPrePaymentCampaignData(szCampaignFilterSQL, szCompanyIDList, szCurrentPeriodFilter: szCurrentPeriodFilter);
        List<String> szCampaignColumns = new List<String>();

        // creates new columns where required insert campaign values
        foreach (DataColumn dc in dtCampaign.Columns) {
            szCampaignColumns.Add(dc.ColumnName);

            if (!dtOfficeAgents.Columns.Contains(dc.ColumnName))
                dtOfficeAgents.Columns.Add(dc.ColumnName, dc.DataType);

            if (CampaignCreateCurrentValue.Contains(dc.ColumnName))
                dtOfficeAgents.Columns.Add(dc.ColumnName + "_current", dc.DataType);
        }

        // Field to count the number of current campaign records
        dtOfficeAgents.Columns.Add("CAMPAIGN_COUNT_current", typeof(int));
        dtOfficeAgents.Columns.Add("CAMPAIGN_COUNT_previous", typeof(int));

        // Iterates through Campaign table and copies values to Office Agents table.
        foreach (DataRow dr in dtCampaign.Rows) {
            // does not copy 'totals' rows - indicated by lack of ID value in row
            // also exculdes rows that do not relate to an agent (have no agent ID)
            if (dr["ID"] == System.DBNull.Value || dr["AGENTID"] == System.DBNull.Value)
                continue;

            //if (!DB.readBool(dr["SHOWONKPIREPORT"]))
            //    continue;

            DataRow drNew = dtOfficeAgents.NewRow();

            // copy all columns
            foreach (String ColName in szCampaignColumns) {
                drNew[ColName] = dr[ColName];
                if (CampaignCreateCurrentValue.Contains(ColName) && Convert.ToString(dr["PERIOD"]) == "Current")
                    drNew[ColName + "_current"] = dr[ColName];
            }

            if (Convert.ToString(dr["PERIOD"]) == "Current") {
                drNew["CAMPAIGN_COUNT_current"] = 1;
                drNew["LISTEDDATE_current"] = drNew["LISTEDDATE"];
            } else
                drNew["CAMPAIGN_COUNT_previous"] = 1;

            dtOfficeAgents.Rows.Add(drNew);
        }
    }

    private void integrateBnDData(ref DataSet ds, DataTable dtBnDData, string Type = "") {
        string szCampaignFilterSQL = String.Format(@" WHERE ADDRESS1 NOT LIKE '%PROMO,%' AND C.STARTDATE {0}", szPreviousAndCurrentPeriodFilter);
        string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

        DataTable dtOfficeAgents = ds.Tables[0];

        List<String> szCampaignColumns = new List<String>();

        // creates new columns where required insert campaign values
        foreach (DataColumn dc in dtBnDData.Columns) {
            szCampaignColumns.Add(dc.ColumnName);

            if (!dtOfficeAgents.Columns.Contains(dc.ColumnName))
                dtOfficeAgents.Columns.Add(dc.ColumnName, dc.DataType);
        }

        // Iterates through Campaign table and copies values to Office Agents table.
        foreach (DataRow dr in dtBnDData.Rows) {
            DataRow drNew = dtOfficeAgents.NewRow();

            // copy all columns
            foreach (String ColName in szCampaignColumns) {
                drNew[ColName] = dr[ColName];
            }

            dtOfficeAgents.Rows.Add(drNew);
        }
    }
}

public class Payroll_Timesheets : Report {
    private static string RFileName = "Payroll Timesheets";
    private int ReportType;
    private int TimesheetCycleID;
    private int UserID = -1;

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    /// <param name="oFilter">Report Type 1 = Normal pay cycles, 2 = Paid in advance, -1 = Produce only current users timesheet</param>
    public Payroll_Timesheets(ReportViewer Viewer, ReportFilters oFilter, int ReportType, int TimesheetCycleID = Int32.MinValue, int UserID = -1) {
        if (!G.User.IsAdmin && ReportType > -1)
            throw new Exception("Non-Admin user attempting to access admin report - (Full) Payroll Summary");
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Payroll Timesheets";
        this.ReportType = ReportType;
        this.TimesheetCycleID = TimesheetCycleID;
        this.UserID = UserID;
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szReportType = string.Format("TIMESHEETCYCLEID = (SELECT MIN(ID) FROM TIMESHEETCYCLE WHERE CYCLETYPEID = {0} AND COMPLETED = 0)", ReportType);

        // Report type -1 : Produce current user timesheet
        if (ReportType == -1)
            szReportType = string.Format("TIMESHEETCYCLEID = {1} AND U.ID = {0}", UserID, TimesheetCycleID);
        // Allows for the recreation of old timesheet cycles
        else if (TimesheetCycleID > int.MinValue)
            szReportType = string.Format("TIMESHEETCYCLEID = {0}", TimesheetCycleID);

        // Get all data for current pay cycle
        DataSet ds = DB.runDataSet(string.Format(@"
            SELECT TC.STARTDATE, TC.ENDDATE, U.FIRSTNAME + ' ' + U.LASTNAME AS STAFFNAME, TS.* FROM TIMESHEETENTRY TS
                JOIN DB_USER U ON TS.USERID = U.ID
                JOIN TIMESHEETCYCLE TC ON TC.ID = TS.TIMESHEETCYCLEID
            WHERE {0} AND U.ISDELETED = 0
            ORDER BY U.LASTNAME", szReportType));

        return ds;
    }
}

public class Payroll_Summary : Report {
    private static string RFileName = "Payroll Summary";
    private int CycleRef = 0;

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public Payroll_Summary(ReportViewer Viewer, ReportFilters oFilter, int CycleRef) {
        this.CycleRef = CycleRef;
        if (!G.User.IsAdmin)
            throw new Exception("Non-Admin user attempting to access admin report - Payroll Summary");
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Payroll Summary";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        // Get all data for current pay cycle
        DataSet ds = DB.runDataSet(string.Format(@"
            SELECT MIN(U.LASTNAME) AS LASTNAME, MIN(U.FIRSTNAME) AS FIRSTNAME, TSC.CYCLETYPEID, MAX(U.ID) AS ID, MIN(U.FIRSTNAME) + ' ' + MIN(U.LASTNAME) AS NAME,
				LEFT(MAX(O.NAME),2) AS OFFICE,
                MIN(ENTRYDATE) AS STARTDATE, MAX(ENTRYDATE) AS ENDDATE,
	            SUM(ACTUAL) AS ACTUAL, SUM(ANNUALLEAVE) AS ANNUALLEAVE,
	            SUM(SICKLEAVE) AS SICKLEAVE, SUM(RDOACRUED) AS RDOACRUED,
	            SUM(RDOTAKEN) AS RDOTAKEN,
                STUFF((
                        SELECT ' ******' + CAST(TSE.ENTRYDATE AS VARCHAR) + ' - ' + TSE.COMMENTS AS [text()]
                          FROM TIMESHEETENTRY TSE
                         WHERE TSE.USERID = TS.USERID AND TSE.TIMESHEETCYCLEID = TS.TIMESHEETCYCLEID AND TSE.COMMENTS != '' AND TSE.COMMENTS is not  null
                         FOR XML PATH('')
                     ), 1, 1, '' ) AS [Comments],
                CASE
                    -- User without supervisor signs off on own timesheet
					WHEN MAX(USERSIGNOFFDATE) IS NULL AND (U.SUPERVISORID IS NULL OR U.SUPERVISORID = 0) THEN 'Awaiting Staff signoff'
					-- User with supervisor submits timesheet for supervisor signoff
					WHEN MAX(USERSIGNOFFDATE) IS NULL AND U.SUPERVISORID IS NOT NULL AND U.SUPERVISORID > 0 THEN 'Awaiting Staff submission'
					-- User with supervisor requires signoff after they submit form
					WHEN MAX(SIGNEDOFFDATE) IS NULL AND U.SUPERVISORID IS NOT NULL AND U.SUPERVISORID > 0 THEN 'SignOff'
					-- Ready to go!
					ELSE 'Finalised' END AS DETAILS
            FROM TIMESHEETENTRY TS
            JOIN DB_USER U ON U.ID = TS.USERID
            JOIN LIST O ON O.ID = U.OFFICEID
            JOIN TIMESHEETCYCLE TSC ON TSC.ID = TS.TIMESHEETCYCLEID
            WHERE TSC.ID in ({0},{1}) AND U.ISDELETED = 0
            GROUP BY TIMESHEETCYCLEID, USERID, TSC.CYCLETYPEID, SUPERVISORID
			UNION
            SELECT LASTNAME, U.FIRSTNAME, U.PAYROLLCYCLEID, U.ID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME,
				LEFT(O.NAME,2) AS OFFICE, NULL AS STARTDATE, NULL AS ENDDATE,
	            NULL AS ACTUAL, NULL AS ANNUALLEAVE,
	            NULL AS SICKLEAVE, NULL AS RDOACRUED,
	            NULL AS RDOTAKEN, NULL AS COMMENTS,
                'User hasn''t entered details' AS DETAILS
            FROM DB_USER U
            JOIN LIST O ON O.ID = U.OFFICEID
            WHERE U.PAYROLLCYCLEID > 0 AND U.ISDELETED = 0
                AND U.ID NOT IN (SELECT DISTINCT USERID FROM TIMESHEETENTRY WHERE TIMESHEETCYCLEID IN ((SELECT MIN(ID) FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 1 AND COMPLETED = 0),(SELECT MIN(ID) FROM TIMESHEETCYCLE WHERE CYCLETYPEID = 2 AND COMPLETED = 0)))
			ORDER BY TSC.CYCLETYPEID, LASTNAME",
                        G.TimeSheetCycleReferences[CycleRef].NormalCycle.CycleID,
                        G.TimeSheetCycleReferences[CycleRef].PayAdvanceCycle.CycleID));
        foreach (DataRow dr in ds.Tables[0].Rows) {
            if(DB.readString(dr["COMMENTS"]) != "") {
                dr["Comments"] = DB.readString(dr["COMMENTS"]).Replace("******", Environment.NewLine).Replace("12:00AM", "");
            }
        }
        return ds;
    }
}

#endregion KPI

#region Advertising

public class Top_Advertising : Report {
    private static string RFileName = "Top advertising";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public Top_Advertising(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Top advertising";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        ReportTitle += " " + oFilter.StartDate + " - " + oFilter.EndDate;

        string szFilter = String.Format(@"
            AND (ISNULL(S.SALEDATE,S.FALLENTHROUGHDATE) BETWEEN '{0}' AND '{1}')
        ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        // Get Role value
        string szRole = Valid.getText("szRoleID", "", VT.TextNormal);
        string szRoleFilter = "";

        // Check for All role
        if (!szRole.Equals("-1"))
            szRoleFilter = " AND U1.ROLEID =" + szRole;

        // Get Company value
        string szCompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
        string szCompanyFilter = "";

        // Check for Company
        if (!szCompanyID.Equals(""))
            szCompanyFilter = " AND L_OFFICE.COMPANYID  IN (" + szCompanyID + ") ";

        string szQuery = @"
                SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, U1.INITIALSCODE, ISNULL(SUM(S.ADVERTISINGSPEND), 0) AS AMOUNT
                FROM USERSALESPLIT USS
                    JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1  AND USS.INCLUDEINKPI = 1
                    JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0  AND L_SPLITTYPE.ID = 10
                    JOIN SALE S ON SS.SALEID = S.ID
                    JOIN DB_USER U ON USS.USERID = U.ID
                    JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    JOIN  DB_USER U1 ON U1.ID = U.TOPPERFORMERREPORTSETTINGS
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND U.ISACTIVE = 1 AND U.ISPAID = 1
                    {0} {1}

                GROUP BY U.TOPPERFORMERREPORTSETTINGS,  U1.INITIALSCODE
                ORDER BY AMOUNT DESC;

                SELECT USR.ID, USR.INITIALSCODE
                FROM DB_USER USR
                JOIN LIST L_OFFICE ON USR.OFFICEID = L_OFFICE.ID
                WHERE 1=1 {1}
                ;";

        string szSQL = string.Format(szQuery, szFilter + szRoleFilter + szCompanyFilter, "");
        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}

public class Prepayment_Average : Report {
    private static string RFileName = "Top prepayment average";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public Prepayment_Average(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Prepayment (average)";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szFilter = String.Format(@"
            AND (ISNULL(S.SALEDATE,S.FALLENTHROUGHDATE) BETWEEN '{0}' AND '{1}')
            ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        // Get Role value
        string szRole = Valid.getText("szRoleID", "", VT.TextNormal);
        string szRoleFilter = "";

        // Check for All role
        if (!szRole.Equals("-1"))
            szRoleFilter = " AND U1.ROLEID =" + szRole;

        // Get Company valu
        string szCompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
        string szCompanyFilter = "";

        // Check for Company
        if (!szCompanyID.Equals(""))
            szCompanyFilter = " AND L_OFFICE.COMPANYID  IN (" + szCompanyID + ") ";
        string szStartDate = Valid.getText("szStartDate", VT.TextNormal);
        string szEndDate = Valid.getText("szEndDate", VT.TextNormal);
        string szCampaignFilterSQL = @" WHERE ADDRESS1 NOT LIKE '%PROMO,%' ";

        if (szStartDate != "")
            szCampaignFilterSQL += " AND C.STARTDATE >= '" + szStartDate + "' ";

        if (szEndDate != "")
            szCampaignFilterSQL += " AND C.STARTDATE <= '" + szEndDate + "' ";

        DataTable dt = CampaignDB.loadPrePaymentCampaignData(szCampaignFilterSQL, szCompanyID);
        DataView dv = dt.DefaultView;
        dv.RowFilter = "AGENT <> ''";
        dv.Sort = "ROLEID DESC";
        DataSet ds = new DataSet();
        ds.Tables.Add(dv.ToTable());

        //Update the ROLEID
        foreach (DataRow dr in ds.Tables[0].Rows) {
            UserDetail uD = G.UserInfo.getUser(DB.readString(dr["AGENT"]));
            if (uD != null)
                dr["ROLEID"] = uD.RoleID;
        }
        return ds;
    }
}

public class Top_Advertising_Average : Report {
    private static string RFileName = "Top advertising average";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public Top_Advertising_Average(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Top advertising (average)";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szFilter = String.Format(@"
            AND (ISNULL(S.SALEDATE,S.FALLENTHROUGHDATE) BETWEEN '{0}' AND '{1}')
            ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        // Get Role value
        string szRole = Valid.getText("szRoleID", "", VT.TextNormal);
        string szRoleFilter = "";

        // Check for All role
        if (!szRole.Equals("-1"))
            szRoleFilter = " AND U1.ROLEID =" + szRole;

        // Get Company valu
        string szCompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
        string szCompanyFilter = "";

        // Check for Company
        if (!szCompanyID.Equals(""))
            szCompanyFilter = " AND L_OFFICE.COMPANYID  IN (" + szCompanyID + ") ";

        string szQuery = @"
                SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, U1.INITIALSCODE, 
                    ISNULL(AVG(S.ADVERTISINGSPEND), 0) AS AMOUNT
	            FROM USERSALESPLIT USS
                    JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1  AND USS.INCLUDEINKPI = 1
                    JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0  AND L_SPLITTYPE.ID = 10
                    JOIN SALE S ON SS.SALEID = S.ID
                    JOIN DB_USER U ON USS.USERID = U.ID
                    JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    JOIN  DB_USER U1 ON U1.ID = U.TOPPERFORMERREPORTSETTINGS
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND U.ISACTIVE = 1 AND U.ISPAID = 1
                {0} {1}
                GROUP BY U.TOPPERFORMERREPORTSETTINGS,  U1.INITIALSCODE
                ORDER BY AMOUNT DESC;

                SELECT USR.ID, USR.INITIALSCODE
                FROM DB_USER USR
                JOIN LIST L_OFFICE ON USR.OFFICEID = L_OFFICE.ID
                WHERE 1=1 {1}
                ;";

        string szSQL = string.Format(szQuery, szFilter + szRoleFilter + szCompanyFilter, "");
        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}

public class Top_Advertising_Detail : Report {
    private static string RFileName = "Top advertising detail";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public Top_Advertising_Detail(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Top advertising detail";
        initReport();
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    public override DataSet getData() {
        string szFilter = String.Format(@"
            AND (ISNULL(S.SALEDATE,S.FALLENTHROUGHDATE) BETWEEN '{0}' AND '{1}')
            ", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());

        // Get Role value
        string szRole = Valid.getText("szRoleID", "", VT.TextNormal);
        string szRoleFilter = "";

        // Check for All role
        if (!szRole.Equals("-1"))
            szRoleFilter = " AND U1.ROLEID =" + szRole;

        // Get Company valu
        string szCompanyID = Valid.getText("szCompanyID", "", VT.TextNormal);
        string szCompanyFilter = "";

        // Check for Company
        if (!szCompanyID.Equals(""))
            szCompanyFilter = " AND L_OFFICE.COMPANYID  IN (" + szCompanyID + ") ";

        string szQuery = @"
                SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, U1.LASTNAME + ' ' + U1.FIRSTNAME AS AGENTNAME, S.ADDRESS,
                    ISNULL(S.ADVERTISINGSPEND, 0) AS AMOUNT, ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE) AS SALEDATE
	            FROM USERSALESPLIT USS
                    JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1  AND USS.INCLUDEINKPI = 1
                    JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0  AND L_SPLITTYPE.ID = 10
                    JOIN SALE S ON SS.SALEID = S.ID
                    JOIN DB_USER U ON USS.USERID = U.ID
                    JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    JOIN  DB_USER U1 ON U1.ID = U.TOPPERFORMERREPORTSETTINGS
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND U.ISACTIVE = 1 AND U.ISPAID = 1
                {0} {1}
                ORDER BY U1.LASTNAME, U1.FIRSTNAME, AMOUNT DESC;

                SELECT USR.ID, USR.INITIALSCODE , ROLEID
                FROM DB_USER USR
                JOIN LIST L_OFFICE ON USR.OFFICEID = L_OFFICE.ID
                WHERE 1=1 {1} AND USR.ISPAID = 1
                ;";

        string szSQL = string.Format(szQuery, szFilter + szRoleFilter + szCompanyFilter, "");
        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}

#endregion Advertising

/// <summary>
/// Base class for all SRSS based reports.
/// </summary>
public abstract class Report {
    public abstract string ReportFileName { get; }
    public string ReportTitle = "SET THE REPORT TITLE";

    public abstract DataSet getData();

    protected ReportViewer oViewer = null;
    protected ReportParameter[] arReportParams = new ReportParameter[4];
    protected ReportFilters oFilter = null;

    // for debuging **SHANE**
    public DataSet dsPublic;

    // for debuging
    public String outputDataSetCSV() {
        // Only for debugging any user log in other than 'gord' will get no output
        if (G.User.UserID != 0)
            return "";

        List<string> ColumnNames = new List<string>();
        String output = "\n";

        foreach (DataColumn dc in dsPublic.Tables[0].Columns)
            ColumnNames.Add(dc.ColumnName);

        output += String.Join(",", ColumnNames) + "\n";

        foreach (DataRow dr in dsPublic.Tables[0].Rows) {
            foreach (String column in ColumnNames) {
                if (dr[column].ToString().Contains(','))
                    output += String.Format("\"{0}\",", dr[column]);
                else
                    output += String.Format("{0},", dr[column]);
            }
            output += "\n";
        }

        return output;
    }

    protected virtual void initReport() {
        oFilter.loadUserFilters();
        loadParameters();
        setupReport();
    }

    /// <summary>
    /// Performs the linking to create and run the report with the data
    /// </summary>
    /// <returns></returns>
    protected bool setupReport() {
        LoadReportDefinition(oViewer, "../reports/templates/" + ReportFileName + ".rdlc");
        oViewer.LocalReport.DataSources.Clear();
        oViewer.LocalReport.DisplayName = ReportTitle;
        oViewer.LocalReport.SetParameters(arReportParams);
        try {
            DataSet ds = getData();
            dsPublic = ds;
            int i = 1;
            foreach (DataTable dt in ds.Tables) {
                oViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet" + i, dt));

                // For debuging
                Utility.dataTableToCSVFile(dt, Path.Combine(Path.Combine(System.IO.Path.GetTempPath(), string.Format(@"output{0}.csv", i))));

                i++;
            }
        } catch (Exception) {
            throw;
        }
        return true;
    }

    protected void loadParameters() {
        int ParamCount = 4;

        if (ReportFileName.ToUpper() == "EOFY BONUS DETAIL")
            ParamCount = 5;
        arReportParams = new ReportParameter[ParamCount];
        arReportParams[0] = new ReportParameter("param_reportName", ReportTitle);
        arReportParams[1] = new ReportParameter("param_startDate", Utility.formatDate(oFilter.StartDate));
        arReportParams[2] = new ReportParameter("param_endDate", Utility.formatDate(oFilter.EndDate));
        arReportParams[3] = new ReportParameter("param_fy", oFilter.FinancialYear);
        if (ReportFileName.ToUpper() == "EOFY BONUS DETAIL")
            arReportParams[4] = new ReportParameter("param_EOFYBonus_Offset", Client.EOFYBonusMonthDelay.ToString());
    }

    protected void LoadReportDefinition(ReportViewer reportViewer, string reportPath) {
        string strReport = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(reportPath), System.Text.Encoding.Default);
        if (strReport.Contains("http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition")) {
            strReport = strReport.Replace("<Report xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\" xmlns:cl=\"http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition\" xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition\">", "<Report xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\" xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition\">");
            strReport = strReport.Replace("<ReportSections>", "").Replace("<ReportSection>", "").Replace("</ReportSection>", "").Replace("</ReportSections>", "");
        }
        byte[] bytReport = System.Text.Encoding.Default.GetBytes(strReport);
        reportViewer.LocalReport.LoadReportDefinition(new MemoryStream(bytReport));
        reportViewer.ShowRefreshButton = false;
    }
}