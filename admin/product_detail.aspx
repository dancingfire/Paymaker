<%@ Page Language="c#" Inherits="product_detail" CodeFile="product_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function viewItem(intID) {
            $("#frmMain").attr("action", "product_update.aspx?intItemID=" + intID).submit();
        }

        function refreshPage() {
            document.location.href = document.location.href;
        }

        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        $(document).ready(function () {
            if ($("#gvCurrent tr").length > 1) {
                var oTable = $('#gvCurrent').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": true,
                    "bSort": true,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollY": 450,
                    "bScrollCollapse": true
                });
            }
        });
    </script>
    <style type="text/css">
        #gvCurrent tbody tr:hover {
            background-color: #DDC691 !important;
            cursor: pointer;
        }
    </style>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server" target="frmUpdate">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            <asp:Label ID="lblItemName" runat="server" Text="Label"></asp:Label>
        </div>

        <div style='float: left; width: 30%;'>
            <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="false"
                OnRowDataBound="gvCurrent_RowDataBound" Width="100%" EmptyDataText="No Data Found" EnableViewState="false">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Description" HeaderStyle-Width="70%" />
                    <asp:BoundField DataField="CreditGLCode" HeaderText="GL code" HeaderStyle-Width="15%" />
                    <asp:BoundField DataField="ExcludeFromInvoice" HeaderText="Exclude from invoicing" HeaderStyle-Width="15%" ItemStyle-HorizontalAlign="Center" />
                </Columns>
            </asp:GridView>
        </div>
        <iframe id='frmUpdate' name='frmUpdate' class='AdminUpdateFrame' frameborder='0' src='../blank.html'></iframe>
    </form>
</body>
</html>