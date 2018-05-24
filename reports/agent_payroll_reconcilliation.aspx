<%@ Page Language="c#" Inherits="Paymaker.agent_payroll_reconcilliation" CodeFile="agent_payroll_reconcilliation.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Agent payroll reconciliation</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
    <style type="text/css">
        .AgentHeader {
            font-size: 1.2em;
            font-weight: 900;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <div style="width: 99%">
            <div id="pPageHeader" class='PageHeader' style='background-color: white' runat="server">
                Agent payroll reconciliation
            </div>
        </div>
        <div id="oDetails" runat="server">
        </div>
    </form>
</body>
</html>