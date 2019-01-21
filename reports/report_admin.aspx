<%@ Page Language="c#" Inherits="Paymaker.report_admin" CodeFile="report_admin.aspx.cs" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Report admin</title>
    <script type="text/javascript">
        blnSingleUser = false;

        $(document).ready(function () {
            createCalendar("txtStartDate");
            createCalendar("txtEndDate");
            showFilter();
            $("#lstReport, #lstUserReport").change(function () {
                showFilter();
            });
            $("#fReport").height($("#dReportFilter").height())
            $(".DataPanel").corner();
        });

        function showFilter() {
            hideAllFilters();
            szReportVal = getReport();
            if (szReportVal == "-1")
                return;
            switch (szReportVal) {
                case "AR_REPORT":
                    $("#spDate").show();
                    $("#spCompany").show();
                    break;
                case "AGENTOFFTHETOP":
                    $("#spUser").show();
                    $("#spDate").show();
                    $("#spOffTheTop").show();
                    $("#spActive").show();
                    break;
                case "BUDGETREPORT":
                    $("#lstFinancialYear").attr('size', '1')
                        .removeAttr('size')
                        .removeAttr('multiple')
                        .val('-1');
                    $("#spDate").show();
                    $("#spFinYear").show();
                    break;

                case "CAMPAIGNOUTSTANDING":
                    $("#spCompany").show();
                    break;
                case "CAMPAIGNSAGING":
                    $("#spCompany").show();
                    break;
                case "COMMISSION":
                case "COMMISSION_NEW":
                    $("#spUser, #spRecreate").show();
                    $("#spPayPeriod").show();
                    $("#spCompany").show();
                    break;

                case "EOFYBONUSSUMMARY":
                    $("#spPayPeriod").show();
                    break;

                case "EOFYBONUSDETAIL":
                    $("#spPayPeriod").show();
                    break;

                case "EXPENSESUMMARY":
                    $("#lstFinancialYear").attr('size', '1')
                        .removeAttr('size')
                        .removeAttr('multiple')
                        .val('-1');
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spFinYear").show();
                    $("#spCompany").show();
                    $("#spExpense").show();
                    $("#spFletcherOrUserAmount").show();
                    break;
                case "INCENTIVE":
                case "INCENTIVECHART":
                    $("#spCompany").show();
                    $("#lstFinancialYear").attr('size', '1')
                        .removeAttr('size')
                        .removeAttr('multiple')
                        .val('-1');
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spFinYear").show();
                    break;
                case "MAINKPI":
                    $("#lstFinancialYear option[value='']").remove();
                    //$("#spUser").show();
                    //$("#spOffice").show();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#spActive").show();

                    break;
                case "KPIOFFICE":
                case "KPIOFFICENEW":
                case "KPIOFFICEAUCTION":
                    $("#lstFinancialYear option[value='']").remove();
                    //$("#spUser").show();
                    //$("#spOffice").show();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#spActive").show();

                    break;
                case "COMMISSIONTOTAL":
                case "MENTORCOMMISSION":
                case "FARMCOMMISSION":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#spActive").show();
                    break;
                case "KPISUBURB":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spOffice").show();
                    $("#spCompany").show();
                    $("#spSuburb").show();
                    $("#spActive").show();

                    $("#spDate").show();
                    break;
                case "KPIMUNICIPALITY":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spOffice").show();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#spActive").show();
                    break;
                case "KPIAGENT":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spUser").show();
                    $("#spDate").show();
                    $("#spActive").show();
                    break;
                case "MENTORBONUSSUMMARY":
                    $("#spPayPeriod").show();
                    break;
                case "MENTORBONUSDETAIL":
                    $("#spPayPeriod").show();
                    break;
                case "MONTHLYRETAINER":
                    $("#spPayPeriod").show();
                    $("#spCompany").show();
                    break;
                case "MONTHLYSALES":
                case "MONTHLYSALESDETAIL":
                    $("#lstFinancialYear option[value='']").remove();
                    if (!blnSingleUser) {
                        $("#spOffice").show();
                        $("#spCompany").show();
                        $("#spUser").show();
                        $("#spActive").show();
                    } else {
                        $("#spNonAdminUserFilter").show()
                    }
                    $("#spDate").show();
                    break;
                case "MONTHLYSALESBYPAYPERIOD":
                    $("#lstFinancialYear option[value='']").remove();
                    if (!blnSingleUser) {
                        $("#spOffice").show();
                        $("#spCompany").show();
                        $("#spUser").show();
                        $("#spActive").show();
                    }  else {
                        $("#spNonAdminUserFilter").show()
                    }
                    $("#spDate").show();
                    break;
                case "SALESLETTER":
                    $("#lstFinancialYear").attr('size', '1')
                        .removeAttr('size')
                        .removeAttr('multiple')
                        .val('-1');
                    $("#spOffice").show();
                    $("#spCompany").show();
                    $("#spUser").show();
                    $("#spFinYear").show();
                    break;

                case "MONTHLYSALESBYAGENT":
                    $("#lstFinancialYear option[value='']").remove();
                    if (!blnSingleUser) {
                        $("#spOffice").show();
                        $("#spCompany").show();
                        $("#spUser").show();
                        $("#spActive").show();
                        $("#spKPIFilter").show();
                    } else {
                        $("#spNonAdminUserFilter").show()
                    }
                    $("#spDate").show();
                    break;
                case "PREPAYMENT":
                    $("#spDate").show();
                    $("#spCompany").show();
                    break;
                case "PREPAYMENTN":
                    $("#spDate").show();
                    $("#spCompany").show();
                    break;

                case "PREPAYMENTCHART":
                    $("#spDate").show();
                    $("#spCompany").show();
                    break;
                case "SYSTEMQUARTERLY":
                    $("#spQuarter").show();
                    $("#spCompany").show();
                    break;
                case "AGENTEOMBALANCE":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spOffice").show();
                    $("#spPayPeriod").show();
                    $("#spCompany").show();
                    break;
                case "PAYROLLESTIMATE":
                    $("#spOffice").show();
                    $("#spUser").show();
                    $("#spCompany").show();
                    $("#spPayPeriod").show();

                    break;
                case "PAYROLL":
                    $("#spDate").show();
                    if (!blnSingleUser) {
                        $("#spUser").show();
                        $("#spCompany").show();
                    }
                    break;
                case "SECTION27":
                    $("#spDate").show();
                    $("#spCompany").show();
                    break;
                case "MISSINGSECTION27":
                    $("#spDate").show();
                    break;
                case "TOPPERFORMER":
                    $("#spRole").show();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#btnExport").show();
                    break;
                case "TOPADVERTISING":
                    $("#spRole").show();
                    $("#spCompany").show();
                    $("#spDate").show();

                    break;
                case "TOPADVERTISINGAVERAGE":
                    $("#spRole").show();
                    $("#spCompany").show();
                    $("#spDate").show();

                    break;
                case "TOPADVERTISINGDETAIL":
                    $("#spRole").show();
                    $("#spCompany").show();
                    $("#spDate").show();

                    break;

                case "NOOFSALES":
                    $("#lstFinancialYear").val('-1');
                    $("#lstFinancialYear").attr('multiple', 'multiple');
                    $("#lstFinancialYear").attr('size', '4');
                    $("#lstFinancialYear option[value='-1']").remove();
                    $("#spFinYear").show();
                    $("#spActive").show();
                    $("#spCompany").show();

                case "OFFTHETOP":
                    $("#spDate").show();
                    $("#spCompany").show();

                    break;
                case "SALESDOLLARS":
                    $("#lstFinancialYear").val('-1');
                    $("#lstFinancialYear").attr('multiple', 'multiple');
                    $("#lstFinancialYear").attr('size', '4');
                    $("#lstFinancialYear option[value='-1']").remove();
                    $("#spFinYear").show();
                    $("#spActive").show();
                    $("#spCompany").show();

                    break;
                case "YTDSALES":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#lstFinancialYear").val('-1');
                    $("#lstFinancialYear").removeAttr('multiple');
                    $("#lstFinancialYear").removeAttr('size');
                    $("#lstFinancialYear").attr('size', '1');
                    $("#lstFinancialYear option[value='-1']").remove();
                    $("#spFinYear").show();
                    $("#spActive").show();
                    $("#spCompany").show();

                    break;

            }
            $("#dButtons").show();
        }

        function hideAllFilters() {
            $("#btnExport, .Filter, #dButtons").hide();

            $("#lstOffice option:selected").removeAttr("selected");
            $("#lstCompany option:selected").removeAttr("selected");
            $("#lstUser option:selected").removeAttr("selected");
            $("#lstNonAdminUser option:selected").removeAttr("selected");
            
        }

        function getFilterValues() {
            var szParam = "?szPayPeriod=" + $("#lstPayPeriod").val();
            if ($("#lstOffice").val() != null)
                szParam += "&szOfficeID=" + escape($("#lstOffice").val());
            if ($("#lstCompany").val() != null)
                szParam += "&szCompanyID=" + escape($("#lstCompany").val());
            if ($("#lstSuburb").val() != null)
                szParam += "&szSuburbID=" + escape($("#lstSuburb").val());
            if ($("#lstFinancialYear").val() != null)
                szParam += "&szFinYear=" + escape($("#lstFinancialYear").val());
        
            szParam += "&szExpenseID=" + $("#lstExpense").val();
            szParam += "&szFletcherOrAgent=" + $("#lstFletcherOrAgent").val();
            szParam += "&szOffTheTopID=" + $("#lstOffTheTop").val();
            szParam += "&szRoleID=" + $("#lstRole").val();
            szParam += "&szFY=" + $("#lstFinancialYear").val();
            szParam += "&szQuarter=" + $("#lstQuarter").val();
            szParam += "&szStartDate=" + $("#txtStartDate").val();
            szParam += "&szEndDate=" + $("#txtEndDate").val();
            if ($("#lstIncludeInactive").val() == "YES")
                szParam += "&blnIncludeInactive=true";
            if ($("#lstApplyKPI").val() == "YES")
                szParam += "&blnApplyKPI=true";

            return szParam;
        }

        function changeDateFilter() {
            szVal = $("#lstDateRange").val();
            if (szVal != "") {
                arRange = szVal.split(':');
                $("#txtStartDate").val(arRange[0]);
                $("#txtEndDate").val(arRange[1]);
            }
        }

        function setDateCustom() {
            $("#lstDateRange").val('');
        }

        function getReport() {
            blnSingleUser = false;
            szReportVal = $("#lstReport").val();
            if (szReportVal == "-1") {
                szReportVal = $("#lstUserReport").val();
                blnSingleUser = true;
            }
            return szReportVal;
        }

        function runReport(TargetFrm, blnPrint) {
            szReport = getReport();
            switch (szReport) {
                case "AGENTOFFTHETOP":
                    var szSrc = "agent_off_the_top.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "AR_REPORT":
                    var szSrc = "AR_report.aspx";
                    var szParam = getFilterValues();
                    break;
                case "AR_REPORT":
                    var szSrc = "AR_report.aspx";
                    var szParam = getFilterValues();
                    break;
                case "SECTION27":
                    var szSrc = "proposed_s27_report.aspx";
                    var szParam = getFilterValues();
                    break;
                case "MISSINGSECTION27":
                    var szSrc = "section_27.aspx";
                    var szParam = getFilterValues();
                    break;
                case "TOPPERFORMER":
                    var szSrc = "top_performer.aspx";

                    var szParam = getFilterValues();
                    if (blnPrint == "EXPORT")
                        szParam += "blnPrint=EXPORT";
                    break;
                case "TOPADVERTISING":
                    var szSrc = "top_advertising.aspx";
                    var szParam = getFilterValues();
                    break;
                case "TOPADVERTISINGAVERAGE":
                    var szSrc = "top_advertising_average.aspx";
                    var szParam = getFilterValues();
                    break;
                case "TOPADVERTISINGDETAIL":
                    var szSrc = "top_advertising_detail.aspx";
                    var szParam = getFilterValues();
                    break;
                case "CAMPAIGNOUTSTANDING":
                    var szSrc = "campaign_outstanding.aspx";
                    var szParam = getFilterValues();
                    break;
                case "CAMPAIGNSAGING":
                    var szSrc = "campaign_aging.aspx";
                    var szParam = getFilterValues();
                    break;
                case "COMMISSION":
                case "COMMISSION_NEW":
                    var szSrc = "commission_statement.aspx";
                    if(szReport == "COMMISSION_NEW")
                        szSrc = "commission_statement_new.aspx";

                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);

                    
                    szParam += "&RecalcTotals=" + $("#lstRecreateTotals").val();
                    break;
                    var szSrc = "commission_statement.aspx";
                    var szParam = getFilterValues();

                    if ($("#lstUser").val() == null) {
                        if ($("#hfUserID").val() != "")
                            szParam += "&szUserID=" + $("#hfUserID").val(); //Get the hidden value as this is a single user view
                        else {
                            $("#lstUser *").attr("selected", "selected");
                            szParam += "&szUserID=" + $("#lstUser").val();
                        }
                    } else {
                        szParam += "&szUserID=" + $("#lstUser").val();
                    }
                    szParam += "&RecalcTotals=" + $("#lstRecreateTotals").val();
                    break;

                case "EOFYBONUSSUMMARY":
                    var szSrc = "eofy_bonus_summary.aspx";
                    var szParam = getFilterValues();
                    break;

                case "EOFYBONUSDETAIL":
                    var szSrc = "eofy_bonus_detail.aspx";
                    var szParam = getFilterValues();
                    break;

                case "EXPENSESUMMARY":
                    if (!enforceFinancialYear())
                        return false;
                    if (!enforceAccount())
                        return false;
                    var szSrc = "expense_summary.aspx";
                    var szParam = getFilterValues();
                    break;

                case "BUDGETREPORT":
                    var szSrc = "budget_report.aspx";
                    var szParam = getFilterValues();
                    break;

                case "INCENTIVE":
                    if (!enforceFinancialYear())
                        return false;
                    var szSrc = "incentive_report.aspx";
                    var szParam = getFilterValues();
                    break;
                case "INCENTIVECHART":
                    if (!enforceFinancialYear())
                        return false;
                    var szSrc = "incentive_report.aspx";
                    var szParam = getFilterValues();
                    szParam += "&blnShowChart=true";
                    break;
                case "MAINKPI":
                    var szSrc = "kpi.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "KPIOFFICE":
                    var szSrc = "kpi_office_agents.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "KPIOFFICENEW":
                    var szSrc = "kpi_office_agents_NEW.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "KPIOFFICEAUCTION":
                    var szSrc = "kpi_office_agents_auction_details.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "MENTORBONUSSUMMARY":
                    var szSrc = "mentor_bonus_summary.aspx";
                    var szParam = getFilterValues();
                    break;

                case "MENTORBONUSDETAIL":
                    var szSrc = "mentor_bonus_detail.aspx";
                    var szParam = getFilterValues();
                    break;
                case "MENTORCOMMISSION":
                    var szSrc = "mentoring_commission.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
            case "\COMMISSIONTOTAL":
                    var szSrc = "commission_total.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "FARMCOMMISSION":
                    var szSrc = "farming_commission.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;

                case "KPISUBURB":
                    var szSrc = "kpi_suburb.aspx";
                    var szParam = getFilterValues();
                    break;
                case "KPIMUNICIPALITY":
                    var szSrc = "kpi_suburb.aspx";
                    var szParam = getFilterValues() + "&blnGroupByMunicipality=true";
                    break;
                case "KPIAGENT":
                    var szSrc = "kpi_agent.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;

                case "MONTHLYRETAINER":
                    var szSrc = "monthly_retainer.aspx";
                    var szParam = getFilterValues();
                    break;
                case "MONTHLYSALES":
                    var szSrc = "monthly_sales.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "MONTHLYSALESDETAIL":
                    var szSrc = "monthly_sales_detail.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "MONTHLYSALESBYPAYPERIOD":
                    var szSrc = "monthly_sales_by_pay_period.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "MONTHLYSALESBYAGENT":
                    var szSrc = "monthly_sales_by_agent.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "SALESLETTER":
                     if (!enforceFinancialYear())
                        return false;
                    
                    var szSrc = "sales_letters.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(false);
                    break;
                case "AGENTEOMBALANCE":
                    var szSrc = "agent_eom_balance.aspx";
                    var szParam = getFilterValues();
                    break;
                case "PAYROLLESTIMATE":
                    var szSrc = "agent_payroll_estimate.aspx";
                    var szParam = getFilterValues();
                    szParam += getSelectedUser(true);
                    break;
                case "PAYROLL":
                    var szSrc = "agent_payroll_reconcilliation.aspx";
                    var szParam = getFilterValues();
                    if ($("#txtStartDate").val() == "") {
                        alert("You must select a start date");
                        return false;
                    }
                    szParam += getSelectedUser(true);
                    break;
                case "SYSTEMQUARTERLY":
                    if ($("#lstQuarter").val() == "") {
                        alert("Please select a quarter.");
                        return false;
                    }
                    var szSrc = "quarterly_top_performer.aspx";
                    var szParam = getFilterValues();

                    break;
                case "YTDSALES":
                    var szSrc = "YTD_sales.aspx";
                    var szParam = getFilterValues();

                    break;
                case "NOOFSALES":
                    var szSrc = "no_of_sales.aspx";
                    var szParam = getFilterValues();
                    szParam += "&szChtType=NOOFSALES";
                    break;
                case "OFFTHETOP":
                    var szSrc = "off_the_top.aspx";
                    var szParam = getFilterValues();
                    break;
                case "PREPAYMENT":
                    var szSrc = "campaign_prepayment.aspx";
                    var szParam = getFilterValues();
                    break;
                case "PREPAYMENTN":
                    var szSrc = "campaign_prepayment_n.aspx";
                    var szParam = getFilterValues();
                    break;

                case "PREPAYMENTCHART":
                    var szSrc = "campaign_prepayment_chart.aspx";
                    var szParam = getFilterValues();
                    break;

                case "SALESDOLLARS":
                    if (!enforceFinancialYear())
                        return false;

                    var szSrc = "no_of_sales.aspx";
                    var szParam = getFilterValues();
                    szParam += "&szChtType=SALESDOLLARS";
                    break;
                case "MISSINGSALES":

                    var szSrc = "missing_sales.aspx";
                    var szParam = getFilterValues();
                    szParam += "&szChtType=SALESDOLLARS";
                    break;
            }

            szParam += "&szOfficeNames=" + escape(getSelectText("lstOffice"));
            szParam += "&szCompanyNames=" + escape(getSelectText("lstCompany"));
            szParam += "&szExpenseNames=" + escape(getSelectText("lstExpense"));
            if (getReport() == "TOPPERFORMER" && TargetFrm == 'fPDF')
                szParam += "&blnPrint=EXPORT";
            else
                szParam += "&blnPrint=" + blnPrint;
            if ($("#chkNewWindow").is(":checked"))
                $("#frmMain").attr("target", "_blank").attr("action", szSrc + szParam).submit().attr("target", "");
            else
                $("#" + TargetFrm).attr("src", szSrc + szParam);
            return false;
        }

        function getSelectedUser(blnSelectAll) {
            if ($("#lstUser").val() == null) {
                if (blnSelectAll) {
                    $("#lstUser *").attr("selected", "selected");
                } else {
                    if ($("#lstNonAdminUser").is(":visible")) { //Check whether the ability to select between the PA and the sales agent is available
                        return "&szUserID=" + $("#lstNonAdminUser").val();
                    }

                    if ($("#hfUserID").val() != "")
                        return "&szUserID=" + $("#hfUserID").val(); //Get the hidden value as this is a single user view
                    else {
                        $("#lstUser *").attr("selected", "selected");
                        return "&szUserID=" + $("#lstUser").val();
                    }
                }   
            }
            return "&szUserID=" + $("#lstUser").val();
        }

        function printReport() {
            window.frames["fReport"].focus();
            if (getReport() == "TOPPERFORMER") {
                runReport("fReport", true);
            } else {
                window.frames["fReport"].print();
                runReport("fPDF", true);
            }
            return false;
        }

        function exportReport() {
            window.frames["fReport"].focus();
            runReport("fPDF", true);
            return false;
        }

        function enforceFinancialYear() {
            if ($("#lstFinancialYear option:selected").length == 0 || $("#lstFinancialYear").val() == -1) {
                alert('Please select a Financial year.');
                $("#lstFinancialYear").focus();
                return false;
            }
            return true;
        }
        function enforceAccount() {
            if ($("#lstExpense").val() == "-1") {
                alert('Please select an account.');
                $("#lstExpense").focus();
                return false;
            }
            return true;
        }

        function getSelectText(Control) {
            var foo = [];
            $('#' + Control + ' :selected').each(function (i, selected) {
                foo[i] = $(selected).text();
            });
            return foo.toString();
        }
    </script>
    <style>
        #dReportFilter {
            position: fixed !important;
            position: absolute;
            top: 120px;
            bottom: 10px;
            width: 20%;
            float: left;
            background: #E6D5AC;
            border: solid 1px #C49C42;
            padding: 5px;
            color: #6F5928;
            margin: 10px;
        }

        #fReport {
            position: fixed !important;
            position: absolute;
            top: 130px;
            bottom: 10px;
            left: 23%;
            float: left;
            width: 76%;
        }

        .Entry {
            width: 100%;
        }

        .RoundPanel {
            background-color: #BE9D52;
            padding: 5px;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="fReport">
        <asp:HiddenField ID="hfUserID" runat="server" />
        <div id="dReportFilter" class="DataPanel NoPrint" style="overflow: auto">
            <asp:Label ID="Label4" CssClass="FilterLabel" runat="server" Text="Report"></asp:Label>
            <select id="lstUserReport" class="Entry" runat="server" visible="false">
                <option value="-1">Select a report...</option>
                <option value="COMMISSION">Commission statement</option>
                <option value="MONTHLYSALES">Monthly sales</option>
                <option value="MONTHLYSALESBYAGENT">Monthly sales by agent</option>
                <option value="PAYROLL">Sales payroll reconcilliation</option>
            </select>
            <select id="lstReport" class="Entry">
                <option value="-1">Select a report...</option>
                <optgroup label='Campaign reports' />
                <option value="PREPAYMENT">Campaign prepayment</option>
                <option value="PREPAYMENTN">Campaign prepayment (N)</option>
                <option value="PREPAYMENTCHART">Campaign prepayment graph</option>
                <option value="CAMPAIGNOUTSTANDING">Outstanding invoice summary</option>
                <option value="CAMPAIGNSAGING">Outstanding invoices aged</option>
                <optgroup label='KPI reports' />
                <option value="MAINKPI">KPI</option>
                <option value="KPIAGENT">KPI agent detail</option>
                <option value="KPIMUNICIPALITY">KPI municipality detail</option>
                <option value="KPISUBURB">KPI suburb detail</option>
                <option value="KPIOFFICE">KPI Office agents BETA</option>
                <option value="KPIOFFICENEW">KPI Office agents NEW BETA</option>
                <option value="KPIOFFICEAUCTION">KPI Office agents Auction Details</option>
                <optgroup label='EOFY reports' />
                <option value="SALESLETTER">Sales letter</option>

                <optgroup label='Sales reports' />
                <option value="AGENTEOMBALANCE">Agents EOM Balances</option>
                <option value="AGENTOFFTHETOP">Agents off the top</option>
                <option value="PAYROLLESTIMATE">Agents payroll estimate</option>
                <option value="BUDGETREPORT">Budget report</option>
                <option value="COMMISSION">Commission statement</option>
                <option value="COMMISSION_NEW">Commission statement - NEW</option>
                <option value="COMMISSIONTOTAL">Commission - Totals</option>
                <option value="MENTORCOMMISSION">Commission - Mentoring</option>
                <option value="FARMCOMMISSION">Commission - Service Area</option>
                <option value="EXPENSESUMMARY">Expense summary</option>
                <option value="INCENTIVE">Incentive summary</option>
                <option value="INCENTIVECHART">Incentive chart</option>
                <option value="MONTHLYRETAINER">Monthly retainer</option>
                <option value="MONTHLYSALES">Monthly sales</option>
                <option value="MONTHLYSALESDETAIL">Monthly sales detail (graph totals)</option>
                <option value="MONTHLYSALESBYAGENT">Monthly sales by agent</option>
                <option value="MONTHLYSALESBYPAYPERIOD">Monthly sales by pay period</option>
                <option value="MISSINGSALES">Missing sales</option>
                <option value="NOOFSALES">No of sales per office</option>
                <option value="OFFTHETOP">Off the top monthly expenses</option>
                <option value="SYSTEMQUARTERLY">Quarterly top performer</option>
                <option value="SALESDOLLARS">Sales Dollars per office</option>
                <option value="TOPPERFORMER">Top performer</option>
                <option value="TOPADVERTISING">Top advertising</option>
                <option value="TOPADVERTISINGAVERAGE">Top advertising (average)</option>
                <option value="TOPADVERTISINGDETAIL">Top advertising (detail)</option>
                <option value="YTDSALES">Year to date branch sales</option>
                <optgroup label='Sales bonus schemes' />
                <option value="EOFYBONUSDETAIL">EOFY bonus detail</option>
                <option value="EOFYBONUSSUMMARY">EOFY bonus summary</option>
                <option value="MENTORBONUSDETAIL">Mentor bonus detail</option>
                <option value="MENTORBONUSSUMMARY">Mentor bonus summary</option>

                <optgroup label='Forecasting reports' />
                <option value="AR_REPORT">Proposed income</option>
                <option value="SECTION27">Section 27 proposed income</option>
                <option value="MISSINGSECTION27">Missing section 27</option>

                <optgroup label='Tax reports' />
                <option value="PAYROLL">Sales payroll reconcilliation</option>
            </select>

            <br class='Align' />
            <span id="spFinYear" class="Filter">
                <asp:Label ID="Label6" CssClass="FilterLabel" runat="server" Text="Financial year">
                </asp:Label>
                <asp:ListBox ID="lstFinancialYear" runat="server" CssClass="Entry"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spUser" runat="server" class="Filter">
                <asp:Label ID="Label2" CssClass="FilterLabel" runat="server" Text="User">
                </asp:Label>
                <asp:ListBox ID="lstUser" runat="server" CssClass="Entry" SelectionMode="Multiple"
                    Rows="10"></asp:ListBox>
                <br class='Align' />
            </span>
             <span id="spNonAdminUserFilter" runat="server" class="Filter" visible="false">
                <asp:Label ID="Label16" CssClass="FilterLabel" runat="server" Text="User">
                </asp:Label>
                 <asp:DropDownList ID="lstNonAdminUser" runat="server" CssClass="Entry"></asp:DropDownList>
                <br class='Align' />
            </span>
            <span id="spRecreate" runat="server" class="Filter">
                <asp:Label ID="Label12" CssClass="FilterLabel" runat="server" Text="Recreate totals">
                </asp:Label>
                <asp:DropDownList ID="lstRecreateTotals" runat="server" CssClass="Entry">
                    <asp:ListItem Text="No" Value="No" />
                    <asp:ListItem Text="Yes" Value="Yes" />
                </asp:DropDownList>
                <br class='Align' />
            </span>
            <span id="spOffice" class="Filter">
                <asp:Label ID="Label5" CssClass="FilterLabel" runat="server" Text="Office">
                </asp:Label>
                <asp:ListBox ID="lstOffice" runat="server" CssClass="Entry" SelectionMode="Multiple" Rows="8"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spCompany" class="Filter" runat="server">
                <asp:Label ID="Label8" CssClass="FilterLabel" runat="server" Text="Company">
                </asp:Label>
                <asp:ListBox ID="lstCompany" runat="server" CssClass="Entry" SelectionMode="Multiple"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spSuburb" class="Filter">
                <asp:Label ID="Label13" CssClass="FilterLabel" runat="server" Text="Suburb">
                </asp:Label>
                <asp:ListBox ID="lstSuburb" runat="server" CssClass="Entry" SelectionMode="Multiple"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spExpense" class="Filter">
                <asp:Label ID="Label10" CssClass="FilterLabel" runat="server" Text="Expense account">
                </asp:Label>
                <asp:ListBox ID="lstExpense" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spFletcherOrUserAmount" class="Filter">
                <asp:Label ID="Label14" CssClass="FilterLabel" runat="server" Text="Fletcher or agent amount">
                </asp:Label>
                <asp:ListBox ID="lstFletcherOrAgent" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1">
                    <asp:ListItem Text="FLETCHER">Fletcher</asp:ListItem>
                    <asp:ListItem Text="AGENT">Agent</asp:ListItem>
                </asp:ListBox>
                <br class='Align' />
            </span>

            <span id="spOffTheTop" class="Filter">
                <asp:Label ID="Label1" CssClass="FilterLabel" runat="server" Text="Off the top account">
                </asp:Label>
                <asp:ListBox ID="lstOffTheTop" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spQuarter" class="Filter">
                <asp:Label ID="Label9" CssClass="FilterLabel" runat="server" Text="Quarter">
                </asp:Label>
                <asp:ListBox ID="lstQuarter" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spPayPeriod" class="Filter">
                <asp:Label ID="Label3" CssClass="FilterLabel" runat="server" Text="Pay period">
                </asp:Label>
                <asp:DropDownList ID="lstPayPeriod" CssClass="Entry" runat="server">
                </asp:DropDownList>
                <br class='Align' />
            </span>

            <div id="spDate" class="Filter">
                <div class="FilterLabel">Date range</div>

                <asp:DropDownList ID="lstDateRange" runat="server" CssClass="Entry" Width="100%"></asp:DropDownList>
                <asp:TextBox ID="txtStartDate" runat="server" CssClass="Entry" Width="100"></asp:TextBox>
                &nbsp;to &nbsp;<asp:TextBox ID="txtEndDate" runat="server" CssClass="Entry" Width="100"></asp:TextBox>
            </div>

            <span id="spRole" class="Filter">
                <asp:Label ID="Label7" CssClass="FilterLabel" runat="server" Text="Role">
                </asp:Label>
                <asp:ListBox ID="lstRole" runat="server" CssClass="Entry" SelectionMode="single"
                    Rows="1"></asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spActive" class="Filter">
                <asp:Label ID="Label11" CssClass="FilterLabel" runat="server" Text="Include inactive people">
                </asp:Label>
                <asp:ListBox ID="lstIncludeInactive" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1">
                    <asp:ListItem Value="NO" Text="No"></asp:ListItem>
                    <asp:ListItem Value="YES" Text="Yes"></asp:ListItem>
                </asp:ListBox>
                <br class='Align' />
            </span>
            <span id="spKPIFilter" class="Filter">
                <asp:Label ID="Label15" CssClass="FilterLabel" runat="server" Text="Apply KPI filter">
                </asp:Label>
                <asp:ListBox ID="lstApplyKPI" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1">
                    <asp:ListItem Value="NO" Text="No"></asp:ListItem>
                    <asp:ListItem Value="YES" Text="Yes"></asp:ListItem>
                </asp:ListBox>
                <br class='Align' />
            </span>
            <div id='dButtons' style='display: none; float: left; text-align: center; margin-top: 10px; margin-left: 20px'>
                <asp:Button ID="btnRunReport" runat="server" Width="100px" CssClass="Button EntryPos btn" Text="Run report" OnClientClick="return runReport('fReport', false);" UseSubmitBehavior="false" />
                <asp:Button ID="btnPrint" runat="server" Width="100px" CssClass="Button EntryPos btn" Text="Print" OnClientClick="return printReport();" UseSubmitBehavior="false" Style="margin-left: 10px" />
                <asp:Button ID="btnExport" runat="server" Width="100px" CssClass="Button EntryPos btn" Text="Export" OnClientClick="return exportReport();" />

                <asp:CheckBox ID="chkNewWindow" runat="server" Text="Open in new window &nbsp;" TextAlign="Left" />
            </div>
        </div>
        <iframe id='fReport' name="fReport" style="overflow: auto; border: 0px;" src="../blank.html" frameborder="0"></iframe>
        <iframe id='fPDF' name="fPDF" style="overflow: auto; border: 0px; height: 0px; width: 1000px; top: 800px" src="about:blank" frameborder="0"></iframe>
    </form>
</body>
</html>