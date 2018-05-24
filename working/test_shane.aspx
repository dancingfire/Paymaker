<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test_shane.aspx.cs" Inherits="test" %>

<!DOCTYPE html>
<html>
<head runat="server">

    <title>Test info</title>
    <script type="text/javascript">
        function loadPage() {
            lockFields('txtDisabled');
        }
    </script>
</head>
<body onload="loadPage();">
    <form id="frmMain" method="post" runat="server">

        <div runat="server" id="dOutput" />
    </form>
</body>
</html>