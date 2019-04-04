<%@ Page Language="c#" Inherits="multiple_tx_update" EnableViewState="True" AutoEventWireup="true" CodeFile="multiple_tx_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Transaction update</title>
    <script type="text/javascript">

        intRow = 0;
        function confirm_delete() {
            return confirm("Are you sure you want to delete this transaction?");
        }

        function cancelPage() {
            if (confirm("Are you sure you want to cancel this page?")) {
                blnAllowClose = true;
                parent.closeTx();
                return true;
            }
            return false;
        }
        
        function recalFletcherContribution(Row) {
            var floatUserAmount = 0;
            var fExGST = parseFloat($.trim($("#lblAmountExGST" + Row).html()));
            //Calc User Amount
            if ($("#lstAmountType" + Row).val() == "1") {//Percentage
                intFletchersAmount = parseFloat($.trim($("#txtFletcherContribution" + Row).val()));
                if (isNaN(intFletchersAmount))
                    intFletchersAmount = 0;
                if (intFletchersAmount == 0) {
                    $("#hdFletcherAmountCalc" + Row).val(0);
                    $("#txtUserAmount" + Row).val(fExGST.toFixed(2));
                } else {
                    floatFletcherAmount = fExGST * parseFloat(intFletchersAmount / 100.0);
                    $("#hdFletcherAmountCalc" + Row).val(parseFloat(floatFletcherAmount).toFixed(2));
                    $("#txtUserAmount" + Row).val(parseFloat(fExGST - floatFletcherAmount).toFixed(2));
                }
            } else {
                //Dollar amount
                $("#hdFletcherAmountCalc" + Row).val(parseFloat($("#txtFletcherContribution" + Row).val()).toFixed(2));
                floatUserAmount = fExGST - parseFloat($("#txtFletcherContribution" + Row).val());
                $("#txtUserAmount" + Row).val(parseFloat(floatUserAmount).toFixed(2));
            }
        }

        function isValid(FieldID) {
            szValue = $.trim($("#" + FieldID + Row).val());
            return !isNaN(parseFloat(szValue) * 1);
        }

        function getExGSTAmount(Row) {
            var floatUserAmount = 0;
            if (!isValid("txtAmount")) {
                alert('Please enter a valid amount.');
                return;
            } else {
                if ($("#chkIncludeGST" + Row).is(':checked')) {
                    $("#lblAmountExGST" + Row).html($("#txtAmount" + Row).val());
                } else {
                    //calc Ex GST Value
                    var valAmount = $.trim($("#txtAmount" + Row).val());
                    $("#lblAmountExGST" + Row).html(parseFloat(valAmount / 1.1).toFixed(2));
                }
                recalFletcherContribution(Row);
            }
        }

        function getSelectedAccount(Row) {
            if ($("#lstType" + Row).val() == "EXPENSE")
                return $("#lstExpenseAccounts" + Row).val();
            else
                return $("#lstIncomeAccounts" + Row).val();
        }

        intAjaxRow = 0;
        function getBudgetAmount(Row) {
            intAccountID = getSelectedAccount(Row);
            blnIsExpense = $("#lstType" + Row).val() == "EXPENSE";
            if (intAccountID != "-1" && $("lstUserID" + Row).val != "-1") {
                intAjaxRow = Row;
                callWebMethod("../web_services/ws_Paymaker.asmx", "getBudgetAmount", ["AccountID", intAccountID, "UserID", $("#lstUserID" + Row).val(), "blnIsExpense", blnIsExpense], getBudgetAmountSuccess);
            }
        }

         function getUserGLSuccess(val) {
            Row = intAjaxRow;
            $("#txtJobCredit" + Row).val(val).select2().trigger('change');
            $("#txtJobDebit" + Row).val(val).select2().trigger('change');
        }

        function getBudgetAmountSuccess(szResult) {
            // Result format: BudgetAmount***CreditGLCode***DebitGLCode***AccountJobCode***OfficeJobCode
            Row = intAjaxRow;
            arResult = szResult.split("***");
            if ($("#chkOverrideCodes" + Row).is(':checked'))
                return;
            $("#lblBudgetAmount" + Row).html(arResult[0]);
            if ($("#lstType" + Row).val().toUpperCase() == "EXPENSE") {
                $("#txtGLCredit" + Row).val(arResult[1]);     //User Credit GL code
                $("#txtJobCredit" + Row).val(arResult[2]);    // User's office job code
                $("#txtGLDebit" + Row).val(arResult[3]);      //Account Debit GL code
                $("#txtJobDebit" + Row).val(arResult[4]);     //Account job code
            } else {
                $("#txtGLDebit" + Row).val(arResult[1]);     //User Credit GL code
                $("#txtJobDebit" + Row).val(arResult[2]);    // User's office job code
                $("#txtGLCredit" + Row).val(arResult[3]);      //Account Debit GL code
                $("#txtJobCredit" + Row).val(arResult[4]);     //Account job code
            }
        }

        function validateRow(blnNewRow) {
            //Check to see if the row has been deleted
            if ($("#lstUserID" + intRow).length == 0)
                return true;

            if ($("#lstUserID" + intRow).val() == "-1") {
                alert("Please select a user.");
                return false;
            } else if (getSelectedAccount(intRow) == -1) {
                alert("Please select an account.");
                return false;
            } else if (!isValid("txtAmount")) {
                alert('Please enter a valid amount.');
                $("#txtAmount" + intRow).focus();
                return false;
            }
            if (blnNewRow)
                createTx();
            return true;
        }

        function checkValidation(Row) {
            $("#txtFletcherContribution" + Row).removeClass("numbersOnly").removeClass("percent").unbind("keypress");
            if ($("#lstAmountType" + Row).val() == "1") {//"%" is selected
                $("#txtFletcherContribution" + Row).addClass("percent");
            } else {
                $("#txtFletcherContribution" + Row).addClass("numbersOnly");
            }
            addValidation(Row);
        }

        window.onbeforeunload = function () {
            if (blnAllowClose == false) {
                return "You have made changes on this page that you have not yet confirmed. If you navigate away from this page you will lose your unsaved changes";
            }
        }

        function showAgentTXHistory(Row) {
            intUserID = $("#lstUserID" + Row).val();
            if (intUserID == -1) {
                alert("Please select an agent first.");
                return false;
            }
            intAccountID = getSelectedAccount(Row);

            if (intAccountID == "-1") {
                alert("Please select an account first.");
                return false;
            }
            callWebMethod("../web_services/ws_Paymaker.asmx", "getAccountHistory", ["AccountID", intAccountID, "UserID", $("#lstUserID" + Row).val()], getAccountHistorySuccess);
        }

        function getAccountHistorySuccess(szHTML) {
            $("#dTXHistory").html(szHTML);
            $('#mTXHistory').modal('show');
        }

        function closeTXHistory() {
            $("#dTXHistory").html("");
            $('#mTXHistory').modal('hide');
            return false;
        }

        function showAccountList(blnGetAmount, Row) {
            if ($("#lstType" + Row).val() == "EXPENSE") {
                $("#lstExpenseAccounts" + Row).show();
                $("#lstIncomeAccounts" + Row).hide();
            } else {
                $("#lstExpenseAccounts" + Row).hide();
                $("#lstIncomeAccounts" + Row).show();
            }
            if (blnGetAmount)
                getBudgetAmount(Row);
        }

        function createTx() {
            intRow += 1;
            $("#hdTXCount").val(intRow);
            szHTML = $("#tTemplate").clone(true).html();
            szHTML = szHTML.replace(/_ROWNUM/g, intRow);
            szHTML = szHTML.replace("tTemplate", "r" + intRow);
            $('#tTX > tbody:last').append("<tr id='r" + intRow + "' valign='middle' style='height: 25px'>" + szHTML + "</tr>");
            checkValidation(intRow);
            addValidation(intRow);
            $("#lstType" + intRow).bind('change', function (e) { showAccountList(true, getID(e.target)); });
            $("#lstAmountType" + intRow).bind('change', function (e) { recalFletcherContribution(getID(e.target)); });
            $("#lstIncomeAccounts" + intRow).bind('change', function (e) { getBudgetAmount(getID(e.target)); });
            $("#lstExpenseAccounts" + intRow).bind('change', function (e) { getBudgetAmount(getID(e.target)); });
            $("#lstUserID" + intRow).bind('change', function (e) {
                getBudgetAmount(getID(e.target));
                 intAjaxRow = getID(e.target);
                callWebMethod("../web_services/ws_Paymaker.asmx", "getUserGLSubAccount", ["UserID", $("#lstUserID" + intRow).val()], getUserGLSuccess);
            });
            $("#txtFletcherContribution" + intRow).bind('change', function (e) { getExGSTAmount(getID(e.target)); });
            $("#txtAmount" + intRow).bind('change', function (e) { getExGSTAmount(getID(e.target)); });
            $("#chkIncludeGST" + intRow).bind('change', function (e) { getExGSTAmount(getID(e.target)); });
            $("#txtGLCredit" + intRow).bind('change', function (e) { lockAccounts(getID(e.target)); });
            $("#txtGLDebit" + intRow).bind('change', function (e) { lockAccounts(getID(e.target)); });
            $("#txtJobCredit" + intRow).bind('change', function (e) { lockAccounts(getID(e.target)); });
            $("#txtJobDebit" + intRow).bind('change', function (e) { lockAccounts(getID(e.target)); });
            $("#imgSearch" + intRow).bind('click', function (e) { showAgentTXHistory(getID(e.target)); });
            $("#txtJobCredit" + intRow).select2({ tags: true });
            $("#txtJobDebit" + intRow).select2({ tags: true });
          $(".select2-selection").on("focus", function () {
                $(this).parent().parent().prev().select2("open");
            });
            $("#r" + intRow + ".Entry").focus(function () {
                // only select if the text has not changed
                if (this.value == this.defaultValue) {
                    this.select();
                }
            });

            $("#r" + intRow).show();
            $("#lstUserID" + intRow).focus().focus();
        }

        function getID(obj) {
            id = $(obj).prop("id");
            if (isNaN(parseInt(id.slice(-2))))
                return id.slice(-1);
            else
                return id.slice(-2);
        }

        function checkDelete(intID) {
            if (confirm("Are you sure you want to remove this transaction?")) {
                szDelIDs = $("#hdSkipIDs").val();
                if (szDelIDs != "")
                    szDelIDs += ",";
                szDelIDs += intID;
                $("#hdSkipIDs").val(szDelIDs);
                $("#r" + intID).remove();
            }
        }

        function lockAccounts(intRow) {
            $("#chkOverrideCodes" + intRow).attr("checked", true);
        }

        function addListItem(intID, szTitle) {
            var szInnerHtml = $('.TxCategoryList').html();
            $('.TxCategoryList').html(szInnerHtml + '<option value="' + intID + '">' + szTitle + '</option>');
        }

        function loadPage() {
            createCalendar("txtTxDate");
            createTx();
            $('form').submit(function () {
                blnAllowClose = true;
                window.onbeforeunload = null;
            });
              
        }
    </script>
    <style>
        .ui-datepicker-trigger {
            display: none;
        }
    </style>
</head>
<body onload='loadPage()' style="margin: 0px">
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdSkipIDs" runat="server" />
        <asp:HiddenField ID="hdTXCount" runat="server" />
        <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos">Date</asp:Label>
        <asp:TextBox CssClass="Entry EntryPos" ID="txtTxDate" runat="server" Text=""></asp:TextBox>
        <br class="Align" />

        <table id="tTX" style="table-layout: fixed; width: 100%">
            <thead>
                <tr class="TableHeader">
                    <th style="width: 8%">User</th>
                    <th style="width: 6%">Type</th>
                    <th style="width: 7%">Account</th>
                    <th style="width: 4%">Budget amount</th>
                    <th style="width: 5%">Total amount</th>
                    <th style="width: 4%">Exclude GST</th>
                    <th style="width: 5%">Total amount (ex GST)</th>
                    <th style="width: 8%">Fletcher's contrib</th>
                    <th style="width: 5%">User amount</th>
                    <th style="width: 5%">Credit GL</th>
                    <th style="width: 7%">Credit Job</th>
                    <th style="width: 5%">Debit  GL</th>
                    <th style="width: 7%">Debit Job</th>
                    <th style="width: 5%">Override codes</th>
                    <th style="width: 8%">Category</th>
                    <th style="width: 11%">Comments</th>
                </tr>
            </thead>
            <tbody>
                <tr id='tTemplate' style="display: none; vertical-align: top" valign="top">
                    <td>
                        <asp:DropDownList ID="lstUserID_ROWNUM" runat="server" CssClass="Entry EntryPos" Width="90%" TabIndex="100"></asp:DropDownList>
                        <asp:HiddenField ID="hdFletcherAmountCalc_ROWNUM" runat="server" />
                    </td>
                    <td>
                        <asp:DropDownList ID="lstType_ROWNUM" runat="server" CssClass="Entry EntryPos" onchange="checkValidation();" Style="width: 90%" TabIndex="100">
                            <asp:ListItem Text="Expense" Value="EXPENSE"></asp:ListItem>
                            <asp:ListItem Text="Income" Value="INCOME"></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:DropDownList ID="lstIncomeAccounts_ROWNUM" runat="server" CssClass="Entry EntryPos" Width="75%" Style="display: none" TabIndex="100"></asp:DropDownList>
                        <asp:DropDownList ID="lstExpenseAccounts_ROWNUM" runat="server" CssClass="Entry EntryPos" Width="75%" TabIndex="100"></asp:DropDownList>
                        <img src='../sys_images/search.png' id="imgSearch_ROWNUM" style="float: left; margin-left: 3px" width="16" height="16" title="View the last 3 months of transactions for this agent for this account" />
                    </td>
                    <td>
                        <asp:Label ID="lblBudgetAmount_ROWNUM" runat="server" CssClass="Entry EntryPos" Width="95%"></asp:Label>
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry EntryPos numbersOnly" ID="txtAmount_ROWNUM" runat="server" Text="" Width="95%" TabIndex="100"></asp:TextBox>
                    </td>
                    <td style="text-align: center">
                        <asp:CheckBox ID="chkIncludeGST_ROWNUM" CssClass="Entry" runat="server" Text="" Width="20" TabIndex="100" />
                    </td>
                    <td>
                        <asp:Label ID="lblAmountExGST_ROWNUM" runat="server" CssClass="Entry EntryPos" Width="60px"></asp:Label>
                    </td>
                    <td style="width: 90px">
                        <asp:TextBox ID="txtFletcherContribution_ROWNUM" runat="server" CssClass="Entry EntryPos" Style="width: 35px;" TabIndex="100"></asp:TextBox>
                        <asp:DropDownList ID="lstAmountType_ROWNUM" runat="server" CssClass="Entry EntryPos" onchange="checkValidation();" Style="width: 35px" TabIndex="100">
                            <asp:ListItem Text="%" Value="1"></asp:ListItem>
                            <asp:ListItem Text="$" Value="0"></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry EntryPos" ID="txtUserAmount_ROWNUM" runat="server" Text="" Width="60px" TabIndex="100"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry EntryPos" ID="txtGLCredit_ROWNUM" runat="server" Text="" Width="55px" TabIndex="100"></asp:TextBox>
                    </td>
                    <td>
                        <asp:DropDownList CssClass="Entry EntryPos" ID="txtJobCredit_ROWNUM" runat="server"  Width="100%" TabIndex="100"></asp:DropDownList>
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry EntryPos" ID="txtGLDebit_ROWNUM" runat="server" Text="" Width="55px" TabIndex="100"></asp:TextBox>
                    </td>
                    <td>
                        <asp:DropDownList CssClass="Entry EntryPos" ID="txtJobDebit_ROWNUM" runat="server"  Width="100%" TabIndex="100"></asp:DropDownList>
                    </td>
                    <td>
                        <asp:CheckBox ID="chkOverrideCodes_ROWNUM" CssClass="Entry EntryPos" runat="server" Width="30px" TabIndex="100" />
                    </td>
                    <td>
                        <asp:DropDownList ID="lstCategory_ROWNUM" runat="server" CssClass="Entry EntryPos TxCategoryList" Width="90%" TabIndex="100"></asp:DropDownList>
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry EntryPos" ID="txtComment_ROWNUM" runat="server" Text="" Width="110px" TabIndex="100" TextMode="MultiLine" Rows="2"></asp:TextBox>
                        <img src="../sys_images/delete.gif" title="Click to remove this transaction" style="margin-left: 3px; cursor: pointer" onclick="checkDelete('_ROWNUM'); " />
                    </td>
                </tr>
            </tbody>
        </table>

        <div id="mTXHistory" class="modal fade">
            <div class="modal-dialog" style='width: 490px;'>
                <div class="modal-content" style="width: 590px; height: 350px">
                    <div class="modal-header">
                        <h4 class="modal-title">Transaction history</h4>
                    </div>
                    <div class="modal-body" style="width: 585px; height: 300px; overflow: scroll;">
                        <div id="dTXHistory" style="float: left; width: 78%">
                        </div>

                        <div class='RightPanel' style="width: 20%; text-align: right">

                            <asp:Button ID="Button2" Style="width: 85px;" runat="server" Text="Close" OnClientClick="return closeTXHistory();"
                                CssClass="Button" CausesValidation="False" TabIndex="100" UseSubmitBehavior="false"></asp:Button>
                        </div>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class='LeftPanel TableHeader' style="width: 98%; margin-top: 20px; padding: 10px">
            <asp:Button ID="btnUpdate" runat="server" Text="Add New" CssClass="Button btn" CausesValidation="False" TabIndex="101" OnClientClick="return validateRow(true);" UseSubmitBehavior="false"
                Style="margin-left: 20px"></asp:Button>
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="Button btn"
                CausesValidation="False" TabIndex="110" OnClientClick="return cancelPage();" UseSubmitBehavior="false"></asp:Button>

            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="Button btn" CausesValidation="False" TabIndex="121" OnClick="btnUpdate_Click"
                OnClientClick="return validateRow(false)" />
            <asp:Button ID="btnNewCategory" runat="server" Text="Add category" CssClass="Button btn" TabIndex="122" Style="float: right;"
                OnClientClick="parent.addTxCategory(); return false;" />
        </div>
    </form>
</body>
</html>