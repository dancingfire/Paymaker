<%@ Page Language="c#" Inherits="Paymaker.proposed_s27_report" CodeFile="proposed_s27_report.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Proposed income</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdFY" Value="" runat="server" />
        <div class='ActionPanel' style="width: 650px; float: left; clear: both">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Proposed Section 27 income
            </div>
        </div>
        <div style="width: 650px; float: left; clear: both">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="650">
                <EmptyDataTemplate>
                    There are no outstanding sales for this period
                </EmptyDataTemplate>
                <Columns>
                    <asp:BoundField HeaderText="Sale date" DataField="SALEDATE" DataFormatString="{0:MMM dd, yyyy}" />
                    <asp:BoundField HeaderText="Code" DataField="CODE" />
                    <asp:BoundField HeaderText="Commission" DataField="CALCULATEDAMOUNT" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N2}" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>