<%@ Page Language="c#" Inherits="payroll_update" CodeFile="payroll_update.aspx.cs" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">

        function refreshPage() {
            document.location.href = "user_detail.aspx";
        }

        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        function checkSignOff() {
            return confirm('If you continue, you will no longer be able to edit your timesheet.  Are you sure you want to do this?');
        }

        function calculateTotals() {
            var sum = 0;
            sum += calculateField('Actual');
            sum += calculateField('AnnualLeave');
            sum += calculateField('SickLeave');
            sum += calculateField('RDOAcrued');
            sum += calculateField('RDOTaken');
            $('#txtComments_total').html('<b>Grand total: </b>' + sum);
        }

        function calculateField(field) {
            var sum = 0;
            $('[id^=txt' + field + ']').not('#txt' + field + '_total').each(function () {
                sum += Number($(this).val());
            });
            $('#txt' + field + '_total').val(sum);
            return sum;
        }

        $(document).ready(function () {
            $('.timesheet-field').change(function () {
                val = $(this).val();
                
                if(val != '' && !$.isNumeric(val))
                    $(this).val("");
                calculateTotals();
            });
            calculateTotals();
            $('#lstCycle').change(function () {
                $('#hdCycleRef').val($('#lstCycle').val());
                $('#frmMain').submit();
            });

            $("#frmMain").submit(function () {
                buttonWait('btnUpdate');
            })
        });
    </script>
    <style>
        .input-sm {
            padding: 2px 5px;
            height: 22px;
        }

        .table.dataTable tbody th, table.dataTable tbody td {
            padding: 2px;
        }

        .dataTables_scrollBody {
            overflow: hidden !important;
        }
    </style>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdCycleRef" runat="server" Value=""></asp:HiddenField>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <div id="dPageHeader" runat="server" class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            <div style="float: left;">Enter payroll details</div>
            <asp:DropDownList ID="lstCycle" runat="server" CssClass="form-control" Style="float: left; width: 180px; height: 23px; margin-left: 15px; padding: 2px 10px;"></asp:DropDownList>
        </div>

        <div id="dContainer" runat="server" class="container">
            <div class="row">
                <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" EmptyDataText="No Data Found" EnableViewState="false" CssClass="table-striped">
                    <Columns>
                        <asp:BoundField DataField="RenderedEntryDate" HeaderText="Date" ItemStyle-Width="20%" HtmlEncode="false" />
                        <asp:BoundField DataField="ActualEntryField" HeaderText="Actual hours worked" ItemStyle-Width="10%" HtmlEncode="false" />
                        <asp:BoundField DataField="AnnualLeaveEntryField" HeaderText="Annual leave hours taken" ItemStyle-Width="10%" HtmlEncode="false" />
                        <asp:BoundField DataField="SickLeaveEntryField" HeaderText="Sick leave hours taken" ItemStyle-Width="10%" HtmlEncode="false" />
                        <asp:BoundField DataField="RDOAcruedEntryField" HeaderText="RDO hours acrued" ItemStyle-Width="10%" HtmlEncode="false" />
                        <asp:BoundField DataField="RDOTakenEntryField" HeaderText="RDO hours taken" ItemStyle-Width="10%" HtmlEncode="false" />
                        <asp:BoundField DataField="CommentsEntryField" HeaderText="Comments" ItemStyle-Width="30%" HtmlEncode="false" />
                    </Columns>
                </asp:GridView>
            </div>
            <div class="row" style="margin-top: 20px">
                <asp:GridView ID="gvModifications" runat="server" AutoGenerateColumns="false" EmptyDataText="No overrides have been made to orignal submission" EnableViewState="false" CssClass="table-striped" Style="width: 98%">
                    <Columns>
                        <asp:BoundField DataField="ChangeDate" HeaderText="Date changed" ItemStyle-Width="10%" HtmlEncode="false" DataFormatString="{0:MMM dd, yyyy}" />
                        <asp:BoundField DataField="VALUE" HeaderText="Change" ItemStyle-Width="70%" HtmlEncode="false" />
                        <asp:BoundField DataField="LOGNAME" HeaderText="Changed by" ItemStyle-Width="20%" HtmlEncode="false" />
                    </Columns>
                </asp:GridView>
            </div>
            <div style="text-align: center">
                <asp:Panel ID="pMessage" runat="server" Visible="false" CssClass="Error">
                    <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
                </asp:Panel>
                <asp:Button ID="btnUpdate" Style="width: 150px; margin-right: 20px;" runat="server" CssClass="Button btn"
                    Text="Save changes" UseSubmitBehavior="true" OnClick="btnUpdate_Click" />
                <asp:Button ID="btnSignOff" Style="width: 150px;" runat="server" CssClass="Button btn"
                    Text="Lock timesheet" UseSubmitBehavior="true" OnClientClick="return checkSignOff();" OnClick="btnSignOff_Click" ToolTip="Select this to lock your timesheet." />
            </div>
            <div id="dViewer">
                <rsweb:ReportViewer ID="rViewer" runat="server" Width="100%" Height="100%" AsyncRendering="true"></rsweb:ReportViewer>
            </div>
        </div>

        <div id="dOldRecords" runat="server" class="Label">
            <p>&nbsp;</p>
            <p>This record has been closed.</p>
            <p>
                If you need to update timesheet information for this record please download manual timesheet, fill in and send to
                <a href="mailto:payroll@fletchers.net.au">payroll@fletchers.net.au</a>
            </p>
            <br />
            <a id="btnDownload" style="width: 200px; float: left; margin-right: 20px;" runat="server" class="Button btn" href=''>Download manual timesheet</a>
        </div>
        <asp:Panel ID="pAdmin" runat="server">
            <div style="text-align: center; margin-top: 10px;">
                <asp:Button ID="btnChange" runat="server" OnClick="btnChange_Click" Text="Override timesheet"></asp:Button>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnApprove" runat="server" OnClick="btnApprove_Click" Text="Approve"></asp:Button>
            </div>
        </asp:Panel>
    </form>
</body>
</html>