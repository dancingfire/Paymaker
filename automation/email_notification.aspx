<%@ Page Language="C#" AutoEventWireup="true" CodeFile="email_notification.aspx.cs" Inherits="email_notification" ValidateRequest="false" EnableEventValidation="false" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Email notification</title>
</head>
<body>
    <form runat="server" id="frmMain">
        <asp:Button ID="btnSendMorning" runat="server" Text="Check for automated emails" OnClick="btnSendMorning_Click" />
        <div id="dOutput" runat="server" />
    </form>
</body>
</html>