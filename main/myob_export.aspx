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
            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos">Start date</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtStartDate" runat="server" Text="" MaxLength="40"></asp:TextBox>
            <br class="Align" />

            <asp:Label ID="lblName" runat="server" CssClass="Label LabelPos">End date</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtEndDate" runat="server" Text="" MaxLength="40"></asp:TextBox>
            <br class="Align" />
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
            <asp:Button ID="btnUpdate" Style="width: 85px;" runat="server" Text="Preview" CommandName="cancel"
                CssClass="Button btn" TabIndex="100" OnClick="btnUpdate_Click" ValidationGroup="EXPORT"></asp:Button>
            <asp:Button ID="btnCancel" Style="width: 85px;" runat="server" Text="Cancel" CommandName="cancel"
                CssClass="Button btn TopSpace" CausesValidation="False" TabIndex="300" OnClick="btnCancel_Click"></asp:Button>
        </div>

        <div runat="server" id="dValidate" class='ActionPanel Normal' style='width: 600px; font-weight: normal; padding: 5px; clear: both;'>
            <strong>Validate GL codes with MYOB</strong>
            <br />
            <br />
            Export the accounts information (Select <i>tab delimited</i> in the options and <i>Account code</i> in the list of fields) from MYOB.

            Select the export file below and we will match it against the codes contained in the current export.
            <br class="Align" />
            <br class="Align" />

            <div style="float: right; clear: right; width: 95px;">
                <asp:Button ID="btnMYOBValidate" Style="width: 85px;" runat="server"
                    Text="Validate" CommandName="MYOB"
                    CssClass="Button btn" TabIndex="100" OnClick="btnMYOBValidate_Click"></asp:Button>
            </div>
            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos" Style='width: 150px'>MYOB accounts file</asp:Label>
            <asp:FileUpload ID="fuMYOBAccount" runat="server" />
        </div>
        <div style="clear: both;">
            <asp:GridView ID="gvPreview" runat="server" Visible="false" EnableViewState="false">
            </asp:GridView>
        </div>
    </form>
</body>
</html>