<%@ Page Language="c#" Inherits="Paymaker.AR_report" CodeFile="AR_report.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Proposed income</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }

        $(document).ready(function () {
            $('#gvTable').DataTable({
                "bSort": false,
                "bFilter": false,
                "order": [[0, "asc"]],
                "stateSave": true,
                "paging": false,
                "info": false,

                dom: 'Bfrtip',
                buttons: [
                    'excelHtml5'
                ]
            });
        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdFY" Value="" runat="server" />
        <div class='ActionPanel' style="width: 650px; float: left; clear: both">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Proposed income
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
                    <asp:BoundField HeaderText="Entitlement date" DataField="ENTITLEMENTDATE" DataFormatString="{0:MMM dd, yyyy}" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField HeaderText="Settlement date" DataField="SETTLEMENTDATE" DataFormatString="{0:MMM dd, yyyy}" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField HeaderText="Gross commission" DataField="CALCULATEDAMOUNT" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N2}" />
                    <asp:BoundField HeaderText="Net commission" DataField="NETCOMMISSION" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N2}" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>