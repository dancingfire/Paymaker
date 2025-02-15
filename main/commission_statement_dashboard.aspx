<%@ Page Language="c#" Inherits="commission_statement_dashboard" CodeFile="commission_statement_dashboard.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome</title>
    <script type="text/javascript">
    $(document).ready(function () {
        if ($("#gvHistory tr").length > 1) {
            var oTable = $('#gvHistory').dataTable({
                "bPaginate": false,
                "bLengthChange": false,
                "bFilter": false,
                "bSort": true,
                "bInfo": false,
                "bAutoWidth": false,
                "sScrollY": "65vh",
                "sScrollX": "99%",
                "aaSorting": []
            });
        }
        $("#lstAgent").select2();
        $(".DataPanel").corner();
    });
    </script>
    <style type="text/css">
        .DataPanel {
            background: #E6D5AC;
            border: solid 1px #C49C42;
            color: #6F5928;
        }

        .DataPanelHeader {
            font-size: 14px;
            font-weight: 800;
            color: #6F5928;
            padding: 2px;
        }

        #gvCurrent tbody tr:hover, #gvFuture tbody tr:hover, #gvCampaign tbody tr:hover {
            background-color: #DDC691 !important;
            cursor: pointer;
        }
    </style>
</head>
<body style='margin-top: 5px'>
<form id="frmMain" method="post" runat="server" target="_self">
    <div class='container-fluid'>
        <div class="row">
            <div class='DataPanel col-sm-4' style='height: 80vh'>
                <div class='DataPanelHeader'>
                    View commission PDFs
                </div>
                <div class="form-group">
                    <asp:Label runat="server" Text="Select agent" CssClass="Label"></asp:Label>    
                     <asp:DropDownList ID="lstAgent" runat="server" AutoPostBack="true" OnSelectedIndexChanged="lstAgent_SelectedIndexChanged" CssClass="Entry form-control" ></asp:DropDownList>
                </div>
            </div> 
            <div class="col-sm-8">
               
                <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" CssClass="SelectTable" EmptyDataText="No Data Found" Width="95%">
                    <Columns>
                        <asp:TemplateField HeaderText="Settled Date" HeaderStyle-Width="50%" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <%# getReportLink(Eval("Month").ToString(), Eval("Year").ToString(), Eval("PayPeriodID").ToString())%>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CommissionTotal" HeaderText="Amount" HeaderStyle-Width="50%" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N2}" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</form>
</body>
</html>