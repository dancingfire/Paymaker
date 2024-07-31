<%@ Page Language="c#" Inherits="Paymaker.login_hidden" CodeFile="login_hidden.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Fletchers Login</title>
    <script type="text/javascript">
        function validatePage() {
            if (jQuery.trim($("#txtUserName").val()) == "") {
                $("#spUserNameError").show();
                return false;
            }
            if (jQuery.trim($("#txtPassword").val()) == "") {
                $("#spPasswordError").show();
                return false;
            }
        }
        $(function () {
            $("#txtUserName").focus();
            $("#spUserNameError").hide();
            $("#spPasswordError").hide();
        });

        function forgotPwdModal() {
            $('#mdForgotPwd').modal('show');
        }

        function validateResetPwd() {
            if (jQuery.trim($("#txtForgotUsername").val()) == "") {
                $("#md_spUserNameError").show();
                return false;
            } else {
                $('#mdForgotPwd').modal('hide');
            }
        }

        $(document).ready(function () {
            $("#dLogin").corner();
        });
    </script>
    <style type="text/css">
        #loginForm label.error {
            margin-left: 10px;
            width: auto;
            display: inline;
        }

        .Content {
            width: 580px;
            margin-left: 0;
            margin-right: 0;
        }
    </style>
</head>
<body style='width: 100%'>
    <form id="frmMain" method="post" runat="server">
         <div style='width: 100%; text-align: center; margin-top: 150px;'>
            <div id='dLogin' style='width: 600px; margin-left: auto; margin-right: auto; padding: 20px' class='Login'>
               <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel"></asp:Panel>
               
                <img src="sys_images/logo.png" alt='Fletchers logo' style='float: left;' />
                 
                <span class='LogoWord' style='float: right; text-align: left; margin-right: 20px'>
                    <span class='LogoLeadingLetter'>C</span>OLLECTIONS<br />
                    <span class='LogoLeadingLetter'>A</span>ND<br />
                    <span class='LogoLeadingLetter'>P</span>AYROLL<br />
                    <span class='LogoLeadingLetter'>S</span>YSTEM<br />
                </span>

                <div id="pPage" class='Login Content' style='text-align: center;' runat="server">
                    <br />
                    <br />
                    <table id="loginForm" cellspacing="1" cellpadding="1" align="center" border="0">
                        <tr>
                            <td style="width: 100px">
                                <asp:Label ID="Label1" runat="server" CssClass="Label LoginLabel" Text="Email:"
                                    Width="100px"></asp:Label>
                            </td>
                            <td style="width: 200px">
                                <asp:TextBox ID="txtUserName" TabIndex="10" runat="server" CssClass="Entry"></asp:TextBox>
                            </td>
                            <td style="width: 100px">
                                <asp:Button ID="btnSubmit" TabIndex="40" runat="server" CssClass="Button btn" Width="70px"
                                    Text="Login" OnClientClick="return validatePage()" OnClick="btnSubmit_Click"></asp:Button>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <span class="Error" id="spUserNameError">You must enter a login name.</span>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label2" runat="server" CssClass="Label LoginLabel" Text="Password:"
                                    Width="100px"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtPassword" TabIndex="20" runat="server" CssClass="Entry" TextMode="Password"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <span class="Error" id="spPasswordError">You must enter a password.</span>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <a href="#" onclick=" return forgotPwdModal();" class='forgot-password' tabindex="40" title="Click to retrive your password.">Forgot your password?</a>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Label ID="lblTimeout" runat="server" Text="Due to security reasons, you have been logged out of the application"
                                    CssClass="Error"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Label ID="Msg" runat="server" ForeColor="red"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>

        <!--Modal Dialog to reset Password -->
        <div id="mdForgotPwd" class="modal fade">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title">Forgot your password</h4>
                    </div>
                    <div class="modal-body form-signin-md">
                        <div class="container" style="width: 100%">
                            <div class="row clearfix">
                                <div class="col-md-12 column">
                                    <h5>Please enter your email address below to receive an email allowing you to reset your password.</h5>
                                    <span id="md_spUserNameError" class="help-inline Error">You must enter an email address below</span>
                                    <div class="form-group">
                                        <label>Email <span class="text-danger">*</span></label>
                                        <input type="text" class="form-control" id="txtForgotUsername" name="txtForgotUsername" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer" style="display: block !important">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                        <asp:Button type="submit" class="btn btn-primary" runat="server" Text="Reset Password" OnClientClick="return validateResetPwd();" OnClick="btnReset_Click" CausesValidation="false" EnableClientScript="true"></asp:Button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </form>
</body>
</html>