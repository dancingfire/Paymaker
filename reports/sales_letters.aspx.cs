using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace Paymaker {

    public partial class sales_letters : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        private DataView dv = null;
        private DataView dvUserValues = null;
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

            DataSet ds = getData();
            foreach (DataRow dr in ds.Tables[0].Rows) {
                Template oT = new Template(G.Settings.SalesLetterTemplateID);
                int UserID = Convert.ToInt32(dr["ID"]);
                UserDetail oU = G.UserInfo.getUser(UserID);

                dvUserValues = ds.Tables[3].DefaultView;

                double SalesTarget = DB.getScalar("SELECT SALESTARGET FROM DB_USER WHERE ID = " + UserID, 0);
                    
                string FilledTemplate = oT.TemplateHTML;
                FilledTemplate = FilledTemplate.Replace("[AGENTFIRSTNAME]", oU.FirstName);
                FilledTemplate = FilledTemplate.Replace("[AGENTLASTNAME]", oU.LastName);
                FilledTemplate = FilledTemplate.Replace("[AGENTOFFICE]", oU.OfficeName);
                FilledTemplate = FilledTemplate.Replace("[SALESTARGET]", Utility.formatReportMoney(SalesTarget));
                double TravelAllowance = getImportedValue(UserID, "Travel Allowance Budget");
                FilledTemplate = FilledTemplate.Replace("[TRAVELALLOWANCE]", Utility.formatReportMoney(TravelAllowance));
 
                dv = ds.Tables[1].DefaultView;
                   
                FilledTemplate = FilledTemplate.Replace("[BENEFITSPAID]", getBenefitsTable(UserID));
                dv = ds.Tables[2].DefaultView;

                FilledTemplate = FilledTemplate.Replace("[ENTITLEMENTS]", getEntitlementsTable(UserID));

                dr["Template"] = FilledTemplate + "<div class='page-break'></div>";
            }

            gvData.DataSource = ds;
            gvData.DataBind();
              
            
        }

    
        private string getBenefitsTable(int UserID) {
            double dTotal = 0.0;

            String szHTML = @"
                <table style='width: 60%; border: solid 1px silver;  margin:1em auto;'>
                    <tr style='background: #E3EBFD'>
                        <td style='width: 70%'><strong>Description</strong></td>
                        <td><strong>Amount Received</strong></td>
                    </tr>";
            szHTML += getImportedValueHTML(UserID, "Salary/Commissions Paid (Net)", ref dTotal);
            szHTML += getImportedValueHTML(UserID, "Superannuation", ref dTotal);
            szHTML += getExpense(UserID, "EOFY Bonus Comm Paid", 95, ref dTotal);
            szHTML += getExpense(UserID, "Team Mentoring Bonus", 97, ref dTotal);
            szHTML += getImportedValueHTML(UserID, "Directors Allowance Paid", ref dTotal);
            szHTML += getImportedValueHTML(UserID, "Directors Car Allowance Paid", ref dTotal);
            szHTML += getImportedValueHTML(UserID, "Travel Allowance Paid", ref dTotal);
            szHTML += getExpense(UserID, "Auctioneer Fees", 45, ref dTotal);
            szHTML += getExpense(UserID, "Conveyancing Commissions", 44, ref dTotal);
            szHTML += getExpense(UserID, "Finance Commissions", 43, ref dTotal);
            szHTML += getExpense(UserID, "PA 1 Allowance", 40, ref dTotal);
            szHTML += getExpense(UserID, "PA 2 Allowance", 68, ref dTotal);
            szHTML += getExpense(UserID, "PA 3 Allowance", 69, ref dTotal);
            szHTML += getExpense(UserID, "PA 4 Allowance", 70, ref dTotal);
            szHTML += getExpense(UserID, "PA 5 Allowance", 72, ref dTotal);
            szHTML += getExpense(UserID, "Rental Referral Commissions", 42, ref dTotal);
            szHTML += getExpense(UserID, "Letterbox Drop Payments", 56, ref dTotal);
            szHTML += getExpense(UserID, "Mobile Phone Payments", 15, ref dTotal);
            szHTML += getExpense(UserID, "Service Area Assistance Payments", 58, ref dTotal);
            szHTML += getExpense(UserID, "Social Media Payments", 88, ref dTotal);
            szHTML += getExpense(UserID, "Training & Seminar Payments", 57, ref dTotal);
            //szHTML += String.Format("<tr style='border: solid 1px silver'><td>Salary on-costs 7% (Payroll Tax & Workcover)</td><td class='RightMargin' >{0}</td></tr>", Utility.formatReportMoney(dTotal * 0.07));
            //dTotal += dTotal * 0.07;
            szHTML += String.Format("<tr style='border: solid 1px silver'><td></td><td class='RightMargin'><strong>{0}</strong></td></tr>", Utility.formatReportMoney(dTotal));
            return szHTML + "</table>";
        }

        private string getEntitlementsTable(int UserID) {
            double dTotal = 0.0;

            String szHTML = @"
                <table style='width: 60%; border: solid 1px silver;  margin:1em auto;'>
                    <tr class='PrintTableHeader'>
                        <td class='PrintTableHeader' style='width: 70%'><strong>Description</strong></td>
                        <td class='PrintTableHeader'><strong>Maximum Allowance</strong></td>
                    </tr>";

            szHTML += getImportedValueHTML(UserID, "Travel Allowance Budget", ref dTotal, "Travel Allowance achieved FY 20/21");
            szHTML += getImportedValueHTML(UserID, "Directors Allowance Budget", ref dTotal);
            szHTML += getImportedValueHTML(UserID, "Directors Car Allowance Budget", ref dTotal);
            szHTML += getExpense(UserID, "PA 1 Allowance", 40, ref dTotal);
            szHTML += getExpense(UserID, "PA 2 Allowance", 68, ref dTotal);
            szHTML += getExpense(UserID, "PA 3 Allowance", 69, ref dTotal);
            szHTML += getExpense(UserID, "PA 4 Allowance", 70, ref dTotal);
            szHTML += getExpense(UserID, "PA 5 Allowance", 72, ref dTotal);
            szHTML += getExpense(UserID, "Letterbox Drop Allowance", 56, ref dTotal);
            szHTML += getExpense(UserID, "Mobile Phone Allowance", 15, ref dTotal);
            szHTML += getExpense(UserID, "Service Area Allowance", 58, ref dTotal);
            szHTML += getExpense(UserID, "Training & Seminar Allowance", 57, ref dTotal);
            return szHTML + "</table>";
        }

        private string getImportedValueHTML(int UserID, string Account, ref double dTotal, string TableName = "") {
            dvUserValues.RowFilter = String.Format(@"USERID={0} AND CATEGORY = '{1}' ", UserID, Account);
            if (dvUserValues.Count > 0) {
               
                double Amount = DB.readDouble(dvUserValues[0]["AMOUNT"]);
                if (Amount > 0) {
                    dTotal += Amount;
                    if (TableName != "")
                        Account = TableName;
                    return String.Format(@"
                    <tr  style='border: solid 1px silver'><td>{0}</td><td class='RightMargin'>{1}</td></tr>", Account, Utility.formatReportMoney(Amount));
                }
            }
            return "";
        }

        private double getImportedValue(int UserID, string Account) {
            dvUserValues.RowFilter = String.Format(@"USERID={0} AND CATEGORY = '{1}' ", UserID, Account);
            if (dvUserValues.Count > 0) {
                return DB.readDouble(dvUserValues[0]["AMOUNT"]);
            }
            return 0.0;
        }

        private string getExpense(int UserID, string Account, int AccountID, ref double dTotal) {
            dv.RowFilter = "ID=" + UserID + " AND ACCOUNTID = " + AccountID;
            double Amount = 0.0;
            if (dv.Count > 0) {
                if(DB.readInt(dv[0]["LISTTYPEID"]) == (int)ListType.Expense) 
                    Amount = DB.readDouble(dv[0]["AMOUNT"]);
                else
                    Amount = DB.readDouble(dv[0]["INCOMEAMOUNT"]);
                
                if (Amount > 0) {
                    dTotal += Amount;
                    return String.Format(@"
                    <tr style='border: solid 1px silver'><td>{0}</td><td class='RightMargin'>{1}</td></tr>", Account, Utility.formatReportMoney(Amount));
                }
            }
            return "";
        }

        public DataSet getData() {
            ReportFilters oFilter = new ReportFilters();
            oFilter.loadUserFilters();
            string szFilter = "";
            if (!String.IsNullOrWhiteSpace(oFilter.UserIDList))
                szFilter += " AND U.ID IN (" + oFilter.UserIDList + ")";
            
            bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
            string szUserActive = " AND U.ISACTIVE = 1 ";
            if (blnIncludeInactive)
                szUserActive = "";

            string szSQL = string.Format(@"
                --0) Users
                Select *, '' AS TEMPLATE
                FROM DB_USER U WHERE 1=1 {0}

                -- 1) Paid
                SELECT U.ID, SUM(T.AMOUNT) AS INCOMEAMOUNT, SUM(T.FLETCHERCONTRIBTOTAL) AS AMOUNT, 
                    T.ACCOUNTID, L_ACCOUNT.NAME, U.LASTNAME, MAX(L_ACCOUNT.LISTTYPEID) AS LISTTYPEID
                FROM USERTX T JOIN DB_USER U ON T.USERID = U.ID 
                JOIN LIST L_ACCOUNT ON L_ACCOUNT.ID = T.ACCOUNTID
                WHERE T.TXDATE BETWEEN 'JUL 1, {2} 00:00' AND 'JUN 30, {3} 2:59' {0}
                GROUP BY U.ID,  T.ACCOUNTID, L_ACCOUNT.NAME, U.LASTNAME;

                -- 2) Budget Query
                SELECT U.ID, LASTNAME + ', ' + FIRSTNAME AS AGENT, UA.ACCOUNTID, SUM(UA.AMOUNT) AS AMOUNT,
                    L_ACCOUNT.Name, 2 AS LISTTYPEID
                FROM DB_USER U
                JOIN USERACCOUNT UA ON UA.USERID = U.ID
                JOIN LIST L_OFFICE ON L_OFFICE.ID = U.OFFICEID
				JOIN LIST L_ACCOUNT ON L_ACCOUNT.ID = UA.ACCOUNTID
                WHERE 1=1 {0}
				GROUP BY U.ID, U.LASTNAME, U.FIRSTNAME,UA.ACCOUNTID, L_ACCOUNT.Name
                ORDER by U.LASTNAME, U.FIRSTNAME, UA.ACCOUNTID;

                -- 3) Uploaded values
                SELECT * FROM USERVALUE
            ", szFilter, szUserActive, oFilter.getSingleFinancialYear() - 1, oFilter.getSingleFinancialYear());

            DataSet ds = DB.runDataSet(szSQL);
            return ds;
        }
    }
}
