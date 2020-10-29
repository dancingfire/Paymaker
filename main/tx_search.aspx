<%@ Page Language="c#" Inherits="tx_search" CodeFile="tx_search.aspx.cs" EnableViewState="true" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">
    intHeight = $(window).height() - 100;

    function getFilterValues() {
        $("#hdExpenseIDList").val($("#lstExpense").val());
    }
    $(document).ready(function () {
        createCalendar("txtStartDate");
        createCalendar("txtEndDate");

        createDataTable("gvTXs", true, false, false, intHeight);
        $("#lstUser, #lstExpense").select2();
    });

    function changeDateFilter() {
        szVal = $("#lstDateRange").val();
        if (szVal != "") {
            arRange = szVal.split(':');
            $("#txtStartDate").val(arRange[0]);
            $("#txtEndDate").val(arRange[1]);
        }
    }

    function setDateCustom() {
        $("#lstDateRange").val('');
    }
    </script>
    <style>
        .ui-datepicker-trigger {
            display: none;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self" onsubmit="getFilterValues()">
        <asp:HiddenField ID="hdExpenseIDList" runat="server" />
        <div>
            <div class='ActionPanel'>
                <asp:Panel ID="pPageHeader" class='PageHeader' runat="server" Width="200px">Accounting dashboard</asp:Panel>
            </div>
            <div class='GridFilter' id='dDrigFilter'>
                <div id="spDate" class="FilterPanel">
                    <div class="FilterLabel">Date range</div>

                    <asp:DropDownList ID="lstDateRange" runat="server" CssClass="Entry" Width="100%"></asp:DropDownList>
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="Entry" Width="110"></asp:TextBox>
                    &nbsp;to &nbsp;<asp:TextBox ID="txtEndDate" runat="server" CssClass="Entry" Width="110"></asp:TextBox>
                </div>
                <div id="spUser" runat="server" class="FilterPanel">
                    <asp:Label ID="Label2" CssClass="FilterLabel" runat="server" Text="User">
                    </asp:Label>
                    <asp:ListBox ID="lstUser" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1" />
                </div>
                <div id="spExpense" class="FilterPanel">
                    <asp:Label ID="Label10" CssClass="FilterLabel" runat="server" Text="Expense account">
                    </asp:Label>
                    <asp:ListBox ID="lstExpense" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1"></asp:ListBox>
                </div>
                <div class="FilterPanel">
                    <asp:Label ID="Label1" CssClass="FilterLabel" runat="server" Text="Description">
                    </asp:Label>
                    <asp:TextBox ID="txtDescFilter" runat="server" CssClass="Entry" Width="90%"></asp:TextBox>
                    <br class='Align' />
                </div>
                <div class="FilterPanel" style="padding-top: 20px">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" class="btn btn-primary" />
                </div>
            </div>
            <asp:GridView ID="gvTXs" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvCurrent_RowDataBound" Width="100%" EnableViewState="false" EmptyDataText="No transactions found.">
                <Columns>
                    <asp:BoundField DataField="User" HeaderText="Staff member" />
                    <asp:BoundField DataField="TxDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="FletcherAmount" HeaderText="Fletchers amount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="Account" HeaderText="Account" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:BoundField DataField="Comment" HeaderText="Comment" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>