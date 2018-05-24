<%@ Page Language="c#" Inherits="campaign_actions" CodeFile="campaign_actions.aspx.cs" EnableViewState="false" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">

        $(document).ready(function () {
            if ($("#gvCurrent tr").length > 1) {
                var oTable = $('#gvCurrent').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": true,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "aaSorting": []
                });
            }
        });

        function updateCampaign(intID) {
            szPage = "../campaign/campaign_update.aspx?";
            szParams = "intCampaignID=" + intID;

            $('#mCampaign').on('show.bs.modal', function () {
                $('#fCampaign').attr("src", szPage + szParams);
            });
            $('#mCampaign').on('hidden.bs.modal', function () {
                $('#fCampaign').attr("src", "../blank.html");
                refreshPage()
            });
            $('#mCampaign').modal({ 'backdrop': 'static' });
            $('#mCampaign').modal('show');
        }

        function closeCampaign() {
            $('#mCampaign').modal('hide');
            refreshPage();

        }

        function refreshPage() {
            document.frmMain.submit();
        }
    </script>
    <style type="text/css">
        #gvCurrent tbody tr:hover {
            background-color: #DDC691 !important;
            cursor: pointer;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <div class='ActionPanel'>
            <asp:Label ID="lblHeader" runat="server" Text="Campaigns with actions" Style='float: left'></asp:Label>
            <div style='float: left;' runat="server" id='dSearch' visible="false">
                <asp:Label ID="Label1" runat="server" Text="Address" Style='float: left'></asp:Label>
                <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="Button btn" />
            </div>
            <a style='float: right; margin-right: 10px;' href='campaign_dashboard.aspx'>Return to dashboard</a>
        </div>

        <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false" CssClass="SelectTable"
            OnRowDataBound="gvCurrent_RowDataBound" Width="100%" Style='margin-bottom: 20px' EmptyDataText="No Data Found" EnableViewState="false">
            <Columns>
                <asp:BoundField DataField="CampaignNumber" HeaderText="Campaign #" HeaderStyle-Width="10%" />
                <asp:BoundField DataField="StartDate" HeaderText="Start date" HeaderStyle-Width="8%" DataFormatString="{0:MMM dd, yyyy}" />
                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" HtmlEncode="false" />
                <asp:BoundField DataField="Agent" HeaderText="Sales person" HeaderStyle-Width="6%" />
                <asp:BoundField DataField="Office" HeaderText="Sales office" HeaderStyle-Width="8%" />
                <asp:BoundField DataField="ApprovedBudget" HeaderText="Approved budget" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALSPENT" HeaderText="Total spent" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="AmountLeft" HeaderText="Over/under" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Total invoiced" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOwing" HeaderText="Owing" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="Action" HeaderText="Action" HeaderStyle-Width="10%" />
            </Columns>
        </asp:GridView>
        <asp:Panel ID="pNoDataCurrent" Visible="false" runat="server">
            <asp:Label ID="lblNoData" class='Label' runat="server" Style="width: 350px" Text="There are no new campaigns"></asp:Label>
        </asp:Panel>
    </form>
</body>
</html>