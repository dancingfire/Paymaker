using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class salary_update : Root {
    private int intUserID = -1;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        intUserID = Valid.getInteger("intUserID");
        if (!IsPostBack) {
            initPage();
            loadHistory();
        }
    }

    private void initPage() {
        Utility.BindList(ref lstTeam, DB.runDataSet(string.Format("select ID, INITIALSCODE + ' - ' + FIRSTNAME + ' ' + LASTNAME AS NAME FROM DB_USER ORDER BY INITIALSCODE, FIRSTNAME, LASTNAME")), "ID", "NAME");
        lstTeam.Items.Insert(0, new ListItem("Select a team...", ""));
    }

    /// <summary>
    /// Reads Salary, ensuring a integer value is read.  All float values will drop the decimal values
    /// If unable to convert, value of zero is returned
    /// </summary>
    private int Salary {
        get {
            int intSalary = 0;
            if (!int.TryParse(txtSalary.Text, out intSalary)) {
                double dSalary = 0.0;
                if (double.TryParse(txtSalary.Text, out dSalary)) {
                    intSalary = Convert.ToInt32(dSalary);
                }
            }
            return intSalary;
        }
    }

    private void loadUpdateObject(ref sqlUpdate oSQL) {
        oSQL.add("STARTDATE", txtStartDate.Text);
        oSQL.add("USERID", intUserID);

        if (txtEndDate.Text == "")
            oSQL.addNull("ENDDATE");
        else
            oSQL.add("ENDDATE", txtEndDate.Text);
        oSQL.add("SALARY", Salary);
        if (lstTeam.SelectedIndex > 0)
            oSQL.add("TEAMID", lstTeam.SelectedValue);
        else
            oSQL.addNull("TEAMID");
    }

    private void updateUserRecord() {
        sqlUpdate oSQL = new sqlUpdate("DB_USER", "ID", intUserID);
        oSQL.add("SALARY", Salary);
        if (lstTeam.SelectedIndex > 0)
            oSQL.add("TEAMID", lstTeam.SelectedValue);
        else
            oSQL.addNull("TEAMID");
        G.UserInfo.forceReload();
        DB.runNonQuery(oSQL.createUpdateSQL());
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        if (hdHistoryID.Value == "")
            hdHistoryID.Value = "-1";

        sqlUpdate oSQL = new sqlUpdate("USERHISTORY", "ID", Convert.ToInt32(hdHistoryID.Value));
        loadUpdateObject(ref oSQL);
        if (hdHistoryID.Value == "-1")
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
        updateUserRecord();
        closePage();
    }

    private void closePage() {
        sbStartJS.AppendFormat("parent.closeSalary(true, '{0}', '{1}')", Salary, lstTeam.SelectedIndex == 0 ? "" : lstTeam.Items[lstTeam.SelectedIndex].Text);
    }

    protected void btnCreateNew_Click(object sender, EventArgs e) {
        sqlUpdate oSQL = new sqlUpdate("USERHISTORY", "ID", Convert.ToInt32(hdHistoryID.Value));
        loadUpdateObject(ref oSQL);
        DB.runNonQuery(oSQL.createInsertSQL());
        closePage();
    }

    private void loadHistory() {
        string szSQL = String.Format(@"
            SELECT U.ID, U.STARTDATE, U.ENDDATE, U.SALARY, U.TEAMID, T.FIRSTNAME + ' ' + T.LASTNAME  AS TEAM
            FROM USERHISTORY U LEFT JOIN DB_USER T ON T.ID = U.TEAMID
            WHERE U.USERID = {0}
            ORDER BY STARTDATE DESC", intUserID);

        gvHistory.DataSource = DB.runReader(szSQL);
        gvHistory.DataBind();
        HTML.formatGridView(ref gvHistory, true);

        using (DataSet ds = DB.runDataSet(szSQL)) {
            if (ds.Tables[0].Rows.Count > 0) {
                DataRow dr = ds.Tables[0].Rows[0];
                txtStartDate.Text = DB.readDateString(dr["STARTDATE"]);
                txtEndDate.Text = DB.readDateString(dr["ENDDATE"]);
                Utility.setListBoxItems(ref lstTeam, DB.readString(dr["TEAMID"]));
                txtSalary.Text = DB.readString(dr["SALARY"]);
                hdHistoryID.Value = DB.readString(dr["ID"]);
                btnCreateNew.Visible = true;
            }
        }
    }
}