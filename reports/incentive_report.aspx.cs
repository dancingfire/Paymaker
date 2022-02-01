using System;
using System.Data;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class incentive_report : Root {
        private bool blnPrint = false;
        private bool blnShowChart = false;
        private int startYear = 2000;
        private int endYear = 3000;
        private ChartArea chtArea = new ChartArea("chtArea");
        protected int iColor = 38;
        private Series oSeries;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            hdOfficeID.Value = Valid.getText("szOfficeID", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);
            hdExpenseID.Value = Valid.getText("szExpenseID", "", VT.TextNormal);
            hdFY.Value = Valid.getText("szFY", "", VT.TextNormal);
            string szOfficeFilterNames = Valid.getText("szOfficeNames", "", VT.TextNormal);
            string szCompanyFilterNames = Valid.getText("szCompanyNames", "", VT.TextNormal);
            string szExpenseFilterNames = Valid.getText("szExpenseNames", "", VT.TextNormal);

            blnPrint = Valid.getBoolean("blnPrint", false);
            blnShowChart = Valid.getBoolean("blnShowChart", false);
            if (hdOfficeID.Value == "null")
                hdOfficeID.Value = "";
            if (hdCompanyID.Value == "null")
                hdCompanyID.Value = "";

            bindData();
            pPageHeader.InnerHtml = pPageHeader.InnerHtml + szExpenseFilterNames + " " + startYear + "/" + endYear;
            if (szOfficeFilterNames != "") {
                pPageHeader.InnerHtml += "<br/><span class='Normal'><strong>Office: </strong>" + szOfficeFilterNames + "</span>";
            }
            if (szCompanyFilterNames != "") {
                pPageHeader.InnerHtml += "<br/><span class='Normal'><strong>Company: </strong>" + szCompanyFilterNames + "</span>";
            }

            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected void bindData() {
            string szFilter = getDateFilter();
            if (!String.IsNullOrWhiteSpace(hdOfficeID.Value))
                szFilter += String.Format(" AND L_OFFICE.ID IN ({0})", hdOfficeID.Value);
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szFilter += String.Format(" AND L_COMPANY.ID IN ({0})", hdCompanyID.Value);
            string szReferralSQL = "";
            if (Valid.getText("blnExcludeReferral", "") == "true") {
                szReferralSQL = " - ISNULL(MAX(OTT.TOTAL), 0) ";
            }
            string szSQL = string.Format(@"
                SELECT USR.LASTNAME + ', ' + USR.FIRSTNAME AS AGENT, SUM(USS.GRAPHCOMMISSION) {1} AS GrossSalesYTD,
                    MAX(L_OFFICE.NAME) AS OFFICE, MAX(USR.SALESTARGET) AS SALESTARGET, MAX(USR.INITIALSCODE) AS INITIALSCODE
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS = 0
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0 AND USR.INCENTIVESUMMARYREPORTSETTINGS = 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USR.OFFICEID
                JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID AND L_SALESPLIT.EXCLUDEONREPORT = 0
                LEFT JOIN (
				    SELECT SUM(CALCULATEDAMOUNT) as TOTAL, SALEID 
				    FROM SALEEXPENSE WHERE EXPENSETYPEID IN (140, 48)
				    GROUP BY SALEID ) OTT ON OTT.SALEID = S.ID
                WHERE {0} AND SS.CALCULATEDAMOUNT > 0 AND USR.ISACTIVE = 1
                GROUP BY USR.ID, USR.LASTNAME + ', ' + USR.FIRSTNAME, USR.ROLEID
                ORDER BY USR.ROLEID, USR.LASTNAME + ', ' + USR.FIRSTNAME"
                , szFilter, szReferralSQL);

            DataSet ds = DB.runDataSet(szSQL);
            formatDataSet(ds.Tables[0]);
        }

        protected void formatDataSet(DataTable dtUserData) {
            dtUserData.Columns.Add(new DataColumn("ProRatedTarget", System.Type.GetType("System.Double")));
            dtUserData.Columns.Add(new DataColumn("AnnualTargetPercent", System.Type.GetType("System.Double")));
            dtUserData.Columns.Add(new DataColumn("ProRatedTargetPercent", System.Type.GetType("System.Double")));
            dtUserData.Columns.Add(new DataColumn("GrossSales90", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("GrossSales100", System.Type.GetType("System.String")));
            dtUserData.Columns.Add(new DataColumn("GrossSales110", System.Type.GetType("System.String")));

            DataRow oTotalRow = dtUserData.NewRow();
            foreach (DataRow drUser in dtUserData.Rows) {
                double dUserTotal = 0;
                double dSalesTarget = Convert.ToDouble(drUser["SalesTarget"]);
                string szInitialValue = drUser["GrossSalesYTD"].ToString();
                if (Utility.IsNumeric(szInitialValue))
                    dUserTotal = Convert.ToDouble(szInitialValue);
                double dYearFraction = 1;
                if (new DateTime(endYear, 6, 30) >= G.CurrentPayPeriodStart) {
                    dYearFraction = G.CurrentPayPeriodStart.Month - 1;
                    if (dYearFraction < 6)
                        dYearFraction += 6; //Jan = 7
                    else
                        dYearFraction -= 5; // Jun = 1

                    dYearFraction = dYearFraction / 12.0;
                }
                double dFractionSales = dSalesTarget * dYearFraction;

                drUser["GrossSalesYTD"] = Utility.formatMoney(dUserTotal);
                drUser["ProRatedTarget"] = Utility.formatMoney(dFractionSales);
                if (dSalesTarget > 0)
                    drUser["AnnualTargetPercent"] = ((dUserTotal / dSalesTarget) * 100).ToString("N2");
                drUser["ProRatedTargetPercent"] = 0;

                if (dSalesTarget > 0)
                    drUser["ProRatedTargetPercent"] = ((dUserTotal / dFractionSales) * 100).ToString("N2");

                double dFractionPercent = (dUserTotal / dFractionSales) * 100;
                if (dFractionPercent > 90)
                    drUser["GrossSales90"] = "X";
                if (dFractionPercent > 100)
                    drUser["GrossSales100"] = "X";
                if (dFractionPercent > 110)
                    drUser["GrossSales110"] = "X";
            }
            dtUserData.AcceptChanges();
            if (blnShowChart) {
                showChart(dtUserData);
                divReport.Visible = false;
            } else {
                divChart.Visible = false;
                addTotalRow(dtUserData);
                gvTable.DataSource = dtUserData;
                gvTable.DataBind();
                HTML.formatGridView(ref gvTable);
            }
        }

        private void addTotalRow(DataTable dtUser) {
            DataRow drTotal = dtUser.NewRow();

            foreach (DataColumn oDC in dtUser.Columns) {
                double dTotal = 0;
                if (oDC.ColumnName != "AGENT" && oDC.ColumnName != "AnnualTargetPercent" && oDC.ColumnName != "GrossSales90" && oDC.ColumnName != "GrossSales100" && oDC.ColumnName != "GrossSales110" && oDC.ColumnName != "ProRatedTargetPercent") {
                    foreach (DataRow dr in dtUser.Rows) {
                        if (Utility.IsNumeric(dr[oDC.ColumnName].ToString())) {
                            dTotal += Convert.ToDouble(dr[oDC.ColumnName].ToString());
                        }
                    }
                    drTotal[oDC.ColumnName] = dTotal;
                }
            }
            dtUser.Rows.Add(drTotal);
        }

        private string getDateFilter() {
            startYear = Convert.ToInt32(hdFY.Value) - 1;
            endYear = Convert.ToInt32(hdFY.Value);
            return string.Format(@" S.SALEDATE > 'Jun 30, {0} 0:00' and S.SALEDATE < 'July 01, {1} 23:59'", startYear, endYear);
        }

        private void showChart(DataTable dv) {
            divChart.Visible = true;
            setChart();
            StripLine sl = new StripLine();
            sl.Interval = -1;
            sl.BackColor = Color.Red;
            sl.IntervalOffset = 90;
            sl.StripWidth = 2;

            sl.BorderDashStyle = ChartDashStyle.Solid;

            chtTopPerformers.ChartAreas[0].AxisY.StripLines.Add(sl);
            sl = new StripLine();
            sl.Interval = -1;
            sl.BackColor = Color.Silver;
            sl.IntervalOffset = 100;
            sl.BorderDashStyle = ChartDashStyle.Dot;
            sl.StripWidth = 2;
            chtTopPerformers.ChartAreas[0].AxisY.StripLines.Add(sl);

            Legend oL = new Legend("Gross sales 90% target");
            oL.ForeColor = Color.Red;
            oL.Docking = Docking.Right;

            oL.CustomItems.Add(Color.Red, "Gross sales 90% target");
            oL.CustomItems.Add(Color.Silver, "Gross sales 100% target");
            oL.CustomItems.Add(Color.Yellow, "Gross sales 110% target");
            chtTopPerformers.Legends.Add(oL);

            oSeries = getSeries("% of target achieved");
            foreach (DataRow oR in dv.Rows) {
                oSeries.Points.AddXY(oR["INITIALSCODE"].ToString(), Math.Round(Convert.ToDouble(oR["ProRatedTargetPercent"])));
                oSeries.Font = new Font("Arial narrow", 8);
                oSeries.YValueType = ChartValueType.Int32;
            }
            chtTopPerformers.Series.Add(oSeries);
            chtTopPerformers.Series[oSeries.Name]["PointWidth"] = (0.4).ToString();
            sl = new StripLine();
            sl.Interval = -1;
            sl.BackColor = Color.Yellow;
            sl.IntervalOffset = 110;
            sl.BorderDashStyle = ChartDashStyle.Dash;
            sl.StripWidth = 2;
            chtTopPerformers.ChartAreas[0].AxisY.StripLines.Add(sl);

            chtArea.AxisY.Maximum = 130;
            chtArea.AxisY.Interval = 10;
            chtArea.AxisX.LabelAutoFitMaxFontSize = 8;
            if (blnPrint) {
                Charts.sendChartToClient(chtTopPerformers);
            }
        }

        protected Series getSeries(string szValue) {
            Series oSeries = new Series(szValue);
            oSeries.ChartType = SeriesChartType.Column;
            return oSeries;
        }

        protected void setChart() {
            chtTopPerformers.Titles[0].Text = "Sales targets";
            chtTopPerformers.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            chtTopPerformers.BackColor = ColorTranslator.FromHtml("#D3DFF0");
            chtTopPerformers.BorderlineDashStyle = ChartDashStyle.Solid;
            chtTopPerformers.Palette = ChartColorPalette.BrightPastel;
            chtTopPerformers.BackSecondaryColor = Color.White;

            chtTopPerformers.BackGradientStyle = GradientStyle.TopBottom;
            chtTopPerformers.BorderlineWidth = 2;
            chtTopPerformers.BorderlineColor = Color.FromArgb(26, 59, 105);
            chtTopPerformers.Width = Unit.Pixel(1600);//2475
            chtTopPerformers.Height = Unit.Pixel(580);//3525
            setChartArea();
        }

        protected void setChartArea() {
            chtArea.BackColor = Color.Transparent;
            chtArea.ShadowColor = Color.Transparent;
            chtArea.AxisY.IsStartedFromZero = true;
            chtArea.AxisX.IsStartedFromZero = false;
            chtArea.AxisY.Minimum = 0;
            chtArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chtArea.AxisY.LabelStyle.Format = "{0}%";
            chtArea.AxisX.Interval = 1; //shows every month, year on the chart

            chtArea.AxisY.MajorGrid.Enabled = true;
            chtArea.AxisX.MajorGrid.Enabled = false;
            chtArea.AxisX.MajorGrid.LineColor = Color.Transparent;
            chtArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.NotSet;

            chtTopPerformers.ChartAreas.Add(chtArea);//Add chart area to chart
        }
    }
}