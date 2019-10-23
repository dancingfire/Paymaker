<%@ Page Language="c#" Inherits="campaign_update" CodeFile="campaign_update.aspx.cs" ValidateRequest="false" EnableViewStateMac="false" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Campaign update</title>
    <script type="text/javascript">
        arContributions = new Array();
        intContributionCount = 0;
        intCurrContribution = 0;
        blnScreenOK = true;
        blnIsDirty = false;

        function addContribution(intDBID, Amount, ContribDate) {
            var o = new Object();
            o.ControlID = parseInt(arContributions.length);
            o.Amount = parseFloat(Amount); ;
            o.DBID = intDBID;
            o.CalcAmount = 0;
            o.ContribDate = ContribDate;
            o.Splits = new Array();
            arContributions[o.ControlID] = o;
            intCurrContribution = o.ControlID;
            intContributionCount = parseInt(arContributions.length);
            drawContributionRow(o);
        }

        function addContributionSplit(intDBID, Amount, Type, Payment, DueDate) {
            var o = new Object();
            o.Amount = Amount;
            o.DBID = intDBID;
            o.Type = Type;
            o.CalcAmount = 0;
            o.Payment = Payment;
            o.DueDate = DueDate;
            o.ControlID = arContributions[intCurrContribution].Splits.length;

            arContributions[intCurrContribution].Splits[o.ControlID] = o;
            drawSplit();
        }

        function outputContrib() {
            szHTML = "";
            for(i in arContributions){
                oC = arContributions[i];
                szHTML += "Contrib " + oC.DBID + " Amount " + oC.Amount + " ControlID " + oC.ControlID + "\r\n";
            }
        }

        function Close() {
            parent.refreshPage();
        }

        function Cancel() {
           parent.closeCampaign(false);
         }

        function drawContributions() {
            for (i in arContributions) {
                oC = arContributions[i];
                intCurrContribution = oC.ControlID;
                 //Draw the contribution row
                $("#txtDate_" + oC.ControlID).val(oC.ContribDate);
                $("#txtAmount_" + oC.ControlID).val(oC.Amount);
                $("#hdDBID_" + oC.ControlID).val(oC.DBID);

                for (s in oC.Splits) {
                    oSplit = oC.Splits[s];
                    //Draw the splits
                    $("#hdSplitDBID_" + oC.ControlID + "_" + oSplit.ControlID).val(oSplit.DBID);
                    $("#txtSplitAmount_" + oC.ControlID + "_" + oSplit.ControlID).val(oSplit.Amount);
                    $("#lstSplit_" + oC.ControlID + "_" + oSplit.ControlID).val(oSplit.Type);
                    $("#lstPaymentOption_" + oC.ControlID + "_" + oSplit.ControlID).val(oSplit.Payment);
                    $("#txtDueDate_" + oC.ControlID + "_" + oSplit.ControlID).val(oSplit.DueDate);
                   changePaymentOption(false, "lstPaymentOption_" + oC.ControlID + "_" + oSplit.ControlID);

                }
                checkCalc();
            }
            disableAllButThis(-1);
        }

        //this remove the % option if the entered value is more than 100
        function removePercentage(elemValueID, elemListID) {
            if (parseFloat($("#" + elemValueID).val()) > 100) {
                $("#" + elemListID + " option[value='1']").remove();
            } else {
                if ($("#" + elemListID + " option").length == 1)
                    $("#" + elemListID).append('<option value="1">%</option>');
            }
        }

        function makeReadOnly() {
            if ($("#hdReadOnly").val() == "true" || $("#hdLocked").val() == "true") {

                $(':input').attr("disabled", true);
                $('.JQHideOnReadOnly').hide();
                $("#btnCancel").removeAttr("disabled").val("Close");
                $("#btnAddNote, #btnMarkActionComplete").removeAttr("disabled");

                if ($("#hdLocked").val() == "false" )
                    $("#btnUpdate").hide()

            }

            //Record is locked because its been exported but admin user can still change status
            if ($("#hdReadOnly").val() == "false" && $("#hdLocked").val() == "true") {
                $("#btnUpdate").removeAttr("disabled");
                $("#lstStatus").removeAttr("disabled");
                $("[id^=__]").removeAttr("disabled");
                $("#hdCampaignID").removeAttr("disabled");
            }
        }

        function highlightTextOnFocus(obj) {
            obj.select();
        }

        function drawContributionRow() {
            disableAllButThis(intCurrContribution);

            if (intContributionCount == 1)
                $("#dContributionInner").html('');

            $items = $("#oTemplate").clone();
            $items[0].id = "dContribution_" + intCurrContribution; //Rename the template div row

            $("[id$='___']", $items).each(function(){
                $(this).attr("id", $(this).attr("id").replace("___", "_" + intCurrContribution)); //Rename all the values for the new row
                $(this).attr("name", $(this).attr("id"));
            });
            $("#dContributionInner").append($items);
            createCalendar("txtDate_" + intCurrContribution);
            toggleAddContribution(false);
            $(".RoundPanel").corner();
            $('#sDeletePanel_' + intCurrContribution).show();
        }

        function toggleAddContribution(blnEnabled) {
            if (blnEnabled) {
                $("#btnInsertContribution").removeAttr("disabled", "").removeAttr("title", "");
                setScreenOK(true);
            } else {
                $("#btnInsertContribution").attr("disabled", "disabled").attr("title", "Please update the previous contribution");
            }
        }

        function addNewContribution() {
            addContribution(-1, 0, null);
            addContributionSplit(-1, 0, '', '', '');
        }

        function addNewSplit() {
            addContributionSplit(-1, 0, '', '');
        }

        function refreshData() {
            $.blockUI();
            $("#btnRefresh").click();
        }

        function drawSplit() {
            oC = arContributions[intCurrContribution];

            oSplit = oC.Splits[oC.Splits.length - 1];
            $items = $("#tPaymentTemplateRow").clone();

            $("[id$='_SPLIT_']", $items).each(function () { //Rename the split objectrs
                $(this).attr("id", $(this).attr("id").replace("_SPLIT_", "_" + intCurrContribution + "_" + oSplit.ControlID)); //Rename all the values for the new row
                $(this).attr("name", $(this).attr("id"));
            });

            $items[0].id = "tRow_" + intCurrContribution + "_" + oSplit.ControlID; //Rename the table row

            $("#tPaymentTemplateRow", $items).attr("id", "tRow_" + intCurrContribution + "_" + oSplit.ControlID);
            $("#tPaymentTemplate_" + intCurrContribution).append($items);

            $("#hdSplitCount_" + intCurrContribution).val(oC.Splits.length);
            changePaymentOption(false, "lstPaymentOption_" + intCurrContribution + "_" + oSplit.ControlID);
        }

        // the szControlID is the name of the payment type list control
        function changePaymentOption(blnUserChange, szControlID) {
            if (blnUserChange) { //Set the TX to be modified
                dirtyContribution();
            }

            szDateControlID =
            intPaymentType = $("#" + szControlID).val();
            szDateFieldID = "txtDueDate_" + szControlID.substr(szControlID.indexOf("_") + 1);
            szDatePanelID = "sDueDate_" + szControlID.substr(szControlID.indexOf("_") + 1);

            if (intPaymentType == 4) {
                $("#" + szDatePanelID).show();
                createCalendar(szDateFieldID);
            } else {
                $("#" + szDatePanelID).hide();
            }
        }
        function dirtyContribution() {
            $("#hdModified_" + intCurrContribution).val('true')
            blnIsDirty = true;
        }

        function refreshPage() {
            szHREF = document.location.href;
            szHREF = szHREF.replace('ActionID', 'a');
            document.location.href = szHREF;
        }

        function copyAmount(oField) {
            if ($("#txtSplitAmount_" + intCurrContribution + "_0").val() == "")
                $("#txtSplitAmount_" + intCurrContribution + "_0").val($(oField).val());
        }

        function checkCalc(blnUserChange) {
            if (blnUserChange) { //Set the TX to be modified
                dirtyContribution();
            }
            oC = arContributions[intCurrContribution];
            oC.Amount = $("#txtAmount_" + intCurrContribution).val();
            oC.CalcAmount = 0;
            intTotal = 0;
            for(s in oC.Splits){
                oSplit = oC.Splits[s];

                oSplit.Amount = $("#txtSplitAmount_" + intCurrContribution + "_" + oSplit.ControlID).val();
                oSplit.Type = $("#lstSplit_" + intCurrContribution + "_" + oSplit.ControlID).val();
                oSplit.Payment = $("#lstPaymentOption_" + intCurrContribution + "_" + oSplit.ControlID).val();

                if (oSplit.Type == 0)
                    oSplit.CalcAmount = parseFloat(oSplit.Amount);
                else
                    oSplit.CalcAmount = oC.Amount * (oSplit.Amount)/100;
                intTotal += oSplit.CalcAmount;
                if(!isNaN(oSplit.CalcAmount))
                    $("#txtCalcAmount_" + intCurrContribution + "_" + oSplit.ControlID).html(parseFloat(oSplit.CalcAmount).toFixed(2));
            }
            oC.CalcAmount = intTotal;
            if(isNaN(parseFloat(intTotal))){
                 showCalcError(oC.ControlID);
             } else {
                $("#tPaymentTotal_" + intCurrContribution).html(parseFloat(intTotal).toFixed(2));
                if(Math.abs(oC.CalcAmount - oC.Amount) < 0.05){
                    showSuccess(oC.ControlID);
                } else {
                    showCalcError(oC.ControlID);
                }
            }
        }

        function completeAction() {
            callWebMethod("../web_services/ws_Paymaker.asmx", "markActionAsCompleted", ["CampaignID", $("#hdCampaignID").val()], actionCompletedSuccess);
        }

        function actionCompletedSuccess() {
            $("#pActionCompleted").show();
        }

        function showSuccess(CalcID) {
            $("#imgContribOK_" + CalcID).show();
            $("#imgContribNotOK_" + CalcID).hide();
            setScreenOK(true);
            toggleAddContribution(true);
        }

        function showCalcError(CalcID) {
            $("#imgContribOK_" + CalcID).hide();
            $("#imgContribNotOK_" + CalcID).show();
            setScreenOK(false);
        }

        function setScreenOK(blnOK) {
            if (blnOK) {
                blnScreenOK = true;
                $("#btnUpdate").removeAttr("disabled");
            } else {
                blnScreenOK = false;
                $("#btnUpdate").attr("disabled", true);
            }
        }

        function enableCalc(CalcID) {
            if ($("#hdReadOnly").val() == "true")
                return;

            disableAllButThis(CalcID);
            intCurrContribution = CalcID;
            $("#dContribution_" + CalcID).find(":input").each(function () {
                $(this).removeAttr("disabled");
            });
            $("#dContribution_" + CalcID).removeClass("EDIT")
                .attr("title", "")
                .unbind();

            $("#sDeletePanel_" + CalcID).show();
        }

        function disableAllButThis(CalcID) {
            for (i in arContributions) {
                if (CalcID != arContributions[i].ControlID)
                    disableCalc(arContributions[i].ControlID);
            }
        }

        function disableCalc(CalcID) {
            //Disable the form controls
            $("#dContribution_" + CalcID).find(":input").each(function () {
                $(this).attr("disabled", true);
            })
            $("#sDeletePanel_" + CalcID).hide();

            toggleAddContribution(true);

            $("#dContribution_" + CalcID).addClass("EDIT")
                .attr("title", "Click to edit")
                .click(function () {
                    if(blnScreenOK)
                        enableCalc(CalcID);
              });

        }
        function deleteContribution() {
            if (confirm("Are you sure that you want to delete this contribution?")) {
                intDBID = arContributions[intCurrContribution].DBID;
                $("#hdDeleteContribID").val(intDBID);
                $("#btnDelete").click();
            }
        }

        function enableAll() {
            for (i in arContributions) {
                $("#dContribution_" + arContributions[i].ControlID).find(":input").each(function () {
                    $(this).removeAttr("disabled");
                });
            }
        }

        function showAction(intID) {
            szPage = "../campaign/action_detail.aspx?intCampaignNoteID=" + intID;
            szParams = "&intCampaignID=" + $("#hdCampaignID").val();

            $('#mAction').on('show.bs.modal', function () {
                $('#fAction').attr("src", szPage + szParams);
            });
            $('#mAction').on('hidden.bs.modal', function () {
                $('#fAction').attr("src", "../blank.html");
                refreshPage()
            });
            $('#mAction').modal({ 'backdrop': 'static' });
            $('#mAction').modal('show').draggable();
        }

        function closeAction() {
            $('#mAction').modal('hide');
        }

        function validatePage(){
            if (blnScreenOK) {
                $("#hdSplitCount").val(intContributionCount);
                enableAll();
                blnIsDirty = false;
                return true;
            }
        }

        function updateContributionStatus(intStatusID, intContributionDBID) {
           callWebMethod("../web_services/ws_Paymaker.asmx", "updateCampaignContributionStatus", ["StatusID", intStatusID, "CampaignContributionID", intContributionDBID]);
        }

        window.onbeforeunload = function (e) {
            if (!blnIsDirty) {
                return;
            }
            var message = "You have modified the page and will lose your changes if you close this page.";
            var e = e || window.event;
            // For IE and Firefox prior to version 4
            if (e) {
                e.returnValue = message;
            }

            // For Safari
            return message;
        };

        $(document).ready(function () {
            $(".RoundPanel").corner();
            makeReadOnly();
            addZebra('tActionList');

           createCalendar("txtAuctionDate");
           createCalendar("txtStartDate");
        });

        function loadPage() {
            if ($("#hdInitialActionID").val() > 0) {
                //Open the initial action
                showAction($("#hdInitialActionID").val());
            }
        }
    </script>
    <style type="text/css">
        .Error {
            border: 1px solid red;
        }

        .ContributionPanel {
            background: #D1B369;
            margin-top: 5px;
            margin-bottom: 5px;
        }

        .EDIT {
            cursor: pointer;
        }

        .EntryPos {
            width: 100px;
        }

        .ui-datepicker-trigger {
            margin-left: 2px;
            padding-top: 2px;
        }

        .TopPanel .Label {
            width: 100px;
        }
    </style>
</head>
<body onload="loadPage();">
    <form id="frmMain" method="post" runat="server" target="_self">
        <asp:HiddenField ID="hdCampaignID" runat="server" Value="-1" />
        <asp:HiddenField ID="hdInitialActionID" runat="server" Value="-1" />
        <asp:HiddenField ID="hdSplitCount" runat="server" Value="-1" />
        <asp:HiddenField ID="hdReadOnly" runat="server" Value="false" />
        <asp:HiddenField ID="hdLocked" runat="server" Value="false" />
        <asp:HiddenField ID="hdDebugMode" runat="server" Value="false" />
        <asp:HiddenField ID="hdDeleteContribID" runat="server" Value="" />
        <input id="hdDifferenceSpiltAmountAndCommissionTotal" type="hidden" value="0" />
        <input id="hdDifferenceUserSpiltAmountAndCommissionTotal" type="hidden" value="0" />

        <div class='ActionPanel' style='width: 100%; float: left; clear: both'>
            <div class='PageHeader' style='float: left; width: 90%'>
                <asp:Label ID="txtCode" CssClass="Normal EntryPos" runat="server" Text="" Style="width: 450px"></asp:Label>
            </div>
            <div style='float: left; width: 23%' class="TopPanel">
                <asp:Label ID="lblSalesDate" CssClass="Label LabelPos" runat="server" Text="Start date"></asp:Label>
                <asp:TextBox ID="txtStartDate" CssClass="Entry EntryPos" runat="server" Text=""></asp:TextBox>
                <br class='Align' />

                <asp:Label ID="Label1" CssClass="Label LabelPos" runat="server" Text="Agent"></asp:Label>
                <asp:Label ID="lblAgent" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="Label4" CssClass="Label LabelPos" runat="server" Text="Office"></asp:Label>
                <asp:Label ID="lblOffice" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="Label3" CssClass="Label LabelPos" runat="server" Text="Status"></asp:Label>
                <asp:DropDownList ID="lstStatus" runat="server" CssClass="Entry EntryPos" Width="120px">
                    <asp:ListItem Text="New" Value="0"></asp:ListItem>
                    <asp:ListItem Text="In-progress" Value="1"></asp:ListItem>
                    <asp:ListItem Text="Completed" Value="100"></asp:ListItem>
                </asp:DropDownList>
                <br class='Align' />
            </div>
            <div style='float: left; width: 20%;' class="TopPanel">
                <asp:Label ID="lblTotalBudget" CssClass="Label LabelPos" runat="server" Text="Total budget"></asp:Label>
                <asp:Label ID="dTotalBudget" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="lblCompanyContribution" CssClass="Label LabelPos" runat="server" Text="Company/agent contribution"></asp:Label>
                <asp:Label ID="dTotalAgentCompanyContribution" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="lblTotalSpent" CssClass="Label LabelPos" runat="server" Text="Total spent"></asp:Label>
                <asp:Label ID="dTotalSpent" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="lblBudgetRemaining" CssClass="Label LabelPos" runat="server" Text="Remaining"></asp:Label>
                <asp:Label ID="lblBudgetRemainingAmount" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />
            </div>
            <div style='float: left; width: 20%' class="TopPanel">
                <asp:Label ID="Label5" CssClass="Label LabelPos" runat="server" Text="Total invoiced"></asp:Label>
                <asp:Label ID="lblInvoiced" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="Label2" CssClass="Label LabelPos" runat="server" Text="Total paid"></asp:Label>
                <asp:Label ID="lblPaidAmount" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="Label6" CssClass="Label LabelPos" runat="server" Text="Owing"></asp:Label>
                <asp:Label ID="lblOwing" CssClass="Entry EntryPos" runat="server" Text=""></asp:Label>
                <br class='Align' />

                <asp:Label ID="Label7" CssClass="Label LabelPos" runat="server" Text="Focus Agent"></asp:Label>
                <asp:DropDownList ID="lstFocusAgent" CssClass="Entry EntryPos" runat="server" />
                <br class='Align' />

                <asp:Label ID="Label9" CssClass="Label LabelPos" runat="server" Text="Distribution Agent"></asp:Label>
                <asp:DropDownList ID="lstDistributionAgent" CssClass="Entry EntryPos" runat="server" />
                <br class='Align' />
            </div>
            <div style='float: left; width: 25%'>
                <asp:TextBox ID="txtNotes" runat="server" CssClass="Entry" Style="width: 100%" TextMode="MultiLine" Height="90px"></asp:TextBox>
            </div>
            <span class='RightActionPanel' style='width: 100px; text-align: right'>
                <asp:Button ID="btnUpdate" CssClass="Button btn" runat="server" Text="Update" UseSubmitBehavior="true" OnClick="btnUpdate_Click" OnClientClick="return validatePage();" /><br />
                <asp:Button ID="btnHide" CssClass="Button btn" runat="server" Text="Hide" OnClick="btnHide_Click" Visible="false" />
                <asp:Button ID="btnDelete" runat="server" Text="Delete" OnClick="btnDelete_Click" CssClass="Invisible" />
                <asp:Button ID="btnCancel" CssClass="Button btn" runat="server" Text="Cancel" OnClientClick="return Cancel();" />
                <asp:Button ID="btnRefresh" CssClass="Invisible" runat="server" Text="Refresh" OnClick="btnRefresh_Click" />

                <asp:Label ID="lblLastImported" CssClass="Entry" runat="server" Text="" Style="color: #BE9D52; float: right; margin-top: 20px"></asp:Label>
            </span>
        </div>
        <div style='width: 100%; float: left' id='dTest'>
            <div class='RoundPanel Normal' style="width: 470px; height: 350px; float: right; margin-top: 10px; clear: right">
                <div class='PageHeader'>Actions</div>
                <div id='dNotes' style='width: 98%; overflow: auto; height: 280px; float: left' runat="server">&nbsp;</div>
                <div style='text-align: center;'>
                    <asp:Button ID="btnAddNote" CssClass="Button btn" runat="server" Text="Add action" OnClientClick="showAction(-1); return false;" UseSubmitBehavior="false" Style='width: 140px' />
                    <asp:Button ID="btnMarkActionComplete" CssClass="Button btn" runat="server" Text="Complete action"
                        OnClientClick="completeAction(); return false;" UseSubmitBehavior="false" Style='width: 140px; margin-left: 20px' />
                    <span id='pActionCompleted' style='display: none; float: left; color: Green; margin-left: 200px'>Completed!</span>
                </div>
            </div>

            <div id="dContributions" runat="server" class='RoundPanel' style="float: left; width: 650px; height: 350px; overflow: auto; margin-top: 10px; margin-right: 10px">
                <div class='PageHeader' style="width: 100%;">Authorities</div>
                <table id='tContributionHeader'>
                    <thead>
                        <th style='width: 120px;' class='ListHeader'>Date
                        </th>
                        <th style='width: 80px;' class='ListHeader'>Amount
                        </th>
                        <th style='width: 450px;' class='ListHeader'>Payment selections
                        </th>
                    </thead>
                </table>
                <div id='dContributionInner' runat="server" style='width: 99%; float: left;'>
                </div>
                <div style='text-align: center; margin-top: 20px;'>
                    <asp:Button ID="btnInsertContribution" CssClass="Button btn" runat="server" Text="Add authority" OnClientClick="addNewContribution(); return false;" UseSubmitBehavior="false" Style='width: 140px' />
                </div>
            </div>
            <div class='RoundPanel Normal' style="width: 470px; height: 200px; float: right; margin-top: 10px; margin-bottom: 10px; clear: right">
                <div class='PageHeader'>Products</div>
                <div id='dProducts' style='width: 98%; overflow: auto; height: 170px; float: left' runat="server"></div>
            </div>
            <div class='RoundPanel Normal' style="width: 650px; height: 200px; float: left; margin-top: 10px;">
                <div class='PageHeader'>Financial history</div>
                <div id='dPayments' style='width: 98%; overflow: auto; height: 170px; float: left' runat="server"></div>
            </div>
        </div>
        <div class='Invisible'>
            This panel is used to create the default contribution
        <div id='oTemplate' class='ContributionPanel RoundPanel'>
            <input id='hdDBID___' type='hidden' value="-1" />
            <input id='hdModified___' type='hidden' value="false" />
            <input id='hdSplitCount___' type='hidden' value="0" />
            <table cellspacing='0' cellpadding='0'>
                <tr valign='top'>
                    <td style='width: 120px;'>
                        <input id='txtDate___' style='width: 90px; margin-right: 3px;' class="Entry" onchange='dirtyContribution()' />
                        <span id='sDeletePanel___' style='display: none'>
                            <a href='javascript: deleteContribution()'>
                                <img src='../sys_images/delete.gif' border='0' alt='Delete this contribution' />Delete</a>
                        </span>
                    </td>
                    <td style='width: 60px;'>
                        <input id='txtAmount___' style='width: 60px' onchange='copyAmount(this); checkCalc(true)' class="Entry" />
                    </td>
                    <td style='width: 470px;'>
                        <div id='tPaymentSplit___' style='float: left; width: 390px'>
                            <table id='tPaymentTemplate___' style='float: left' cellpadding='0' cellspacing='0'>
                            </table>
                            <div style='float: left; font-weight: 800; clear: left; margin-left: 10px;'>
                                <span id='tPaymentTotal___' style='float: left;'></span>
                            </div>
                        </div>
                        <span style='float: right; width: 40px'>
                            <img id='imgContribOK___' src='../sys_images/check.png' style='display: none; float: right' alt='OK' />
                            <img id='imgContribNotOK___' src='../sys_images/x.png' style='display: none; float: right' alt='Not in balance' />
                            <input type='button' id='btnAddSplit___' value="Add" onclick='addNewSplit()' style='float: right' />
                        </span>
                    </td>
                </tr>
            </table>
        </div>

            The row of this table is used to create the table row
        <table style='float: left'>
            <tr id='tPaymentTemplateRow' valign='top'>
                <td width="180">
                    <input id='hdSplitDBID_SPLIT_' type='hidden' value='-1' />
                    <input id='txtSplitAmount_SPLIT_' style='width: 50px' onchange='checkCalc(true)' class="Entry" />
                    <select id='lstSplit_SPLIT_' style='width: 40px' onchange='checkCalc(true)' class="Entry">
                        <option value='0'>$</option>
                        <option value='1'>%</option>
                    </select>
                    <span id='txtCalcAmount_SPLIT_' style='width: 80px; margin-left: 4px;' class="Entry">&nbsp;</span>
                </td>
                <td width="190">
                    <select id='lstPaymentOption_SPLIT_' class="Entry" style='width: 160px; float: left' onchange='changePaymentOption(true, this.id)'>
                        <option value='0'>7 days</option>
                        <option value='1'>14 days</option>
                        <option value='2'>28 days</option>
                        <option value='3'>14 days after invoicing</option>
                        <option value='4'>Fixed date</option>
                    </select>
                    <div id='sDueDate_SPLIT_' style='width: 160px; float: left'>
                        <input id='txtDueDate_SPLIT_' style='width: 136px;' class="Entry" onchange='dirtyContribution()' />
                    </div>
                </td>
            </tr>
        </table>
        </div>
    </form>
</body>
</html>