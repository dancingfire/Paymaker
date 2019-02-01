<%@ Page Language="c#" Inherits="holiday_detail" CodeFile="holiday_detail.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function viewItem(intID) {
            $("#frmMain").attr("action", "holiday_update.aspx?intItemID=" + intID ).submit();
        }

        $(document).ready(function () {
            createDataTable("gvList", true, true, 380);
        });

        function refreshPage() {
            document.location.href = document.location.href;
        }

        function uploadFile() {
            $("#frmMain").attr("target", "");
        }
        function closePage() {
            document.location.href = "../welcome.aspx";
        }
    </script>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server" target="frmUpdate">
        <div class="PageHeader" style="z-index: 107; width: 100%; top: 1px">
            <asp:Label ID="lblItemName" runat="server" Text="Public holidays"></asp:Label>
        </div>

        <div style='float: left; width: 30%;'>
            <div class="ListContainer" style="overflow: hidden; width: 100%; height: 457px; float: left">
                <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"
                    OnRowDataBound="gvList_RowDataBound" Width="100" EnableViewState="false">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Name" HeaderStyle-Width="70%" ItemStyle-Width="70%" />
                        <asp:BoundField DataField="HolidayDate" HeaderText="Date" HeaderStyle-Width="30%"  DataFormatString="{0: MMM dd, yyyy}"/>
                    </Columns>
                </asp:GridView>
            </div>
            <div class='AdminActionPanel' style='float: left; text-align: right; width: 100%'><a href='javascript: viewItem(-1)'>
                <asp:Label ID="lblInsertText" runat="server" Text="Add holiday" CssClass="LinkButton"></asp:Label></a></div>
                <asp:FileUpload ID="FileUpload1" runat="server" />
        <asp:Button ID="btnUploadFile" runat="server" Text="Upload file" OnClick="btnUploadFile_Click"  OnClientClick="uploadFile()" />
        </div>

     
        <iframe id='frmUpdate' name='frmUpdate' class='AdminUpdateFrame' frameborder='0' src='../blank.html'></iframe>
    </form>
</body>
</html>