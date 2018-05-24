<%@ Page Language="c#" Inherits="campaign_detail" CodeFile="campaign_detail.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">

        intHeight = $(window).height() - 90;
        $(document).ready(function () {
            if ($("#gvCurrent tr").length > 1) {
                var oTable = $('#gvCurrent').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": true,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollY": intHeight,
                    "bScrollCollapse": true,
                    "aaSorting": []
                });
            }
            $("#txtSearch").focus();
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

        function closeCampaign(blnRefresh) {
            $('#mCampaign').modal('hide');
            if (blnRefresh)
                refreshPage();
        }

        function validatePage() {
            if ($("#txtSearch").val() == "") {
                alert("Please enter some search criteria.");
                return false;
            }
            return true;
        }

        function restoreCampaign(intID) {
            event.preventDefault ? event.preventDefault() : event.returnValue = false;
            $("#hdRestoreCampaignID").val(intID);
            $("#btnRestore").click();
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
            <asp:HiddenField ID="hdRestoreCampaignID" runat="server" />
            <asp:Label ID="lblHeader" runat="server" Text="New campaigns" Style='float: left'></asp:Label>
            <div style='float: left;' runat="server" id='dSearch' visible="false">
                <asp:Label ID="Label1" runat="server" Text="Enter address or campaign number: " Style='float: left; margin-top: 4px'></asp:Label>
                <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="Button btn" OnClick="btnSearch_Click" OnClientClick="return validatePage()" />
                <asp:Button ID="btnRestore" runat="server" Text="Search" CssClass="Button btn" OnClick="btnRestore_Click" Style='display: none' />
            </div>
            <a style='float: right; margin-right: 10px;' href='campaign_dashboard.aspx'>Return to dashboard</a>
        </div>
        <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false"
            OnRowDataBound="gvCurrent_RowDataBound" Width="100%" EmptyDataText="No Data Found" EnableViewState="false">
            <Columns>
                <asp:BoundField DataField="CampaignNumber" HeaderText="Campaign #" HeaderStyle-Width="10%" />
                <asp:BoundField DataField="StartDate" HeaderText="Start date" HeaderStyle-Width="8%" DataFormatString="{0:MMM dd, yyyy}" />
                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" HtmlEncode="false" />
                <asp:BoundField DataField="Agent" HeaderText="Sales person" HeaderStyle-Width="6%" />
                <asp:BoundField DataField="Office" HeaderText="Sales office" HeaderStyle-Width="4%" />
                <asp:BoundField DataField="ProductCount" HeaderText="PC" HeaderStyle-Width="2%" />
                <asp:BoundField DataField="ProductsInvoiced" HeaderText="PI" HeaderStyle-Width="2%" />
                <asp:BoundField DataField="ApprovedBudget" HeaderText="Approved budget" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALSPENT" HeaderText="Total spent" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="AmountLeft" HeaderText="Over/under" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Total invoiced" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="TOTALOwing" HeaderText="Owing" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                <asp:TemplateField HeaderText="" HeaderStyle-Width="10%">
                    <ItemTemplate>
                        <%# getIcons(Convert.ToInt32(Eval("NOTECOUNT")), Convert.ToInt32(Eval("CONTRIBUTIONSTATUS")),  Convert.ToBoolean(Eval("ISDELETED")), Convert.ToInt32(Eval("ID")))%>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:Panel ID="pNoDataCurrent" Visible="false" runat="server">
            <asp:Label ID="lblNoData" class='Label' runat="server" Style="width: 350px" Text="There are no new campaigns"></asp:Label>
        </asp:Panel>
    </form>
</body>
</html>