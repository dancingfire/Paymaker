<%@ Page Language="c#" Inherits="Paymaker.top_advertising_detail" CodeFile="top_advertising_detail.aspx.cs" %>


<!DOCTYPE html>
<html>
<head runat="server">
    <title>Advertising sales</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
    <style>
        table.Report {
            font-size: 1em;
        }

        .gvTable {
            table-layout: fixed;
        }

        @media print {
            .PrintControlLandscape {
                width: 950px;
                background: none;
                float: left;
            }

            .Report {
                margin: 0;
                padding: 0;
            }
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