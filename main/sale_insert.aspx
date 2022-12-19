<%@ Page Language="c#" Inherits="sale_insert" CodeFile="sale_insert.aspx.cs" ValidateRequest="false" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Sale update</title>
    <script type="text/javascript">

        function Close() {
            parent.refreshPage();
        }

        function Cancel() {
            parent.closeSale();
        }

        $(document).ready(function () {
            $(".RoundPanel").corner();
            createCalendar("txtSalesDate");
            createCalendar("txtEntitlementDate");
            createCalendar("txtSettlementDate");
            blUpdatePressed = false;

            $(".EntryPos").change(function () {
                $("#lstLocked").val("1");
            })

            $("#lstLocked").unbind("change");
        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self">
        <asp:HiddenField ID="hdSaleID" runat="server" Value="-1" />
        <div class='ActionPanel' style="width: 99%; float: left">
            <div class='RightActionPanel'>
                <asp:Button ID="btnUpdate" CssClass="Button btn" runat="server" Text="Update" UseSubmitBehavior="true" OnClick="btnUpdate_Click" OnClientClick="return updateSplit();" Width="80" />
                <asp:Button ID="btnCancel" CssClass="Button btn MarginTop" runat="server" Text="Cancel" OnClientClick="Cancel(); return false;" Width="80" UseSubmitBehavior="false" />
            </div>
            <div id="pPageHeader" class='PageHeader' runat="server" style='font-size: 11px; width: 80%; float: left;'>
                <div style='margin-right: 2%; float: left; width: 80%' class='RoundPanel Short'>
                    <asp:Label ID="Label6" CssClass="Label LabelPos" runat="server" Text="Code"></asp:Label>
                     <asp:TextBox ID="txtCode" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />
    
                    <asp:Label ID="Label8" CssClass="Label LabelPos" runat="server" Text="Address"></asp:Label>
                     <asp:TextBox ID="txtAddress" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />
                    <asp:Label ID="lblCommission" CssClass="Label LabelPos" runat="server" Text="Gross commission"></asp:Label>
                    <asp:TextBox ID="txtGrossCommission" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                      <br class='Align' />
                    <asp:Label ID="lblConjCommission" CssClass="Label LabelPos" runat="server" Text="Conj commission"></asp:Label>
                    <asp:TextBox ID="txtConjCommission" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>

                    <br class='Align' />
                    <asp:Label ID="Label5" CssClass="Label LabelPos" runat="server" Text="Sale price"></asp:Label>
                    <asp:TextBox ID="txtSalePrice" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />
                    
              
                    <asp:Label ID="lblSalesDate" CssClass="Label LabelPos" runat="server" Text="Sale date"></asp:Label>
                    <asp:TextBox ID="txtSalesDate" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />

                    <asp:Label ID="Label1" CssClass="Label LabelPos" runat="server" Text="Settlement date"></asp:Label>
                    <asp:TextBox ID="txtSettlementDate" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />

                    <asp:Label ID="lblEntitlementDate" CssClass="Label LabelPos" runat="server" Text="Entitlement date"></asp:Label>
                    <asp:TextBox ID="txtEntitlementDate" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />

                    <asp:Label ID="Label2" CssClass="Label LabelPos" runat="server" Text="PayPeriod"></asp:Label>
                    <asp:DropDownList ID="lstPayPeriod" runat="server" CssClass="Entry EntryPos">
                    </asp:DropDownList>

                    <asp:Label ID="Label3" CssClass="Label LabelPos" runat="server" Text="Status"></asp:Label>
                    <asp:DropDownList ID="lstStatus" runat="server" CssClass="Entry EntryPos" Width="120px">
                        <asp:ListItem Text="Incomplete" Value="0"></asp:ListItem>
                        <asp:ListItem Text="Completed" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Finalized" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Hidden" Value="3"></asp:ListItem>
                    </asp:DropDownList>
                    <br class='Align' />

                    <asp:Label ID="Label4" CssClass="Label LabelPos" runat="server" Text="Locked"></asp:Label>
                    <asp:DropDownList ID="lstLocked" runat="server" CssClass="Entry EntryPos" Width="120px">
                        <asp:ListItem Text="Unlocked" Value="0"></asp:ListItem>
                        <asp:ListItem Text="Locked" Value="1"></asp:ListItem>
                    </asp:DropDownList>
                    <br class='Align' />

                    <asp:Label ID="Label7" CssClass="Label LabelPos" runat="server" Text="Comments"></asp:Label>
                    <asp:TextBox ID="txtComments" runat="server" CssClass="Normal EntryPos" TextMode="MultiLine" Height="200" Width="60%"></asp:TextBox>
                </div>
            </div>
            <br class='Align' />
        </div>
    </form>
</body>
</html>