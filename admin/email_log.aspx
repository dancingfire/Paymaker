<%@ Page Language="c#" Inherits="email_log" CodeFile="email_log.aspx.cs" %>

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
<body class='AdminPage'>
    <div class="container-fluid">
        <form id="frmMain" name="frmMain" method="post" runat="server" target="frmUpdate" class="form-hotizontal">
            <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
                Email log
            </div>
            <div class="row">
                <div class="col-sm-6">
                    <div class="form-group">
                        <label for="txtToFilter" class="control-label col-xs-4">Filter by email body:</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtSearch" runat="server" Text=""></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="txtToFilter" class="control-label col-xs-4">Filter to:</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtToFilter" runat="server" Text=""></asp:TextBox>
                        </div>
                    </div>
                </div>
                <div class="col-sm-6">
                    <asp:Button ID="btnSearch"
                        runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn btn-primary"
                        Style="height: 26px" />
                </div>
            </div>

            <asp:GridView ID="gvLog" runat="server" AutoGenerateColumns="false" Width="99%" CssClass="SelectTable" EnableViewState="false">
                <Columns>
                    <asp:BoundField DataField="SentDate" HeaderText="Sent date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="10%" />
                    <asp:BoundField DataField="Subject" HeaderText="Subject" HeaderStyle-Width="10%" />
                    <asp:BoundField DataField="SentTo" HeaderText="To" HeaderStyle-Width="20%" />
                    <asp:BoundField DataField="Body" HeaderText="Body" HtmlEncode="false" />
                </Columns>
            </asp:GridView>
        </form>
    </div>
</body>
</html>
