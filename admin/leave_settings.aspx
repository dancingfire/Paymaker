<%@ Page Language="c#" Inherits="leave_settings" CodeFile="leave_settings.aspx.cs" %>

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
              $("#lstPermittedUsers").select2();
            $("#frmMain").submit(function () {
                $("#hdLeaveTesters").val($("#lstPermittedUsers").val());
            })
        });
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdLeaveTesters" runat="server" />
        
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Application settings
        </div>
        <div style='float: left; width: 40%;'>
            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos" Text="Catch all email" Style="width: 200px" ToolTip="AN email address will be sent to this address if the user has no manager"></asp:Label>
            <asp:TextBox ID="txtCatchallEmail" runat="server" CssClass="Entry EntryPos email required"></asp:TextBox>
            <br class='Align' />

            
              <asp:Label ID="Label5" runat="server" CssClass="Label LabelPos" Text="Leave process testers" Style="width: 200px" ToolTip="Select the people you want to be able to see the leave testing"></asp:Label>
             <asp:ListBox ID="lstPermittedUsers" runat="server" CssClass="Entry EntryPos" SelectionMode="Multiple"  >
            </asp:ListBox>

     </div>
        <div class='AdminActionPanel' style='float: left; text-align: right; width: 100px'>
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn" OnClick="btnUpdate_Click" />
        </div>
    </form>
</body>
</html>