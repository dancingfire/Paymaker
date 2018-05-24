using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class YTD_sales : Root {
        private Hashtable htValueMonth = new Hashtable();
        private ArrayList alMonths = new ArrayList();
        private ArrayList alOffice = new ArrayList();
        private DataTable oDataTable = new DataTable();
        private int startYear = 2000;
        private int endYear = 3000;
        private bool blnPrint = false;
        private string szCompanyIDList = "";

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szFY = hdFY.Value = Request.QueryString["szFY"].ToString();
            blnPrint = Valid.getBoolean("blnPrint", false);
            szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

            makeDataTable();
            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void makeDataTable() {
            oDataTable.Columns.Add(new DataColumn("MONTH", System.Type.GetType("System.String")));

            DataSet dsOffice = DB.Office.loadOfficeList(szCompanyIDList);
            int colCount = 0;
            foreach (DataRow oRow in dsOffice.Tables[0].Rows) {
                alOffice.Add(Convert.ToInt32(oRow["ID"]));
                oDataTable.Columns.Add(new DataColumn(oRow["NAME"].ToString(), System.Type.GetType("System.String")));
                DataColumn oDC = new DataColumn("NOOFSALES" + "_" + colCount, System.Type.GetType("System.String"));
                oDC.DefaultValue = 0;
                oDataTable.Columns.Add(oDC);
                colCount = colCount + 1;
            }
            oDataTable.Columns.Add(new DataColumn("CORPORATE", System.Type.GetType("System.String")));
            oDataTable.Columns.Add(new DataColumn("TOTAL NO OF SALES", System.Type.GetType("System.String")));

            alMonths.Add(7);
            alMonths.Add(8);
            alMonths.Add(9);
            alMonths.Add(10);
            alMonths.Add(11);
            alMonths.Add(12);
            alMonths.Add(1);
            alMonths.Add(2);
            alMonths.Add(3);
            alMonths.Add(4);
            alMonths.Add(5);
            alMonths.Add(6);
        }

        protected void bindData() {
            bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
            string szUserActive = " AND USR.ISACTIVE = 1 ";
            if (blnIncludeInactive)
                szUserActive = "";
            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

            string szSQL = string.Format(@"
                SELECT DATEPART(YY, S.SALEDATE) AS YEAR, DATEPART(MM, S.SALEDATE) AS MONTH, SUM(USS.GRAPHCOMMISSION) AS USSCALCULATEDAMOUNT,
                SUM(CASE
                    WHEN COMMISSIONTYPEID = 10 THEN 1 ELSE 0 END) AS SALECOUNT,
                L_OFFICE.NAME, L_OFFICE.ID AS OFFICEID
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID AND L_OFFICE.ISACTIVE = 1
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE USR.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 {1}
                {0} {2}
                GROUP BY DATEPART(YY, S.SALEDATE), DATEPART(MM, S.SALEDATE),
                 L_OFFICE.NAME, L_OFFICE.ID
                ", getDateFilter(), szUserActive, szCompanyFilter);
            DataSet ds = DB.runDataSet(szSQL);

            double intSaleCount = 0;
            double dSalesIncome = 0;

            int colCount = 1;

            object[] rowVals = new object[oDataTable.Columns.Count];
            object[] rowFooterVals = new object[oDataTable.Columns.Count];

            DataRowCollection drCollection;
            DataRow drNewRow;
            drCollection = oDataTable.Rows;

            double intSaleCorporateCount = 0;
            double dSaleCorporateSum = 0;
            foreach (int month in alMonths) {
                rowVals = new object[oDataTable.Columns.Count];
                rowVals[0] = getMonth(Convert.ToInt32(month));
                colCount = 0;
                intSaleCorporateCount = 0;
                dSaleCorporateSum = 0;
                foreach (int intOfficeID in alOffice) {
                    dSalesIncome = 0;
                    intSaleCount = 0;
                    DataView dv = ds.Tables[0].DefaultView;
                    dv.RowFilter = string.Format(@" MONTH = {0} AND OFFICEID = {1} ", month, intOfficeID.ToString());
                    foreach (DataRowView drData in dv) {
                        intSaleCount = Convert.ToDouble(drData["SALECOUNT"]);
                        intSaleCorporateCount += intSaleCount;
                        dSalesIncome += Convert.ToDouble(drData["USSCALCULATEDAMOUNT"]);
                        dSaleCorporateSum = dSaleCorporateSum + Convert.ToDouble(drData["USSCALCULATEDAMOUNT"]);
                    }
                    colCount = colCount + 1;
                    rowFooterVals[colCount] = dSalesIncome + Convert.ToDouble(rowFooterVals[colCount]);
                    rowVals[colCount] = "$" + Utility.formatMoneyShort(dSalesIncome);
                    colCount = colCount + 1;
                    rowFooterVals[colCount] = (intSaleCount + Convert.ToDouble(rowFooterVals[colCount])).ToString("N0");
                    rowVals[colCount] = intSaleCount.ToString("N0");
                }
                colCount = colCount + 1;
                rowFooterVals[colCount] = dSaleCorporateSum + Convert.ToDouble(rowFooterVals[colCount]);
                rowVals[colCount] = "$" + Utility.formatMoneyShort(dSaleCorporateSum); ;
                colCount = colCount + 1;
                rowFooterVals[colCount] = (intSaleCorporateCount + Convert.ToDouble(rowFooterVals[colCount])).ToString("N0");
                rowVals[colCount] = intSaleCorporateCount.ToString("N0");
                drNewRow = drCollection.Add(rowVals);
            }
            rowFooterVals[0] = "Totals";
            for (int i = 1; i < rowFooterVals.Length; i = i + 2) {
                rowFooterVals[i] = "$" + Utility.formatMoneyShort(Convert.ToDouble(rowFooterVals[i]));
            }

            drNewRow = drCollection.Add(rowFooterVals);
            oDataTable.AcceptChanges();

            gvTable.DataSource = oDataTable;
            gvTable.DataBind();
            HTML.formatGridView(ref gvTable);
        }

        protected void gvTable_RowDataBound(object sender, GridViewRowEventArgs e) {
            int colCount = 0;
            if (e.Row.RowType == DataControlRowType.DataRow) {
                GridViewRow gvrHeader = ((GridView)sender).HeaderRow;
                colCount = gvrHeader.Cells.Count;
                for (int i = 0; i <= gvrHeader.Cells.Count - 1; i++) {
                    if (gvrHeader.Cells[i].Text.IndexOf("NOOFSALES") > -1)
                        gvrHeader.Cells[i].Text = "NO OF SALES";
                }
            }
            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            for (int j = 0; j < e.Row.Cells.Count; j++) {
                if (e.Row.Cells[j].Text.IndexOf("$") > -1) {
                    e.Row.Cells[j].HorizontalAlign = HorizontalAlign.Right;
                } else {
                    e.Row.Cells[j].HorizontalAlign = HorizontalAlign.Center;
                }
            }
        }

        private string getMonth(int intMonth) {
            string szMonth = "";
            switch (intMonth) {
                case 1: szMonth = "JAN"; break;
                case 2: szMonth = "FEB"; break;
                case 3: szMonth = "MAR"; break;
                case 4: szMonth = "APR"; break;
                case 5: szMonth = "MAY"; break;
                case 6: szMonth = "JUN"; break;
                case 7: szMonth = "JUL"; break;
                case 8: szMonth = "AUG"; break;
                case 9: szMonth = "SEPT"; break;
                case 10: szMonth = "OCT"; break;
                case 11: szMonth = "NOV"; break;
                case 12: szMonth = "DEC"; break;
            }
            return szMonth;
        }

        private string getDateFilter() {
            startYear = Convert.ToInt32(hdFY.Value) - 1;
            endYear = Convert.ToInt32(hdFY.Value);
            DateTime dtEndDate = Utility.getEndOfLastMonth();
            pPageHeader.InnerHtml = String.Format("Monthly sales - Jul 1, {0} to {1}", startYear, Utility.formatDate(dtEndDate));
            return string.Format(@" AND S.SALEDATE > 'Jun 30, {0} 0:00' and S.SALEDATE <= '{1} 23:59'", startYear, Utility.formatDate(dtEndDate));
        }
    }
}