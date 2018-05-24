<%@ Page Language="c#" Inherits="management_dashboard" CodeFile="management_dashboard.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">

    function refreshPage() {
        document.frmMain.submit();
    }
    </script>
    <style type="text/css">
        #gvCurrent tbody tr:hover {
            background-color: #DDC691 !important;
            cursor: pointer;
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">

        <div class='ActionPanel'>
            <asp:HiddenField ID="hdRestoreCampaignID" runat="server" />
            <asp:Label ID="lblHeader" runat="server" Text="Management dashboard" Style='float: left'></asp:Label>
        </div>

        <table>
            <tr>
                <td width='60%'>Total current campaigns</td>
                <td width='40%' align='right'>
                    <asp:Label ID="lblTotalCampaigns" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value of current campaigns</td>
                <td align='right'>
                    <asp:Label ID="lblTotalValue" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value of pre-payments</td>
                <td align='right'>
                    <asp:Label ID="lblTotalPrePayment" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value not invoiced</td>
                <td align='right'>
                    <asp:Label ID="lblTotalNotInvoiced" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value invoiced</td>
                <td align='right'>
                    <asp:Label ID="lblTotalInvoiced" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value due in current terms</td>
                <td align='right'>
                    <asp:Label ID="lblTotalCurrentOwing" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value overdue by 30 days or more</td>
                <td align='right'>
                    <asp:Label ID="lblTotalOverDue" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Number campaigns exceeding authority</td>
                <td align='right'>
                    <asp:Label ID="lblNumberExceedingAuthority" class='Label' runat="server"></asp:Label></td>
            </tr>
            <tr>
                <td>Total $ value exceeding authority</td>
                <td align='right'>
                    <asp:Label ID="lblTotalExceedingAuthority" class='Label' runat="server"></asp:Label></td>
            </tr>
        </table>

        <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvCurrent_RowDataBound"
            Width="100%" EmptyDataText="No Data Found" EnableViewState="false" Visible="false">
            <Columns>
                <asp:BoundField DataField="CampaignNumber" HeaderText="Campaign #" HeaderStyle-Width="10%" />
                <asp:BoundField DataField="StartDate" HeaderText="Start date" HeaderStyle-Width="8%" DataFormatString="{0:MMM dd, yyyy}" />
                <asp:BoundField DataField="ApprovedBudget" HeaderText="Approved budget" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALSPENT" HeaderText="Total spent" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="AmountLeft" HeaderText="Over/under" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Total invoiced" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOwing" HeaderText="Owing" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>