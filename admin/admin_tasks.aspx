<%@ Page Language="c#" Inherits="PAYMAKER.admin_tasks" CodeFile="admin_tasks.aspx.cs"  Async="true"%>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Admin</title>
    <script type="text/javascript">
    </script>
    <style type="text/css">
        .ItemHelp {
            float: left;
            width: 350px;
        }

        .DBAction {
            float: left;
            width: 500px;
            margin-bottom: 10px;
            clear: both;
            padding: 5px;
            border: solid #D8D9CC 2px;
            border-radius: 3px;
            margin-left: 5px;
        }
    </style>
</head>
<body>
    <form id="frmMain" runat="server">
        <div class="PageHeader" style="z-index: 105; float: left; clear: both; width: 100%; top: 24px; margin-bottom: 20px">
            Admin tasks
        </div>

        <asp:Panel ID="pPageNotification" runat="server" Width="60%" Visible="false" CssClass="PageNotificationPanel"></asp:Panel>

        <asp:Panel ID="Panel2" runat="server" CssClass="DBAction">
            <asp:Label ID="Label2" runat="server" Width="250px" Text="Update graph totals from Jan 2020 onwards" CssClass="LabelPos"></asp:Label>
            <asp:Button ID="btnUpdateGraphTotals" CssClass="Button" runat="server" Width="150px" Text="Update totals" OnClientClick="showWait('Updating totals...');" OnClick="btnUpdateGraphTotals_Click" />
        </asp:Panel>
       
        <asp:Panel ID="Panel6" runat="server" CssClass="DBAction">
            <asp:Label ID="Label6" runat="server" Width="250px" Text="Test the timesheet email" CssClass="LabelPos"></asp:Label>
            <asp:Button ID="Button3" CssClass="Button" runat="server" Width="150px" Text="Test email"  OnClick="btnTestTimesheet_Click" />
        </asp:Panel>
         <asp:Panel ID="Panel7" runat="server" CssClass="DBAction">
            <asp:Label ID="Label7" runat="server" Width="250px" Text="Sends any emails in the queue" CssClass="LabelPos"></asp:Label>
            <asp:Button ID="Button2" CssClass="Button" runat="server" Width="150px" Text="Send queued email"  OnClick="btnSendQueuedEmails_Click" />
        </asp:Panel>
        <asp:Panel ID="Panel5" runat="server" CssClass="DBAction">
            <asp:Label ID="Label5" runat="server" Width="250px" Text="Import B&D Sales Listing" CssClass="LabelPos"></asp:Label>
            <asp:TextBox ID="txtBnDID" runat="server" />
            <asp:Button ID="btnImportSales" CssClass="Button" runat="server" Width="150px" Text="Import sales listing"  OnClick="btnImportSales_Click"  />
        </asp:Panel>

        <asp:Panel ID="Panel1" runat="server" CssClass="DBAction">
            <asp:Label ID="Label1" runat="server" Width="250px" Text="Recalculate single agent pays" CssClass="LabelPos"></asp:Label>
            <asp:DropDownList ID="lstAgentFinYear" runat="server">
                <asp:ListItem Value="2014" Text="2014/15"></asp:ListItem>
                <asp:ListItem Value="2015" Text="2015/16"></asp:ListItem>
                <asp:ListItem Value="2016" Text="2016/17"></asp:ListItem>
                <asp:ListItem Value="2017" Text="2017/18"></asp:ListItem>
                <asp:ListItem Value="2018" Text="2018/19"></asp:ListItem>
                <asp:ListItem Value="2019" Text="2019/20"></asp:ListItem>
                <asp:ListItem Value="2020" Text="2020/21"></asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="lstAgent" runat="server"></asp:DropDownList>
            <asp:Button ID="Button1" CssClass="Button" runat="server" Width="150px" Text="Update totals" OnClientClick="showWait('Updating totals...');" OnClick="btnUpdateAgentActual_Click" />
        </asp:Panel>

        <asp:Panel ID="Panel4" runat="server" CssClass="DBAction">
            <asp:Label ID="Label4" runat="server" Width="250px" Text="Recalculate EOY bonus totals" CssClass="LabelPos"></asp:Label>
            <asp:DropDownList ID="lstFinYear" runat="server">
                <asp:ListItem Value="2014" Text="2014/15"></asp:ListItem>
                <asp:ListItem Value="2015" Text="2015/16"></asp:ListItem>
                <asp:ListItem Value="2016" Text="2016/17"></asp:ListItem>
                <asp:ListItem Value="2017" Text="2017/18"></asp:ListItem>
                <asp:ListItem Value="2018" Text="2018/19"></asp:ListItem>
                <asp:ListItem Value="2019" Text="2019/20"></asp:ListItem>
                <asp:ListItem Value="2020" Text="2020/21"></asp:ListItem>
            </asp:DropDownList>

            <asp:Button ID="btnUpdateSBS" CssClass="Button" runat="server" Width="150px" Text="Recalc EOFY" OnClientClick="showWait('Updating totals...');" OnClick="btnUpdateAnnualEOYB_Click" />
        </asp:Panel>

        <asp:Panel ID="Panel3" runat="server" CssClass="DBAction">
            <asp:Label ID="Label3" runat="server" Width="250px" Text="Reset application" CssClass="LabelPos"></asp:Label>
            <asp:Button ID="btnReload" CssClass="Button" runat="server" Width="150px" Text="Reset application" OnClientClick="showWait('Resetting application...');" OnClick="btnReload_Click" />
        </asp:Panel>
    </form>
</body>
</html>