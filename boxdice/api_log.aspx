<%@ Page Language="c#" Inherits="api_log" CodeFile="api_log.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Email history</title>

    <script>
        $(document).ready(function () {
            $('#gvLog').dataTable({
                "bPaginate": false,
                "bLengthChange": false,
                "bFilter": true,
                "bSort": false,
                "bInfo": false,
                "bAutoWidth": false
            });
            addZebra('gvCurrent');
        });
    </script>
</head>
<body>
  
    <form id="frmMain" name="frmMain" method="post" runat="server" >
           
        <div class="row">
            <div class="col-xs-12">
                <asp:GridView ID="gvLog" runat="server" AutoGenerateColumns="false" Width="99%" CssClass="SelectTable" EnableViewState="false">
                    <Columns>
                        <asp:BoundField DataField="TIMESTAMP" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy hh:mm}" HeaderStyle-Width="20%" />
                        <asp:BoundField DataField="LOG" HeaderText="Message" HeaderStyle-Width="80%" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
 </body>
</html>