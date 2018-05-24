using System;
using System.Data;
using System.Data.OleDb;

public partial class import_data : System.Web.UI.Page {
    private DataSet ds = new DataSet();

    protected void Page_Load(object sender, EventArgs e) {
        string szFilePath = Server.MapPath("saleshistory.xls");
        string strExcelConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=NO;MAXSCANROWS=16;IMEX=1'", szFilePath);
        //You must use the $ after the object you reference in the spreadsheet
        OleDbDataAdapter oCmd = new OleDbDataAdapter("SELECT * FROM [# Results$]", strExcelConn);

        oCmd.Fill(ds, "ExcelInfo");
        importSaleCount();
        oCmd = new OleDbDataAdapter("SELECT * FROM [$ Results$]", strExcelConn);
        ds = new DataSet();
        oCmd.Fill(ds, "ExcelInfo");
        importSaleAmount();
    }

    private void importSaleCount() {
        int intCol = 0;
        DB.runNonQuery("truncate table  salehistory");
        foreach (DataColumn dc in ds.Tables[0].Columns) {
            string szDate = Convert.ToString(ds.Tables[0].Rows[0][intCol]);
            if (!Utility.isDateTime(szDate)) {
                intCol++;
                continue;
            }
            szDate = Utility.formatDate(Convert.ToDateTime(szDate));

            int intValue = 0;
            string szValue = Convert.ToString(ds.Tables[0].Rows[1][intCol]);
            if (Utility.IsNumeric(szValue))
                intValue = Convert.ToInt32(szValue);
            addSaleCount(szDate, 21, intValue); //Balwyn

            intValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[2][intCol]);
            if (Utility.IsNumeric(szValue))
                intValue = Convert.ToInt32(szValue);
            addSaleCount(szDate, 22, intValue); //Cant

            intValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[3][intCol]);
            if (Utility.IsNumeric(szValue))
                intValue = Convert.ToInt32(szValue);
            addSaleCount(szDate, 23, intValue); //Manningham

            intValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[4][intCol]);
            if (Utility.IsNumeric(szValue))
                intValue = Convert.ToInt32(szValue);
            addSaleCount(szDate, 25, intValue); //Hawth

            intValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[5][intCol]);
            if (Utility.IsNumeric(szValue))
                intValue = Convert.ToInt32(szValue);
            addSaleCount(szDate, 27, intValue); //Queens
            intValue = 0;

            szValue = Convert.ToString(ds.Tables[0].Rows[6][intCol]);
            if (Utility.IsNumeric(szValue))
                intValue = Convert.ToInt32(szValue);
            addSaleCount(szDate, 24, intValue); //Gee

            intCol++;
        }
    }

    private void importSaleAmount() {
        int intCol = 0;

        foreach (DataColumn dc in ds.Tables[0].Columns) {
            string szDate = Convert.ToString(ds.Tables[0].Rows[0][intCol]);
            Response.Write(szDate + " <br/>");
            if (!Utility.isDateTime(szDate)) {
                intCol++;
                continue;
            }
            szDate = Utility.formatDate(Convert.ToDateTime(szDate));

            double dValue = 0;
            string szValue = Convert.ToString(ds.Tables[0].Rows[1][intCol]);
            szValue = szValue.Replace("$", "").Replace(",", "");
            if (Utility.IsNumeric(szValue))
                dValue = Convert.ToDouble(szValue);
            updateSaleValue(szDate, 21, dValue); //Balwyn

            dValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[2][intCol]);
            szValue = szValue.Replace("$", "").Replace(",", "");
            if (Utility.IsNumeric(szValue))
                dValue = Convert.ToDouble(szValue);
            updateSaleValue(szDate, 22, dValue); //Cant

            dValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[3][intCol]);
            szValue = szValue.Replace("$", "").Replace(",", "");
            if (Utility.IsNumeric(szValue))
                dValue = Convert.ToDouble(szValue);
            updateSaleValue(szDate, 23, dValue); //Mann

            dValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[4][intCol]);
            szValue = szValue.Replace("$", "").Replace(",", "");
            if (Utility.IsNumeric(szValue))
                dValue = Convert.ToDouble(szValue);
            updateSaleValue(szDate, 25, dValue); //Hawth

            dValue = 0;
            szValue = Convert.ToString(ds.Tables[0].Rows[5][intCol]);
            szValue = szValue.Replace("$", "").Replace(",", "");
            if (Utility.IsNumeric(szValue))
                dValue = Convert.ToDouble(szValue);
            updateSaleValue(szDate, 27, dValue); //Queens
            dValue = 0;

            szValue = Convert.ToString(ds.Tables[0].Rows[6][intCol]);
            szValue = szValue.Replace("$", "").Replace(",", "");
            if (Utility.IsNumeric(szValue))
                dValue = Convert.ToDouble(szValue);
            updateSaleValue(szDate, 24, dValue); //Gee

            intCol++;
        }
    }

    private void addSaleCount(string Date, int Office, int Count) {
        string szSQL = String.Format(@"
                insert into salehistory(monthdate, officeid, salecount)
                values('{0}', {1}, {2});", Date, Office, Count);
        DB.runNonQuery(szSQL);
    }

    private void updateSaleValue(string Date, int Office, double Value) {
        int intID = DB.getScalar(String.Format(@"SELECT ID FROM salehistory where monthdate = '{0}' and officeid = {1}", Date, Office), -1);
        if (intID == -1) {
            string szSQL = String.Format(@"
                insert into salehistory(monthdate, officeid, salecount, salevalue)
                values('{0}', {1}, 0, {2});", Date, Office, Value);
            Response.Write(szSQL + "<br/>");
            DB.runNonQuery(szSQL);
        } else {
            string szSQL = String.Format("UPDATE SALEHISTORY SET SALEVALUE = {0} WHERE ID = {1}", Value, intID);
            Response.Write(szSQL + "<br/>");

            DB.runNonQuery(szSQL);
        }
    }
}