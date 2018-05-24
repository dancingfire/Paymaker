using System;
using System.Data;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class quarterly_top_performer : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szQuarter = Request.QueryString["szQuarter"].ToString();
            blnPrint = Valid.getBoolean("blnPrint", false);

            pPageHeader.InnerHtml = pPageHeader.InnerHtml + "<br/>" + szQuarter;
            bindData(szQuarter);
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData(string DateFilter) {
            string szFilter = String.Format(" AND S.SALEDATE {0}", DateFilter);
            string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

            string szSQL = string.Format(@"
                SELECT SUM(USS.GRAPHCOMMISSION AS COMMISSION,
                SUM(CASE
                    WHEN COMMISSIONTYPEID = 6 THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * .11 --lead
                    WHEN COMMISSIONTYPEID = 10 THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * .45 --list
                    WHEN COMMISSIONTYPEID = 7 THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * .22
                    WHEN COMMISSIONTYPEID = 8 THEN  (USS.CALCULATEDAMOUNT/SS.CALCULATEDAMOUNT) * .22
                END ) AS SALECOUNT,
                SUM(CASE
                    WHEN COMMISSIONTYPEID = 6 THEN  (S.SALEPRICE) * .11 --lead
                    WHEN COMMISSIONTYPEID = 10 THEN  (S.SALEPRICE) * .45 --list
                    WHEN COMMISSIONTYPEID = 7 THEN  (S.SALEPRICE) * .22
                    WHEN COMMISSIONTYPEID = 8 THEN  (S.SALEPRICE) * .22
                END ) AS SALETOTAL,
                '' AS OFFICENAME, '' [AGENT], QUARTERLYTOPPERFORMERREPORTSETTINGS AS USERID
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                   JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                JOIN LIST L_OFFICE ON USR.OFFICEID = L_OFFICE.ID
                WHERE SS.CALCULATEDAMOUNT > 0 {0}  AND USR.ISACTIVE = 1 {1} AND USR.ISPAID = 1
                GROUP BY USR.QUARTERLYTOPPERFORMERREPORTSETTINGS;

                SELECT USR.ID, USR.FIRSTNAME + ' ' + USR.LASTNAME AS AGENT, L_OFFICE.NAME AS OFFICE
                FROM DB_USER USR JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                WHERE 1 = 1 {1}
                ", szFilter, szCompanyFilter);

            DataSet ds = DB.runDataSet(szSQL);
            formatDataSet(ds);

            GridViewHelper helper = new GridViewHelper(gvTable);

            helper.RegisterGroup("OFFICENAME", true, true);
            helper.GroupHeader += new GroupEvent(helper_GroupHeader);

            helper.ApplyGroupSort();

            DataView dv = ds.Tables[0].DefaultView;
            dv.Sort = "OFFICENAME, AGENT";

            gvTable.DataSource = dv;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }

        protected void formatDataSet(DataSet ds) {
            DataView dvUserDetails = ds.Tables[1].DefaultView;

            foreach (DataRow oR in ds.Tables[0].Rows) {
                if (oR["USERID"].ToString() == "")
                    continue;
                dvUserDetails.RowFilter = "ID = " + oR["USERID"].ToString();
                if (dvUserDetails.Count > 0) {
                    oR["AGENT"] = dvUserDetails[0]["AGENT"].ToString();
                    oR["OFFICENAME"] = dvUserDetails[0]["OFFICE"].ToString();
                }
            }
        }

        private void helper_GroupHeader(string groupName, object[] values, GridViewRow row) {
            row.Height = 30;
            //BackColor = Color.LightGray;
            row.Cells[0].Text = "&nbsp;&nbsp;<strong>" + row.Cells[0].Text + "</strong>";
        }

        protected void gvTable_Sorting(object sender, GridViewSortEventArgs e) {
        }
    }
}