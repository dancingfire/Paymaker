<%@ Page Language="c#" Inherits="Paymaker.expense_summary" CodeFile="expense_summary.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Expense summary report</title>
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
        <div id="pPageHeader" class='PageHeader' runat="server">
            Expense Summary Report
        </div>
        <div id="oDetails" style="float: left;">
            <div id="dFletchersHeader" class='PageHeader' runat="server" visible="false" style="float: left; clear: both; margin-top: 20px">
                Fletchers contribution
            </div>
            <asp:GridView ID="gvFletchers" runat="server" AutoGenerateColumns="false" Width="99%" Style="float: left; clear: both" EmptyDataText="There are no fletchers contributed data.">
                <Columns>
                    <asp:BoundField DataField="AGENT" HeaderText="Agent" />
                    <asp:BoundField DataField="7" HeaderText="Jul" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="8" HeaderText="Aug" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="9" HeaderText="Sep" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="10" HeaderText="Oct" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="11" HeaderText="Nov" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="12" HeaderText="Dec" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="1" HeaderText="Jan" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="2" HeaderText="Feb" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="3" HeaderText="Mar" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="4" HeaderText="Apr" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="5" HeaderText="May" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="6" HeaderText="Jun" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="Budget" HeaderText="Budget" ItemStyle-CssClass='AlignRight' />
                    <asp:BoundField DataField="Total" HeaderText="Total" ItemStyle-CssClass='AlignRight' />
                </Columns>
            </asp:GridView>
            <asp:Panel ID="dAgentContribution" runat="server" Visible="false">
                <div id="dAgentHeader" class='PageHeader' style="float: left; clear: both; margin-top: 20px">
                    Agent contribution
                </div>
                <asp:GridView ID="gvAgent" runat="server" AutoGenerateColumns="false" Width="99%" Style="float: left; clear: both" EmptyDataText="There are no agent contributed data.">
                    <Columns>
                        <asp:BoundField DataField="AGENT" HeaderText="Agent" />
                        <asp:BoundField DataField="7" HeaderText="Jul" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="8" HeaderText="Aug" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="9" HeaderText="Sep" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="10" HeaderText="Oct" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="11" HeaderText="Nov" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="12" HeaderText="Dec" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="1" HeaderText="Jan" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="2" HeaderText="Feb" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="3" HeaderText="Mar" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="4" HeaderText="Apr" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="5" HeaderText="May" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="6" HeaderText="Jun" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="Budget" HeaderText="Budget" ItemStyle-CssClass='AlignRight' />
                        <asp:BoundField DataField="Total" HeaderText="Total" ItemStyle-CssClass='AlignRight' />
                    </Columns>
                </asp:GridView>
            </asp:Panel>
        </div>
    </form>
</body>
</html>