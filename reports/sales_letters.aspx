<%@ Page Language="c#" Inherits="Paymaker.sales_letters" CodeFile="sales_letters.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Sales letters</title>

    <script type="text/javascript">

        function printReport() {
            window.print();
        }

        $(document).ready(function () {

        });
    </script>
    <style>
        @media print {
            @page {
                size: A4 portrait;
                margin: 20mm 20mm 20mm 20mm;
                padding: 0;
            }

            .PrintTableHeader {
                background-color: #E3EBFD;
            }

            .page-break {
                display: block;
                page-break-before: always;
            }
        }

        @media screen {
            body {
                width: 800px;
            }
        }

        @media all {
            .page-break {
                display: none;
            }
             .PrintTableHeader {
                background-color: #E3EBFD;
            }
            }
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPayPeriod" Value="" runat="server" />
        <asp:HiddenField ID="hdOfficeID" Value="" runat="server" />
        <asp:HiddenField ID="hdCompanyID" Value="" runat="server" />

        <asp:GridView ID="gvData" runat="server" AutoGenerateColumns="false" ShowHeader="false" GridLines="None">
            <Columns>
                <asp:BoundField DataField="TEMPLATE" HtmlEncode="false" />
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>