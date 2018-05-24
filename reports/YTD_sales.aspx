<%@ Page Language="c#" Inherits="Paymaker.YTD_sales" CodeFile="YTD_sales.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Year to date branch sales</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdFY" Value="" runat="server" />
        <div class='ActionPanel' style="width: 99%">
            <div id="pPageHeader" class='PageHeader' runat="server">
                MONTHLY SALES
            </div>
        </div>
        <div id="oDetails" style="float: left; clear: both;">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="true" Width="99%"
                OnRowDataBound="gvTable_RowDataBound">
            </asp:GridView>
        </div>
    </form>
</body>
</html>