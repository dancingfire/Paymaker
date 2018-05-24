using System.Data;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for Utility.
/// </summary>
public class Export {

    /// <summary>
    /// Exports a datafile to the browser
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="FileName"></param>
    public static void ExportToExcel(DataTable dt, string FileName) {
        if (dt.Rows.Count > 0) {
            System.IO.StringWriter tw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);
            DataGrid dgGrid = new DataGrid();
            dgGrid.DataSource = dt;
            dgGrid.DataBind();

            //Get the HTML for the control.
            dgGrid.RenderControl(hw);
            //Write the HTML back to the browser.
            //Response.ContentType = application/vnd.ms-excel;
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + FileName + "");
            HttpContext.Current.Response.Write(tw.ToString());
            HttpContext.Current.Response.End();
        } else {
            HttpContext.Current.Response.Write("There is no data available to export.");
            HttpContext.Current.Response.End();
        }
    }

    public static void ExportToExcel2(DataTable dt, string FileName) {
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.ClearContent();
        HttpContext.Current.Response.ClearHeaders();
        HttpContext.Current.Response.Buffer = true;
        HttpContext.Current.Response.ContentType = "application/ms-excel";
        HttpContext.Current.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName);

        HttpContext.Current.Response.Charset = "utf-8";
        HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
        //sets font
        HttpContext.Current.Response.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
        HttpContext.Current.Response.Write("<BR><BR><BR>");
        //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
        HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff' " +
          "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
          "style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");
        //am getting my grid's column headers
        int columnscount = dt.Columns.Count;

        for (int j = 0; j < columnscount; j++) {      //write in new column
            HttpContext.Current.Response.Write("<Td>");
            //Get column headers  and make it as bold in excel columns
            HttpContext.Current.Response.Write("<B>");
            HttpContext.Current.Response.Write(dt.Columns[j].ColumnName);
            HttpContext.Current.Response.Write("</B>");
            HttpContext.Current.Response.Write("</Td>");
        }
        HttpContext.Current.Response.Write("</TR>");
        foreach (DataRow row in dt.Rows) {//write in new row
            HttpContext.Current.Response.Write("<TR>");
            for (int i = 0; i < dt.Columns.Count; i++) {
                HttpContext.Current.Response.Write("<Td>");
                HttpContext.Current.Response.Write(row[i].ToString());
                HttpContext.Current.Response.Write("</Td>");
            }

            HttpContext.Current.Response.Write("</TR>");
        }
        HttpContext.Current.Response.Write("</Table>");
        HttpContext.Current.Response.Write("</font>");
        HttpContext.Current.Response.Flush();
        HttpContext.Current.Response.End();
    }

    public static bool exportFile(string szPath, DataSet ds) {
        return exportFile(szPath, ds, true, false, 0);
    }

    public static bool exportFile(string szPath, DataSet ds, int intHeaderRow) {
        return exportFile(szPath, ds, intHeaderRow >= 0, false, intHeaderRow);
    }

    public static bool exportFile(string szPath, DataSet ds, bool blnPrintHeader, bool blnPrintDoubleLineBreaks) {
        return exportFile(szPath, ds, blnPrintHeader, blnPrintDoubleLineBreaks, 0);
    }

    public static bool exportFile(string szPath, DataSet ds, bool blnPrintHeader, bool blnPrintDoubleLineBreaks, int intHeaderRow) {
        string szDelim = "";

        StreamWriter output;
        try {
            if (File.Exists(szPath))
                File.Delete(szPath);
            output = new StreamWriter(szPath);
        } catch (System.Exception e) {
            HttpContext.Current.Response.Write("<script>alert('An error has occured trying to write the file. The error is:" + HttpContext.Current.Server.HtmlEncode(e.Message) + "');</script>");
            return false;
        }
        using (output) {
            int intRowCount = 0;
            if (ds.Tables[0].Rows.Count == 0 && blnPrintHeader && intHeaderRow == 0) {
                foreach (DataColumn col in ds.Tables[0].Columns) {
                    output.Write(szDelim);
                    output.Write(col.ColumnName);
                    szDelim = ",";
                }
                output.WriteLine();
            }
            foreach (DataRow row in ds.Tables[0].Rows) {
                if (blnPrintHeader && intRowCount == intHeaderRow) {
                    szDelim = "";

                    foreach (DataColumn col in ds.Tables[0].Columns) {
                        output.Write(szDelim);
                        output.Write(col.ColumnName);
                        szDelim = ",";
                    }
                    output.WriteLine();
                }
                szDelim = "";
                foreach (object value in row.ItemArray) {
                    output.Write(szDelim);
                    if (value.GetType().ToString() == "String") {
                        output.Write("\"");
                        output.Write(value);
                        output.Write("\"");
                    } else {
                        output.Write(value);
                    }
                    szDelim = ",";
                }

                intRowCount++;
                output.WriteLine();
                if (blnPrintDoubleLineBreaks)
                    output.WriteLine();
            }
        }
        return true;
    }

    public static bool validatePath(string szPath) {
        if (!Directory.Exists(szPath)) {
            Directory.CreateDirectory(szPath);
        }
        return true;
    }
}