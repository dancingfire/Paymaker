<%@ Page Language="c#" Inherits="product_update" EnableViewState="True" AutoEventWireup="true" CodeFile="product_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Product update</title>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdItemID" runat="server" Value=""></asp:HiddenField>
        <asp:HiddenField ID="hdListTypeID" runat="server" Value=""></asp:HiddenField>
        <div class="ListHeader" style="font-size: 10pt; width: 600px; padding-top: 2px; top: 0px; height: 16px; margin-bottom: 10px">
            Product GL mapping details
        </div>
        <div style="width: 380px; float: left">
            <asp:Label ID="lblName" runat="server" CssClass="Label LabelPos">Name</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtName" runat="server" Text=""
                MaxLength="40" TabIndex="10"></asp:TextBox>
            <br class="Align" />

            <br class="Align" />
            <asp:Panel ID="pCreditGLCode" runat="server">
                <asp:Label ID="Label5" runat="server" CssClass="Label LabelPos">Credit GL Code</asp:Label>
                <asp:DropDownList ID="lstGLCode" runat="server" CssClass="Entry EntryPos" TabIndex="50">
                </asp:DropDownList>
                <br class="Align" />
            </asp:Panel>
            <asp:Panel ID="Panel1" runat="server">
                <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos" Text="Exclude from invoicing"></asp:Label>
                <asp:CheckBox CssClass="EntryPos" ID="chkExcludeFromInvoicing" runat="server" />
                <br class="Align" />
            </asp:Panel>
             <asp:Panel ID="Panel2" runat="server">
                <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos" Text="Exclude from MYOB export"></asp:Label>
                <asp:CheckBox CssClass="EntryPos" ID="chkExcludeFromMYOB" runat="server" />
                <br class="Align" />
            </asp:Panel>
        </div>
        <div class='LeftPanel' style="width: 95px; text-align: right">
            <asp:Button ID="btnUpdate" Style="width: 85px;" runat="server" Text="Update" CommandName="cancel"
                CssClass="Button btn" CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click"></asp:Button>
            <asp:Button ID="btnCancel" Style="width: 85px;" runat="server" Text="Cancel" CommandName="cancel"
                CssClass="Button btn" CausesValidation="False" TabIndex="300" OnClick="btnCancel_Click"></asp:Button>
        </div>
    </form>
</body>
</html>