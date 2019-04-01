using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class tx_update : Root {
    protected int intTxID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        intTxID = Valid.getInteger("intItemID");
        hdTXID.Value = intTxID.ToString();
        btnDelete.Attributes.Add("onClick", "return confirm('Are you sure you want to delete this transaction?')");
        if (!IsPostBack) {
            bindData();
            if (intTxID == -1)
                loadDefaults();
            else
                loadTx();
        }
    }

    private void bindData() {
        string szSQL = String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0} ORDER BY SEQUENCENO, NAME", (int)ListType.Expense);
        Utility.BindList(ref lstExpenseAccounts, DB.runReader(szSQL), "ID", "NAME");
        lstExpenseAccounts.Items.Insert(0, new ListItem("Select an account...", "-1"));

        szSQL = String.Format("Select ID, Name from LIST where LISTTYPEID = {0} ORDER BY SEQUENCENO, NAME", (int)ListType.Income);
        Utility.BindList(ref lstIncomeAccounts, DB.runReader(szSQL), "ID", "NAME");
        lstIncomeAccounts.Items.Insert(0, new ListItem("Select an account...", "-1"));

        szSQL = String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0} ORDER BY SEQUENCENO, NAME", (int)ListType.TXCategory);
        Utility.BindList(ref lstCategory, DB.runReader(szSQL), "ID", "NAME");
        lstCategory.Items.Insert(0, new ListItem("Select a category...", "-1"));

        Utility.BindList(ref lstUserID, DB.runReader(@"
            SELECT U.id, U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME AS NAME
            FROM [DB_USER] U
            WHERE U.ISACTIVE = 1
            ORDER BY U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME"), "ID", "NAME");
        lstUserID.Items.Insert(0, new ListItem("Select a user...", "-1"));
        using (DataSet ds = DB.runDataSet(@"
            SELECT L_OFF.OFFICEMYOBCODE + '-' + U.INITIALSCODE  AS NAME , L_OFF.OFFICEMYOBCODE + '-' + U.INITIALSCODE  AS ID 
            FROM DB_USER U 
            JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID
            WHERE U.ISACTIVE = 1 AND U.INITIALSCODE != ''  AND U.ISDELETED = 0 AND U.ID > 0
                AND L_OFF.OFFICEMYOBCODE + '-' + U.INITIALSCODE IS NOT NULL
            ORDER BY L_OFF.OFFICEMYOBCODE + '-' + U.INITIALSCODE")) {
            Utility.BindList(ref txtJobCredit, ds);
            txtJobCredit.Items.Insert(0, new ListItem(""));
            Utility.BindList(ref txtJobDebit, ds);
            txtJobDebit.Items.Insert(0, new ListItem(""));
        }

        rbIncome.Attributes["onclick"] = "showAccountList(true);";
        rbExpense.Attributes["onclick"] = "showAccountList(true);";
        chkIncludeGST.Attributes["onclick"] = "getExGSTAmount()";

        txtGLCredit.Attributes["onchange"] = "lockAccounts();";
        txtGLDebit.Attributes["onchange"] = "lockAccounts();";
        txtJobCredit.Attributes["onchange"] = "lockAccounts();";
        txtJobDebit.Attributes["onchange"] = "lockAccounts();";
    }

    private void loadDefaults() {
        txtTxDate.Text = Utility.formatDate(DateTime.Now);
        if (!string.IsNullOrEmpty(hdDateUsed.Value))
            txtTxDate.Text = hdDateUsed.Value;
        btnDelete.Visible = false;
        rbExpense.Checked = true;
        txtFletcherContribution.Text = "50";
        btnUpdate.Text = "Insert";
    }

    private void loadTx() {
        UserTX tx = new UserTX(intTxID);
        txtUserAmount.Text = Utility.formatMoney(tx.UserAmount);
        Utility.setListBoxItems(ref lstAmountType, ((int)tx.FletcherAmountType).ToString());
        txtTxDate.Text = Utility.formatDate(tx.Date);
        hdFletcherAmountCalc.Value = Utility.formatMoney(tx.FletcherContribTotal);
        txtFletcherContribution.Text = Utility.formatMoney(tx.FletcherContribAmount);
        chkIncludeGST.Checked = tx.ShowExGST;
        txtAmount.Text = Utility.formatMoney(tx.TotalAmount);
        txtComment.Text = tx.Comment;
        chkOverrideCodes.Checked = tx.OverrideGLCodes;
        Utility.setListBoxItems(ref lstIncomeAccounts, tx.AccountID.ToString());
        Utility.setListBoxItems(ref lstExpenseAccounts, tx.AccountID.ToString());
        Utility.setListBoxItems(ref lstCategory, tx.CategoryID.ToString());
        if (lstExpenseAccounts.SelectedIndex > 0)
            rbExpense.Checked = true;
        else
            rbIncome.Checked = true;

        //calc the TotalAmount
        ws_Paymaker ows_Paymaker = new ws_Paymaker();
        string szResult = ows_Paymaker.getBudgetAmount(tx.AccountID, tx.UserID, rbExpense.Checked);
        if (szResult.IndexOf("***") > -1) {
            string[] arResult = Utility.SplitByString(szResult, "***");
            if (!String.IsNullOrEmpty(arResult[0]))
                lblBudgetAmount.Text = Utility.formatMoney(Convert.ToDouble(arResult[0]));
        }
        txtTxDate.Text = Utility.formatDate(tx.Date);
        Utility.setListBoxItems(ref lstUserID, tx.UserID.ToString());

        txtGLCredit.Text = tx.CreditGLCode;
        txtGLDebit.Text = tx.DebitGLCode;
        if (tx.CreditJobCode != "") {
            Utility.setListBoxItems(ref txtJobCredit, tx.CreditJobCode);
        }
        if(txtJobCredit.SelectedIndex == 0) {
            txtJobCredit.Items[0].Selected = false;
            //Add the code dynamically
            ListItem li = new ListItem(tx.CreditJobCode, tx.CreditJobCode);
            li.Selected = true;
            txtJobCredit.Items.Add(li);
        }
        if (tx.DebitJobCode != "") {
            Utility.setListBoxItems(ref txtJobDebit, tx.DebitJobCode);
        }
        if (txtJobDebit.SelectedIndex == 0) {
            txtJobDebit.Items[0].Selected = false;

            //Add the code dynamically
            ListItem li = new ListItem(tx.DebitJobCode, tx.DebitJobCode);
            li.Selected = true;
            txtJobDebit.Items.Add(li);
        }

        if (tx.MYOBExportID != -1 && !G.User.hasPermission(RolePermissionType.UpdatePreviousPayPeriod))
            hdReadOnly.Value = "true";
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        string szSQL;
        sqlUpdate oSQL = new sqlUpdate("USERTX", "ID", intTxID);
        oSQL.add("AMOUNT", txtUserAmount.Text);
        oSQL.add("TOTALAMOUNT", txtAmount.Text);
        oSQL.add("FLETCHERAMOUNT", txtFletcherContribution.Text);
        oSQL.add("FLETCHERCONTRIBTOTAL", hdFletcherAmountCalc.Value);
        oSQL.add("AMOUNTTYPEID", lstAmountType.SelectedValue);
        oSQL.add("TXDATE", txtTxDate.Text);
        oSQL.add("CREDITGLCODE", txtGLCredit.Text);
        oSQL.add("DEBITGLCODE", txtGLDebit.Text);
        oSQL.add("CREDITJOBCODE", txtJobCredit.SelectedValue);
        oSQL.add("DEBITJOBCODE", txtJobDebit.SelectedValue);
        oSQL.add("SHOWEXGST", chkIncludeGST);
        oSQL.add("OVERRIDEGLCODES", chkOverrideCodes);
        if (lstCategory.SelectedValue == "")
            oSQL.addNull("OVERRIDEGLCODES");
        else
            oSQL.add("TXCATEGORYID", lstCategory.SelectedValue);

        if (rbExpense.Checked)
            oSQL.add("ACCOUNTID", lstExpenseAccounts.SelectedValue);
        else
            oSQL.add("ACCOUNTID", lstIncomeAccounts.SelectedValue);
        oSQL.add("USERID", lstUserID.SelectedValue);
        oSQL.add("COMMENT", txtComment.Text);

        if (intTxID == -1)
            szSQL = oSQL.createInsertSQL();
        else
            szSQL = oSQL.createUpdateSQL();
        DB.runNonQuery(szSQL);

        //prepare for next entry
        if (intTxID == -1) {
            hdDateUsed.Value = txtTxDate.Text;
            txtUserAmount.Text = txtAmount.Text = txtFletcherContribution.Text = hdFletcherAmountCalc.Value = txtGLCredit.Text = txtGLDebit.Text = txtJobCredit.Text = txtJobDebit.Text = "";
            intTxID = -1;
            bindData();
            loadDefaults();
        } else {
            sbStartJS.Append("parent.closeTx(true);");
        }
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
            UPDATE USERTX
            SET ISDELETED = 1 WHERE ID = {0}", intTxID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.closeTx(true);");
    }
}