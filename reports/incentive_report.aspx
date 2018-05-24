<%@ Page Language="c#" Inherits="Paymaker.incentive_report" CodeFile="incentive_report.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Incentive tracking report</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }

        function loadPage() {
            $("#gvTable tr:last").css({ fontWeight: 'bolder' });
        }
    </script>
</head>
<body onload='loadPage()'>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <asp:HiddenField ID="hdFY" Value="" runat="server" />
        <asp:HiddenField ID="hdExpenseID" Value="" runat="server" />
        <div class='ActionPanel' style="width: 99%">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Incentive Tracking Report
            </div>
        </div>
        <div id="divReport" runat="server">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="99%">
                <Columns>
                    <asp:BoundField DataField="AGENT" HeaderText="Agent" />
                    <asp:BoundField DataField="SALESTARGET" HeaderText="Sales target ($)" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="ProRatedTarget" HeaderText="Pro rated target ($)" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="GrossSalesYTD" HeaderText="Gross sales YTD ($)" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="AnnualTargetPercent" HeaderText="% of annual target achieved" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="ProRatedTargetPercent" HeaderText="Pro rated target (%)" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="GrossSales90" HeaderText="Hit 90% target" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="GrossSales100" HeaderText="Hit 100% target" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="GrossSales110" HeaderText="Hit 110% target" ItemStyle-HorizontalAlign="Center" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="divChart" runat="server">
            <asp:Chart ID="chtTopPerformers" runat="server" Width="1600">
                <Titles>
                    <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" Text=""
                        ForeColor="26, 59, 105">
                    </asp:Title>
                    <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" Text=""
                        ForeColor="26, 59, 105">
                    </asp:Title>
                </Titles>
            </asp:Chart>
        </div>
    </form>
</body>
</html>