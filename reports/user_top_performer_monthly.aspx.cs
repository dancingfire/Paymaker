using System;
using System.Data;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class user_top_performer_monthly : Root {
        private ChartArea chtArea = new ChartArea("chtArea");
        protected int intRoleID = -1;
        protected string szFY = "";
        protected int startYear = 2000;
        protected int endYear = 3000;
        protected int iColor = 38;
        private Series oSeries;
        private int MaxSalesValue = 0;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            intRoleID = Valid.getInteger("szRoleID");
            
            setChart();
            bindData();
        }

        protected void bindData() {
            string szSQLFilter = getDateFilter();

            string szSQL = string.Format(@"
                
                SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, MAX(u.INitialsCode) as INitialsCode , SUM(GRAPHCOMMISSION)  AS CALCULATEDAMOUNT, DATEPART(yy, SaleDate) AS SALEYEAR, DATEPART(mm, SaleDate) AS SALEMONTH
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
                WHERE U.ID = {1} AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND U1.ISACTIVE = 1 AND U1.ISPAID = 1
                {0} 
                GROUP BY U.TOPPERFORMERREPORTSETTINGS, DATEPART(yy, SaleDate), DATEPART(mm, SaleDate) 
                having MAX(S.GROSSCOMMISSION) > 0;

                SELECT SUM(GRAPHCOMMISSION)  AS CALCULATEDAMOUNT
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
                WHERE U.ID = {1} AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND U1.ISACTIVE = 1 AND U1.ISPAID = 1
                {0}
                having MAX(S.GROSSCOMMISSION) > 0;

               ", szSQLFilter, G.User.UserID);
            DataSet ds = DB.runDataSet(szSQL);
            double Total = 0.0;
            foreach(DataRow d in ds.Tables[1].Rows) {
                Total = Convert.ToDouble(d["CALCULATEDAMOUNT"]);
            }
            chtTopPerformers.Titles[2].Text = "Total: " + Utility.formatReportMoney(Total);

            DataView dv = ds.Tables[0].DefaultView;
            dv.Sort = "SALEYEAR, SALEMONTH";

            //sort on the basis of the role, then amount
            oSeries = getSeries("Top Performers");
            foreach (DataRowView oR in dv) {
                oSeries.Points.AddXY(getMonth(DB.readInt(oR["SALEMONTH"])) +" " + Convert.ToString(oR["SALEYEAR"]), Math.Round(Convert.ToDouble(oR["CALCULATEDAMOUNT"])));
                oSeries.YValueType = ChartValueType.Double;
                if (Math.Round(Convert.ToDouble(oR["CALCULATEDAMOUNT"])) > MaxSalesValue)
                    MaxSalesValue = Convert.ToInt32(Math.Round(Convert.ToDouble(oR["CALCULATEDAMOUNT"])));
            }
            oSeries.Font = new Font("Arial narrow", 8);
            oSeries.IsValueShownAsLabel = true;
            oSeries.SmartLabelStyle.Enabled = true;
            SmartLabelStyle oS = new SmartLabelStyle();
            oS.Enabled = true;
            // oS.CalloutStyle = LabelCalloutStyle.Box;
            oS.MovingDirection = LabelAlignmentStyles.Top;
            oS.MinMovingDistance = 40;
            oSeries.SmartLabelStyle = oS;

            chtTopPerformers.Series.Add(oSeries);
            chtTopPerformers.Series[oSeries.Name]["PointWidth"] = (0.4).ToString();
            // oSeries.LabelAngle = 90;
           
            if (MaxSalesValue > 2000000)
                chtArea.AxisY.Maximum = MaxSalesValue + 150000;
            else if (MaxSalesValue > 200000)
                chtArea.AxisY.Maximum = MaxSalesValue + 60000;
            else
                chtArea.AxisY.Maximum = MaxSalesValue + 8000;
            chtArea.AxisY.Interval = Math.Round((chtArea.AxisY.Maximum / 15) / 500) * 500; //Round to nearest 500
            chtArea.AxisX.LabelAutoFitMinFontSize = 8;
            chtArea.AxisX.LabelAutoFitMaxFontSize = 8;
        }

        string getMonth(int month) {
            DateTime date = new DateTime(2020, month, 1);

            return date.ToString("MMM");
        }

        protected Series getSeries(string Value) {
            Series oSeries = new Series(Value);
            oSeries.ChartType = SeriesChartType.Column;
            return oSeries;
        }

        protected void setChart() {
            chtTopPerformers.Titles[0].Text = "Top Performer Report - " + G.UserInfo.getName(G.User.UserID);
            
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