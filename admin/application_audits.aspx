<%@ Page Language="c#" Inherits="Paymaker.application_audits" CodeFile="application_audits.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Application Settings</title>
    <style>
        .Adminpanel {
            width: 900px;
        }
    </style>
    <script type="text/javascript">
        $().ready(function () {
            createCalendar("txtLoginSearchStartDate");
            createCalendar("txtLoginSearchEndDate");

            $('#frmMain').submit(function () {
                return checkDates(); 
            });
        });

        function checkDates() {
            var $start = $('#txtLoginSearchStartDate');
            var $end = $('#txtLoginSearchEndDate');
            var $startDate = new Date($start.val());
            var $endDate = new Date($end.val());
            if ($start.val() == "" || $end.val() == "") {
                alert("Please enter the start and end dates");
                return false;
            } else if ($startDate != "" && $endDate != "") {
                if ($startDate > $endDate) {
                    alert("Start date cannot be  after the end date");
                    return false;
                }
            }
            return true;
        }
    </script>
</head>
<body class="Search">
    <form id="frmMain" method="post" runat="server">
        <div class="Normal">
            Enter the information that you wish to search on:
        </div>

        <label for="txtLoginSearchStartDate">Start date</label>
        <asp:TextBox ID="txtLoginSearchStartDate" runat="server" Width="224px" Text="" CssClass="Entry" EnableViewState="False" />


        <label for="txtLoginSearchEndDate">End date</label>
        <asp:TextBox ID="txtLoginSearchEndDate" runat="server" EnableViewState="False" Text="" Width="224px" CssClass="Entry" />

        <asp:Button ID="btnSearch"  type="submit" runat="server" Text="View" />

        <div style="height: 500px; width: 50%; overflow: auto;">
            <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false"
                EmptyDataText="No Data Found" Width="99%" EnableViewState="false">
                <Columns>
                    <asp:BoundField DataField="DATE" HeaderText="Date" ItemStyle-Width="20%" />
                    <asp:BoundField DataField="TIME" HeaderText="Time" ItemStyle-Width="18%" />
                    <asp:BoundField DataField="USERNAME" HeaderText="Username" ItemStyle-Width="30%" />
                    <asp:BoundField DataField="RESULT" HeaderText="Result" ItemStyle-Width="16%" />
                    <asp:BoundField DataField="IPADDRESS" HeaderText="IP Address" ItemStyle-Width="16%" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>