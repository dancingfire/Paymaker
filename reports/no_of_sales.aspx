<%@ Page Language="c#" Inherits="Paymaker.no_of_sales" CodeFile="no_of_sales.aspx.cs" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>No of sales</title>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdFY" Value="" runat="server" />
        <div class='ActionPanel' style="width: 99%">
            <div id="pPageHeader" class='PageHeader' runat="server">
                Number of Sales Per Office
            </div>
        </div>
        <div id="oInnerChartDiv">
            <asp:Chart ID="chtNoOfSales" runat="server" Width="1050">
                <Titles>
                    <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" Text=""
                        ForeColor="26, 59, 105">
                    </asp:Title>
                </Titles>
            </asp:Chart>
        </div>
    </form>
</body>
</html>