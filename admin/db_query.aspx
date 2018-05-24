<%@ Page Language="c#" Inherits="db_query" CodeFile="db_query.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>

    <script type="text/javascript">
        $(document).ready(function () {
            //createDataTable("gvResults", true, true, 500, '100%');
            //$(".AdminDetailPanel").corner();
        });
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server" defaultbutton="btnRun">
        <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel"></asp:Panel>

        <asp:TextBox ID="txtSQL" runat="server" TextMode="MultiLine" Width="90%" Height="200" Style="float: left"></asp:TextBox>
        <asp:Button ID="btnRun" runat="server" Text="Run" OnClick="btnRun_Click" CssClass="Button btn" Style="float: right" Width="100" />
        <asp:Button ID="btnRunREOffice" runat="server" Text="Run REOffice" OnClick="btnRunREOffice_Click" CssClass="Button btn" Style="float: right" Width="100" />
        <asp:GridView ID="gvResults" runat="server"></asp:GridView>
    </form>
</body>
</html>