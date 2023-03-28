<%@ Page Language="c#" Inherits="Paymaker.report_admin" CodeFile="report_admin.aspx.cs" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Report admin</title>
    <script src="https://kit.fontawesome.com/bd4008270a.js" crossorigin="anonymous"></script>
    <script type="text/javascript">
        blnSingleUser = false;
        arReports = new Array();
        
        $(document).ready(function () {
            createCalendar("txtStartDate");
            createCalendar("txtEndDate");
         
            $(".DataPanel").corner();
            $("#dFilterToggle").click(function () {
                toggleFilter();
                $("#dFilterToggle").toggle();
                $("#frmReport").hide();
            });

           
            setupFavourites();
            $('.MyReports:not(.FavouriteReport)').one("click", function () {
                addAsFavourite($(this));
            })

            $('.FavouriteReport').on("click", function () {
                removeAsFavourite($(this));
            })
        });

        function addAsFavourite(e) {
            $(e).addClass("FavouriteReport");
            Report = $(e).attr('data-report')
            callWebMethod("../web_services/ws_Paymaker.asmx", "saveFavouriteReport", ["Report", Report], null);
            arReports.push(Report);
            e.one("click", function () {
                removeAsFavourite($(this));
            })
        }

        function removeAsFavourite(e) {
            $(e).removeClass("FavouriteReport");
            Report = $(e).attr('data-report')
            callWebMethod("../web_services/ws_Paymaker.asmx", "removeFavouriteReport", ["Report", Report], null);
            arReports.push(Report);
            e.one("click", function () {
                addAsFavourite($(this));
            })
        }
        
        function setupFavourites() {
            
            if ($("#hdFavouriteReports").val() != "") {
                arReports = $("#hdFavouriteReports").val().split(',');
            }

            blnHasFavourites = false;
            if (arReports.length > 0) {
                blnHasFavourites = true;
                $(".MyReports").each(function () {
                    Report = $(this).attr('data-report');
                    if (arReports.indexOf(Report) > -1) {
                        //This is a favourited report
                        $(this).addClass("FavouriteReport");
                    }
                })
                console.log('Hiding reports that are not favourited');
                $(".MyReports").prop("title", "Click to add to favourite reports");
                $(".FavouriteReport").show().prop("title", "Click to remove from your favourite reports");
            }
            showReports(arReports.length == 0);
        }

        function toggleFilter() {
            console.log('toggleFilter');
            $("#dReportFilter").animate({ width: 'toggle' }, 500);
            $("#pReports").toggle();
        }

        function toggleReportSelection() {
            console.log('toggleReportSelection');
            $("#fReport").hide();
            toggleFilter();
        }
        
        function selectReport(Report) {
            $("#hdReport").val(Report);
            toggleFilter();
            showFilter();
        }


        function showReport(Report) {
            showFilter();
        }

        function showFilter() {
            hideAllFilters();
            szReportVal = getReport();
            console.log("Showing filters for: " + szReportVal)
            if (szReportVal == "-1")
                return;
            switch (szReportVal) {
                case "AR_REPORT":
                    $("#spDate").show();
                    $("#spCompany").show();
                    break;
                case "CASHFLOW":
                case "CASHFLOWPREDICTION":
                    $("#spDate").show();
                    $("#spOffice").show();
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
                    $("#spReferral").show();
                    $("#spFinYear").show();
                    break;
                case "MAINKPI":
                    $("#lstFinancialYear option[value='']").remove();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#spActive").show();

                    break;

                case "KPICARD":
                    $("#spDate").hide();
                    $("#spOffice").show();
                    $("#spUser").show();
                    //$("#spMonth").show();
                    $("#spQuarter").show();
                    $("#spActive").show();
                    break;

                case "KPIOFFICE":
                case "KPIOFFICENEW":
                case "KPIOFFICEAUCTION":
                    $("#lstFinancialYear option[value='']").remove();
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
                case "MONTHLYSALESAGENTEXPENSES":
                case "NATIONALSALESAWARD":
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
                    if (szReportVal == "MONTHLYSALESDETAIL") {
                        $("#spReferral").show();
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
                    $("#spReferral").show();
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
                    $("#spNumberOfPeople").show();
                    $("#spCompany").show();
                    $("#spDate").show();
                    $("#spReferral").show();
                    $("#btnExport").show();
                    break;
                case "USERTOPPERFORMER":
                    $("#spPayPeriod").show();
                    break;
                case "USERTOPPERFORMERMONTHLY":
                    $("#spDate").show();
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
            szParam += "&szNumberOfPeople=" + $("#lstNumberOfPeople").val();
            szParam += "&szFY=" + $("#lstFinancialYear").val();
            szParam += "&szQuarter=" + $("#lstQuarter").val();
            szParam += "&szMonth=" + $("#lstMonth").val();
            szParam += "&szStartDate=" + $("#txtStartDate").val();
            szParam += "&szEndDate=" + $("#txtEndDate").val();
            if ($("#lstIncludeInactive").val() == "YES")
                szParam += "&blnIncludeInactive=true";
            if ($("#lstApplyKPI").val() == "YES")
                szParam += "&blnApplyKPI=true";
            if ($("#lstExcludeReferral").val() == "YES")
                szParam += "&blnExcludeReferral=true";

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
            szReportVal = $("#hdReport").val();
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
                 case "CASHFLOW":
                    var szSrc = "cash_flow.aspx";
                    var szParam = getFilterValues();
                    break;
                 case "CASHFLOWPREDICTION":
                    var szSrc = "cash_flow_prediction.aspx";
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
                case "USERTOPPERFORMER":
                    var szSrc = "user_top_performer.aspx";

                    var szParam = getFilterValues();
                    if (blnPrint == "EXPORT")
                        szParam += "blnPrint=EXPORT";
                    break;
                case "USERTOPPERFORMERMONTHLY":
                    var szSrc = "user_top_performer_monthly.aspx";

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
                case "KPICARD":
                    var szSrc = "kpi_agent_card.aspx";
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
                case "COMMISSIONTOTAL":
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
                case "MONTHLYSALESAGENTEXPENSES":
                    var szSrc = "monthly_sales_agent_expenses.aspx";
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
                case "NATIONALSALESAWARD":
                    var szSrc = "national_sales_award.aspx";
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
            console.log(szSrc + szParam)
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

        function setupForSingleUser() {
            blnSingleUser = true;
            $(".AdminReports").hide();
            $("#dUserReports").show();
        }

        function showReports(ShowAll) {
            console.log(ShowAll);
            if (ShowAll) {
                $(".MyReports").each(function () {
                    $(this).parent().show();  
                })
            } else {
                $(".MyReports").each(function () {
                    $(this).parent().hide();
                })
                $(".FavouriteReport").each(function () {
                    $(this).parent().show();
                })
            }
        }

    </script>
    <style>
          #dReportFilter {
            bottom: 0%;
            float: left;
            height: 85vh;
        }

        #dFilterToggle {
            position: fixed !important;
            top: 40%;
            left: 0px;
            display: none;
            cursor: pointer;
        }

         #fReport {
            right: 0;
            bottom: 0;
            width: 100%;
            display: none;
            height: 90vh;
        }

        .Entry {
            width: 100%;
        }

        .RoundPanel {
            background-color: #BE9D52;
            padding: 5px;
            width: 100%
        }

          #dFilterToggle {
            position: fixed !important;
            top: 40%;
            left: 0px;
            display: none;
            cursor: pointer;
        }

        .ReportCol {
            width: 25%;
            float: left;
        }

        .ReportColHeader {
            font-size: 1.2em;
            font-weight: 600;
        }

        .ReportLink {
            padding: 6px;
        }

        .MyReports {
            cursor: pointer;
            margin-right: 10px;
            color: silver;
        }

         .MyReports:hover {
            color: black;
        }

        .FavouriteReport {
            background: yellow;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="fReport">
        <asp:HiddenField ID="hfUserID" runat="server" />
         <asp:HiddenField ID="hdReport" runat="server" />
        <asp:HiddenField ID="hdFavouriteReports" runat="server" />
        <img id="dFilterToggle" src="../sys_images/show_filter.gif" title="Show the report filters" />
        <div class="container-fluid">
            <div class="row" ID="pReports" runat="server" style="margin-top: 10px">
                <div id="dHidden" style="display:none">
                    <div class="ReportColHeader">KPI</div>
                        <div class='ReportLink' ><i class="far fa-user MyReports" data-report="MAINKPI"></i><a href='javascript: selectReport("MAINKPI")'>KPI</a></div>
                        <div class='ReportLink'><a href='javascript: selectReport("KPIAGENT")'>KPI agent detail</a></div>
                        <div class='ReportLink'><a href='javascript: selectReport("KPIMUNICIPALITY")'>KPI municipality detail</a></div>
                        <div class='ReportLink'><a href='javascript: selectReport("KPISUBURB")'>KPI suburb detail</a></div>
                        <div class='ReportLink'><a href='javascript: selectReport("KPIOFFICE")'>KPI Office agents</a></div>
                        <div class='ReportLink'><a href='javascript: selectReport("KPICARD")'>KPI agent card</a></div>
                        <div class='ReportLink'><a href='javascript: selectReport("KPIOFFICEAUCTION")'>KPI Office agents auction details</a></div>
                </div>
                <div id="dUserReports" style="display:none">
                    <div class="ReportColHeader">Reports</div>
                        <div class='ReportLink'><i class="far fa-star" data-report="COMMISSION"></i><a href='javascript: selectReport("COMMISSION")'>Commission statement</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALES"></i><a href='javascript: selectReport("MONTHLYSALES")'>Monthly sales</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALESBYAGENT"></i><a href='javascript: selectReport("MONTHLYSALESBYAGENT")'>Monthly sales by agent</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALESDETAIL"></i><a href='javascript: selectReport("MONTHLYSALESDETAIL")'>Monthly sales detail (graph totals)</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="USERTOPPERFORMER"></i><a href='javascript: selectReport("USERTOPPERFORMER")'>Top performer</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="USERTOPPERFORMERMONTHLY"></i><a href='javascript: selectReport("USERTOPPERFORMERMONTHLY")'>Top performer (monthly)</a></div>
                </div>
                <div id="pCampaign" class="col-sm-2 AdminReports">
                    <div class="ReportColHeader">Campaign reports</div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="PREPAYMENT"></i><a href='javascript: selectReport("PREPAYMENT")'>Campaign prepayment</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="PREPAYMENTN"></i><a href='javascript: selectReport("PREPAYMENTN")'>Campaign prepayment (N)</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="PREPAYMENTCHART"></i><a href='javascript: selectReport("PREPAYMENTCHART")'>Campaign prepayment graph</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="CAMPAIGNOUTSTANDING"></i><a href='javascript: selectReport("CAMPAIGNOUTSTANDING")'>Outstanding invoice summary</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="CAMPAIGNSAGING"></i><a href='javascript: selectReport("CAMPAIGNSAGING")'>Outstanding invoices aged</a></div>
                </div>
                     
                <div id="pPayment" class="col-sm-3 AdminReports">
                    <div class="ReportColHeader">Sales Peports</div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="AGENTEOMBALANCE"></i><a href='javascript: selectReport("AGENTEOMBALANCE")'>Agents EOM Balances</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="AGENTOFFTHETOP"></i><a href='javascript: selectReport("AGENTOFFTHETOP")'>Agents off the top</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="PAYROLLESTIMATE"></i><a href='javascript: selectReport("PAYROLLESTIMATE")'>Agents payroll estimate</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="BUDGETREPORT"></i><a href='javascript: selectReport("BUDGETREPORT")'>Budget report</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="COMMISSION"></i><a href='javascript: selectReport("COMMISSION")'>Commission statement</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="COMMISSIONTOTAL"></i><a href='javascript: selectReport("COMMISSIONTOTAL")'>Commission - Totals</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MENTORCOMMISSION"></i><a href='javascript: selectReport("MENTORCOMMISSION")'>Commission - Mentoring</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="FARMCOMMISSION"></i><a href='javascript: selectReport("FARMCOMMISSION")'>Commission - Service Area</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="EXPENSESUMMARY"></i><a href='javascript: selectReport("EXPENSESUMMARY")'>Expense summary</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="INCENTIVE"></i><a href='javascript: selectReport("INCENTIVE")'>Incentive summary</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYRETAINER"></i><a href='javascript: selectReport("MONTHLYRETAINER")'>Monthly retainer</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALES"></i><a href='javascript: selectReport("MONTHLYSALES")'>Monthly sales</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALESAGENTEXPENSES"></i><a href='javascript: selectReport("MONTHLYSALESAGENTEXPENSES")'>Monthly sales - agent expenses</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALESDETAIL"></i><a href='javascript: selectReport("MONTHLYSALESDETAIL")'>Monthly sales detail (graph totals)</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALESBYAGENT"></i><a href='javascript: selectReport("MONTHLYSALESBYAGENT")'>Monthly sales by agent</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MONTHLYSALESBYPAYPERIOD"></i><a href='javascript: selectReport("MONTHLYSALESBYPAYPERIOD")'>Monthly sales by pay period</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="MISSINGSALES"></i><a href='javascript: selectReport("MISSINGSALES")'>Missing sales</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="NATIONALSALESAWARD"></i><a href='javascript: selectReport("NATIONALSALESAWARD")'>National sales award</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="OFFTHETOP"></i><a href='javascript: selectReport("OFFTHETOP")'>Off the top monthly expenses</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="TOPADVERTISINGDETAIL"></i><a href='javascript: selectReport("TOPADVERTISINGDETAIL")'>Top advertising (detail)</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="YTDSALES"></i><a href='javascript: selectReport("YTDSALES")'>Year to date branch sales</a></div>
                </div>
                <div id="pGraph" class="col-sm-2 AdminReports">
                    <div class="ReportColHeader">Graphing reports</div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="INCENTIVECHART"></i><a href='javascript: selectReport("INCENTIVECHART")'>Incentive chart</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="NOOFSALES"></i><a href='javascript: selectReport("NOOFSALES")'>No of sales per office</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="SYSTEMQUARTERLY"></i><a href='javascript: selectReport("SYSTEMQUARTERLY")'>Quarterly top performer</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="SALESDOLLARS"></i><a href='javascript: selectReport("SALESDOLLARS")'>Sales Dollars per office</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="TOPPERFORMER"></i><a href='javascript: selectReport("TOPPERFORMER")'>Top performer</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="TOPADVERTISING"></i><a href='javascript: selectReport("TOPADVERTISING")'>Top advertising</a></div>
                        <div class='ReportLink'><i class="far fa-star MyReports" data-report="TOPADVERTISINGAVERAGE"></i><a href='javascript: selectReport("TOPADVERTISINGAVERAGE")'>Top advertising (average)</a></div>

                </div>
                <div id="pOther" class="col-sm-2 AdminReports">
                    <div class="ReportColHeader">Other Reports</div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="SALESLETTER"></i><a href='javascript: selectReport("SALESLETTER")'>Sales letter</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="EOFYBONUSDETAIL"></i><a href='javascript: selectReport("EOFYBONUSDETAIL")'>EOFY bonus detail</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="EOFYBONUSSUMMARY"></i><a href='javascript: selectReport("EOFYBONUSSUMMARY")'>EOFY bonus summary</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="MENTORBONUSDETAIL"></i><a href='javascript: selectReport("MENTORBONUSDETAIL")'>Mentor bonus detail</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="MENTORBONUSSUMMARY"></i><a href='javascript: selectReport("MENTORBONUSSUMMARY")'>Mentor bonus summary</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="PAYROLL"></i><a href='javascript: selectReport("PAYROLL")'>Sales payroll reconcilliation</a></div>
                </div>
                     
                <div id="pAdmin" class="col-sm-3 AdminReports" style="height: 75vh">
                    <div class="ReportColHeader">Forecasting reports</div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="CASHFLOW"></i><a href='javascript: selectReport("CASHFLOW")'>Sales statistics</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="CASHFLOWPREDICTION"></i><a href='javascript: selectReport("CASHFLOWPREDICTION")'>Cash flow prediction</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="AR_REPORT"></i><a href='javascript: selectReport("AR_REPORT")'>Proposed income</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="SECTION27"></i><a href='javascript: selectReport("SECTION27")'>Section 27 proposed income</a></div>
                    <div class='ReportLink'><i class="far fa-star MyReports" data-report="MISSINGSECTION27"></i><a href='javascript: selectReport("MISSINGSECTION27")'>Missing section 27</a></div>
                </div>
                <div class="col-sm-3">
                </div>
                <div class="col-sm-3">
                    <button type='button' class="btn btn-block" id='ShowAll' onclick="showReports(true)">Show all reports</button>
                </div>
                <div class="col-sm-3">
                    <button type='button' id='ShowMyReports' class="btn btn-block" onclick="showReports(false)">Show my favourites</button>
                </div>
            </div>
            <div class="row">

                <div class="col-sm-3 col-md-3 col-l-3 ">
                    
                    <asp:Panel ID="dReportFilter" runat="server" CssClass="RoundPanel" style="display:none" >
                    <div id='dSelReportName' class="FilterHeader" style="width: 100%; float: left; clear: left">
                    </div>
                    <button type="button" class="btn btn-block btn-primary" onclick="toggleReportSelection()">Change report</button>
                            
                    <div class="DataPanel NoPrint" style="overflow: auto;">
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
                                <span id="spMonth" class="Filter">
                                <asp:Label ID="Label17" CssClass="FilterLabel" runat="server" Text="Month">
                                </asp:Label>
                                <asp:ListBox ID="lstMonth" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1"></asp:ListBox>
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
                            <span id="spNumberOfPeople" class="Filter">
                                <asp:Label ID="Label19" CssClass="FilterLabel" runat="server" Text="Show Top # of People">
                                </asp:Label>
                                <asp:ListBox ID="lstNumberOfPeople" runat="server" CssClass="Entry" SelectionMode="single" Rows="1">
                                    <asp:ListItem Value="5">5</asp:ListItem>
                                    <asp:ListItem Value="7">7</asp:ListItem>
                                    <asp:ListItem Value="5000">All</asp:ListItem>
                                </asp:ListBox>
                                <br class='Align' />
                            </span>
                            <span id="spRole" class="Filter">
                                <asp:Label ID="Label7" CssClass="FilterLabel" runat="server" Text="Role">
                                </asp:Label>
                                <asp:ListBox ID="lstRole" runat="server" CssClass="Entry" SelectionMode="single"
                                    Rows="1"></asp:ListBox>
                                <br class='Align' />
                            </span>
                           <span id="spReferral" class="Filter">
                                <asp:Label ID="Label18" CssClass="FilterLabel" runat="server" Text="Exclude referral expenses">
                                </asp:Label>
                                <asp:ListBox ID="lstExcludeReferral" runat="server" CssClass="Entry" SelectionMode="Single" Rows="1">
                                    <asp:ListItem Value="NO" Text="No"></asp:ListItem>
                                    <asp:ListItem Value="YES" Text="Yes"></asp:ListItem>
                                </asp:ListBox>
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
                                <asp:Button ID="btnRunReport" runat="server" Width="100px" CssClass="Button EntryPos btn" Text="Run report" OnClientClick="$('#fReport').show(); return runReport('fReport', false);" UseSubmitBehavior="false" />
                                <asp:Button ID="btnPrint" runat="server" Width="100px" CssClass="Button EntryPos btn" Text="Print" OnClientClick="return printReport();" UseSubmitBehavior="false" Style="margin-left: 10px" />
                                <asp:Button ID="btnExport" runat="server" Width="100px" CssClass="Button EntryPos btn" Text="Export" OnClientClick="return exportReport();" />
                                <br />
                                <asp:CheckBox ID="chkNewWindow" runat="server" Text="Open in new window &nbsp;" TextAlign="Left" />
                            </div>
                                    
                        </div>
                    </asp:Panel>
                </div>
                
                <div class="col-sm-9 col-md-9 col-l-9">
                    <iframe id='fReport' name='fReport' src='about:blank' frameborder="0" style="height: 85vh; width: 97%; float: right; display: none"></iframe>
                </div>
            </div>
        </div>
        <iframe id='fPDF' name="fPDF" style="overflow: auto; border: 0px; height: 0px; width: 1000px; top: 800px" src="about:blank" frameborder="0"></iframe>
    </form>
</body>
</html>