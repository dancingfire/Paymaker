<%@ Page Language="c#" Inherits="Paymaker.leave_request" CodeFile="leave_request.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Leave requests</title>
    <script type="text/javascript">
        $(document).ready(function () {
            createCalendar("txtStartDate", true);
            createCalendar("txtEndDate", true);
            $("#txtStartDate, #txtEndDate").attr("readonly", "readonly")
        });
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <div class="container-fluid" id="dAdminControls" runat="server">
            <div class="row">
                <div class="col-sm-3">
                    <label>Date range</label>

                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-field" Width="100"></asp:TextBox>
                    &nbsp;to &nbsp;<asp:TextBox ID="txtEndDate" runat="server" CssClass="formfied" Width="100"></asp:TextBox>
                </div>
                <div class="col-sm-2">
                    <asp:Button ID="btnViewReport" runat="server" Text="View excel report" CssClass="Button btn btn-block" OnClick="btnViewReport_Click" /><br />
                    <asp:Button ID="btnExportExcel" runat="server" Text="Export to excel" CssClass="Button btn btn-block" style="margin-top: 10px" OnClick="btnExportExcel_Click" />
                </div>
            </div>
        </div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:Panel ID="pNoData" runat="server" Style="float: left; margin-left: 20px" Visible="false">There was no data on the report</asp:Panel>
        <div id="dViewer" style="margin-top: 30px">
            <rsweb:ReportViewer ID="rViewer" runat="server" Width="100%" AsyncRendering="true" SizeToReportContent="true" ShowExportControls="false"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>