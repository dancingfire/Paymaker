<%@ Page Language="c#" Inherits="user_settings_detail" CodeFile="user_settings_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <link href="../Paymaker.css" type="text/css" rel="stylesheet">
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <div class="PageHeader" style="z-index: 109; left: -1px; width: 100%; position: absolute; top: 1px">Personal preferences </div>
        <div class="Normal" style="display: inline; z-index: 106; left: 16px; width: 480px; position: absolute; top: 32px; height: 24px">Modify your user settings below</div>

        <div class="Label" style="display: inline; font-size: 10pt; z-index: 108; left: 7px; width: 244px; padding-top: 2px; position: absolute; top: 87px; height: 23px">Prompt before printing labels?</div>
        <asp:ListBox ID="lstPromptBeforePrintingLabels" Style="z-index: 100; left: 256px; position: absolute; top: 88px" runat="server" Rows="1" CssClass="Entry" Width="229px">
            <asp:ListItem Text="No" Value="No"></asp:ListItem>
            <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
        </asp:ListBox>

        <div class="Label" style="display: inline; font-size: 10pt; z-index: 107; left: 7px; width: 244px; padding-top: 2px; position: absolute; top: 60px; height: 24px">Disable sample label printing</div>
        <asp:ListBox ID="lstCancelEntryPrinting" Style="z-index: 102; left: 256px; position: absolute; top: 60px" runat="server" Width="229px" CssClass="Entry" Rows="1">
            <asp:ListItem Text="No" Value="No"></asp:ListItem>
            <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
        </asp:ListBox>

        <div class="Label" style="display: inline; font-size: 10pt; z-index: 110; left: 8px; width: 244px; padding-top: 2px; position: absolute; top: 307px; height: 23px">Look & feel</div>
        <asp:ListBox ID="lstLookAndFeel" Style="z-index: 112; left: 255px; position: absolute; top: 305px" runat="server" Rows="1" CssClass="Entry" Width="229px">
            <asp:ListItem Text="Normal (green)" Value=""></asp:ListItem>
            <asp:ListItem Text="Autumn (orange)" Value="Autumn"></asp:ListItem>
        </asp:ListBox>

        <asp:Button ID="btnInsert" Style="z-index: 103; left: 504px; position: absolute; top: 62px" runat="server" Width="70px" CssClass="Button btn" Text="Update" OnClick="btnInsert_Click"></asp:Button>
        <asp:Button ID="btnClose" Style="z-index: 104; left: 504px; position: absolute; top: 96px" runat="server" Width="70px" CssClass="Button btn" Text="Close" OnClick="btnClose_Click"></asp:Button>

        <div class="Label" style="display: inline; font-size: 10pt; z-index: 111; left: 8px; width: 244px; padding-top: 2px; position: absolute; top: 120px; height: 23px">Email signature</div>

        <asp:TextBox ID="txtSignature" Style="z-index: 105; left: 256px; position: absolute; top: 120px" runat="server" Width="232px" CssClass="Entry" Height="176px" TextMode="MultiLine"></asp:TextBox>
    </form>
</body>
</html>