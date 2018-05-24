<%@ Page Language="c#" Inherits="Paymaker.import" CodeFile="import.aspx.cs" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Import date testing</title>
    <style type="text/css">
    </style>
</head>
<body>
    <form id="Form1" method="post" runat="server" onsubmit="showWait('Performing your request')">
        <div class="container" style="margin-top: 50px">
            <asp:Button ID="btnTestPing" runat="server" Text="Test ping" OnClick="btnTestPing_Click" Visible="false" />
            <asp:Button ID="btnOffice" runat="server" Text="Load offices" OnClick="btnOffice_Click" />
            <asp:Button ID="btnConsultants" runat="server" Text="Load consultants" OnClick="btnConsultants_Click" Visible="true" />
            <asp:Button ID="btnPropertyType" runat="server" Text="Load property type" OnClick="btnPropertyType_Click" />
            <asp:Button ID="btnContactActivityType" runat="server" Text="Load contact activity type" OnClick="btnContactActivityType_Click" />
            <asp:Button ID="btnContactActivity" runat="server" Text="Load contact activity" OnClick="btnContactActivity_Click" />
            <asp:Button ID="btnContactCategoryType" runat="server" Text="Load contact category type" OnClick="btnContactCategoryType_Click" />
            <asp:Button ID="btnContactCategory" runat="server" Text="Load contact category" OnClick="btnContactCategory_Click" />
            <asp:Button ID="btnPropertyCategory" runat="server" Text="Load property category" OnClick="btnPropertyCategory_Click" />
            <asp:Button ID="btnProperty" runat="server" Text="Load properties" OnClick="btnProperty_Click" />
            <asp:Button ID="btnSalesListing" runat="server" Text="Load sales listings" OnClick="btnSalesListing_Click" />
            <asp:Button ID="btnListingSource" runat="server" Text="Load Listing Source" OnClick="btnListingSource_Click" />
            <asp:Button ID="btnSalesVouchers" runat="server" Text="Sales vouchers" OnClick="btnSalesVouchers_Click" />
            <asp:Button ID="btnDeductions" runat="server" Text="Deductions" OnClick="btnDeductions_Click" />
            <asp:Button ID="btnExpense" runat="server" Text="Expenses" OnClick="btnExpense_Click" />
            <asp:Button ID="btnCommissions" runat="server" Text="Commissions" OnClick="btnCommission_Click" />
            <asp:Button ID="btnContacts" runat="server" Text="Contacts" OnClick="btnContacts_Click" />
            <asp:Button ID="btnTasks" runat="server" Text="Tasks" OnClick="btnTasks_Click" />
            <asp:Button ID="btnTest" runat="server" Text="Import all" OnClick="btnTest_Click" />
            <asp:Button ID="btnProcess" runat="server" Text="Process sales" OnClick="btnProcess_Click" Visible="false" />
            <br />
            <br />
            <asp:Button ID="btnViewReport" runat="server" Text="View API history" OnClientClick="window.open('../report/apilog_detail.aspx'); return false;" />
            <div id='dOutput' runat="server" />
        </div>
    </form>
</body>
</html>