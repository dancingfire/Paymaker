using FlexCel.Core;
using FlexCel.XlsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Paymaker {

    public partial class budget_report : Root {
        private string TemplateFile = "~/templates/Manual Time Sheet.xlsx";

        private XlsFile oFile = null;

        private int intUserID = -1;
        private int intCycleID = -1;

        protected void Page_Load(object sender, System.EventArgs e) {
            blnShowMenu = false;
            intUserID = Valid.getInteger("UserID", -1);
            intCycleID = Valid.getInteger("CycleID", -1);

            oFile = new XlsFile(false);
            oFile.Open(Server.MapPath(TemplateFile));
            prefillDetails();
            updateTemplate();
        }

        private void prefillDetails() {
            UserDetail oUser = G.UserInfo.getUser(intUserID);
            oFile.SetCellValue(8, 4, oUser.FirstName + ' ' + oUser.LastName);

            string szSQL = string.Format(@"SELECT CYCLETYPEID, STARTDATE, ENDDATE FROM TIMESHEETCYCLE WHERE ID = {0}", intCycleID);
            using(DataSet ds = DB.runDataSet(szSQL)) {
                foreach(DataRow dr in ds.Tables[0].Rows) {
                    int intCycleTypeID = DB.readInt(dr["CYCLETYPEID"]);
                    DateTime dtStart = DB.readDate(dr["STARTDATE"]);
                    DateTime dtEnd = DB.readDate(dr["ENDDATE"]);
                    for(int c=0; c<14; c++) {
                        oFile.SetCellValue(12 + c, 1, dtStart.AddDays(c).ToString("dddd"));
                        oFile.SetCellValue(12 + c, 2, dtStart.AddDays(c).ToString("dd-MMM-yy"));
                    }
                }
            }
        }

        private void updateTemplate() {
            oFile.ActiveSheet = 1;
            MemoryStream oM = new MemoryStream();
            oFile.Save(oM, TFileFormats.Xls);

            byte[] bytes = oM.GetBuffer();
            Response.Buffer = true;
            Response.Clear();
            Response.ContentType = "application/excel";
            Response.AddHeader("content-disposition", "attachment; filename=ManualTimesheet.xls");
            Response.BinaryWrite(bytes);
            Response.Flush();
        }
    }
}
