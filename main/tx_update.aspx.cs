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
        if (!Page.IsPostBack) {
            bindData();
            if (intTxID == -1)
                loadDefaults();
            else
                loadTx();
        }
    }
    
    private void bindData() {
        string szSQL = String.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID IN(2, 14) ORDER BY SEQUENCENO, NAME", ListType.Expense, ListType.AgentOffTheTop);
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
            WHERE U.ISACTIVE = 1 AND U.ISDELETED = 0
            ORDER BY U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME"), "ID", "NAME");
        lstUserID.Items.Insert(0, new ListItem("Select a user...", "-1"));
        using (DataSet ds = DB.MYOBAccount.getSubAccountList()) {
            txtJobCredit.ClearSelection();
            txtJobCredit.SelectedValue = null;

            Utility.BindList(ref txtJobCredit, ds, "NAME", "NAME");
            txtJobCredit.Items.Insert(0, new ListItem(""));
            txtJobDebit.SelectedValue = null;
            txtJobDebit.ClearSelection();
            Utility.BindList(ref txtJobDebit, ds, "NAME", "NAME");
            txtJobDebit.Items.Insert(0, new ListItem(""));
        }

        rbIncome.Attributes["onclick"] = "showAccountList(true);";
        rbExpense.Attributes["onclick"] = "showAccountList(true);";
        chkIncludeGST.Attributes["onclick"] = "getExGSTAmount()";

        txtGLCredit.Attributes["onchange"] = "lockAccounts();";
        txtGLDebit.Attributes["onchange"] = "lockAccounts();";      
    }

    private void loadDefaults() {
        txtTxDate.Text = Utility.formatDate(DateTime.Now);
        if (!string.IsNullOrEmpty(hdDateUsed.Value))
            txtTxDate.Text = hdDateUsed.Value;
        btnDelete.Visible = false;
        rbExpense.Checked = true;
        txtFletcherContribution.Text = "50";
        btnUpdate.Text = "Insert";
        pReversal.Visible = false;
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

        chkIsTemplate.Checked = tx.IsTemplate;
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
            ListItem li = txtJobCredit.Items.FindByValue(tx.CreditJobCode);
            txtJobCredit.ClearSelection();
            if (li == null) {
                //Add the code dynamically
                ListItem liNew = new ListItem(tx.CreditJobCode, tx.CreditJobCode);
                liNew.Selected = true;
                txtJobCredit.Items.Add(liNew);
            } else {
                li.Selected = true;
            }
        }
        
        if (txtJobDebit.SelectedIndex == 0) {
            txtJobDebit.ClearSelection();

            ListItem li = txtJobDebit.Items.FindByValue(tx.DebitJobCode);
            if (li == null) { 
                //Add the code dynamically
                ListItem liNew = new ListItem(tx.DebitJobCode, tx.DebitJobCode);
                liNew.Selected = true;
                txtJobDebit.Items.Add(liNew);
            } else {
                li.Selected = true;
            }
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
        oSQL.add("CREDITJOBCODE", Valid.getText("txtJobCredit", ""));
        oSQL.add("DEBITJOBCODE", Valid.getText("txtJobDebit", ""));
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
        
        oSQL.add("ISTEMPLATE", chkIsTemplate);
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

    protected void btnReverse_Click(object sender, EventArgs e) {
        DB.runNonQuery(String.Format(@"
            INSERT INTO USERTX(ACCOUNTID, USERID, AMOUNT, FLETCHERAMOUNT, TXDATE, COMMENT, AMOUNTTYPEID, ISDELETED, FLETCHERCONTRIBTOTAL, SHOWEXGST, TOTALAMOUNT, CREDITGLCODE, DEBITGLCODE, CREDITJOBCODE, DEBITJOBCODE, OVERRIDEGLCODES, TXCATEGORYID)
            SELECT ACCOUNTID, USERID, -1 * AMOUNT, -1* FLETCHERAMOUNT, '{0}', 'CREDIT - ' + COMMENT, AMOUNTTYPEID, ISDELETED, -1 * FLETCHERCONTRIBTOTAL, SHOWEXGST, -1* TOTALAMOUNT, DEBITGLCODE, CREDITGLCODE, DEBITJOBCODE, CREDITJOBCODE, OVERRIDEGLCODES, TXCATEGORYID
            FROM USERTX where ID = {1};", txtReversalDate.Text, intTxID));
        sbStartJS.Append("parent.closeTx(true);");

    }

}