<%@ Page Language="c#" Inherits="Paymaker.missing_sales" CodeFile="missing_sales.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Missing sales</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <div class='ActionPanel' style="width: 99%">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Missing Sales
            </div>
        </div>
        <div id="oDetails">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="99%">
                <Columns>
                    <asp:BoundField DataField="CODE" HeaderText="Code" />
                    <asp:BoundField DataField="ADDRESS" HeaderText="Address" />
                    <asp:BoundField DataField="SALEDATE" HeaderText="Sale Date" />
                    <asp:BoundField DataField="SALEPRICE" HeaderText="Sale Price" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="ENTITLEMENTDATE" HeaderText="Entitlement date" DataFormatString="{0:MMM dd, yyyy}" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>