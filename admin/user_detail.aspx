<%@ Page Language="c#" Inherits="user_detail" CodeFile="user_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function viewUser(intID) {
            $("#frmMain").attr("target", "frmUpdate").attr("action", "user_update.aspx?intUserID=" + intID).submit().attr("target", "").attr("action", "");
        }

        function refreshPage() {
            document.location.href = "user_detail.aspx";
        }

        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        $(document).ready(function () {
            $(".AdminDetailPanel").corner();
            $('#gvList').DataTable({
                "bSort": true,
                "bFilter": true,
                "paging": false,
                "info": false,
                "stateSave": true,
                scrollY: '70vh',
                scrollCollapse: true,
                fixedHeader: true,
                initComplete: function () {
                    this.api().columns([1, 3]).every(function () {
                        var column = this;
                        var title = this.header().innerHTML.toLowerCase();
                        var select = $('<select class="TableFilter"><option value="">All ' + title + 's...</option></select>')
                        .appendTo($(column.header()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                            .search(val ? '^' + val + '$' : '', true, false)
                            .draw();
                        });

                        column.data().unique().sort().each(function (d, j) {
                            select.append('<option value="' + d + '">' + d + '</option>')
                        });
                    });
                    processDataTableHeaderFilters();
                }
            });

        });
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Staff admin
        </div>

        <div style='float: left; width: 40%; overflow-x: hidden' class='AdminDetailPanel'>
            
                <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"
                    OnRowDataBound="gvList_RowDataBound" BorderWidth="0"
                    EmptyDataText="No Data Found" EnableViewState="false">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Name" HeaderStyle-Width="30%" />
                        <asp:BoundField DataField="Role" HeaderText="Role" HeaderStyle-Width="30%" />
                        <asp:BoundField DataField="Team" HeaderText="Team" HeaderStyle-Width="20%" />
                        <asp:BoundField DataField="Office" HeaderText="Office" HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Supervisor" HeaderText="Supervisor" HeaderStyle-Width="10%"  />
                    </Columns>
                </asp:GridView>
            <div class='AdminActionPanel' style='float: left; width: 100%'>
                <span style="float: left">View inactive staff<asp:CheckBox ID="chkViewInactive" runat="server" AutoPostBack="true" /></span>
                <span style="float: right"><a class='LinkButton' href='javascript: viewUser(-1)'>Insert new user</a></span>
            </div>
        </div>

        <iframe id='frmUpdate' class='AdminUpdateFrame' frameborder='0' name='frmUpdate' style='height: 75vh' src='../blank.html'></iframe>
    </form>
</body>
</html>