<%@ Page Language="c#" Inherits="campaign_settings" CodeFile="campaign_settings.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">

        function closePage() {
            document.location.href = "../welcome.aspx";
        }
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Campaign admin settings
        </div>
        <div style='float: left; width: 450px;'>
            <asp:Label ID="lblCurrentPayPeriod" runat="server" CssClass="Label LabelPos" Text="Prepayment number of days" Style="width: 200px"></asp:Label>
            <img src='../sys_images/help.gif' style='float: left; margin-left: -25px' title='The number of days during which payments count as prepayments calculated from the date of listing. (Note: Sales that have an auction date use the auction date as the cutoff date.)' />
            <asp:TextBox ID="txtNumberOfDays" runat="server" CssClass="Entry EntryPos"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos" Text="Invoice total threshold" Style="width: 200px"></asp:Label>
            <img src='../sys_images/help.gif' style='float: left; margin-left: -25px' title='When a campaign has this outstanding amount, it becomes a candidate for partial invoicing' />

            <asp:TextBox ID="txtInvoiceThreshold" runat="server" CssClass="Entry EntryPos"></asp:TextBox>
        </div>
        <div class='AdminActionPanel' style='float: left; text-align: right; width: 100px'>
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn" Style='float: right' OnClick="btnUpdate_Click" />
        </div>
    </form>
</body>
</html>