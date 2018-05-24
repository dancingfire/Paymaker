<%@ Page Language="c#" Inherits="sale_modification" CodeFile="sale_modification.aspx.cs" EnableViewState="true" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script>
        function checkFields() {
            return true;
        }
    </script>
</head>
<body>

    <form id="frmMain" method="post" runat="server" target="_self" defaultbutton="btnSearch" onsubmit="return checkFields()">
        <div class='ActionPanel'>
            <asp:Panel ID="pPageHeader" class='PageHeader' runat="server">Property administration</asp:Panel>
        </div>
        <div class="Instruction">
            You can change the property sales period using this page. Enter the code you are searching for, and press <i>Search</i>.
        Click <em>Edit</em> to modify a specific sale.
        Once you have made your changes, click Update.
        </div>

        <div style="float: left; clear: right">
            <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel" Style="width: 650px; margin-bottom: 10px"></asp:Panel>

            <asp:Label ID="lblPropertyFilter" class='Label' runat="server" Text="Property code"></asp:Label>
            <asp:TextBox ID="txtPropertyFilter" class='Edit' runat="server"
                Style="width: 426px;"></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-default"
                OnClick="btnSearch_Click" />
        </div>

        <asp:Panel ID="pDetails" runat="server" Visible="false">

            <asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="false" Style="width: 750px; cursor: pointer"
                OnRowDataBound="gvSales_RowDataBound" DataKeyNames="ID" ClientIDMode="Predictable">
                <Columns>
                    <asp:BoundField DataField="CODE" HeaderText="Code" ReadOnly="true" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="100" />
                    <asp:BoundField DataField="ADDRESS" HeaderText="Address" ReadOnly="true" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="300" />
                    <asp:BoundField DataField="ENTITLEMENTDATE" HeaderText="Entitlement date" ReadOnly="true" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="100" />
                    <asp:TemplateField HeaderText="Status" HeaderStyle-Width="100" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server" Text='<%# getStatus(Convert.ToInt32(Eval("STATUSID"))) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Pay Period" HeaderStyle-Width="100" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Label ID="lblPayPeriodID" runat="server" Text='<%# Eval("PAYPERIOD") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Locked" HeaderStyle-Width="60" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Label ID="lblLocked" runat="server" Text='<%# getLocked(Convert.ToInt32(Eval("LOCKCOMMISSION"))) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>
        <asp:HiddenField ID="hdSaleID" runat="server" />
    </form>
</body>
</html>