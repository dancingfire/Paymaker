<%@ Page Language="c#" Inherits="Paymaker.about" CodeFile="about.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Funk eConsulting</title>
</head>
<body>
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="1" cellpadding="1" width="100%" border="0">
            <tr>
                <td width="30%"></td>
                <td width="356" style="width: 356px">
                    <p>
                        &nbsp;
                    </p>
                    <p>
                        &nbsp;
                    </p>
                    <p>
                        &nbsp;
                    </p>
                </td>
                <td width="30"></td>
            </tr>
            <tr>
                <td>
                    <p align="right">
                    </p>
                </td>
                <td style="width: 357px">
                    <div style="width: 457px; color: white; position: relative; height: 352px">
                        <table width="100%" border="0" cellspacing="0" cellpadding="1" class="Label">
                            <tr>
                                <td style="width: 82px"></td>
                                <td>
                                    <p>
                                        &nbsp;
                                    </p>
                                    <p>
                                        &nbsp;
                                    </p>
                                    <p>
                                        &nbsp;
                                    </p>
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <td style="width: 82px"></td>
                                <td>
                                    <p>
                                        <font size="1"></font>&nbsp;
                                    </p>
                                    <p>
                                        <font size="6" color="#ffcc66">Funk eConsulting</font>
                                    </p>
                                    <p>
                                        &nbsp;
                                    </p>
                                </td>
                                <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
                            </tr>
                            <tr valign='top'>
                                <td style="width: 82px">
                                    <p>Created by</p>
                                </td>
                                <td>
                                    <p>
                                        Gord Funk [ <a href="mailto:gord.funk@gmail.com">gord.funk@gmail.com</a>]<br />
                                    </p>
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <td style="width: 82px"></td>
                                <td align="center">© 2015 Funk eConsulting&nbsp;
                                </td>
                                <td></td>
                            </tr>
                        </table>
                        <asp:Button ID="btnReload" runat="server" Text="Reset application" OnClick="btnReload_Click" />
                    </div>
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td id="txtValue1" style="width: 356px"></td>
                <td></td>
            </tr>
        </table>
    </form>
</body>
</html>