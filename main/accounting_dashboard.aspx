<%@ Page Language="c#" Inherits="accounting_dashboard" CodeFile="accounting_dashboard.aspx.cs" EnableViewState="true" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">
        intHeight = $(window).height() - 100;

        function viewMultipleTx() {
            $('#mTx').on('show.bs.modal', function () {
                $('#fTx').attr("src", "multiple_tx_update.aspx?IsPopup=true");
            });
            $('#mTx').modal({ 'backdrop': 'static' });

            var winHeight = $(window).height();

            $("#mTx .modal-dialog").css({ width: '99%' });
            $("#mTx .modal-body").css({ height: winHeight - 100 });
            $("#fTx").css({ height: winHeight - 110 });
            $('#mTx').modal('show');

            return false;
        }

        function viewTemplateTx() {
            $('#mTx').on('show.bs.modal', function () {
                $('#fTx').attr("src", "templated_tx_update.aspx?IsPopup=true");
            });
            $('#mTx').modal({ 'backdrop': 'static' });

            var winHeight = $(window).height();

            $("#mTx .modal-dialog").css({ width: '99%' });
            $("#mTx .modal-body").css({ height: winHeight - 100 });
            $("#fTx").css({ height: winHeight - 110 });
            $('#mTx').modal('show');

            return false;
        }

        function addTxCategory() {
            $('#mAddTxCategory').on('show.bs.modal', function () {
                $('#fAddTxCategory').attr("src", "../admin/list_update.aspx?IsPopup=true&intListTypeID=10&intItemID=-1");
            });
            $('#mAddTxCategory').on('hidden.bs.modal', function () {
                $('#fAddTxCategory')[0].contentWindow.removeValidation();
            });

            $('#mTx').hide();
            $('#mAddTxCategory').modal('show');
            return false;
        }

        function closeList(intID, szTitle) {
            if (intID) {
                $('#fTx')[0].contentWindow.addListItem(intID, szTitle);
            }
            closeAddTxCategory();
            $('#mTx').show();
        }

        $(document).ready(function () {
            createDataTable("gvTXs", true, false, false, intHeight);

            // reset size for 'Insert' single
            $('#mTx').on('hidden.bs.modal', function () {
                window.removeValidation();
                $("#mTx .modal-dialog").css({ width: 630 });
                $("#mTx .modal-body").css({ height: 600 });
                $("#fTx").css({ height: 590 });
            });

            $("#lstPayPeriod").select2();
        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self">
        <div style="width: 99%">
          
            <div class='GridFilter' id='dDrigFilter' style="padding: 10px">
                 <span class='RightActionPanel' style="width: 400px">
                    <asp:Button ID="btnInsert" runat="server" Text="Insert" CssClass="Button btn" OnClientClick=" return viewTx(-1);" UseSubmitBehavior="false" Style="margin-left: 10px; padding:4px; padding-left: 6px; padding-right: 6px;"/>
                    <asp:Button ID="btnInsertMultiple" runat="server" Text="Multiple" CssClass="Button btn" OnClientClick=" return viewMultipleTx();" UseSubmitBehavior="false" Style="margin-left: 10px; padding:4px; padding-left: 6px; padding-right: 6px;" />
                    <asp:Button ID="btnTemplated" runat="server" Text="Recurring" CssClass="Button btn" OnClientClick=" return viewTemplateTx();" UseSubmitBehavior="false" Style="margin-left: 10px; padding:4px; padding-left: 6px; padding-right: 6px;" />
                </span>

                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" OnClick="btnRefresh_Click" Style="display: none" />
                <span class='FilterPanel'>
                    <asp:Label ID="lblPropertyFilter" class='Label' runat="server" Text="Pay period"></asp:Label>

                    <asp:DropDownList ID="lstPayPeriod" runat="server" CssClass="Edit" AutoPostBack="true" Width="150">
                        <asp:ListItem Text="All..." Value=""></asp:ListItem>
                    </asp:DropDownList>
                </span>
            </div>
            <asp:GridView ID="gvTXs" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvCurrent_RowDataBound" Width="100%" EnableViewState="false">
                <Columns>
                    <asp:BoundField DataField="User" HeaderText="Staff member" />
                    <asp:BoundField DataField="TxDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="FletcherAmount" HeaderText="Fletchers amount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="Account" HeaderText="Account" />
                    <asp:BoundField DataField="CATEGORY" HeaderText="Category" />
                    <asp:BoundField DataField="Comment" HeaderText="Comment" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>