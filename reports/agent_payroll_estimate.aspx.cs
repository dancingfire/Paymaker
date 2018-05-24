using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class agent_payroll_estimate : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szPayPeriod = hdPayPeriod.Value = Request.QueryString["szPayPeriod"].ToString();

            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(hdPayPeriod.Value));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }
            hdOfficeID.Value = Valid.getText("szOfficeID", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);
            blnPrint = Valid.getBoolean("blnPrint", false);
            if (hdOfficeID.Value == "null")
                hdOfficeID.Value = "";
            if (hdCompanyID.Value == "null")
                hdCompanyID.Value = "";

            pPageHeader.InnerHtml = pPageHeader.InnerHtml + "<br/>Between " + Utility.formatDate(dtStart) + " - " + Utility.formatDate(dtEnd);
            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szFilter = " ";
            if (!String.IsNullOrWhiteSpace(hdOfficeID.Value))
                szFilter += String.Format(" AND L_OFFICE.ID IN ({0})", hdOfficeID.Value);
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", hdCompanyID.Value);

            string szSQL = String.Format(@"
                    SELECT U.ID, U.FIRSTNAME + ' ' + U.LASTNAME AS AGENT, 0.00 AS AMOUNT, L_OFFICE.NAME AS OFFICENAME FROM DB_USER U
                    JOIN LIST l_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    WHERE U.ID IN ({0}) {1}
                    ORDER BY OFFICENAME, LASTNAME, FIRSTNAME", Valid.getText("szUserID", VT.List), szFilter);

            using (DataSet dsUsers = DB.runDataSet(szSQL)) {
                foreach (DataRow dr in dsUsers.Tables[0].Rows) {
                    string szUserID = dr["ID"].ToString();

                    int intUserID = Convert.ToInt32(szUserID);

                    szSQL = String.Format(@"
                        SELECT SUM(AMOUNT) AS RETAINER, CAST(DATEPART(year, TXDATE) AS VARCHAR) + ' ' + CAST(DATEPART(month, TXDATE) as VARCHAR) AS PERIOD
                        FROM USERTX WHERE ACCOUNTID IN (SELECT ID FROM LIST WHERE NAME = 'Monthly retainer')
                        AND TXDATE BETWEEN '{1}' AND '{2} 23:59' AND ISDELETED = 0 AND USERID = {0}
                        GROUP BY CAST(DATEPART(year, TXDATE) AS VARCHAR) + ' ' + CAST(DATEPART(month, TXDATE) as VARCHAR);

                        SELECT SUM(USS.CALCULATEDAMOUNT) AS AMOUNT, P.ID
                        FROM USERSALESPLIT USS
                        JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                        JOIN SALE S ON SS.SALEID = S.ID
                        JOIN PAYPERIOD P ON P.ID = S.PAYPERIODID
                        WHERE USS.USERID = {0} AND S.STATUSID = 2  AND P.ID IN ({3})
                        GROUP BY P.ID;

                        SELECT ISNULL(SUM(TX.AMOUNT), 0) AS AMOUNT, ISNULL(SUM(TX.FLETCHERCONTRIBTOTAL), 0) AS FLETCHERAMOUNT, LISTTYPEID, L.NAME AS ACCOUNT, ISNULL(MAX(UA.AMOUNT), 0) AS BUDGET
                        FROM USERTX TX JOIN DB_USER USR ON USR.ID = TX.USERID AND USR.ID= {0} AND TX.ISDELETED = 0
                        JOIN LIST L ON TX.ACCOUNTID = L.ID
                        LEFT JOIN USERACCOUNT UA ON UA.USERID = USR.ID AND UA.ACCOUNTID = TX.ACCOUNTID
                        WHERE  TX.TXDATE BETWEEN '{1}' AND '{2} 23:59' AND NAME != 'Monthly retainer'
                        GROUP BY L.LISTTYPEID, L.NAME
                        ORDER BY L.NAME;
                    ", intUserID, Utility.formatDate(dtStart), Utility.formatDate(dtEnd), hdPayPeriod.Value);
                    double dCommissions = 0;
                    double dRetainer = 0;
                    double dTotalPayable = 0;
                    DataSet dsData = DB.runDataSet(szSQL);

                    double dMonthRetainer = 0, dMonthComm = 0, dMonthHeldover = 0, dMonthPayable = 0;

                    //Business rule is that we either get the commission OR the retainer, if one has been paid
                    DataTable dtRetainer = new DataView(dsData.Tables[0], "PERIOD = '" + dtStart.ToString("yyyy M") + "'", "", DataViewRowState.OriginalRows).ToTable();
                    if (dtRetainer.Rows.Count > 0) {
                        dMonthRetainer = Convert.ToDouble(dtRetainer.Rows[0]["RETAINER"]);
                        dRetainer += dMonthRetainer;
                    }
                    int intPrevPayperiodID = Convert.ToInt32(hdPayPeriod.Value) - 1;
                    if (intPrevPayperiodID == 14)
                        intPrevPayperiodID = 13;

                    szSQL = string.Format(@"
                        SELECT HELDOVERAMOUNT
                        FROM USERPAYPERIOD WHERE USERID = {0} AND PAYPERIODID = {1}", intUserID, intPrevPayperiodID);
                    dMonthHeldover = Convert.ToDouble(DB.getScalar(szSQL, "0"));

                    DataTable dtComm = new DataView(dsData.Tables[1], "ID = " + hdPayPeriod.Value, "", DataViewRowState.OriginalRows).ToTable();
                    if (dtComm.Rows.Count > 0) {
                        dMonthComm = Convert.ToDouble(dtComm.Rows[0]["AMOUNT"]);
                        dCommissions += dMonthComm;
                    }

                    dMonthPayable = dMonthComm + dMonthHeldover;
                    if (dMonthPayable < 0)
                        dMonthPayable = 0;
                    dTotalPayable += dMonthPayable;

                    dr["AMOUNT"] = dMonthPayable;
                }

                GridViewHelper helper = new GridViewHelper(gvTable);
                helper.RegisterGroup("OFFICENAME", true, true);
                helper.GroupHeader += new GroupEvent(helper_GroupHeader);
                helper.RegisterSummary("AMOUNT", SummaryOperation.Sum, "OFFICENAME");
                helper.RegisterSummary("AMOUNT", SummaryOperation.Sum);
                helper.GeneralSummary += new FooterEvent(helper_GeneralSummary);
                gvTable.DataSource = dsUsers;
                gvTable.DataBind();
            }
        }

        private void helper_GeneralSummary(GridViewRow row) {
            row.BackColor = Color.FromArgb(230, 213, 172);
            row.Font.Bold = true;
            foreach (TableCell c in row.Cells)
                c.HorizontalAlign = HorizontalAlign.Right;
        }

        private void helper_GroupHeader(string groupName, object[] values, GridViewRow row) {
            row.Height = 30;
            //BackColor = Color.LightGray;
            row.Cells[0].Text = "&nbsp;&nbsp;<strong>" + row.Cells[0].Text + "</strong>";
        }

        private void helper_GroupFooter(string groupName, object[] values, GridViewRow row) {
            row.Height = 30;
            row.Font.Bold = true;
        }

        protected void gvTable_Sorting(object sender, GridViewSortEventArgs e) {
        }
    }
}