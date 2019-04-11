<%@ Page Language="c#" Inherits="Paymaker.leave_dashboard" CodeFile="leave_dashboard.aspx.cs" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Leave requests</title>

    <script type="text/javascript">

        function loadPage() {
            createDataTable("gvList", false, true, 520, false, true);

        }
        function editRequest(intID) {
            $("#fModalUpdate").attr('src', 'request_update.aspx?id=' + intID);
            $("#mModalUpdate").modal("show");
        }

        function showManagerDashboard() {
            document.location.href = 'leave_manager_dashboard.aspx';
            return false;
        }
    </script>
</head>
<body onload='loadPage()'>
    <form id="frmMain" method="post" runat="server">

        <div class="container-fluid">

            <div class="row">
                <div class="col-sm-10">
                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"   GridLines="None" EnableViewState="false"  OnRowDataBound="gvList_RowDataBound">

                        <Columns>
                            <asp:BoundField DataField="LeaveStatus" HeaderText="Status" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="LeaveType" HeaderText="Leave type" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start date" ItemStyle-Width="15%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="EndDate" HeaderText="End date" ItemStyle-Width="15%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="TotalDays" HeaderText="Total days" ItemStyle-Width="10%" HtmlEncode="false" ItemStyle-HorizontalAlign="Center"/>
                            <asp:BoundField DataField="EntryDate" HeaderText="Requested" ItemStyle-Width="10%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="Comments" HeaderText="Notes" ItemStyle-Width="50%" HtmlEncode="false" />
                        </Columns>
                        <EmptyDataTemplate>
                            There are no leave requests. Select <i>Add request</i> to create a new leave request
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <div class="col-md-2">
                    <button type="button" class="btn btn-primary btn-block Button" onclick="editRequest(-1);">Apply for leave</button>

                    <asp:Button ID="btnSuperviser" runat="server" Text="Manage staff requests" CssClass="btn btn-secondary btn-block Button" OnClientClick="return showManagerDashboard()" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>