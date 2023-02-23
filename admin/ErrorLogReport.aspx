<%@ Page Language="c#" Inherits="ErrorLogReport" CodeFile="ErrorLogReport.aspx.cs"
    AutoEventWireup="true" AspCompat="true" %>

<!DOCTYPE HTML>
<html>
<head runat="server" id="Head">
    <title>Report</title>
    <link href="../main.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="Form1" method="post" runat="server">
        <div style="width: 100%; height: 44px;" class="RWTableHeader">
            ASP.NET Error Report
            <div class="RightPanel">
                <asp:Button ID="btnMarkAsRead" runat="server" Text="Purge all" OnClick="btnMarkAsRead_Click" />
                <asp:Button ID="blnDeleteAll" runat="server" Text="Delete all" OnClick="blnDeleteAll_Click" />&nbsp;
                <asp:CheckBox ID="chkShowAll" runat="server" CssClass="RWInstruction" OnCheckedChanged="chkShowAll_CheckedChanged"
                    Text="Show all   " AutoPostBack="True" />&nbsp;<br />
                &nbsp;
            </div>
        </div>
        <asp:GridView ID="oList" Style="z-index: 101; left: 11px; table-layout: fixed; word-wrap: break-word"
            runat="server" Width="100%" Height="282px" AutoGenerateColumns="False" CssClass="RWInstruction"
            EnableViewState="False">
            <Columns>
                <asp:TemplateField HeaderText="Details" HeaderStyle-Width="200px">
                    <ItemTemplate>
                        <b>Date: </b>
                        <%# String.Format("{0:MMM dd, yyyy}", Eval("LogDateTime"))%>
                        <br />
                        <b>Client: </b>
                        <%# Eval("Client")%>
                        <br />
                        <b>Message: </b>
                        <%# Eval("Message")%>
                        <br />
                        <b>Referrer: </b>
                        <%# Eval("Referer")%>
                        <br />
                        <b>USer ID: </b>
                        <%# Eval("UserID")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Form" HeaderStyle-Width="300px">
                    <ItemTemplate>
                        <%# doFormatForm(Eval("Form").ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="QueryString" HeaderText="QueryString">
                    <HeaderStyle Width="200px" />
                </asp:BoundField>
                <asp:BoundField DataField="StackTrace" HeaderText="StackTrace">
                    <HeaderStyle Width="200px" />
                </asp:BoundField>
            </Columns>
            <HeaderStyle CssClass="RWTableHeader" />
            <RowStyle VerticalAlign="Top" />
            <AlternatingRowStyle CssClass="RWOddRow" VerticalAlign="Top" />
        </asp:GridView>
        <br />
    </form>
</body>
</html>