<%@ Page Language="C#" AutoEventWireup="true" CodeFile="action_detail.aspx.cs" Inherits="action_detail" ValidateRequest="false" %>

<!DOCTYPE html>
<html>
<head runat="server" id="Head">
    <title>Create email</title>

    <script type="text/javascript">
arActions = new Array();
function addAction(intID, intTemplateID, intDays, szEmailSubject) {
    var o = new Object();
    o.ID = intID;
    o.TemplateID = intTemplateID;
    o.Days = intDays;
    o.EmailSubject = unescape(szEmailSubject);
    arActions[o.ID] = o;
}

function doValidate(){
     if($("#txtSubject").val() == ""){
        alert("You must enter a subject.");
        return false;
     }

     return true;
}

function closePage() {
    parent.closeAction();
}

function changeActionList() {
    if ($("#lstActionID").val() == -1)
        return;

    oAction = arActions[$("#lstActionID").val()];
    intTemplateID = oAction.TemplateID;
    intDays = oAction.Days;
    if (intDays > 0) {
        var dtToday = new Date();
        dtToday.setDate(dtToday.getDate() + intDays);
        $("#txtReminder").datepicker('setDate', dtToday);
    }

    $("#txtSubject").val(oAction.EmailSubject);
    if (intTemplateID > -1)
         callWebMethod("../web_services/ws_Paymaker.asmx", "getTemplateHTML", ["TemplateID", intTemplateID, "CampaignID", $("#HDCampaignID").val(), "CurrentUserID", $("#HDCurrentUserID").val()], loadTemplateSuccess);
}

function loadTemplateSuccess(szHTML) {
    CKEDITOR.instances.txtContent.setData(szHTML);
}

function Close() {
    parent.closeAction();
}

function replyEmail() {
    toggleEmail(true);
    toggleHistory(false);
}

function toggleEmail(blnShow) {
    if (blnShow) {
        if ($("#HDCurrentNoteUserID").val() == $("#HDCurrentUserID").val()) {
            //This is the agent replying - keep the screen really simple
            $("#dNewNote").hide();
            $("#btnPrint").hide();
            $("#txtSubject").val("Agent replying");
            $("#txtContent").val();
        } else {
            $("#lstActionID").val(-1);
        }
        $("#dEmailPanel").slideDown();
    } else {
        $("#dEmailPanel").hide();
    }
}

function toggleHistory(blnShow) {
    if (blnShow) {
        $("#dHistory").show();
    } else {
        $("#dHistory").hide();
    }
}

function updateReminderDate() {
    saveReminderDate($("#txtOrigReminder").val());
}

function clearReminderDate() {
    $("#txtOrigReminder").val('')
    saveReminderDate("NULL");
}

function saveReminderDate(szDate) {
    callWebMethod("../web_services/ws_Paymaker.asmx", "updateActionReminderDate", ["ReminderDate", szDate, "CampaignNoteID", $("#HDCampaignNoteID").val(), "CurrentUserID", $("#HDCurrentUserID").val()], loadTemplateSuccess);
}

function loadPage() {
    $(".Panel").corner();
    createCalendar("txtReminder");
    if ($("#HDCampaignNoteID").val() > -1) {
        toggleEmail(false);
        toggleHistory(true);
    } else {
        toggleEmail(true);
        toggleHistory(false);
    }
}

$(document).ready(function () {
    CKEDITOR.replace('txtContent');

});
    </script>
    <style type="text/css">
        .EntryPos {
            width: 240px;
        }

        .Panel {
            background: #E6D5AC;
            border: solid 1px #C49C42;
        }

        .ActionHeader {
            font-size: 14px;
            background: #E6D5AC;
            font-weight: 900;
            margin-top: 5px;
            float: left;
            clear: both;
            width: 750px;
        }

        .Email {
            border-bottom: 1px solid #C49C42;
            float: left;
            background: #F3ECDA;
            width: 750px;
        }

        .EmailDetails {
            border-right: solid 1px #C49C42;
            background: #FAF7ED;
        }

        .EmailMessage {
            background: #F3ECDA;
        }
    </style>
</head>
<body style="margin-top: 0px" onload='loadPage()'>
    <form id="frmMain" runat="server">
        <div id="dPageHeader" class='PageHeader ActionPanel' runat="server">
            <asp:Label ID="txtPageHeader" CssClass="Normal EntryPos" runat="server" Text=""></asp:Label>
        </div>
        <asp:HiddenField ID="HDCampaignID" runat="server" />
        <asp:HiddenField ID="HDCurrentUserID" runat="server" />
        <asp:HiddenField ID="HDCampaignNoteID" runat="server" Value="-1" />
        <asp:HiddenField ID="HDCurrentNoteUserID" runat="server" Value="-1" />
        <asp:HiddenField ID="HDPropertyDetails" runat="server" Value="" />
        <div id='dHistory' style='float: left; width: 850px; display: none;' runat="server" enableviewstate="false">
            <div style='float: left; width: 750px; margin-top: 5px;' id='dHistoryDetail' runat="server">
                <div id='dDetails' runat="server"></div>
                <br class='Align' />
            </div>

            <div class="RightPanel" style="width: 90px">
                <asp:Button ID="btnReply" runat="server" CssClass="Button btn" Text="Reply" OnClientClick="return replyEmail();" UseSubmitBehavior="false" Width="80" />
                <asp:Button ID="btnCancelH" runat="server" CssClass="Button btn" Text="Cancel" OnClientClick="closePage()" Width="80" />
            </div>
        </div>
        <div id='dEmailPanel' style='float: left; width: 850px; display: none' runat="server" enableviewstate="false">
            <div id='dNewNote' style='float: left; width: 650px;' runat="server">
                <asp:Label ID="lblTemplate" runat="server" Text="Action" CssClass="Label LabelPos"></asp:Label>
                <asp:DropDownList ID="lstActionID" runat="server" CssClass="Entry EntryPos" Width="400px">
                </asp:DropDownList>
                <br class="Align" />

                <div class="Label LabelPos">Subject: </div>
                <asp:TextBox ID="txtSubject" runat="server" CssClass="Entry EntryPos" MaxLength="1000" Width="400px"></asp:TextBox>&nbsp;
        <br class="Align" />

                <div class="Label LabelPos">Reminder date: </div>
                <asp:TextBox ID="txtReminder" runat="server" CssClass="Entry EntryPos" MaxLength="20" Width="400px"></asp:TextBox>&nbsp;
            </div>
            <div class="RightPanel" style="width: 90px">
                <asp:Button ID="btnSend" runat="server" CssClass="Button btn" OnClick="btnSend_Click" Text="Send" OnClientClick="return doValidate();" Width="80" />
                <asp:Button ID="btnPrint" runat="server" CssClass="Button btn" Text="Print" UseSubmitBehavior="false" OnClick="btnPrint_Click" Width="80" />

                <asp:Button ID="blnCancel" runat="server" CssClass="Button btn" TabIndex="60" Text="Cancel" OnClientClick="closePage()" Width="80" />
            </div>

            <div id="divTemplate" runat="server" style="width: 700px; float: left; height: 400px" enableviewstate="false">
                <asp:TextBox ID="txtContent" runat="server" Width="700px" Height="400" TextMode="MultiLine"></asp:TextBox>
            </div>
        </div>
        <div id="dPrint" runat="server" style='width: 750px;' visible="false">
            <object id="factory" viewastext style="display: none"
                classid="clsid:1663ed61-23eb-11d2-b92f-008048fdd814"
                codebase="http://commission.fletchers.com.au/include/smsx.cab#Version=7,0,0,8">
            </object>
            <div id="dPrintContent" runat="server"></div>
            <script>
        factory.printing.Print(true);
            </script>
        </div>
    </form>
</body>
</html>