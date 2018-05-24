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

            if ($("#gvPreview tr").length > 1) {
                var oTable = $('#gvPreview').dataTable({
                    "bPaginate": false,
                    "bLengthChange": false,
                    "bFilter": false,
                    "bSort": false,
                    "bInfo": false,
                    "bAutoWidth": false,
                    "sScrollY": '300',
                    "bScrollCollapse": true,
                    asSorting: ''
                });
            }
        });

        function hideInvoice(intInvoiceID, intCampaignID, obj) {
            callWebMethod("../web_services/ws_Paymaker.asmx", "markExportedToMYOB", ["InvoiceID", intInvoiceID, "CampaignID", intCampaignID], actionCompletedSuccess);
            $(obj).parents("tr").hide();
        }

        function actionCompletedSuccess() {
            ;
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdItemID" runat="server" Value=""></asp:HiddenField>
        <asp:HiddenField ID="hdListTypeID" runat="server" Value=""></asp:HiddenField>
        <div class="ListHeader" style="font-size: 10pt; width: 600px; padding-top: 2px; top: 0px; height: 16px; margin-bottom: 10px">
            Export invoices to MYOB
        </div>

        <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel"></asp:Panel>
        <div style="width: 500px; float: left">
            <div id="oLinks" runat="server" visible="false">
                <p>Here are the files that you need to import. Please ensure that you import the Card files first. Its also a good idea to save these files in case you need to perform the import again.</p>
            </div>
        </div>
        <div class='LeftPanel' style="width: 160px;">
            <asp:Button ID="btnUpdate" Style="width: 85px;" runat="server" Text="Export" CommandName="cancel"
                CssClass="Button btn" TabIndex="100" OnClick="btnUpdate_Click"></asp:Button>
            <asp:Button ID="btnCancel" Style="width: 85px;" runat="server" Text="Cancel" CommandName="cancel"
                CssClass="Button btn" CausesValidation="False" TabIndex="300" OnClick="btnCancel_Click"></asp:Button>
        </div>

        <div style="clear: both; width: 500px">
            <a href='../admin/myob_files.aspx' target='_blank'>View previously exported files</a>
            <asp:GridView ID="gvPreview" runat="server" Visible="false" AutoGenerateColumns="false" EnableViewState="false">
                <Columns>
                    <asp:BoundField DataField="CardID" HeaderText="Card ID" />
                    <asp:BoundField DataField="InvoiceNumber" HeaderText="Invoice Number" />
                    <asp:BoundField DataField="AccountNum" HeaderText="Accont" />
                    <asp:BoundField DataField="Job" HeaderText="Job code" />
                    <asp:BoundField DataField="IncTaxAmount" HeaderText="Inc GST" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                    <asp:BoundField DataField="NonGSTAmount" HeaderText="Ex GST" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:F2}" />
                    <asp:TemplateField HeaderText="Hide">
                        <ItemTemplate>
                            <a onclick='hideInvoice(<%#Eval("INVOICEID") %>, <%#Eval("CAMPAIGNID") %>, this)' href='#'>hide</a>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    All campaigns and invoices have been exported.
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </form>
</body>
</html>