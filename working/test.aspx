<%@ Page Language="c#" Inherits="test" CodeFile="test.aspx.cs" ValidateRequest="false" EnableViewStateMac="false" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
</head>
<body>

    <div class="container-fluid">
        <form method="post" id="frmMain" runat="server" class="form-horizontal">
            
                    <asp:Button ID="btnSend" runat="server" Text="Send email" OnClick="btnSend_Click" CssClass="btn btn-default" />
            
        </form>
    </div>
</body>
</html>