<%@ Page Language="c#" Inherits="Paymaker.user_top_performer" CodeFile="user_top_performer.aspx.cs" EnableViewState="false" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Top performer</title>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdFY" Value="" runat="server" />
        <div id="oInnerChartDiv">
            <asp:Chart ID="chtTopPerformers" runat="server" Width="1600">
                <Titles>
                    <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" Text=""
                        ForeColor="26, 59, 105">
                    </asp:Title>
                    <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" Text=""
                        ForeColor="26, 59, 105">
                    </asp:Title>
                </Titles>
            </asp:Chart>
        </div>
    </form>
</body>
</html>