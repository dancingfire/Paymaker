<%@ Page Language="c#" Inherits="campaign_import" CodeFile="campaign_import.aspx.cs" Async="true" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">
        function getCurrentCount() {
            callWebMethod("../web_services/ws_Paymaker.asmx", "oProgress", ["ImportID", -1], getCurrentCountDone);
        }

        function getCurrentCountDone(szCount) {
            if (szCount == "undefined" || szCount == "DONE") {
                window.setTimeout("getCurrentCount()", 3000);
            }
            else {
                szTotalCount = parseInt(szCount);
                szCount = parseInt(szCount.substr(szCount.indexOf("|") + 1));
                $("#oImportProgress").html("Currently importing " + szCount + " record out of total records of " + szTotalCount);

                if (parseInt(szTotalCount) > parseInt(szCount)) {
                    window.setTimeout("getCurrentCount()", 1000);
                } else if (parseInt(szTotalCount) == parseInt(szCount)) {
                    window.setTimeout("getCurrentCount()", 0);
                    getDetailsCount();
                } else {
                    window.setTimeout("getCurrentCount()", 1000);
                }
            }
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self">
        <div class='ActionPanel'>
            <asp:Panel ID="pPageHeader" class='PageHeader' runat="server">
                <asp:Label ID="Label2" runat="server" Text="Import campaigns"></asp:Label>
            </asp:Panel>
        </div>
        <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel" Style="left: 655px; top: 60px; position: absolute; width: 300px"></asp:Panel>
        <div id='oProgress'>
        </div>
        <asp:Button ID="btnImport" runat="server" Text="Start" CssClass="Button btn" OnClick="btnImport_Click" />
        <asp:CheckBox ID="chkUpdateOnly" runat="server"
            ToolTip="Only updates the details of the property, not all the products, invoices etc..."
            Text="Only import base properties (Fast debugging)"></asp:CheckBox>
    </form>
</body>
</html>