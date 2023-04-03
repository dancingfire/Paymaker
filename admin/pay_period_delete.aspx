<%@ Page Language="c#" Inherits="pay_period_delete" CodeFile="pay_period_delete.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">

        function closePage() {
            document.location.href = "../welcome.aspx";
        }
        $(document).ready(function () {
            createCalendar("txtStartDate"); createCalendar("txtEndDate");
        });
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Set current pay period
        </div>
        <div style='float: left; width: 30%;'>
            The current pay period is:
        <asp:Label ID="lblCurrentPayPeriod" runat="server" Text="Currently not set" Style='font-weight: bold'></asp:Label>
            <div class='AdminActionPanel' style='float: left; text-align: right; width: 100%'>
                <a href='javascript: togglePayPeriod(true)'>Create new pay period</a>
            </div>
        </div>
        <div style='float: left; width: 10%'>
        </div>
        <div id="oSetPayPeriod" runat="server" visible="false" style='float: left; clear: none; width: 50%; border: 2px solid red'>
            <div>
                This is only visible to our (Username Gord login)
            </div>
            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos" Text="Set working pay period"></asp:Label>
            <asp:DropDownList ID="lstPayPeriod" runat="server" CssClass="Entry EntryPos">
            </asp:DropDownList>
            <asp:Button ID="btnDelete" runat="server" Text="Delete Pay period"
                CssClass="Button btn" Width="170px" Style='float: right' OnClick="btnDelete_Click" />
            <br class='Align' />
        </div>
    </form>
</body>
</html>