using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace Paymaker {

    public partial class monthly_sales_agent_expenses : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        /// <summary>
        /// Loads the page with the data according to the received start and end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }

            if (!Page.IsPostBack) {
                Report oReport = getReport();
                rViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                rViewer.ShowParameterPrompts = false;
                rViewer.ShowPageNavigationControls = true;
                rViewer.LocalReport.Refresh();
                rViewer.Visible = true;
            }
        }

        private Report getReport() {
            //Load the parameters from the page
            ReportFilters oFilter = new ReportFilters();
            Report oR = new MonthlySalesAgentExpenses(rViewer, oFilter);
            return oR;
        }
    }
}

public class MonthlySalesAgentExpenses : Report {
    private static string RFileName = "Monthly sales - agent expenses";

    /// <summary>
    /// Constructor Method that receives the Viewer and the Filter and creates the report.
    /// </summary>
    /// <param name="Viewer"></param>
    /// <param name="oFilter"></param>
    public MonthlySalesAgentExpenses(ReportViewer Viewer, ReportFilters oFilter) {
        oViewer = Viewer;
        this.oFilter = oFilter;
        ReportTitle = "Monthly sales agent expenses";
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
        string szFilter = String.Format(" S.SALEDATE BETWEEN '{0}' AND '{1}'", oFilter.getDBSafeStartDate(), oFilter.getDBSafeEndDate());
        if (!String.IsNullOrWhiteSpace(oFilter.OfficeIDList))
            szFilter += String.Format(" AND S.OFFICEID IN ({0})", oFilter.OfficeIDList);
        if (!String.IsNullOrWhiteSpace(oFilter.CompanyID))
            szFilter += String.Format(" AND L_COMPANY.ID IN ({0})", oFilter.CompanyID);
        string szUserIDFilter = Valid.getText("szUserID", "", VT.List);
        if (!G.User.IsAdmin) //Filter for single user mode
            szFilter += " AND USR.TOPPERFORMERREPORTSETTINGS IN (" + G.User.UserID + ") ";
        else if (!String.IsNullOrEmpty(oFilter.UserIDList)) {
            szFilter += " AND USR.TOPPERFORMERREPORTSETTINGS IN (" + oFilter.UserIDList + ")";
        }
        
        bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
        
        if (!G.User.IsAdmin && String.IsNullOrEmpty(szUserIDFilter)) //Filter for single user mode
            szFilter += " AND USS.USERID IN (" + G.User.UserID + ") ";
        else if (!String.IsNullOrEmpty(szUserIDFilter) && !blnIncludeInactive) {
            //Filter by the selected users unless the Include INactive is selected, in which case we want to include everyone
            szFilter += " AND USS.USERID IN (" + szUserIDFilter + ")";
        }
        string szUserActive = " AND USR.ISACTIVE = 1 ";
        if (blnIncludeInactive)
            szUserActive = "";
        string szOrderBy = " S.ADDRESS ";
        if (G.Settings.ClientID == global::ClientID.Eltham)
            szOrderBy = "S.SALEDATE";
        
        string szSQL = string.Format(@"
                SELECT  S.*, ASE.AMOUNT AS EXPENSEAMOUNT, L.NAME AS EXPENSETYPE
                FROM 
                ( 
                    SELECT S.ID, MAX(S.ADDRESS) AS ADDRESS, CAST(CAST(DATEPART(YEAR, S.SALEDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH, S.SALEDATE) AS VARCHAR), 2) AS INT) AS GROUPING,
                    MAX(S.SALEDATE) AS SALEDATE, MAX(S.SALEPRICE) as SALEPRICE, MAX(S.SETTLEMENTDATE) AS SETTLEMENTDATE, MAX(S.GROSSCOMMISSION) as GROSSCOMMISSION, 
                    MAX(S.CONJUNCTIONALCOMMISSION) AS CONJUNCTIONALCOMMISSION, 
                    MAX(USS.GRAPHCOMMISSION) AS CALCULATEDAMOUNT, MAX(L_OFFICE.NAME) AS Office, MAX(A.INITIALSCODE) AS AUCTIONEER,
                    MAX(LIST.INITIALS) AS LISTER, MAX(MANAGE.INITIALS) AS MANAGER, MAX(SELL.INITIALS) AS SELLER, MAX(LEAD.INITIALS) AS LEAD,
                    MAX(SS_COMM.COMMISSIONPERCENT) AS CommissionPercentage, MAX(STATUS.NAME) AS STATUS, MAX(ENTITLEMENTDATE) AS ENTITLEMENTDATE
                    FROM SALE S
                    JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS < 1
                    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                    JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                    JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                    JOIN bdSALESLISTING sl on s.BnDSALEID = sl.id
                    LEFT JOIN DB_USER A ON A.ID = SL.AUCTIONEERID
                    LEFT JOIN SALESSTATUS STATUS ON STATUS.ID = S.STATUSID
                    JOIN (
						SELECT SS.SALEID, SUM(SS.AMOUNT) AS COMMISSIONPERCENT
						FROM SALESPLIT SS  WHERE SS.COMMISSIONTYPEID != 35
						GROUP BY SS.SALEID
						) AS SS_COMM ON SS_COMM.SALEID = S.ID
                    LEFT JOIN (
					    SELECT SS.SALEID, dbo.ufCommissionInitials(sS.SaleID, 6) as INITIALS
					    FROM  SALESPLIT SS 
					    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1  AND SS.RECORDSTATUS < 1
					    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID
					    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
					    WHERE L_SALESPLIT.NAME = 'LEAD'
					    GROUP BY SS.SALEID,  L_SALESPLIT.NAME)  as LEAD ON LEAD.SALEID = S.ID
				    LEFT JOIN (
					    SELECT SS.SALEID, dbo.ufCommissionInitials(sS.SaleID, 10) as INITIALS
					    FROM  SALESPLIT SS 
					    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1  AND SS.RECORDSTATUS < 1
					    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID
					    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
					    WHERE L_SALESPLIT.NAME = 'LISTER'
					    GROUP BY SS.SALEID,  L_SALESPLIT.NAME)  as LIST ON LIST.SALEID = S.ID
				    LEFT JOIN (
					    SELECT SS.SALEID, dbo.ufCommissionInitials(sS.SaleID, 7) as INITIALS
					    FROM  SALESPLIT SS 
					    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1  AND SS.RECORDSTATUS < 1
					    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID
					    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
					    WHERE L_SALESPLIT.NAME = 'MANAGER'
					    GROUP BY SS.SALEID,  L_SALESPLIT.NAME)  as MANAGE ON MANAGE.SALEID = S.ID

				    LEFT JOIN (
					    SELECT SS.SALEID, dbo.ufCommissionInitials(sS.SaleID, 8) as INITIALS
					    FROM  SALESPLIT SS 
					    JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1  AND SS.RECORDSTATUS < 1
					    JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID
					    JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
					    WHERE L_SALESPLIT.NAME = 'SELLER'
					    GROUP BY SS.SALEID,  L_SALESPLIT.NAME)  as SELL ON SELL.SALEID = S.ID
                    
                    WHERE {0} AND SS.CALCULATEDAMOUNT > 0 {1}
                    GROUP BY CAST(CAST(DATEPART(YEAR, S.SALEDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH, S.SALEDATE) AS VARCHAR), 2) AS INT), S.ID, SUBSTRING(S.ADDRESS, CHARINDEX(' ', S.ADDRESS) + 1, LEN(S.ADDRESS))
                ) AS S LEFT JOIN AGENTSALEEXPENSE ASE ON ASE.SALEID = S.ID 
                    LEFT JOIN LIST L ON ASE.EXPENSETYPEID = L.ID
                ORDER BY CAST(CAST(DATEPART(YEAR, S.SALEDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH, S.SALEDATE) AS VARCHAR), 2) AS INT), {2}, S.ID"
            , szFilter, szUserActive, szOrderBy);

        return DB.runDataSet(szSQL);
    }

    protected DataSet formatDataSet(DataSet ds) {
        int currSaleID = -1;
        DataSet dsClone = ds.Clone();
        dsClone.Tables[0].Columns.Remove("SALEDATE");
        dsClone.Tables[0].Columns.Remove("SETTLEMENTDATE");
        dsClone.Tables[0].Columns.Remove("SALEPRICE");
        dsClone.Tables[0].Columns.Remove("GROSSCOMMISSION");
        dsClone.Tables[0].Columns.Remove("CONJUNCTIONALCOMMISSION");

        dsClone.Tables[0].Columns.Add(new DataColumn("SaleDate", System.Type.GetType("System.String")));
        dsClone.Tables[0].Columns.Add(new DataColumn("SettlementDate", System.Type.GetType("System.String")));
        dsClone.Tables[0].Columns.Add(new DataColumn("OfficeComission", System.Type.GetType("System.Double")));
        dsClone.Tables[0].Columns.Add(new DataColumn("SalePrice", System.Type.GetType("System.Double")));
        dsClone.Tables[0].Columns.Add(new DataColumn("GrossCommission", System.Type.GetType("System.Double")));
        dsClone.Tables[0].Columns.Add(new DataColumn("ConjunctionalCommission", System.Type.GetType("System.Double")));
        dsClone.Tables[0].Columns.Add(new DataColumn("AgentCommission", System.Type.GetType("System.Double")));

        dsClone.Tables[0].Columns.Add(new DataColumn("Lead", System.Type.GetType("System.String")));
        dsClone.Tables[0].Columns.Add(new DataColumn("Lister", System.Type.GetType("System.String")));
        dsClone.Tables[0].Columns.Add(new DataColumn("Manager", System.Type.GetType("System.String")));
        dsClone.Tables[0].Columns.Add(new DataColumn("Seller", System.Type.GetType("System.String")));
        dsClone.Tables[0].Columns.Add(new DataColumn("Mentor", System.Type.GetType("System.String")));

        DataRow oCloneRow = null;
        DataRow oTotalRow = dsClone.Tables[0].NewRow();
        oTotalRow["SALEPRICE"] = 0;
        oTotalRow["GROSSCOMMISSION"] = 0;
        oTotalRow["CONJUNCTIONALCOMMISSION"] = 0;
        oTotalRow["OFFICECOMMISSION"] = 0;
        oTotalRow["AGENTCOMMISSION"] = 0;
        int intSaleCount = 0;

        foreach (DataRow oR in ds.Tables[0].Rows) {
            if (currSaleID == -1 || currSaleID != Convert.ToInt32(oR["ID"])) {
                currSaleID = Convert.ToInt32(oR["ID"]);
                if (oCloneRow != null) {
                    oCloneRow["OFFICECOMMISSION"] = Utility.formatMoneyShort(readDouble(oCloneRow["OFFICECOMMISSION"]));
                    dsClone.Tables[0].Rows.Add(oCloneRow);
                    intSaleCount++;
                }
                oCloneRow = dsClone.Tables[0].NewRow();
                oCloneRow["AGENTCOMMISSION"] = 0;
                oCloneRow["SALEPRICE"] = 0;
                oCloneRow["OFFICECOMMISSION"] = 0;
                oCloneRow["GROUPING"] = oR["GROUPING"];
                oCloneRow["ID"] = oR["ID"];
                string szAddress = oR["Address"].ToString();
                szAddress = Regex.Replace(szAddress, @",[\s]*Vic[\s]*,[\s]*[\d]{4}", "", RegexOptions.IgnoreCase);
                oCloneRow["Address"] = szAddress;
                oCloneRow["SALEDATE"] = Utility.formatDate(oR["SALEDATE"].ToString());
                oCloneRow["SETTLEMENTDATE"] = Utility.formatDate(oR["SETTLEMENTDATE"].ToString());

                oCloneRow["SALEPRICE"] = Utility.formatMoneyShort(readDouble(oR["SALEPRICE"]));
                oTotalRow["SALEPRICE"] = readDouble(oTotalRow["SALEPRICE"]) + readDouble(oR["SALEPRICE"]);

                oCloneRow["GROSSCOMMISSION"] = Utility.formatMoneyShort(readDouble(oR["GROSSCOMMISSION"]));
                oTotalRow["GROSSCOMMISSION"] = readDouble(oTotalRow["GROSSCOMMISSION"]) + readDouble(oR["GROSSCOMMISSION"]);

                oCloneRow["CONJUNCTIONALCOMMISSION"] = Utility.formatMoneyShort(readDouble(oR["CONJUNCTIONALCOMMISSION"]));
                oTotalRow["CONJUNCTIONALCOMMISSION"] = readDouble(oTotalRow["CONJUNCTIONALCOMMISSION"]) + readDouble(oR["CONJUNCTIONALCOMMISSION"]);
            }
            oCloneRow["OFFICECOMMISSION"] = readDouble(oCloneRow["OFFICECOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);
            oTotalRow["OFFICECOMMISSION"] = readDouble(oTotalRow["OFFICECOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);

            oCloneRow["AGENTCOMMISSION"] = readDouble(oCloneRow["AGENTCOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);
            oTotalRow["AGENTCOMMISSION"] = readDouble(oTotalRow["AGENTCOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);

            switch (oR["COMMISSIONNAME"].ToString()) {
                case "Lead":
                    oCloneRow["Lead"] = Utility.Append(oCloneRow["Lead"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                    break;

                case "Lister":
                    oCloneRow["Lister"] = Utility.Append(oCloneRow["Lister"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                    break;

                case "Manager":
                    oCloneRow["Manager"] = Utility.Append(oCloneRow["Manager"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                    break;

                case "Seller":
                    oCloneRow["Seller"] = Utility.Append(oCloneRow["Seller"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                    break;

            }
        }
        if (oCloneRow != null) {
            oCloneRow["OFFICECOMMISSION"] = Utility.formatMoneyShort(readDouble(oCloneRow["OFFICECOMMISSION"]));
            dsClone.Tables[0].Rows.Add(oCloneRow);
            intSaleCount++;
        }

        return dsClone;
    }

    private double readDouble(object o) {
        if (o == null || o == System.DBNull.Value)
            return 0;
        else
            return Convert.ToDouble(o);
    }

}
