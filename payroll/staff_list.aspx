<%@ Page Language="c#" Inherits="Paymaker.staff_list" CodeFile="staff_list.aspx.cs" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Staff list</title>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#gvList').DataTable({
                "bSort": true,
                "bFilter": true,
                "paging": false,
                "info": false,
                "stateSave": true,
                scrollY: '70vh',
                dom: 'Bfrtip',
                scrollCollapse: true,
                fixedHeader: true,

                buttons: ['excelHtml5']
            });
        });
    </script>
</head>
<body >
    <form id="frmMain" method="post" runat="server">

        <div class="container-fluid">

            <div class="row">
                <div class="col-sm-10">
                    <asp:GridView ID="gvList" runat="server" GridLines="None" EnableViewState="false"  >

                    </asp:GridView>
                </div>
               
            </div>
        </div>
    </form>
</body>
</html>