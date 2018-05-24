using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Web;
using System.Web.UI.DataVisualization.Charting;

public class Charts {

    public Charts() {
    }

    public static void sendChartToClient(Chart oChart) {
        System.IO.MemoryStream PDFData = getPDF(oChart);
        // Clear response content & headers
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.ClearContent();
        HttpContext.Current.Response.ClearHeaders();
        HttpContext.Current.Response.ContentType = "application/pdf";
        HttpContext.Current.Response.Charset = string.Empty;
        HttpContext.Current.Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=Chart.pdf");

        HttpContext.Current.Response.OutputStream.Write(PDFData.GetBuffer(), 0, PDFData.GetBuffer().Length);
        HttpContext.Current.Response.OutputStream.Flush();
        HttpContext.Current.Response.OutputStream.Close();
        HttpContext.Current.Response.End();
    }

    private static MemoryStream getPDF(Chart oChart) {
        MemoryStream PDFData = new MemoryStream();
        Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
        PdfWriter PDFWriter = PdfWriter.GetInstance(document, PDFData);
        document.Open();
        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(chartImage(oChart));
        image.ScaleToFit(PageSize.A4.Height - 20, PageSize.A4.Width - 20);
        document.Add(image);
        document.Close();
        return PDFData;
    }

    private static Byte[] chartImage(Chart oChart) {
        using (var chartimage = new MemoryStream()) {
            oChart.SaveImage(chartimage, ChartImageFormat.Png);
            return chartimage.GetBuffer();
        }
    }
}