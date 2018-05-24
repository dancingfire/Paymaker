using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class tx_search : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack) {
            initFilters();
        }
    }

    private void initFilters() {
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

        //Expense list
        string szSQL = string.Format(@"
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

        Utility.BindList(ref lstUser, DB.runDataSet(@"
                SELECT ID, LASTNAME + ', ' + FIRSTNAME AS NAME FROM DB_USER WHERE ISACTIVE = 1 AND ISDELETED = 0 ORDER BY LASTNAME, FIRSTNAME"), "ID", "NAME");
        lstUser.Items.Insert(0, new ListItem("All...", ""));
    }

    private void showData() {
        string szWhere = "";
        string ExpenseCategoryIDList = Valid.getText("hdExpenseIDList", "", VT.TextNormal);
        if (ExpenseCategoryIDList == "null" || ExpenseCategoryIDList == "-1")
            ExpenseCategoryIDList = "";

        if (ExpenseCategoryIDList != "")
            szWhere += String.Format(" AND TX.ACCOUNTID IN ({0}) ", DB.escape(ExpenseCategoryIDList));
        if (txtStartDate.Text != "")
            szWhere += String.Format(" AND TX.TXDATE >= '{0} 00:00:00' ", Utility.formatDate(txtStartDate.Text));

        if (txtStartDate.Text != "")
            szWhere += String.Format(" AND TX.TXDATE <= '{0}  23:59:59' ", Utility.formatDate(txtEndDate.Text));

        if (txtDescFilter.Text != "")
            szWhere += String.Format(" AND TX.COMMENT like '%{0}%' ", DB.escape(txtDescFilter.Text));

        if (lstUser.SelectedValue != "")
            szWhere += String.Format(" AND TX.USERID IN ({0}) ", lstUser.SelectedValue);

        string szSQL = String.Format(@"
            SELECT TOP 500 TX.*, L.NAME AS ACCOUNT,LASTNAME + ', ' +  FIRSTNAME as [USER], C.NAME AS CATEGORY
            FROM USERTX TX
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN DB_USER U ON U.ID = TX.USERID AND TX.ISDELETED = 0
            LEFT JOIN LIST C ON C.ID = TX.TXCATEGORYID
            WHERE 1=1 {0}
            ORDER BY [USER], TX.TXDATE DESC"
           , szWhere);

        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Tx", "User transaction", "630px", 600, "tx_update.aspx")));
        gvTXs.DataSource = DB.runDataSet(szSQL);
        gvTXs.DataBind();
        HTML.formatGridView(ref gvTXs, true);
    }

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "viewTx(" + szID + ")";
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
        showData();
    }
}