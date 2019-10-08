using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Paymaker {

    public partial class monthly_sales_by_pay_period : Root {
        private bool blnPrint = false;
        private DateTime dtStart = DateTime.MaxValue;
        private DateTime dtEnd = DateTime.MaxValue;
        private bool blnIsMultiMonth = false;
        private GridViewHelper helper = null;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szPayPeriod = hdPayPeriod.Value = Request.QueryString["szPayPeriod"].ToString();
            hdOfficeID.Value = Valid.getText("szOfficeID", "", VT.TextNormal);
            hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);
            string szOfficeFilterNames = Valid.getText("szOfficeNames", "", VT.TextNormal);
            string szCompanyFilterNames = Valid.getText("szCompanyNames", "", VT.TextNormal);
            string szStartDate = Valid.getText("szStartDate", "", VT.TextNormal);
            string szEndDate = Valid.getText("szEndDate", "", VT.TextNormal);
            if (!String.IsNullOrEmpty(szStartDate)) {
                blnIsMultiMonth = true;
                dtStart = DateTime.Parse(szStartDate);
                if (String.IsNullOrEmpty(szEndDate))
                    dtEnd = DateTime.Now;
                else
                    dtEnd = DateTime.Parse(szEndDate);
            } else {
                PayPeriod oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(hdPayPeriod.Value));
                if (oP != null) {
                    dtStart = oP.StartDate;
                    dtEnd = oP.EndDate;
                }
            }
            blnPrint = Valid.getBoolean("blnPrint", false);
            if (hdOfficeID.Value == "null")
                hdOfficeID.Value = "";
            if (hdCompanyID.Value == "null")
                hdCompanyID.Value = "";

            pPageHeader.InnerHtml = pPageHeader.InnerHtml + " between " + Utility.formatDate(dtStart) + " - " + Utility.formatDate(dtEnd);
            if (szOfficeFilterNames != "") {
                pPageHeader.InnerHtml += "<br/><span><strong>Office: </strong>" + szOfficeFilterNames + "</span>";
            }
            if (szCompanyFilterNames != "") {
                pPageHeader.InnerHtml += "<br/><span><strong>Company: </strong>" + szCompanyFilterNames + "</span>";
            }
            bindData();
            if (blnPrint) {
                sbEndJS.Append("printReport();");
            }
        }

        protected string getSettlementDate(string szSettlementDate) {
            if (String.IsNullOrWhiteSpace(szSettlementDate) || Convert.ToDateTime(szSettlementDate) == DateTime.MinValue) {
                return "";
            }
            return Utility.formatDate(szSettlementDate);
        }

        protected void bindData() {
            string szFilter = String.Format(" P.STARTDATE BETWEEN '{0} 00:00:00' AND '{1} 23:59:59'", Utility.formatDate(dtStart), Utility.formatDate(dtEnd));
            if (!String.IsNullOrWhiteSpace(hdOfficeID.Value))
                szFilter += String.Format(" AND L_OFFICE.ID IN ({0})", hdOfficeID.Value);
            if (!String.IsNullOrWhiteSpace(hdCompanyID.Value))
                szFilter += String.Format(" AND L_COMPANY.ID IN ({0})", hdCompanyID.Value);
            string szUserIDFilter = Valid.getText("szUserID", "", VT.List);
            if (!G.User.IsAdmin) //Filter for single user mode
                szFilter += " AND USS.USERID IN (" + G.User.UserID + ") ";
            else if (!String.IsNullOrEmpty(szUserIDFilter)) {
                szFilter += " AND USS.USERID IN (" + szUserIDFilter + ")";
            }
            bool blnIncludeInactive = Valid.getBoolean("blnIncludeInactive", false);
            string szUserActive = " AND USR.ISACTIVE = 1 ";
            if (blnIncludeInactive)
                szUserActive = "";

            string szSQL = string.Format(@"
                SELECT S.ID, S.ADDRESS, CAST(CAST(DATEPART(YEAR,  P.STARTDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH,  P.STARTDATE) AS VARCHAR), 2) AS INT) AS GROUPING,
                S.SALEDATE, S.SALEPRICE, S.SETTLEMENTDATE, S.GROSSCOMMISSION, S.CONJUNCTIONALCOMMISSION,
                USS.GRAPHCOMMISSION AS CALCULATEDAMOUNT,
                L_OFFICE.NAME, L_SALESPLIT.NAME AS COMMISSIONNAME, SS.COMMISSIONTYPEID,
                USR.INITIALSCODE
                FROM SALE S
                JOIN SALESPLIT SS ON SS.SALEID = S.ID AND S.STATUSID IN (1, 2) AND SS.RECORDSTATUS < 1 AND SS.COMMISSIONTYPEID NOT in (9, 89) --Ignore Service Area and mentoring
                JOIN USERSALESPLIT USS ON USS.SALESPLITID = SS.ID AND USS.RECORDSTATUS < 1
                JOIN DB_USER USR ON USR.ID = USS.USERID AND USR.ID > 0
                JOIN LIST L_OFFICE ON L_OFFICE.ID = USS.OFFICEID AND L_OFFICE.ISACTIVE = 1
                JOIN LIST L_COMPANY ON L_COMPANY.ID = L_OFFICE.COMPANYID
                JOIN LIST L_SALESPLIT ON L_SALESPLIT.ID = SS.COMMISSIONTYPEID --AND L_SALESPLIT.EXCLUDEONREPORT = 0
                JOIN PAYPERIOD P ON S.PAYPERIODID = P.ID AND S.STATUSID IN (1,2)
                WHERE {0} AND SS.CALCULATEDAMOUNT > 0 {1}
                ORDER BY CAST(CAST(DATEPART(YEAR, P.STARTDATE) AS VARCHAR) + RIGHT('0' + CAST(DATEPART(MONTH,  P.STARTDATE) AS VARCHAR), 2) AS INT), SUBSTRING(S.ADDRESS, CHARINDEX(' ', S.ADDRESS) + 1, LEN(S.ADDRESS))"
                , szFilter, szUserActive);

            DataSet ds = DB.runDataSet(szSQL);
            formatDataSet(ds);
        }

        protected void formatDataSet(DataSet ds) {
            int currSaleID = -1;
            DataSet dsClone = ds.Clone();
            dsClone.Tables[0].Columns.Remove("SALEDATE");
            dsClone.Tables[0].Columns.Remove("SETTLEMENTDATE");
            dsClone.Tables[0].Columns.Remove("SALEPRICE");
            dsClone.Tables[0].Columns.Remove("GROSSCOMMISSION");
            dsClone.Tables[0].Columns.Remove("CONJUNCTIONALCOMMISSION");

            dsClone.Tables[0].Columns.Add(new DataColumn("SALEDATE", System.Type.GetType("System.String")));
            dsClone.Tables[0].Columns.Add(new DataColumn("SETTLEMENTDATE", System.Type.GetType("System.String")));
            dsClone.Tables[0].Columns.Add(new DataColumn("OFFICECOMMISSION", System.Type.GetType("System.Double")));
            dsClone.Tables[0].Columns.Add(new DataColumn("SALEPRICE", System.Type.GetType("System.Double")));
            dsClone.Tables[0].Columns.Add(new DataColumn("GROSSCOMMISSION", System.Type.GetType("System.Double")));
            dsClone.Tables[0].Columns.Add(new DataColumn("CONJUNCTIONALCOMMISSION", System.Type.GetType("System.Double")));
            dsClone.Tables[0].Columns.Add(new DataColumn("AGENTCOMMISSION", System.Type.GetType("System.Double")));

            dsClone.Tables[0].Columns.Add(new DataColumn("Lead", System.Type.GetType("System.String")));
            dsClone.Tables[0].Columns.Add(new DataColumn("Lister", System.Type.GetType("System.String")));
            dsClone.Tables[0].Columns.Add(new DataColumn("Manager", System.Type.GetType("System.String")));
            dsClone.Tables[0].Columns.Add(new DataColumn("Seller", System.Type.GetType("System.String")));
            dsClone.Tables[0].Columns.Add(new DataColumn("Service Area", System.Type.GetType("System.String")));

            DataRow oCloneRow = null;
            DataRow oTotalRow = dsClone.Tables[0].NewRow();
            oTotalRow["SALEPRICE"] = 0;
            oTotalRow["GROSSCOMMISSION"] = 0;
            oTotalRow["CONJUNCTIONALCOMMISSION"] = 0;
            oTotalRow["OFFICECOMMISSION"] = 0;
            oTotalRow["AGENTCOMMISSION"] = 0;
            int intSaleCount = 0;

            foreach (DataRow oR in ds.Tables[0].Rows) {
                if (currSaleID == -1 || currSaleID != Convert.ToInt32(oR["ID"])) {
                    currSaleID = Convert.ToInt32(oR["ID"]);
                    if (oCloneRow != null) {
                        oCloneRow["OFFICECOMMISSION"] = Utility.formatMoneyShort(readDouble(oCloneRow["OFFICECOMMISSION"]));
                        dsClone.Tables[0].Rows.Add(oCloneRow);
                        intSaleCount++;
                    }
                    oCloneRow = dsClone.Tables[0].NewRow();
                    oCloneRow["AGENTCOMMISSION"] = 0;
                    oCloneRow["SALEPRICE"] = 0;
                    oCloneRow["OFFICECOMMISSION"] = 0;
                    oCloneRow["GROUPING"] = oR["GROUPING"];
                    oCloneRow["ID"] = oR["ID"];
                    string szAddress = oR["Address"].ToString();
                    szAddress = Regex.Replace(szAddress, @",[\s]*Vic[\s]*,[\s]*[\d]{4}", "", RegexOptions.IgnoreCase);
                    oCloneRow["Address"] = szAddress;
                    oCloneRow["SALEDATE"] = Utility.formatDate(oR["SALEDATE"].ToString());
                    oCloneRow["SETTLEMENTDATE"] = Utility.formatDate(oR["SETTLEMENTDATE"].ToString());

                    oCloneRow["SALEPRICE"] = Utility.formatMoneyShort(readDouble(oR["SALEPRICE"]));
                    oTotalRow["SALEPRICE"] = readDouble(oTotalRow["SALEPRICE"]) + readDouble(oR["SALEPRICE"]);

                    oCloneRow["GROSSCOMMISSION"] = Utility.formatMoneyShort(readDouble(oR["GROSSCOMMISSION"]));
                    oTotalRow["GROSSCOMMISSION"] = readDouble(oTotalRow["GROSSCOMMISSION"]) + readDouble(oR["GROSSCOMMISSION"]);

                    oCloneRow["CONJUNCTIONALCOMMISSION"] = Utility.formatMoneyShort(readDouble(oR["CONJUNCTIONALCOMMISSION"]));
                    oTotalRow["CONJUNCTIONALCOMMISSION"] = readDouble(oTotalRow["CONJUNCTIONALCOMMISSION"]) + readDouble(oR["CONJUNCTIONALCOMMISSION"]);
                }
                oCloneRow["OFFICECOMMISSION"] = readDouble(oCloneRow["OFFICECOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);
                oTotalRow["OFFICECOMMISSION"] = readDouble(oTotalRow["OFFICECOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);

                oCloneRow["AGENTCOMMISSION"] = readDouble(oCloneRow["AGENTCOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);
                oTotalRow["AGENTCOMMISSION"] = readDouble(oTotalRow["AGENTCOMMISSION"]) + readDouble(oR["CALCULATEDAMOUNT"]);

                switch (oR["COMMISSIONNAME"].ToString()) {
                    case "Lead":
                        oCloneRow["Lead"] = Utility.Append(oCloneRow["Lead"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                        break;

                    case "Lister":
                        oCloneRow["Lister"] = Utility.Append(oCloneRow["Lister"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                        break;

                    case "Manager":
                        oCloneRow["Manager"] = Utility.Append(oCloneRow["Manager"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                        break;

                    case "Seller":
                        oCloneRow["Seller"] = Utility.Append(oCloneRow["Seller"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                        break;

                    case "Service Area":
                        oCloneRow["Service Area"] = Utility.Append(oCloneRow["Service Area"].ToString(), oR["INITIALSCODE"].ToString(), Environment.NewLine);
                        break;
                }
            }
            if (oCloneRow != null) {
                oCloneRow["OFFICECOMMISSION"] = Utility.formatMoneyShort(readDouble(oCloneRow["OFFICECOMMISSION"]));
                dsClone.Tables[0].Rows.Add(oCloneRow);
                if (!blnIsMultiMonth) {
                    oTotalRow["SALEPRICE"] = Utility.formatMoneyShort(readDouble(oTotalRow["SALEPRICE"]));
                    oTotalRow["GROSSCOMMISSION"] = Utility.formatMoneyShort(readDouble(oTotalRow["GROSSCOMMISSION"]));
                    oTotalRow["CONJUNCTIONALCOMMISSION"] = Utility.formatMoneyShort(readDouble(oTotalRow["CONJUNCTIONALCOMMISSION"]));
                    oTotalRow["OFFICECOMMISSION"] = Utility.formatMoneyShort(readDouble(oTotalRow["OFFICECOMMISSION"]));
                    dsClone.Tables[0].Rows.Add(oTotalRow);
                }
                intSaleCount++;
            }
            if (blnIsMultiMonth) {
                helper = new GridViewHelper(gvTable, true);
                helper.RegisterGroup("Grouping", true, true);
                helper.GroupHeader += new GroupEvent(helper_GroupHeader);
                helper.RegisterSummary("CONJUNCTIONALCOMMISSION", SummaryOperation.Sum, "Grouping");
                helper.RegisterSummary("CONJUNCTIONALCOMMISSION", SummaryOperation.Sum);
                helper.RegisterSummary("GROSSCOMMISSION", SummaryOperation.Sum, "Grouping");
                helper.RegisterSummary("GROSSCOMMISSION", SummaryOperation.Sum);
                helper.RegisterSummary("SALEPRICE", SummaryOperation.Sum, "Grouping");
                helper.RegisterSummary("SALEPRICE", SummaryOperation.Sum);
                helper.RegisterSummary("OFFICECOMMISSION", SummaryOperation.Sum, "Grouping");
                helper.RegisterSummary("OFFICECOMMISSION", SummaryOperation.Sum);
                helper.GeneralSummary += new FooterEvent(helper_GeneralSummary);
            }

            gvTable.DataSource = dsClone;
            gvTable.DataBind();
            //HTML.formatGridView(ref gvTable);
            lTotal.Text = "Totals sales for the period: " + intSaleCount;
        }

        private double readDouble(object o) {
            if (o == null || o == System.DBNull.Value)
                return 0;
            else
                return Convert.ToDouble(o);
        }

        private void helper_GroupHeader(string groupName, object[] values, GridViewRow row) {
            row.BackColor = Color.FromArgb(230, 213, 172);
            row.Font.Bold = true;

            if (String.IsNullOrEmpty(row.Cells[0].Text)) {
                row.Cells[0].Text = "Grand total";
            } else if (groupName == "Grouping") {
                row.Cells[0].Text = "&nbsp;&nbsp;" + getFriendlyDate(row.Cells[0].Text);
            }
        }

        private void helper_GeneralSummary(GridViewRow row) {
            row.BackColor = Color.FromArgb(230, 213, 172);
            row.Font.Bold = true;
            foreach (TableCell c in row.Cells)
                c.HorizontalAlign = HorizontalAlign.Right;
        }

        private string getFriendlyDate(string szDate) {
            if (szDate.Length < 6)
                return szDate;
            string szYear = szDate.Substring(0, 4);
            string szMonth = szDate.Substring(szDate.Length - 2, 2);
            return new DateTime(2011, Convert.ToInt32(szMonth), 1).ToString("MMM") + "-" + szYear;
        }

        protected void gvTable_Sorting(object sender, GridViewSortEventArgs e) {
        }
    }
}