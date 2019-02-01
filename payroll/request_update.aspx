<%@ Page Language="c#" Inherits="request_update" EnableViewState="True" AutoEventWireup="true" CodeFile="request_update.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Leave request update</title>
    <script type="text/javascript">
        function confirmDelete() {
            return confirm("Are you sure you want to delete this request?");
        }
        function confirmApproval() {
            return confirm("Are you sure you want to approve this request?");
        }
        function confirmRejection() {
            return confirm("Are you sure you want to reject this request? Please inform the staff member verbally why this has been rejected. ");
        }

        function cancelPage() {
            parent.closeEditModal(false);
            return false;
        }

        $(document).ready(function () {
            createCalendar("txtStartDate", true);
            createCalendar("txtEndDate", true);
            $("#txtStartDate, #txtEndDate").attr("readonly", "readonly")

        });

        function checkForReadOnly() {
            if ($("#hdReadOnly").val() == "true") {
                $("[id*=btnDelete_SE]").each(function () {
                    $(this).hide();
                });

                $(':input:select').attr("disabled", true);
                $('.JQHideOnReadOnly').hide();
                $("#btnCancel").removeAttr("disabled");
            }
        }
        function validatePage() {
            if ($("#txtStartDate").val() == "" || $("#txtEndDate").val() == "") {
                alert('Please enter a valid start and end date');
                return false;
            }
            return true;
        }
        function loadPage() {
            checkForReadOnly();
        }
    </script>
</head>
<body onload='loadPage()'>
    <form id="frmMain" method="post" runat="server" onsubmit="return validatePage();" class="form-horizontal" enctype="application/x-www-form-urlencoded">
        <asp:HiddenField ID="hdTXID" runat="server" />
        <asp:HiddenField ID="hdDateUsed" runat="server" />
        <asp:HiddenField ID="hdReadOnly" runat="server" />
        <asp:HiddenField ID="hdFletcherAmountCalc" runat="server" Value="" />
        <div class="container-fluid">
            <div class="row">
                <div class="col-xs-12">
                    <div class="form-group">
                        <label for="lstLeaveType" class="control-label col-xs-2">Leave type:</label>
                        <div class="col-xs-10">
                            <asp:DropDownList ID="lstLeaveType" runat="server" CssClass="Entry  form-control" required></asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="txtStartDate" class="control-label col-xs-2">Start date:</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtStartDate" runat="server" Text="" required></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="txtEndDate" class="control-label col-xs-2">End date:</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtEndDate" runat="server" Text="" required></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="lblTotalDays" class="control-label col-xs-2">Total days:</label>
                        <div class="col-xs-10">
                            <asp:Label ID="lblTotalDays" runat="server" CssClass="Entry  form-control"></asp:Label>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="lblComments" class="control-label col-xs-2">Reason</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtComments" runat="server" TextMode="MultiLine"></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="lblComments" class="control-label col-xs-2">Supporting evidence</label>
                        <div class="col-xs-10">
                            <asp:FileUpload ID="FileUpload1" runat="server" CssClass="Entry "  BorderStyle="0"/>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class='col-xs-3'>
                    <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="Button btn btn-primary btn-block"
                        CausesValidation="False" TabIndex="300" OnClientClick="return confirmApproval();" Visible="false"
                        Style="margin-bottom: 15px" OnClick="btnApprove_Click"></asp:Button>

                    <asp:Button ID="btnUpdate" runat="server" Text="Send approval request" CssClass="Button btn btn-block"
                        CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click" OnClientClick="return validatePage()"
                        Style="margin-bottom: 5px"></asp:Button>
                </div>
                <div class='col-xs-3 col-xs-offset-1'>
                    <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="Button btn btn-secondary btn-block" Visible="false"
                        CausesValidation="False" TabIndex="300" OnClientClick="return confirmRejection();"
                        Style="margin-bottom: 15px" OnClick="btnReject_Click"></asp:Button>

                    <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="Button btn btn-block"
                        CausesValidation="False" TabIndex="200" OnClick="btnDelete_Click" OnClientClick="return confirmDelete()"></asp:Button>
                </div>
                <div class='col-xs-3 col-xs-offset-1'>
                    <button class="Button btn btn-block" id="btnCancel" runat="server" onclick="return cancelPage();" tabindex="120">Cancel</button>
                </div>
            </div>
        </div>
    </form>
</body>
</html>