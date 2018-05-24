<%@ Page Language="c#" Inherits="debug_detail" CodeFile="debug_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
</head>
<body>
    <div class="PageHeader" style="z-index: 103; left: -1px; width: 100%; position: absolute; top: -1px">
        Debug&nbsp;admin
    </div>
    <form id="frmMain" method="post" runat="server">
        <asp:Button ID="btnClear" Style="z-index: 102; left: 918px; position: absolute; top: 35px"
            AccessKey="L" runat="server" Text="Clear" CssClass="Button btn" OnClick="btnInsert_Click"></asp:Button>
        <div style="z-index: 100; left: 8px; overflow: auto; width: 900px; position: absolute; top: 34px; height: 550px"
            class="ListContainer">
            <asp:DataList ID="dlList" runat="server" Width="860px" EnableViewState="False">
                <SelectedItemStyle CssClass="RowSelected"></SelectedItemStyle>
                <ItemTemplate>
                    <table width="860px" border="0" onmouseover="this.className='RowMouseOver'" onmouseout="this.className='Row'"
                        class="Row" cellspacing="0">
                        <tr>
                            <td width="100px">
                                <%# DataBinder.Eval(Container.DataItem, "BATCHID") %>
                            </td>
                            <td width="740px">
                                <%# DataBinder.Eval(Container.DataItem, "SQL") %>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
            </asp:DataList>
        </div>
        <input id="szSort" style="z-index: 104; left: 540px; width: 53px; position: absolute; top: 185px; height: 22px"
            type="hidden" size="3" name="szSort" runat="server">
        <asp:Button ID="btnReload" Style="z-index: 105; left: 920px; position: absolute; top: 64px"
            AccessKey="L" runat="server" CssClass="Button btn" Text="Load" DESIGNTIMEDRAGDROP="23"
            OnClick="btnReload_Click"></asp:Button>
    </form>
</body>
</html>