<%@ Page Language="C#" AutoEventWireup="true" CodeFile="dialog_frame.aspx.cs" Inherits="dialog_frame" ValidateRequest="false" %>

<!DOCTYPE html>
<html>
<head runat="server" id="Head">
    <title></title>
    <script type="text/javascript">

function doReload(){
    frames['fmain'].location.href = frames['fmain'].location.href;
}

function resizeWindow(){
    //Resize the screen to the user screen
    intScreenHeight = getWindowHeight();
    intScreenWidth = clientWidth();

    $("#fMain").height(intScreenHeight - 20)
               .show();

}

function getWindowHeight(){
    var functionReturn = 0;

    if ( (document.documentElement) && (document.documentElement.clientHeight) )
        functionReturn = document.documentElement.clientHeight;
    else if ( (document.body) && (document.body.clientHeight) )
        functionReturn = document.body.clientHeight;
    else if ( (document.body) && (document.body.offsetHeight) )
        functionReturn = document.body.offsetHeight;
    else if ( window.innerHeight )
        functionReturn = window.innerHeight - 18;

    functionReturn = parseInt(functionReturn);
    if ( (isNaN(functionReturn) == true) || (functionReturn < 0) )
        functionReturn = 0;

    return functionReturn;
}
    </script>
</head>
<body onload='resizeWindow()' style="overflow: hidden; margin: 0px;">
    <form id="frmMain" runat="server">
        <iframe name="fMain" id="fMain" runat="server" frameborder="0" scrolling="auto" style='width: 100%; display: none; height: 100%; overflow: auto;'></iframe>
        <iframe name="header" src="../blank.html" scrolling="no" style="height: 0px; width: 0px; display: none; border: solid 1px red"></iframe>
    </form>
</body>
</html>