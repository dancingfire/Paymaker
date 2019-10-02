<%@ Page Language="c#" Inherits="user_login_as" EnableViewState="True" AutoEventWireup="true" CodeFile="user_login_as.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>User login</title>
    <script type="text/javascript">

        function loadPage() {
            $("#lstUser").change(function () {
                if ($("#lstUser").val() > -1)
                    enable($("#btnLogin"));
                else
                    disable($("#btnLogin"));
            });
            disable($("#btnLogin"));
        }
    </script>
</head>
<body onload="loadPage()">
    <form id="frmMain" method="post" runat="server">
        <div style="width: 70%; float: left; overflow: hidden">
            <div class='Label LabelPos'>Person: </div>
            <asp:DropDownList ID="lstUser" runat="server" CssClass="Entry EntryPos" requried></asp:DropDownList>
            <br class='Align' />
        </div>

        <div class='RightActionPanel' style="width: 29%; text-align: center; height: 280px; padding-top: 25px">
            <asp:Button ID="btnLogin" Style="width: 120px;" runat="server" Text="Login" CssClass="Button" CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click" />
            <br />
            <asp:Button ID="btnLoginSelf" Style="width: 120px;" runat="server" Text="Login as myself" OnClick="btnLoginSelf_Click"
                CssClass="Button TopMargin" CausesValidation="False" TabIndex="300" />
            <br />
            <asp:Button ID="btnClose" Style="width: 120px;" runat="server" Text="Close"
                CssClass="Button TopMargin" CausesValidation="False" TabIndex="300" OnClientClick="parent.closeLoginAs(); return false;" />
        </div>
    </form>
</body>
</html>