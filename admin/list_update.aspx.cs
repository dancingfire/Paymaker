using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class list_update : Root {
    protected int intItemID = -1;
    private ListType oListType = ListType.Office;
    private bool blnIsPopup = false;

    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        blnShowMenu = false;

        intItemID = Valid.getInteger("intItemID");
        hdItemID.Value = intItemID.ToString();
        hdListTypeID.Value = Valid.getInteger("intListTypeID").ToString();
        oListType = (ListType)Valid.getInteger("intListTypeID");
        hdItemID.Value = intItemID.ToString();
        btnDelete.Enabled = false;
        if (!IsPostBack) {
            initPage();
            if (intItemID != -1)
                loadItem();
        }
    }

    private void initPage() {
        if (oListType == ListType.Office) {
            pCompany.Visible = true;
            Utility.BindList(ref lstCompany, DB.runDataSet(String.Format("Select ID, NAME from LIST where LISTTYPEID = {0}", (int)ListType.Company)), "ID", "NAME");
            lstCompany.Items.Insert(0, new ListItem("Select a company...", "-1"));
            txtDescription.TextMode = TextBoxMode.SingleLine;
            lblDescription.Text = "Report name";
        } else if (oListType == ListType.Company) {
            txtDescription.TextMode = TextBoxMode.SingleLine;
            lblDescription.Text = "Report name";
        }

        pDefaultAmount.Visible = oListType == ListType.Commission || oListType == ListType.OffTheTop;
        pJobCode.Visible = oListType == ListType.Office || oListType == ListType.Expense || oListType == ListType.Income;
        pCreditGLCode.Visible = oListType == ListType.Expense || oListType == ListType.CampaignGL;
        pDebitGLCode.Visible = oListType == ListType.Income;
        pOfficeCode.Visible =  oListType == ListType.Company;
        if (oListType == ListType.CampaignGL)
            lblDescription.Text = "MYOB text";
    }

    private void loadItem() {
        string szSQL = String.Format("Select * from LIST where ID = {0}", intItemID);
        DataSet dsItem = DB.runDataSet(szSQL);
        DataRow dr = dsItem.Tables[0].Rows[0];

        txtName.Text = dr["Name"].ToString();
        txtDescription.Text = dr["Description"].ToString();
        txtAmount.Text = dr["Amount"].ToString();
        txtSortOrder.Text = dr["SEQUENCENO"].ToString();
        txtCreditGLCode.Text = dr["CREDITGLCode"].ToString();
        txtDebitGLCode.Text = dr["DEBITGLCode"].ToString();
        txtOfficeCode.Text = dr["OFFICEMYOBCODE"].ToString();
        txtMYOBBranch.Text = dr["OFFICEMYOBBRANCH"].ToString();
        Utility.setListBoxItems(ref lstAmountValue, dr["AMOUNTTYPEID"].ToString());
        Utility.setListBoxItems(ref lstStatus, Convert.ToInt32(dr["ISACTIVE"]).ToString());
        if (pCompany.Visible)
            Utility.setListBoxItems(ref lstCompany, dr["COMPANYID"].ToString());
        if (pJobCode.Visible) {
            txtJobCode.Text = dr["JOBCODE"].ToString();
            txtAdvertisingJobCode.Text = dr["ADVERTISINGJOBCODE"].ToString();
        }
        if (pDebitGLCode.Visible)
            txtDebitGLCode.Text = dr["DEBITGLCODE"].ToString();
        if (pCreditGLCode.Visible)
            txtCreditGLCode.Text = dr["CREDITGLCODE"].ToString();
        checkDelete();
    }

    private void checkDelete() {
        int intCount = 0;
        string szSQL = "";
        switch (oListType) {
            case ListType.Expense:
                szSQL = String.Format(@"
                    SELECT (SELECT COUNT(*) FROM USERTX WHERE ACCOUNTID = {0}) +
                            (SELECT COUNT(*) FROM USERACCOUNT WHERE ACCOUNTID = {0})", intItemID);
                break;

            case ListType.Income:
                szSQL = String.Format(@"
                    SELECT (SELECT COUNT(*) FROM USERTX WHERE ACCOUNTID = {0}) +
                            (SELECT COUNT(*) FROM USERACCOUNT WHERE ACCOUNTID = {0})", intItemID);
                break;

            case ListType.Commission:
                szSQL = String.Format(@"SELECT COUNT(*) FROM SALESPLIT WHERE COMMISSIONTYPEID = {0}", intItemID);
                break;

            case ListType.Company:
                szSQL = String.Format(@"SELECT COUNT(*) FROM LIST WHERE COMPANYID = {0}", intItemID);
                break;

            case ListType.Office:
                szSQL = String.Format(@"SELECT COUNT(*) FROM DB_USER WHERE OFFICEID = {0}", intItemID);
                break;

            case ListType.OffTheTop:
                szSQL = String.Format(@"SELECT COUNT(*) FROM SALEEXPENSE WHERE EXPENSETYPEID = {0}", intItemID);
                break;

            case ListType.CampaignGL:
                szSQL = String.Format(@"SELECT COUNT(*) FROM PRODUCT WHERE CREDITGLCODE = {0} ", intItemID);
                break;

            case ListType.TXCategory:
                szSQL = String.Format(@"SELECT COUNT(*) FROM USERTX WHERE TXCATEGORYID = {0} ", intItemID);
                break;

            case ListType.LeaveType:
                szSQL = String.Format(@"SELECT COUNT(*) FROM LEAVEREQUEST WHERE LEAVETYPEID = {0} ", intItemID);
                break;
        }
        intCount = DB.getScalar(szSQL, 0);

        btnDelete.Enabled = intCount == 0;
        if (intCount > 0) {
            btnDelete.ToolTip = "This item is being used in the system and cannot be deleted.";
        }
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        string szSQL;
        sqlUpdate oSQL = new sqlUpdate("LIST", "ID", intItemID);
        oSQL.add("NAME", txtName.Text);
        oSQL.add("DESCRIPTION", txtDescription.Text);
        if (Utility.IsNumeric(txtSortOrder.Text))
            oSQL.add("SEQUENCENO", txtSortOrder.Text);
        oSQL.add("LISTTYPEID", (int)oListType);
        oSQL.add("ISACTIVE", lstStatus.SelectedValue);
        if (pDefaultAmount.Visible) {
            oSQL.add("AMOUNT", txtAmount.Text);
            oSQL.add("AMOUNTTYPEID", lstAmountValue.SelectedValue);
        }
        if (pCreditGLCode.Visible)
            oSQL.add("CREDITGLCODE", txtCreditGLCode.Text);
        if (pOfficeCode.Visible) {
            oSQL.add("OFFICEMYOBCODE", txtOfficeCode.Text);
            oSQL.add("OFFICEMYOBBRANCH", txtMYOBBranch.Text);

        }
        if (pDebitGLCode.Visible)
            oSQL.add("DEBITGLCODE", txtDebitGLCode.Text);
        if (pCompany.Visible)
            oSQL.add("COMPANYID", lstCompany.SelectedValue);
        if (pJobCode.Visible) {
            oSQL.add("JOBCODE", txtJobCode.Text);
            oSQL.add("ADVERTISINGJOBCODE", txtAdvertisingJobCode.Text);
        }
        if (intItemID == -1)
            szSQL = oSQL.createInsertSQL();
        else
            szSQL = oSQL.createUpdateSQL();
        int intID = Convert.ToInt32(DB.getScalar(szSQL));
        if (oListType == ListType.Commission)
            G.CommTypeInfo.forceReload();

        if (Valid.getBoolean("IsPopup", false))
            sbStartJS.AppendFormat(" parent.closeList({0},'{1}'); ", intID, txtName.Text);
        else
            sbStartJS.Append("parent.refreshPage();");
    }

    private void doClose() {
        if (Valid.getBoolean("IsPopup", false))
            sbStartJS.Append(" parent.closeList(); ");
        else
            Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
                DELETE FROM LIST
                WHERE ID = {0}", intItemID);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.refreshPage();");
    }
}