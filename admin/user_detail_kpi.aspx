<%@ Page Language="c#" Inherits="user_detail_kpi" CodeFile="user_detail_kpi.aspx.cs" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Paymaker</title>
    <script type="text/javascript">
        function closePage() {
            document.location.href = "../welcome.aspx";
        }

        $(document).ready(function () { 
            $("#lstUser").select2();
        });
        
    </script>
    <style>
        .ui-datepicker-trigger {
            display: none;
        }

        /* For Firefox */
        input[type='number'] {
            -moz-appearance: textfield;
        }
        /* Webkit browsers like Safari and Chrome */
        input[type=number]::-webkit-inner-spin-button,
        input[type=number]::-webkit-outer-spin-button {
            -webkit-appearance: none;
            margin: 0;
        }

        .Header {
            font-weight: 800; 
            font-size: 1.2em;
            padding: 10px;
            padding-left: 0px;
        }
    </style>
</head>
<body class='AdminPage'>
    <form id="frmMain" name="frmMain" method="post" runat="server">
        <asp:HiddenField ID="hdPrevUserID" runat="server" Value="-1" />
        <div class="PageHeader" style="z-index: 107; left: -1px; width: 100%; top: 1px">
            Staff KPI admin
        </div>
        <div class="row" style="padding-top: 10px; padding-bottom: 10px">
            <div class='col-sm-6 col-sm-offset-2 '>
                <bw:bwDropDownList runat="server" ID="lstUser" Label="Select staff" OnSelectedIndexChanged="lstUser_SelectedIndexChanged" AutoPostBack="true" />
            </div>
            <div class=' col-sm-2'>
                <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button btn btn-block" OnClick="btnUpdate_Click"></asp:Button>
            </div>
        </div>
        <asp:Panel ID="pDetails" runat="server" CssClass="row" Visible="false">
            <div class='col-sm-6 col-sm-offset-2'>
                <bw:bwDropDownList runat="server" ID="lstShowOnReport" Label="Show on report">
                    <asp:ListItem Text="Yes" Value="1" />
                    <asp:ListItem Text="No" Value="0" />
                </bw:bwDropDownList>
                <div class="Header">Pre-Listing</div>
                <bw:bwTextBox ID="txtVideo" runat="server" Label="Agent profile video" type="date" />
                <bw:bwTextBox ID="txtAPPRAISALSTOLISTINGS" runat="server" Label="Appraisals to listings" />
                <bw:bwTextBox ID="txtAPPRAISALSPvsC" runat="server" Label="Appraisals personal v company" />
                <bw:bwTextBox ID="txtAPPRAISALS_ABC" runat="server" Label="Appraisals (ABC)" type="number" />
                <bw:bwTextBox ID="txtAPPRAISALSCATEGORY_A" runat="server" Label="Appraisals Category A (%)" type="number" min="1" max="100" step="any"  />
                <bw:bwTextBox ID="txtAPPRAISALS_FOLLOWUP" runat="server" Label="Appraisals with followup" type="number" />
                <bw:bwTextBox ID="txtNUMBEROFCONTACTS" runat="server" Label="Database contact numbers" type="number" />

                 <div class="Header">Listing Campaign Activity</div>
                <bw:bwTextBox ID="txtTOTALLISTINGS" runat="server" Label="Total listings in B&D	" type="number" />
                <bw:bwTextBox ID="txtWITHDRAWNLISTINGS" runat="server" Label="Withdrawn Listings (%)" type="number"    min="1" max="100" step="any" />
                <bw:bwTextBox ID="txtADVERTISINGPERPROPERTY" runat="server" Label=" Advertising per Property ($)" type="number" />
                <bw:bwTextBox ID="txtPLACES" runat="server" Label="Places Magazine / Listings (%)" type="number"  min="1" max="100" step="any" />
                <bw:bwTextBox ID="txtGLOSSIESPERLISTING" runat="server" Label="Glossies / Listings (%)" type="number"  min="1" max="100" step="any" />
                <bw:bwTextBox ID="txtGLOSSIESAVG" runat="server" Label="Glossies Average per Campaign " type="number" />
                <bw:bwTextBox ID="txtPROPERTYVIDEO" runat="server" Label="Property Video %" type="number" />

                <div class="Header">Sold Outcomes</div>
               <bw:bwTextBox ID="txtSalesTarget" runat="server" Label="Sales target ($)" type="number" />
                <bw:bwTextBox ID="txtAVGCOMMISION" runat="server" Label="Commission Per Property Sold ($)" type="number" />
                <bw:bwTextBox ID="txtAVGCOMMISIONPERCENT" runat="server" Label="Commission Per Property Sold (%)" type="number" min="1" max="100"  step="any" />
                <bw:bwTextBox ID="txtAVGSALEPRICE" runat="server" Label="Average Sale Price" type="number" />
                <bw:bwTextBox ID="txtAUCTIONCLEARANCE" runat="server" Label="Auction Clearance Rate (%)" type="number" min="1" max="100" step="any"  />
                <bw:bwTextBox ID="txtNUMBEROFDAYSAUCTION" runat="server" Label="Number Of Days To Sell (Auction)" type="number" />
                <bw:bwTextBox ID="txtNUMBEROFDAYSPRIVATE" runat="server" Label="Number Of Days To Sell (Private)" type="number" />

            </div>
        </asp:Panel>
    </form>
</body>
</html>