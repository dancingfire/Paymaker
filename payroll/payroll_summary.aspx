<%@ Page Language="c#" Inherits="payroll_summary" CodeFile="payroll_summary.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Payroll summary</title>
    <script type="text/javascript">

        function submitPayroll(id, timesheetCycleID) {
            callWebMethod("../web_services/ws_Paymaker.asmx", "submitTimesheet", ["UserID", id, "TimesheetCycleID", timesheetCycleID, "SupervisorSignOff", true], actionCompletedSuccess);
            return false;
        }

        function actionCompletedSuccess(data) {
            $("#btnSubmit" + data).after("Finalised")
            $("#btnSubmit" + data).hide();
        }

        function refreshPage() {
            document.location.href = "payroll_summary.aspx";
        }

        var tableFixed = false;

        function fixTable() {
            if (!tableFixed)
                createDataTable("gvListPrepay", false, true, 520, false, true);
            tableFixed = true;
        }

        function checkComplete() {
            if ($('#hdCycleRef').val() != '0')
                return false;
            if (confirm('If you continue, these paycycles will be locked. Are you sure you want to do this?')) {
                buttonWait('btnComplete');
                return true;
            }
            return false;
        }

        $(document).ready(function () {
            createDataTable("gvList", false, true, 520, false, true);
            
            $("#tMain").tabs();
            $('#lstCycle').change(function () {
                $('#hdCycleRef').val($('#lstCycle').val());
                $('#btnChange').click();
            });

            $(".trEdit").click(function(e){
                editTimesheet($(this).data("id"), $('#lstCycle').val())
            })

            $('.ApproveButton').click(function (e) {
                e.stopPropagation();
                submitPayroll($(this).data("id"), $(this).data("tsid"));
                return false;
            });
        });
    </script>
    <style>
        .ui-tabs {
            position: inherit;
            padding-top: 170px;
        }
    </style>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdCycleRef" runat="server" Value=""></asp:HiddenField>

        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            <div style="float: left;">Payroll summary</div>
            <asp:DropDownList ID="lstCycle" runat="server" CssClass="form-control" Style="float: left; width: 180px; height: 23px; margin-left: 15px; padding: 2px 10px;"></asp:DropDownList>
            <a id="btnComplete" style="width: 150px; float: right; margin-right: 20px;" runat="server" onclick="return checkComplete();" class="Button btn" href='../reports/payroll_timesheets.aspx'>Create timesheet pdf</a>
            <asp:Button ID="btnUnlock"  Visible="false" runat="server" Text="Unlock cycle"  style="width: 150px; float: right; margin-right: 20px;" CssClass="Button btn"  OnClick="btnUnlock_Click"/>
            <a id="btnExport" style="width: 150px; float: right; margin-right: 20px;" runat="server" class="Button btn" href='../reports/payroll_summary_rpt.aspx'>Export</a>
            <a id="btnExportSummary" style="width: 150px; float: right; margin-right: 20px;" runat="server" class="Button btn" href='../reports/payroll_leave_summary_rpt.aspx'>Export leave summary</a>
        </div>

        <div class="PageContent">
            <div id="tMain">
                <ul id="ulMain">
                    <li id='Normal'><a href="#oTab1">Normal</a></li>
                    <li id='PayInAdvance' onclick="fixTable()"><a href="#oTab2">PayInAdvance</a></li>
                </ul>
                <div id='oTab1'>
                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" EmptyDataText="No Data Found" EnableViewState="false" OnRowDataBound="gvList_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-Width="15%" />
                            <asp:BoundField DataField="Office" HeaderText="Office" ItemStyle-Width="3%" />
                            <asp:BoundField DataField="Actual" HeaderText="Actual hours worked" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="AnnualLeave" HeaderText="Annual leave hours taken" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="SickLeave" HeaderText="Sick leave hours taken" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="RDOAcrued" HeaderText="RDO hours acrued" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="RDOTaken" HeaderText="RDO hours taken" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="Comments" HeaderText="Comments" ItemStyle-Width="25%" />
                            <asp:BoundField DataField="Details" HeaderText="" ItemStyle-Width="22%" />
                        </Columns>
                    </asp:GridView>
                </div>
                <div id='oTab2' style='display: none;'>
                    <asp:GridView ID="gvListPrepay" runat="server" AutoGenerateColumns="false" EmptyDataText="No Data Found" EnableViewState="false" OnRowDataBound="gvList_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-Width="15%" />
                            <asp:BoundField DataField="Office" HeaderText="Office" ItemStyle-Width="3%" />
                            <asp:BoundField DataField="Actual" HeaderText="Actual hours worked" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="AnnualLeave" HeaderText="Annual leave hours taken" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="SickLeave" HeaderText="Sick leave hours taken" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="RDOAcrued" HeaderText="RDO hours acrued" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="RDOTaken" HeaderText="RDO hours taken" ItemStyle-Width="7%" />
                            <asp:BoundField DataField="Comments" HeaderText="Comments" ItemStyle-Width="25%" />
                            <asp:BoundField DataField="Details" HeaderText="" ItemStyle-Width="22%" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
        <asp:Button ID="btnChange" Style="display: none;" runat="server" OnClick="btnChange_Click"></asp:Button>
    </form>
</body>
</html>