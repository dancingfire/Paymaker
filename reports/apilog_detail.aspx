<%@ Page Language="c#" Inherits="apilog_detail" CodeFile="apilog_detail.aspx.cs" EnableViewState="true" %>

<!DOCTYPE html>
<html>
<head runat="server">
<title>B&D API access</title>
<script type="text/javascript">
   
    
    function loadPage() {
        
            
    }

   
</script>

</head>
<body onload='loadPage()'>
    <form id="frmMain" method="post" runat="server" >
       <div class="container-fluid" >

             <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" Width="400" EmptyDataText="No Data Found" EnableViewState="false"  >
                <Columns>
                    <asp:BoundField DataField="Timestamp" HeaderText="Started" HeaderStyle-Width="50%"   DataFormatString="{0:MMM dd, yyyy hh:mm}" />
                    <asp:BoundField DataField="Log" HeaderText="Message" HeaderStyle-Width="50%"   ItemStyle-HorizontalAlign="Center" />
                </Columns>
                </asp:GridView>
        </div>

    </form>
</body>
</html>
