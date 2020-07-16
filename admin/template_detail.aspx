<%@ Page Language="C#" AutoEventWireup="true" CodeFile="template_detail.aspx.cs" Inherits="template_detail" ValidateRequest="false" %>

<!DOCTYPE HTML>
<html>
<head runat="server" id="Head">
    <title>Template detail</title>

    <script type="text/javascript">
        blnIsTemplate = true;
        function doValidate() {
            if ($("#txtName").val() == "") {
                alert("Please enter a name for this template.");
                return false;
            }

            szContent = CKEDITOR.instances.txtContent.getData();
            if (szContent == "") {
                alert("You must enter some text into the template.");
                return false;
            }

            blnPageDirty = false; //Turn off our page leaving check
            return true;
        }

        function insertTag(szValue) {
            CKEDITOR.instances.txtContent.insertText('[' + szValue + ']');
        }

        function checkDelete() {
            return confirm("Are you sure that you want to delete this template?")
        }

        function doLoad() {

        }

        blnPageDirty = false;
        function checkPageDirty() {
            if (blnPageDirty)
                return ""; //THrow the default browser message
        }

        function xsContentChanged(id) {
            blnPageDirty = true;
        }

        $(document).ready(function () {

            CKEDITOR.replace("txtContent", {
                height: 80vh
            });
            $('form').submit(function (event) {
                $('#txtSubmit').val(btoa(CKEDITOR.instances['txtContent'].getData()));
                CKEDITOR.instances['txtContent'].setData('');
            });
        });
    </script>
    <style type="text/css">
        .Help {
            border: 2px solid #374351;
            font-family: verdana;
            background: #FFFD9F;
            text-align: center;
        }

        .EntryPos {
            width: 400px;
        }

        .HelpText {
            font-family: verdana;
            font-size: 10pt;
            padding: 5px;
            background: #FFFD9F;
            text-align: left;
        }
    </style>
</head>
<body id="oBody" runat="server" style="margin-top: 10px">
    <form id="frmMain" runat="server">
        <input type="hidden" name="txtSubmit" id="txtSubmit" />
        <div id="Page" style='width: 850px'>
            <asp:Panel ID="pMaster" runat="server" Height="50px">
                <asp:Label ID="lblTemplate" runat="server" Text="Templates" CssClass="Label LabelPos"></asp:Label>
                <asp:DropDownList ID="lstSelTemplateID" runat="server" CssClass="Entry EntryPos" OnSelectedIndexChanged="lstSelTemplateID_SelectedIndexChanged" AutoPostBack="True">
                </asp:DropDownList>

                <asp:Panel ID="Panel1" runat="server" CssClass="RightPanel" Width="100" Height="26px">
                    <asp:Button ID="btnInsert" runat="server" CssClass="Button btn" OnClick="btnInsert_Click" Text="Insert" />
                </asp:Panel>
            </asp:Panel>
        </div>
        <asp:Panel ID="pTemplateUpdate" runat="server" Visible="False" Style="width: 95%">
            <div style="width: 98%;">
                <div style="width: 750px">
                    <div class="RightPanel" style="width: 90px">
                        <asp:Button ID="btnUpdate" runat="server" CssClass="Button btn" OnClick="btnUpdate_Click" Text="Update" OnClientClick="return doValidate();" />
                        <asp:Button ID="blnCancel" runat="server" CssClass="Button btn"
                            TabIndex="60" Text="Cancel" OnClick="blnCancel_Click" />
                        <asp:Button ID="btnDelete" runat="server" CssClass="Button btn" OnClick="btnDelete_Click"
                            TabIndex="60" Text="Delete" OnClientClick="return checkDelete();" />
                        <br />
                    </div>
                    <div style='float: left; width: 650px;'>

                        <div class="Label LabelPos">Name: </div>
                        <asp:TextBox ID="txtName" runat="server" CssClass="Entry EntryPos" MaxLength="100"></asp:TextBox>&nbsp;

            <div class="Label LabelPos">
                Description:
                        </div>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="Entry EntryPos" Rows="4"
                            TextMode="MultiLine"></asp:TextBox>&nbsp;
                    </div>
                    <br class="Align" />
                </div>
                <br class="Align" />

                <div id="divTemplate" runat="server" style="width: 70%; float: left; height: 80vh" enableviewstate="false">
                    <asp:TextBox ID="txtContent" runat="server" Width="100%" style="height: 75vh" TextMode="MultiLine" Rows="40"></asp:TextBox>
                </div>
                <div id="divTemplateTerms" enableviewstate="false" runat="server" style="margin-left: 5px; float: left; padding-left: 0px; clear: right; width: 28%">
                    <div style='background: silver; text-align: center; font-family: Arial; padding: 3px;'>Replaceable terms</div>
                    <div id="divCustomWords" runat="server" style="float: left; text-align: left; display: inline-block; overflow-x: hidden; height: 220px; width: 100%; margin-bottom: 5px; overflow: auto; border: solid 1px silver">
                        <a href='javascript: insertTag("AGENTFIRSTNAME")'>Recipent first name</a><br />
                        <a href='javascript: insertTag("AGENTLASTNAME")'>Recipent last name</a><br />
                        <a href='javascript: insertTag("SENDERFIRSTNAME")'>Sender first name</a><br />
                        <a href='javascript: insertTag("SENDERLASTNAME")'>Sender last name</a><br />
                        <a href='javascript: insertTag("PROPERTYDETAILS")'>Property details</a><br />
                        <br />

                        <a href='javascript: insertTag("AMOUNTEXCEEDINGAUTHORITY")'>Amount exceeding authority</a><br />
                        <a href='javascript: insertTag("INVOICEDETAILS")'>Invoice details</a><br />
                        <a href='javascript: insertTag("OUSTANDINGBALANCE")'>Outstanding balance</a><br />
                        <a href='javascript: insertTag("TOTALSPEND")'>Total spend</a><br />
                        <a href='javascript: insertTag("TOTALINVOICED")'>Total invoiced</a><br />
                        <a href='javascript: insertTag("TOTALPAID")'>Total paid</a><br />
                    </div>
                    <br class="Align" />
                </div>
            </div>
        </asp:Panel>
    </form>
</body>
</html>