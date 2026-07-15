<%@ Page Language="c#" Inherits="list_detail" CodeFile="list_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function viewItem(intID) {
            $("#frmMain").attr("action", "list_update.aspx?intItemID=" + intID + "&intListTypeID=" + intListTypeID).submit();
        }

        $(document).ready(function () {
            $('.JQInactive').each(function () {
                if ($(this).text() > 0) {
                    $(this).parent().css('color', 'red');
                    var tempText = $(this).next().html();
                    $(this).next().html(tempText + ' (in-active)');
                }
            });
            createDataTable("gvList", true, true, $(window).height() - 120);
        });

        function refreshPage() {
            document.location.href = document.location.href;
        }

        function closePage() {
            document.location.href = "../welcome.aspx";
        }
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server" target="frmUpdate" style="display: flex; flex-direction: column; height: 100vh;">
        <div class="PageHeader" style="z-index: 107; width: 100%; top: 1px; flex-shrink: 0;">
            <asp:Label ID="lblItemName" runat="server" Text="Label"></asp:Label>
        </div>

        <div style="display: flex; flex: 1; overflow: hidden;">
        <div style='float: left; width: 30%; display: flex; flex-direction: column;'>
            <div class="ListContainer" style="overflow: auto; overflow-x: hidden; width: 100%; flex: 1; float: left">
                <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"
                    OnRowDataBound="gvList_RowDataBound" Width="100%"
                    EmptyDataText="No Data Found" EnableViewState="false">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Name" HeaderStyle-Width="80%" ItemStyle-Width="80%" />
                        <asp:BoundField DataField="Status" HeaderText="Status" HeaderStyle-Width="20%" ItemStyle-HorizontalAlign="Center" />
                    </Columns>
                </asp:GridView>
            </div>
            <div class='AdminActionPanel' style='float: left; text-align: right; width: 100%'><a href='javascript: viewItem(-1)'>
                <asp:Label ID="lblInsertText" runat="server" Text="Label" CssClass="LinkButton"></asp:Label></a></div>
        </div>

        <iframe id='frmUpdate' name='frmUpdate' class='AdminUpdateFrame' frameborder='0' src='../blank.html' style='flex: 1; height: 100%;'></iframe>
        </div>
    </form>
</body>
</html>