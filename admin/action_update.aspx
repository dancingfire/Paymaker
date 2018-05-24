<%@ Page Language="c#" Inherits="action_update" EnableViewState="True" AutoEventWireup="true" CodeFile="action_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Action update</title>
    <script type="text/javascript">
        function confirmDelete() {
            return confirm("Are you sure you want to delete this unused action?");
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdItemID" runat="server" Value=""></asp:HiddenField>
        <asp:HiddenField ID="hdListTypeID" runat="server" Value=""></asp:HiddenField>
        <div class="ListHeader" style="font-size: 10pt; width: 600px; padding-top: 2px; top: 0px; height: 16px; margin-bottom: 10px">
            Action details
        </div>
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" />
        <div style="width: 380px; float: left">
            <asp:Label ID="lblName" runat="server" CssClass="Label LabelPos">Name</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtName" runat="server" Text=""
                MaxLength="40" TabIndex="10"></asp:TextBox>
            <br class="Align" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="Name is a required field" ControlToValidate="txtName" Display="None"></asp:RequiredFieldValidator>

            <asp:Label ID="Label4" runat="server" CssClass="Label LabelPos">Email subject</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtEmailSubject" runat="server" Text=""
                MaxLength="40" TabIndex="10"></asp:TextBox>
            <br class="Align" />

            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos" Text="Active"></asp:Label>
            <asp:CheckBox CssClass="EntryPos" ID="chkActive" runat="server" Checked="true" />
            <br class="Align" />

            <asp:Label ID="Label7" CssClass="label
            LabelPos"
                runat="server" Text="Email template"></asp:Label>
            <asp:DropDownList ID="lstTemplate" runat="server" CssClass="Entry EntryPos">
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="Please select a template" ControlToValidate="lstTemplate" Display="None"></asp:RequiredFieldValidator>
            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos">Default reminder (days)</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtDays" runat="server" Text="" MaxLength="40" TabIndex="10"></asp:TextBox>
        </div>
        <div class='LeftPanel' style="width: 95px; text-align: right">
            <asp:Button ID="btnUpdate" Style="width: 85px;" runat="server" Text="Update"
                CssClass="Button btn" TabIndex="100" OnClick="btnUpdate_Click"></asp:Button>
            <asp:Button ID="btnDelete" Style="width: 85px;" runat="server" Text="Delete"
                CssClass="Button btn" CausesValidation="False" TabIndex="300" Visible="false" OnClientClick="return confirmDelete()"
                OnClick="btnDelete_Click"></asp:Button>
            <asp:Button ID="btnCancel" Style="width: 85px;" runat="server" Text="Cancel"
                CssClass="Button btn" CausesValidation="False" TabIndex="300" OnClick="btnCancel_Click"></asp:Button>
        </div>
    </form>
</body>
</html>