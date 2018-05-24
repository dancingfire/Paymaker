using FlexCel.Core;
using FlexCel.XlsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Paymaker {

    public partial class budget_report : Root {

        //Office Budget Template - Work In Progress
        //string TemplateFile = "~/templates/Office Budget Template.xlsx";
        private string TemplateFile = "~/templates/Office Budget Template - Work In Progress.xlsx";

        private XlsFile oFile = null;
        private List<SheetAgent> lAgents = new List<SheetAgent>();
        private string szStartDate = "";
        private string szEndDate = "";
        private const int cStartMonthDataCol = 8;
        private Hashtable htSheetMonthInfo = new Hashtable();

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            string szFY = Valid.getText("szFY", "");

            if (szFY != "") {
                szStartDate = String.Format(@"Jun 30, {0} 0:00", Convert.ToInt32(szFY) - 1);
                szEndDate = String.Format(@"July 01, {0} 23:59", Convert.ToInt32(szFY));
            } else {
                szStartDate = Utility.formatDate(Valid.getDate("szStartDate")) + " 00:00";
                szEndDate = Utility.formatDate(Valid.getDate("szEndDate")) + " 23:59";
            }
            oFile = new XlsFile(false);
            oFile.Open(Server.MapPath(TemplateFile));
            loadAgentsOnSheet();
            loadSummaryStartPerWorksheet();
            updateTemplate();
        }

        private void loadSummaryStartPerWorksheet() {
            for (int i = 2; i <= oFile.SheetCount; i++) {
                oFile.ActiveSheet = i;
                if (oFile.SheetName.StartsWith("Agent"))
                    break;

                int intRow = 4;
                while (true) {
                    object szMonth = oFile.GetCellValue(intRow, 1);
                    if (Convert.ToString(szMonth) == "JUL") {
                        htSheetMonthInfo[i] = intRow;
                        break;
                    }
                    intRow++;
                }
            }
        }

        private void loadAgentsOnSheet() {
            for (int i = 2; i <= oFile.SheetCount; i++) {
                oFile.ActiveSheet = i;
                int intRow = 4;
                while (true) {
                    object szInitial = oFile.GetCellValue(intRow, 1);
                    if (szInitial == null)
                        break;
                    lAgents.Add(new SheetAgent(Convert.ToString(szInitial), i, intRow));
                    intRow++;
                }
            }
        }

        private void updateTemplate() {
            using (DataSet ds = bindData()) {
                //Set the monthly agent totals
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    string szInitials = DB.readString(dr["AGENT"]);
                    SheetAgent oA = lAgents.Find(o => o.Initials == szInitials);
                    if (oA == null)
                        continue;
                    oFile.ActiveSheet = oA.Sheet;
                    int intMonth = DB.readInt(dr["MONTH"]);
                    //Set the monthly value
                    oFile.SetCellValue(oA.SheetRow, getCol(intMonth), DB.readDouble(dr["AMOUNT"]));
                }

                //Set the previous year agent commissions
                foreach (DataRow dr in ds.Tables[1].Rows) {
                    string szInitials = DB.readString(dr["AGENT"]);
                    SheetAgent oA = lAgents.Find(o => o.Initials == szInitials);
                    if (oA == null)
                        continue;
                    oFile.ActiveSheet = oA.Sheet;
                    oFile.SetCellValue(oA.SheetRow, 4, DB.readDouble(dr["AMOUNT"]));
                }

                //Set the summary values for current year
                foreach (DataRow dr in ds.Tables[2].Rows) {
                    string szOffice = DB.readString(dr["OFFICE"]);
                    int intSheetNumber = getSheetNumberByOfficeName(szOffice);
                    if (intSheetNumber == -1)
                        continue; //We don't have a match on office
                    oFile.ActiveSheet = intSheetNumber;

                    //Set the total value
                    int intMonth = DB.readInt(dr["MONTH"]);
                    int intMonthRow = getSummaryRowPosition(intMonth);
                    double dTotal = DB.readDouble(dr["AMOUNT"]);

                    oFile.SetCellValue(intMonthRow, 4, dTotal);
                    oFile.SetCellValue(intMonthRow, 5, DB.readInt(dr["SALECOUNT"]));
                }

                //Set the previous yearssummary values
                foreach (DataRow dr in ds.Tables[3].Rows) {
                    string szOffice = DB.readString(dr["OFFICE"]);
                    int intSheetNumber = getSheetNumberByOfficeName(szOffice);
                    if (intSheetNumber == -1)
                        continue; //We don't have a match on office
                    oFile.ActiveSheet = intSheetNumber;

                    //Set the total value
                    int intMonth = DB.readInt(dr["MONTH"]);
                    int intMonthRow = getSummaryRowPosition(intMonth);
                    double dTotal = DB.readDouble(dr["AMOUNT"]);

                    oFile.SetCellValue(intMonthRow, 2, dTotal);
                    oFile.SetCellValue(intMonthRow, 3, DB.readInt(dr["SALECOUNT"]));
                }
            }
            //Clean up the spreadsheet by hiding columns
            /*for (int SheetIndex = 2; SheetIndex <= oFile.SheetCount; SheetIndex++) {
                oFile.ActiveSheet = SheetIndex;
                if (intFocusMonth > -1) {
                    for (int CurrMonth = 1; CurrMonth <= 12; CurrMonth++) {
                        if (CurrMonth != intFocusMonth) {
                            hideRelatedCols(CurrMonth);
                        }
                    }
                }
                oFile.ScrollWindow(1, 1);
            }*/
            oFile.ActiveSheet = 1;
            MemoryStream oM = new MemoryStream();
            oFile.Save(oM, TFileFormats.Xls);

            byte[] bytes = oM.GetBuffer();
            Response.Buffer = true;
            Response.Clear();
            Response.ContentType = "application/excel";
            Response.AddHeader("content-disposition", "attachment; filename=BudgetReport.xls");
            Response.BinaryWrite(bytes);
            Response.Flush();
        }

        private int getSheetNumberByOfficeName(string Office) {
            if (Office.Contains("Balwyn"))
                return 2;
            else if (Office.Contains("Canterbury"))
                return 3;
            else if (Office.Contains("Manningham"))
                return 4;
            else if (Office.Contains("Maroondah"))
                return 5;
            else if (Office.Contains("Whitehorse") || Office.Contains("Blackburn"))
                return 6;
            else if (Office.Contains("Glen Iris"))
                return 7;
            else if (Office.Contains("Glen Waverly"))
                return 8;
            else if (Office.Contains("Hawthorn"))
                return 9;
            return -1;
        }

        private void hideRelatedCols(int ColNumber) {
            oFile.SetColHidden(getCol(ColNumber), true);
            oFile.SetColHidden(getCol(ColNumber) + 1, true);
            oFile.SetColHidden(getCol(ColNumber) + 2, true);
            oFile.SetColHidden(getCol(ColNumber) + 3, true);
            oFile.SetColHidden(getCol(ColNumber) + 4, true);
        }

        private int getSummaryRowPosition(int Month) {
            int intStart = Convert.ToInt32(htSheetMonthInfo[oFile.ActiveSheet]);

            //int intStart = 15;
            if (Month > 6)
                return intStart + ((Month - 7));
            else
                return intStart + ((Month + 5));
        }

        private int getCol(int Month) {
            if (Month > 6)
                return cStartMonthDataCol + ((Month - 7) * 5);
            else
                return cStartMonthDataCol + ((Month + 5) * 5);
        }

        protected DataSet bindData() {
            DateTime dtPrevYearStart = DateUtil.LastFinYear(Convert.ToDateTime(szStartDate)).Start;
            DateTime dtPrevYearEnd = DateUtil.LastFinYear(Convert.ToDateTime(szStartDate)).End;

            string szSQL = string.Format(@"
                -- 0) Commission by agent by month for current period
                SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, U.INITIALSCODE AS AGENT, SUM(GRAPHCOMMISSION) AS AMOUNT, MONTH(S.SALEDATE) as MONTH, MAX(L_OFFICE.NAME) AS OFFICE,
                    COUNT(DISTINCT S.ID) AS SALECOUNT
                FROM USERSALESPLIT USS
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0
                JOIN SALE S ON SS.SALEID = S.ID
                JOIN DB_USER U ON USS.USERID = U.ID
                JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0 AND {0}
                GROUP BY TOPPERFORMERREPORTSETTINGS, U.INITIALSCODE, MONTH(S.SALEDATE)
                ORDER BY MONTH(S.SALEDATE), U.INITIALSCODE;

                -- 1) Commission by agent for previous year
                SELECT U.TOPPERFORMERREPORTSETTINGS AS USERID, U.INITIALSCODE AS AGENT, SUM(GRAPHCOMMISSION) AS AMOUNT, MAX(L_OFFICE.NAME) AS OFFICE,
                    COUNT(DISTINCT S.ID) AS SALECOUNT
                FROM USERSALESPLIT USS
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0
                JOIN SALE S ON SS.SALEID = S.ID
                JOIN DB_USER U ON USS.USERID = U.ID
                JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0  AND S.SALEDATE BETWEEN '{1} 00:00' AND  '{2} 23:59' --Last year's data
                GROUP BY TOPPERFORMERREPORTSETTINGS, U.INITIALSCODE
                ORDER BY U.INITIALSCODE;

                -- Commission and sale count by office
                SELECT  SUM(GRAPHCOMMISSION) AS AMOUNT, MONTH(S.SALEDATE) as MONTH, L_OFFICE.NAME AS OFFICE,
                    COUNT(DISTINCT S.ID) AS SALECOUNT
                FROM USERSALESPLIT USS
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0
                JOIN SALE S ON SS.SALEID = S.ID
                JOIN DB_USER U ON USS.USERID = U.ID
                JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0   AND {0}
                GROUP BY L_OFFICE.NAME, MONTH(S.SALEDATE)
                ORDER BY L_OFFICE.NAME, MONTH(S.SALEDATE);

                SELECT  SUM(GRAPHCOMMISSION) AS AMOUNT, MONTH(S.SALEDATE) as MONTH, L_OFFICE.NAME AS OFFICE,
                    COUNT(DISTINCT S.ID) AS SALECOUNT
                FROM USERSALESPLIT USS
                JOIN SALESPLIT SS ON USS.SALESPLITID = SS.ID AND SS.RECORDSTATUS = 0 AND USS.RECORDSTATUS < 1
                JOIN LIST L_SPLITTYPE ON SS.COMMISSIONTYPEID = L_SPLITTYPE.ID  AND L_SPLITTYPE.EXCLUDEONREPORT = 0
                JOIN SALE S ON SS.SALEID = S.ID
                JOIN DB_USER U ON USS.USERID = U.ID
                JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
                WHERE U.ID > 0 AND S.STATUSID IN (1, 2) AND SS.CALCULATEDAMOUNT > 0   AND S.SALEDATE BETWEEN '{1} 00:00' AND  '{2} 23:59' --Last year's data
                GROUP BY L_OFFICE.NAME, MONTH(S.SALEDATE)
                ORDER BY L_OFFICE.NAME, MONTH(S.SALEDATE)
                "
                , getDateFilter(), Utility.formatDate(dtPrevYearStart), Utility.formatDate(dtPrevYearEnd));

            return DB.runDataSet(szSQL);
        }

        private string getDateFilter() {
            return string.Format(@" S.SALEDATE BETWEEN '{0}' AND  '{1}'", szStartDate, szEndDate);
        }
    }
}

/// <summary>
/// The details we need to store this agent record
/// </summary>
public class SheetAgent {
    public string Initials { get; set; }
    public int Sheet { get; set; }
    public int SheetRow { get; set; }

    public SheetAgent(string Initials, int Sheet, int SheetRow) {
        this.Initials = Initials;
        this.Sheet = Sheet;
        this.SheetRow = SheetRow;
    }
}