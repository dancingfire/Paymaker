using System;
using System.Data;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_dashboard : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        showData();
        if (!Page.IsPostBack) {
            Utility.loadPayPeriodList(ref lstPayPeriod);
            for (int intMonth = 11; intMonth >= 0; intMonth--) {
                DateTime dtMonth = DateTime.Now.AddMonths(-intMonth);
                lstSaleMonth.Items.Insert(0, new ListItem(String.Format("{0:MMM yy}", dtMonth), intMonth.ToString()));
            }
            lstSaleMonth.Items.Insert(0, new ListItem("All..", ""));
        }
    }

    public string getPayPeriod(int PayPeriodID) {
        if (PayPeriodID == -1)
            return "";
        else {
            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(PayPeriodID);
            if (oP != null)
                return oP.StartDate.ToString("MMM");
        }
        return "";
    }

    private void showData() {
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Sale", "Sale", "95%", -1, "sale_update.aspx")));
        if ((lstPayPeriod.SelectedValue == "" || Convert.ToInt32(lstPayPeriod.SelectedValue) == G.CurrentPayPeriod) && lstSaleMonth.SelectedValue == "")
            gvCurrent.DataSource = loadSalesForAdminPerson(-1, G.CurrentPayPeriodStart, DateTime.MaxValue, DateTime.MinValue, txtPropertyFilter.Text, lstStatus.SelectedValue);
        else {
            DateTime dtStart = DateTime.MinValue;
            DateTime dtEnd = DateTime.MaxValue;
            if (lstPayPeriod.SelectedValue != "") {
                string szSQL = "SELECT * FROM PAYPERIOD WHERE ID = " + lstPayPeriod.SelectedValue;
                DataSet ds = DB.runDataSet(szSQL);
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    dtStart = Convert.ToDateTime(dr["STARTDATE"]);
                    dtEnd = Convert.ToDateTime(dr["ENDDATE"]);
                }
            }

            DateTime dtSaleMonth = DateTime.MinValue;
            if (lstSaleMonth.SelectedValue != "") {
                int intMonth = Convert.ToInt32(lstSaleMonth.SelectedValue);
                dtSaleMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0).AddMonths(-intMonth);
            }

            int intPayPeriod = -1;
            if (lstPayPeriod.SelectedValue != "")
                intPayPeriod = Convert.ToInt32(lstPayPeriod.SelectedValue);

            gvCurrent.DataSource = loadSalesForAdminPerson(intPayPeriod, dtStart, dtEnd, dtSaleMonth, txtPropertyFilter.Text, lstStatus.SelectedValue);
        }
        gvCurrent.DataBind();
        HTML.formatGridView(ref gvCurrent, true);
    }

    /// <summary>
    /// Loads all the records where a salesperson has a part of the splits
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    private static DataSet loadSalesForAdminPerson(int PayPeriodID, DateTime dtStartDate, DateTime dtEndDate, DateTime dtSaleMonth, string LocationFilter = "", string StatusFilter = "") {
        string szFilter = "";
        if (LocationFilter != "")
            szFilter += String.Format(" AND CODE LIKE '%{0}%' or S.ADDRESS LIKE '%{0}%' ", Utility.formatForDB(LocationFilter));
        string szStatusFilter = " AND STATUSID < 3";
        if (StatusFilter != "")
            szStatusFilter = String.Format(" AND STATUSID = {0}", Utility.formatForDB(StatusFilter));

        if (PayPeriodID == -1)
            szFilter += String.Format(" AND S.ENTITLEMENTDATE BETWEEN '{0} 00:00:00' AND '{1} 23:59:59'", Utility.formatDate(dtStartDate), Utility.formatDate(dtEndDate));
        else
            szFilter += " AND S.PAYPERIODID = " + PayPeriodID;

        if (dtSaleMonth != DateTime.MinValue)
            szFilter += String.Format(" AND S.SALEDATE BETWEEN '{0} 00:00:00' AND '{1} 23:59:59'", Utility.formatDate(dtSaleMonth), Utility.formatDate(dtSaleMonth.AddMonths(1).AddDays(-1)));

        string szSQL = String.Format(@"
            SELECT S.*, ISNULL(S.PAYPERIODID, -1) AS SAFEPAYPERIODID
            FROM SALE S
            WHERE  STATUSID != 3 {0} {1}
            ORDER BY S.ENTITLEMENTDATE ", szFilter, szStatusFilter);

        return DB.runDataSet(szSQL);
    }

 

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "updateSale(" + szID + ")";
        }
    }

    public string getStatus(int StatusID, bool IsSourceModified) {
        string szReturn = "";
        szReturn = "Incomplete";
        if (StatusID == 1)
            szReturn = "Complete";
        else if (StatusID == 2)
            szReturn = "Finalized";
        else if (StatusID == 3)
            szReturn = "Hidden";

        if (StatusID < 2 && IsSourceModified)
            szReturn += " <strong>Modified</strong>";
        return szReturn;
    }

    public string getPercentage(double SalePrice, double Commission) {
        return Math.Round((Commission / SalePrice * 100), 2).ToString();
    }

    public string getHasBnD(int BnDSaleID) {
        return BnDSaleID == Int32.MinValue ? "No" : "Yes";
    }

    protected string getSettlementDate(string szSettlementDate) {
        if (szSettlementDate == "" || Convert.ToDateTime(szSettlementDate) == DateTime.MinValue) {
            return "";
        }
        return Utility.formatDate(szSettlementDate);
    }
}