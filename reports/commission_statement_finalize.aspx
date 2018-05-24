<%@ Page Language="c#" Inherits="Paymaker.commission_statement_finalize" CodeFile="commission_statement_finalize.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Commission statement finalize</title>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <div id="pPageHeader" class='PageHeader' runat="server">
            Finalize Commission Statement
        </div>
        <div class='ActionPanel' style="width: 99%">
            <div style='width: 49%; float: left'>
                <span id="spPayPeriod">
                    <asp:Label ID="Label3" CssClass="Label LabelPos" runat="server" Text="Pay period">
                    </asp:Label>
                    <asp:DropDownList ID="lstPayPeriod" CssClass="Entry EntryPos" runat="server">
                    </asp:DropDownList>
                    <br class='Align' />
                    <asp:Label ID="Label1" CssClass="Label LabelPos" runat="server" Text="Select records">
                    </asp:Label>
                    <asp:DropDownList ID="lstRecords" CssClass="Entry EntryPos" runat="server">
                        <asp:ListItem Text="All" Value=""></asp:ListItem>
                        <asp:ListItem Text="Junior sales bonus" Value="JuniorSalesBonus"></asp:ListItem>
                        <asp:ListItem Text="Senior sales bonus scheme" Value="SeniorSBS"></asp:ListItem>
                    </asp:DropDownList>
                    <br class='Align' />
                    <asp:Label ID="Label2" CssClass="Label LabelPos" runat="server" Text="Select Company">
                    </asp:Label>
                    <asp:DropDownList ID="lstCompany" CssClass="Entry EntryPos" runat="server">
                        <asp:ListItem Text="All" Value=""></asp:ListItem>
                    </asp:DropDownList>
                    <br class='Align' />
                </span>
            </div>
            <div id='dButtons' style='width: 160px; float: left; display: inline'>
                <asp:Button ID="btnFinalize" runat="server" Width="150px" CssClass="Button EntryPos" Text="Finalize Commission" OnClick="btnFinalize_Click" />
                <asp:Button ID="Button1" runat="server" Width="150px" CssClass="Button EntryPos" Text="Preview commission" OnClick="btnPreview_Click" Style="margin-top: 10px" />
            </div>
        </div>
        <div style="width: 100%">
            <div id="oLinks" runat="server">
            </div>
        </div>

        <asp:GridView ID="gvData" runat="server" Visible="false" Style="float: left; clear: both" EnableViewState="false"></asp:GridView>
    </form>
</body>
</html>