<%@ Page Language="c#" Inherits="Paymaker.off_the_top" CodeFile="off_the_top.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Off the top expenses</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
    <style>
        table.Report {
            font-size: 1em;
        }
    </style>
</head>
<body class='Report'>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <div class='ActionPanel' style="width: 99%">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Off the top expenses
            </div>
        </div>
        <div id="oDetails" style="float: left; width: 99%">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="50%" CssClass='Report'>
                <Columns>
                    <asp:BoundField DataField="ADDRESS" HeaderText="Address" HeaderStyle-Width="40%" HtmlEncode="false" />
                    <asp:BoundField DataField="SALEDATE" HeaderText="Sale Date" HeaderStyle-Width="20%" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:MMM dd, yyyy}" />
                    <asp:BoundField DataField="GIFTS" HeaderText="Gifts" ItemStyle-CssClass='AlignRight' HeaderStyle-Width="20%" DataFormatString="{0:F2}" />
                    <asp:BoundField DataField="INCENTIVE" HeaderText="Incentive" ItemStyle-CssClass='AlignRight' HeaderStyle-Width="30%" DataFormatString="{0:F2}" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>