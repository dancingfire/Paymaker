<%@ Page Language="c#" Inherits="campaign_outstanding" CodeFile="campaign_outstanding.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Outstanding campaign report</title>
    <script type="text/javascript">
    $(document).ready(function () {
        $(".RoundPanel").corner();
    });
    </script>
    <style>
        .Bold {
            font-weight: bold;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">

        <div class='ActionPanel'>
            <asp:HiddenField ID="hdRestoreCampaignID" runat="server" />
            <asp:Label ID="lblHeader" runat="server" Text="Outstanding Advertising" Style='float: left'></asp:Label>
        </div>

        <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false" Width="350px" EmptyDataText="No Data Found" EnableViewState="false" Style="float: left; clear: both">
            <Columns>
                <asp:BoundField DataField="Agent" HeaderText="Sales person" HeaderStyle-Width="18%" HtmlEncode="false" />
                <asp:BoundField DataField="Amount" HeaderText="Amount outstanding" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>

        <asp:Panel ID="dData" Style="float: left; clear: both; height: 20px; width: 350px; text-align: right; margin-top: 5px;" class="RoundPanel" runat="server">
            <asp:Label ID="lblTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>
    </form>
</body>
</html>