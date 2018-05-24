using System;
using System.Data;
using System.IO;
using System.Web;

public partial class myob_export : Root {
    protected int intItemID = -1;
    private int intMYOBExportID = 0;

    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        hdItemID.Value = intItemID.ToString();
        if (!IsPostBack) {
            previewInvoices();
            string szSQL = "SELECT COUNT(*) FROM PRODUCT WHERE CREDITGLCODE <= 0";
            int intCount = DB.getScalar(szSQL, 0);
            if (intCount > 0) {
                string szErrorMessage = "There are " + intCount + " products missing a GL code. These will have to be updated before you can export the data. Click <a href='../admin/product_detail.aspx?NOTSET=true'>here</a> to view the product list";
                PageNotificationManager.PageNotificationInfo.addPageNotification(PageNotificationType.Warning, "Products missing GL Codes", szErrorMessage);
                btnUpdate.Visible = false;
            }
        }
        PageNotificationManager.PageNotificationInfo.showPageNotification();
    }

    private void previewInvoices() {
        gvPreview.DataSource = getData(false, "", -1, false);
        gvPreview.DataBind();
        HTML.formatGridView(ref gvPreview, true);
        gvPreview.Visible = true;
        if (gvPreview.Rows.Count == 0) {
            btnUpdate.Visible = false;
        }
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        //Load all Company
        string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);

        DataSet dsCompany = DB.runDataSet(szSQL);
        foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
            string szFileName = "CampaignTrack_INVOICE_" + drCompany["NAME"].ToString() + DateTime.Now.ToString("_yyyyMMdd-HHmm") + ".txt";
            DataSet dsData = getData(true, szFileName, Convert.ToInt32(drCompany["ID"]));
            if (dsData.Tables[0].Rows.Count > 0) {
                exportFile(szFileName, dsData.Tables[0]);
                addLineSplits(szFileName);
            }
        }
    }

    private void addLineSplits(string FileName) {
        FileName = G.Settings.MYOBDir + FileName;
        string[] lines = System.IO.File.ReadAllLines(FileName);
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileName)) {
            string szCurrClientID = "";

            foreach (string line in lines) {
                if (szCurrClientID != "" && !line.StartsWith(szCurrClientID)) {
                    file.WriteLine(Environment.NewLine);
                }
                file.WriteLine(line);
                szCurrClientID = line.Split(',')[0];
            }
        }
    }

    private void exportFile(string FileName, DataTable dtData) {
        string szPath = G.Settings.MYOBDir + FileName;
        if (File.Exists(szPath))
            File.Delete(szPath);
        StreamWriter output = new StreamWriter(szPath);

        // Use CsvWriter class to output the datatable to a CSV file with the same name as the journal entry
        output.Write(CsvWriter.WriteToString(dtData, true, false));
        output.Flush();
        output.Close();
        oLinks.Visible = true;
        oLinks.InnerHtml += string.Format("<a href='../MYOB/{0}' target='_blank'>{0}</a> <br/>", FileName);
    }

    /// <summary>
    /// Returns the datatable for the type of export that we are doing
    /// </summary>
    /// <param name="UpdateDB">When true, we need to create a MYOB export record and tag each transaction we are exporting with the MYOBExportID</param>
    /// <param name="ExportName">When true, we need to create a MYOB export record and tag each transaction we are exporting with the MYOBExportID</param>
    /// <returns></returns>
    private DataSet getData(bool UpdateDB, string ExportName, int intCompanyID, bool ClearDBIDFields = true) {
        if (UpdateDB)
            intMYOBExportID = Utility.getMYOBExportID(ExportType.Campaign, ExportName);

        string szSQL = string.Format(@"
            SELECT
            'Campaign Agent' as [LASTNAME],
            '' as [FIRSTNAME],
            '' AS ADDR1,
            '' AS ADDR2,
            '' AS ADDR3,
            '' AS ADDR4,
            '' AS INCLUSIVE,
            '' AS INVOICENUMBER,
            CONVERT(varchar(8), CI.INVOICEDATE, 3) as INVOICEDATE,
            '' AS CUSTOMERPO,
            '' AS SHIPVIA,
            '' AS DELIVERYSTATUS,
            MAX(L_GLCODE.DESCRIPTION) AS DESCRIPTION,
             L_GLCODE.CREDITGLCODE AS ACCOUNTNUM,
            CAST(SUM(IP.AMOUNTINCGST) AS MONEY) AS AMOUNT,
            CAST(SUM(IP.AMOUNTINCGST) AS MONEY) AS IncTaxAMOUNT,
            MAX(L_OFF.ADVERTISINGJOBCODE) AS JOB,
            '' AS COMMENT,
            '' AS JOURNALMEMO,
            '' AS SPLASTNAME,
            '' AS SPFIRSTNAME,
            '' AS SHIPPINGDATE,
            'Advertising' as ReferralSource,
            'GST' AS TAXCODE,
            '' AS NONGSTAMOUNT,
            CAST(SUM(IP.AMOUNTINCGST) - SUM(IP.AMOUNTEXGST) as MONEY) as GST_AMOUNT,
            '' AS LCTAMOUNT,
            '' AS FREIGHTAMOUNT,
            '' AS INCTAXFREIGHTAMOUNT,
            '' AS FREIGHTTAXCODE,
            '' AS FREIGHTNONGST,
            '' AS FREIGHTGST,
            '' AS FREIGHTLCT,
            '' AS SALESTATUS,
            '' AS CURRENCYCODE,
            '' AS EXCHANGERATE,
            '' AS TERMS1,
            '' AS TERMS2,
            '' AS TERMS3,
            '' AS TERMS4,
            '' AS TERMS5,
            '' AS AMOUNTPAID,
            '' AS PAYMENTMETHOD,
            '' AS PAYMENTNOTES,
            '' AS NAMEONCARD,
            '' AS CARDNUMBER,
            '' AS EXPIRYDATE,
            '' AS AUTHCODE,
            '' AS BSB,
            '' AS ACCOUNTNUMBER,
            '' AS ACCOUNTNAME,
            '' AS CHECQUENUM,
            '' AS CATEGORY,
            'Campaign Agent' AS CARDID,
            '' AS STOP,  CI.ID, CI.ID AS INVOICEID, MAX(C.ID) AS CAMPAIGNID, MAX(C.ADDRESS1)  AS ADDRESS1
            FROM CAMPAIGNINVOICE CI JOIN CAMPAIGN C ON C.ID = CI.CAMPAIGNID
            JOIN INVOICEPRODUCT IP ON IP.CAMPAIGNINVOICEID = CI.ID
            LEFT JOIN CAMPAIGNPRODUCT CP ON IP.CAMPAIGNPRODUCTID = CP.ID AND CP.ISDELETED = 0
            JOIN PRODUCT P ON P.ID = CP.PRODUCTID
            JOIN LIST L_GLCODE ON L_GLCODE.ID = P.CREDITGLCODE
            JOIN DB_USER U ON U.ID = C.AGENTID
            JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID
            JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID
            WHERE CI.EXPORTID IS NULL AND (L_OFF.COMPANYID = {0} OR {0} = -1)  AND C.OFFICEID = {1}
            GROUP BY C.MYOBCARDID, CI.ID, CI.INVOICENUMBER, CI.INVOICEDATE,  L_GLCODE.CREDITGLCODE;
        ", intCompanyID, G.Settings.CampaignTrackOffice);
        DataSet dsTX = DB.runDataSet(szSQL);
        foreach (DataRow tx in dsTX.Tables[0].Rows) {
            tx["INVOICENUMBER"] = Sale.CreateCode(Convert.ToString(tx["ADDRESS1"]));
        }
        if (UpdateDB) {
            foreach (DataRow tx in dsTX.Tables[0].Rows) {
                szSQL = String.Format(@"
                    UPDATE CAMPAIGNINVOICE SET EXPORTID = {0}
                    WHERE ID = {1}", intMYOBExportID, tx["INVOICEID"].ToString());
                DB.runNonQuery(szSQL);
            }
        }
        if (ClearDBIDFields) {
            clearBeyondStop(dsTX.Tables[0]);
        }
        return dsTX;
    }

    private void clearBeyondStop(DataTable dt) {
        //Remove the datacolumns that are beyond the STOP i
        int intSTOPColumnOrdinal = 0;
        foreach (DataColumn dc in dt.Columns) {
            if (dc.ColumnName == "STOP") {
                intSTOPColumnOrdinal = dc.Ordinal;
            }
        }
        while (dt.Columns.Count > intSTOPColumnOrdinal) {
            dt.Columns.RemoveAt(intSTOPColumnOrdinal);
        }
    }

    private void doClose() {
        Response.Redirect("campaign_dashboard.aspx");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }
}