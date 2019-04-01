<%@ Page Language="c#" Inherits="tx_update" EnableViewState="True" AutoEventWireup="true" CodeFile="tx_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Transaction update</title>
    <script type="text/javascript">
        function confirm_delete() {
            return confirm("Are you sure you want to delete this transaction?");
        }

        function cancelPage() {
            parent.closeTx();
            return false;
        }

        function recalFletcherContribution() {
            var floatUserAmount = 0;
            var fExGST = parseFloat($.trim($("#lblAmountExGST").html()));
            //Calc User Amount
            if ($("#lstAmountType").val() == "1") {//Percentage
                intFletchersAmount = parseFloat($.trim($("#txtFletcherContribution").val()));
                if (isNaN(intFletchersAmount))
                    intFletchersAmount = 0;

                if (intFletchersAmount == 0) {
                    $("#hdFletcherAmountCalc").val(0);
                    $("#txtUserAmount").val(fExGST.toFixed(2));
                } else {
                    floatFletcherAmount = fExGST * parseFloat(intFletchersAmount / 100.0);
                    $("#hdFletcherAmountCalc").val(parseFloat(floatFletcherAmount).toFixed(2));
                    $("#txtUserAmount").val(parseFloat(fExGST - floatFletcherAmount).toFixed(2));
                }
            } else {
                //Dollar amount
                $("#hdFletcherAmountCalc").val(parseFloat($("#txtFletcherContribution").val()).toFixed(2));
                floatUserAmount = fExGST - parseFloat($("#txtFletcherContribution").val());
                $("#txtUserAmount").val(parseFloat(floatUserAmount).toFixed(2));
            }
        }

        function getExGSTAmount() {
            var floatUserAmount = 0;
            if (isNaN($.trim($("#txtAmount").val()) * 1)) {
                alert('Please enter a valid amount.');
                return;
            } else {
                if ($("#chkIncludeGST").is(':checked')) {
                    $("#lblAmountExGST").html($("#txtAmount").val());
                } else {
                    //calc Ex GST Value
                    var valAmount = $.trim($("#txtAmount").val());
                    $("#lblAmountExGST").html(parseFloat(valAmount / 1.1).toFixed(2));
                }
                recalFletcherContribution();
            }
        }

        function getSelectedAccount() {
            if ($("#rbExpense").is(':checked'))
                return $("#lstExpenseAccounts").val();
            else
                return $("#lstIncomeAccounts").val();
        }

        function getBudgetAmount() {
            intAccountID = getSelectedAccount();
            blnIsExpense = $("#rbExpense").is(':checked');
            if (intAccountID != "-1" && $("lstUserID").val != "-1") {
                callWebMethod("../web_services/ws_Paymaker.asmx", "getBudgetAmount", ["AccountID", intAccountID, "UserID", $("#lstUserID").val(), "blnIsExpense", blnIsExpense], getBudgetAmountSuccess);
            }
        }

        function getBudgetAmountSuccess(szResult) {
            // Result format: BudgetAmount***CreditGLCode***DebitGLCode***AccountJobCode***OfficeJobCode
            arResult = szResult.split("***");
            $("#lblBudgetAmount").html(arResult[0]);
            if ($("#chkOverrideCodes").is(':checked'))
                return;
            $("#lblBudgetAmount").html(arResult[0]);
            if ($("#rbExpense").is(':checked')) {
                $("#txtGLCredit").val(arResult[1]);     //User Credit GL code
                $("#txtJobCredit").val(arResult[2]);    // User's office job code
                $("#txtGLDebit").val(arResult[3]);      //Account Debit GL code
                $("#txtJobDebit").val(arResult[4]);     //Account job code
            } else {
                $("#txtGLDebit").val(arResult[1]);     //User Credit GL code
                $("#txtJobDebit").val(arResult[2]);    // User's office job code
                $("#txtGLCredit").val(arResult[3]);      //Account Debit GL code
                $("#txtJobCredit").val(arResult[4]);     //Account job code
            }
        }

        function validatePage() {
            if ($("#lstUserID").val() == "-1") {
                alert("Please select a user.");
                return false;
            } else if (getSelectedAccount() == -1) {
                alert("Please select an account.");
                return false;
            }
            return true;
        }

        function checkValidation() {
            $("#txtFletcherContribution").removeClass("numbersOnly").removeClass("percent").unbind("keypress");
            if ($("#lstAmountType").val() == "1") {//"%" is selected
                $("#txtFletcherContribution").addClass("percent");
            } else {
                $("#txtFletcherContribution").addClass("numbersOnly");
            }
            addValidation();
        }

        $(document).ready(function () {
            createCalendar("txtTxDate");
            checkValidation();
            addValidation();
            $("#lstAmountType").unbind('change').bind('change', function () { recalFletcherContribution(); });
            $("#lstIncomeAccounts").unbind('change').bind('change', function () { getBudgetAmount(); });
            $("#lstExpenseAccounts").unbind('change').bind('change', function () { getBudgetAmount(); });
            $("#txtFletcherContribution").bind('change', function () { getExGSTAmount(); });
            $("#txtAmount").unbind('change').bind('change', function () { getExGSTAmount(); });
            $("#txtJobCredit, #txtJobDebit").select2({
                tags: true
            });
            $("#lstUserID").select2().change(function () { getBudgetAmount(); });

            $(".select2-selection").on("focus", function () {
                $(this).parent().parent().prev().select2("open");
            });
        });

        function showAccountList(blnGetAmount) {
            if ($("#rbExpense").is(':checked')) {
                $("#pExpense").show();
                $("#pIncome").hide();
            } else {
                $("#pExpense").hide();
                $("#pIncome").show();
            }
            if (blnGetAmount)
                getBudgetAmount();
        }

        function lockAccounts() {
            $("#chkOverrideCodes").attr("checked", true);
        }

        function checkForReadOnly() {
            if ($("#hdReadOnly").val() == "true") {
                $("[id*=btnDelete_SE]").each(function () {
                    $(this).hide();
                });

                $(':input:select').attr("disabled", true);
                $('.JQHideOnReadOnly').hide();
                $("#btnCancel").removeAttr("disabled");
            }
        }

        function doLoad() {
            showAccountList(false)
            checkForReadOnly();
        }
    </script>
    <style>
        .EntryPos {
            width: 250px;
        }
    </style>
</head>
<body onload='doLoad()' style="margin: 0px" id="txBody">
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdTXID" runat="server" />
        <asp:HiddenField ID="hdDateUsed" runat="server" />
        <asp:HiddenField ID="hdReadOnly" runat="server" />
        <asp:HiddenField ID="hdFletcherAmountCalc" runat="server" Value="" />
        <div style="width: 500px; float: left">
            <asp:Label ID="Label4" runat="server" CssClass="Label LabelPos">User</asp:Label>
            <asp:DropDownList ID="lstUserID" runat="server" CssClass="Entry EntryPos">
            </asp:DropDownList>
            <br class="Align" />
            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos">Date</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtTxDate" runat="server" Text=""></asp:TextBox>
            <br class="Align" />
            <asp:Label ID="Label8" runat="server" CssClass="Label LabelPos">Tx type</asp:Label>
            <div class='EntryPos TableHeader' style='text-align: left; padding-right: 10px; padding-bottom: 3px'>
                <asp:RadioButton ID="rbExpense" GroupName="Account" runat="server" Text="Expense"
                    Style='float: left'></asp:RadioButton>
                <asp:RadioButton ID="rbIncome" GroupName="Account" runat="server" Text="Income  "
                    Style='float: right'></asp:RadioButton>
            </div>
            <br class="Align" />
            <asp:Panel ID="pIncome" runat="server">
                <asp:Label ID="Label9" runat="server" CssClass="Label LabelPos">Account</asp:Label>
                <asp:DropDownList ID="lstIncomeAccounts" runat="server" CssClass="Entry EntryPos">
                </asp:DropDownList>
            </asp:Panel>
            <asp:Panel ID="pExpense" runat="server">
                <asp:Label ID="Label7" runat="server" CssClass="Label LabelPos">Account</asp:Label>
                <asp:DropDownList ID="lstExpenseAccounts" runat="server" CssClass="Entry EntryPos">
                </asp:DropDownList>
            </asp:Panel>
            <br class="Align" />
            <asp:Label ID="Label5" runat="server" CssClass="Label LabelPos">Budget amount</asp:Label>
            <asp:Label ID="lblBudgetAmount" runat="server" CssClass="Entry EntryPos"></asp:Label>
            <br class="Align" />
            <asp:Label ID="lblAmount" runat="server" CssClass="Label LabelPos">Total amount</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos numbersOnly" ID="txtAmount" runat="server"
                Text="" Width="170"></asp:TextBox>
            <asp:CheckBox ID="chkIncludeGST" CssClass="Entry" runat="server" Text="Exclude GST"
                Width="100" Style='float: left; clear: right' />
            <br class="Align" />
            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos">Total amount (ex GST)</asp:Label>
            <asp:Label ID="lblAmountExGST" runat="server" CssClass="Entry EntryPos"></asp:Label>
            <br class="Align" />
            <asp:Label ID="lblFletcher" runat="server" CssClass="Label LabelPos">Fletcher's contribution</asp:Label>
            <asp:TextBox ID="txtFletcherContribution" runat="server" CssClass="Entry EntryPos"
                MaxLength="80" Text="50" Style="width: 210px; margin-right: 5px"></asp:TextBox>
            <asp:DropDownList ID="lstAmountType" runat="server" CssClass="Entry EntryPos" onchange="checkValidation();"
                Style="width: 40px">
                <asp:ListItem Text="%" Value="1"></asp:ListItem>
                <asp:ListItem Text="$" Value="0"></asp:ListItem>
            </asp:DropDownList>
            <br class="Align" />
            <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos">User amount</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtUserAmount" runat="server" Text=""></asp:TextBox>
            <br class="Align" />
            <table style='width: 410px; float: left; table-layout: fixed' cellpadding="2">
                <tr>
                    <td width="125">&nbsp;
                    </td>
                    <td width="145" class="TableHeader">GL code
                    </td>
                    <td width="140" class="TableHeader">Job
                    </td>
                </tr>
                <tr>
                    <td class="Label">Credit
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry" ID="txtGLCredit" runat="server" Text="" Style='width: 100%'></asp:TextBox>
                    </td>
                    <td>
                        <asp:DropDownList ID="txtJobCredit" CssClass="Entry" runat="server" Style='width: 90%'></asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td class="Label">Debit
                    </td>
                    <td>
                        <asp:TextBox CssClass="Entry" ID="txtGLDebit" runat="server" Text="" Style='width: 100%'></asp:TextBox>
                    </td>
                    <td>
                        <asp:DropDownList ID="txtJobDebit" CssClass="Entry" runat="server" Style='width: 90%'></asp:DropDownList>
                   </td>
                </tr>
            </table>
            <asp:Label ID="Label10" runat="server" CssClass="Label LabelPos">Override codes</asp:Label>
            <asp:CheckBox ID="chkOverrideCodes" CssClass="Entry EntryPos" runat="server" />
            <asp:Label ID="Label11" runat="server" CssClass="Label LabelPos">Category</asp:Label>
            <asp:DropDownList ID="lstCategory" runat="server" CssClass="Entry EntryPos" />
            <br class="Align" />
            <asp:Label ID="Label6" runat="server" CssClass="Label LabelPos">Comment</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtComment" runat="server" Text="" TextMode="MultiLine"
                Rows="10"></asp:TextBox>
            <br class="Align" />
        </div>
        <div class='LeftPanel' style="width: 90px">
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn" Width="80"
                CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click" OnClientClick="return validatePage()"
                Style="margin-bottom: 5px"></asp:Button>
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="Button btn" Width="80"
                CausesValidation="False" TabIndex="300" OnClientClick="return cancelPage();"
                Style="margin-bottom: 15px"></asp:Button>
            <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="cancel" CssClass="Button btn" Width="80"
                CausesValidation="False" TabIndex="200" OnClick="btnDelete_Click"></asp:Button>
        </div>
    </form>
</body>
</html>