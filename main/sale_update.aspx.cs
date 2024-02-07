using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sale_update : Root {
    private bool blnUserReadOnly = false;

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;

        if (!Page.IsPostBack) {
            blnUserReadOnly = Valid.getBoolean("ReadOnly", false);
            hdReadOnly.Value = blnUserReadOnly.ToString().ToLower();
            hdSaleID.Value = Valid.getInteger("intItemID").ToString();
            loadJSSaleExpense();
            loadSale();
        }
    }

    protected void loadJSSaleExpense() {
        string szJS = string.Format(@"var arrSalesExpense=new Array(); ");
        string szSQL = string.Format(@"
            SELECT ID, AMOUNT, AMOUNTTYPEID
            FROM LIST WHERE LISTTYPEID = {0} AND ISACTIVE = {1}", (int)ListType.OffTheTop, (int)TrueFalse.True);
        DataSet dsSalesExpense = DB.runDataSet(szSQL);
        foreach (DataRow oR in dsSalesExpense.Tables[0].Rows) {
            szJS += string.Format(@"
                var expenseObj=new Object();
                expenseObj.Amount ='{0}';
                expenseObj.AmountType = '{1}';
                arrSalesExpense[{2}] = expenseObj;
                ", oR["AMOUNT"].ToString(), oR["AMOUNTTYPEID"].ToString(), oR["ID"].ToString());
        }
        sbEndJS.Append(szJS);
    }

    private void loadSale() {
        StringBuilder sbHTML = new StringBuilder();
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value), true, true);
        txtCode.Text = oS.Code + " - " + oS.Address;
        txtComments.Text = oS.Comments;
        txtSection27Comments.Text = oS.Section27Comments;
        txtSalesDate.Text = oS.SaleDate;
        dOrigCommission.Text = Utility.formatMoney(oS.GrossCommission);
        dConjCommission.Text = Utility.formatMoney(oS.ConjCommission);
        txtAdvertisingSpend.Text = Utility.formatMoney(oS.AdvertisingSpend);
        txtAuctionDate.Text = oS.AuctionDate;
        chkIsSection27.Checked = oS.IsSection27;
        chkIgnoreBonus.Checked = oS.IgnoreSaleBonus;

        hdGrossCommission.Value = Utility.formatMoney(oS.GrossCommission - oS.ConjCommission);
        hdSplitAmount.Value = Utility.formatMoney(oS.GrossCommission - oS.ExpenseAmount);

        // this is for transition to B&D only.  Once transition is made th
        if (oS.BnDSaleID != Int32.MinValue) {
            btnRefresh.Visible = true;
        }

        if (oS.SaleDate == "") {
            //This is a withdrawn sale so its always updateable
        } else if (Convert.ToDateTime(oS.SaleDate) < G.CurrentPayPeriodStart && !G.User.hasPermission(RolePermissionType.UpdatePreviousPayPeriod) && !blnReadOnly && oS.PayPeriodID > -1) {
            if (!oS.IsFinalized)
                btnFinalize.Visible = true && !blnUserReadOnly;
            hdReadOnly.Value = "true";
        }

        Utility.setListBoxItems(ref lstStatus, oS.Status.ToString());
        hdCurrStatusID.Value = oS.Status.ToString();
        drawSaleExpense(oS);
        drawAgentExpense(oS);
        drawAgentAllocations(oS);

        //load the maximum values
        string szSQL = string.Format(@"
            SELECT ID, ISNULL(MAXAMOUNT, 0) as MAXAMOUNT
            FROM LIST WHERE LISTTYPEID = {0} AND ISACTIVE = 1", (int)ListType.OffTheTop);
        foreach (DataRow oR in DB.runDataSet(szSQL).Tables[0].Rows) {
            Form.Controls.Add(HTML.createHidden("hdMaxOTTCategoryAmount_" + oR["ID"].ToString(), oR["MAXAMOUNT"].ToString()));
        }

        sbHTML.AppendFormat(@"<table class='Box1' id='tSalesInfo' width='100%'><thead class='Box1Head'><tr><td colspan='5'></td><tr></thead>");
        int rowCount = 0;
        foreach (SalesSplit oSS in oS.lSaleSplits) {
            ++rowCount;
            string szAltRow = "";
            if (rowCount % 2 == 0)
                szAltRow = "dgARow";
            string szIDTag = "CT_" + oSS.CommissionTypeID + "_SS_" + oSS.ID;
            DropDownList ddlAmountType = HTML.createAmountTypeListBox("lstAmountType_" + szIDTag, "Entry", "width: 45px");
            ddlAmountType.Attributes.Add("onchange", "amountTypeChange(this);");

            string szValidationType = "numbersOnly";
            Utility.setListBoxItems(ref ddlAmountType, ((int)oSS.AmountType).ToString());
            if (oSS.AmountType == AmountType.Percent)
                szValidationType = "percent";
            string szFletchersSaleSplitClass = "";
            string szFletchersSaleSplitAmountClass = "";

            if (oSS.CommissionName.IndexOf("Fletchers") > -1) {
                szFletchersSaleSplitClass = "JQFletchersSaleSplit";
                szFletchersSaleSplitAmountClass = "JQFletchersSaleSplitAmount";
            }
            string szHTMLAmount = string.Format(@"
                <input id='txtAmount_{0}' name='txtAmount_{0}' value='{1}' type='text' class='Entry JQSaleSplitAmount {3} {2}' style='width:120px;' onfocus='highlightTextOnFocus(this)' onblur='saleSplitAmountChange(this); '/>
                "
                , szIDTag, oSS.Amount, szValidationType, szFletchersSaleSplitAmountClass, oSS.ID);

            string szHTMLCalcAmount = string.Format(@"
                <div id='{0}' class='JQSaleSplit {2} CalcTotal' style='width:120px; text-align:right'>{1}</div>"
                , "txtCalculatedAmount_" + szIDTag, Utility.formatMoney(oSS.CalculatedAmount), szFletchersSaleSplitClass);

            string szUserSplitSumHTML = string.Format(@"
                <tr id='rowUserSplitSum'>
                    <td style='width:30%' colspan='2'></td>
                    <td style='width:10%'></td>
                    <td style='width:10%; text-align:right' class='calcTotal' >Total:</td>
                    <td style='width:5%'>&nbsp;</td>
                    <td style='width:15%'><div id='UserSplit_{0}' name='UserSplit_{0}' class='CalcTotal AlignRight UserSaleSplitSum'></div></td>
                    <td style='width:7%'>&nbsp;</td>
                    <td style='width:13%'></td>
                    <td style='width:15%'>&nbsp;</td>
                </tr>", oSS.CommissionTypeID);

            sbHTML.AppendFormat(@"
                    <tr id='dSS_{0}' class='{2}'>
                         <td style='width:10%' ><strong> {1} </strong> </td>
                         <td style='width:10%' class='HalignRight'>{3}</td>
                         <td style='width:10%'>{4}</td>
                         <td style='width:20%'>{5}</td>
                         <td style='width:50%'>
                            <table id='tblSplit_{0}' class='Box2' width='100%'>
                            <tbody>
                                {6}
                            </tbody>
                            <tfoot class='Box2Foot{2}'>
                              {7}
                            </tfoot>
                            </table>
                        </td>
                    </tr>
                ", szIDTag, oSS.CommissionName, szAltRow, szHTMLAmount, Utility.ControlToString(ddlAmountType),
                 szHTMLCalcAmount, getUserSplitHTML(oSS), szUserSplitSumHTML);
        }

        string szUserSplitCommissionTotalHTML = string.Format(@"
                <tr id='rowUserSplitSum'>
                    <td colspan ='2' style='width:55%; text-align:right' class='calcTotal'>User commissions total:</td>
                    <td style='width:5%'>&nbsp;</td>
                    <td style='width:15%'>
                        <div id='dUserCommissionsTotal' name='dUserCommissionsTotal' class='CalcTotal AlignRight'>{0}</div>
                        <div id='dDifferenceUserSpiltAmountAndCommissionTotal' name='dDifferenceUserSpiltAmountAndCommissionTotal' class='CalcTotal AlignRight' style='color:Red'>{0}</div>
                    </td>
                    <td style='width:7%'>&nbsp;</td>
                    <td style='width:13%'></td>
                    <td style='width:10%'>&nbsp;</td>
                </tr>", Utility.formatMoney(oS.GrossCommission - oS.ExpenseAmount));

        string szHTMLFooter = String.Format(@"
            <tfoot class='Box1Foot'>
            <tr>
                <td colspan='5'>
                    <table class='Box1Foot' width='100%'>
                        <tr>
                            <td style='width:10%;'></td>
                            <td colspan='2' style='width:20%;' class='HalignRight CalcTotal' text-align:right'>Commissions total:</td>
                            <td style='width:20%'>
                                <div id='dCommissionsTotal' name='dCommissionsTotal' class='CalcTotal' style='width:120px; text-align:right'>{0}</div>
                                <div id='dDifferenceSpiltAmountAndCommissionTotal' name='dDifferenceSpiltAmountAndCommissionTotal' class='CalcTotal' style='width:120px; text-align:right; color:Red'>{0}</div>
                            </td>
                            <td style='width:50%'>
                                <table class='Box1Foot' width='100%'>
                                    {1}
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            </tfoot>", Utility.formatMoney(oS.GrossCommission - oS.ExpenseAmount), szUserSplitCommissionTotalHTML);

        dSaleInfo.InnerHtml = sbHTML.ToString() + szHTMLFooter + "</table>";
    }

    protected string getUserSplitHTML(SalesSplit CurrSalesSplit) {
        StringBuilder sbHTML = new StringBuilder();
        int count = 0;
        bool blnAddButton = false;
        if (CurrSalesSplit.CommissionName.IndexOf("Fletchers") > -1) {
            return HTML.userSplitHTML(CurrSalesSplit.CommissionTypeID, CurrSalesSplit.ID);
        } else {
            foreach (UserSalesSplit oUSS in CurrSalesSplit.lUserSplits) {
                count++;
                if (count == CurrSalesSplit.lUserSplits.Count) {
                    blnAddButton = true;
                }
                sbHTML.Append(HTML.userSplitHTML(CurrSalesSplit.CommissionTypeID, CurrSalesSplit.ID, blnAddButton, oUSS));
            }
            if (sbHTML.ToString() != "")
                return sbHTML.ToString();
            else {
                if (!blnAddButton)
                    blnAddButton = true;
                return HTML.userSplitHTML(CurrSalesSplit.CommissionTypeID, CurrSalesSplit.ID, blnAddButton);
            }
        }
    }

    protected string getCommissionTypeTotalAmountBox(SalesSplit CurrSalesSplit) {
        string szIDTag = "txtBxCommissionTypeTotalAmount_CT_" + CurrSalesSplit.CommissionTypeID;
        string szHTML = string.Format(@"<div id='{0}' class='CalcTotal' style='width:120px; text-align:right'></div>", szIDTag);
        return szHTML;
    }


    protected void btnUpdate_Click(object sender, EventArgs e) {
        sqlUpdate oSQL = new sqlUpdate("SALE", "ID", Convert.ToInt32(hdSaleID.Value));
        oSQL.add("STATUSID", lstStatus.SelectedValue);
        if (txtAuctionDate.Text == "")
            oSQL.addNull("AUCTIONDATE");
        else
            oSQL.add("AUCTIONDATE", txtAuctionDate.Text);
        oSQL.add("ADVERTISINGSPEND", Utility.isDouble(txtAdvertisingSpend.Text));
        oSQL.add("ISSECTION27", chkIsSection27);
        oSQL.add("IGNORESALEBONUS", chkIgnoreBonus);
        oSQL.add("COMMENTS", txtComments.Text);
        oSQL.add("SECTION27COMMENTS", txtSection27Comments.Text);
        if (lstStatus.SelectedValue == "2") {
            //Check to see if the payperiod has already been set - we never overwrite the payperiod
            int CurrPayPeriodID = DB.getScalar("SELECT ISNULL(PAYPERIODID, -1) AS PAYPERIODID FROM SALE WHERE ID = " + hdSaleID.Value, -1);
            if (CurrPayPeriodID == -1) {
                //Check to make sure this isn't a withdrawn sale, as we don't ever want to tag those with a pay period
                object szSaleDate = DB.getScalar("SELECT SALEDATE  FROM SALE WHERE ID = " + hdSaleID.Value);
                if (szSaleDate != System.DBNull.Value)
                    oSQL.add("PAYPERIODID", G.CurrentPayPeriod);
            }
        }
        DB.runNonQuery(oSQL.createUpdateSQL());

        if (hdCurrStatusID.Value != lstStatus.SelectedValue) {
            if (lstStatus.SelectedValue == "1") {
                DBLog.addGenericRecord(DBLogType.SaleCompleted, "Completed", Convert.ToInt32(hdSaleID.Value));
            } else if (lstStatus.SelectedValue == "2") {
                DBLog.addGenericRecord(DBLogType.SaleFinalized, "Finalized", Convert.ToInt32(hdSaleID.Value));
            }
        }
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value));

        string szSQLUpdate = "";

        updateSaleExpenses(ref szSQLUpdate, ref oS);
        updateAgentExpenses(ref szSQLUpdate, ref oS);
        updateAgentAllocations(ref szSQLUpdate, ref oS);

        string[] arrSaleSplit = hdSaleSplit.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string oTagID in arrSaleSplit) {
            string CalculatedAmount = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[1];
            string szTagID = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] arrIDs = szTagID.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            int CommissionType = Convert.ToInt32(arrIDs[2]);
            int SaleSplitID = Convert.ToInt32(arrIDs[4]);
            string Amount = Valid.getText(szTagID.Replace("txtCalculatedAmount", "txtAmount"), VT.NoValidation);
            if (!Utility.IsNumeric(Amount))
                Amount = "0.0";
            if (!Utility.IsNumeric(CalculatedAmount))
                CalculatedAmount = "0.0";

            AmountType oAmountType = (AmountType)Convert.ToInt32(Valid.getList(szTagID.Replace("txtCalculatedAmount", "lstAmountType"), "0"));
            Utility.Append(ref szSQLUpdate, oS.updateSaleSplit(SaleSplitID, CommissionType, Convert.ToDouble(Amount), oAmountType, Convert.ToDouble(CalculatedAmount)), ";");
        }

        string[] arrUserSaleSplit = hdUserSaleSplit.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        int intKPIUser = Valid.getInteger("hdKPIUserID");
        if (szSQLUpdate != "") {
            DB.runNonQuery(szSQLUpdate);
            szSQLUpdate = "";
        }
        foreach (string oTagID in arrUserSaleSplit) {
            string[] szTagSplit = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (szTagSplit.Length == 2) {
                string CalculatedAmount = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[1];
                string szTagID = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[0];
                string[] arrIDs = szTagID.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                int CommissionType = Convert.ToInt32(arrIDs[2]);
                int SaleSplitID = Convert.ToInt32(arrIDs[4]);
                int UserSaleSplitID = Convert.ToInt32(arrIDs[6]);
                int UserID = Convert.ToInt32(Valid.getList(szTagID.Replace("txtCalculatedAmount", "lstUserSaleSplit"), "-1"));
                string Amount = Valid.getText(szTagID.Replace("txtCalculatedAmount", "txtAmount"), "0.0", VT.NoValidation);
                if (!Utility.IsNumeric(Amount))
                    Amount = "0.0";
                if (!Utility.IsNumeric(CalculatedAmount))
                    CalculatedAmount = "0.0";

                AmountType oAmountType = (AmountType)Convert.ToInt32(Valid.getList(szTagID.Replace("txtCalculatedAmount", "lstAmountType"), "0"));
                bool blnIncludeInKPI = CommissionType == 10 && intKPIUser == UserID;

                if (SaleSplitID == -1)
                    SaleSplitID = DB.getScalar(String.Format(@"SELECT ID FROM SALESPLIT WHERE SALEID = {0} AND COMMISSIONTYPEID = {1}", Convert.ToInt32(hdSaleID.Value), CommissionType), -1);
                int OfficeID = -1;
                UserDetail uD = G.UserInfo.getUser(UserID);
                if (uD != null)
                    OfficeID = uD.OfficeID;
                Utility.Append(ref szSQLUpdate, oS.updateUserSaleSplit(SaleSplitID, UserSaleSplitID, UserID, CommissionType, Convert.ToDouble(Amount), oAmountType, Convert.ToDouble(CalculatedAmount), 0, blnIncludeInKPI, OfficeID), ";");
            }
        }
        //Update the office for the sales split
        szSQLUpdate += String.Format(@"
            UPDATE USERSALESPLIT SET OFFICEID = (SELECT OFFICEID FROM DB_USER WHERE ID = USERID) WHERE OFFICEID = -1;

            UPDATE SALE SET OFFICEID = (
                SELECT TOP 1 U.OFFICEID
                FROM SALESPLIT SS
                JOIN USERSALESPLIT USS on SS.ID = USS.SALESPLITID AND SS.COMMISSIONTYPEID = 10 AND USS.INCLUDEINKPI = 1 AND USS.RECORDSTATUS < 1
                JOIN DB_USER U ON USS.USERID = U.ID AND SS.SALEID = {0})
            WHERE ID = {0}", hdSaleID.Value);

        DB.runNonQuery(szSQLUpdate);
        oS.performPostProcessing();
        ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }

    private void updateSaleExpenses(ref string szSQLUpdate, ref Sale oS) {
        string[] arrOffTheTopExpensesSplit = hdOffTheTopExpensesSplit.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string oTagID in arrOffTheTopExpensesSplit) {
            string CalculatedAmount = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[1];
            string szTagID = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] arrIDs = szTagID.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            int CategoryID = Convert.ToInt32(Valid.getList(szTagID.Replace("txtCalculatedAmount", "lstCategory"), "0"));
            int intDBID = Convert.ToInt32(Valid.getInteger(szTagID.Replace("txtCalculatedAmount", "hdExpenseDBID"), -1));
            AmountType oAmountType = (AmountType)Convert.ToInt32(Valid.getList(szTagID.Replace("txtCalculatedAmount", "lstAmountType"), "0"));
            string Amount = Valid.getText(szTagID.Replace("txtCalculatedAmount", "txtAmount"), VT.NoValidation);
            Utility.Append(ref szSQLUpdate, oS.updateSaleExpense(intDBID, CategoryID, Convert.ToDouble(Amount), oAmountType, Convert.ToDouble(CalculatedAmount)), ";");
        }
        string szDelIDs = hdDelExpenseIDs.Value;
        if (!String.IsNullOrWhiteSpace(szDelIDs)) {
            string[] arIDs = szDelIDs.Split(',');
            foreach (string szID in arIDs) {
                oS.deleteSalesExpense(Convert.ToInt32(szID));
            }
        }
    }

    private void updateAgentExpenses(ref string szSQLUpdate, ref Sale oS) {
        string[] arrAgentOTTSplit = hdAgentOTTExpensesSplit.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string oTagID in arrAgentOTTSplit) {
            string szTagID = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] arrIDs = szTagID.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            int CategoryID = Convert.ToInt32(Valid.getList(szTagID.Replace("txtAmount", "lstCategory"), "0"));
            int intDBID = Convert.ToInt32(Valid.getInteger(szTagID.Replace("txtAmount", "hdAgentExpenseDBID"), -1));
            string Amount = Valid.getText(szTagID, VT.NoValidation);
            if (!String.IsNullOrEmpty(Amount)) {
                Utility.Append(ref szSQLUpdate, oS.updateAgentSaleExpense(intDBID, CategoryID, Convert.ToDouble(Amount)), ";");
            }
        }

        string szDelIDs = hdDelAgentExpenseIDs.Value;
        if (!String.IsNullOrWhiteSpace(szDelIDs)) {
            string[] arIDs = szDelIDs.Split(',');
            foreach (string szID in arIDs) {
                oS.deleteAgentExpense(Convert.ToInt32(szID));
            }
        }
    }

    private void updateAgentAllocations(ref string szSQLUpdate, ref Sale oS) {
        string[] arrAgentOTTSplit = hdAgentAllocationSplits.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        List<int> lAgents = new List<int>();
        foreach (string oTagID in arrAgentOTTSplit) {
            string szTagID = oTagID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] arrIDs = szTagID.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            int UserID = Convert.ToInt32(arrIDs[1]);
            string Amount = Valid.getText(szTagID, VT.NoValidation);
            if (!String.IsNullOrEmpty(Amount)) {
                Utility.Append(ref szSQLUpdate, oS.updateAgentAllocation(UserID, Convert.ToDouble(Amount)), ";");
                lAgents.Add(UserID);
            }
        }
        if (lAgents.Count > 0) {
            DB.runNonQuery(String.Format(@"
                DELETE FROM USERTX 
                WHERE AGENTSALEALLOCATIONID IN 
                    (SELECT ID FROM AGENTSALEALLOCATION WHERE SALEID = {0} and USERID NOT IN ({1}));

                DELETE FROM AGENTSALEALLOCATION WHERE SALEID = {0} AND USERID NOT IN ({1});
                ", oS.SaleID, String.Join(",", lAgents)));
        }
    }

    protected void drawSaleExpense(Sale oS) {
        StringBuilder sbHTML = new StringBuilder();
        int count = 0;
        bool blnAddButton = false;
        foreach (SalesExpense oSE in oS.lSaleExpenses) {
            count++;
            if (count == oS.lSaleExpenses.Count) {
                blnAddButton = true;
            }

            sbHTML.Append(SalesExpense.getExpenseHTML(blnAddButton, oSE.ExpenseTypeID, oSE));
        }
        dSaleExpenses.InnerHtml = sbHTML.ToString();
    }

    protected void drawAgentExpense(Sale oS) {
        StringBuilder sbHTML = new StringBuilder();
        int count = 0;
        bool blnAddButton = false;
        foreach (AgentExpense oSE in oS.lAgentExpenses) {
            count++;
            if (count == oS.lAgentExpenses.Count) {
                blnAddButton = true;
            }

            sbHTML.Append(AgentExpense.getExpenseHTML(blnAddButton, oSE.ExpenseTypeID, oSE));
        }
        dAgentExpenses.InnerHtml = sbHTML.ToString();
    }

    protected void drawAgentAllocations(Sale oS) {
        StringBuilder sbHTML = new StringBuilder();
        
        foreach (AgentAllocation oAA in oS.lAgentAllocations) {
            sbHTML.Append(AgentAllocation.getExpenseHTML(oAA.UserID, oAA));
        }
        dAgentAllocations.InnerHtml = sbHTML.ToString();
    }
    protected void btnHide_Click(object sender, EventArgs e) {
        string szSQL = "UPDATE SALE SET STATUSID = 3 WHERE ID = " + hdSaleID.Value;
        DB.runNonQuery(szSQL);
        ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }

    protected void btnFinalize_Click(object sender, EventArgs e) {
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value));
        oS.finalizeSale();

        ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }

    protected void btnRefreshBnD_Click(object sender, EventArgs e) {
        Sale oS = new Sale(Convert.ToInt32(hdSaleID.Value));
        SalesListing.importCompleteRecord(oS.BnDSaleID, true);
        loadJSSaleExpense();
        loadSale();
    }
}