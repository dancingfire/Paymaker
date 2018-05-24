<%@ Page Language="c#" Inherits="Paymaker.db_test" CodeFile="db_test.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>about</title>

    <script>
        function doTableSize() {
            window.open("../admin/table_size.aspx", "", "");
        }
    </script>
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
                                <td align="center">© 2011 Funk eConsulting&nbsp;
                                </td>
                                <td></td>
                            </tr>
                        </table>
                        <asp:Button ID="btnTableSize" Style="z-index: 101; left: 360px; position: absolute; top: 312px"
                            runat="server" Text="DB Size" Width="72px" CssClass="Button btn"></asp:Button>
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