<%@ Page Language="c#" Inherits="Paymaker.mentor_bonus_detail" CodeFile="mentor_bonus_detail.aspx.cs" %>

    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Mentor bonus detail report</title>
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
            <rsweb:ReportViewer ID="rViewer" runat="server" Width="100%"  AsyncRendering="true" SizeToReportContent="true"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>