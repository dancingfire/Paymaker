using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

public partial class user_account_update : Root {
    protected int intTxID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        if (!IsPostBack) {
            bindData();
        }
    }

    private void bindData() {
        Utility.BindList(ref lstUserID, DB.runReader(@"
                SELECT id, U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME AS NAME
                FROM [DB_USER] U
                WHERE U.ISACTIVE = 1
                ORDER BY U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME"), "ID", "NAME");
        lstUserID.Items.Insert(0, new ListItem("Select a user...", "-1"));
    }

    private void loadUserData() {
        if (Convert.ToInt32(lstUserID.SelectedValue) == -1) {
            return;
        }

        string szSQL = String.Format(@"
            SELECT L.ID, L.NAME, ISNULL(UA.AMOUNT, 0 ) as AMOUNT, ISNULL(USERID, -1) AS USERID, UA.ID AS USERACCOUNTID
            FROM LIST L LEFT JOIN USERACCOUNT UA ON L.ID = UA.ACCOUNTID AND USERID = {2}
            where LISTTYPEID IN({0}, {1})
            ORDER BY LISTTYPEID, NAME;

            SELECT SALESTARGET FROM DB_USER WHERE ID = {2};
            ", (int)ListType.Expense, (int)ListType.Income, lstUserID.SelectedValue);
        using (DataSet ds = DB.runDataSet(szSQL)) {
            StringBuilder sbHTML = new StringBuilder();

            foreach (DataRow dr in ds.Tables[0].Rows) {
                sbHTML.AppendFormat(@"
                    <span class='Label LabelPos'>{0}</span>
                    <input type='text' name='txtAccount{1}' id='txtAccount{1}' value='{2}' class='Entry EntryPos numbersOnly' />
                    <br class='Align'/>
                ", dr["NAME"].ToString(), dr["ID"].ToString(), dr["AMOUNT"].ToString());
            }
            dAccountHTML.InnerHtml = sbHTML.ToString();

            foreach (DataRow dr in ds.Tables[1].Rows)
                txtSalesTarget.Text = Utility.formatMoney(DB.readDouble(dr["SALESTARGET"]));
        }
        pSalesTarget.Visible = true;
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        DB.runNonQuery(String.Format(@"UPDATE DB_USER SET SALESTARGET = {0} WHERE ID = {1}", txtSalesTarget.Text, lstUserID.SelectedValue));

        string szSQL = String.Format(@"
            SELECT L.ID, L.NAME, ISNULL(UA.AMOUNT, 0 ) as AMOUNT, ISNULL(USERID, -1) AS USERID, ISNULL(UA.ID, -1) AS USERACCOUNTID
            FROM LIST L
            LEFT JOIN USERACCOUNT UA ON L.ID = UA.ACCOUNTID AND USERID = {2}
            where LISTTYPEID IN({0}, {1})
            ORDER BY LISTTYPEID, NAME", (int)ListType.Expense, (int)ListType.Income, lstUserID.SelectedValue);
        DataSet ds = DB.runDataSet(szSQL);
        foreach (DataRow oRow in ds.Tables[0].Rows) {
            string Amount = Valid.getText("txtAccount" + oRow["ID"].ToString(), VT.NoValidation);
            sqlUpdate oSQL = new sqlUpdate("USERACCOUNT", "ID", Convert.ToInt32(oRow["USERACCOUNTID"]));
            oSQL.add("USERID", lstUserID.SelectedValue);
            oSQL.add("ACCOUNTID", oRow["ID"].ToString());
            oSQL.add("AMOUNT", Amount);
            if (Convert.ToInt32(oRow["USERACCOUNTID"]) == -1) {
                DB.runNonQuery(oSQL.createInsertSQL());
            } else {
                DB.runNonQuery(oSQL.createUpdateSQL());
            }
        }
        loadUserData();
    }

    private void doClose() {
        Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }

    protected void lstUserID_SelectedIndexChanged(object sender, EventArgs e) {
        loadUserData();
    }
}