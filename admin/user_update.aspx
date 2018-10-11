<%@ Page Language="c#" Inherits="user_update" EnableViewState="True" AutoEventWireup="true"
    CodeFile="user_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>User update</title>
    <script type="text/javascript">
        function confirmDelete() {
            return confirm("Are you sure you want to delete this user?");
        }

        function showPermissions() {
            szParams = 'intSelUserID=' + $("#HDUserID").val();

            $('#mPermission').on('show.bs.modal', function () {
                $('#fPermission').attr("src", "../admin/permissions_update.aspx?" + szParams);
            });
            $('#mPermission').on('hidden.bs.modal', function () {
                $('#fPermission').attr("src", "../blank.html");
                refreshPage()
            });
            $('#mPermission').modal({ 'backdrop': 'static' });
            $('#mPermission').modal('show');
            return false;
        }

        function closePermissions(blnRefresh) {
            $('#mPermission').modal('hide');
        }

        function showSalary() {
            szPage = "salary_update.aspx?intUserID=" + $("#HDUserID").val();
            $('#mSalary').on('show.bs.modal', function () {
                $('#fSalary').attr("src", szPage);
            });
            $('#mSalary').on('hidden.bs.modal', function () {
                $('#fSalary').attr("src", "../blank.html");
            });
            $('#mSalary').modal({ 'backdrop': 'static' });
            $('#mSalary').modal('show');
            return false;
        }

        function closeSalary(blnRefresh, Salary, Team) {
            $('#mSalary').modal('hide');
            if (blnRefresh) {
                $("#lblSalary").html(Salary);
                $("#lblTeam").html(Team);
            }
        }

        function changeRole() {
            intRoleID = $("#lstRole").val();
            if (parseInt(intRoleID) == 5) {
                $(".HideIfInternalServices").hide();
            } else {
                $(".HideIfInternalServices").show();
            }
        }

        function loadPage() {
            if ($("#HDUserID").val() > -1) {
                $("#lstOffice").change(function () {
                    $("#pEffectiveDate").show();
                });
            }
            $("#lstRole, #lstTimesheetType").change(function () {
                changeRole();
            });

            createCalendar("txtEffectiveDate");
            changeRole();
        }
    </script>
    <style>
        .LabelPos {
            width: 200px;
        }
    </style>
</head>
<body onload='loadPage()' style="margin-top: 15px">
    <form id="frmMain" method="post" runat="server">

        <asp:HiddenField ID="HDUserID" runat="server" Value=""></asp:HiddenField>
        <asp:HiddenField ID="HDOrigOfficeID" runat="server" Value=""></asp:HiddenField>
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" />

        <div style="width: 460px; float: left;">
            <asp:Label ID="Label4" runat="server" CssClass="Label LabelPos">Role</asp:Label>
            <asp:DropDownList ID="lstRole" runat="server" CssClass="Entry EntryPos">
            </asp:DropDownList>
            <br class="Align" />

            <asp:Label ID="lblName" runat="server" CssClass="Label LabelPos">First</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtFirstName" runat="server" Text=""></asp:TextBox>
            <br class="Align" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="First name is a required field" ControlToValidate="txtFirstName" Display="None"></asp:RequiredFieldValidator>

            <asp:Label ID="Label6" runat="server" CssClass="Label LabelPos">Last</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtLastName" runat="server" Text=""></asp:TextBox>
            <br class="Align" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="Last name is a required field" ControlToValidate="txtLastName" Display="None"></asp:RequiredFieldValidator>

            <asp:Label ID="lblInitials" runat="server" CssClass="Label LabelPos">Initials</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtInitials" runat="server" Text=""></asp:TextBox>
            <br class="Align" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="Initials is a required field" ControlToValidate="txtInitials" Display="None"></asp:RequiredFieldValidator>

            <asp:Label ID="Label1" runat="server" CssClass="Label LabelPos">Login</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtLogin" runat="server" Text=""></asp:TextBox>
            <br class="Align" />

            <asp:Label ID="Label2" runat="server" CssClass="Label LabelPos">Password</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtPassword" runat="server" Text="" TextMode="Password" MaxLength="256"></asp:TextBox>
            <br class="Align" />

            <asp:Label ID="Label3" runat="server" CssClass="Label LabelPos">Email</asp:Label>
            <asp:TextBox CssClass="Entry EntryPos" ID="txtEmail" runat="server" Text=""></asp:TextBox>
            <br class="Align" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="Email is a required field" ControlToValidate="txtEmail" Display="None"></asp:RequiredFieldValidator>

            <asp:Label ID="Label21" runat="server" CssClass="Label LabelPos" Visible="false">Show on timesheet</asp:Label>
            <asp:DropDownList ID="lstTimesheetType" runat="server" CssClass="Entry EntryPos" Visible="false">
                <asp:ListItem Text="Do not show" Value="0"></asp:ListItem>
                <asp:ListItem Text="Internal services" Value="1"></asp:ListItem>
                <asp:ListItem Text="Sales assistant" Value="2"></asp:ListItem>
            </asp:DropDownList>

            <br class="Align" />

             <asp:Label ID="Label10" runat="server" CssClass="Label LabelPos">Status</asp:Label>
                <asp:DropDownList ID="lstStatus" runat="server" CssClass="Entry EntryPos">
                    <asp:ListItem Text="Active" Value="1"></asp:ListItem>
                    <asp:ListItem style="color: red" Text="In-active" Value="0"></asp:ListItem>
                </asp:DropDownList>
                <br class="Align" />
            <div id='dTimeSheetFields'>
                <asp:Label ID="Label20" runat="server" CssClass="Label LabelPos">Supervisor </asp:Label>
                <asp:DropDownList ID="lstSupervisor" runat="server" CssClass="Entry EntryPos"></asp:DropDownList>
                <br class="Align" />
            </div>

            <asp:Label ID="Label9" runat="server" CssClass="Label LabelPos">Office</asp:Label>
            <asp:DropDownList ID="lstOffice" runat="server" CssClass="Entry EntryPos"></asp:DropDownList>
            <br class="Align" />

            <asp:Label ID="Label22" runat="server" CssClass="Label LabelPos">Payroll cycle</asp:Label>
            <asp:DropDownList ID="lstPayrollCycle" runat="server" CssClass="Entry EntryPos">
                <asp:ListItem Text="Do not show" Value="0"></asp:ListItem>
                <asp:ListItem Text="Normal" Value="1"></asp:ListItem>
                <asp:ListItem Text="Paid in advance" Value="2"></asp:ListItem>
            </asp:DropDownList>
            <br class="Align" />

            <div class="HideIfInternalServices">
                <asp:Panel ID="pTeamMembers" runat="server" Visible="false">
                    <asp:Label ID="Label19" runat="server" CssClass="Label LabelPos">Team members</asp:Label>
                    <asp:Label ID="lblTeamMembers" runat="server" CssClass="Entry EntryPos"></asp:Label>
                    <br class="Align" />
                </asp:Panel>
                <div class="panel panel-default" style="margin-top: 10px; width: 98%">
                    <div class="panel-heading">Employment details</div>
                    <div class="panel-body">
                        <asp:Button ID="btnChangeSalary" Style="width: 95px; float: right" runat="server" Text="Change" CssClass="Button btn" OnClientClick="return showSalary();"></asp:Button>
                        <asp:Label ID="Label16" runat="server" CssClass="Label LabelPos">Team </asp:Label>
                        <asp:Label ID="lblTeam" runat="server" CssClass="Entry EntryPos" Width="100"></asp:Label>
                        <br class="Align" />

                        <asp:Label ID="Label17" runat="server" CssClass="Label LabelPos">Salary </asp:Label>
                        <asp:Label ID="lblSalary" runat="server" CssClass="Entry EntryPos" Width="100"></asp:Label>
                        <br class="Align" />
                    </div>
                </div>

                <asp:Panel runat="server" ID='pEffectiveDate' Style='display: none'>
                    <asp:Label ID="Label18" runat="server" CssClass="Label LabelPos">Effective date</asp:Label>
                    <asp:TextBox CssClass="Entry EntryPos" ID="txtEffectiveDate" runat="server" Text=""></asp:TextBox>
                    <br class="Align" />
                </asp:Panel>
               

                <asp:Label ID="Label11" runat="server" CssClass="Label LabelPos">Is Paid</asp:Label>
                <asp:CheckBox ID="chkIsPaid" runat="server" />
                <br class="Align" />

                <asp:Label ID="Label8" runat="server" CssClass="Label LabelPos">Sales target</asp:Label>
                <asp:TextBox CssClass="Entry EntryPos" ID="txtSalesTarget" runat="server" Text=""
                    MaxLength="80"></asp:TextBox>
                <br class="Align" />

                <asp:Label ID="Label5" runat="server" CssClass="Label LabelPos">Credit GL Code</asp:Label>
                <asp:TextBox CssClass="Entry EntryPos" ID="txtCreditGLCode" runat="server" Text=""
                    MaxLength="80"></asp:TextBox>
                <br class="Align" />

                <asp:Label ID="Label7" runat="server" CssClass="Label LabelPos">Debit GL Code</asp:Label>
                <asp:TextBox CssClass="Entry EntryPos" ID="txtDebitGLCode" runat="server" Text=""></asp:TextBox>
                <br class="Align" />

                <div class="panel panel-default" style="margin-top: 10px; width: 98%">
                    <div class="panel-heading">Report settings</div>
                    <div class="panel-body">
                        You can select whether you want agents to show on the reports, and whether you want their totals to acrue to another agent.
                        <br />

                        <asp:Label ID="Label12" runat="server" CssClass="Label LabelPos">Agent EOMc</asp:Label>
                        <asp:DropDownList ID="lstAgentEOMReportSettings" runat="server" CssClass="Entry EntryPos">
                        </asp:DropDownList>
                        <br class="Align" />

                        <asp:Label ID="Label13" runat="server" CssClass="Label LabelPos">Quarterly top performer</asp:Label>
                        <asp:DropDownList ID="lstQuarterlyTopPerformer" runat="server" CssClass="Entry EntryPos">
                        </asp:DropDownList>
                        <br class="Align" />

                        <asp:Label ID="Label14" runat="server" CssClass="Label LabelPos">Top performer</asp:Label>
                        <asp:DropDownList ID="lstTopPerformer" runat="server" CssClass="Entry EntryPos">
                        </asp:DropDownList>
                        <br class="Align" />

                         <asp:Label ID="Label23" runat="server" CssClass="Label LabelPos">Admin PA for</asp:Label>
                        <asp:DropDownList ID="lstAdminPA" runat="server" CssClass="Entry EntryPos">
                        </asp:DropDownList>
                        <br class="Align" />
                        <asp:Label ID="Label15" runat="server" CssClass="Label LabelPos">Hide on incentive report</asp:Label>
                        <asp:CheckBox ID="chkShowIncentiveSummary" runat="server" />
                        <br class="Align" />
                    </div>
                </div>
            </div>
        </div>
        <div class='LeftPanel' style='width: 100px'>

            <asp:Button ID="btnUpdate" Style="width: 95px;" runat="server" Text="Update" CommandName="cancel" CssClass="Button btn" TabIndex="100" OnClick="btnUpdate_Click"></asp:Button>
            <asp:Button ID="btnCancel" Style="z-index: 102;" runat="server" Text="Cancel" CssClass="Button btn MarginTop" CausesValidation="False" TabIndex="300" OnClick="btnCancel_Click" Width="95px"></asp:Button>
            <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="Button btn MarginTop" CausesValidation="False" TabIndex="200" OnClientClick="return confirmDelete();" OnClick="btnDelete_Click" Width="95px"></asp:Button>
            <asp:Button ID="btnPermissions" runat="server" Width="95px" Text="Permissions" CssClass="Button btn MarginTop" CausesValidation="False" TabIndex="250" OnClientClick=" return showPermissions()"></asp:Button>
        </div>
    </form>
</body>
</html>