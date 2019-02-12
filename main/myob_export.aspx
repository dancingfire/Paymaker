<%@ Page Language="c#" Inherits="myob_export" EnableViewState="True" AutoEventWireup="true"
    CodeFile="myob_export.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>MYOB export</title>
    <script type="text/javascript">
        $(document).ready(function () {
            createCalendar("txtStartDate");
            createCalendar("txtEndDate");

            $(".ActionPanel").corner();

        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdItemID" runat="server" Value=""></asp:HiddenField>
        <asp:HiddenField ID="hdListTypeID" runat="server" Value=""></asp:HiddenField>
        <div class="PageHeader" style="margin-bottom: 10px">
            Export to MYOB
        </div>
        <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel"></asp:Panel>
        <div style="width: 500px; float: left">
           
            <asp:Label ID="Label1" CssClass="Label LabelPos" runat="server" Text="Pay period">
            </asp:Label>
            <asp:DropDownList ID="lstPayPeriod" CssClass="Entry EntryPos" runat="server">
            </asp:DropDownList>
                <br class='Align' />
             <asp:Label ID="Label4" CssClass="Label LabelPos" runat="server" Text="Select Company">
                    </asp:Label>
                    <asp:DropDownList ID="lstCompany" CssClass="Entry EntryPos" runat="server">
                        <asp:ListItem Text="All" Value=""></asp:ListItem>
                    </asp:DropDownList>
                    <br class='Align' />
            <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos">Journal #</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtJournalNumber" runat="server" Text="" MaxLength="40"></asp:TextBox><asp:RequiredFieldValidator
                ID="RequiredFieldValidator1" runat="server" ValidationGroup="EXPORT" ControlToValidate="txtJournalNumber" ErrorMessage="You must enter the journal number" EnableClientScript="true" Display="Dynamic"></asp:RequiredFieldValidator>
            <br />
            <br />
            <div style="width: 500px; float: left">
                <div id="oLinks" runat="server" visible="false">
                    <p>Here are the files that you need to import. Its a good idea to save these files in case you need to perform the import again.</p>
                </div>
            </div>
        </div>
        <div class='LeftPanel' style="width: 160px;">
            <asp:Button ID="btnExport" Style="width: 85px;" runat="server" Text="Export" 
                CssClass="Button btn" TabIndex="100" OnClick="btnExport_Click" ValidationGroup="EXPORT"></asp:Button> <br /><br />
            <asp:Button ID="btnPreview" Style="width: 85px;" runat="server" Text="Preview" 
                CssClass="Button btn" TabIndex="100" OnClick="btnPreview_Click"></asp:Button>
         </div>

        <div style="clear: both;">
            <asp:GridView ID="gvPreview" runat="server" Visible="false" EnableViewState="false">
            </asp:GridView>
        </div>
    </form>
</body>
</html>