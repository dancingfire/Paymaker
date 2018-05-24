using System;
using System.Web;
using System.Web.UI.WebControls;

public partial class multiple_tx_update : Root {
    protected int intTxID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);

        if (!IsPostBack) {
            sbStartJS.Append(@"blnAllowClose = false;");
            bindData();
            loadDefaults();
        }
    }

    private void bindData() {
        string szSQL = String.Format("Select ID, Name from LIST where LISTTYPEID = {0} ORDER BY NAME", (int)ListType.Expense);
        Utility.BindList(ref lstExpenseAccounts_ROWNUM, DB.runReader(szSQL), "ID", "NAME");
        lstExpenseAccounts_ROWNUM.Items.Insert(0, new ListItem("Select an account...", "-1"));

        szSQL = String.Format("Select ID, Name from LIST where LISTTYPEID = {0} ORDER BY NAME", (int)ListType.Income);
        Utility.BindList(ref lstIncomeAccounts_ROWNUM, DB.runReader(szSQL), "ID", "NAME");
        lstIncomeAccounts_ROWNUM.Items.Insert(0, new ListItem("Select an account...", "-1"));

        szSQL = String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0} ORDER BY SEQUENCENO, NAME", (int)ListType.TXCategory);
        Utility.BindList(ref lstCategory_ROWNUM, DB.runReader(szSQL), "ID", "NAME");
        lstCategory_ROWNUM.Items.Insert(0, new ListItem("Select a category...", "-1"));

        Utility.BindList(ref lstUserID_ROWNUM, DB.runReader(@"
            SELECT U.id, U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME AS NAME
            FROM [DB_USER] U
            WHERE U.ISACTIVE = 1
            ORDER BY U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME"), "ID", "NAME");
        lstUserID_ROWNUM.Items.Insert(0, new ListItem("Select a user...", "-1"));
        chkIncludeGST_ROWNUM.Attributes["onclick"] = "getExGSTAmount()";
    }

    private void loadDefaults() {
        txtTxDate.Text = Utility.formatDate(DateTime.Now);
        txtFletcherContribution_ROWNUM.Text = "50";
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        int intTXCount = Valid.getInteger("hdTXCount");

        for (int i = 1; i <= intTXCount; i++) {
            if (Utility.InCommaSeparatedString(i.ToString(), hdSkipIDs.Value))
                continue;
            sqlUpdate oSQL = new sqlUpdate("USERTX", "ID", -1);
            oSQL.add("AMOUNT", Valid.getMoney("txtUserAmount" + i, 0));
            oSQL.add("TOTALAMOUNT", Valid.getMoney("txtAmount" + i, 0));
            oSQL.add("FLETCHERAMOUNT", Valid.getMoney("txtFletcherContribution" + i, 0));
            oSQL.add("FLETCHERCONTRIBTOTAL", Valid.getMoney("hdFletcherAmountCalc" + i, 0));
            oSQL.add("AMOUNTTYPEID", Valid.getInteger("lstAmountType" + i, 0));
            oSQL.add("TXDATE", Utility.formatDate(Valid.getDate("txtTxDate")));
            oSQL.add("CREDITGLCODE", Valid.getText("txtGLCredit" + i, ""));
            oSQL.add("DEBITGLCODE", Valid.getText("txtGLDebit" + i, ""));
            oSQL.add("CREDITJOBCODE", Valid.getText("txtJobCredit" + i, ""));
            oSQL.add("DEBITJOBCODE", Valid.getText("txtJobDebit" + i, ""));
            if (Valid.getCheck("chkIncludeGST" + i))
                oSQL.add("SHOWEXGST", 1);
            else
                oSQL.add("SHOWEXGST", 0);

            if (Valid.getCheck("chkOverrideCodes" + i))
                oSQL.add("OVERRIDEGLCODES", 1);
            else
                oSQL.add("OVERRIDEGLCODES", 0);

            if (Valid.getText("lstType" + i, "") == "EXPENSE")
                oSQL.add("ACCOUNTID", Valid.getInteger("lstExpenseAccounts" + i));
            else
                oSQL.add("ACCOUNTID", Valid.getInteger("lstIncomeAccounts" + i));
            oSQL.add("USERID", Valid.getInteger("lstUserID" + i));
            oSQL.add("COMMENT", Valid.getText("txtComment" + i, ""));
            int intTXCategory = Valid.getInteger("lstCategory" + i, -1);
            if (intTXCategory > -1)
                oSQL.add("TXCATEGORYID", intTXCategory);

            DB.runNonQuery(oSQL.createInsertSQL());
            sbStartJS.Append(" blnAllowClose = true; parent.closeTx(true);");
        }
    }
}