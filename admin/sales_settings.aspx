<%@ Page Language="c#" Inherits="sales_settings" CodeFile="sales_settings.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">

        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        $(document).ready(function () {
            addFormValidation('frmMain');
        });
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Application settings
        </div>
        <div style='float: left; width: 40%;'>
            <asp:Label ID="lblCurrentPayPeriod" runat="server" CssClass="Label LabelPos" Text="Retainer threshold" Style="width: 200px" ToolTip="The amount that will be paid out if there is not enough income on the month"></asp:Label>
            <asp:TextBox ID="txtRetainerAmount" runat="server" CssClass="Entry EntryPos number required"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos" Text="Super annuation percentage" Style="width: 200px" ToolTip="The percentage that comes off the agent's pay for superannuation"></asp:Label>
            <asp:TextBox ID="txtSuperPercentage" runat="server" CssClass="Entry EntryPos number required"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos" Text="Super maximum" Style="width: 200px" ToolTip="The maximum annual amount that can be contributed to super "></asp:Label>
            <asp:TextBox ID="txtSuperMax" runat="server" CssClass="Entry EntryPos number required"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos" Text="Calculate bonus" Style="width: 200px" ToolTip="Calculate and show the bonuses on the commission statements"></asp:Label>
            <asp:DropDownList ID="lstCalcBonus" runat="server" CssClass="Entry EntryPos">
                <asp:ListItem Text="False" Value="FALSE"></asp:ListItem>
                <asp:ListItem Text="True" Value="TRUE"></asp:ListItem>
            </asp:DropDownList>
            <br class='Align' />
            <asp:Button ID="btnDelete" runat="server" Text="Remove all bonus records" CssClass="Button btn" OnClick="btnDeleteBonus_Click" />
        </div>
        <div class='AdminActionPanel' style='float: left; text-align: right; width: 100px'>
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn" OnClick="btnUpdate_Click" />
        </div>
    </form>
</body>
</html>