using System;
using System.Collections;
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
            initPage();
        }
    }

    private void initPage() {
        txtStartDate.Text = Utility.formatDate(G.CurrentPayPeriodStart);
        txtEndDate.Text = Utility.formatDate(G.CurrentPayPeriodEnd);
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        if (btnUpdate.Text == "Preview") {
            DataTable dtData = getDataTable(false);
            gvPreview.DataSource = dtData;
            gvPreview.DataBind();
            HTML.formatGridView(ref gvPreview);
            btnUpdate.Text = "Export";
            gvPreview.Visible = true;
            dValidate.Visible = false;
        } else {
            gvPreview.Visible = false;
            //Load all Company
            string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);

            DataSet dsCompany = DB.runDataSet(szSQL);
            foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
                DataTable dtData = getDataTable(true, Convert.ToInt32(drCompany["ID"]));
                if (dtData.Rows.Count == 0)
                    continue;
                string szFileName = txtJournalNumber.Text + "_USERTX_" + drCompany["NAME"].ToString() + ".csv";
                string szPath = G.Settings.MYOBDir + szFileName;
                if (File.Exists(szPath))
                    File.Delete(szPath);
                StreamWriter output = new StreamWriter(szPath);

                // Use CsvWriter class to output the datatable to a CSV file with the same name as the journal entry
                output.Write(CsvWriter.WriteToString(dtData, true, false));
                output.Flush();
                output.Close();
                oLinks.InnerHtml += string.Format("<a href='../admin/myob_doc.aspx?file={0}' target='_blank'>{1}</a> <br/>", Server.UrlEncode(szFileName), szFileName);
                oLinks.Visible = true;
            }
        }
    }

    /// <summary>
    /// Returns the datatable for the type of export that we are doing
    /// </summary>
    /// <param name="UpdateDB">When true, we need to create a MYOB export record and tag each transaction we are exporting with the MYOBExportID</param>
    /// <returns></returns>
    private DataTable getDataTable(bool UpdateDB, int intCompanyID = -1) {
        sqlUpdate oSQL = new sqlUpdate("MYOBEXPORT", "ID", -1);
        string szName = txtEndDate.Text + "_USERTX_" + Convert.ToString(DB.getScalar("SELECT ISNULL(COUNT(ID), 0) FROM MYOBEXPORT WHERE UPDATETIME = getdate()", 0) + 1) + ".csv";

        if (UpdateDB)
            intMYOBExportID = Utility.getMYOBExportID(ExportType.UserTx, szName);

        string szSQL = string.Format(@"
            SELECT '' as JOURNALNUMBER, '' AS TXDATE, '' as MEMO, 'P' AS GST, '0' AS INCLUSIVE, '' AS ACCOUNTNUMBER, '' AS DEBITEXGST, '' AS DEBITINCGST,
            '' AS CREDITEXGST, '' AS CREDITINCGST, '' as JOB, 'N-T' as TAXCODE, '' as STOP, L_OFF.COMPANYID, L_COMP.NAME AS COMPANY,
            --Debit Info
            TX.ACCOUNTID AS DEBITACCOUNT, L.NAME AS DEBITACCOUNTNAME, REPLACE(TX.DEBITGLCODE, '-', '') AS DEBITACCOUNTGLCODE,
            --Credit info
            U.ID AS CREDITACCOUNT, U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME as CREDITACCOUNTNAME, REPLACE(TX.CREDITGLCODE, '-','') AS CREDITACCOUNTGLCODE,
            TX.AMOUNT, TX.FLETCHERAMOUNT, TX.COMMENT, TX.TXDATE AS TXTXDATE, TX.CREDITJOBCODE, TX.DEBITJOBCODE, L.LISTTYPEID, TX.SHOWEXGST, TX.ID AS TXID
            FROM USERTX TX
            JOIN DB_USER U ON  U.ID = TX.USERID AND TX.ISDELETED = 0
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID
            JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID
            WHERE TX.TXDATE BETWEEN  '{0} 00:00:00' AND '{1} 23:59:59'  AND TX.AMOUNT != 0", txtStartDate.Text, txtEndDate.Text);
        DataSet dsTX = DB.runDataSet(szSQL);
        DataTable dtNew = dsTX.Tables[0].Clone();
        string szJournalNumber = txtJournalNumber.Text;
        DataView dv = dsTX.Tables[0].DefaultView;
        if (intCompanyID > -1)
            dv.RowFilter = "COMPANYID = " + intCompanyID;
        DataTable dtFiltered = dv.ToTable();
        foreach (DataRow tx in dtFiltered.Rows) {
            dtNew.ImportRow(tx);
            DataRow rCurr = dtNew.Rows[dtNew.Rows.Count - 1];

            //Debit
            rCurr["JOURNALNUMBER"] = szJournalNumber;
            rCurr["TXDATE"] = Utility.formatDateForMYOBExport(rCurr["TXTXDATE"].ToString());
            rCurr["ACCOUNTNUMBER"] = rCurr["DEBITACCOUNTGLCODE"];
            rCurr["DEBITEXGST"] = rCurr["AMOUNT"];
            rCurr["DEBITINCGST"] = rCurr["AMOUNT"];  //Math.Round(Convert.ToDouble(rCurr["AMOUNT"]) + (Convert.ToDouble(rCurr["AMOUNT"]) * 0.1), 2);  // Set this appropriately
            rCurr["JOB"] = rCurr["DEBITJOBCODE"];

            //Do the same for the inverse of this transaction
            //Credit
            dtNew.ImportRow(tx);
            rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
            rCurr["JOURNALNUMBER"] = szJournalNumber;
            rCurr["TXDATE"] = Utility.formatDateForMYOBExport(rCurr["TXTXDATE"].ToString());
            rCurr["ACCOUNTNUMBER"] = rCurr["CREDITACCOUNTGLCODE"];
            rCurr["DEBITEXGST"] = "";
            rCurr["CREDITEXGST"] = rCurr["AMOUNT"];
            rCurr["CREDITINCGST"] = rCurr["AMOUNT"];  //Math.Round(Convert.ToDouble(rCurr["AMOUNT"]) + (Convert.ToDouble(rCurr["AMOUNT"]) * 0.1), 2);  // Set this appropriately
            rCurr["JOB"] = rCurr["CREDITJOBCODE"];

            //TODO: If we are updating the DB update the row with the MYOBExportID

            szSQL = String.Format(@"
                UPDATE USERTX SET MYOBEXPORTID = {0}
                WHERE ID = {1}", intMYOBExportID, rCurr["TXID"].ToString());
            DB.runNonQuery(szSQL);
        }
        dtNew.AcceptChanges();

        //Remove the datacolumns that are beyond the STOP in dtNew
        int intSTOPColumnOrdinal = 0;
        foreach (DataColumn dc in dtNew.Columns) {
            if (dc.ColumnName == "STOP") {
                intSTOPColumnOrdinal = dc.Ordinal;
            }
        }
        while (dtNew.Columns.Count > intSTOPColumnOrdinal) {
            dtNew.Columns.RemoveAt(intSTOPColumnOrdinal);
        }

        return dtNew;
    }

    private void doClose() {
        Response.Redirect("../blank.html");
    }

    protected void btnCancel_Click(object sender, System.EventArgs e) {
        doClose();
    }

    protected void btnMYOBValidate_Click(object sender, EventArgs e) {
        Hashtable ht = new Hashtable();
        if (!fuMYOBAccount.HasFile) {
            PageNotificationManager.PageNotificationInfo.addPageNotification(PageNotificationType.Error, "File not selected", "Please select the MYOB export file");
            PageNotificationManager.PageNotificationInfo.showPageNotification();
            return;
        }

        using (StreamReader reader = new StreamReader(fuMYOBAccount.FileContent)) {
            do {
                string szCode = reader.ReadLine();
                if (szCode.Contains("-"))
                    szCode = szCode.Replace("-", "");
                ht.Add(szCode, szCode);
            } while (reader.Peek() != -1);
        }
        DataTable dtData = getDataTable(false);
        string szErrorMessage = "";
        foreach (DataRow dr in dtData.Rows) {
            string szAccountNumber = dr["ACCOUNTNUMBER"].ToString();
            if (szAccountNumber.Contains("-"))
                szAccountNumber = szAccountNumber.Replace("-", "");
            if (!ht.ContainsKey(szAccountNumber)) {
                szErrorMessage += szAccountNumber + "<br/>";
            }
        }

        if (!String.IsNullOrEmpty(szErrorMessage)) {
            szErrorMessage = "The following account codes need to be added to MYOB: <br/><br/>" + szErrorMessage;
            PageNotificationManager.PageNotificationInfo.addPageNotification(PageNotificationType.Warning, "MYOB codes out of date", szErrorMessage);
        } else {
            PageNotificationManager.PageNotificationInfo.addPageNotification(PageNotificationType.Success, "MYOB codes valid", "All the GL codes are up to date.");
        }
        PageNotificationManager.PageNotificationInfo.showPageNotification();
    }
}

// Journal entry MYOB fields
/* Journal Number 255 characters, alphanumeric
Date 10 characters, alphanumeric. Follows date convention of your system. Allows any non-numeric as a separator between months, days and years.
Memo S for Sale (Supply) or P for Purchase (Acquisition). When importing, if the field is blank and the transaction doesn’t have a tax amount, it will be imported and reported on the GST Exceptions report.
Inclusive 1 character alphanumeric indicates the amount is tax-inclusive.
*Account Number 5 characters, numeric. Must be a valid, pre-existing MYOB account number. May have an optional non‑numeric separator between the first digit and the last 4 digits (example: 1-1234).
*Debit Ex-Tax Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
*Debit Inc-Tax Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
*Credit Ex-Tax Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
*Credit Inc-Tax Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
Job
15 characters, alphanumeric Tax Code
3 characters, alphanumeric. If the tax code is not a pre-existing MYOB tax code, the transaction will be imported on a tax-exclusive basis.
Non-GST/LCT Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
GST Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
LCT Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
Import Duty Amount 15 characters (including 2 decimal places). More than 2 decimal places are rounded to 2 decimal places.
Allocation Memo 255 characters, alphanumeric.
Category 15 characters, alphanumeric.*/
