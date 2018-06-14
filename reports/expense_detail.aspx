<%@ Page Language="c#" Inherits="Paymaker.expense_detail" CodeFile="expense_detail.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD html 4.0 Transitional//EN" >
<html>
<head runat="server">
    <title>Expense detail report</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }

        function loadPage() {
            $("#gvTable tr:last").css({ fontWeight: 'bolder' });
        }
    </script>
</head>
<body onload='loadPage()'>
    <form id="frmMain" method="post" runat="server">
    <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
    <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
    <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
    <asp:HiddenField ID="hdFY" Value="" runat="server" />
    <asp:HiddenField ID="hdExpenseID" Value="" runat="server" />
    <div class='ActionPanel' style="width: 99%">
        <div id="pPageHeader" class='PageHeader' runat="server">
            Expense Detail Report
        </div>
        </div>
    <div id="oDetails">
        <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false" Width="99%">
            <Columns>
                <asp:BoundField DataField="AGENT" HeaderText="Agent" />
                <asp:BoundField DataField="1" HeaderText="Jul" ItemStyle-CssClass='AlignRight'/>
                <asp:BoundField DataField="2" HeaderText="Aug" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="3" HeaderText="Sep" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="4" HeaderText="Oct" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="5" HeaderText="Nov" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="6" HeaderText="Dec" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="7" HeaderText="Jan" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="8" HeaderText="Feb" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="9" HeaderText="Mar" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="10" HeaderText="Apr" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="11" HeaderText="May" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="12" HeaderText="Jun" ItemStyle-CssClass='AlignRight' />
                <asp:BoundField DataField="Total" HeaderText="Total" ItemStyle-CssClass='AlignRight' />
            </Columns>
        </asp:GridView>

    </div>
    </form>
</body>
</html>
