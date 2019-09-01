<%@ Page Language="c#" Inherits="Paymaker.quarterly_top_performer" CodeFile="quarterly_top_performer.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Quarterly top performance</title>
    <script type="text/javascript">
        function printReport() {
            window.print();
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />
        <div class='ActionPanel' style="width: 99%">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Quarterly top performer
            </div>
        </div>
        <div id="oDetails">
            <asp:GridView ID="gvTable" runat="server" AutoGenerateColumns="false"
                Width="99%" OnSorting="gvTable_Sorting">
                <Columns>

                    <asp:BoundField DataField="OFFICENAME" HeaderText="OfficeName" />
                    <asp:BoundField DataField="AGENT" HeaderText="Agent" />
                    <asp:BoundField DataField="COMMISSION" HeaderText="Highest Commission Earned" ItemStyle-CssClass='AlignRight' DataFormatString="{0:$0.00}" HeaderStyle-CssClass="AlignRight" />
                    <asp:BoundField DataField="SALECOUNT" HeaderText="Highest Number of Pty’s Sold" ItemStyle-CssClass='AlignRight' DataFormatString="{0:0.00}" HeaderStyle-CssClass="AlignRight"/>
                    <asp:BoundField DataField="SALETOTAL" HeaderText="Highest Value of Pty’s Sold" ItemStyle-CssClass='AlignRight' DataFormatString="{0:$0.00}" HeaderStyle-CssClass="AlignRight"/>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>