using FlexCel.Core;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class top_performer : Root {
        private ChartArea chtArea = new ChartArea("chtArea");
        private bool blnPrint = false, blnExport = false;
        protected int intRoleID = -1;
        protected string szFY = "";
        protected int startYear = 2000;
        protected int endYear = 3000;
        protected int iColor = 38;
        private Series oSeries;
        private int MaxSalesValue = 0;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        int NumberOfPeople = 0;
        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            intRoleID = Valid.getInteger("szRoleID");
            blnPrint = Valid.getBoolean("blnPrint", false);
            blnExport = Valid.getText("blnPrint", "") == "EXPORT";
            NumberOfPeople = Valid.getInteger("szNumberOfPeople", 1000);    
            setChart();
            bindData();
        }

        protected void bindData() {
            string szSQLFilter = "";
            if (intRoleID > -1) {
                szSQLFilter = string.Format(@" AND U.ROLEID IN ({0}) ", intRoleID);
            }
            szSQLFilter += getDateFilter();

            string szCompanyIDList = Valid.getText("szCompanyID", "", VT.TextNormal);
            string szCompanyFilter = "";
            if (!String.IsNullOrWhiteSpace(szCompanyIDList))
                szCompanyFilter += String.Format(" AND L_OFFICE.COMPANYID IN ({0})", szCompanyIDList);

            string szReferralSQL = "";
            if(Valid.getText("blnExcludeReferral", "") == "true") {
                //Need to determine the amount of the referral costs to apply to the proportion of the graph commission that is included in this sale
                szReferralSQL = " - (ISNULL(MAX(OTT.TOTAL), 0) * (SUM(GRAPHCOMMISSION)/MAX(S.GROSSCOMMISSION))) ";
            }
            string szSQL = string.Format(@"
                WITH CTE AS (
                    SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, SUM(GRAPHCOMMISSION) {2} AS CALCULATEDAMOUNT
                    FROM USERSALESPLIT USS
                    JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS < 1 AND USS.RECORDSTATUS < 1
                    JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID
                    JOIN SALE S ON SS.SALEID = S.ID
                    LEFT JOIN (
				        SELECT SUM(CALCULATEDAMOUNT) as TOTAL, SALEID 
				        FROM SALEEXPENSE WHERE EXPENSETYPEID IN (140, 48)
				        GROUP BY SALEID ) OTT ON OTT.SALEID = S.ID
                    JOIN DB_USER U ON USS.USERID = U.ID
                    JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                    JOIN  DB_USER U1 ON U1.ID = U.TOPPERFORMERREPORTSETTINGS
                    WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND U1.ISACTIVE = 1 AND U1.ISPAID = 1
                    {0} {1}
                GROUP BY U.TOPPERFORMERREPORTSETTINGS, S.ID
                having MAX(S.GROSSCOMMISSION) > 0
                )

                SELECT TOP {3} USERID, '' AS INITIALSCODE, 0 AS ROLEID, SUM(CALCULATEDAMOUNT) AS CALCULATEDAMOUNT
                FROM CTE
                GROUP BY USERID
                ORDER BY SUM(CALCULATEDAMOUNT) DESC
            
                SELECT USR.ID, USR.INITIALSCODE , ISNULL(T.AMOUNT, 0) AS OFFSET
                FROM DB_USER USR
                JOIN LIST L_OFFICE ON USR.OFFICEID = L_OFFICE.ID
                LEFT JOIN TPOFFSET2015 T ON T.USERID = USR.ID
                WHERE 1=1 {1} AND USR.ISPAID = 1
                ;

               ", szSQLFilter, szCompanyFilter, szReferralSQL, NumberOfPeople);
            DataSet ds = DB.runDataSet(szSQL);
            formatDataSet(ds);
            DataView dv = ds.Tables[0].DefaultView;
            dv.Sort = "CALCULATEDAMOUNT DESC, INITIALSCODE";

            if (blnExport) {
                DataTable dt = dv.ToTable();
                dt.Columns.Remove("USERID");

                Export.ExportToExcel2(dt, "TopPerformer" + DateTime.Now.Ticks + ".xls");
            }
            //sort on the basis of the role, then amount
            oSeries = getSeries("Top Performers");
            foreach (DataRowView oR in dv) {
                if (String.IsNullOrEmpty(oR["INITIALSCODE"].ToString()))
                    continue;
                //oSeries.LabelFormat =
                oSeries.Points.AddXY(oR["INITIALSCODE"].ToString(), Math.Round(Convert.ToDouble(oR["CALCULATEDAMOUNT"])));
                oSeries.YValueType = ChartValueType.Double;
                if (Math.Round(Convert.ToDouble(oR["CALCULATEDAMOUNT"])) > MaxSalesValue)
                    MaxSalesValue = Convert.ToInt32(Math.Round(Convert.ToDouble(oR["CALCULATEDAMOUNT"])));
            }
            
            oSeries.Font = new Font("Arial narrow", NumberOfPeople < 10? 12:8);
            oSeries.IsValueShownAsLabel = true;
            oSeries.SmartLabelStyle.Enabled = true;
            if(NumberOfPeople < 10)
                oSeries.LabelFormat = "{#,###}";
            else
                oSeries.LabelFormat = "{#,##,}K";
            
            
            SmartLabelStyle oS = new SmartLabelStyle();
            oS.Enabled = true;
            oS.CalloutStyle = LabelCalloutStyle.Box;
            oS.MovingDirection = LabelAlignmentStyles.Top;
            oS.MinMovingDistance = 40;
            
            oSeries.SmartLabelStyle = oS;

            chtTopPerformers.Series.Add(oSeries);
            chtTopPerformers.Series[oSeries.Name]["PointWidth"] = (0.6).ToString();
            // oSeries.LabelAngle = 90;
            foreach (DataPoint dp in chtTopPerformers.Series[0].Points) {
                dp.IsValueShownAsLabel = false;
                dp.Color = Color.FromKnownColor((KnownColor)Enum.ToObject(typeof(KnownColor), iColor++));
            }

            chtArea.AxisY.LabelStyle.Format = "{#,##}";
            chtArea.AxisY.Interval = Math.Round((chtArea.AxisY.Maximum / 15) / 500) * 500; //Round to nearest 500
            chtArea.AxisX.LabelAutoFitMinFontSize = 8;
            chtArea.AxisX.LabelAutoFitMaxFontSize = 8;
            var Max = getRoundedMaxValue(MaxSalesValue);
            if (Max > 700000)
                Max = Max += 100000;
            chtArea.AxisY.Maximum = Max;
            if (blnPrint) {
                Charts.sendChartToClient(chtTopPerformers);
            }
        }

        protected void formatDataSet(DataSet ds) {
            DataView dvUserDetails = ds.Tables[1].DefaultView;

            foreach (DataRow oR in ds.Tables[0].Rows) {
                if (System.DBNull.Value == oR["USERID"] || Convert.ToInt32(oR["USERID"]) == -1) {
                    oR.Delete();
                    continue;
                }
                dvUserDetails.RowFilter = "ID = " + oR["USERID"].ToString();
                if (dvUserDetails.Count > 0) {
                    oR["INITIALSCODE"] = dvUserDetails[0]["INITIALSCODE"].ToString();
                }
            }
            ds.Tables[0].AcceptChanges();
        }

        protected Series getSeries(string szVlaue) {
            Series oSeries = new Series(szVlaue);
            oSeries.ChartType = SeriesChartType.Column;
            return oSeries;
        }

        int getRoundedMaxValue(int MaxSalesValue) {
            int Max = MaxSalesValue;
            if (MaxSalesValue > 6000000)
                Max = MaxSalesValue + 500000;
            else if (MaxSalesValue > 2000000)
                Max = MaxSalesValue + 200000;
            else if (MaxSalesValue > 200000)
                Max = MaxSalesValue + 20000;
            else
                Max = MaxSalesValue + 8000;
            if (Max < 50000) {
                // Set max to nearest 10000 value
                Max = (int)(Math.Ceiling((double)Max / 10000) * 10000);
            } else if (Max < 1000000) {
                // Set max to nearest 100000 value
                Max = (int)(Math.Ceiling((double)Max / 100000) * 100000);
            }  else {
                // Set max to nearest 100000 value
                Max = (int)(Math.Ceiling((double)Max / 250000) * 250000);
            }
            
            return Max;

        }
        protected void setChart() {
            chtTopPerformers.Titles[0].Text = "Top Performer";
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
            //chtArea.AxisX.LabelStyle.Format = "MMM yyyy";
            chtArea.AxisY.IsStartedFromZero = true;
            chtArea.AxisX.IsStartedFromZero = false;
            chtArea.AxisY.Minimum = 0;
            chtArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chtArea.AxisY.LabelStyle.Format = "${0.00}";
            chtArea.AxisX.Interval = 1; //shows every month, year on the chart

            chtArea.AxisY.MajorGrid.Enabled = true;
            chtArea.AxisX.MajorGrid.Enabled = false;
            chtArea.AxisX.MajorGrid.LineColor = Color.Transparent;
            chtArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.NotSet;

            chtTopPerformers.ChartAreas.Add(chtArea);//Add chart area to chart
        }

        private string getDateFilter() {
            string szStartDate = Valid.getText("szStartDate", "", VT.TextNormal);
            string szEndDate = Valid.getText("szEndDate", "", VT.TextNormal);
            chtTopPerformers.Titles[1].Text = szStartDate + " - " + szEndDate;
            return string.Format(@" AND S.SALEDATE BETWEEN '{0} 00:00' and '{1} 23:59:59'", szStartDate, szEndDate);
        }
    }
}