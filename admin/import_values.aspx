<%@ Page Language="c#" Inherits="import_values" CodeFile="import_values.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <link href="../Paymaker.css" type="text/css" rel="stylesheet">
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <div class='container-fluid'>
            <div class="row">
                <asp:Panel ID="pPageNotification" runat="server" Width="100%" Visible="false" CssClass="PageNotificationPanel col-sm-12"></asp:Panel>
                <div class=" col-sm-12">
                   <p> Upload the XLS file that contains the values for this year's EOFY sales letters.</p>
                    <p>Download the template file <a href='SalesFiguresTemplate.xlsx'>here</a></p>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-12">
                    <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos">Payment file</asp:Label>
                    <asp:FileUpload ID="fuImport" runat="server" CssClass="Entry EntryPos" />
                    <div class='LeftPanel' style="width: 95px; text-align: right">
                        <asp:Button ID="btnUpdate" Style="width: 85px; margin-left: 50px" runat="server" Text="Upload" CssClass="Button" CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click"></asp:Button>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-12" style="margin-top: 20px">
                    The current values are displayed in the table below, which will be updated when you upload the new values.
                </div>
            </div>
            <div class="row">
                <div style="margin-top: 40px" class="col-sm-12">
                    <asp:GridView ID="gvData" runat="server" Width="600"></asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>