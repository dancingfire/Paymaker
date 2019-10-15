<%@ Page Language="c#" Inherits="admin_user_delegation" EnableViewState="True" AutoEventWireup="true" CodeFile="admin_user_delegation.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>User delegation</title>
    <script type="text/javascript">
        function confirmDelete(intID) {
            $("#hdDBID").val(intID);
            if (confirm("Are you sure you want to delete this item?")) {
                $("#btnDelete").click();
            }
        }

        function updateDelegation(intID, TypeID, AwayUserID, DelegatedUserID, StartDate, EndDate, ReceiveEmail) {
            $("#hdDBID").val(intID);
            if (intID == -1) {
                $("#lstType").val("0");
                $("#lstAwayUser").val("-1");
                $("#lstDelegatedToUser").val("-1");
                $("#txtStartDate").val("");
                $("#txtEndDate").val("");
                $("#lstReceiveEmails").val("0");
            } else {
                $("#lstType").val(TypeID);
                $("#lstAwayUser").val(AwayUserID);
                $("#lstDelegatedToUser").val(DelegatedUserID);

                $("#txtStartDate").val(StartDate);
                $("#txtEndDate").val(EndDate);
                $("#lstReceiveEmails").val(ReceiveEmail);
            }
            $('#mUpdate').modal({ 'backdrop': 'static' });
            $('#mUpdate').modal('show');
        }

        function closeDelegation() {
            $('#mUpdate').modal('hide');
        }

        function loadPage() {
            parent.$("#mModalTitleDelegation").html("Delegation preferences");

            addFormValidation("frmMain");
            $("#lstAwayUser, #lstDelegatedToUser").select2();
            createCalendar("txtStartDate", true);
            createCalendar("txtEndDate", true);
            createDataTable("gvList", true, false, 234, false);

        }
    </script>
</head>
<body onload="loadPage()" style="overflow-x: hidden">
    <div class="container-fluid">
        <form id="frmMain" method="post" runat="server">
            <div class="row">
                <div class="col-xs-10">
                    <asp:Label ID="lblCurrDelegations" runat="server" ></asp:Label>
                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"
                        Width="80%" OnRowDataBound="gvList_RowDataBound"
                        EmptyDataText="There are no current delegations" EnableViewState="false">
                        <Columns>
                            <asp:BoundField DataField="AwayName" HeaderText="Away User" HeaderStyle-Width="25%" />
                            <asp:BoundField DataField="DelegatedName" HeaderText="Delegated User" HeaderStyle-Width="25%" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start date" HeaderStyle-Width="25%" DataFormatString="{0: MMM dd, yyyy}" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="EndDate" HeaderText="End date" HeaderStyle-Width="25%" DataFormatString="{0: MMM dd, yyyy}" ItemStyle-HorizontalAlign="Center" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <%# getDelImage(Convert.ToInt32(Eval("ID"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="col-xs-2" style="height: 280px">

                    <asp:Button ID="btnAdd" Style="width: 85px;" runat="server" Text="Add"
                        CssClass="btn btn-primary btn-block" CausesValidation="False" TabIndex="100" OnClientClick="updateDelegation(-1); return false;" />

                    <asp:Button ID="btnClose" Style="width: 85px;" runat="server" Text="Close"
                        CssClass="btn  btn-block" CausesValidation="False" TabIndex="300" OnClientClick="parent.closeDelegation(); return false;" />
                    <asp:Button ID="btnDelete" runat="server" Text="Delete"
                        CssClass="Invisible" CausesValidation="False" OnClick="btnDelete_Click" />
                </div>
            </div>
            <div id='mUpdate' class='modal fade'>
                <div class='modal-dialog' style='width: 450px; height: 300px'>
                    <div class='modal-content'>
                        <div class='modal-header'>
                            <h4 class='modal-title'>Update delegation</h4>
                        </div>
                        <div class='modal-body'>
                            <asp:HiddenField ID="hdDBID" runat="server" />

                            <bw:bwDropDownList runat="server" ID="lstAwayUser" Label="Person on leave" required  />
                             <bw:bwDropDownList runat="server" ID="lstDelegatedToUser" Label="Delegate to" required  />
                           
                            <bw:bwTextBox runat="server" ID="txtStartDate" Label="Start date:" required />

                            <bw:bwTextBox runat="server" ID="txtEndDate" Label="End date:" required />

                            <bw:bwDropDownList runat="server" ID="lstReceiveEmails" Label="Continue to receive emails?" required>
                                <asp:ListItem Text="No" Value="0"  Selected="True"></asp:ListItem>
                                <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                            </bw:bwDropDownList>

                            <div style='text-align: center; '>
                                
                                <asp:Button ID="btnUpdate" Style="width: 120px; margin-top:20px" runat="server" Text="Update"
                                    CssClass="btn btn-sm" CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click" />

                                <input type='button' onclick='return closeDelegation();' class='btn btn-sm' value='Cancel' style='width: 120px; margin-top:20px' />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </form>
    </div>
</body>
</html>