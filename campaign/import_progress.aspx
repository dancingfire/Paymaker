<%@ Page Language="c#" Inherits="import_progress" CodeFile="import_progress.aspx.cs" EnableSessionState="False" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <link href="../Paymaker.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        function getCurrentCount() {
            document.frmMain.submit();
        }

        function getCurrentCountTimer() {
            window.setTimeout("getCurrentCount()", 3000);
        }
    </script>
</head>
<body onload='getCurrentCountTimer()'>
    <form id="frmMain" method="post" runat="server" target="_self">
        <div class='ActionPanel'>
            <asp:Panel ID="pPageHeader" class='PageHeader' runat="server">
                <asp:Label ID="Label2" runat="server" Text="Campaign import progress"></asp:Label>
            </asp:Panel>
        </div>
        <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel" Style="left: 655px; top: 60px; position: absolute; width: 300px"></asp:Panel>
        <div id='oProgress' runat="server" style='width: 300px; margin-left: auto; margin-right: auto'>
            Loading the initial records from Campaign Track. This process may take several minutes. This window will automatically close when the update process has completed.
        </div>
    </form>
</body>
</html>