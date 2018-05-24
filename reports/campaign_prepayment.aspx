<%@ Page Language="c#" Inherits="campaign_prepayment" CodeFile="campaign_prepayment.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Agent prepayment report</title>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />

        <div class='ActionPanel'>
            <asp:HiddenField ID="hdRestoreCampaignID" runat="server" />
            <asp:Label ID="lblHeader" runat="server" Text="Campaign prepayment report" Style='float: left'></asp:Label>
        </div>

        <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false"
            Width="750px" EmptyDataText="No Data Found" EnableViewState="false">
            <Columns>
                <asp:BoundField DataField="Agent" HeaderText="Sales person" HeaderStyle-Width="18%" HtmlEncode="false" />
                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="Campaign #" HeaderStyle-Width="5%" />
                <asp:BoundField DataField="ListedDate" HeaderText="Start date" HeaderStyle-Width="8%" DataFormatString="{0:MMM dd, yyyy}" />
                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" HtmlEncode="false" />
                <asp:BoundField DataField="TOTALSPENT" HeaderText="Adv $'s sold" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Adv $'s Paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPREPAID" HeaderText="Pre paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="% Prepaid" HeaderText="% Prepaid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:0%}" />
                <asp:BoundField DataField="AgentAverage" HeaderText="Agent average" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="PropertyCount" HeaderText="#" HeaderStyle-Width="4%" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="TOtalOwing" HeaderText="Outstanding $'s" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="pNoDataCurrent" Visible="false" runat="server">
            <asp:Label ID="lblNoData" class='Label' runat="server" Style="width: 350px" Text="There are no new campaigns"></asp:Label>
        </asp:Panel>
        <div id="tempOutput" runat="server" style="display: none;"></div>
    </form>
</body>
</html>