using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Web.UI;

public partial class campaign_update : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
        if (!Page.IsPostBack) {
            hdReadOnly.Value = Valid.getText("ReadOnly", "", VT.NoValidation);
            hdCampaignID.Value = Valid.getInteger("intCampaignID").ToString();
            hdInitialActionID.Value = Valid.getInteger("intActionID", -1).ToString();
            initPage();
            loadCampaign();
        } else {
            hdInitialActionID.Value = "";
        }
    }

    private void initPage() {
        Form.Controls.Add(HTML.createLabel(HTML.createModalIFrameHTML("Action", "Add note", "900px", 500)));
    }

    private void loadCampaign() {
        StringBuilder sbHTML = new StringBuilder();

        Campaign oC = new Campaign(Convert.ToInt32(hdCampaignID.Value));
        txtCode.Text = oC.CampaignTrackID + " - " + oC.Address;
        lblAgent.Text = oC.Agent;
        lblOffice.Text = oC.Office;
        lblLastImported.Text = "&nbsp;<a href='javascript: refreshData()'>Refresh</a><br/>" + Utility.formatDateTime(oC.LastImportedDate);
        txtStartDate.Text = Utility.formatDate(oC.CampaignDate);
        //        Utility.setListBoxItems(ref lstAction, Convert.ToString(oC.ActionID));
        dTotalBudget.Text = Utility.formatMoney(oC.TotalBudget);
        dTotalSpent.Text = Utility.formatMoney(oC.ActualSpent);

        Utility.BindList(ref lstFocusAgent, DB.Paymaker_User.loadList("", true, oC.FocusAgentID), "ID", "NAME");
        Utility.BindList(ref lstDistributionAgent, DB.Paymaker_User.loadList("", true, oC.DistributionAgentID), "ID", "NAME");

        Utility.setListBoxItems(ref lstFocusAgent, oC.FocusAgentID.ToString());
        Utility.setListBoxItems(ref lstDistributionAgent, oC.DistributionAgentID.ToString());

        if (oC.BudgetRemaining < 0) {
            lblBudgetRemaining.Text = "Over budget";
            lblBudgetRemaining.ForeColor = Color.Red;
            lblBudgetRemainingAmount.ForeColor = Color.Red;
        }
        lblBudgetRemainingAmount.Text = Utility.formatMoney(oC.BudgetRemaining);
        txtNotes.Text = oC.Notes;
        lblInvoiced.Text = Utility.formatMoney(oC.TotalInvoices);
        lblPaidAmount.Text = Utility.formatMoney(oC.TotalPaid);
        lblOwing.Text = Utility.formatMoney(oC.AmountOwing);
        dTotalAgentCompanyContribution.Text = Utility.formatMoney(oC.TotalAgentCompany);
        Utility.setListBoxItems(ref lstStatus, Convert.ToString((int)oC.Status));
        loadProducts(oC);
        loadContributions(oC);
        loadFinancial(oC);
        loadActions(oC);
    }

    private void loadProducts(Campaign oC) {
        StringBuilder sbHTML = new StringBuilder();
        foreach (CampaignProduct oP in oC.ProductList) {
            if (oP.ActualPrice == 0 && oP.CostPrice == 0 && oP.VendorPrice == 0)
                continue;
            if (oP.IsDeleted)
                continue;
            sbHTML.AppendFormat(@"
                <tr>
                    <td>{0}</td>
                    <td>{1}</td>
                    <td align='right'>{2}</td>
                    <td align='right'>{3}</td>
                </tr>", Utility.formatDate(oP.Date), oP.Description, Utility.formatMoney(Convert.ToDouble(oP.VendorPrice)), oP.InvoiceNo);
        }
        if (sbHTML.Length > 0) {
            dProducts.InnerHtml = String.Format(@"
                <table cellspacing='1' cellpadding='0' width='98%' id='tProductList'>
                    <thead class='ListHeader'>
                        <th width='20%'>Date</th>
                        <th width='40%'>Item</th>
                        <th width='20%'>Amount</th>
                        <th width='20%'>Inv #</th>
                    </thead>
                    {0}
                    <tr>
                        <td>&nbsp;</td>
                        <td><strong>Totals</strong></td>
                        <td align='right'><strong>{1}</strong></td>
                        <td align='right'><strong>{2}</strong></td>
                    </tr>
                    </table>", sbHTML.ToString(), Utility.formatMoney(oC.ActualSpent), Utility.formatMoney(oC.TotalVendor));
        } else {
            dProducts.InnerHtml = "<p>No items are entered for this campaign.</p>";
        }
    }

    private void loadFinancial(Campaign oC) {
        StringBuilder sbHTML = new StringBuilder();
        string szDate = "";
        string szType = "";
        string szCredit = "";
        string szDebit = "";
        SortedList oSL = new SortedList(); //Sort the payments and contributions by date
        foreach (CampaignContribution oCC in oC.ContributionList) {
            if (oCC.IsPayment || (oCC.Type == ContributionType.Fletchers || oCC.Type == ContributionType.Agent)) {
                oSL.Add(oC.CampaignDate.Ticks + oSL.Count, oCC);
            }
        }

        foreach (CampaignInvoice oI in oC.InvoiceList) {
            oSL.Add(oI.Date.Ticks + oSL.Count, oI);
        }

        double dBalance = 0;
        string szStatusIcon = "";

        foreach (DictionaryEntry oDE in oSL) {
            szCredit = szDebit = "";
            szStatusIcon = "";

            if (oDE.Value is CampaignInvoice) {
                CampaignInvoice oI = (CampaignInvoice)oDE.Value;
                szDate = Utility.formatDate(oI.Date);
                szCredit = "";
                szDebit = Utility.formatMoney(Convert.ToDouble(oI.Amount));
                szType = "Invoice";
                dBalance -= oI.Amount;
            } else {
                CampaignContribution oCC = (CampaignContribution)oDE.Value;
                szDate = Utility.formatDate(oCC.Date);
                szType = oCC.Type.ToString();

                //Check to see whether this has been paid or is a debit
                if (oCC.IsPayment || (oCC.Type == ContributionType.Agent || oCC.Type == ContributionType.Fletchers))
                    szCredit = Utility.formatMoney(Convert.ToDouble(oCC.Amount));
                else
                    szDebit = Utility.formatMoney(Convert.ToDouble(oCC.Amount));
                dBalance += oCC.Amount;
                if (oCC.Type == ContributionType.Agent || oCC.Type == ContributionType.Fletchers) {
                    if (oCC.Status == ContributionStatus.New && oCC.Amount != 0) {
                        szStatusIcon = String.Format("<input type='checkbox' onclick='updateContributionStatus(1, {0})' title='Check this box once you have notified finance'/>", oCC.DBID);
                    } else if (oCC.Status == ContributionStatus.SentToFinance) {
                        szStatusIcon = String.Format("<input type='checkbox' checked='checked' disabled='disabled' title='This contribution has been flagged to finance' />", oCC.DBID);
                    }
                }
            }

            sbHTML.AppendFormat(@"
                <tr>
                    <td>{0}</td>
                    <td>{1}</td>
                    <td align='right'>{2}</td>
                    <td align='right'>{3}</td>
                    <td align='right'>{4}</td>
                    <td align='center'>{5}</td>
                </tr>", szDate, szType, szCredit, szDebit, Utility.formatMoney(dBalance), szStatusIcon);
        }

        if (sbHTML.Length > 0) {
            dPayments.InnerHtml = String.Format(@"
                <table cellspacing='1' cellpadding='0' width='98%' id='tPaymentList'>
                    <thead class='ListHeader'>
                        <th width='20%'>Date</th>
                        <th width='18%'>Contributor</th>
                        <th width='18%'>Credit</th>
                        <th width='18%'>Debit</th>
                        <th width='18%'>Balance</th>
                        <th width='8%'>Finance</th>
                    </thead>
                    {0}
                    </table>", sbHTML.ToString());
        } else {
            dPayments.InnerHtml = "<p>No payments have been made for this campaign.</p>";
        }
    }

    private void loadContributions(Campaign oCampaign) {
        bool blnAdded = false;
        foreach (CampaignContribution oC in oCampaign.ContributionList) {
            blnAdded = true;
            if (oC.IsPayment || (oC.Type == ContributionType.Agent || oC.Type == ContributionType.Fletchers))
                continue;   //Skip all non-vendor contributions
            sbEndJS.AppendFormat(@"
                addContribution({0}, {1}, '{2}');", oC.DBID, Convert.ToDouble(oC.Amount), Utility.formatDate(oC.Date));
            foreach (ContributionSplit oS in oC.Splits) {
                //intContribID, intSplitID, Amount, Type, Payment)
                sbEndJS.AppendFormat(@"
                    addContributionSplit({0}, {1}, {2}, {3}, '{4}', {5});", oS.DBID, Convert.ToDouble(oS.SplitAmount), (int)oS.SplitType, (int)oS.PaymentOption, Utility.formatDate(oS.DueDate), oS.IsAuctionDate.ToString().ToLower());
            }
        }
        if (!blnAdded)
            dContributionInner.InnerHtml = "No contributions are entered for this campaign.";
        else
            sbEndJS.AppendLine("drawContributions()");
    }

    private void loadActions(Campaign oCampaign) {
        StringBuilder oSB = new StringBuilder();
        int intCount = 0;
        foreach (CampaignAction oA in oCampaign.ActionList) {
            string szReminderDate = "";
            if (oA.ReminderDate != DateTime.MinValue)
                szReminderDate = Utility.formatDate(oA.ReminderDate);
            string szClass = "DGRow even";
            if (intCount % 2 == 1)
                szClass = "DGRow odd";
            intCount++;
            string szStyle = "";
            string szImg = "";
            if (intCount == 1) {
                szStyle = "font-weight: 900";
                szImg = "<img src='../sys_images/alert_small.png' align='left'/>";
            }
            oSB.AppendFormat(@"
                <tr valign='top' onclick='showAction({3})' style='cursor: pointer; {7}' class='{6}'>
                    <td>{8}{0}</td>
                    <td>{1}</td>
                    <td>{2}</td>
                    <td>{5}</td>
                </tr>
                ", Utility.formatDate(oA.Date), oA.Sender, oA.Action, oA.DBID, Utility.EscapeJS(oA.Note),
                 szReminderDate, szClass, szStyle, szImg);
        }
        if (oSB.Length > 0) {
            dNotes.InnerHtml = String.Format(@"
                <table id='tActionList' name='tActionList' cellspacing='1' cellpadding='0' width='99%' class='SelectTable' >
                    <tr class='ListHeader'>
                        <td width='25%'>Date</td>
                        <td width='20%'>Sender</td>
                        <td width='35%'>Action</td>
                        <td width='20%'>Reminder</td>
                    </tr>
                    {0}
                </table>", oSB.ToString());
        } else {
            dNotes.InnerHtml = "There are no actions entered for this campaign";
        }
        if (oCampaign.AgentID == -1) {
            btnAddNote.Visible = false;
        }

        oSB = null;
    }

    private void updatePage() {
        int intContribCount = Convert.ToInt32(hdSplitCount.Value);
        Campaign oC = new Campaign(Convert.ToInt32(hdCampaignID.Value));
        CampaignContribution oCurrContribution;
        oC.Notes = txtNotes.Text;
        if (Utility.isDateTime(txtStartDate.Text)) {
            oC.setCampaignDate(Convert.ToDateTime(txtStartDate.Text));
        }
        oC.Status = ((CampaignStatus)Convert.ToInt32(lstStatus.SelectedValue));
        oC.FocusAgentID = Valid.getInteger("lstFocusAgent");
        oC.DistributionAgentID = Valid.getInteger("lstDistributionAgent");
        oC.updateCampaign();

        for (int intCurrContrib = 0; intCurrContrib < intContribCount; intCurrContrib++) {
            int intContributionDBID = Valid.getInteger("hdDBID_" + intCurrContrib, -1);
            if (Valid.getText("hdModified_" + intCurrContrib, "false", VT.TextNormal) == "false")
                continue; //Nothing has changed so we don't need to do anything

            int intSplitCount = Valid.getInteger("hdSplitCount_" + intCurrContrib, -1);
            DateTime dtDate = Valid.getDate("txtDate_" + intCurrContrib, DateTime.MinValue);
            if (dtDate == DateTime.MinValue)
                continue; // There is no value for this field
            double dAmount = Valid.getMoney("txtAmount_" + intCurrContrib, 0, false);

            //Check to see if we have an existing contribution in the system
            if (intContributionDBID > -1) {
                oCurrContribution = oC.findContributionByDBID(intContributionDBID);
                oCurrContribution.Date = dtDate;
                oCurrContribution.Amount = dAmount;
                oCurrContribution.updateToDB();
                if (oCurrContribution == null)
                    oCurrContribution = oC.addContribution(dtDate, dAmount, false);
            } else {
                oCurrContribution = oC.addContribution(dtDate, dAmount, false);
            }

            for (int CurrSplit = 0; CurrSplit < intSplitCount; CurrSplit++) {
                int intSplitDBID = Valid.getInteger("hdSplitDBID_" + intCurrContrib + "_" + CurrSplit);
                double dSplitAmount = Valid.getMoney("txtSplitAmount_" + intCurrContrib + "_" + CurrSplit, 0, false);
                AmountType oAmountType = (AmountType)Valid.getInteger("lstSplit_" + intCurrContrib + "_" + CurrSplit);
                string szFixedDate = Valid.getText("txtDueDate_" + intCurrContrib + "_" + CurrSplit, "", VT.TextNormal);
                DateTime dtFixedDate = DateTime.MinValue;
                if (szFixedDate != "")
                    dtFixedDate = Convert.ToDateTime(szFixedDate);

                double dCalcAmount = 0.0;
                if (oAmountType == AmountType.Dollar)
                    dCalcAmount = dSplitAmount;
                else
                    dCalcAmount = dAmount * (dSplitAmount / 100.0);
                PaymentOption oPaymentOption = (PaymentOption)Valid.getInteger("lstPaymentOption_" + intCurrContrib + "_" + CurrSplit);
                if (dSplitAmount > 0) {
                    if (intSplitDBID > -1)
                        oCurrContribution.updateSplit(intSplitDBID, dSplitAmount, oAmountType, dCalcAmount, oPaymentOption, oCurrContribution.Date, dtFixedDate, false);
                    else
                        oCurrContribution.addSplit(dSplitAmount, oAmountType, dCalcAmount, oPaymentOption, oCurrContribution.Date, dtFixedDate, false);
                } else {
                    if (intSplitDBID > -1)
                        oCurrContribution.deleteSplit(intSplitDBID, oC.DBID);
                }
            }
        }
        if (intContribCount > 0 && oC.Status == CampaignStatus.New) {
            oC.Status = CampaignStatus.InProgress;
            oC.updateCampaign();
        }
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        updatePage();
        ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }

    protected void btnHide_Click(object sender, EventArgs e) {
        string szSQL = "UPDATE SALE SET STATUSID = 3 WHERE ID = " + hdCampaignID.Value;
        DB.runNonQuery(szSQL);
        ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }

    protected void btnDelete_Click(object sender, EventArgs e) {
        Campaign oC = new Campaign(Convert.ToInt32(hdCampaignID.Value));
        int intContributionDBID = Valid.getInteger("hdDeleteContribID", -1);
        if (intContributionDBID > -1) {
            CampaignContribution oCurrContribution = oC.findContributionByDBID(intContributionDBID);
            oCurrContribution.delete();
            ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
        }
    }

    protected void btnRefresh_Click(object sender, EventArgs e) {
        Campaign oC = new Campaign(Convert.ToInt32(hdCampaignID.Value));
        bool blnSuccess = CampaignImport.refreshProperty(oC);
        if (blnSuccess)
            loadCampaign();
        else
            ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }
}