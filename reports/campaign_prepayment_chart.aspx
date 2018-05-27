<%@ Page Language="C#" CodeFile="campaign_prepayment_chart.aspx.cs" Inherits="Paymaker.campaign_prepayment_chart" %>

<!DOCTYPE html>
<html>
<head id="Head1" runat="server">
    <title>Top advertising average</title>
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
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <asp:Panel ID="pNoData" runat="server" Style="float: left; margin-left: 20px" Visible="false">There was no data on the report</asp:Panel>
        <div id="dViewer">
            <rsweb:ReportViewer ID="rViewer" runat="server" Width="100%"  AsyncRendering="true"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>