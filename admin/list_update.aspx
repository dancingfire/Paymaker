<%@ Page Language="c#" Inherits="list_update" EnableViewState="True" AutoEventWireup="true" CodeFile="list_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>User update</title>
    <script type="text/javascript">
        function confirmDelete() {
            return confirm("Are you sure you want to delete this item?");
        }

        function checkValidation() {
            $("#txtAmount").removeClass("numbersOnly").removeClass("percent").unbind("keypress");
            if ($("#lstAmountValue").val() == "1") {//"%" is selected
                $("#txtAmount").addClass("percent");
            } else {
                $("#txtAmount").addClass("numbersOnly");
            }
            addValidation();
        }

        function validatePage() {
            if (isRequiredFieldNotValid("txtName", 1)) {
                alert("You must enter a value for the Name field");
                return false;
            } else if (isRequiredFieldNotValid("txtCreditGLCode", 1)) {
                alert("You must enter a value for the Credit GL Code ");
                return false;
            }
            return true;
        }

        $(document).ready(function () {
            checkValidation();
            addValidation();
            $(".Entry").focus(function () {
                // only select if the text has not changed
                if (this.value == this.defaultValue) {
                    this.select();
                }
            });
        });
    </script>
</head>
<body style="margin-top: 15px">
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdItemID" runat="server" Value=""></asp:HiddenField>
        <asp:HiddenField ID="hdListTypeID" runat="server" Value=""></asp:HiddenField>
        <div style="width: 380px; float: left" class="PageContent">
            <asp:Label ID="lblName" runat="server" CssClass="Label LabelPos">Name</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtName" runat="server" Text=""
                MaxLength="40" TabIndex="10"></asp:TextBox>
            <br class="Align" />
            <asp:Label ID="lblDescription" runat="server" CssClass="Label LabelPos">Description</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtDescription" runat="server" Text=""
                TabIndex="20" MaxLength="40" TextMode="MultiLine"></asp:TextBox>
            <br class="Align" />

            <asp:Panel ID="pDefaultAmount" runat="server" Visible="false">
                <asp:Label ID="Label11" runat="server" CssClass="Label LabelPos">Default amount</asp:Label>
                <asp:TextBox ID="txtAmount" runat="server" CssClass="Entry EntryPos"
                    Text="" Style="width: 150px; margin-right: 5px" TabIndex="30"></asp:TextBox>
                <asp:DropDownList ID="lstAmountValue" onchange="checkValidation();" runat="server"
                    CssClass="Entry EntryPos" Style="width: 40px" TabIndex="40">
                    <asp:ListItem Text="$" Value="0"></asp:ListItem>
                    <asp:ListItem Text="%" Value="1"></asp:ListItem>
                </asp:DropDownList>
                <br class="Align" />
            </asp:Panel>

            <asp:Label ID="Label10" runat="server" CssClass="Label LabelPos">Status</asp:Label>
            <asp:DropDownList ID="lstStatus" runat="server" CssClass="Entry EntryPos"
                TabIndex="50">
                <asp:ListItem Text="Active" Value="1"></asp:ListItem>
                <asp:ListItem style="color: red" Text="In-active" Value="0"></asp:ListItem>
            </asp:DropDownList>

            <asp:Label ID="Label6" runat="server" CssClass="Label LabelPos">Sort order</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos numbersOnly" ID="txtSortOrder" runat="server" Text=""
                MaxLength="40" TabIndex="5"></asp:TextBox>

            <br class="Align" />
            <asp:Panel ID="pCreditGLCode" runat="server" Visible="false">
                <asp:Label ID="Label5" runat="server" CssClass="Label LabelPos">Credit GL Code</asp:Label>
                <asp:TextBox ID="txtCreditGLCode" runat="server" CssClass="Entry EntryPos" MaxLength="50"
                    Text="" TabIndex="60"></asp:TextBox>
                <br class="Align" />
            </asp:Panel>
            <asp:Panel ID="pDebitGLCode" runat="server" Visible="false">
                <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos">Debit GL Code</asp:Label>
                <asp:TextBox ID="txtDebitGLCode" runat="server" CssClass="Entry EntryPos" MaxLength="50"
                    Text="" TabIndex="70"></asp:TextBox>
                <br class="Align" />
            </asp:Panel>

            <asp:Panel ID="pCompany" runat="server" Visible="false">
                <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos">Company</asp:Label>
                <asp:DropDownList ID="lstCompany" runat="server" CssClass="Entry EntryPos"
                    TabIndex="80">
                </asp:DropDownList>
                <br class="Align" />
            </asp:Panel>
            <asp:Panel ID="pJobCode" runat="server" Visible="false">
                <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos">Job #</asp:Label>
                <asp:TextBox ID="txtJobCode" runat="server" CssClass="Entry EntryPos" MaxLength="50"
                    Text="" TabIndex="90"></asp:TextBox>
                <br class="Align" />

                <asp:Label ID="Label4" runat="server" CssClass="Label LabelPos">Advertising job #</asp:Label>
                <asp:TextBox ID="txtAdvertisingJobCode" runat="server" CssClass="Entry EntryPos" MaxLength="50"
                    Text="" TabIndex="90"></asp:TextBox>
                <br class="Align" />
            </asp:Panel>
        </div>
        <div class='LeftPanel' style="width: 95px; text-align: right">
            <asp:Button ID="btnUpdate" Style="width: 85px;" runat="server" Text="Update"
                CssClass="Button btn" CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click" OnClientClick="return validatePage()"></asp:Button>
            <asp:Button ID="btnCancel" Style="width: 85px;" runat="server" Text="Cancel"
                CssClass="Button TopSpace btn" CausesValidation="False" TabIndex="300" OnClick="btnCancel_Click"></asp:Button>
            <asp:Button ID="btnDelete" Style="width: 85px;" runat="server" Text="Delete"
                CssClass="Button TopSpace btn" CausesValidation="False" TabIndex="500" OnClick="btnDelete_Click"
                OnClientClick="return confirmDelete();"></asp:Button>
        </div>
    </form>
</body>
</html>