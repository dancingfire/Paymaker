<%@ Page Language="c#" Inherits="salary_update" CodeFile="salary_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        $(document).ready(function () {
            createCalendar("txtStartDate");
            createCalendar("txtEndDate");
            addFormValidation('frmMain');
        });
    </script>
</head>
<body>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdHistoryID" runat="server" />
        <div style="float: right; width: 110px">
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn" Style='float: right' Width="100" OnClick="btnUpdate_Click" />
            <asp:Button ID="btnCreateNew" runat="server" Text="Create new" CssClass="Button btn TopMargin" Style='float: right; clear: right' OnClick="btnCreateNew_Click" Width="100" Visible="false" />
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="Button btn TopMargin" Style='float: right; clear: right' Width="100" OnClientClick="parent.closeSalary(false); return false;" />
        </div>
        <div style="width: 450px; float: left;">
            <asp:Label ID="lblStartDate" runat="server" CssClass="Label LabelPos" Text="Start date" required></asp:Label>
            <asp:TextBox ID="txtStartDate" runat="server" CssClass="Entry EntryPos" required></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos" Text="End date"></asp:Label>
            <asp:TextBox ID="txtEndDate" runat="server" CssClass="Entry EntryPos"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos" Text="Salary"></asp:Label>
            <asp:TextBox ID="txtSalary" runat="server" CssClass="Entry EntryPos"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos" Text="Team"></asp:Label>
            <asp:DropDownList ID="lstTeam" runat="server" CssClass="Entry EntryPos"></asp:DropDownList>
            <br class='Align' />
        </div>
        <div class="panel panel-default" style="margin-top: 10px; width: 68%; float: left">
            <div class="panel-heading">Employment history</div>
            <div class="panel-body">
                <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false">
                    <Columns>
                        <asp:BoundField DataField="StartDate" ItemStyle-Width="25%" HeaderText="Effective date" DataFormatString="{0:MMM d, yyyy}" />
                        <asp:BoundField DataField="EndDate" ItemStyle-Width="25%" HeaderText="Ending date" DataFormatString="{0:MMM d, yyyy}" />
                        <asp:BoundField DataField="Salary" ItemStyle-Width="20%" HeaderText="Salary" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Team" ItemStyle-Width="35%" HeaderText="Team" ItemStyle-HorizontalAlign="Center" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>