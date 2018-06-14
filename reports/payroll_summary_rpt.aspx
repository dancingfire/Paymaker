<%@ Page Language="c#" Inherits="Paymaker.payroll_summary_rpt" CodeFile="payroll_summary_rpt.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>KPI</title>
    <script type="text/javascript">
            function printReport()
            {
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
        <asp:Panel ID="pNoData" runat="server" Style="float: left; margin-left: 20px" Visible="false">There was no data on the report</asp:Panel>
        <div id="tempOutput" runat="server" style="display: none;"></div>
        <div id="dViewer">
            <rsweb:ReportViewer ID="rViewer" runat="server" Width="100%"  AsyncRendering="true" SizeToReportContent="true"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>