using System;
using System.Data;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class agent_eom_balance_and_rollover : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szPayPeriod = hdPayPeriod.Value = Request.QueryString["szPayPeriod"].ToString();
            hdOfficeID.Value = Valid.getText("szOfficeID", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);
            blnPrint = Valid.getBoolean("blnPrint", false);
            if (hdOfficeID.Value == "null")
                hdOfficeID.Value = "";
            if (hdCompanyID.Value == "null")
                hdCompanyID.Value = "";
            PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(hdPayPeriod.Value));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }

            pPageHeader.InnerHtml = pPageHeader.InnerHtml + "<br/>Between " + Utility.formatDate(dtStart) + " - " + Utility.formatDate(dtEnd);
            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szFilter = " 1 = 1 ";
            if (!String.IsNullOrWhiteSpace(hdOfficeID.Value))
                szFilter += String.Format(" AND L_OFFICE.ID IN ({0})", hdOfficeID.Value);
            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", hdCompanyID.Value);

            string szSQL = string.Format(@"
                -- 0) 
                SELECT SUM(USS.ACTUALPAYMENT) AS PENDING, 0 AS RETAINER, '' AS OFFICENAME, '' AS AGENT, 0 AS DEBITBALANCE, USR.AGENTEOMBALANCEREPORTSETTINGS AS USERID
                FROM  DB_USER USR
                JOIN USERSALESPLIT USS ON USS.USERID = USR.ID  AND USS.RECORDSTATUS < 1
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID  AND SS.CALCULATEDAMOUNT > 0
                JOIN SALE S ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND S.PAYPERIODID IS NULL
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID {4} AND L_OFFICE.ISACTIVE = 1
                WHERE {0} AND USR.AGENTEOMBALANCEREPORTSETTINGS > -1 AND USR.ID > 0 AND USR.ISACTIVE = 1
                GROUP BY USR.AGENTEOMBALANCEREPORTSETTINGS

                -- 1) Retainer
                SELECT SUM(AMOUNT) AS RETAINER, USERID
                FROM USERTX WHERE ACCOUNTID IN (SELECT ID FROM LIST WHERE NAME = 'Monthly retainer')
                AND TXDATE BETWEEN '{1}' AND '{2}' AND ISDELETED = 0
                GROUP BY USERID;

                -- 2) Heldover
                SELECT HELDOVERAMOUNT AS HELDOVER, USERID
                FROM USERPAYPERIOD
                WHERE PAYPERIODID = {3}
                ORDER BY USERID;

                -- 3) User Details
                SELECT USR.ID AS USERID, USR.FIRSTNAME + ' ' + USR.LASTNAME AS AGENT, L_OFFICE.NAME AS OFFICE, '' as USED
                FROM DB_USER USR JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                WHERE {0} AND USR.ISACTIVE = 1 AND  USR.AGENTEOMBALANCEREPORTSETTINGS = USR.ID AND L_OFFICE.ISACTIVE = 1  {4};

                -- 4) EOM Rollover
                SELECT DISTRIBUTIONOFFUNDS, USERID
                FROM USERPAYPERIOD
                WHERE PAYPERIODID = {3} AND DISTRIBUTIONOFFUNDS > 0
                ORDER BY USERID;
    
                -- 5) Future commission data
                SELECT 
                    SUM(USS.ACTUALPAYMENT) AS AMOUNT, USS.UserID
                
                FROM USERSALESPLIT USS
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
			    JOIN DB_USER USR on USS.UseRID = USR.ID
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                JOIN SALE S ON SS.SALEID = S.ID
			    where {0} AND S.STATUSID != 3 AND S.SALEDATE IS NOT NULL 
                AND (S.PAYPERIODID IS NULL OR (s.PayPeriodID != {3} AND S.ENTITLEMENTDATE > '{1}')) AND S.SALEDATE <= '{2}'
			    group by USS.UserID
                "
                , szFilter, Utility.formatDate(dtStart), Utility.formatDate(dtEnd), hdPayPeriod.Value, szCompanyFilter);

            DataSet ds = DB.runDataSet(szSQL);
            
            if (ds.Tables[0].Rows.Count > 0)
                formatDataSet(ds);
            GridViewHelper helper = new GridViewHelper(gvTable);
            DataView dtFinal = ds.Tables[0].DefaultView;
            dtFinal.Sort = "OFFICENAME, AGENT";
            helper.RegisterGroup("OFFICENAME", true, true);
            helper.GroupHeader += new GroupEvent(helper_GroupHeader);

            helper.ApplyGroupSort();
            gvTable.DataSource = dtFinal;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }

        private void helper_GroupHeader(string groupName, object[] values, GridViewRow row) {
            row.Height = 30;
            //BackColor = Color.LightGray;
            row.Cells[0].Text = "&nbsp;&nbsp;<strong>" + row.Cells[0].Text + "</strong>";
        }

        protected void gvTable_Sorting(object sender, GridViewSortEventArgs e) {
        }

        protected void formatDataSet(DataSet ds) {
            DataView dvRetainer = ds.Tables[1].DefaultView;
            DataView dvHeldover = ds.Tables[2].DefaultView;
            DataView dvUserDetails = ds.Tables[3].DefaultView;
            DataView dvRollover = ds.Tables[4].DefaultView;
            DataView dvPending = ds.Tables[5].DefaultView;

            foreach (DataRow oR in ds.Tables[0].Rows) {
                dvRetainer.RowFilter = "USERID = " + oR["USERID"].ToString();
                if (dvRetainer.Count > 0)
                    oR["RETAINER"] = Convert.ToDouble(dvRetainer[0]["RETAINER"]);

                dvHeldover.RowFilter = "USERID = " + oR["USERID"].ToString();
                if (dvHeldover.Count > 0)
                    oR["DEBITBALANCE"] = -1 * Convert.ToDouble(dvHeldover[0]["HELDOVER"]);

                dvRollover.RowFilter = "USERID = " + oR["USERID"].ToString();
                dvPending.RowFilter = "USERID = " + oR["USERID"].ToString();
                if (dvPending.Count > 0)
                    oR["PENDING"] = Convert.ToDouble(dvPending[0]["AMOUNT"]);
                if (dvRollover.Count > 0)
                    oR["PENDING"] =  Convert.ToDouble(oR["PENDING"]) + Convert.ToDouble(dvRollover[0]["DISTRIBUTIONOFFUNDS"]);

                dvUserDetails.RowFilter = "USERID = " + oR["USERID"].ToString();
                if (dvUserDetails.Count > 0) {
                    oR["OFFICENAME"] = dvUserDetails[0]["OFFICE"].ToString();
                    oR["AGENT"] = dvUserDetails[0]["AGENT"].ToString();
                    dvUserDetails[0]["USED"] = "1"; //Mark this record as used
                }
            }
            dvUserDetails.RowFilter = "USED  = ''"; //Filter to users who have not been used.
            foreach (DataRowView dr in dvUserDetails) {
                //Add the user to the main dataset
                DataRow drNew = ds.Tables[0].NewRow();
                drNew.ItemArray = ds.Tables[0].Rows[0].ItemArray;
                drNew["AGENT"] = dr["AGENT"].ToString();
                drNew["OFFICENAME"] = dr["OFFICE"].ToString();
                drNew["DEBITBALANCE"] = 0;
                drNew["RETAINER"] = 0;

                dvHeldover.RowFilter = "USERID = " + dr["USERID"].ToString();
                if (dvHeldover.Count > 0)
                    drNew["DEBITBALANCE"] = -1 * Convert.ToDouble(dvHeldover[0]["HELDOVER"]);
                dvRetainer.RowFilter = "USERID = " + dr["USERID"].ToString();
                if (dvRetainer.Count > 0)
                    drNew["RETAINER"] = Convert.ToDouble(dvRetainer[0]["RETAINER"]);

                drNew["PENDING"] = 0;

                ds.Tables[0].Rows.Add(drNew);
            }
        }
    }
}