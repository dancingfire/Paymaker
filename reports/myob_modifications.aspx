<%@ Page Language="c#" Inherits="Paymaker.commission_statement_finalize" CodeFile="myob_modifications.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>MYOB differences</title>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel" Width="800"></asp:Panel>

        <div class='ActionPanel' style="width: 99%; margin-top: 10px">
            <div style='width: 49%; float: left'>
                <span id="spPayPeriod">
                    <asp:Label ID="Label3" CssClass="Label LabelPos" runat="server" Text="Pay period">
                    </asp:Label>
                    <asp:DropDownList ID="lstPayPeriod" CssClass="Entry EntryPos" runat="server">
                    </asp:DropDownList>
                    <br class='Align' />
                </span>
            </div>
            <div id='dButtons' style='width: 49%; float: right; display: inline'>

                <asp:Button ID="btnCreate" runat="server" Width="150px" CssClass="Button btn EntryPos" Text="Create report" OnClick="btnFinalize_Click" />
            </div>
        </div>
        <div>
            <div id="oLinks" runat="server" style="float: left; clear: both">
            </div>
        </div>
    </form>
</body>
</html>