<%@ Page Language="c#" Inherits="delegation" CodeFile="delegation.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
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
                dom: 'Bfrtip',
                scrollCollapse: true,
                fixedHeader: true,
                buttons: ['excelHtml5']
            });

        });
    </script>
    <style>
        .dt-buttons {
            display: inline
        }
    </style>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
           Delegation history
        </div>

        <div style='float: left; width: 70%; overflow-x: hidden' class='AdminDetailPanel'>
            
                <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"
                    OnRowDataBound="gvList_RowDataBound" BorderWidth="0"
                    EmptyDataText="No delegations have been entered in the last year." EnableViewState="false">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Name" HeaderStyle-Width="30%" />
                        <asp:BoundField DataField="DelegatedTo" HeaderText="Delagated to" HeaderStyle-Width="30%"  />
                        <asp:BoundField DataField="StartDate" HeaderText="Starting" HeaderStyle-Width="20%" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:MMM dd, yyyy}"/>
                        <asp:BoundField DataField="EndDate" HeaderText="Ending" HeaderStyle-Width="20%" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:MMM dd, yyyy}"/>
                    </Columns>
                </asp:GridView>
          
        </div>

        
    </form>
</body>
</html>