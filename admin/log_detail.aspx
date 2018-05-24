<%@ Page Language="c#" Inherits="log_detail" CodeFile="log_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Log history</title>

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
    <form id="frmMain" name="frmMain" method="post" runat="server" target="frmUpdate">
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Change log
        </div>
        <asp:Label ID="Label1" runat="server" Text="Search text:" CssClass="Label LabelPos"></asp:Label>
        <asp:TextBox ID="txtSearch" runat="server" CssClass="Edit Editpos"></asp:TextBox>
        <asp:Button ID="btnSearch"
            runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="Button btn"
            Style="height: 26px" />
        <br class='Align' />
        <asp:Label ID="Label2" runat="server" Text="Log type:" CssClass="Label LabelPos"></asp:Label>
        <asp:DropDownList
            ID="lstType" runat="server">
            <asp:ListItem Text="Campaign" Value="2, 6, 7, 8"></asp:ListItem>
            <asp:ListItem Text="Sale" Value="1, 3, 4, 5"></asp:ListItem>
        </asp:DropDownList>
        <br class='Align' />

        <asp:GridView ID="gvLog" runat="server" AutoGenerateColumns="false" Width="99%" CssClass="SelectTable" EnableViewState="false">
            <Columns>
                <asp:BoundField DataField="ChangeDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-Width="10%" />
                <asp:BoundField DataField="INFO" HeaderText="Property reference" HtmlEncode="false" HeaderStyle-Width="20%" />
                <asp:BoundField DataField="VALUE" HeaderText="Change" HtmlEncode="false" HeaderStyle-Width="70%" />
                <asp:BoundField DataField="Person" HeaderText="Person" />
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>