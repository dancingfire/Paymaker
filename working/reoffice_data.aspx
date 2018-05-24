<%@ Page Language="c#" Inherits="reoffice_data" CodeFile="reoffice_data.aspx.cs"
    EnableViewState="false" EnableViewStateMac="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Re office data</title>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self">
        <div class='ActionPanel'>
            <span class='RightActionPanel'>
                <asp:Button ID="btnShow" runat="server" Text="Show reoffice data" OnClick="btnShow_Click" /></span>
        </div>
        <br />
        <div class='GridFilter' id='dDrigFilter'>
            <asp:GridView ID="gvCurrent" runat="server" AutoGenerateColumns="true" CssClass="SelectTable"
                Width="100%">
            </asp:GridView>
        </div>
    </form>
</body>
</html>