using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class payroll_summary : Root {
    int intCycleRef;


    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack) {
            loadData();
            setupPage();
        }
    }

    private void setupPage() {
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Timesheet", "Timesheet", "95%", 620, "payroll_update.aspx", exitX: true)));

        // Only Admin can access these buttons (Export / create PDF)
        btnComplete.Visible = btnExport.Visible = (G.CurrentUserRoleID == 1); // Admin
    }

    private void loadData() {
        if (hdCycleRef.Value == "")
            hdCycleRef.Value = "0";

        intCycleRef = Convert.ToInt32(hdCycleRef.Value);

        btnExport.HRef = string.Format(@"../reports/payroll_summary_rpt.aspx?CycleRef={0}", intCycleRef);

        lstCycle.Items.Clear();
        foreach (int c in G.TimeSheetCycleReferences.dValues.Keys) {
            ListItem oLI = new ListItem(
                                string.Format("{0:d MMM} - {1:d MMM}", G.TimeSheetCycleReferences[c].NormalCycle.StartDate, G.TimeSheetCycleReferences[c].NormalCycle.EndDate)
                                , c.ToString());
            if (c < 0)
                oLI.Attributes.Add("style", "color:red;");
            lstCycle.Items.Add(oLI);
        }

        lstCycle.SelectedValue = hdCycleRef.Value;

        string szSupervisor = string.Format("AND U.SUPERVISORID = {0}", G.User.UserID);
        if (G.CurrentUserRoleID == 1) // Admin
            szSupervisor = "";

        string szSQL = string.Format(@"
                    -- Normal Pay Cycle
                    SELECT MAX(U.ID) AS ID, MAX(TSC.ID) AS TSCID, MIN(U.FIRSTNAME) + ' ' + MIN(U.LASTNAME) AS NAME, MIN(U.FIRSTNAME) AS FIRSTNAME, MIN(U.LASTNAME) AS LASTNAME,
                        LEFT(MAX(O.NAME),2) AS OFFICE, MIN(ENTRYDATE) AS STARTDATE,
	                    SUM(ACTUAL) AS ACTUAL, SUM(ANNUALLEAVE) AS ANNUALLEAVE,
	                    SUM(SICKLEAVE) AS SICKLEAVE, SUM(RDOACRUED) AS RDOACRUED,
	                    SUM(RDOTAKEN) AS RDOTAKEN, '' AS COMMENTS,
                        CASE
							-- User without supervisor signs off on own timesheet
							WHEN MAX(USERSIGNOFFDATE) IS NULL AND (U.SUPERVISORID IS NULL OR U.SUPERVISORID = 0) THEN 'Awaiting Staff signoff'
							-- User with supervisor submits timesheet for supervisor signoff
							WHEN MAX(USERSIGNOFFDATE) IS NULL AND U.SUPERVISORID IS NOT NULL AND U.SUPERVISORID > 0 THEN 'Awaiting Staff submission'
							-- User with supervisor requires signoff after they submit form
							WHEN MAX(SIGNEDOFFDATE) IS NULL AND U.SUPERVISORID IS NOT NULL AND U.SUPERVISORID > 0 THEN 'SignOff'
							-- Ready to go!
							ELSE 'Finalised' END AS DETAILS
                    FROM TIMESHEETENTRY TS
                    JOIN DB_USER U ON U.ID = TS.USERID
                    JOIN LIST O ON O.ID = U.OFFICEID
                    JOIN TIMESHEETCYCLE TSC ON TSC.ID = TS.TIMESHEETCYCLEID
                    WHERE TSC.ID = {1}
                    {0}
                    GROUP BY TIMESHEETCYCLEID, USERID, SUPERVISORID
                    UNION
                    SELECT U.ID, -1 AS TSCID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME, U.FIRSTNAME, U.LASTNAME,
                        LEFT(O.NAME,2) AS OFFICE, NULL AS STARTDATE,
	                    NULL AS ACTUAL, NULL AS ANNUALLEAVE,
	                    NULL AS SICKLEAVE, NULL AS RDOACRUED,
	                    NULL AS RDOTAKEN, NULL AS COMMENTS,
                        'User hasn''t entered details' AS DETAILS
                    FROM DB_USER U
                    JOIN LIST O ON O.ID = U.OFFICEID
                    WHERE U.PAYROLLCYCLEID = 1 {0}
                        AND U.ID NOT IN (SELECT DISTINCT USERID FROM TIMESHEETENTRY WHERE TIMESHEETCYCLEID = {1})
                    ORDER BY 2;

                    -- Pay in advance pay cycle
                    SELECT MAX(U.ID) AS ID, MAX(TSC.ID) AS TSCID, MIN(U.FIRSTNAME) + ' ' + MIN(U.LASTNAME) AS NAME, MIN(U.FIRSTNAME) AS FIRSTNAME, MIN(U.LASTNAME) AS LASTNAME,
                        LEFT(MAX(O.NAME),2) AS OFFICE, MIN(ENTRYDATE) AS STARTDATE,
	                    SUM(ACTUAL) AS ACTUAL, SUM(ANNUALLEAVE) AS ANNUALLEAVE,
	                    SUM(SICKLEAVE) AS SICKLEAVE, SUM(RDOACRUED) AS RDOACRUED,
	                    SUM(RDOTAKEN) AS RDOTAKEN, '' AS COMMENTS,
                        CASE
							-- User without supervisor signs off on own timesheet
							WHEN MAX(USERSIGNOFFDATE) IS NULL AND (U.SUPERVISORID IS NULL OR U.SUPERVISORID = 0) THEN 'Awaiting Staff signoff'
							-- User with supervisor submits timesheet for supervisor signoff
							WHEN MAX(USERSIGNOFFDATE) IS NULL AND U.SUPERVISORID IS NOT NULL AND U.SUPERVISORID > 0 THEN 'Awaiting Staff submission'
							-- User with supervisor requires signoff after they submit form
							WHEN MAX(SIGNEDOFFDATE) IS NULL AND U.SUPERVISORID IS NOT NULL AND U.SUPERVISORID > 0 THEN 'SignOff'
							-- Ready to go!
							ELSE 'Finalised' END AS DETAILS
                    FROM TIMESHEETENTRY TS
                    JOIN DB_USER U ON U.ID = TS.USERID
                    JOIN LIST O ON O.ID = U.OFFICEID
                    JOIN TIMESHEETCYCLE TSC ON TSC.ID = TS.TIMESHEETCYCLEID
                    WHERE TSC.ID = {2}
                    {0}
                    GROUP BY TIMESHEETCYCLEID, USERID, SUPERVISORID
                    UNION
                    SELECT U.ID, -1 AS TSCID, U.FIRSTNAME + ' ' + U.LASTNAME AS NAME, U.FIRSTNAME, U.LASTNAME,
                        LEFT(O.NAME,2) AS OFFICE, NULL AS STARTDATE,
	                    NULL AS ACTUAL, NULL AS ANNUALLEAVE,
	                    NULL AS SICKLEAVE, NULL AS RDOACRUED,
	                    NULL AS RDOTAKEN, NULL AS COMMENTS,
                        'User hasn''t entered details' AS DETAILS
                    FROM DB_USER U
                    JOIN LIST O ON O.ID = U.OFFICEID
                    WHERE U.PAYROLLCYCLEID = 2 {0}
                        AND U.ID NOT IN (SELECT DISTINCT USERID FROM TIMESHEETENTRY WHERE TIMESHEETCYCLEID = {2})
                    ORDER BY 2;
", 
                        szSupervisor, 
                        G.TimeSheetCycleReferences[intCycleRef].NormalCycle.CycleID,
                        G.TimeSheetCycleReferences[intCycleRef].PayAdvanceCycle.CycleID);

        DataSet ds = DB.runDataSet(szSQL);

        gvList.DataSource = ds.Tables[0];
        gvList.DataBind();

        HTML.formatGridView(ref gvList, true);

        gvListPrepay.DataSource = ds.Tables[1];
        gvListPrepay.DataBind();

        HTML.formatGridView(ref gvListPrepay, true);

        // Only the current Timesheet payroll cycle can be completed
        btnComplete.Disabled = intCycleRef != 0;
        if(intCycleRef == -1) {
            // We want to be able to undo the last cycle completion if required
            btnUnlock.Visible = true;
        } else {
            btnUnlock.Visible = false;
        }
    }

    protected void btnChange_Click(object sender, System.EventArgs e) {
        loadData();
        setupPage();
    }

    protected void btnUnlock_Click(object sender, System.EventArgs e) {
        DB.runNonQuery(String.Format(@"
            UPDATE TIMESHEETCYCLE SET COMPLETED = 0
            WHERE ID IN ({0}, {1})", G.TimeSheetCycleReferences[-1].NormalCycle.CycleID,
            G.TimeSheetCycleReferences[-1].PayAdvanceCycle.CycleID));

        G.TimeSheetCycleReferences = null;
        TimesheetCycle.checkTimesheetCycles();
        G.TimeSheetCycleReferences = Payroll.getTimeSheetCycleReferences();
        hdCycleRef.Value = "0";
        loadData();
        setupPage();
    }

    
    protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            string szTSCID = ((DataRowView)e.Row.DataItem)["TSCID"].ToString();

            e.Row.Attributes["data-id"] = szID;
            e.Row.CssClass += " trEdit";
            if (e.Row.Cells[8].Text == "SignOff") {
                // Only supervisor can sign off on staff timesheet
                if (G.UserInfo.getUser(Convert.ToInt32(szID)).SupervisorID == G.CurrentUserID)
                    e.Row.Cells[8].Text = String.Format(@"<input name='btnSubmit{0}' class='ApproveButton' value='Approve' id='btnSubmit{0}' type='submit' data-id='{0}' data-tsid='{1}'>", szID, szTSCID);
                else
                    e.Row.Cells[8].Text = "Awaiting Supervisor signoff";
            }
        }
    }
}