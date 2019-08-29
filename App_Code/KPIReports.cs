using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public class KPI_Agent_Card : Report {
    private static string RFileName = "KPI Agent Card";
    private List<ReportData> lData = new List<ReportData>();

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public KPI_Agent_Card(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Agent KPIs";
        initReport();
    }

    protected override void initReport() {
        oFilter.loadUserFilters();
        loadParameters();
        LoadReportDefinition(oViewer, "../reports/templates/" + ReportFileName + ".rdlc");
        oViewer.LocalReport.DataSources.Clear();
        oViewer.LocalReport.DisplayName = ReportTitle;
        oViewer.LocalReport.SetParameters(arReportParams);

        oViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", this.loadReportObjects()));
    }

    /// <summary>
    /// The name within the get, corresponds with the report to return.
    /// </summary>
    public override string ReportFileName {
        get {
            return RFileName;
        }
    }

    private String szCurrentPeriodFilter;
    private String szOverallFilter;

    public List<ReportData> loadReportObjects() {
        szCurrentPeriodFilter = String.Format("BETWEEN '{0}' AND '{1}'", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        szOverallFilter = String.Format("BETWEEN '{0}' AND '{1}'", Utility.formatDate(Utility.getFinYearStart(oFilter.StartDate)), oFilter.getDBSafeEndDate());

        DateTime dtPreviousPeriodEnd = oFilter.StartDate.AddSeconds(-1);

        string szFilter = String.Format(" ((S.SALEDATE BETWEEN '{0}' AND '{1}') OR (S.SALEDATE IS NULL AND S.FALLENTHROUGHDATE BETWEEN '{0}' AND '{1}'))", Utility.formatDate(Utility.getFinYearStart(oFilter.StartDate)), oFilter.getDBSafeEndDate());
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
					USS.USERID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ' ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
                 	USR.OFFICEID,
                    CASE
			            WHEN S.FALLENTHROUGHDATE IS NOT NULL OR S.ISAUCTION = 1 OR S.AUCTIONDATE IS NOT NULL OR S.LISTEDDATE IS NULL OR S.SALEDATE IS NULL THEN 0
			            ELSE 1
    		            END AS PRIVATESALE,
                    CASE
			            WHEN S.FALLENTHROUGHDATE IS NOT NULL OR S.ISAUCTION = 1 OR S.AUCTIONDATE IS NOT NULL OR S.LISTEDDATE IS NULL OR S.SALEDATE IS NULL THEN 0
			            ELSE
                        DATEDIFF(DAY, S.LISTEDDATE, S.SALEDATE)
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
		            S.LISTEDDATE,
		            S.FALLENTHROUGHDATE as WITHDRAWNDATE,
                    CASE WHEN S.FALLENTHROUGHDATE IS NULL THEN 0 ELSE 1 END AS ISWITHDRAWN,
                    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLD,
				    CASE WHEN S.SALEDATE IS NULL THEN 0 ELSE 1 END AS ISSOLDCOMPANY,
		            S.AUCTIONDATE,
                    CASE WHEN S.AUCTIONDATE > S.SALEDATE THEN 1 ELSE 0 END AS SOLDBEFORE,
                    CASE WHEN ISNULL(S.SALEDATE, S.FALLENTHROUGHDATE) {5}
					    THEN '1' ELSE '0' END AS INPERIOD
                FROM
                    SALE S JOIN SALESPLIT SS ON SS.SALEID = S.ID AND SS.RECORDSTATUS = 0
                            JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                            JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                            JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                            JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                            JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                            LEFT Join CAMPAIGN C on S.BnDSALEID = C.BnDLISTINGID
                WHERE {0}
                    AND USS.INCLUDEINKPI = 1 AND USR.SHOWONKPIREPORT = 1
                ORDER BY USS.USERID

                -- 1 B&D Data Listing count - total
                SELECT
					USR.ID AS AGENTID,
					COUNT(DISTINCT P.ID) AS LISTING_COUNT,
                    'TOTAL' AS PERIOD
				FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
					JOIN Fletchers_BoxDiceAPI.dbo.PROPERTY P ON P.ID = SL.PROPERTYID
					LEFT JOIN Fletchers_BoxDiceAPI.dbo.CONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
					JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1 AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE (LISTEDDATE {4} OR STARTDATE {4}) {1} AND USR.SHOWONKPIREPORT = 1
				GROUP BY SL.CONSULTANT1ID, USR.ID
                UNION
                SELECT
					USR.ID AS AGENTID,
					COUNT(DISTINCT P.ID) AS LISTING_COUNT,
                    'PERIOD' AS PERIOD
				FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
					JOIN Fletchers_BoxDiceAPI.dbo.PROPERTY P ON P.ID = SL.PROPERTYID
					LEFT JOIN Fletchers_BoxDiceAPI.dbo.CONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
					JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1 AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE (LISTEDDATE {5} OR STARTDATE {5}) {1} AND USR.SHOWONKPIREPORT = 1
				GROUP BY SL.CONSULTANT1ID, USR.ID;

                -- 2. B&D Data Appraisal count
				WITH APPRAISALS AS (
				  SELECT
						SL.ID AS LISTINGID,
						ISNULL(MIN(CA_APPR.STARTDATE),SL.LISTEDDATE) AS DATE,
						MAX(T.ACTIONON) AS FOLLOWUPDATE,
						MIN(USR.ID) AS AGENTID,
						MIN(LS.NAME) AS LISTING_SOURCE,
						MIN(SL.PROPERTYID) AS PROPERTYID
					FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
						LEFT JOIN Fletchers_BoxDiceAPI.dbo.LISTINGSOURCE LS ON LS.ID = SL.LISTINGSOURCEID
						JOIN Fletchers_BoxDiceAPI.dbo.PROPERTY P ON P.ID = SL.PROPERTYID
						LEFT JOIN Fletchers_BoxDiceAPI.dbo.CONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
						JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
						JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
						LEFT JOIN Fletchers_BoxDiceAPI.dbo.TASK T ON T.PROPERTYID = SL.PROPERTYID
					WHERE (
						SL.ID IN (
							-- NOTE: There is a potential for multiple Appraisal dates in B&D - the earliest is the required date
							-- Find all Sales Listings where the first appraisal date is in the given range
							SELECT SL1.ID
							FROM FLETCHERS_BOXDICEAPI.DBO.SALESLISTING SL1
								JOIN FLETCHERS_BOXDICEAPI.DBO.CONTACTACTIVITY CA ON CA.SALESLISTINGID = SL1.ID AND CA.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
							GROUP BY SL1.ID
							HAVING MIN(CA.STARTDATE) {4}
						) OR
						LISTEDDATE {4}
					)
					GROUP BY SL.ID, SL.LISTEDDATE)

				SELECT
					MIN(AGENTID) AS AGENTID,
                    1 AS APPRAISAL_COUNT,
					1 AS APPRAISAL_COUNT_current,
                    -- Follow up count looks for any tasks with actionon date later than appraisal.
                    CASE WHEN DATE < MAX(T.ACTIONON)
                        THEN 1 ELSE 0 END AS APPRAISAL_FOLLOWUP_COUNT,
                    CASE WHEN  DATE < MAX(T.ACTIONON)
                        THEN 1 ELSE 0 END AS APPRAISAL_FOLLOWUP_COUNT_current,
					CASE WHEN LISTING_SOURCE LIKE 'C - %'
                        THEN 1 ELSE 0 END AS COMPANY_APPRAISAL_COUNT,
					CASE WHEN  LISTING_SOURCE LIKE 'C - %'
                        THEN 1 ELSE 0 END AS COMPANY_APPRAISAL_COUNT_current,
					CASE WHEN LISTING_SOURCE LIKE 'P - %'
                        THEN 1 ELSE 0 END AS PERSONAL_APPRAISAL_COUNT,
					CASE WHEN LISTING_SOURCE LIKE 'P - %'
                        THEN 1 ELSE 0 END AS PERSONAL_APPRAISAL_COUNT_current
				 FROM APPRAISALS A
					JOIN DB_USER USR ON A.AGENTID = USR.ID
					JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                    LEFT JOIN Fletchers_BoxDiceAPI.dbo.TASK T ON T.PROPERTYID = A.PROPERTYID
				WHERE 1= 1 {1}
				GROUP BY A.LISTINGID, A.DATE, LISTING_SOURCE;

                -- 3. B&D Data Withdrawn count
				SELECT
					USR.ID AS AGENTID,
				    USR.OFFICEID,
					CASE WHEN WITHDRAWNON {4} THEN 1 ELSE 0 END AS WITHDRAWNCOUNT,
					CASE WHEN WITHDRAWNON {5} THEN 1 ELSE 0 END AS WITHDRAWNCOUNT_current,
					CASE WHEN WITHDRAWNON IS NULL AND SOLDDATE {4} THEN 1 ELSE 0 END AS SOLDCOUNT,
					CASE WHEN WITHDRAWNON IS NULL AND SOLDDATE {5} THEN 1 ELSE 0 END AS SOLDCOUNT_current
				FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
				  JOIN Fletchers_BoxDiceAPI.dbo.SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
					JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE ((WITHDRAWNON IS NULL AND SOLDDATE {4}) OR WITHDRAWNON {4}) {1} AND USR.SHOWONKPIREPORT = 1;

                -- 4. Extended Campaign Track data - glossy, video, places
				SELECT
					USR.ID AS AGENTID,
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
				FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
				  JOIN Fletchers_BoxDiceAPI.dbo.SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
				  LEFT JOIN CAMPAIGN C ON SL.ID = CAMPAIGNNUMBER - 1595100000
					JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
					JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE ((WITHDRAWNON IS NULL AND SOLDDATE {4}) OR WITHDRAWNON {4}) {1} AND USR.SHOWONKPIREPORT = 1
                ORDER BY USR.ID;

                -- 5. Glossies values
				SELECT
					USR.ID AS AGENTID,
					CP.DESCRIPTION, 0.0 AS GLOSSY_AMOUNT, 0.0 AS GLOSSY_AMOUNT_current
				FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
				JOIN Fletchers_BoxDiceAPI.dbo.SALESVOUCHER SV ON SV.SALESLISTINGID = SL.ID
				LEFT JOIN CAMPAIGN C ON SL.ID = CAMPAIGNNUMBER - 1595100000
				JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
				JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND
	                        (CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW:%' OR
	                            CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW EASTERN:%' OR
	                            CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW HDV:%' OR
	                            CP.DESCRIPTION LIKE 'THE WEEKLY REVIEW MELBOURNE TIMES:%' OR
	                            CP.DESCRIPTION LIKE 'MAROONDAH LEADER%' OR
	                            CP.DESCRIPTION LIKE 'MONASH LEADER%')
				WHERE ((WITHDRAWNON IS NULL AND SOLDDATE {4}) OR WITHDRAWNON {4}) {1} AND USR.SHOWONKPIREPORT = 1
                ORDER BY USR.ID;

                ", szFilter, szUserFilter, szFilter.Replace("S.SALEDATE", "S.LISTEDDATE"), szFilter.Replace("S.SALEDATE", "S.AUCTIONDATE"), szOverallFilter,
                szCurrentPeriodFilter);
        DataSet ds = DB.runDataSet(szSQL);
        lData = new List<ReportData>();
        int CurrAgentID = -1;
        ReportData u = null;
        foreach (DataRow dr in ds.Tables[0].Rows) {
            if (CurrAgentID == -1 || CurrAgentID != DB.readInt(dr["AGENTID"])) {
                u = new ReportData();
                u.AgentName = DB.readString(dr["AGENTNAME"]);
                u.OfficeName = DB.readString(dr["OFFICENAME"]);
                u.OfficeID = DB.readInt(dr["OFFICEID"]);
                u.AgentID = DB.readInt(dr["AGENTID"]);
                lData.Add(u);
                CurrAgentID = DB.readInt(dr["AGENTID"]);
            }
            u.ListingCount += 1;
            if (DB.readInt(dr["INPERIOD"]) == 1)
                u.ListingCountPeriod += 1;

            if (DB.readInt(dr["ISAUCTION"]) == 1)
                u.AuctionCountTotal += 1;

            if (DB.readInt(dr["ISSOLD"]) == 1) {
                u.TotalAdvertising += DB.readDouble(dr["ADVERTISING_DOLLARS"]);
                u.TotalCommission += DB.readDouble(dr["COMMISSIONEARNED"]);
                u.TotalSalePrice += DB.readDouble(dr["SALEPRICE"]);
                u.TotalSaleCount += 1;
                if (DB.readInt(dr["INPERIOD"]) == 1) {
                    u.TotalAdvertisingPeriod += DB.readDouble(dr["ADVERTISING_DOLLARS"]);
                    u.TotalCommissionPeriod += DB.readDouble(dr["COMMISSIONEARNED"]);
                    u.TotalSalePricePeriod += DB.readDouble(dr["SALEPRICE"]);
                    u.TotalSaleCountPeriod += 1;
                }

                //Add the number of auctions sold, and the number of days on market
                if (DB.readInt(dr["SOLDATAUCTION"]) == 1) {
                    u.AuctionSoldTotal += 1;
                    u.AuctionTotalDays += DB.readDouble(dr["DAYS_TO_AUCTION_SELL"]);

                    if (DB.readInt(dr["INPERIOD"]) == 1) {
                        u.AuctionTotalDaysPeriod += DB.readDouble(dr["DAYS_TO_AUCTION_SELL"]);
                        u.AuctionSoldPeriod += 1;
                    }
                }

                //Add the numberfor private sales
                if (DB.readInt(dr["PRIVATESALE"]) == 1 || DB.readInt(dr["SOLDBEFORE"]) == 1) {
                    u.PrivateSoldTotal += 1;
                    u.PrivateTotalDays += DB.readDouble(dr["DAYS_TO_PRIVATE_SELL"]);
                    if (DB.readInt(dr["INPERIOD"]) == 1) {
                        u.PrivateSoldPeriod += 1;
                        u.PrivateTotalDaysPeriod += DB.readDouble(dr["DAYS_TO_PRIVATE_SELL"]);
                    }
                }
            }
        }
        processCampaignData(ref ds);
        processAppraisalData(ds.Tables[2]);

        // Add sales counts
        CurrAgentID = -1;
        foreach (DataRow dr in ds.Tables[1].Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);
            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }

            if (DB.readString(dr["PERIOD"]) == "TOTAL")
                u.TOTALLISTINGS = DB.readDouble(dr["LISTING_COUNT"], 0);
            else
                u.TOTALLISTINGSPERIOD = DB.readDouble(dr["LISTING_COUNT"], 0);
        }

        // Add withdrawn counts
        CurrAgentID = -1;
        foreach (DataRow dr in ds.Tables[3].Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);
            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }

            u.WithdrawListingsCount += DB.readDouble(dr["WITHDRAWNCOUNT"], 0);
            u.WithdrawListingsCountPeriod += DB.readDouble(dr["WITHDRAWNCOUNT_CURRENT"], 0);
        }

        processGlossyAmounts(ref ds, 5);
        CurrAgentID = -1;
        // Add campaign advertising data
        foreach (DataRow dr in ds.Tables[4].Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);
            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }

            u.CampaignCount += DB.readInt(dr["CAMPAIGNLISTING_COUNT"]);
            u.CampaignCountPeriod += DB.readInt(dr["CAMPAIGNLISTING_COUNT_CURRENT"]);

            u.GlossyCount += DB.readInt(dr["HASGLOSSY"]);
            u.GlossyCountPeriod += DB.readInt(dr["HASGLOSSY_current"]);

            u.VideoCount += DB.readInt(dr["HASVIDEO"]);
            u.VideoCountPeriod += DB.readInt(dr["HASVIDEO_current"]);

            u.PlacesCount += DB.readInt(dr["HASPLACEMAG"]);
            u.PlacesCountPeriod += DB.readInt(dr["HASPLACEMAG_current"]);
        }

        CurrAgentID = -1;
        // Add campaign advertising data
        foreach (DataRow dr in ds.Tables[5].Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);
            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }

            u.GlossyTotalValue = DB.readDouble(dr["GLOSSY_AMOUNT"], 0);
            u.GlossyTotalValuePeriod = DB.readDouble(dr["GLOSSY_AMOUNT_CURRENT"], 0);
        }

        // Get list of all users found in report
        var lUsers = ds.Tables[0].AsEnumerable().Select(n => DB.readInt(n["AGENTID"])).Distinct();
        string szUserIDs = string.Join(",", lUsers);
        if (szUserIDs == "")
            szUserIDs = "-1";
        szSQL = string.Format(@"
				-- 0. Financial Year values
                SELECT
					USR.ID AS AGENTID,
                    (SELECT SUM(USS_.GRAPHCOMMISSION)
                        FROM USERSALESPLIT USS_
                        JOIN SALESPLIT SS_ ON USS_.SALESPLITID = SS_.ID AND SS_.RECORDSTATUS < 1 AND USS_.RECORDSTATUS < 1
                        JOIN SALE S_ ON SS_.SALEID = S_.ID
                        JOIN DB_USER U_ ON USS_.USERID = U_.ID
                        WHERE U_.TOPPERFORMERREPORTSETTINGS = USR.ID AND S_.STATUSID IN (1, 2) AND SS_.CALCULATEDAMOUNT > 0 AND U_.ISACTIVE = 1
                        AND S_.SALEDATE {0}) AS YTD_ACTUALAMOUNT,
                    USR.SALESTARGET AS FY_SALESTARGET
				FROM DB_USER USR
				WHERE USR.ID IN ({1})

				-- 1. Agent Contact counts
                SELECT
					USR.ID AS AGENTID,
				(SELECT TOP 1 CONTACTCOUNT FROM AGENTCONTACTCOUNT ACC WHERE MONTHDATE < '{4}' AND ACC.USERID = USR.ID ORDER BY MONTHDATE DESC) AS TOTAL,
				(SELECT TOP 1 CONTACTCOUNT FROM AGENTCONTACTCOUNT ACC WHERE MONTHDATE < '{2}' AND ACC.USERID = USR.ID  ORDER BY MONTHDATE DESC) AS PERIOD,
                (SELECT TOP 1 CONTACTCOUNT FROM AGENTCONTACTCOUNT ACC WHERE MONTHDATE <  DATEADD(m, -{5}, '{2}') AND ACC.USERID = USR.ID  ORDER BY MONTHDATE DESC) AS PREVPERIOD
				FROM DB_USER USR
				WHERE USR.ID IN ({1});

			    -- 2 Listing Sources (Same temporary view is used for query -- 4)
				WITH APPRAISALS AS (
				  SELECT
						SL.ID AS LISTINGID,
						ISNULL(MIN(CA_APPR.STARTDATE),SL.LISTEDDATE) AS DATE,
						MAX(T.ACTIONON) AS FOLLOWUPDATE,
						MIN(USR.ID) AS AGENTID,
						MIN(LS.NAME) AS LISTING_SOURCE,
						MIN(SL.PROPERTYID) AS PROPERTYID
					FROM Fletchers_BoxDiceAPI.dbo.SALESLISTING SL
						LEFT JOIN Fletchers_BoxDiceAPI.dbo.LISTINGSOURCE LS ON LS.ID = SL.LISTINGSOURCEID
						JOIN Fletchers_BoxDiceAPI.dbo.PROPERTY P ON P.ID = SL.PROPERTYID
						LEFT JOIN Fletchers_BoxDiceAPI.dbo.CONTACTACTIVITY CA_APPR ON CA_APPR.SALESLISTINGID = SL.ID AND CA_APPR.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
						JOIN Fletchers_BoxDiceAPI.dbo.DB_USER U ON U.ID = SL.CONSULTANT1ID
						JOIN DB_USER USR ON USR.INITIALSCODE = U.INITIALS COLLATE Latin1_General_CI_AS AND USR.ISACTIVE = 1  AND USR.ISDELETED = 0
						LEFT JOIN Fletchers_BoxDiceAPI.dbo.TASK T ON T.PROPERTYID = SL.PROPERTYID
					WHERE (
						SL.ID IN (
							-- NOTE: There is a potential for multiple Appraisal dates in B&D - the earliest is the required date
							-- Find all Sales Listings where the first appraisal date is in the given range
							SELECT SL1.ID
							FROM FLETCHERS_BOXDICEAPI.DBO.SALESLISTING SL1
								JOIN FLETCHERS_BOXDICEAPI.DBO.CONTACTACTIVITY CA ON CA.SALESLISTINGID = SL1.ID AND CA.CONTACTACTIVITYTYPEID IN (10,11,17) --APPRAISALS
							GROUP BY SL1.ID
							HAVING MIN(CA.STARTDATE) {3}
						) OR
						LISTEDDATE {3}
					)
					GROUP BY SL.ID, SL.LISTEDDATE)
                SELECT
					USR.ID AS AGENTID,
                    ISNULL(USR.FIRSTNAME,'') + ', ' +  ISNULL(USR.LASTNAME,'') as AGENTNAME,
					USR.OFFICEID,
                    CASE WHEN ROW_NUMBER() OVER(PARTITION BY USR.ID ORDER BY COUNT(*) DESC) > 5
                       THEN 'Other' ELSE ISNULL(LISTING_SOURCE, 'Unknown') END AS SOURCE_NAME,
                    COUNT(*) AS SOURCE_COUNT
				  FROM DB_USER USR LEFT JOIN APPRAISALS A ON A.AGENTID = USR.ID
					JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
				WHERE USR.ID IN ({1})
				GROUP BY USR.ID, USR.OFFICEID, USR.FIRSTNAME, USR.LASTNAME, LISTING_SOURCE
				ORDER BY USR.ID, COUNT(*) DESC;

                -- 3 Budget values
                SELECT * FROM USERKPIBUDGET
                WHERE USERID IN ({1}) AND VALUE != ''
                ORDER BY USERID;
                ", szOverallFilter, szUserIDs, oFilter.getDBSafeEndDate(), szCurrentPeriodFilter, Utility.formatDate(Utility.getFinYearEnd(oFilter.StartDate)),
                oFilter.MonthsinPeriod);

        DataSet ds1 = DB.runDataSet(szSQL);

        // Add YTD actuals and targets
        CurrAgentID = -1;
        foreach (DataRow dr in ds1.Tables[0].Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);

            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }
            u.ActualAmount = DB.readDouble(dr["YTD_ACTUALAMOUNT"], 0);
            u.BudgetAmount = DB.readDouble(dr["FY_SALESTARGET"], 0);
        }

        // Set budget Amounts
        CurrAgentID = -1;
        foreach (DataRow dr in ds1.Tables[3].Rows) {
            int AgentID = DB.readInt(dr["USERID"]);

            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }
            u.setBudgetValue(DB.readString(dr["CATEGORY"]), DB.readString(dr["VALUE"]));
        }

        // Add YTD actuals and targets
        CurrAgentID = -1;
        foreach (DataRow dr in ds1.Tables[1].Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);

            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }
            u.NUMBEROFCONTACTSPERIOD = DB.readDouble(dr["PERIOD"], 0) - DB.readDouble(dr["PREVPERIOD"], 0);
            u.NUMBEROFCONTACTS = DB.readDouble(dr["TOTAL"], 0);
        }

        // This is for debugging purposes - all data used in the report can be viewed in it's final state in this saved file.
        //Utility.dataTableToCSVFile(ds.Tables[0], string.Format(@"C:\Temp\final.csv"));

        return lData;
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

                    // If the description has upgrade, continue to look for higher values.
                    // EG. Full Page Select Suburb (Winter Deal Upgrade from 1/2 Page)
                    if (!szDescription.Contains("UPGRADE"))
                        break;
                }
            }
        }
    }

    // inserts data from CampaignDB object into DataSet table[0]
    private void processAppraisalData(DataTable dt) {
        int CurrAgentID = -1;
        ReportData u = null;

        foreach (DataRow dr in dt.Rows) {
            int AgentID = DB.readInt(dr["AGENTID"]);

            if (CurrAgentID == -1 || CurrAgentID != AgentID) {
                u = findDataRow(AgentID);
                CurrAgentID = AgentID;
            }
            u.AppraisalCount += DB.readInt(dr["APPRAISAL_COUNT"]);
            u.AppraisalCountPeriod += DB.readInt(dr["APPRAISAL_COUNT_CURRENT"]);
            u.APPRAISALS_FOLLOWUP += DB.readInt(dr["APPRAISAL_FOLLOWUP_COUNT"]);
            u.APPRAISALS_FOLLOWUPPERIOD += DB.readInt(dr["APPRAISAL_FOLLOWUP_COUNT_CURRENT"]);
            u.PersonalCount += DB.readInt(dr["PERSONAL_APPRAISAL_COUNT"]);
            u.PersonalCountPeriod += DB.readInt(dr["PERSONAL_APPRAISAL_COUNT_CURRENT"]);
            u.CompanyCount += DB.readInt(dr["COMPANY_APPRAISAL_COUNT"]);
            u.CompanyCountPeriod += DB.readInt(dr["COMPANY_APPRAISAL_COUNT_CURRENT"]);
        }
    }

    // List of Campaign values requiring a VALUE_current
    private string[] CampaignCreateCurrentValue = new string[] { "TOTALPREPAID", "TOTALSPENT" };

    // inserts data from CampaignDB object into DataSet table[0]
    private void processCampaignData(ref DataSet ds) {
        string szCampaignFilterSQL = String.Format(@" WHERE ADDRESS1 NOT LIKE '%PROMO,%' AND C.STARTDATE {0}", szCurrentPeriodFilter);
        string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

        DataTable dtCampaign = CampaignDB.loadPrePaymentCampaignData(szCampaignFilterSQL, szCompanyIDList, szCurrentPeriodFilter: szCurrentPeriodFilter);

        // Iterates through Campaign table and copies values to Office Agents table.
        foreach (DataRow dr in dtCampaign.Rows) {
            // does not copy 'totals' rows - indicated by lack of ID value in row
            // also exculdes rows that do not relate to an agent (have no agent ID)
            if (dr["ID"] == System.DBNull.Value || dr["AGENTID"] == System.DBNull.Value)
                continue;
            int AgentID = DB.readInt(dr["AGENTID"]);
            ReportData d = findDataRow(AgentID);

            d.CampaignCount += 1;
            d.CampaignTotalPrePaid += DB.readDouble(dr["TOTALPREPAID"]);
            d.CampaignTotalSpent += DB.readDouble(dr["TOTALSPENT"]);
            if (Convert.ToString(dr["PERIOD"]) == "Current") {
                d.CampaignCountPeriod += 1;
                d.CampaignTotalPrePaidPeriod += DB.readDouble(dr["TOTALPREPAID"]);
                d.CampaignTotalSpentPeriod += DB.readDouble(dr["TOTALSPENT"]);
            }
        }
    }

    /// <summary>
    /// Finds the user data row or returns a new one
    /// </summary>
    /// <param name="AgentID"></param>
    /// <returns></returns>
    public ReportData findDataRow(int AgentID) {
        ReportData d = lData.Find(u => u.AgentID == AgentID);
        if (d == null) {
            UserDetail ud = G.UserInfo.getUser(AgentID);
            d = new ReportData();
            d.AgentName = ud.Name;
            lData.Add(d);
        }
        return d;
    }

    public override DataSet getData() {
        return new DataSet();
    }
}

public class ReportData {
    public string AgentName { get; set; }
    public string OfficeName { get; set; }
    public double BudgetAmount { get; set; }
    public double ActualAmount { get; set; }
    public int OfficeID { get; set; }
    public int AgentID { get; set; }

    //Pre listing
    public string bAPPRAISALSTOLISTINGS { get; set; }

    public string APPRAISALSTOLISTINGS {
        get {
            return PersonalCount.ToString() + ":" + ListingCount.ToString();
        }
    }

    public string APPRAISALSTOLISTINGSPERIOD {
        get {
            return PersonalCountPeriod.ToString() + ":" + ListingCountPeriod.ToString();
        }
    }

    public double AppraisalCount { get; set; }
    public double AppraisalCountPeriod { get; set; }
    public double PersonalCount { get; set; }
    public double PersonalCountPeriod { get; set; }
    public double CompanyCount { get; set; }
    public double CompanyCountPeriod { get; set; }

    public string bAPPRAISALSPvsC { get; set; }

    public string APPRAISALSPvsC {
        get {
            return PersonalCount.ToString() + ":" + CompanyCount.ToString();
        }
    }

    public string APPRAISALSPvsCPERIOD {
        get {
            return PersonalCountPeriod.ToString() + ":" + CompanyCountPeriod.ToString();
        }
    }

    public double bAPPRAISALS_ABC { get; set; }
    public double APPRAISALS_ABC { get { return AppraisalCount; } }
    public double APPRAISALS_ABCPERIOD { get { return AppraisalCountPeriod; } }

    public double bAPPRAISALSCATEGORY_A { get; set; }

    public double APPRAISALSCATEGORY_A { get; set; }

    public double APPRAISALSCATEGORY_APERIOD { get; set; }

    public double bAPPRAISALS_FOLLOWUP { get; set; }
    public double APPRAISALS_FOLLOWUP { get; set; }
    public double APPRAISALS_FOLLOWUPPERIOD { get; set; }

    public double bNUMBEROFCONTACTS { get; set; }
    public double NUMBEROFCONTACTS { get; set; }
    public double NUMBEROFCONTACTSPERIOD { get; set; }

    //Auction data
    public double AuctionCountTotal { get; set; }

    public double AuctionCountPeriod { get; set; }
    public double AuctionSoldTotal { get; set; }
    public double AuctionSoldPeriod { get; set; }

    /// <summary>
    /// Total number of days for all auctions - used to generate the avg
    /// </summary>
    public double AuctionTotalDays { get; set; }

    /// <summary>
    /// Total number of days for all auctions in the period- used to generate the avg
    /// </summary>

    public double AuctionTotalDaysPeriod { get; set; }

    //Private days
    public double PrivateCountTotal { get; set; }

    public double PrivateCountPeriod { get; set; }
    public double PrivateSoldTotal { get; set; }
    public double PrivateSoldPeriod { get; set; }

    /// <summary>
    /// Total number of days for all auctions - used to generate the avg
    /// </summary>
    public double PrivateTotalDays { get; set; }

    /// <summary>
    /// Total number of days for all auctions in the period- used to generate the avg
    /// </summary>

    public double PrivateTotalDaysPeriod { get; set; }

    public double bAUCTIONCLEARANCE { get; set; }

    public double AUCTIONCLEARANCE {
        get {
            if (AuctionCountTotal == 0)
                return 0;
            return AuctionSoldTotal / AuctionCountTotal;
        }
    }

    public double AUCTIONCLEARANCEPERIOD {
        get {
            if (AuctionCountPeriod == 0 || AuctionSoldPeriod == 0)
                return 0;
            return AuctionSoldPeriod / AuctionCountPeriod;
        }
    }

    public double bAVGCOMMISION { get; set; }

    public double AVGCOMMISION {
        get {
            if (TotalSaleCount == 0)
                return 0;
            return TotalCommission / TotalSaleCount;
        }
    }

    public double AVGCOMMISIONPERIOD {
        get {
            if (TotalSaleCountPeriod == 0)
                return 0;
            return TotalCommissionPeriod / TotalSaleCountPeriod;
        }
    }

    public double bAVGCOMMISIONPERCENT { get; set; }

    public double AVGCOMMISIONPERCENT {
        get {
            if (TotalSalePrice == 0)
                return 0;
            return TotalCommission / TotalSalePrice;
        }
    }

    public double AVGCOMMISIONPERCENTPERIOD {
        get {
            if (TotalSalePricePeriod == 0)
                return 0;
            return TotalCommissionPeriod / TotalSalePricePeriod;
        }
    }

    public double bAVGSALEPRICE { get; set; }

    public double AVGSALEPRICE {
        get {
            if (TotalSaleCount == 0)
                return 0;
            return TotalSalePrice / TotalSaleCount;
        }
    }

    public double AVGSALEPRICEPERIOD {
        get {
            if (TotalSaleCountPeriod == 0)
                return 0;
            return TotalSalePricePeriod / TotalSaleCountPeriod;
        }
    }

    public double bGLOSSIESAVG { get; set; }
    public double GLOSSIESAVG { get; set; }
    public double GLOSSIESAVGPERIOD { get; set; }

    public double GlossyCountPeriod { get; set; }
    public double GlossyCount { get; set; }
    public double GlossyTotalValuePeriod { get; set; }
    public double GlossyTotalValue { get; set; }

    public double bGLOSSIESPERLISTING { get; set; }
    public double GLOSSIESPERLISTING { get; set; }
    public double GLOSSIESPERLISTINGPERIOD { get; set; }

    public double bNUMBEROFDAYSAUCTION { get; set; }

    public double NUMBEROFDAYSAUCTION {
        get {
            if (AuctionTotalDays == 0 || AuctionSoldTotal == 0)
                return 0;
            return AuctionTotalDays / AuctionSoldTotal;
        }
    }

    public double NUMBEROFDAYSAUCTIONPERIOD {
        get {
            if (AuctionTotalDaysPeriod == 0 || AuctionSoldPeriod == 0)
                return 0;
            return AuctionTotalDaysPeriod / AuctionSoldPeriod;
        }
    }

    public double bNUMBEROFDAYSPRIVATE { get; set; }

    public double NUMBEROFDAYSPRIVATE {
        get {
            if (PrivateSoldTotal == 0)
                return 0;
            return PrivateTotalDays / PrivateSoldTotal;
        }
    }

    public double NUMBEROFDAYSPRIVATEPERIOD {
        get {
            if (PrivateSoldPeriod == 0)
                return 0;
            return PrivateTotalDaysPeriod / PrivateSoldPeriod;
        }
    }

    public double bPLACES { get; set; }

    public double PLACES {
        get {
            if (CampaignCount == 0)
                return 0;
            return PlacesCount / CampaignCount;
        }
    }

    public double PLACESPERIOD {
        get {
            if (CampaignCountPeriod == 0)
                return 0;
            return PlacesCountPeriod / CampaignCountPeriod;
        }
    }

    public double PlacesCountPeriod { get; set; }
    public double PlacesCount { get; set; }

    public double bPROPERTYVIDEO { get; set; }

    public double PROPERTYVIDEO {
        get {
            if (CampaignCount == 0)
                return 0;
            return VideoCount / CampaignCount;
        }
    }

    public double PROPERTYVIDEOPERIOD {
        get {
            if (CampaignCountPeriod == 0)
                return 0;
            return VideoCountPeriod / CampaignCountPeriod;
        }
    }

    public double VideoCountPeriod { get; set; }
    public double VideoCount { get; set; }

    public double bTOTALLISTINGS { get; set; }
    public double TOTALLISTINGSPERIOD { get; set; }
    public double TOTALLISTINGS { get; set; }

    public double bWITHDRAWNLISTINGS { get; set; }
    public double WITHDRAWNLISTINGS {
        get {
            if (ListingCount  == 0)
                return 0;
            return WithdrawListingsCount / ListingCount;
        }
    }
    public double WITHDRAWNLISTINGSPERIOD {

        get {
            if (ListingCountPeriod == 0)
                return 0;
            return WithdrawListingsCountPeriod / ListingCountPeriod;
        }
    }
    public double WithdrawListingsCount { get; set; }
    public double WithdrawListingsCountPeriod { get; set; }

    public double bADVERTISINGPERPROPERTY { get; set; }

    public double ADVERTISINGPERPROPERTY {
        get {
            if (ListingCount == 0)
                return 0;
            return TotalAdvertising / ListingCount;
        }
    }

    public double ADVERTISINGPERPROPERTYPERIOD {
        get {
            if (ListingCountPeriod == 0)
                return 0;
            return TotalAdvertisingPeriod / ListingCountPeriod;
        }
    }

    //Campaign data
    public double CampaignCountPeriod { get; set; }

    public double CampaignCount { get; set; }
    public double CampaignTotalPrePaidPeriod { get; set; }
    public double CampaignTotalPrePaid { get; set; }
    public double CampaignTotalSpentPeriod { get; set; }
    public double CampaignTotalSpent { get; set; }

    //Sale data
    public double TotalSalePrice { get; set; }

    public double TotalSalePricePeriod { get; set; }
    public double TotalSaleCount { get; set; }
    public double TotalSaleCountPeriod { get; set; }

    public double TotalAdvertisingPeriod { get; set; }
    public double TotalAdvertising { get; set; }

    public double TotalCommissionPeriod { get; set; }
    public double TotalCommission { get; set; }
    public double ListingCountPeriod { get; set; }
    public double ListingCount { get; set; }

    public void setBudgetValue(string Category, string Value) {
        if (Category == "ADVERTISINGPERPROPERTY")
            bADVERTISINGPERPROPERTY = Convert.ToDouble(Value);
        else if (Category == "APPRAISALS_ABC")
            bAPPRAISALS_ABC = Convert.ToDouble(Value);
        else if (Category == "APPRAISALS_FOLLOWUP")
            bAPPRAISALS_FOLLOWUP = Convert.ToDouble(Value);
        else if (Category == "APPRAISALSCATEGORY_A")
            bAPPRAISALSCATEGORY_A = Convert.ToDouble(Value);
        else if (Category == "APPRAISALSPvsC")
            bAPPRAISALSPvsC = Value;
        else if (Category == "APPRAISALSTOLISTINGS")
            bAPPRAISALSTOLISTINGS = Convert.ToString(Value);
        else if (Category == "AUCTIONCLEARANCE")
            bAUCTIONCLEARANCE = Convert.ToDouble(Value);
        else if (Category == "AVGCOMMISION")
            bAVGCOMMISION = Convert.ToDouble(Value);
        else if (Category == "AVGCOMMISIONPERCENT")
            bAVGCOMMISIONPERCENT = Convert.ToDouble(Value);
        else if (Category == "AVGSALEPRICE")
            bAVGSALEPRICE = Convert.ToDouble(Value);
        else if (Category == "GLOSSIESAVG")
            bGLOSSIESAVG = Convert.ToDouble(Value);
        else if (Category == "GLOSSIESPERLISTING")
            bGLOSSIESPERLISTING = Convert.ToDouble(Value);
        else if (Category == "NUMBEROFCONTACTS")
            bNUMBEROFCONTACTS = Convert.ToDouble(Value);
        else if (Category == "NUMBEROFDAYSAUCTION")
            bNUMBEROFDAYSAUCTION = Convert.ToDouble(Value);
        else if (Category == "NUMBEROFDAYSPRIVATE")
            bNUMBEROFDAYSPRIVATE = Convert.ToDouble(Value);
        else if (Category == "PLACES")
            bPLACES = Convert.ToDouble(Value);
        else if (Category == "PROPERTYVIDEO")
            bPROPERTYVIDEO = Convert.ToDouble(Value);
        else if (Category == "TOTALLISTINGS")
            bTOTALLISTINGS = Convert.ToDouble(Value);
        else if (Category == "WITHDRAWNLISTINGS")
            bWITHDRAWNLISTINGS = Convert.ToDouble(Value);
    }
}