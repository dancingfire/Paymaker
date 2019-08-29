<%@ Page Language="c#" Inherits="Paymaker.leave_manager_dashboard" CodeFile="leave_manager_dashboard.aspx.cs" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Leave requests</title>

    <script type="text/javascript">

         $(document).ready(function () {
            createDataTable("gvList", false, true, 520, false, true);

        });

        function editRequest(intID) {
            $("#fModalUpdate").attr('src', 'request_update.aspx?id=' + intID);
            $("#mModalUpdate").modal("show");
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" class="form-inline">
         <asp:HiddenField ID="hdCycleRef" runat="server" Value=""></asp:HiddenField>

        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-12">
                    
                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" EmptyDataText="No Data Found" EnableViewState="false" OnRowDataBound="gvList_RowDataBound">

                        <Columns>
                            <asp:BoundField DataField="LeaveStatus" HeaderText="Status" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="StaffMember" HeaderText="Staff" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="LeaveType" HeaderText="Leave type" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start date" ItemStyle-Width="15%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="EndDate" HeaderText="End date" ItemStyle-Width="15%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="Duration" HeaderText="Duration" ItemStyle-Width="10%" HtmlEncode="false" ItemStyle-HorizontalAlign="Center"/>
                            <asp:BoundField DataField="EntryDate" HeaderText="Requested" ItemStyle-Width="10%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="Comments" HeaderText="Notes" ItemStyle-Width="50%" HtmlEncode="false" />
                        </Columns>
                        <EmptyDataTemplate>
                            There are no outstanding requests.
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>