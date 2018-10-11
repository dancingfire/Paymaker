using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for client_update.
/// </summary>
public partial class user_update : Root {
    protected int intUserID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        intUserID = Valid.getInteger("intUserID");
        HDUserID.Value = intUserID.ToString();
        checkDelete();
        initPage();
        if (!IsPostBack) {
            bindData();
            if (intUserID != -1)
                loadUser();
        }
        Form.Controls.Add(HTML.createLabel(HTML.createModalIFrameHTML("Permission", "Set permissions", "600px", 400)));
    }

    private void initPage() {
        Form.Controls.Add(new LiteralControl(HTML.createModalIFrameHTML("Salary", "Salary/team history", "600px", 400)));
    }

    private void bindData() {
        string szSQL = String.Format("Select ID, Name from LIST where LISTTYPEID = {0} ORDER BY NAME", (int)ListType.Office);
        Utility.BindList(ref lstOffice, DB.runReader(szSQL), "ID", "NAME");
        Utility.BindList(ref lstRole, DB.runReader("SELECT id, NAME  FROM [ROLE] ORDER BY ID"), "ID", "NAME");

        Utility.BindList(ref lstAgentEOMReportSettings, DB.Paymaker_User.loadList("", false), "ID", "NAME");
        lstAgentEOMReportSettings.Items.Insert(0, new ListItem("Hide on report", "-1"));

        Utility.BindList(ref lstSupervisor, DB.Paymaker_User.loadList("", false), "ID", "NAME");
        lstSupervisor.Items.Insert(0, new ListItem("Select a supervisor...", ""));

        Utility.BindList(ref lstTopPerformer, DB.Paymaker_User.loadList("", false), "ID", "NAME");
        lstTopPerformer.Items.Insert(0, new ListItem("Hide on report", "-1"));

        Utility.BindList(ref lstAdminPA, DB.Paymaker_User.loadList("3,4", false), "ID", "NAME");
        lstAdminPA.Items.Insert(0, new ListItem("No one", "-1"));

        Utility.BindList(ref lstQuarterlyTopPerformer, DB.Paymaker_User.loadList("", false), "ID", "NAME");
        lstQuarterlyTopPerformer.Items.Insert(0, new ListItem("Hide on report", "-1"));

        txtEffectiveDate.Text = Utility.formatDate(DateTime.Now);
    }

    private void checkDelete() {
        if (intUserID == -1)
            btnDelete.Visible = false;
    }

    private void loadUser() {
        string szSQL = string.Format(@"
                SELECT U.* , T.FIRSTNAME + ' ' + T.LASTNAME AS TEAM
                FROM DB_USER U LEFT JOIN DB_USER T ON T.ID = U.TEAMID
                WHERE U.ID = {0}
                ", intUserID);
        DataSet dsUser = DB.runDataSet(szSQL);
        DataRow dr = dsUser.Tables[0].Rows[0];

        txtFirstName.Text = dr["FirstName"].ToString();
        txtLastName.Text = dr["LastName"].ToString();
        lblSalary.Text = DB.readString(dr["SALARY"]);
        lblTeam.Text = DB.readString(dr["TEAM"]);

        txtInitials.Text = dr["INITIALSCODE"].ToString();
        txtInitials.MaxLength = 3;

        int intIsActive = Convert.ToInt32(dr["ISACTIVE"]);
        if (intIsActive == 0) {
            txtInitials.Enabled = false;
            txtInitials.ToolTip = "Initials can not be edited on an inactive record";
        }

        txtLogin.Text = dr["Login"].ToString();
        txtEmail.Text = dr["Email"].ToString();
        txtCreditGLCode.Text = dr["CreditGLCode"].ToString();
        txtDebitGLCode.Text = dr["DebitGLCode"].ToString();
        txtSalesTarget.Text = dr["SALESTARGET"].ToString();
        lstRole.Items.FindByValue(dr["RoleID"].ToString()).Selected = true;

        if (dr["SUPERVISORID"] != DBNull.Value) {
            string szSupervisorID = dr["SUPERVISORID"].ToString();

            Utility.setListBoxItems(ref lstSupervisor, szSupervisorID);
        }

        Utility.setListBoxItems(ref lstStatus, intIsActive.ToString());
        Utility.setListBoxItems(ref lstOffice, dr["OFFICEID"].ToString());
        Utility.setListBoxItems(ref lstPayrollCycle, dr["PAYROLLCYCLEID"].ToString());
        HDOrigOfficeID.Value = dr["OFFICEID"].ToString();
        Utility.setListBoxItems(ref lstAgentEOMReportSettings, dr["AGENTEOMBALANCEREPORTSETTINGS"].ToString());
        Utility.setListBoxItems(ref lstQuarterlyTopPerformer, dr["QUARTERLYTOPPERFORMERREPORTSETTINGS"].ToString());
        Utility.setListBoxItems(ref lstTopPerformer, dr["TOPPERFORMERREPORTSETTINGS"].ToString());
        Utility.setListBoxItems(ref lstAdminPA, dr["ADMINPAFORUSERID"].ToString());
        chkIsPaid.Checked = Convert.ToBoolean(dr["ISPAID"]);
        chkShowIncentiveSummary.Checked = Convert.ToBoolean(dr["INCENTIVESUMMARYREPORTSETTINGS"]);
        //        Utility.setListBoxItems(ref lstTimesheetType, Convert.ToString(dr["TIMESHEETTYPEID"]));

        szSQL = String.Format(@"
            SELECT FIRSTNAME + ' ' + LASTNAME AS NAME
            FROM DB_USER WHERE TEAMID = {0} AND ISACTIVE = 1", intUserID);
        string szTeamUsers = "";
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr1 in ds.Tables[0].Rows)
                Utility.Append(ref szTeamUsers, Convert.ToString(dr1["NAME"]), "<br/>");
        }
        if (!String.IsNullOrEmpty(szTeamUsers)) {
            lblTeamMembers.Text = szTeamUsers;
            pTeamMembers.Visible = true;
        }

        btnPermissions.Visible = false;

        if (G.User.hasPermission(RolePermissionType.ViewPermissionButton)) {
            btnPermissions.Visible = true;
        }
    }

    private string __szInactiveTag = "[inactive]";
    private string __szDeletedTag = "[deleted]";

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        string szSQL;
        sqlUpdate oSQL = new sqlUpdate("DB_USER", "ID", intUserID);
        oSQL.add("FIRSTNAME", txtFirstName.Text);
        oSQL.add("LASTNAME", txtLastName.Text);
        oSQL.add("LOGIN", txtLogin.Text);
        oSQL.add("EMAIL", txtEmail.Text);
        oSQL.add("ROLEID", lstRole.SelectedValue);
        oSQL.add("OFFICEID", lstOffice.SelectedValue);
        oSQL.add("PAYROLLCYCLEID", lstPayrollCycle.SelectedValue);
        if (lstRole.SelectedValue == "5") {
            oSQL.add("ISPAID", "0");
        } else {
            oSQL.add("ISPAID", chkIsPaid);
        }
        oSQL.add("CREDITGLCODE", txtCreditGLCode.Text);
        oSQL.add("DEBITGLCODE", txtDebitGLCode.Text);

        string szInitialsCode = txtInitials.Text.Replace(__szInactiveTag, "");
        string szListStatus = lstStatus.SelectedValue;

        // inactive records have initials tagged '[inactive]INITIALS' to avoid confusion with current
        // users that have same initials
        if (szListStatus == "0")
            szInitialsCode = __szInactiveTag + szInitialsCode;

        oSQL.add("INITIALSCODE", szInitialsCode);
        oSQL.add("SALESTARGET", txtSalesTarget.Text);
        oSQL.add("ISACTIVE", szListStatus);
        oSQL.add("SUPERVISORID", lstSupervisor.SelectedValue);
        if (lstAgentEOMReportSettings.SelectedValue == "")
            oSQL.addNull("AGENTEOMBALANCEREPORTSETTINGS");
        else
            oSQL.add("AGENTEOMBALANCEREPORTSETTINGS", lstAgentEOMReportSettings.SelectedValue);

        if (lstQuarterlyTopPerformer.SelectedValue == "")
            oSQL.addNull("QUARTERLYTOPPERFORMERREPORTSETTINGS");
        else
            oSQL.add("QUARTERLYTOPPERFORMERREPORTSETTINGS", lstQuarterlyTopPerformer.SelectedValue);

        if (lstTopPerformer.SelectedValue == "")
            oSQL.addNull("TOPPERFORMERREPORTSETTINGS");
        else
            oSQL.add("TOPPERFORMERREPORTSETTINGS", lstTopPerformer.SelectedValue);
        if (lstAdminPA.SelectedValue == "")
            oSQL.addNull("ADMINPAFORUSERID");
        else
            oSQL.add("ADMINPAFORUSERID", lstAdminPA.SelectedValue);

        oSQL.add("INCENTIVESUMMARYREPORTSETTINGS", chkShowIncentiveSummary);

        if (intUserID == -1)
            szSQL = oSQL.createInsertSQL();
        else
            szSQL = oSQL.createUpdateSQL();
        DB.runNonQuery(szSQL);

        if (intUserID == -1) {
            intUserID = DB.getScalar("select max(id) from DB_USER", 0);
            szSQL = String.Format(@"
                UPDATE DB_USER
                SET AGENTEOMBALANCEREPORTSETTINGS = {0} WHERE ID = {0} AND AGENTEOMBALANCEREPORTSETTINGS IS NULL", intUserID);
            DB.runNonQuery(szSQL);
        }

        if (txtPassword.Text != "") {
            szSQL = String.Format(@"
                    UPDATE DB_USER
                    SET PASSWORD = convert(varbinary(255), PWDENCRYPT('{0}'))
                    WHERE ID = {1}"
                , txtPassword.Text, intUserID);
            DB.runNonQuery(szSQL);
        }

        if (HDOrigOfficeID.Value != "" && HDOrigOfficeID.Value != lstOffice.SelectedValue) {
            szSQL = String.Format(@"
                UPDATE USERSALESPLIT SET OFFICEID = {2}
                WHERE ID IN (SELECT USS.ID FROM SALE S JOIN SALESPLIT SS ON S.ID = SS.SALEID JOIN USERSALESPLIT USS ON SS.ID = USS.SALESPLITID WHERE S.SALEDATE >= '{0}' AND USS.USERID = {1})
            ", txtEffectiveDate.Text, intUserID, lstOffice.SelectedValue);
            DB.runNonQuery(szSQL);
        }
        G.UserInfo.forceReload();
        sbStartJS.Append("parent.refreshPage();");
    }

    private void doClose() {
        Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }

    protected void btnDelete_Click(object sender, System.EventArgs e) {
        string szSQL = string.Format(@"
                UPDATE DB_USER
                SET
					-- Initials Code is saved with [deleted] appended to it to ensure the record
                    -- is not confused with any current records of the same
                    INITIALSCODE = '[deleted]'+REPLACE(REPLACE(INITIALSCODE,'{1}',''),'{2}',''),
					ISDELETED = 1 WHERE ID = {0}", intUserID, __szDeletedTag, __szInactiveTag);
        DB.runNonQuery(szSQL);
        sbStartJS.Append("parent.refreshPage();");
    }
}