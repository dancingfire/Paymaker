<%@ Page Language="c#" Inherits="Paymaker.agent_payroll_estimate" CodeFile="agent_payroll_estimate.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Agents proposed payables</title>
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
                Agents proposed payables
            </div>
        </div>
        <div id="oDetails" style="float: left; clear: left; width: 90%">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="50%" OnSorting="gvTable_Sorting">
                <Columns>
                    <asp:BoundField DataField="OFFICENAME" HeaderText="OfficeName" />
                    <asp:BoundField DataField="AGENT" HeaderText="Agent" />
                    <asp:BoundField DataField="Amount" HeaderText="Payable" ItemStyle-CssClass='AlignRight' DataFormatString="{0:$0.00}" />
                </Columns>
                <EmptyDataTemplate>
                    There is no information available for this report.
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </form>
</body>
</html>