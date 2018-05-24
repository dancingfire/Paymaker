<%@ Page Language="c#" Inherits="campaign_aging" CodeFile="campaign_aging.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Aging campaign report</title>

    <style>
        .Bold {
            font-weight: bold;
        }

        .SectionHeader {
            margin-top: 15px;
            margin-bottom: 5px;
            font-size: larger;
            float: left;
            clear: both;
        }

        .SectionFooter {
            margin-bottom: 5px;
            font-size: larger;
            float: left;
            clear: both;
            width: 650px;
            height: 20px;
            text-align: right;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">

        <div class='ActionPanel'>
            <asp:HiddenField ID="hdRestoreCampaignID" runat="server" />
            <asp:Label ID="lblHeader" runat="server" Text="Outstanding Advertising" Style='float: left'></asp:Label>
        </div>

        <asp:Panel ID="Panel1" runat="server" class="SectionHeader">
            <asp:Label ID="Label2" runat="server" Text="Current " class="Bold"></asp:Label>
        </asp:Panel>
        <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false" Width="750px" EmptyDataText="No Data Found" EnableViewState="false" Style="float: left; clear: both">
            <Columns>
                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="#" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="50%" HtmlEncode="false" />
                <asp:BoundField DataField="AGENT" HeaderText="Agent" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Amount outstanding" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOWING" HeaderText="Amount remaining" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="dData" class="RoundPanel SectionFooter" runat="server">
            <asp:Label ID="lblCurrentTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="p30" runat="server" class="SectionHeader">
            <asp:Label ID="Label1" runat="server" Text="30 Days" class="Bold"></asp:Label>
        </asp:Panel>
        <asp:GridView ID="gv30" runat="server" AutoGenerateColumns="false" Width="750px" EmptyDataText="No Data Found" EnableViewState="false" Style="float: left; clear: both">
            <Columns>
                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="#" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="50%" HtmlEncode="false" />
                <asp:BoundField DataField="AGENT" HeaderText="Agent" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Amount outstanding" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOWING" HeaderText="Amount remaining" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="Panel2" class="RoundPanel SectionFooter" runat="server">
            <asp:Label ID="lbl30DaysTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="Panel3" runat="server" class="SectionHeader">
            <asp:Label ID="Label3" runat="server" Text="60 Days" class="Bold"></asp:Label>
        </asp:Panel>
        <asp:GridView ID="gv60" runat="server" AutoGenerateColumns="false" Width="750px" EmptyDataText="No Data Found" EnableViewState="false" Style="float: left; clear: both">
            <Columns>
                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="#" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="50%" HtmlEncode="false" />
                <asp:BoundField DataField="AGENT" HeaderText="Agent" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Amount outstanding" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOWING" HeaderText="Amount remaining" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="Panel4" class="RoundPanel SectionFooter" runat="server">
            <asp:Label ID="lbl60DaysTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="Panel5" runat="server" class="SectionHeader">
            <asp:Label ID="lbl90" runat="server" Text="90 Days" class="Bold"></asp:Label>
        </asp:Panel>
        <asp:GridView ID="gv90" runat="server" AutoGenerateColumns="false" Width="750px" EmptyDataText="No Data Found" EnableViewState="false" Style="float: left; clear: both">
            <Columns>
                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="#" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="50%" HtmlEncode="false" />
                <asp:BoundField DataField="AGENT" HeaderText="Agent" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Amount outstanding" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOWING" HeaderText="Amount remaining" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="Panel6" class="RoundPanel SectionFooter" runat="server">
            <asp:Label ID="lbl90DaysTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="Panel7" runat="server" class="SectionHeader">
            <asp:Label ID="lbl120" runat="server" Text="120+ Days" class="Bold"></asp:Label>
        </asp:Panel>
        <asp:GridView ID="gv120" runat="server" AutoGenerateColumns="false" Width="750px" EmptyDataText="No Data Found" EnableViewState="false" Style="float: left; clear: both">
            <Columns>
                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="#" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="50%" HtmlEncode="false" />
                <asp:BoundField DataField="AGENT" HeaderText="Agent" HeaderStyle-Width="10%" HtmlEncode="false" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Amount outstanding" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOWING" HeaderText="Amount remaining" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="Panel8" class="RoundPanel SectionFooter" runat="server">
            <asp:Label ID="lbl120DaysTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="Panel9" class="RoundPanel SectionFooter" runat="server">
            <asp:Label ID="lblGrandTotal" runat="server" Text="Overall total" CssClass="Bold"></asp:Label>
        </asp:Panel>
    </form>
</body>
</html>