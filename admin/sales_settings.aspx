<%@ Page Language="c#" Inherits="sales_settings" CodeFile="sales_settings.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">

        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        function validateHistoryEntry(dateID, valueID) {
            var date = document.getElementById(dateID).value;
            var value = document.getElementById(valueID).value;
            if (date.trim() === '' || value.trim() === '') {
                alert('Please enter both an effective date and a value.');
                return false;
            }
            return true;
        }

        $(document).ready(function () {
            addFormValidation('frmMain');
            createCalendar("txtNewSuperPercentageDate");
            createCalendar("txtNewSuperMaxDate");
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

            <asp:Label ID="Label4" runat="server" CssClass="Label LabelPos" Text="Super GL code" Style="width: 200px" ToolTip="The GL code for the super amount"></asp:Label>
            <asp:TextBox ID="txtSuperGLCode" runat="server" CssClass="Entry EntryPos required"></asp:TextBox>
            <br class='Align' />

            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos" Text="Calculate bonus" Style="width: 200px" ToolTip="Calculate and show the bonuses on the commission statements"></asp:Label>
            <asp:DropDownList ID="lstCalcBonus" runat="server" CssClass="Entry EntryPos">
                <asp:ListItem Text="False" Value="FALSE"></asp:ListItem>
                <asp:ListItem Text="True" Value="TRUE"></asp:ListItem>
            </asp:DropDownList>
            <br class='Align' />

              <asp:Label ID="Label5" runat="server" CssClass="Label LabelPos" Text="Leave process testers" Style="width: 200px" ToolTip="Select the people you want to be able to see the leave testing"></asp:Label>
             <asp:ListBox ID="lstPermittedUsers" runat="server" CssClass="Entry EntryPos" SelectionMode="Multiple"  >
            </asp:ListBox>

            <br class='Align' />
            <asp:Button ID="btnDelete" runat="server" Text="Remove all bonus records" CssClass="Button btn" OnClick="btnDeleteBonus_Click"  Visible="false"/>
        </div>
        <div class='AdminActionPanel' style='float: left; text-align: right; width: 100px'>
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn" OnClick="btnUpdate_Click" />
        </div>

        <div class="panel panel-default" style="margin-top: 10px; width: 30%; float: left; clear: both;">
            <div class="panel-heading">Superannuation percentage history</div>
            <div class="panel-body">
                <asp:GridView ID="gvSuperPercentageHistory" runat="server" AutoGenerateColumns="false" DataKeyNames="ID" OnRowDeleting="gvSuperPercentageHistory_RowDeleting">
                    <Columns>
                        <asp:BoundField DataField="EffectiveFrom" ItemStyle-Width="40%" HeaderText="Effective from" DataFormatString="{0:MMM d, yyyy}" />
                        <asp:BoundField DataField="Value" ItemStyle-Width="40%" HeaderText="Percentage" ItemStyle-HorizontalAlign="Center" />
                        <asp:TemplateField ItemStyle-Width="20%" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="Delete" Text="Delete" CausesValidation="false" OnClientClick="return confirm('Delete this entry?');"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <div style="margin-top: 10px;">
                    <asp:TextBox ID="txtNewSuperPercentageDate" runat="server" CssClass="Entry"></asp:TextBox>
                    <asp:TextBox ID="txtNewSuperPercentageValue" runat="server" CssClass="Entry number"></asp:TextBox>
                    <asp:Button ID="btnAddSuperPercentage" runat="server" Text="Add" CssClass="Button btn" OnClick="btnAddSuperPercentage_Click" OnClientClick="return validateHistoryEntry('<%= txtNewSuperPercentageDate.ClientID %>', '<%= txtNewSuperPercentageValue.ClientID %>');" />
                </div>
            </div>
        </div>

        <div class="panel panel-default" style="margin-top: 10px; width: 30%; float: left; clear: both;">
            <div class="panel-heading">Super maximum (monthly) history</div>
            <div class="panel-body">
                <asp:GridView ID="gvSuperMaxHistory" runat="server" AutoGenerateColumns="false" DataKeyNames="ID" OnRowDeleting="gvSuperMaxHistory_RowDeleting">
                    <Columns>
                        <asp:BoundField DataField="EffectiveFrom" ItemStyle-Width="40%" HeaderText="Effective from" DataFormatString="{0:MMM d, yyyy}" />
                        <asp:BoundField DataField="Value" ItemStyle-Width="40%" HeaderText="Max ($)" ItemStyle-HorizontalAlign="Center" />
                        <asp:TemplateField ItemStyle-Width="20%" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="Delete" Text="Delete" CausesValidation="false" OnClientClick="return confirm('Delete this entry?');"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <div style="margin-top: 10px;">
                    <asp:TextBox ID="txtNewSuperMaxDate" runat="server" CssClass="Entry"></asp:TextBox>
                    <asp:TextBox ID="txtNewSuperMaxValue" runat="server" CssClass="Entry number"></asp:TextBox>
                    <asp:Button ID="btnAddSuperMax" runat="server" Text="Add" CssClass="Button btn" OnClick="btnAddSuperMax_Click" OnClientClick="return validateHistoryEntry('<%= txtNewSuperMaxDate.ClientID %>', '<%= txtNewSuperMaxValue.ClientID %>');" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>