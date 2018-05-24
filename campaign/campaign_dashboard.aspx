<%@ Page Language="c#" Inherits="Paymaker.campaign_dashboard" CodeFile="campaign_dashboard.aspx.cs" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Campaign dashboard</title>

    <script type="text/javascript">
        function loadPage() {
            $(".Panel").corner();
        }

        function showProgress(){
            window.open("import_progress.aspx");
        }

        function performImport() {
            document.frmMain.target = '_blank';
            ;// window.setTimeout("showProgress()", 2000);
        }

        function startImportSuccess(szResults) {
            getCurrentCount();
        }
    </script>
    <style>
        .Panel {
            background: #E6D5AC;
            border: solid 1px #C49C42;
            float: left;
            width: 280px;
            font-size: 14px;
            font-weight: 800;
            padding: 20px;
            color: #6F5928;
            clear: both;
            margin-top: 15px;
        }

        .PanelHeader {
            background: #CFB472;
            border: solid 1px #C49C42;
            float: left;
            width: 300px;
            font-size: 16px;
            font-weight: 800;
            padding: 10px;
            color: #6F5928;
            margin-top: 15px;
        }

        .PanelCol {
            float: left;
            width: 330px;
        }

        .Panel a {
            color: #957937;
        }
    </style>
</head>
<body onload='loadPage()'>
    <form id="frmMain" method="post" runat="server">
        <div style='text-align: center; float: left; padding-left: 50px; margin-top: 40px'>
            <div class='PanelCol'>
                <div class='PanelHeader'>
                    Campaign management
                </div>
                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=NEW'>New campaigns</a>
                </div>
                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=ALL'>Summary of all current campaigns</a>
                </div>
                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=EXCEEDING'>Campaigns exceeding authority</a>
                </div>
                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=DUEFORCOLLECTION'>Campaigns due for collection</a>
                </div>
                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=OVERDUE30DAYS'>Campaigns overdue for collection</a>
                </div>
                <div class='Panel'>
                    <a href='campaign_actions.aspx'>Campaigns requiring action</a>
                </div>
                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=COMPLETED'>Archived campaigns </a>
                </div>
            </div>

            <div class='PanelCol'>
                <div class='PanelHeader'>
                    Invoice management
                </div>

                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=INVOICEDUE'>Campaigns due for invoicing</a><br />
                </div>

                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=INVOICEPARTIAL'>Campaigns due for partial invoicing</a>
                </div>

                <div class='Panel'>
                    <a href='campaign_detail.aspx?type=MANUAL'>Invoices requiring manual adjustment</a>
                </div>

                <div class='Panel'>
                    <a href='myob_export.aspx'>Export to MYOB</a>
                </div>
            </div>

            <div class='PanelCol'>
                <div class='PanelHeader'>
                    Administration
                </div>

                <div class='Panel'>
                    <a href='../admin/campaign_settings.aspx'>Settings</a><br />
                </div>

                <div class='Panel'>
                    <a href='../admin/template_detail.aspx'>Email templates</a><br />
                </div>

                <div class='Panel'>
                    <a href='../admin/product_detail.aspx'>Product admin</a><br />
                </div>
                <div class='Panel'>
                    <a href='../campaign/management_dashboard.aspx'>Management dashboard</a><br />
                </div>

                <div class='Panel'>
                    <a href='../admin/action_detail.aspx'>Campaign actions</a><br />
                </div>

                <div class='Panel'>
                    <asp:Button ID="btnLoad" runat="server" Text="Load new campaigns" target="_blank" CssClass="Button btn"
                        OnClientClick="performImport()"
                        OnClick="btnLoad_Click" />
                    <br />
                </div>
                <div class='Panel'>
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh all campaigns" target="_blank" CssClass="Button btn"
                        OnClientClick="performImport()"
                        OnClick="btnRefreah_Click" />
                    <br />
                </div>
            </div>
        </div>
        <div id='oImportProgress' style='display: none; border: 2px;'>
            This is the dialog
        </div>
    </form>
</body>
</html>