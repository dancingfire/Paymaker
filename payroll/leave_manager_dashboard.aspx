<%@ Page Language="c#" Inherits="Paymaker.leave_manager_dashboard" CodeFile="leave_manager_dashboard.aspx.cs" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Leave requests</title>

    <script type="text/javascript">

        $(document).ready(function () {
            $.fn.dataTable.moment('ll');

            createDataTable("gvList", true, true, 520, false, true);

            $(".archive").click(function (e) {
                e.stopPropagation();
                id = $(this).attr('data-id');
                $("#hdArchiveID").val();
                archiveRequest('A', id);
                $(this).closest("tr").hide();
                $("#hdArchiveID").val('');
            })

            $(".unarchive").click(function (e) {
                e.stopPropagation();
                id = $(this).attr('data-id');
                archiveRequest('U', id);
                $(this).closest("tr").hide();
                $("#hdUnArchiveID").val('');
            })
        });

        function editRequest(intID) {
            $("#fModalUpdate").attr('src', 'request_update.aspx?id=' + intID);
            $("#mModalUpdate").modal("show");
        }

        function archiveRequest(mode, id) {
           
            $.ajax({
                type: "POST",
                url: "leave_manager_dashboard.aspx/archiveRequest?mode=" + mode + "&id=" + id,
                data: '',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                failure: function (response) {
                    alert(response.d);
                }
            });
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" class="form-inline">
        <asp:HiddenField ID="hdArchiveID" runat="server" Value="" />
        <asp:HiddenField ID="hdUnArchiveID" runat="server" Value="" />

        <div class="container-fluid">
            <div class="row" style="position: relative; top: 30px; z-index: 400; height: 35px">
                <div class="col-sm-2">
                    <label>Include archived requests </label>
                    &nbsp;<asp:CheckBox ID="chkViewArchived" runat="server" />
                </div>
                <div class="col-sm-2">
                    <asp:TextBox ID="txtSearch" runat="server" PlaceHolder="Search name/initials"></asp:TextBox>
                </div>
                <div class="col-sm-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Filter" OnClick="btnSearch_Click" CssClass="btn btn-primary" />
                </div>
            </div>
            <div class="row">
                <div class="col-sm-12">

                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" EmptyDataText="No Data Found" EnableViewState="false" OnRowDataBound="gvList_RowDataBound">

                        <Columns>
                            <asp:BoundField DataField="LeaveStatus" HeaderText="Status" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="StaffMember" HeaderText="Staff" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="LeaveType" HeaderText="Leave type" ItemStyle-Width="10%" HtmlEncode="false" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start date" ItemStyle-Width="15%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" HeaderStyle-CssClass="dt-date" />
                            <asp:BoundField DataField="EndDate" HeaderText="End date" ItemStyle-Width="15%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" HeaderStyle-CssClass="dt-date" />
                            <asp:BoundField DataField="Duration" HeaderText="Duration" ItemStyle-Width="10%" HtmlEncode="false" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="EntryDate" HeaderText="Requested" ItemStyle-Width="10%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" HeaderStyle-CssClass="dt-date" />
                            <asp:BoundField DataField="Comments" HeaderText="Notes" ItemStyle-Width="50%" HtmlEncode="false" />

                            <asp:TemplateField HeaderText="Archive">
                                <ItemTemplate>
                                    <%#getArchiveButton(Convert.ToInt32(Eval("ID")),Convert.ToBoolean(Eval("ISARCHIVED")),  Eval("LeaveStatus").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div style='margin-top: 35px'>There are no outstanding requests.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>