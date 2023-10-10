<%@ Page Language="c#" Inherits="sales_dashboard" CodeFile="sales_dashboard.aspx.cs" EnableViewState="true" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">

        function updateSale(intID) {
            viewSale(intID);
        }

        function updateCampaign(intID, intActionID) {
            $('#mCampaign').on('show.bs.modal', function () {
                {
                    $('#fCampaign').attr("src", "../campaign/campaign_update.aspx?IsPopup=true&blnReadOnly=" + blnReadOnly + "&intCampaignID=" + intID + "&intActionID=" + intActionID);
                }
            });
            $('#mCampaign').on('hidden.bs.modal', function () {
                {
                    $('#fCampaign')[0].contentWindow.removeValidation();
                }
            });

            $('#mCampaign').modal('show');
            return false;
        }

        function refreshPage() {
            closeSale();
            closeCampaign();
            document.frmMain.submit();
        }

        $(document).ready(function () {
            if ($("#gvCurrent tr").length > 1) {
                var oTable = $('#gvCurrent').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": false,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollX": "99%",
                    "aaSorting": []
                });
            }

            if ($("#gvPayments tr").length > 1) {
                var oTable = $('#gvPayments').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": false,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollX": "99%",
                    "aaSorting": []
                });
            }

            if ($("#gvFuture tr").length > 1) {
                var oTable = $('#gvFuture').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": false,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollX": "99%",
                    "aaSorting": []
                });
            }

            if ($("#gvCampaign tr").length > 1) {
                var oTable = $('#gvCampaign').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": true,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollX": "99%",
                    "aaSorting": []
                });
            }

            if ($("#gvPrePayment tr").length > 1) {
                var oTable = $('#gvPrePayment').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": false,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollX": "99%",
                    "aaSorting": []
                });
            }
            $(".DataPanel").corner();
        });
    </script>
    <style type="text/css">
        .DataPanel {
            background: #E6D5AC;
            border: solid 1px #C49C42;
            padding: 5px;
            color: #6F5928;
            margin-bottom: 10px;
            margin-right: 10px;
            min-height: 50px
        }

        .DataPanelHeader {
            font-size: 14px;
            font-weight: 800;
            color: #6F5928;
            padding: 2px;
        }

        #gvCurrent tbody tr:hover, #gvFuture tbody tr:hover, #gvCampaign tbody tr:hover {
            background-color: #DDC691 !important;
            cursor: pointer;
        }
    </style>
</head>
<body style='margin-top: 5px'>
    <form id="frmMain" method="post" runat="server" target="_self">
        <div class="container-fluid">
            <div class="col-sm-2 DataPanel" style="height: 80vh">
                <div class='DataPanelHeader'>
                    Monthly commission
                </div>
                <asp:GridView ID="gvHistory" runat="server"  EnableViewState="false" AutoGenerateColumns="false" CssClass="SelectTable" EmptyDataText="No Data Found" Width="95%">
                    <Columns>
                        <asp:TemplateField HeaderText="Settled Date" HeaderStyle-Width="50%" >
                            <ItemTemplate>
                                <%# getReportLink(Eval("Month").ToString(), Eval("Year").ToString(), Eval("PayPeriodID").ToString())%>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CommissionTotal" HeaderText="Amount" HeaderStyle-Width="50%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N2}" />
                    </Columns>
                </asp:GridView>
            </div>
            <div class="col-sm-9">
                <div class="row">
                    <div class="DataPanel">
                        <div class='DataPanelHeader'>
                            Displaying data for
                            <asp:DropDownList ID="lstPayPeriod" runat="server"  OnSelectedIndexChanged="lstPayPeriod_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                        </div>
                    </div>
                </div>
                <div class="row">

                    <div class="DataPanel">
                        <div class='DataPanelHeader'>
                            Current commissions
                        </div>
                        <asp:GridView ID="gvCurrent" runat="server"  AutoGenerateColumns="false" EnableViewState="false" CssClass="test" OnRowDataBound="gvCurrent_RowDataBound" Width="100%" EmptyDataText="No Data Found">
                            <Columns>
                                <asp:BoundField DataField="Code" HeaderText="Code" HeaderStyle-Width="20%" />
                                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" />
                                <asp:BoundField DataField="SaleDate" HeaderText="Sale date" DataFormatString="{0:MMM dd, yyyy}"
                                    HeaderStyle-Width="15%" />
                                <asp:BoundField DataField="EntitlementDate" HeaderText="Entitlement date" DataFormatString="{0:MMM dd, yyyy}"
                                    HeaderStyle-Width="15%" />
                                <asp:TemplateField HeaderText="Settled Date" HeaderStyle-Width="10%">
                                    <ItemTemplate>
                                        <%# getSettlementDate(Eval("SettlementDate").ToString())%>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="CommissionTotal" HeaderText="Commission total" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" />
                                <asp:BoundField DataField="SalePrice" HeaderText="Sale price" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" />
                            </Columns>
                        </asp:GridView>
                        <asp:Panel ID="pNoDataCurrent" Visible="false" runat="server">
                            <asp:Label ID="lblNoData" class='Label' runat="server" Style="width: 350px" Text="There are no current sales for this pay period."></asp:Label>
                        </asp:Panel>
                    </div>
                </div>

                <div class="row">
                    <div class="DataPanel">
                        <div id="dPendingHeader" class='DataPanelHeader' runat="server">
                            Pending commissions
                        </div>
                        <asp:GridView ID="gvFuture" runat="server"  EnableViewState="false" AutoGenerateColumns="false" OnRowDataBound="gvCurrent_RowDataBound" Width="100%">
                            <Columns>
                                <asp:BoundField DataField="Code" HeaderText="Code" HeaderStyle-Width="20%" />
                                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" />
                                <asp:BoundField DataField="SaleDate" HeaderText="Sale date" DataFormatString="{0:MMM dd, yyyy}"
                                    HeaderStyle-Width="15%" />
                                <asp:BoundField DataField="EntitlementDate" HeaderText="Entitlement date" DataFormatString="{0:MMM dd, yyyy}"
                                    HeaderStyle-Width="15%" />
                                <asp:TemplateField HeaderText="Settled Date" HeaderStyle-Width="10%">
                                    <ItemTemplate>
                                        <%# getSettlementDate(Eval("SettlementDate").ToString())%>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="CommissionTotal" HeaderText="Commission total" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" />
                                <asp:BoundField DataField="SalePrice" HeaderText="Sale price" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:C2}"/>
                            </Columns>
                            <EmptyDataTemplate>
                                There are no pending sales.
                            </EmptyDataTemplate>
                        </asp:GridView>
                        <asp:Panel ID="pNoFutureSales" Visible="false" runat="server">
                            <asp:Label ID="Label3" class='Label' runat="server" Style="width: 350px" Text="There are no pending sales."></asp:Label>
                        </asp:Panel>
                    </div>
                </div>
                     <div class="row">
                    <div class="DataPanel">
                        <div id="dvPayments" class='DataPanelHeader' runat="server">
                            Pending payments
                        </div>
                        <asp:GridView ID="gvPayments" runat="server"  EnableViewState="false" AutoGenerateColumns="false" Width="100%">
                            <Columns>
                                <asp:BoundField DataField="Account" HeaderText="Expense type" HeaderStyle-Width="20%" />
                                <asp:BoundField DataField="Comment" HeaderText="Address" HeaderStyle-Width="20%" />
                                <asp:BoundField DataField="TxDate" HeaderText="Entitlement date" DataFormatString="{0:MMM dd, yyyy}"
                                    HeaderStyle-Width="15%" />
                               
                                <asp:BoundField DataField="Amount" HeaderText="Amount" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}"/>
                            </Columns>
                            <EmptyDataTemplate>
                                There are no pending payments.
                            </EmptyDataTemplate>
                        </asp:GridView>
                        <asp:Panel ID="pNoPayments" Visible="false" runat="server">
                            <asp:Label ID="Label2" class='Label' runat="server" Style="width: 350px" Text="There are no pending payments."></asp:Label>
                        </asp:Panel>
                    </div>
                </div>
                <div class="row">
                    <div class="DataPanel">
                        <div class='DataPanelHeader'>
                            Campaign actions required
                        </div>
                        <asp:GridView ID="gvCampaign" runat="server"  EnableViewState="false" AutoGenerateColumns="false" CssClass="SelectTable"
                            OnRowDataBound="gvCampaign_RowDataBound" Width="100%" EmptyDataText="No Data Found">
                            <Columns>
                                <asp:BoundField DataField="CampaignNumber" HeaderText="Campaign #" HeaderStyle-Width="10%" />
                                <asp:BoundField DataField="StartDate" HeaderText="Start date" HeaderStyle-Width="8%" DataFormatString="{0:MMM dd, yyyy}" />
                                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="24%" HtmlEncode="false" />
                                <asp:BoundField DataField="ApprovedBudget" HeaderText="Approved budget" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="TOTALSPENT" HeaderText="Total spent" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="AmountLeft" HeaderText="Over/under" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="TOTALINVOICED" HeaderText="Total invoiced" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="TOTALPAID" HeaderText="Total paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="TOTALOwing" HeaderText="Owing" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="Action" HeaderText="Action" HeaderStyle-Width="20%" />
                            </Columns>
                        </asp:GridView>
                        <asp:Panel ID="pCampaignNoData" Visible="false" runat="server">
                            <asp:Label ID="Label1" class='Label' runat="server" Style="width: 350px" Text="There are no campaigns that require your attention"></asp:Label>
                        </asp:Panel>
                    </div>
                </div>
                <div class="row">
                    <div class="DataPanel">
                        <div class='DataPanelHeader'>
                            Campaign prepayment
                        </div>
                        <asp:GridView ID="gvPrePayment" runat="server" AutoGenerateColumns="false" Width="100%" EmptyDataText="No Data Found" EnableViewState="false" Style='float: left'>
                            <Columns>
                                <asp:BoundField DataField="CAMPAIGNNUMBER" HeaderText="Campaign #" HeaderStyle-Width="5%" />
                                <asp:BoundField DataField="ListedDate" HeaderText="Listed date" HeaderStyle-Width="8%" DataFormatString="{0:MMM dd, yyyy}" />
                                <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" HtmlEncode="false" />
                                <asp:BoundField DataField="TOTALSPENT" HeaderText="Adv $'s sold" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="TOTALPAID" HeaderText="Adv $'s Paid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="% Prepaid" HeaderText="% Prepaid" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:0%}" HtmlEncode="false" />
                                <asp:BoundField DataField="TOtalOwing" HeaderText="Outstanding $'s" HeaderStyle-Width="8%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>