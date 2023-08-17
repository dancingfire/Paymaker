<%@ Page Language="c#" Inherits="run_sql" CodeFile="run_sql.aspx.cs" ValidateRequest="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>SQL</title>
    <script type="text/javascript">

    function loadPage() {
    }
    </script>
</head>
<body onload="loadPage()">
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdCompanyID" runat="server" Value="1" />
        <asp:HiddenField ID="hdCompanyTypeID" runat="server" Value="0" />
        
        <div class='container-fluid' >
            <div class="row">
                <div class="col-sm-10">
                    <asp:TextBox ID="txtSQL" runat="server" CssClass="Entry EntryPos" TextMode="MultiLine" Rows="15" Width="80%" Height="200"></asp:TextBox>
                   
                </div>
                <div class="col-sm-2">
                    <asp:Button ID="btnUpdate" runat="server" Text="Run SQL" CssClass="btn btn-block" OnClick="btnUpdate_Click" />
                    <br />    

                </div>
            </div>
            <div class="row">
                <div class="col-sm-12">
                    <asp:GridView ID="gvOutput" runat="server" EnableViewState="false"></asp:GridView>
                </div>

            </div>
        </div>
        
    </form>
</body>
</html>