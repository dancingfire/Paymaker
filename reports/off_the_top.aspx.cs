using System;
using System.Collections;
using System.Data;

namespace Paymaker {

    public partial class off_the_top : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        private ReportFilters oF = null;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            oF = new ReportFilters();
            oF.loadUserFilters();

            pPageHeader.InnerHtml = pPageHeader.InnerHtml + " between " + Utility.formatDate(oF.StartDate) + " - " + Utility.formatDate(oF.EndDate);
            if (oF.CompanyNames != "") {
                pPageHeader.InnerHtml += "<br/><span><strong>Company: </strong>" + oF.CompanyNames + "</span>";
            }
            blnPrint = Valid.getBoolean("blnPrint", false);

            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szFilter = String.Format(" S.SALEDATE BETWEEN '{0}' AND '{1}'", oF.getDBSafeStartDate(), oF.getDBSafeEndDate());

            //Company filter is processed below - we need all the rows to determine the split percentage

            string szSQL = string.Format(@"
                SELECT S.ID, S.ADDRESS, ISNULL(OWNER.CALCULATEDAMOUNT, 0) AS CALCULATEDAMOUNT, OWNER.COMPANYID,
                S.SALEDATE, ISNULL((SELECT SE.CALCULATEDAMOUNT FROM SALEEXPENSE SE JOIN LIST L ON SE.EXPENSETYPEID = L.ID AND L.NAME like 'Gifts%' WHERE SE.SALEID = S.ID), 0) as GIFTS,
                ISNULL((SELECT sum(SE.CALCULATEDAMOUNT) FROM SALEEXPENSE SE JOIN LIST L ON SE.EXPENSETYPEID = L.ID AND L.NAME like '%Incentive%' WHERE SE.SALEID = S.ID), 0) as INCENTIVE
                FROM SALE S LEFT JOIN
					(SELECT SS.SALEID,  SUM(USS.CALCULATEDAMOUNT) AS CALCULATEDAMOUNT, L_OFFICE.COMPANYID
					FROM SALESPLIT SS JOIN USERSALESPLIT USS ON SS.ID = USS.SALESPLITID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
					JOIN DB_USER USR ON USR.ID = USS.USERID
					JOIN LIST L_OFFICE ON L_OFFICE.ID = USS.OFFICEID
					JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
					WHERE COMMISSIONTYPEID = 10
                    GROUP BY SS.SALEID, L_OFFICE.COMPANYID) OWNER ON S.ID = OWNER.SALEID
                WHERE S.STATUSID IN (1, 2) AND {0}
                ORDER BY SUBSTRING(S.ADDRESS, CHARINDEX(' ', S.ADDRESS) + 1, LEN(S.ADDRESS))"
                , szFilter);

            DataSet ds = DB.runDataSet(szSQL);
            formatDataSet(ds);
        }

        protected void formatDataSet(DataSet ds) {
            double dblGifts = 0;
            double dblIncentive = 0;

            Hashtable htDups = new Hashtable();
            //process all the sales to determine the correct splits based on the listing percentage
            DataView dv = ds.Tables[0].DefaultView;

            foreach (DataRow oR in ds.Tables[0].Rows) {
                if (htDups.ContainsKey(oR["ID"].ToString())) {
                    continue;
                }
                htDups.Add(oR["ID"].ToString(), 1);
                //Filter the rows to the current sale
                dv.RowFilter = "ID = " + oR["ID"].ToString();
                double dTotal = 0;
                foreach (DataRowView drv in dv) {
                    dTotal += Convert.ToDouble(drv["CALCULATEDAMOUNT"]);
                }
                //Set the amount to the proportional value
                foreach (DataRowView drv in dv) {
                    drv["GIFTS"] = Convert.ToDouble(drv["GIFTS"]) * (Convert.ToDouble(drv["CALCULATEDAMOUNT"]) / dTotal);
                    drv["INCENTIVE"] = Convert.ToDouble(drv["INCENTIVE"]) * (Convert.ToDouble(drv["CALCULATEDAMOUNT"]) / dTotal);
                    drv.EndEdit();
                }
            }
            //Apply the company filter now that we have assigned costs
            if (!String.IsNullOrWhiteSpace(oF.CompanyID))
                dv.RowFilter = String.Format(" COMPANYID IN (-1,{0})", oF.CompanyID);

            foreach (DataRowView oR in dv) {
                dblGifts += Convert.ToDouble(oR["GIFTS"]);
                dblIncentive += Convert.ToDouble(oR["INCENTIVE"]);
            }
            DataRowView drTotals = dv.AddNew();

            drTotals["COMPANYID"] = -1;
            drTotals["ADDRESS"] = "<b>Totals</b>";
            drTotals["GIFTS"] = dblGifts;
            drTotals["INCENTIVE"] = dblIncentive;
            drTotals.EndEdit();

            gvTable.DataSource = dv;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }
    }
}