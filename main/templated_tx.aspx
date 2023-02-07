<%@ Page Language="c#" Inherits="templated_tx" CodeFile="templated_tx.aspx.cs" EnableViewState="true" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">
        intHeight = $(window).height() - 100;
        function removeTemplate(id) {
            callWebMethod("../web_services/ws_Paymaker.asmx", "removeTemplatedTX", ["ID", id], null);
            $("#tag" + id).closest('tr').fadeOut("slow", function () { });
        }

        $(document).ready(function () {
            createDataTable("gvTXs", true, false, false, intHeight);
        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self">
        <div style="width: 99%">
            <asp:GridView ID="gvTXs" runat="server" AutoGenerateColumns="false"  Width="100%" EnableViewState="false" EmptyDataText="There are no transactions set to be recurring">
                <Columns>
                    <asp:BoundField DataField="User" HeaderText="Staff member" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="FletcherAmount" HeaderText="Fletchers amount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="Account" HeaderText="Account" />
                    <asp:BoundField DataField="CATEGORY" HeaderText="Category" />
                    <asp:BoundField DataField="Action" HeaderText="Action"  HtmlEncode="false"/>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>