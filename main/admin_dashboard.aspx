<%@ Page Language="c#" Inherits="admin_dashboard" CodeFile="admin_dashboard.aspx.cs" EnableViewState="true" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">
    intHeight = $(window).height() - 130;

    function updateSale(intID) {
        viewSale(intID);
    }

    $(document).ready(function () {
        createDataTable("gvCurrent", true, false, intHeight, false);
        createButtons();
        $("#lstStatus, #lstSaleMonth, #lstPayPeriod").select2();
    });

    function runImport() {
        $.ajax({
            async: true,
            type: "POST",
            url: "../web_services/ws_BoxDice.asmx/runFullBDImport",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
            }
        });
        
        alert("We are running the import process. You can check on the status of the import process from the import log page. ");
        return false;
     }

    function refreshPage() {
        closeSale();
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

    <form id="frmMain" method="post" runat="server" target="_self" defaultbutton="btnSearch">
        <div class='ActionPanel'>
        </div>
        <br />
        <div class='GridFilter' id='dDrigFilter'>
            <span style='float: right; width: 140px'>
                <asp:Button ID="btnImport" runat="server" Text="Import latest data" CssClass="Button btn" OnClientClick="return runImport()" /><br />
                <a href="../boxdice/api_log.aspx">View log</a>
            </span>
            <span class='FilterPanel'>
                <asp:Label ID="lblPropertyFilter" class='Label' runat="server" Text="Property search"></asp:Label>
                <asp:TextBox ID="txtPropertyFilter" class='Edit' runat="server" Height="28"></asp:TextBox>
            </span>
            <span class='FilterPanel'>
                <asp:Label ID="lblComplete" class='Label' runat="server" Text="Status "></asp:Label>
                <asp:DropDownList ID="lstStatus" runat="server" CssClass="Edit" AutoPostBack="true">
                    <asp:ListItem Text="All..." Value=""></asp:ListItem>
                    <asp:ListItem Text="Incomplete" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Completed" Value="1"></asp:ListItem>
                    <asp:ListItem Text="Finalized" Value="2"></asp:ListItem>
                </asp:DropDownList>
            </span>
            <span class='FilterPanel'>
                <asp:Label ID="Label1" class='Label' runat="server" Text="Pay period"></asp:Label>

                <asp:DropDownList ID="lstPayPeriod" runat="server" CssClass="Edit" AutoPostBack="true" Width="120px">
                    <asp:ListItem Text="All..." Value=""></asp:ListItem>
                </asp:DropDownList>
            </span>
            <span class='FilterPanel'>
                <asp:Label ID="Label2" class='Label' runat="server" Text="Sale month"></asp:Label>

                <asp:DropDownList ID="lstSaleMonth" runat="server" CssClass="Edit" AutoPostBack="true" >
                </asp:DropDownList>

                   <asp:Button ID="btnSearch" runat="server" Text="Search" Style='float: right; padding-right: 10px; padding-left: 10px' CssClass="Button btn" />
            </span>
         
        </div>
        <div style="float: left; clear: both; width: 100%">
            <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvCurrent_RowDataBound" EnableViewState="false" Width="100%">
                <Columns>
                    <asp:TemplateField HeaderText="Status" HeaderStyle-Width="10%">
                        <ItemTemplate>
                            <%# getStatus(Convert.ToInt32(Eval("STATUSID")), Convert.ToBoolean(Eval("ISSOURCEMODIFIED")))%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Code" HeaderText="Code" HeaderStyle-Width="7%" />
                    <asp:BoundField DataField="Address" HeaderText="Address" HeaderStyle-Width="20%" ItemStyle-Width="20%" />
                    <asp:BoundField DataField="SaleDate" HeaderText="Sale date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="8%" ItemStyle-Width="8%" />
                    <asp:BoundField DataField="EntitlementDate" HeaderText="Entitlement date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="8%" ItemStyle-Width="8%" />
                    <asp:TemplateField HeaderText="Settled Date" HeaderStyle-Width="11%" ItemStyle-Width="11%">
                        <ItemTemplate>
                            <%# getSettlementDate(Eval("SettlementDate").ToString())%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="SalePrice" HeaderText="Sale price" HeaderStyle-Width="8%" ItemStyle-Width="8%" />
                    <asp:TemplateField HeaderText="Pay period" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                        <ItemTemplate>
                            <%# getPayPeriod(Convert.ToInt32(Eval("SAFEPAYPERIODID")))%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="GROSSCOMMISSION" HeaderText="Commission amount" HeaderStyle-Width="12%" ItemStyle-Width="12%" DataFormatString="{0:F2}" />
                    <asp:TemplateField HeaderText="%" HeaderStyle-Width="6%" ItemStyle-Width="6%" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <%# getPercentage(Convert.ToDouble(Eval("SALEPRICE")), Convert.ToDouble(Eval("GROSSCOMMISSION")))%>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>