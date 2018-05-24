<%@ Page Language="c#" Inherits="sale_update" CodeFile="sale_update.aspx.cs" ValidateRequest="false"
    EnableViewStateMac="false" EnableViewState="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Sale update</title>
    <script type="text/javascript">

        function updateCalculations() {
            calcOffTheTopExpenses();
            updateSplitAmount();
            runSplitUpdate();
            calcUserSaleSplitCommissionTypeTotal();
            calcCommissionTotal();
            validForSave();
        }
        //blnPassedAsID is true when we have passed thisID as an ID for the obj and not the obj itself
        function saleSplitAmountChange(thisID, blnPassedAsID) {
            //get the id of the Amount text field
            var $thisID = thisID.id;
            if (blnPassedAsID) {
                $thisID = thisID;
            }

            //get the id of the amount type
            $thisAmountTypeID = $thisID.replace("txtAmount_", "lstAmountType_");
            if ($("#" + $thisAmountTypeID).val() == "1") { //%
                if ($("#" + $thisID).hasClass('JQSaleSplitAmount')) {
                    var calcValue = getPercentage(getNetCommission(), $("#" + $thisID).val());
                    $("#" + $thisID.replace("txtAmount_", "txtCalculatedAmount_")).text(calcValue);
                }
                if ($("#" + $thisID).hasClass('JQUserSaleSplitAmount')) {
                    var $AmountToSplit = $("#" + $thisID.split("_USS_")[0].replace("txtAmount_", "txtCalculatedAmount_")).text();
                    var calcValue = getPercentage($AmountToSplit, $("#" + $thisID).val());
                    $("#" + $thisID.replace("txtAmount_", "txtCalculatedAmount_")).text(calcValue);
                }
                if ($("#" + $thisID).hasClass('JQSaleExpenseAmount')) {
                    var $currID = $thisID.replace("txtAmount_", "txtCalculatedAmount_");
                    var fOffTheTopExpenses = 0;
                    $(".JQExpenseSplit").each(function () {
                        if ($("#" + $thisID).attr("id") != $currID)//we don't want to take the current value in consideration
                            fOffTheTopExpenses += parseFloat($("#" + $thisID).val());
                    });
                    var calcValue = (parseFloat($("#hdGrossCommission").val()) - parseFloat(fOffTheTopExpenses)) * parseFloat($("#" + $thisID).val()) / 100;
                    $("#" + $thisID.replace("txtAmount_", "txtCalculatedAmount_")).text(parseFloat(calcValue).toFixed(2));

                }
            } else {
                $("#" + $thisID.replace("txtAmount_", "txtCalculatedAmount_")).text(parseFloat($("#" + $thisID).val()).toFixed(2));
            }
            updateCalculations();
            return true;
        }

        function amountTypeChange(thisID) {
            //get the id of the amount type
            var $thisID = thisID.id;
            var $thisAmountID = $thisID.replace("lstAmountType_", "txtAmount_");
            $("#" + $thisAmountID).removeClass("numbersOnly").removeClass("percent").unbind("keypress");

            if ($("#" + $thisID).val() == "1") {//"%" is selected
                $("#" + $thisAmountID).addClass("percent").unbind("keypress");
                addValidation();
                if ($("#" + $thisID).hasClass('JQSaleSplitAmount')) {
                    var calcValue = getPercentage(getNetCommission(), $("#" + $thisID).val());
                    $("#" + $thisID.replace("txtAmount_", "txtCalculatedAmount_")).text(calcValue);
                }
                if ($("#" + $thisID).hasClass('JQUserSaleSplitAmount')) {
                    var $AmountToSplit = $("#" + $thisID.split("_USS_")[0].replace("txtAmount_", "txtCalculatedAmount_")).text();
                    var calcValue = getPercentage($AmountToSplit, $("#" + $thisID).val());
                    $("#" + $thisID.replace("txtAmount_", "txtCalculatedAmount_")).text(calcValue);
                }
                if ($("#" + $thisID).hasClass('JQSaleExpenseAmountType')) {
                    var $currID = $thisID.replace("txtAmount_", "txtCalculatedAmount_");
                    var fOffTheTopExpenses = 0;
                    $(".JQExpenseSplit").each(function () {
                        if ($("#" + $thisID).attr("id") != $currID)//we don't want to take the current value in consideration
                            fOffTheTopExpenses += parseFloat($("#" + $thisID).text());
                    });
                    var calcValue = (parseFloat($("#hdGrossCommission").val()) - parseFloat(fOffTheTopExpenses)) * parseFloat($("#" + $thisAmountID).val()) / 100;
                    $("#" + $thisID.replace("lstAmountType_", "txtCalculatedAmount_")).text(parseFloat(calcValue).toFixed(2));
                }
            } else {
                $("#" + $thisAmountID).addClass("numbersOnly").removeClass("percent").unbind("keypress");
                addValidation();
                $("#" + $thisID.replace("lstAmountType_", "txtCalculatedAmount_")).text(parseFloat($("#" + $thisAmountID).val()).toFixed(2));
            }
            updateCalculations();
        }

        //Calculates the total of Sale Split & User Sale Split
        function calcCommissionTotal() {
            var fCommissionTotal = 0; //Commission Sale Split
            var fUserCommissionTotal = 0;
            var fOffTheTopExpenseTotal = 0;
            $(".JQSaleSplit").each(function () {
                fCommissionTotal += parseFloat($(this).text());
            });
            if ($("hdDebugMode").val() == "true") {
                alert(fCommissionTotal);
            }
            $(".JQUserSaleSplit").each(function () {
                if ($(this).text() != "" && !isNaN($(this).text())) {
                    fUserCommissionTotal += parseFloat($(this).text());
                }
            });
            if ($("hdDebugMode").val() == "true") {
                alert(fUserCommissionTotal);
            }

            $(".JQExpenseSplit").each(function () {
                fOffTheTopExpenseTotal += parseFloat($(this).text());
            });
            if ($("hdDebugMode").val() == "true") {
                alert(fOffTheTopExpenseTotal);
            }

            $("#dCommissionsTotal").text(fCommissionTotal.toFixed(2));
            $("#dUserCommissionsTotal").text(fUserCommissionTotal.toFixed(2));
            $("#dOffTheTopTotal").text(fOffTheTopExpenseTotal.toFixed(2));

            var fGrossCommissionTotal = parseFloat($("#dGrossCommission").text());
            //Commission not allocated
            var fDifferenceSpiltAmountAndCommissionTotal = fGrossCommissionTotal - fOffTheTopExpenseTotal - fCommissionTotal;
            $("#hdDifferenceSpiltAmountAndCommissionTotal").val(fDifferenceSpiltAmountAndCommissionTotal.toFixed(2));
            $("#dDifferenceSpiltAmountAndCommissionTotal").text('');
            if (parseFloat($("#hdDifferenceSpiltAmountAndCommissionTotal").val()) != 0) {
                $("#dDifferenceSpiltAmountAndCommissionTotal").text("(" + fDifferenceSpiltAmountAndCommissionTotal.toFixed(2) + ")");
            }

            var fDifferenceUserSpiltAmountAndCommissionTotal = fGrossCommissionTotal - fOffTheTopExpenseTotal - fUserCommissionTotal;
            $("#hdDifferenceUserSpiltAmountAndCommissionTotal").val(fDifferenceUserSpiltAmountAndCommissionTotal.toFixed(2));
            $("#dDifferenceUserSpiltAmountAndCommissionTotal").text('');
            if (parseFloat($("#hdDifferenceUserSpiltAmountAndCommissionTotal").val()) != 0) {
                $("#dDifferenceUserSpiltAmountAndCommissionTotal").text("(" + fDifferenceUserSpiltAmountAndCommissionTotal.toFixed(2) + ")");
            }
            $("#hdSplitAmount").val(fGrossCommissionTotal - fOffTheTopExpenseTotal);

            var fDifferenceUserSpiltAmountAndCommissionTotal = parseFloat($("#hdDifferenceUserSpiltAmountAndCommissionTotal").val());
            if (Math.abs(fDifferenceUserSpiltAmountAndCommissionTotal) > 1.00) {
                $("#btnUpdate").attr("disabled", true);
                //   $("#btnUpdate").attr("title", "User commission total does not match with Commissions total");
            }

            var fDifferenceSpiltAmountAndCommissionTotal = parseFloat($("#hdDifferenceSpiltAmountAndCommissionTotal").val());
            if (Math.abs(fDifferenceSpiltAmountAndCommissionTotal) < 1.00) {
                //alert(fDifferenceSpiltAmountAndCommissionTotal);
                adjustFletchersAmount();
            }
        }

        function adjustFletchersAmount() {
            //return;
            var fDifferenceSpiltAmountAndCommissionTotal = parseFloat($("#hdDifferenceSpiltAmountAndCommissionTotal").val());

            if (Math.abs(fDifferenceSpiltAmountAndCommissionTotal) < 1.00 && Math.abs(fDifferenceSpiltAmountAndCommissionTotal) > 0.00) {
                //if (confirm("Fletchers sales split will be adjusted by $" + parseFloat(fDifferenceSpiltAmountAndCommissionTotal).toFixed(2))) {
                var fFletchersSaleSplit = parseFloat($(".JQFletchersSaleSplit").text());
                if (fDifferenceSpiltAmountAndCommissionTotal > 0) {
                    var fAdjustedFletchersSaleSplit = fFletchersSaleSplit - fDifferenceSpiltAmountAndCommissionTotal;
                    $("#dDifferenceUserSpiltAmountAndCommissionTotal").text('');
                    $("#dDifferenceSpiltAmountAndCommissionTotal").text('');
                }
                else {
                    fAdjustedFletchersSaleSplit = fFletchersSaleSplit + fDifferenceSpiltAmountAndCommissionTotal;
                    $("#dDifferenceUserSpiltAmountAndCommissionTotal").text('');
                    $("#dDifferenceSpiltAmountAndCommissionTotal").text('');

                }
                $(".JQFletchersSaleSplit").text(parseFloat(fAdjustedFletchersSaleSplit).toFixed(2));
                $(".JQFletchersUserSaleSplit").text(parseFloat(fAdjustedFletchersSaleSplit).toFixed(2));
                //}
            }
        }

        /*
        Calculates the sub total of the User Sales splits
        */
        function calcUserSaleSplitCommissionTypeTotal() {
            var currUserSaleSplitSumCommissionType = -100;
            var fUserSaleSplitCommissionTypeTotal = 0;
            $(".UserSaleSplitSum").each(function () {
                if (parseInt(currUserSaleSplitSumCommissionType) == -100 || parseInt(currUserSaleSplitSumCommissionType) != parseInt($(this).attr("id").replace("UserSplit_", ""))) {
                    currUserSaleSplitSumCommissionType = parseInt($(this).attr("id").replace("UserSplit_", ""));
                    fUserSaleSplitCommissionTypeTotal = 0;
                    $("[id*=txtCalculatedAmount_CT_" + currUserSaleSplitSumCommissionType + "_SS_]").each(function () {
                        if ($(this).hasClass('JQUserSaleSplit')) {
                            fUserSaleSplitCommissionTypeTotal += parseFloat($(this).text());
                        }
                    });
                    if (!isNaN(fUserSaleSplitCommissionTypeTotal))
                        $(this).text(fUserSaleSplitCommissionTypeTotal.toFixed(2));
                }
            });
        }

        //User split total row to be shown when we have two or more users in the split
        function showHideUserSplitTotalRow() {
            $("[id*=tblSplit_CT]").each(function () {
                var $tableId = $(this).attr('id');
                currNoOfRows = $("#" + $tableId + " tbody").find('tr').length;
                $("#" + $tableId + " tfoot").hide();
                if (parseInt(currNoOfRows) > 1) {
                    $("#" + $tableId + " tfoot").show();
                }
            });
        }

        //This recalc the Split Amount after more off the top expenses are added.
        function calcOffTheTopExpenses() {
            var fOffTheTopExpenses = 0;
            $(".JQExpenseSplit").each(function () {
                fOffTheTopExpenses += parseFloat($(this).text());
            });
            $("#dOffTheTopTotal").text(parseFloat(fOffTheTopExpenses).toFixed(2));
        }

        function updateSplitAmount() {
            $("#hdSplitAmount").val(parseFloat($("#hdGrossCommission").val()) - parseFloat($("#dOffTheTopTotal").text()));
        }

        function getNetCommission() {
            calcOffTheTopExpenses();
            updateSplitAmount();
            return $("#hdSplitAmount").val();
        }

        //Updates Sale Split & User Sale Split
        function runSplitUpdate() {
            $(".JQSaleSplit").each(function () {
                //txtCalculatedAmount_CT_6_SS_1
                $thisID = $(this).attr("id");
                if ($("#" + $thisID.replace("txtCalculatedAmount_", "lstAmountType_")).val() == "1") { //%
                    var calcValue = getPercentage(getNetCommission(), $("#" + $thisID.replace("txtCalculatedAmount_", "txtAmount_")).val());
                    $(this).text(calcValue);
                }
                else {
                    ;
                }
            });

            $(".JQUserSaleSplit").each(function () {
                //txtCalculatedAmount_CT_6_SS_100_USS_24
                $thisID = $(this).attr("id");
                if ($("#" + $thisID.replace("txtCalculatedAmount_", "lstAmountType_")).val() == "1") { //%
                    var oSaleSplitAmount = $("#" + $thisID.split("_USS_")[0]).text();
                    var calcValue = getPercentage(oSaleSplitAmount, $("#" + $thisID.replace("txtCalculatedAmount_", "txtAmount_")).val());
                    $(this).text(parseFloat(calcValue).toFixed(2));
                } else {
                    ;
                }
            });

            $(".JQFletchersUserSaleSplit").text($(".JQFletchersSaleSplit").text());
        }

        //checks that a valid User is selected for each Split
        function validForSave() {
            $("#btnUpdate").removeAttr("disabled");
            $(".JQUserSaleSplitList").each(function () {
                //1. Find if Commission split is avaliable for this user split
                //2. If the value is greater than 0 we need a valis user selected
                if (parseFloat($("#" + $(this).attr("id").split("_USS_")[0].replace("lstUserSaleSplit", "txtCalculatedAmount")).text()) > 0) {
                    if ($(this).val() == "-1") {
                        $("#btnUpdate").attr("disabled", true);
                    }
                }
                if (parseFloat($("#" + $(this).attr("id").replace("lstUserSaleSplit", "txtAmount")).val()) == 0) {
                    if ($(this).val() == "-1") {
                        $("#btnUpdate").removeAttr("disabled");
                    }
                }
            });
            //for expense we don't want to have the same expense selected more than once
            var selExpenseCategories = "";

            $(".JQSaleExpenseCategory").each(function () {
                if (inCommaSeparatedString($(this).val(), selExpenseCategories)) {
                    $("#btnUpdate").attr("disabled", true);
                    alert('You have selected a sale expense that is already selected above. Please select a different sale expense.');
                }
                if ($(this).val() == "-1") {
                    $("#btnUpdate").attr("disabled", true);
                }
                else {
                    selExpenseCategories = selExpenseCategories + "," + $(this).val();
                }
            });

        }

        function getPercentage(dAmount, dPercent) {
            return parseFloat(dAmount * dPercent / 100).toFixed(2);
        }

        var selectdRowID = "";
        var jsHTML = "";
        var $table = null;
        function addSplitRow(obj) {
            selectdRowID = obj.id;
            intCType = parseInt(selectdRowID.split("_CT_")[1]);
            $table = $("#" + selectdRowID.replace("btnSplit_", "tblSplit_"));
            intSaleSplitID = parseInt(selectdRowID.split("_SS_")[1]);
            intUserSaleSplitCount = getRowCount($table);
            callWebMethod("../web_services/ws_Paymaker.asmx", "getSplitHTML", ["CommissionTypeID", intCType, "SaleSplitID", intSaleSplitID, "UserSaleSplitCount", intUserSaleSplitCount], getSplitHTMLSuccess);
        }

        function getSplitHTMLSuccess(szHTML) {
            jsHTML = szHTML;
            addTableRow($table);
            showHideUserSplitTotalRow();
            addValidation();
            calcRestOfUserSplit($table);
        }

        function calcRestOfUserSplit(jQtable) {
            //get the IDs of the UserSplit Amount
            var enteredPercentage = 0;
            var enteredValue = 0;
            NoOfRows = getRowCount(jQtable);

            var blnUsePercentage = false;
            var blnUseValue = false;

            jQtable.each(function () {
                var $tableId = $(this).attr('id');
                var currRow = 0;
                for (i = 0; i < NoOfRows - 1; i++) {
                    if ($("#" + $tableId).find('tr:eq(' + i + ')').find('.percent').length > 0) {
                        enteredPercentage = parseFloat(enteredPercentage) + parseFloat($("#" + $tableId).find('tr:eq(' + i + ')').find('.percent').val());
                        blnUsePercentage = true;
                        blnUseValue = false;
                    }
                }
                for (i = 0; i < NoOfRows - 1; i++) {
                    if ($("#" + $tableId).find('tr:eq(' + i + ')').find('.numbersOnly').length > 0) {
                        enteredValue = parseFloat(enteredValue) + parseFloat($("#" + $tableId).find('tr:eq(' + i + ')').find('.numbersOnly').val());
                        blnUsePercentage = false;
                        blnUseValue = true;
                    }
                }
                if (blnUsePercentage) {
                    //make it select the % as well
                    $("#" + $tableId).find('tr:eq(' + parseInt(NoOfRows - 1) + ')').find('.JQUserSaleSplitAmountTypeList').val('1');
                    $("#" + $tableId).find('tr:eq(' + parseInt(NoOfRows - 1) + ')').find('.JQUserSaleSplitAmount').removeClass('numbersOnly').addClass('percent');
                    addValidation();
                    $("#" + $tableId).find('tr:eq(' + parseInt(NoOfRows - 1) + ')').find('.JQUserSaleSplitAmount').val(100 - parseFloat(enteredPercentage));
                }
                //tblSplit_CT_6_SS_154
                //txtCalculatedAmount_CT_6_SS_154
                if (blnUseValue) {
                    $("#" + $tableId).find('tr:eq(' + parseInt(NoOfRows - 1) + ')').find('.JQUserSaleSplitAmountTypeList').val('0');
                    var calValue = parseFloat($("#" + $tableId.replace("tblSplit_", "txtCalculatedAmount_")).text());
                    $("#" + $tableId).find('tr:eq(' + parseInt(NoOfRows - 1) + ')').find('.JQUserSaleSplitAmount').val(parseFloat(calValue - enteredValue).toFixed(2));
                }
            });
        }

        function getRowCount(jQtable) {
            return parseInt($('tbody tr', jQtable).length);
        }

        function addTableRow(jQtable) {
            jQtable.each(function () {
                var $tableId = $(this).attr('id');
                currNoOfRows = $("#" + $tableId + " tbody").find('tr').length;

                if ($('tbody', this).length > 0) {
                    $('tbody', this).append(jsHTML);
                } else {
                    $(this).append(jsHTML);

                }
                //focus on the select user on the recently added user split row
                $('tbody', this).find('tr').eq(parseInt(currNoOfRows)).find('select').eq(0).focus();

                //remove the add button from the row above
                $("#" + $tableId + " tbody tr:eq(" + parseInt(currNoOfRows - 1) + ")" + " td:eq(6)").html('');
            });
        }

        function deleteSaleExpense(thisID, intID) {
            if (jQuery($("#" + thisID.id).parent().parent().attr('id') == 'uncategorized')) {
                jQuery($("#" + thisID.id).parent().parent().remove());
                szDelIDs = $("#hdDelExpenseIDs").val();
                if (szDelIDs != "")
                    szDelIDs += ",";
                szDelIDs += intID;
                $("#hdDelExpenseIDs").val(szDelIDs)
            }
            updateCalculations();
        }

        function addSaleExpense() {
            callWebMethod("../web_services/ws_Paymaker.asmx", "getExpenseHTML", ["SaleExpenseCount", $("#ulExpenses li").size()], getExpenseHTMLSuccess);
        }
        function checkExpenseCategory(obj) {
            setDefaultAmountTypeForExpense(obj);
        }
        function setDefaultAmountTypeForExpense(obj) {
            var oExpenseObj = arrSalesExpense[$("#" + obj.id).val()];
            $("#" + obj.id.replace("lstCategory_SE_", "lstAmountType_SE_")).val(oExpenseObj.AmountType);
            $("#" + obj.id.replace("lstCategory_SE_", "txtAmount_SE_")).val(oExpenseObj.Amount);

            if ($("#" + obj.id.replace("lstCategory_SE_", "lstAmountType_SE_")).val() == "1") {
                $("#" + obj.id.replace("lstCategory_SE_", "txtAmount_SE_")).removeClass("numbersOnly").removeClass("percent").unbind("keypress").addClass("percent");
            }
            if ($("#" + obj.id.replace("lstCategory_SE_", "lstAmountType_SE_")).val() == "0") {
                $("#" + obj.id.replace("lstCategory_SE_", "txtAmount_SE_")).removeClass("numbersOnly").removeClass("percent").unbind("keypress").addClass("numbersOnly");
            }
            addValidation();
            saleSplitAmountChange(obj.id.replace("lstCategory_SE_", "txtAmount_SE_"), true);
        }

        function getExpenseHTMLSuccess(szHTML) {
            if (szHTML == "")
                return;

            $("#ulExpenses").append("<li id='li_" + szHTML.split('^^***^^')[1] + "'>" + szHTML.split('^^***^^')[0] + "</li>");
            addValidation();
            $("#ulExpenses select[id*=lstCategory_SE_]").focus();
            validForSave();
        }
        function updateSaleSplit() {
            $(".JQSaleSplit").each(function () {
                if ($(this).hasClass('JQSaleSplit')) {
                    $("#hdSaleSplit").val($("#hdSaleSplit").val() + "," + $(this).attr("id") + ";" + $(this).text());
                }
            });
            //alert($("#hdSaleSplit").val());
        }

        function updateUserSaleSplit() {
            $("[id*=txtCalculatedAmount]").each(function () {
                if ($(this).hasClass('JQUserSaleSplit') && $thisID.indexOf("_USS_") > -1) {
                    $("#hdUserSaleSplit").val($("#hdUserSaleSplit").val() + "," + $(this).attr("id") + ";" + $(this).text());
                }
            });
            //alert($("#hdUserSaleSplit").val());
        }

        function updateOffTheTopExpensesSplit() {
            $(".JQExpenseSplit").each(function () {
                if ($(this).hasClass('JQExpenseSplit')) {
                    $("#hdOffTheTopExpensesSplit").val($("#hdOffTheTopExpensesSplit").val() + "," + $(this).attr("id") + ";" + $(this).text());
                }
            });
        }

        //lstAmountType_CT_9_SS_-1 -> txtAmount_CT_9_SS_-1 ->  txtCalculatedAmount_CT_9_SS_-1
        //lstAmountType_CT_9_SS_-1_USS_1 -> txtAmount_CT_9_SS_-1_USS_1 -> txtCalculatedAmount_CT_9_SS_-1_USS_1

        var blUpdatePressed = false;

        function updateSplit() {
            if (checkAddSum()) {
                // ensures form is only submitted once
                if (blUpdatePressed)
                    return false;
                updateOffTheTopExpensesSplit();
                updateSaleSplit();
                updateUserSaleSplit();
                writeKPIUser();
                blUpdatePressed = true;
                return true;
            }
            blUpdatePressed = false;
            return false;
        }

        function Close() {
            parent.refreshPage();
        }

        function Cancel() {
            parent.closeSale();
        }

        function checkAddSum() {
            fGrossCommission = parseFloat($("#dGrossCommission").text());
            fCommissionsTotal = parseFloat($("#dCommissionsTotal").text());
            fOffTheTopTotal = parseFloat($("#dOffTheTopTotal").text());
            if (Math.abs(parseFloat(fGrossCommission - fCommissionsTotal - fOffTheTopTotal).toFixed(2)) > 0.02) {
                var szMsg = 'Error : Gross Commission amount $' + fGrossCommission.toFixed(2) + ' does not match with the total of Sale split of $' + parseFloat(fCommissionsTotal).toFixed(2) + ' and Off the top expense total of $' + parseFloat(fOffTheTopTotal).toFixed(2);
                alert(szMsg);
                return false;
            }
            return true;
        }
        //this remove the % option if the entered value is more than 100
        function removePercentage(elemValueID, elemListID) {
            if (parseFloat($("#" + elemValueID).val()) > 100) {
                $("#" + elemListID + " option[value='1']").remove();
            } else {
                if ($("#" + elemListID + " option").size() == 1)
                    $("#" + elemListID).append('<option value="1">%</option>');
            }
        }

        function makeReadOnly() {
            if ($("#hdReadOnly").val() == "true") {
                $("[id*=btnDelete_SE]").each(function () {
                    $(this).hide();
                });
                $("[id*=btnSplit_]").each(function () {
                    $(this).hide();
                });
                $('.JQSaleSplitAmount').attr("disabled", true);
                $('select').attr("disabled", true);
                $('input').attr("disabled", true);
                $('.JQHideOnReadOnly').hide();
                $("#btnCancel").removeAttr("disabled");
                $("#btnUpdate").hide()
                $("#btnFinalize").removeAttr("disabled");
                if ($("#btnFinalize").length > 0) {
                    $("#hdSaleID").removeAttr("disabled");
                    $("[id^=__]").removeAttr("disabled"); //Enable the viewstate
                }
            }
        }

        function writeKPIUser() {
            //Check that there is a valid lister selected
            if ($(".SelectKPI:checked").length == 0)
                $(".SelectKPI:first").attr("checked", "checked");

            $(".SelectKPI:checked").each(function () {
                szID = $(this).attr("id");
                szID = szID.replace("chkIncludeInKPI", "lstUserSaleSplit");
                var szSelectedKPIUser = $("#" + szID).val();
                $("#hdKPIUserID").val(szSelectedKPIUser);
            });
        }

        function highlightTextOnFocus(obj) {
            obj.select();
        }

        $(document).ready(function () {
            $(".RoundPanel").corner();
            $("#hdUserSaleSplit").val("");
            $("#dGrossCommission").text(parseFloat($("#hdGrossCommission").val()).toFixed(2));
            updateCalculations();
            addValidation();
            showHideUserSplitTotalRow();
            makeReadOnly();
            $("[id*=lstUserSaleSplit_CT_]").unbind('change').bind('change', function () {
                validForSave();
            });
            $(".JQFletchersSaleSplitAmount").unbind('blur').bind('blur', function () {
                $(".JQFletchersUserSaleSplit").text($(".JQFletchersSaleSplit").text());
            });
            createCalendar("txtAuctionDate");
            blUpdatePressed = false;
        });
    </script>
    <style type="text/css">
        .Box1 {
            font-family: Tahoma, Arial, Verdana;
            font-size: 12px;
            color: #333333;
            border-collapse: collapse;
            margin-top: 10px;
        }

        .Box1Head tr td {
            border-bottom: 1px solid #E6D5AC;
        }

        .Box1 td {
            vertical-align: text-top;
            padding: 5px;
        }

        .Box1 .1Row {
            background-color: #d8e2f6;
        }

        .Box1 .2Row {
            background-color: #f2f5fa;
        }

        .Box2 {
            font-family: Tahoma, Arial, Verdana;
            font-size: 12px;
            color: #333333;
            border-collapse: collapse;
        }

            .Box2 td {
                vertical-align: text-top;
                border: 0px solid #FFFFFF;
            }

        .Box1Foot {
            background-color: #E6D5AC;
            font-size: 12px;
            color: #333333;
            vertical-align: top;
            border-collapse: collapse;
        }

            .Box1Foot td {
                vertical-align: text-top;
            }

        .nobullets {
            list-style-type: none;
            clear: both;
            width: 100%;
        }

        .Error {
            border: 1px solid red;
        }

        .Short .EntryPos {
            width: 100px;
        }
    </style>
</head>
<body>
    <form id="frmMain" method="post" runat="server" target="_self">
        <asp:HiddenField ID="hdSaleID" runat="server" Value="-1" />
        <asp:HiddenField ID="hdReadOnly" runat="server" Value="false" />
        <asp:HiddenField ID="hdGrossCommission" runat="server" Value="0" />
        <asp:HiddenField ID="hdSplitAmount" runat="server" Value="0" />
        <asp:HiddenField ID="hdOffTheTopExpensesSplit" runat="server" Value="" />
        <asp:HiddenField ID="hdSaleSplit" runat="server" Value="" />
        <asp:HiddenField ID="hdUserSaleSplit" runat="server" Value="" />
        <asp:HiddenField ID="hdDebugMode" runat="server" Value="false" />
        <asp:HiddenField ID="hdKPIUserID" runat="server" Value="-1" />
        <asp:HiddenField ID="hdDelExpenseIDs" runat="server" Value="" />
        <input id="hdDifferenceSpiltAmountAndCommissionTotal" type="hidden" value="0" />
        <input id="hdDifferenceUserSpiltAmountAndCommissionTotal" type="hidden" value="0" />
        <div class='ActionPanel' style="width: 99%; float: left">
            <div class='RightActionPanel'>
                <asp:Button ID="btnUpdate" CssClass="Button btn" runat="server" Text="Update" UseSubmitBehavior="true" OnClick="btnUpdate_Click" OnClientClick="return updateSplit();" Width="80" />
                <asp:Button ID="btnHide" CssClass="Button btn" runat="server" Text="Hide" OnClick="btnHide_Click" Visible="false" />
                <asp:Button ID="btnCancel" CssClass="Button btn MarginTop" runat="server" Text="Cancel" OnClientClick="Cancel(); return false;" Width="80" UseSubmitBehavior="false" />
                <asp:Button ID="btnRefresh" CssClass="Button btn MarginTop" runat="server" Text="Refresh B&D" OnClick="btnRefreshBnD_Click" Width="80" Visible="false" />

                <br />
                <br />
                <br />
                <asp:Button ID="btnFinalize" CssClass="Button btn" runat="server" Text="Finalize" UseSubmitBehavior="true" OnClick="btnFinalize_Click" Visible="false" />
            </div>
            <div id="pPageHeader" class='PageHeader' runat="server" style='font-size: 11px; width: 90%; float: left;'>
                <div style='width: 24%; margin-right: 2%; float: left;' class='RoundPanel Short'>
                    <asp:Label ID="txtCode" CssClass="Normal EntryPos" runat="server" Text="" Style="width: 200px"></asp:Label>
                    <br class='Align' />
                    <asp:Label ID="lblCommission" CssClass="Label LabelPos" runat="server" Text="Gross commission"></asp:Label>
                    <asp:Label ID="dOrigCommission" CssClass="Normal EntryPos" runat="server" Text="" Style="text-align: right"></asp:Label>
                    <br class='Align' />
                    <asp:Label ID="lblConjCommission" CssClass="Label LabelPos" runat="server" Text="Conj commission"></asp:Label>
                    <asp:Label ID="dConjCommission" CssClass="Normal EntryPos" runat="server" Text="" Style="text-align: right"></asp:Label>
                    <br class='Align' />
                    <asp:Label ID="lblGrossCommission" CssClass="Label LabelPos" runat="server" Text="Total commission"></asp:Label>
                    <asp:Label ID="dGrossCommission" CssClass="Normal EntryPos" runat="server" Text="" Style="text-align: right"></asp:Label>
                    <br class='Align' />
                    <asp:Label ID="lblSalesDate" CssClass="Label LabelPos" runat="server" Text="Sale date"></asp:Label>
                    <asp:Label ID="txtSalesDate" CssClass="Normal EntryPos" runat="server" Text="" Style="text-align: right"></asp:Label>
                    <br class='Align' />
                    <asp:Label ID="Label3" CssClass="Label LabelPos" runat="server" Text="Status"></asp:Label>
                    <asp:DropDownList ID="lstStatus" runat="server" CssClass="Entry EntryPos" Width="120px">
                        <asp:ListItem Text="Incomplete" Value="0"></asp:ListItem>
                        <asp:ListItem Text="Completed" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Finalized" Value="2"></asp:ListItem>
                    </asp:DropDownList>
                    <br class='Align' />
                </div>
                <div style='width: 24%; margin-right: 2%; float: left;' class='RoundPanel Short'>
                    <div class="Box1Head" style="float: left; width: 99%; clear: both">KPI information</div>
                    <asp:Label ID="Label1" CssClass="Label LabelPos" runat="server" Text="Auction date"></asp:Label>
                    <asp:TextBox ID="txtAuctionDate" runat="server" CssClass="Normal EntryPos"></asp:TextBox>
                    <br class='Align' />

                    <asp:Label ID="Label2" CssClass="Label LabelPos" runat="server" Text="Advertising spend"></asp:Label>
                    <asp:TextBox ID="txtAdvertisingSpend" runat="server" CssClass="Normal EntryPos" Style="text-align: right"></asp:TextBox>
                    <br class='Align' />

                    <asp:Label ID="Label4" CssClass="Label LabelPos" runat="server" Text="Section 27 supplied"></asp:Label>
                    <asp:CheckBox ID="chkIsSection27" runat="server" />
                    <br class='Align' />

                    <asp:Label ID="Label6" CssClass="Label LabelPos" runat="server" Text="Comments"></asp:Label>
                    <asp:TextBox ID="txtSection27Comments" runat="server" CssClass="Normal EntryPos" TextMode="MultiLine" Height="30"></asp:TextBox>
                    <br class='Align' />

                    <asp:Label ID="Label5" CssClass="Label LabelPos" runat="server" Text="Ignore junior sales bonus"></asp:Label>
                    <asp:CheckBox ID="chkIgnoreBonus" runat="server" />
                    <br class='Align' />
                </div>
                <div style='width: 44%; float: left' class='RoundPanel'>
                    <span class='RoundPanelHeader' id='oOFfTheTopExpense'>Off the top expenses</span>
                    <ul id="ulExpenses" class="nobullets" runat="server" style='margin-top: 0px; margin-left: 5px'>
                    </ul>
                    <div style="text-align: right; padding-right: 30px">
                        <a class='LinkButton JQHideOnReadOnly' onclick='addSaleExpense(this)' href='#'>Add new expense</a>
                    </div>
                    <div style="text-align: right; padding-right: 30px">
                        <hr />
                        <span class="JQExpenseSplitTotal CalcTotal">Expense total :</span> <span class="JQExpenseSplitTotal CalcTotal" id="dOffTheTopTotal" runat="server"></span>
                    </div>
                </div>
            </div>
            <asp:Label ID="Label7" CssClass="Label LabelPos" runat="server" Text="Comments"></asp:Label>
            <asp:TextBox ID="txtComments" runat="server" CssClass="Normal EntryPos" TextMode="MultiLine" Height="30" Width="80%"></asp:TextBox>
            <br class='Align' />
        </div>
        <div id='dSaleInfo' runat="server" style="width: 99%; float: left; clear: left">
        </div>
    </form>
</body>
</html>