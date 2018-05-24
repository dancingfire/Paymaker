<%@ Page Language="c#" Inherits="Paymaker.monthly_retainer" CodeFile="monthly_retainer.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Mpnthly retainer report</title>
    <script type="text/javascript">
    function printReport() {
        factory.printing.portrait = false;
        factory.printing.Print(true);
    }

    function openFullScreen() {
        window.moveTo(0, 0);
        window.resizeTo(screen.width, screen.height);
    }
    </script>
    <style type="text/css">
        @media print {
            .CommissionTable {
                width: 750px;
                font-family: Tahoma, Arial, Verdana;
                font-size: 10px;
                color: black;
                border-collapse: collapse;
            }
        }
    </style>
</head>
<body onload='openFullScreen()'>
    <object id="factory" viewastext style="display: none"
        classid="clsid:1663ed61-23eb-11d2-b92f-008048fdd814"
        codebase="http://commission.fletchers.com.au/include/smsx.cab#Version=7,0,0,8">
    </object>
    <form id="frmMain" method="post" runat="server">
        <div id="Container">
            <!-- MeadCo ScriptX -->

            <div id="pPageHeader" class='ReportHeader' runat="server">
                Retainer statement
            </div>
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="40%" CssClass='Report'>
                <Columns>
                    <asp:BoundField DataField="NAME" HeaderText="Name" HeaderStyle-Width="80%" />
                    <asp:BoundField DataField="Amount" HeaderText="Distribution amount" HeaderStyle-Width="20%" ItemStyle-HorizontalAlign="Right" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>