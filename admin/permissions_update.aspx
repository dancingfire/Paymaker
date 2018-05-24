<%@ Page Language="C#" AutoEventWireup="true" CodeFile="permissions_update.aspx.cs"
    Inherits="permissions_update" EnableEventValidation="false" ValidateRequest="false"
    EnableViewState="false" %>

<!DOCTYPE HTML>
<html>
<head runat="server" id="Head">
    <link rel="StyleSheet" type="text/css" href="admin.css" />
    <title></title>

    <script type="text/javascript">
        var arHelp = new Array();

        function updatePermissions() {
            szReturn = "";
            $("input:checkbox:checked").each(function () {
                szReturn = append(szReturn, this.value, ",");
            });
            $("#szPermissions").val(szReturn);
        }

        function doCancel() {
            parent.closePermissions();
        }

        function doShowHelp(intPermissionIndex, oSrcObj) {
            oSrcObj.style.textDecoration = 'underline';
            oSrcObj.style.cursor = 'hand';
            szText = unescape(arHelp[intPermissionIndex]);
            document.getElementById("oToolTipBody").innerHTML = szText;
            document.getElementById("oToolTip").style.display = "inline";
        }

        function doHideHelp(oSrcObj) {
            document.getElementById("oToolTip").style.display = "none";
            oSrcObj.style.textDecoration = 'none';
            oSrcObj.style.cursor = 'default';
        }
    </script>
</head>
<body>
    <form id="frmMain" runat="server">
        <asp:HiddenField runat="server" ID='szPermissions' />
        <asp:HiddenField runat="server" ID='szCommand' />
        <table width="100%" border="0" cellspacing="0" cellpadding="0">
            <tr>
                <td class="Normal" height="26" width="80%">
                    <div id="oInstructions" runat="server" class="Normal">
                    </div>
                    <br />
                    <br />
                </td>
                <td align='right' width='20%' style="">
                    <asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" OnClientClick="updatePermissions();"
                        CssClass="Button btn" Width="90px" Text="Update" />
                    <br />
                    <br style="font-size: 4px;" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClientClick="return doCancel();"
                        CssClass="Button btn" Width="90px" UseSubmitBehavior="false" />
                </td>
            </tr>
        </table>
        <br style="font-size: 7px;" />
        <asp:Panel ID="pPermissions" runat="server">
        </asp:Panel>
        <asp:PlaceHolder ID="phToolTip" runat="server"></asp:PlaceHolder>
        <div id="oToolTip" runat="server">
            <b>Help information...</b><hr size="1" style="border: 1px solid black;" />
            <span id='oToolTipBody'></span>
        </div>
    </form>
</body>
</html>