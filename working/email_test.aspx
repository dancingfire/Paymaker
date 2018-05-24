<%@ Page Language="c#" Inherits="email_test" CodeFile="email_test.aspx.cs" ValidateRequest="false" EnableViewStateMac="false" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
</head>
<body>

    <div class="container-fluid">
        <form method="post" id="frmMain" runat="server" class="form-horizontal">
            <div class="col-sm-6">
                <div class="form-group">
                    <label for="txtTo" class="control-label">To</label>
                    <asp:TextBox ID="txtTo" runat="server" Text="payroll@fletchers.net.au" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label for="txtMsg" class="control-label">Message</label>
                    <asp:TextBox ID="txtMsg" runat="server" CssClass="form-control" TextMode="MultiLine">Sending a test email to the above address. The email will be sent from do-not-reply@fletchers.net.au</asp:TextBox>
                </div>
                <div class="row">
                    <asp:Button ID="btnSend" runat="server" Text="Send email" OnClick="btnSend_Click" CssClass="btn btn-default" />
                </div>
            </div>
        </form>
    </div>
</body>
</htm>