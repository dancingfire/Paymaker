using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace Paymaker {

    internal enum ChartType {
        NOOFSALES = 0,
        SALESDOLLARS = 1
    }

    public partial class no_of_sales : Root {
        private ChartArea chtArea = new ChartArea("chtArea");
        private Hashtable htIDNameOffice = new Hashtable();
        private ArrayList alMonthYear = new ArrayList();
        private ArrayList alOffice = new ArrayList();
        private int intMarkerStyle = 1;
        private DataSet dsData = null;
        private int startYear = 2000;
        private int endYear = 3000;
        private ChartType oChartType = ChartType.NOOFSALES;
        private bool blnPrint = false;

        private DateTime dtMaxValue = DateTime.MinValue;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szFY = hdFY.Value = Valid.getText("szFY", VT.TextNormal);
            oChartType = (ChartType)Enum.Parse(typeof(ChartType), Valid.getText("szChtType", VT.TextNormal), true);
            blnPrint = Valid.getBoolean("blnPrint", false);

            getDateRange(szFY);
            loadData();
            loadMonthList(szFY);
            makeChart();
        }

        protected void makeChart() {
            setChart();
            switch (oChartType) {
                case ChartType.NOOFSALES:
                    makeNoOfSaleChart();
                    break;

                case ChartType.SALESDOLLARS:
                    makeSalesDollarsChart();
                    break;
            }
            if (blnPrint) {
                chtNoOfSales.Width = Unit.Pixel(990);//2475
                chtNoOfSales.Height = Unit.Pixel(580);//3525

                Charts.sendChartToClient(chtNoOfSales);
            } else {
                chtNoOfSales.Width = Unit.Pixel(1000);//2475
                chtNoOfSales.Height = Unit.Pixel(580);//3525
            }
        }

        protected void getDateRange(string szValue) {
            if (szValue.IndexOf(",") > -1) {
                string[] arValue = szValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                startYear = Convert.ToInt32(arValue[0]);
                endYear = Convert.ToInt32(arValue[0]);
                foreach (string szFY in arValue) {
                    if (Convert.ToInt32(szFY) < startYear) {
                        startYear = Convert.ToInt32(szFY);
                    }
                    if (Convert.ToInt32(szFY) > endYear) {
                        endYear = Convert.ToInt32(szFY);
                    }
                }
            } else {
                startYear = Convert.ToInt32(szValue) - 1;
                endYear = Convert.ToInt32(szValue);
            }
        }

        protected void loadMonthList(string szValue) {
            alMonthYear.Clear();
            if (szValue.IndexOf(",") > -1) {
                string[] arValue = szValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                startYear = Convert.ToInt32(arValue[0]);
                endYear = Convert.ToInt32(arValue[0]);
                foreach (string szFY in arValue) {
                    if (Convert.ToInt32(szFY) < startYear) {
                        startYear = Convert.ToInt32(szFY);
                    }
                    if (Convert.ToInt32(szFY) > endYear) {
                        endYear = Convert.ToInt32(szFY);
                    }
                    fillMonthYear(Convert.ToInt32(szFY));
                }
            } else {
                startYear = Convert.ToInt32(szValue) - 1;
                endYear = Convert.ToInt32(szValue);
                fillMonthYear(Convert.ToInt32(szValue));
            }
        }

        protected void setChartTitle() {
            switch (oChartType) {
                case ChartType.NOOFSALES:
                    chtNoOfSales.Titles[0].Text = "Number of Sales Per Office";
                    break;

                case ChartType.SALESDOLLARS:
                    chtNoOfSales.Titles[0].Text = "Sales Dollars Per Office";
                    break;
            }
        }

        protected void setChart() {
            setChartTitle();
            chtNoOfSales.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            chtNoOfSales.BackColor = ColorTranslator.FromHtml("#D3DFF0");
            chtNoOfSales.BorderlineDashStyle = ChartDashStyle.Solid;
            chtNoOfSales.Palette = ChartColorPalette.None;
            chtNoOfSales.PaletteCustomColors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Brown, Color.Orange, Color.LightSkyBlue, Color.Black }; chtNoOfSales.BackSecondaryColor = Color.White;
            chtNoOfSales.BackGradientStyle = GradientStyle.TopBottom;
            chtNoOfSales.BorderlineWidth = 2;
            chtNoOfSales.BorderlineColor = Color.FromArgb(26, 59, 105);
            chtNoOfSales.Width = Unit.Pixel(800);//2475
            chtNoOfSales.Height = Unit.Pixel(480);//3525
            setChartArea();
        }

        protected void setChartArea() {
            chtArea.BackColor = Color.Transparent;
            chtArea.ShadowColor = Color.Transparent;
            chtArea.AxisX.LabelStyle.Format = "MMM yy";
            chtArea.AxisY.IsStartedFromZero = true;
            chtArea.AxisX.IsStartedFromZero = false;
            chtArea.AxisY.Minimum = 0;
            if (ChartType.NOOFSALES == oChartType) {
                chtArea.AxisY.Interval = 10;
            }
            if (ChartType.SALESDOLLARS == oChartType) {
                chtArea.AxisY.Interval = 100000;
                chtArea.AxisY.LabelStyle.Format = "${0}";
            }
            chtArea.AxisX.Interval = 1; //shows every month, year on the chart
            chtArea.AxisY.MajorGrid.Enabled = true;
            chtArea.AxisX.MajorGrid.Enabled = false;
            chtArea.AxisX.MajorGrid.LineColor = Color.Transparent;
            chtArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chtNoOfSales.ChartAreas.Add(chtArea);//Add chart area to chart
        }

        protected Series getSeries(string szVlaue) {
            Series oSeries = new Series(szVlaue);
            oSeries.ChartType = SeriesChartType.Line;
            return oSeries;
        }

        protected Legend getLegend(string szValue) {
            Legend oLegend = new Legend(szValue);
            oLegend.DockedToChartArea = "chtArea";
            oLegend.IsDockedInsideChartArea = false;
            oLegend.BackColor = Color.Transparent;
            oLegend.BorderDashStyle = ChartDashStyle.Solid;
            oLegend.BorderWidth = 1;
            oLegend.BorderColor = Color.FromArgb(26, 59, 105);
            oLegend.Alignment = StringAlignment.Center;
            oLegend.IsEquallySpacedItems = true;
            oLegend.AutoFitMinFontSize = 7;
            oLegend.IsTextAutoFit = true;
            oLegend.Position.Auto = true;
            if (ChartType.SALESDOLLARS == oChartType)
                oLegend.Position = new ElementPosition(16, 3, 12, 26);
            else
                oLegend.Position = new ElementPosition(10, 3, 12, 26);
            oLegend.BackColor = Color.WhiteSmoke;
            return oLegend;
        }

        protected void loadData() {
            string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);

            bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
            string szUserActive = " AND USR.ISACTIVE = 1 ";
            if (blnIncludeInactive)
                szUserActive = "";
            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

            string szSQL = string.Format(@"
                SELECT DATEPART(YYYY, S.SALEDATE) AS YEAR, DATEPART(MM, S.SALEDATE) AS MONTH, S.ID, USR.OFFICEID, 1 AS SALECOUNT,
                S.GROSSCOMMISSION, USS.USERID, L_OFFICE.DESCRIPTION AS OFFICENAME ,
                L_SALESPLIT.NAME AS SPLITNAME, 0 as DATESORT
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID  AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID {2}
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID --AND L_SALESPLIT.EXCLUDEONREPORT = 0
                WHERE USR.ID > 0 AND S.STATUSID IN (1, 2) {1}
                {0}
                UNION --Sales history
                SELECT DATEPART(YYYY, SH.MONTHDATE) AS YEAR, DATEPART(MM, SH.MONTHDATE) AS MONTH, SH.ID as ID, SH.OFFICEID, SH.SALECOUNT,
                SALEVALUE, 0 as USERID, L_OFFICE.DESCRIPTION AS OFFICENAME , 'Lister' as SPLITNAME, 0
                FROM SALEHISTORY SH
                JOIN LIST L_OFFICE ON L_OFFICE.ID = SH.OFFICEID AND L_OFFICE.ISACTIVE = 1
                WHERE 1=1 {3} {2}
                ORDER BY 1, 2, 3;
                ", getDateFilter("S.SALEDATE"), szUserActive, szCompanyFilter, getDateFilter("SH.MONTHDATE"));
            dsData = DB.runDataSet(szSQL);
            foreach (DataRow dr in dsData.Tables[0].Rows) {
                DateTime SaleDate = new DateTime(Convert.ToInt32(dr["YEAR"]), Convert.ToInt32(dr["MONTH"]), 28);
                if (SaleDate > dtMaxValue)
                    dtMaxValue = SaleDate;
                if (!htIDNameOffice.ContainsKey(Convert.ToInt32(dr["OFFICEID"]))) {
                    alOffice.Add(dr["OFFICENAME"].ToString());
                    htIDNameOffice.Add(Convert.ToInt32(dr["OFFICEID"]), dr["OFFICENAME"].ToString());
                }
            }
        }

        protected void makeNoOfSaleChart() {
            foreach (DictionaryEntry office in htIDNameOffice) {
                drawOfficeSaleCount(Convert.ToInt32(office.Key), office.Value.ToString());
            }
            drawOfficeSaleCount(-1, "Total");
        }

        private void drawOfficeSaleCount(int OfficeID, string OfficeName) {
            int intSaleCount = 0;
            DataTable dtData = new DataTable();
            dtData.Columns.Add(new DataColumn("MONTH", System.Type.GetType("System.Int32")));
            dtData.Columns.Add(new DataColumn("YEAR", System.Type.GetType("System.Int32")));
            DataColumn oDC = new DataColumn("NOOFSALES", System.Type.GetType("System.Int32"));
            oDC.DefaultValue = 0;
            dtData.Columns.Add(oDC);
            object[] rowVals = new object[dtData.Columns.Count];
            DataRowCollection drCollection;
            DataRow drNewRow;
            drCollection = dtData.Rows;
            foreach (string mnthYear in alMonthYear) {
                intSaleCount = 0;
                rowVals[0] = mnthYear.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0].ToString();// month;
                rowVals[1] = mnthYear.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1];// year
                DataView dv = dsData.Tables[0].DefaultView;
                if (OfficeID > -1)
                    dv.RowFilter = string.Format(@" MONTH = {0} AND YEAR = {1} AND OFFICEID = {2} ", rowVals[0].ToString(), rowVals[1].ToString(), OfficeID);
                else
                    dv.RowFilter = string.Format(@" MONTH = {0} AND YEAR = {1} ", rowVals[0].ToString(), rowVals[1].ToString());
                foreach (DataRowView drData in dv) {
                    if (drData["SPLITNAME"].ToString().IndexOf("Lister") > -1) {
                        intSaleCount = intSaleCount + Convert.ToInt32(drData["SALECOUNT"]);
                    }
                }
                rowVals[2] = intSaleCount;
                drNewRow = drCollection.Add(rowVals);
            }
            dtData.AcceptChanges();
            Series oSeries = getSeries(OfficeName);
            int intMaxCount = 0;
            DataView dvFinal = dtData.DefaultView;
            dvFinal.Sort = "YEAR, MONTH";
            foreach (DataRowView oR in dvFinal) {
                DateTime xDate = new DateTime(Convert.ToInt32(oR["YEAR"]), Convert.ToInt32(oR["MONTH"]), 1);
                int intNoSales = Convert.ToInt32(oR["NOOFSALES"]);
                if (Convert.ToInt32(oR["NOOFSALES"]) > intMaxCount)
                    intMaxCount = Convert.ToInt32(oR["NOOFSALES"]);
                oSeries.Points.AddXY(xDate, Convert.ToInt32(oR["NOOFSALES"]));
                oSeries.IsXValueIndexed = true;
                oSeries.YValueType = ChartValueType.Int32;
                oSeries.MarkerStyle = getNextMarkerStyle(ref intMarkerStyle);
            }
            chtNoOfSales.ChartAreas[0].AxisY.Maximum = intMaxCount;
            chtNoOfSales.Series.Add(oSeries);
            chtNoOfSales.Legends.Add(getLegend(OfficeName));
        }

        protected void makeSalesDollarsChart() {
            foreach (DictionaryEntry office in htIDNameOffice)
                drawOfficeDollarLine(Convert.ToInt32(office.Key), office.Value.ToString());
            drawOfficeDollarLine(-1, "Total");
        }

        private void drawOfficeDollarLine(int OfficeID, string OfficeName) {
            double dDollarTotal = 0;
            DataTable dtData = new DataTable();
            dtData.Columns.Add(new DataColumn("MONTH", System.Type.GetType("System.Int32")));
            dtData.Columns.Add(new DataColumn("YEAR", System.Type.GetType("System.Int32")));
            DataColumn oDC = new DataColumn("DOLLARS", System.Type.GetType("System.Double"));
            oDC.DefaultValue = 0;
            dtData.Columns.Add(oDC);
            object[] rowVals = new object[dtData.Columns.Count];
            DataRowCollection drCollection;
            DataRow drNewRow;
            drCollection = dtData.Rows;
            foreach (string mnthYear in alMonthYear) {
                dDollarTotal = 0;
                rowVals[0] = mnthYear.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0].ToString();// month;
                rowVals[1] = mnthYear.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1];// year
                DataView dv = dsData.Tables[0].DefaultView;
                if (OfficeID > -1)
                    dv.RowFilter = string.Format(@" MONTH = {0} AND YEAR = {1} AND OFFICEID = {2} ", rowVals[0].ToString(), rowVals[1].ToString(), OfficeID);
                else
                    dv.RowFilter = string.Format(@" MONTH = {0} AND YEAR = {1} ", rowVals[0].ToString(), rowVals[1].ToString());
                int intCurrSaleID = -1;
                foreach (DataRowView drData in dv) {
                    if (intCurrSaleID != Convert.ToInt32(drData["ID"])) {
                        dDollarTotal += Convert.ToDouble(drData["GROSSCOMMISSION"].ToString());
                        intCurrSaleID = Convert.ToInt32(drData["ID"]);
                    }
                }
                rowVals[2] = dDollarTotal;
                drNewRow = drCollection.Add(rowVals);
            }
            dtData.AcceptChanges();
            Series oSeries = getSeries(OfficeName);
            DataView dvFinal = dtData.DefaultView;
            dvFinal.Sort = "YEAR, MONTH";
            foreach (DataRowView oR in dvFinal) {
                DateTime xDate = new DateTime(Convert.ToInt32(oR["YEAR"]), Convert.ToInt32(oR["MONTH"]), 1);
                double dValue = Convert.ToDouble(oR["DOLLARS"]);
                oSeries.Points.AddXY(xDate, dValue);
                oSeries.IsXValueIndexed = true;
                oSeries.YValueType = ChartValueType.Double;
                oSeries.MarkerStyle = getNextMarkerStyle(ref intMarkerStyle);
            }
            chtNoOfSales.Series.Add(oSeries);
            chtNoOfSales.Legends.Add(getLegend(OfficeName));
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

        private string getDateFilter(string FieldName) {
            DateTime dtEndDate = new DateTime(endYear, 6, 30);
            if (endYear == DateTime.Now.Year)
                dtEndDate = Utility.getEndOfLastMonth();
            return string.Format(@" AND {2} BETWEEN 'Jun 30, {0} 0:00'  AND  '{1} 23:59'", startYear - 1, Utility.formatDate(dtEndDate), FieldName);
        }

        protected void fillMonthYear(int intFY) {
            checkMonthYear(7, intFY - 1);
            checkMonthYear(8, intFY - 1);
            checkMonthYear(9, intFY - 1);
            checkMonthYear(10, intFY - 1);
            checkMonthYear(11, intFY - 1);
            checkMonthYear(12, intFY - 1);
            checkMonthYear(1, intFY);
            checkMonthYear(2, intFY);
            checkMonthYear(3, intFY);
            checkMonthYear(4, intFY);
            checkMonthYear(5, intFY);
            checkMonthYear(6, intFY);
        }

        private void checkMonthYear(int Month, int Year) {
            DateTime dtCheck = new DateTime(Year, Month, 1);
            if (dtCheck > dtMaxValue)
                return;
            alMonthYear.Add(Month + "_" + Year);
        }

        protected MarkerStyle getNextMarkerStyle(ref int intSeriesCount) {
            if (intSeriesCount == 5) {
                //reset it to 1
                intSeriesCount = 1;
            } else {
                intSeriesCount = intSeriesCount + 1;
            }
            return (MarkerStyle)intSeriesCount;
        }
    }
}