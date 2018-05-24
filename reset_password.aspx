<%@ Page Language="c#" Inherits="Paymaker.reset_password" CodeFile="reset_password.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Reset Password</title>
    <script type="text/javascript">

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

        .PageNotificationPanel {
            clear: both;
        }

        .Instructions {
            color: white;
            text-align: left;
            float: left; clear: both;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <div id="resetForm" runat="server">
            <div style='width: 100%; text-align: center; margin-top: 150px;'>
                 <div  style='width: 600px; margin-left: auto; margin-right: auto; padding: 20px;' >
                    <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel" ></asp:Panel>
                  </div>
                <div id='dLogin' style='width: 600px; margin-left: auto; margin-right: auto; padding: 20px;' class='Login' runat="server">
                    <img src="sys_images/logo.png" alt='Fletchers logo' style='float: left;' />

                   <div class="Instructions">
                      <p>To ensure the security of Fletcher's data, your passwords must:</p>
                       <ul>
                           <li> Be at least 8 characters long.</li>
                            <li>Contain at least one uppercase letter,  one lowercase letter, and one non-alphabetic character (IE: 0-9, %!@#)</li>

                       </ul>
                   </div>

                    <div id="pPage" class='Login Content' style='text-align: center;' runat="server">
                        <br />
                        <br />
                        <table id="loginForm" cellspacing="1" cellpadding="1" align="center" border="0">
                            <tr>
                                <td style="width: 100px">
                                    <asp:Label ID="Label1" runat="server" CssClass="Label LoginLabel" Text="New password:"
                                        Width="100px"></asp:Label>
                                </td>
                                <td style="width: 200px">
                                    <asp:TextBox ID="txtResetPassword" TabIndex="10" runat="server" CssClass="Entry" TextMode="Password"></asp:TextBox>
                                </td>
                                <td style="width: 100px">
                                    <asp:Button ID="btnPasswordReset" class="Button btn" runat="server" Text="Reset Password" OnClick="btnPasswordReset_Click" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="Label2" runat="server" CssClass="Label LoginLabel" Text="Confirm:"
                                        Width="100px"></asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtConfirmPassword" TabIndex="20" runat="server" CssClass="Entry" TextMode="Password"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                   
                                    
                                    <asp:CompareValidator runat="server" id="cmpNumbers"  ForeColor="Red" ControlToValidate="txtResetPassword" controltocompare="txtConfirmPassword" operator="Equal" type="String" errormessage="Your passwords do not match. Please enter them again." /><br />
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>