using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class report_admin : Root {

        protected void Page_Load(object sender, System.EventArgs e) {
            if (!Page.IsPostBack) {
                loadFilters();
                formatPage();
            }
        }

        protected void loadFilters() {
            string szSQL = string.Format("select ID, STARTDATE , ENDDATE, '' AS NAME from PAYPERIOD ORDER BY ID DESC");
            DataSet ds = DB.runDataSet(szSQL);
            DateTime dtMin = DateTime.MaxValue;
            DateTime dtMax = DateTime.MinValue;

            foreach (DataRow dr in ds.Tables[0].Rows) {
                if (Convert.ToDateTime(dr["StartDate"]) < dtMin)
                    dtMin = Convert.ToDateTime(dr["StartDate"]);
                if (Convert.ToDateTime(dr["StartDate"]) > dtMax)
                    dtMax = Convert.ToDateTime(dr["StartDate"]);

                dr["NAME"] = Utility.formatDate(dr["StartDate"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
            }
            Utility.BindList(ref lstPayPeriod, ds, "ID", "NAME");
            lstPayPeriod.SelectedIndex = 0;
            dtMax = Utility.getFinYearEnd(dtMax);
            DateTime dtCurr = dtMax;

            //Quarter
            while (dtCurr > dtMin) {
                int intCurrQuarter = ((dtCurr.Month - 1) / 3) + 1;
                DateTime dtFirstDay = new DateTime(dtCurr.Year, (intCurrQuarter - 1) * 3 + 1, 1);
                DateTime dtLastDay = dtFirstDay.AddMonths(3).AddDays(-1);
                lstQuarter.Items.Add(new ListItem(Utility.formatDate(dtFirstDay) + " - " + Utility.formatDate(dtLastDay), "Between  '" + Utility.formatDate(dtFirstDay) + "' AND '" + Utility.formatDate(dtLastDay) + "' "));
                dtCurr = dtFirstDay.AddDays(-1);
            }
            lstQuarter.Items.Insert(0, new ListItem("Select a quarter...", ""));

            dtCurr = dtMax;

            //Month
            while (dtCurr > dtMin) {
                DateTime dtFirstDay = new DateTime(dtCurr.Year, dtCurr.Month, 1);
                DateTime dtLastDay = dtFirstDay.AddMonths(1).AddDays(-1);
                lstMonth.Items.Add(new ListItem(dtFirstDay.ToString("MMM yyyy"), "Between  '" + Utility.formatDate(dtFirstDay) + "' AND '" + Utility.formatDate(dtLastDay) + "' "));
                dtCurr = dtFirstDay.AddDays(-1);
            }
            lstMonth.Items.Insert(0, new ListItem("Select a month...", ""));

            //User
            Utility.BindList(ref lstUser, DB.runDataSet(@"
                SELECT ID, LASTNAME + ', ' + FIRSTNAME AS NAME FROM DB_USER WHERE ISACTIVE = 1 AND ISDELETED = 0 AND ISPAID = 1 ORDER BY LASTNAME, FIRSTNAME"), "ID", "NAME");

            Utility.BindList(ref lstNonAdminUser, DB.runDataSet(String.Format(@"
                SELECT ID, LASTNAME + ', ' + FIRSTNAME AS NAME
                FROM DB_USER
                WHERE ISACTIVE = 1 AND ISDELETED = 0 AND ID IN ({0}, {1})
                ORDER BY LASTNAME, FIRSTNAME", G.User.ID, G.User.AdminPAForThisUser)));

            //Suburb
            szSQL = string.Format("select DISTINCT SUBURB AS ID, SUBURB AS NAME FROM SALE ORDER BY SUBURB ");
            Utility.BindList(ref lstSuburb, DB.runDataSet(szSQL), "ID", "NAME");

            //Offices
            szSQL = string.Format(@"
                SELECT ID, NAME
                FROM LIST
                WHERE LISTTYPEID = {0}
                ORDER BY SEQUENCENO", (int)ListType.Office);
            Utility.BindList(ref lstOffice, DB.runDataSet(szSQL), "ID", "NAME");

            //Companies
            szSQL = string.Format(@"
                SELECT ID, NAME
                FROM LIST
                WHERE LISTTYPEID = {0} AND ISACTIVE = 1
                ORDER BY SEQUENCENO", (int)ListType.Company);
            Utility.BindList(ref lstCompany, DB.runDataSet(szSQL), "ID", "NAME");

            //Expense list
            szSQL = string.Format(@"
                SELECT ID, NAME, LISTTYPEID
                FROM LIST
                WHERE LISTTYPEID IN( {0}, {1})
                ORDER BY LISTTYPEID, NAME", (int)ListType.Expense, (int)ListType.Income);

            ListItem oLI = new ListItem("Expense accounts", "-1");
            oLI.Attributes["style"] = "background: #EFEFED; font-weight: bold";
            lstExpense.Items.Add(oLI);
            int intCurrListTypeID = (int)ListType.Expense;
            foreach (DataRow dr in DB.runDataSet(szSQL).Tables[0].Rows) {
                if (intCurrListTypeID != Convert.ToInt32(dr["LISTTYPEID"])) {
                    oLI = new ListItem("Income accounts", "-1");
                    oLI.Attributes["style"] = "background: #EFEFED; font-weight: bold";
                    lstExpense.Items.Add(oLI);
                    intCurrListTypeID = Convert.ToInt32(dr["LISTTYPEID"]);
                }

                oLI = new ListItem("  " + dr["NAME"].ToString(), dr["ID"].ToString());
                lstExpense.Items.Add(oLI);
            }
            loadFinanacialYearList();

            Utility.BindList(ref lstOffTheTop, DB.runReader("select * from list where LISTTYPEID = 6 order by NAME"), "ID", "NAME");
            lstOffTheTop.Items.Insert(0, new ListItem("All", "-1"));

            Utility.BindList(ref lstRole, DB.runReader("SELECT id, NAME  FROM [ROLE] WHERE ID >0 ORDER BY ID"), "ID", "NAME");
            lstRole.Items.Insert(0, new ListItem("All", "-1"));

            loadDateRange();
        }

        private void loadDateRange() {
            txtStartDate.Attributes["onchange"] = "setDateCustom();";
            txtEndDate.Attributes["onchange"] = "setDateCustom();";
            DateTime dtNow = DateTime.Now;
            DateRange oR = DateUtil.ThisMonth(dtNow);
            txtStartDate.Text = Utility.formatDate(DateUtil.ThisMonth(dtNow).Start);
            txtEndDate.Text = Utility.formatDate(DateUtil.ThisMonth(dtNow).End);

            lstDateRange.Items.Add(new ListItem("This month", DateUtil.ThisMonth(dtNow).DateRangeString));
            lstDateRange.Items.Add(new ListItem("This quarter", DateUtil.ThisQuarter(dtNow).DateRangeString));
            lstDateRange.Items.Add(new ListItem("This financial year", DateUtil.ThisFinYear(dtNow).DateRangeString));
            lstDateRange.Items.Add(new ListItem("Last month", DateUtil.LastMonth(dtNow).DateRangeString));
            lstDateRange.Items.Add(new ListItem("Last quarter", DateUtil.LastQuarter(dtNow).DateRangeString));
            lstDateRange.Items.Add(new ListItem("Last financial year", DateUtil.LastFinYear(dtNow).DateRangeString));
            lstDateRange.Items.Add(new ListItem("Custom", ""));
            lstDateRange.Attributes["onchange"] = "changeDateFilter();";
        }

        private void formatPage() {
            if (!G.User.IsAdmin) {
                sbEndJS.Append("setupForSingleUser()");
                //Show the commission report
                spUser.Visible = false;
                spRecreate.Visible = false;
                spNonAdminUserFilter.Visible = true;

                // Only show the expense summary report - TODO
                if (G.User.hasPermission(RolePermissionType.ReportExpenseSummary)) {
                }
                spCompany.Visible = false;
                hfUserID.Value = G.User.ID.ToString();
            } else {
                string Reports = "";
                //Load up the favourite reports
                using (DataSet ds = DB.runDataSet(String.Format("SELECT * FROM [USERREPORT] WHERE USERID = {0}", G.User.UserID))) {
                    foreach (DataRow dr in ds.Tables[0].Rows) {
                        Utility.Append(ref Reports, DB.readString(dr["REPORTNAME"]), ",");
                    }
                }
                hdFavouriteReports.Value = Reports; 
            }
        }

        protected void loadFinanacialYearList() {
            lstFinancialYear.Items.Clear();
            DateTime oDateNow = DateTime.Now;
            int currYear = oDateNow.Year;
            int currMonth = oDateNow.Month;
            int lastFincialYeartoShow = 0;

            if (currMonth > 6) {
                lastFincialYeartoShow = currYear + 1;
            } else {
                lastFincialYeartoShow = currYear;
            }
            lstFinancialYear.Items.Add(new ListItem("", ""));
            for (int i = 0; i <= 10; i++) {
                int intFY = lastFincialYeartoShow - i;
                lstFinancialYear.Items.Add(new ListItem("FY " + intFY, intFY.ToString()));
            }
        }
    }
}