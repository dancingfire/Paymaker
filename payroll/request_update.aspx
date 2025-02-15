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
            return confirm("Please confirm your approval of this request?");
        }

        function confirmRejection() {
            if ($("#txtManagerComments").val() == "") {
                alert("Please enter the reason why you are rejecting this request.");
                return false;
            }
            return confirm("Are you sure you want to reject this request?  ");
        }

        function confirmDiscussion() {
            if ($("#txtManagerComments").val() == "") {
                alert("Please enter the reason why you would like further discussion.");
                return false;
            }
            return true;
        }

        function cancelPage() {
            parent.closeEditModal(false);
            return false;
        }

        szFormat = "M d, yy";
        szMomentFormat = "MMM DD, YYYY"
        $(document).ready(function () {

            $("#txtStartDate").datepicker({
                dateFormat: szFormat,
                onSelect: function (dateText, inst) {
                    $("#txtEndDate").datepicker("option", "minDate", $("#txtStartDate").datepicker("getDate"));
                    checkDiff();
                }
            });

            $("#lstPartial").change(function () {
                checkPartial();
            })

            $("#txtEndDate").datepicker({
                dateFormat: szFormat,
                minDate: $("#txtStartDate").val(),
                onSelect: function (dateText, inst) {
                    checkDiff();
                }
            });
            
            moment.updateLocale('au', {
                holidays: arHolidayDates,
                holidayFormat: szMomentFormat,
                workingWeekdays: [1, 2, 3, 4, 5]
            });

            $("#txtStartDate, #txtEndDate, #txtTotalDays").attr("readonly", "readonly");
            checkDiff();
            checkForReadOnly();
            checkPartial();
        });

        function checkPartial() {
            blnPartial = $("#lstPartial").val() == "PARTIAL";
            if (blnPartial) {
                $("#bwContainer-txtHours").show();
                $("#bwContainer-txtTotalDays").hide();
                $("#txtHours").prop('required', true);
            } else {
                $("#bwContainer-txtHours").hide();
                $("#bwContainer-txtTotalDays").show();
                $("#txtHours").removeAttr("required").val("");

            } 
        }

        function checkDiff() {
            diff = "";

            if ($("#txtStartDate").val() != "" && $("#txtEndDate").val() != "") {
                d1 = moment($("#txtStartDate").datepicker("getDate"), szMomentFormat);
                d2 = moment($("#txtEndDate").datepicker("getDate"), szMomentFormat);
                diff = d1.businessDiff(d2);
            }
            $("#txtTotalDays").val(diff);
        }

        function checkForReadOnly() {

            if ($("#hdReadOnly").val() == "true") {

                $("#btnApprove, #btnReject").removeAttr("readonly").removeAttr("disabled");
            }
        }

        function validatePage() {
            if ($("#txtStartDate").val() == "" || $("#txtEndDate").val() == "") {
                alert('Please enter a valid start and end date');
                return false;
            }
            return true;
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server" onsubmit="return validatePage();" class="form-horizontal" enctype="multipart/form-data">
        <asp:HiddenField ID="hdTXID" runat="server" />
        <asp:HiddenField ID="hdDateUsed" runat="server" />
        <asp:HiddenField ID="hdReadOnly" runat="server" />
        <asp:HiddenField ID="hdFletcherAmountCalc" runat="server" Value="" />
        <div class="container-fluid">
            <div class="row">
                <div class="col-xs-12">
                    <bw:bwDropDownList ID="lstLeaveType" runat="server" Label="Leave type" LabelCols="2" required/>
                    <bw:bwTextBox ID="txtStartDate" runat="server" Label="Start date" required Style="background: white" LabelCols="2"/>
                    <bw:bwTextBox ID="txtEndDate" runat="server" Label="End date" required Style="background: white" LabelCols="2"/>
                    
                    <bw:bwDropDownList ID="lstPartial" runat="server" Label="Full/part day" HelpText="If the leave request is for less than one full 8 hour day, select Part Day from the drop down list and then enter the number of hours." LabelCols="2">
                        <asp:ListItem Value="" Text="Full days" />
                        <asp:ListItem Value="PARTIAL" Text="Part day" />
                    </bw:bwDropDownList>
                    
                    <bw:bwTextBox ID="txtTotalDays" runat="server" Label="Duration" LabelCols="2"  HelpText="The duration of the leave request, excluding any public holidays"/>
                    <bw:bwTextBox ID="txtHours" runat="server" Label="Hours:" LabelCols="2" type="number" min="1" max="8"/>
                    
                    <div class="form-group">
                        <label for="lblComments" class="control-label col-xs-2">Reason</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtComments" runat="server" TextMode="MultiLine"></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="FileUpload1" class="control-label col-xs-2">Supporting evidence</label>
                        <div class="col-xs-10">
                            <asp:FileUpload ID="FileUpload1" runat="server" CssClass="Entry " BorderStyle="0" accept=".pdf,.jpg" />
                            <asp:Literal ID="lExistingFile" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <asp:Panel ID="pApprovalPanel" runat="server" CssClass="form-group" Visible="false">
                        <label for="lblApprovalComment" class="control-label col-xs-2">Approval comments</label>
                        <div class="col-xs-10">
                            <asp:TextBox CssClass="Entry  form-control" ID="txtManagerComments" runat="server" TextMode="MultiLine"></asp:TextBox>
                        </div>
                    </asp:Panel>
                </div>
            </div>

            <div class="row">
                <div class='col-xs-3  col-xs-offset-1'>
                    <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="Button btn btn-primary btn-block"
                        CausesValidation="False" TabIndex="300" OnClientClick="return confirmApproval();" Visible="false"
                        Style="margin-bottom: 15px" OnClick="btnApprove_Click"></asp:Button>
                    <asp:Button ID="btnUpdate" runat="server" Text="Send approval request" CssClass="Button btn btn-block"
                        CausesValidation="False" TabIndex="100" OnClick="btnUpdate_Click" OnClientClick="return validatePage()"
                        Style="margin-bottom: 5px"></asp:Button>
                </div>
                <div class='col-xs-3 col-xs-offset-1'>
                    <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="Button btn btn-secondary btn-block center-block" Visible="false"
                        CausesValidation="False" TabIndex="300" OnClientClick="return confirmRejection();"
                        Style="margin-bottom: 15px" OnClick="btnReject_Click"></asp:Button>

                    <asp:Button ID="btnDiscussion" runat="server" Text="Discussion required" CssClass="Button btn btn-primary btn-block"
                        CausesValidation="False" TabIndex="300" OnClientClick="return confirmDiscussion();" Visible="false"
                        Style="margin-bottom: 15px" OnClick="btnDiscussion_Click"></asp:Button>

                    <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="Button btn btn-block"
                        CausesValidation="False" TabIndex="200" OnClick="btnDelete_Click" OnClientClick="return confirmDelete()"></asp:Button>
                </div>
                <div class='col-xs-3 col-xs-offset-1 pull-right'>
                    <button class="Button btn btn-block" id="btnCancel" runat="server" onclick="return cancelPage();" tabindex="120">Cancel</button>
                </div>
            </div>
            <div runat="server" id="dHistory" class="row">
                <div class="col-xs-10 col-xs-offset-2" style="margin-top: 20px">
                    <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" GridLines="None" EnableViewState="false" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="Action" HeaderText="Action" ItemStyle-Width="20%" HtmlEncode="false" />
                            <asp:BoundField DataField="SentDate" HeaderText="Email date" ItemStyle-Width="20%" HtmlEncode="false" DataFormatString="{0: MMM dd, yyyy}" />
                            <asp:BoundField DataField="Comments" HeaderText="Notes" ItemStyle-Width="60%" HtmlEncode="false" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>