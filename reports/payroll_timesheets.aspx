<%@ Page Language="c#" Inherits="Paymaker.payroll_timesheets" CodeFile="payroll_timesheets.aspx.cs" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>KPI</title>
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
        <asp:Panel ID="pNoData" runat="server" Style="float: left; margin-left: 20px" Visible="false">There was no data on the report</asp:Panel>
        <div id="tempOutput" runat="server" style="display: none;"></div>
        <div id="dViewer">
            <rsweb:ReportViewer ID="rViewer" runat="server" Width="100%" Height="100%" AsyncRendering="true"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>