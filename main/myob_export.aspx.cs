using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

public partial class myob_export : Root {
    protected int intItemID = -1;
    private int intMYOBExportID = 0;
    DateTime dtStart = DateTime.MaxValue;
    DateTime dtEnd = DateTime.MaxValue;
    PayPeriod oP = null;
    protected void Page_Load(object sender, System.EventArgs e) {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetExpires(System.DateTime.Now);
        hdItemID.Value = intItemID.ToString();
        if (!IsPostBack) {
            loadFilters();
        } else {
            dtStart = DateTime.MaxValue;
            dtEnd = DateTime.MaxValue;
            oP = G.PayPeriodInfo.getPayPeriod(Convert.ToInt32(lstPayPeriod.SelectedValue));
            if (oP != null) {
                dtStart = oP.StartDate;
                dtEnd = oP.EndDate;
            }
        }
    }

    protected void loadFilters() {
        string szSQL = string.Format("select ID, STARTDATE , ENDDATE, '' AS NAME from PAYPERIOD ORDER BY ID DESC");
        DataSet ds = DB.runDataSet(szSQL);

        foreach (DataRow dr in ds.Tables[0].Rows) {
            dr["NAME"] = Utility.formatDate(dr["StartDate"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
        }
        Utility.BindList(ref lstPayPeriod, ds, "ID", "NAME");

        Utility.BindList(ref lstCompany, DB.runDataSet(string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company)), "ID", "NAME");
        lstCompany.Items.Insert(0, new ListItem("All", ""));
    }

    protected void btnPreview_Click(object sender, System.EventArgs e) {
       
        DataTable dtData = getAdvancedDataTable(false);
        gvPreview.DataSource = dtData;
        gvPreview.DataBind();
        HTML.formatGridView(ref gvPreview);
        gvPreview.Visible = true;
    }

    protected void btnExport_Click(object sender, System.EventArgs e) {
        gvPreview.Visible = false;
        processExport();
        processExportAdvanced();
    }

    private void processExport() {
        //Load all Company
        string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);

        DataSet dsCompany = DB.runDataSet(szSQL);
        foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
            DataTable dtData = getDataTable(true, Convert.ToInt32(drCompany["ID"]));
            if (dtData.Rows.Count == 0)
                continue;
            string szFileName = txtJournalNumber.Text + "_USERTX_" + drCompany["NAME"].ToString() + ".txt";
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

    protected void processExportAdvanced() {
        //Load all Company
        string szSQL = string.Format("SELECT ID, NAME FROM LIST WHERE LISTTYPEID = {0}", (int)ListType.Company);

        DataSet dsCompany = DB.runDataSet(szSQL);
        foreach (DataRow drCompany in dsCompany.Tables[0].Rows) {
            DataTable dtData = getAdvancedDataTable(true, Convert.ToInt32(drCompany["ID"]));
            if (dtData.Rows.Count == 0)
                continue;
            string szFileName = txtJournalNumber.Text + "_USERTX_ADVANCED_" + drCompany["NAME"].ToString() + ".csv";
            string szPath = Path.Combine(G.Settings.MYOBDir, szFileName);
            if (File.Exists(szPath))
                File.Delete(szPath);
            StreamWriter output = new StreamWriter(szPath);

            // Use CsvWriter class to output the datatable to a CSV file with the same name as the journal entry
            output.Write(CsvWriter.WriteToString(dtData, true, false, false));
            output.Flush();
            output.Close();
            oLinks.InnerHtml += string.Format("<a href='../admin/myob_doc.aspx?file={0}' target='_blank'>{1}</a> <br/>", Server.UrlEncode(szFileName), szFileName);
            oLinks.Visible = true;
        }
    }

    /// <summary>
    /// Returns the datatable for the type of export that we are doing
    /// </summary>
    /// <param name="UpdateDB">When true, we need to create a MYOB export record and tag each transaction we are exporting with the MYOBExportID</param>
    /// <returns></returns>
    private DataTable getAdvancedDataTable(bool UpdateDB, int intCompanyID = -1) {
        sqlUpdate oSQL = new sqlUpdate("MYOBEXPORT", "ID", -1);
        string szName = Utility.formatDate(dtStart) + "_USERTX_ADVANCED_" + Convert.ToString(DB.getScalar("SELECT ISNULL(COUNT(ID), 0) FROM MYOBEXPORT WHERE UPDATETIME = getdate()", 0) + 1) + ".csv";

        if (UpdateDB)
            intMYOBExportID = Utility.getMYOBExportID(ExportType.UserTx, szName);

        string szWhere = "";
        if(lstCompany.SelectedValue != "") {
            szWhere = " AND L_COMP.ID = " + lstCompany.SelectedValue;
        }
        string szSQL = string.Format(@"
            SELECT L_OFF.OFFICEMYOBBRANCH as Branch, '2-3000' AS Account, L_OFF.OFFICEMYOBCODE + '-' + U.INITIALSCODE  AS Subaccount,
            '' as [Debit Amount], '' as [Credit Amount], '' as [Ref. Number], L_CAT.NAME AS [Transaction Description],
            '' as STOP, L_OFF.COMPANYID, L_COMP.NAME AS COMPANY,
            --Debit Info
            TX.ACCOUNTID AS DEBITACCOUNT, L.NAME AS DEBITACCOUNTNAME,TX.DEBITGLCODE AS DEBITACCOUNTGLCODE,
            --Credit info
            U.ID AS CREDITACCOUNT, U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME as CREDITACCOUNTNAME, TX.CREDITGLCODE AS CREDITACCOUNTGLCODE,
             TX.CREDITJOBCODE, TX.DEBITJOBCODE,  TX.ID AS TXID, TX.AMOUNT, TX.TXDATE, TX.USERID, ISNULL(UPP.SUPERPAID, 0) as SUPERPAID
            FROM USERTX TX
            JOIN DB_USER U ON  U.ID = TX.USERID AND TX.ISDELETED = 0
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID
            JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID
            LEFT JOIN  list L_CAT on ACCOUNTID = L_CAT.ID
            JOIN USERPAYPERIOD UPP ON U.ID = UPP.USERID AND UPP.PAYPERIODID = {2}
            WHERE TX.TXDATE BETWEEN  '{0} 00:00:00' AND '{1} 23:59:59'  AND TX.AMOUNT != 0 {3}
            ", Utility.formatDate(dtStart), Utility.formatDate(dtEnd), oP.ID, szWhere);
        DataSet dsTX = DB.runDataSet(szSQL);
        DataTable dtNew = dsTX.Tables[0].Clone();
        string szJournalNumber = txtJournalNumber.Text;
        DataView dv = dsTX.Tables[0].DefaultView;
        if (intCompanyID > -1)
            dv.RowFilter = "COMPANYID = " + intCompanyID;
        DataTable dtFiltered = dv.ToTable();

        List<int> lSuperUsers = new List<int>();
        foreach (DataRow tx in dtFiltered.Rows) {
            dtNew.ImportRow(tx);
            DataRow rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
            //53050
            //Debit
            rCurr["Ref. Number"] = Utility.formatDate(Convert.ToDateTime(rCurr["TXDATE"]));
            rCurr["Account"] = Convert.ToString(rCurr["DEBITACCOUNTGLCODE"]);
            rCurr["DEBIT AMOUNT"] = rCurr["AMOUNT"];

            //Do the same for the inverse of this transaction
            //Credit
            dtNew.ImportRow(tx);
            rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
            rCurr["Ref. Number"] = Utility.formatDate(Convert.ToDateTime(rCurr["TXDATE"]));
            rCurr["ACCOUNT"] = Convert.ToString(rCurr["CREDITACCOUNTGLCODE"]);
            rCurr["CREDIT AMOUNT"] = rCurr["AMOUNT"];
          
            if (!lSuperUsers.Contains(DB.readInt(rCurr["USERID"]))) {
                lSuperUsers.Add(DB.readInt(rCurr["USERID"]));
                //Output the super record -  if it is > 0
                double Super = DB.readDouble(rCurr["SUPERPAID"]);
                if (Super > 0) {
                    dtNew.ImportRow(tx);
                    rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                    rCurr["Ref. Number"] = "";
                    rCurr["Transaction Description"] = "Super";
                    rCurr["ACCOUNT"] = "2-3050";
                    rCurr["DEBIT AMOUNT"] = Utility.formatMoney(Super);

                    dtNew.ImportRow(tx);
                    rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
                    rCurr["Ref. Number"] = "";
                    rCurr["Transaction Description"] = "Super";
                    rCurr["ACCOUNT"] = G.Settings.SuperGLCode;
                    rCurr["CREDIT AMOUNT"] = Super;
                }
            }
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

    /// <summary>
    /// Returns the datatable for the type of export that we are doing
    /// </summary>
    /// <param name="UpdateDB">When true, we need to create a MYOB export record and tag each transaction we are exporting with the MYOBExportID</param>
    /// <returns></returns>
    private DataTable getDataTable(bool UpdateDB, int intCompanyID = -1) {
        sqlUpdate oSQL = new sqlUpdate("MYOBEXPORT", "ID", -1);
        string szName = Utility.formatDate(dtStart) + "_USERTX_" + Convert.ToString(DB.getScalar("SELECT ISNULL(COUNT(ID), 0) FROM MYOBEXPORT WHERE UPDATETIME = getdate()", 0) + 1) + ".csv";

        if (UpdateDB)
            intMYOBExportID = Utility.getMYOBExportID(ExportType.UserTx, szName);

        string szSQL = string.Format(@"
            SELECT '' as [Journal Number], '' AS [Date], '' as Memo, 'P' AS [GST (BAS) Reporting], '0' AS Inclusive, '' AS [Account Number], 'N' AS [Is Credit], TX.AMOUNT AS Amount,
             '' as Job, 'N-T' as TAXCODE,
            '' as STOP, L_OFF.COMPANYID, L_COMP.NAME AS COMPANY,
            --Debit Info
            TX.ACCOUNTID AS DEBITACCOUNT, L.NAME AS DEBITACCOUNTNAME, REPLACE(TX.DEBITGLCODE, '-', '') AS DEBITACCOUNTGLCODE,
            --Credit info
            U.ID AS CREDITACCOUNT, U.INITIALSCODE + ' ' + FIRSTNAME + ' ' + LASTNAME as CREDITACCOUNTNAME, REPLACE(TX.CREDITGLCODE, '-','') AS CREDITACCOUNTGLCODE,
             TX.CREDITJOBCODE, TX.DEBITJOBCODE,  TX.ID AS TXID
            FROM USERTX TX
            JOIN DB_USER U ON  U.ID = TX.USERID AND TX.ISDELETED = 0
            JOIN LIST L ON L.ID = TX.ACCOUNTID
            JOIN LIST L_OFF ON L_OFF.ID = U.OFFICEID
            JOIN LIST L_COMP ON L_COMP.ID = L_OFF.COMPANYID
            WHERE TX.TXDATE BETWEEN  '{0} 00:00:00' AND '{1} 23:59:59'  AND TX.AMOUNT != 0", Utility.formatDate(dtStart), Utility.formatDate(dtEnd));
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
            rCurr["JOURNAL NUMBER"] = szJournalNumber;
            rCurr["DATE"] = Utility.formatDateForMYOBExport(rCurr["DATE"].ToString());
            rCurr["Account number"] = rCurr["DEBITACCOUNTGLCODE"];
            rCurr["AMOUNT"] = rCurr["AMOUNT"];
            rCurr["JOB"] = rCurr["DEBITJOBCODE"];

            //Do the same for the inverse of this transaction
            //Credit
            dtNew.ImportRow(tx);
            rCurr = dtNew.Rows[dtNew.Rows.Count - 1];
            rCurr["JOURNAL NUMBER"] = szJournalNumber;
            rCurr["DATE"] = Utility.formatDateForMYOBExport(rCurr["DATE"].ToString());
            rCurr["ACCOUNT NUMBER"] = rCurr["CREDITACCOUNTGLCODE"];
            rCurr["JOB"] = rCurr["CREDITJOBCODE"];
            rCurr["IS CREDIT"] = "Y";

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

}