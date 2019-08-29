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
        
        <div class='RightActionPanel' style="width: 14%; height: 540px; padding-top: 20px; text-align: center; float: right">
            <asp:Button ID="btnUpdate" runat="server" Text="Run SQL" CssClass="Button" OnClick="btnUpdate_Click" />
            <br />
        </div>
        <asp:Label ID="dOut" runat="server" />
        <br />
        <asp:TextBox ID="txtSQL" runat="server" CssClass="Entry EntryPos" TextMode="MultiLine" Rows="15" Width="80%"></asp:TextBox>
        <asp:TextBox ID="txtResult" runat="server" CssClass="Entry EntryPos" TextMode="MultiLine" Rows="15" Width="80%"></asp:TextBox>
        <asp:GridView ID="gvOutput" runat="server" EnableViewState="false"></asp:GridView>
    </form>
</body>
</html>