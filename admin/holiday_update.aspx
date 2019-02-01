<%@ Page Language="c#" Inherits="holiday_update" EnableViewState="True" AutoEventWireup="true" CodeFile="holiday_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>User update</title>
    <script type="text/javascript">
        function confirmDelete() {
            return confirm("Are you sure you want to delete this item?");
        }

        function validatePage() {
            if ($("#txtHolidayDate").val() == '') {
                alert("You must enter the holiday date");
                return false;
            } 
            return true;
        }

        $(document).ready(function () {
            createCalendar("txtHolidayDate", true);
            $("#txtHolidayDate").attr("readonly", "readonly")
        });
    </script>
</head>
<body style="margin-top: 15px">
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdItemID" runat="server" Value=""></asp:HiddenField>
        <div style="width: 380px; float: left" class="PageContent">
            <asp:Label ID="lblName" runat="server" CssClass="Label LabelPos">Name</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtName" runat="server" Text=""
                MaxLength="40" TabIndex="10"></asp:TextBox>
            <br class="Align" />
            <asp:Label ID="lblDate" runat="server" CssClass="Label LabelPos">Date</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtHolidayDate" runat="server" TabIndex="20" ></asp:TextBox>
            <br class="Align" />

           
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