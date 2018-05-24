<%@ Page Language="c#" Inherits="db_timeout" CodeFile="db_timeout.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
</head>
<body>
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
            <tr>
                <td></td>
                <td align="center">
                    <p>&nbsp;</p>
                    <p>&nbsp;</p>
                    <p>&nbsp;</p>
                    <p>&nbsp;</p>
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td align="center">
                    <div style="font-weight: bold; font-size: 12pt; width: 336px; color: #F82905; font-family: Verdana; position: relative; height: 175px; background-color: #FAFCB1" class="menu">
                        <div class="Normal" id="dWelcome" style="display: inline; z-index: 101; left: 64px; width: 216px; position: absolute; top: 32px; height: 96px"
                            runat="server">
                            The DB has timed out. Please try again.<br />
                            :(&nbsp;
                        </div>
                    </div>
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td align="center"></td>
                <td></td>
            </tr>
        </table>
    </form>
</body>
</html>