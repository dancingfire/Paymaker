<%@ Page Language="c#" Inherits="Paymaker.payroll_dashboard" CodeFile="payroll_dashboard.aspx.cs" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>Payroll dashboard</title>

    <script type="text/javascript">
      
        function loadPage() {
            $(".Panel").corner();
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
            <div id='dPayrollManagement' runat="server" class='PanelCol'>
                <div class='PanelHeader'>
                    Payroll management
                </div>
                <div class='Panel'>
                    <a href='payroll_update.aspx'>Enter current hours</a>
                </div>
            </div>

            <div id='dPayrollAdmin' runat="server" class='PanelCol'>
                <div class='PanelHeader'>
                    Administration
                </div>

                <div class='Panel'>
                    <a href='payroll_summary.aspx?type=ALL'>View current payroll cycle</a>
                </div>

                <div ID='dPDFFiles' class='Panel' runat="server">
                    <a href='../admin/payroll_pdf_files.aspx'>View payroll PDF records</a>
                </div>

                <div id="dCheck" runat="server" class='Panel'>
                    <a href='../automation/email_notification.aspx?Check=true' target="blank">Check emails</a>
                </div>

                 <div id="Div1" runat="server" class='Panel'>
                    <a href='../payroll/staff_list.aspx?' target="blank">View staff list</a>
                </div>
            </div>
        </div>
    </form>
</body>
</html>