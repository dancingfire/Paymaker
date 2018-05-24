<%@ Page Language="c#" Inherits="user_account_update" EnableViewState="True" AutoEventWireup="true"
    CodeFile="user_account_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Transaction update</title>
    <script type="text/javascript">
        function confirm_delete() {
            return confirm("Are you sure you want to delete this transaction?");
        }

        function cancelPage() {
            document.location.href = "welcome.aspx";
            return false;
        }

        $(document).ready(function () {
            addValidation();
            $(".Entry").focus(function () {
                // only select if the text has not changed
                if (this.value == this.defaultValue) {
                    this.select();
                }
            });

            var tabindex = 1;
            $('input,select').each(function () {
                if (this.type != "hidden") {
                    var $input = $(this);
                    $input.attr("tabindex", tabindex);
                    tabindex++;
                }
            });
        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdTXID" runat="server" />
        <div class="ListHeader" style="margin-top: 10px">
            User account budget details
        </div>
        <div style="width: 460px; float: left;">
            <asp:Label ID="Label10" runat="server" CssClass="Label LabelPos">User</asp:Label>
            <asp:DropDownList ID="lstUserID" runat="server" CssClass="Entry EntryPos" OnSelectedIndexChanged="lstUserID_SelectedIndexChanged" AutoPostBack="true">
                <asp:ListItem Text="Active" Value="1"></asp:ListItem>
                <asp:ListItem style="color: red" Text="In-active" Value="0"></asp:ListItem>
            </asp:DropDownList>
            <br class="Align" />
            <asp:Panel ID="pSalesTarget" runat="server" Visible="false">
                <asp:Label ID="Label8" runat="server" CssClass="Label LabelPos">Sales target</asp:Label>
                <asp:TextBox CssClass="Entry EntryPos" ID="txtSalesTarget" runat="server" Text=""
                    MaxLength="80"></asp:TextBox>
                <br class="Align" />
            </asp:Panel>
            <div id='dAccountHTML' runat="server" style="height: 400px; overflow: auto; width: 420px;"></div>

            <div id='dAccountHTMLInsert' visible="false" runat="server" style="height: 400px; overflow: auto; width: 420px;">
                <asp:Label ID="Label1" CssClass="label LabelPos" runat="server" Text="Amount"></asp:Label>
                <asp:TextBox ID="txtCode" TabIndex="20" CssClass="Entry EntryPos" runat="server"></asp:TextBox>
                <br class='Align' />
                <asp:Label ID="Label2" CssClass="label LabelPos" runat="server" Text="Comment"></asp:Label>
                <asp:TextBox ID="txtComments" Width="250px" Height="100px" runat="server" Text=""
                    TextMode="MultiLine" TabIndex="30" CssClass="Entry" MaxLength="1000"></asp:TextBox>
                <br class='Align' />
                <asp:Label ID="Label3" CssClass="label LabelPos" runat="server" Text="Financial Year"></asp:Label>
                <asp:DropDownList ID="lstFinancialYear" TabIndex="40" CssClass="Entry EntryPos" runat="server">
                    <asp:ListItem Text="2010" Value="2010"></asp:ListItem>
                    <asp:ListItem Text="2111" Value="2011"></asp:ListItem>
                    <asp:ListItem Text="2012" Value="2012"></asp:ListItem>
                    <asp:ListItem Text="2013" Value="2013"></asp:ListItem>
                    <asp:ListItem Text="2014" Value="2014"></asp:ListItem>
                    <asp:ListItem Text="2015" Value="2015"></asp:ListItem>
                </asp:DropDownList>
                <br class='Align' />
            </div>
        </div>
        <div class='LeftPanel' style='width: 100px'>
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn"
                CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click"></asp:Button>
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="Button btn"
                CausesValidation="False" TabIndex="300" OnClientClick="return cancelPage();"></asp:Button>
        </div>
    </form>
</body>
</html>