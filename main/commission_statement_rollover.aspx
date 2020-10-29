<%@ Page Language="c#" Inherits="Paymaker.commission_statement_rollover" CodeFile="commission_statement_rollover.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Commission Rollover</title>
    <script>
        $(document).ready(function () {
            $('#gvData').dataTable({
                bPaginate: false,
                bLengthChange: false,
                bFilter: true,
                bSort: true,
                bInfo: false,
                scrollY: '70vh',
                scrollCollapse: true,
            });
            addZebra('gvData');
        });

        function getSelections() {
            IDList = "";
            $(".check:checked").each(function () {
                if (IDList != "") {
                    IDList += ",";
                }
                IDList += $(this).attr("data-id");
            });
            return IDList;
        }

        function validatePage() {
            IDList = getSelections();
            if (IDList == "") {
                alert("Please select the agents first please.");
                return false;
            }
            $("#hdSelValues").val(IDList);
            return true;
        }
    </script>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdSelValues" runat="server" />
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <div class="container-fluid">
            <div class="row">
                <div id="pPageHeader" class='col-12 PageHeader' runat="server">
                    Commission Statement Rollover
                </div>
            </div>
            <div class="row ActionPanel">
                <div class="col-xs-1">
                    <asp:Label ID="Label3" CssClass="Label" runat="server" Text="Pay period" />
                </div>
                <div class="col-xs-2">
                    <asp:DropDownList ID="lstPayPeriod" CssClass="Entry EntryPos" runat="server">
                    </asp:DropDownList>
                </div>
                <div class="col-xs-2">
                    <asp:Button ID="btnPreview" runat="server" CssClass="btn btn-primary btn-block" Text="Show commissions" OnClick="btnPreview_Click" />
                </div>
               
            </div>
            <div class="row">
                <div class="col-xs-6">
                     <asp:Panel ID="pPageNotification" runat="server" Visible="false" CssClass="PageNotificationPanel"></asp:Panel>

                    <asp:GridView ID="gvData" runat="server" Visible="false" Style="float: left; clear: both" EnableViewState="false" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="NAME" HeaderText="Agent" />
                            <asp:BoundField DataField="DISTRIBUTIONOFFUNDS" DataFormatString="{0:C}" HeaderText="Distribution amount" />
                            <asp:BoundField DataField="CHECKBOX" HtmlEncode="false" HeaderText="Create forward balance" />
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="col-xs-2">
                    <asp:Button ID="btnFinalize" runat="server" CssClass="btn btn-primary btn-block" Text="Create commission records" OnClick="btnFinalize_Click" OnClientClick="return validatePage()" Visible="false" style="margin-top: 30px"/>
                </div>
            </div>
        </div>
    </form>
</body>
</html>