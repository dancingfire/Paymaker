<%@ Page Language="c#" Inherits="Paymaker.commission_statement_new" CodeFile="commission_statement_new.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Commission statement</title>
    <script type="text/javascript">
    function printReport() {
        window.moveTo(0, 0);
        window.resizeTo(screen.width, screen.height);
        factory.printing.leftMargin = 4.0;
        factory.printing.topMargin = 4.0;
        factory.printing.rightMargin = 4.0;
        factory.printing.bottomMargin = 4.0;
        factory.printing.portrait = true;
        factory.printing.Print(true);
    }
    </script>
    <style type="text/css">
        @media print {
            .CommissionTable {
                width: 100%;
                font-family: Tahoma, Arial, Verdana;
                font-size: 9px;
                color: black;
                border-collapse: collapse;
            }
        }

        .CommissionTable {
            width: 99%;
            font-family: Tahoma, Arial, Verdana;
            font-size: 10px;
            color: #333333;
            border-collapse: collapse;
        }

            .CommissionTable td {
                padding-left: 5px;
                padding-right: 5px;
            }

        .bottomBorder td {
            border-bottom: 1px solid black;
        }

        .bottomBorderItalics td {
            border-bottom: 1px solid black;
            padding-bottom: 5px;
            font-style: italic;
        }

        .topBorder td {
            border-top: 1px solid black;
            padding-bottom: 5px;
            padding-top: 5px;
        }

        .Header {
            width: 750px;
        }
    </style>
</head>
<body>
    <object id="factory" viewastext style="display: none"
        classid="clsid:1663ed61-23eb-11d2-b92f-008048fdd814"
        codebase="http://commission.fletchers.com.au/include/smsx.cab#Version=7,0,0,8">
    </object>
    <form id="frmMain" method="post" runat="server">
        <div id="dReport" runat="server">
        </div>
    </form>
</body>
</html>