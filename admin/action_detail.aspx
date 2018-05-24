<%@ Page Language="c#" Inherits="action_detail" CodeFile="action_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function viewItem(intID) {
            $("#frmMain").attr("action", "action_update.aspx?intItemID=" + intID).submit();
        }

        function refreshPage() {
            document.location.href = document.location.href;
        }

        function closePage() {
            document.location.href = "../welcome.aspx";
        }
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server" target="frmUpdate">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            <asp:Label ID="lblItemName" runat="server" Text="Label"></asp:Label>
        </div>

        <div style='float: left; width: 30%;'>
            <table width="100%" border="0">
                <tr>
                    <td class="ListHeader" width="100%">Name</td>
                </tr>
            </table>

            <div class="ListContainer" style="overflow: auto; width: 100%; height: 457px; float: left">
                <asp:DataList ID="dlList" runat="server" Width="95%" BackColor="White">
                    <SelectedItemStyle CssClass="RowSelected"></SelectedItemStyle>
                    <ItemTemplate>
                        <table width="100%" border="0" onmouseover="this.className='RowMouseOver'" onmouseout="this.className='Row'" class="Row">
                            <tr onclick='javascript:viewItem(<%# DataBinder.Eval(Container.DataItem, "ID") %>)'>
                                <td width="100%">
                                    <%# DataBinder.Eval(Container.DataItem, "Name")%>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </div>
            <div class='AdminActionPanel' style='float: left; text-align: right; width: 100%'><a href='javascript: viewItem(-1)'>
                <asp:Label ID="lblInsertText" runat="server" Text="Add a new action" CssClass="LinkButton"></asp:Label></a></div>
        </div>
        <iframe id='frmUpdate' name='frmUpdate' class='AdminUpdateFrame' frameborder='0' src='about:blank'></iframe>
    </form>
</body>
</html>