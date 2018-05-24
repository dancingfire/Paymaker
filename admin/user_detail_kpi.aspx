<%@ Page Language="c#" Inherits="user_detail_kpi" CodeFile="user_detail_kpi.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        $(document).ready(function () {
            createDataTable("tList", true, true, 500);
            $(".AdminDetailPanel").corner();
            $(".EntryDate").each(function () { createCalendar($(this).attr('id')); });

            //createCalendar("txtVIDEO_239");
        });
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
    <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
        Staff KPI admin
    </div>

    <div class='AdminDetailPanel col-sm-8'>
        <asp:Repeater ID="rrList" runat="server">
            <HeaderTemplate>
                <table id='tList' class="RWInstruction RWEdit display table-hover table-striped" style="width: 100%; background: white; border: 0px" cellpadding="0" cellspacing="0">
                    <thead>
                        <tr>
                            <th style="width: 40%">Name</th>
                            <th style="width: 20%">Agent profile video</th>
                            <th style="width: 20%">Show on report</th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <%# Eval("NAME") %>
                    </td>
                    <td align="center">
                        <input class="Entry EntryDate" id="txtVIDEO_<%# Eval("ID") %>" name="txtVIDEO_<%# Eval("ID") %>" value="<%# Eval("PROFILEVIDEODATE") == System.DBNull.Value ? "" : Utility.formatDate(Convert.ToDateTime(Eval("PROFILEVIDEODATE"))) %>" />
                    </td>
                    <td align="center">
                        <input type="checkbox" id="chkSHOWONREPORT_<%# Eval("ID") %>" name="chkSHOWONREPORT_<%# Eval("ID") %>" <%# (Convert.ToInt32(Eval("SHOWONKPIREPORT")) == 1? "checked='checked'":"") %>" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>
    <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn col-sm-1" style="margin-left: 20px; margin-top:20px" OnClick="btnUpdate_Click" ></asp:Button>
    </form>
</body>
</html>