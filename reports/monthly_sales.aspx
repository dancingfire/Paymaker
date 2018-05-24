<%@ Page Language="c#" Inherits="Paymaker.monthly_sales" CodeFile="monthly_sales.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta http-equiv="x-ua-compatible" content="IE=Edge" />
    <title>Monthly sales</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
    <style>
        table.Report {
            font-size: 1em;
        }

        .gvTable {
            table-layout: fixed;
        }

        @media print {
            .PrintControlLandscape {
                width: 950px;
                background: none;
                float: left;
            }

            .Report {
                margin: 0;
                padding: 0;
            }
        }
    </style>
</head>
<body class='Report'>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <div class="PrintControlLandscape" style="width: 950px">
            <div id="pPageHeader" class='PageHeader ActionPanel' runat="server" style="width: 99%; float: left">
                Monthly Sales
            </div>

            <div style="float: left">
                <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false"
                    Width="99%" CssClass='Report' OnSorting="gvTable_Sorting">
                    <Columns>
                        <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="24%" />
                        <asp:BoundField DataField="SALEDATE" HeaderText="Sale Date" HeaderStyle-Width="9%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="SALEPRICE" HeaderText="Sale Price" ItemStyle-CssClass='AlignRight' HeaderStyle-Width="9%" DataFormatString="{0:C0}" />
                        <asp:TemplateField HeaderText="Settlement Proposed" HeaderStyle-Width="9%" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <%# getSettlementDate(Eval("SettlementDate").ToString())%>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="GROSSCOMMISSION" HeaderText="Overall Comm." ItemStyle-CssClass='AlignRight' HeaderStyle-Width="6%" DataFormatString="{0:C0}" />
                        <asp:BoundField DataField="CONJUNCTIONALCOMMISSION" HeaderText="Conj. Comm."
                            ItemStyle-CssClass='AlignRight' HeaderStyle-Width="7%" DataFormatString="{0:C0}" />
                        <asp:BoundField DataField="OFFICECOMMISSION" HeaderText="Office Comm." ItemStyle-CssClass='AlignRight' HeaderStyle-Width="6%" DataFormatString="{0:C0}" />
                        <asp:BoundField DataField="Lister" HeaderText="Lister (1-3)" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="MANAGER" HeaderText="Manager (1-2)" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="SELLER" HeaderText="Seller" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="LEAD" HeaderText="Lead" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Service Area" HeaderText="Service Area" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="MENTOR" HeaderText="Mentor" HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Label ID="lTotal" runat="server" Text="Label" Class="Normal"></asp:Label>
        </div>
    </form>
</body>
</html>