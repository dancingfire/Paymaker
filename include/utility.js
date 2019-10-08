function LTrim(str) {
    var whitespace = new String(" \t\n\r");
    var s = new String(str);
    if (whitespace.indexOf(s.charAt(0)) != -1 || s.charCodeAt(0) == 160) {
        var j = 0, i = s.length;
        while (j < i && whitespace.indexOf(s.charAt(j)) != -1 || s.charCodeAt(j) == 160)
            j++;
        s = s.substring(j, i);
    }
    return s;
}

function processDataTableHeaderFilters() {
    $('#tData tfoot tr').insertAfter($('#tData thead tr'))
    $(".TableFilter").click(function (e) {
        e.stopPropagation();
    })
}


function showDelegation() {
    $('#mDelegation').modal({ 'backdrop': 'static' });
    $('#mDelegation').one('show.bs.modal', function () {
        $('#fDelegation').attr("src", "../admin/user_delegation.aspx");
    });
    $('#mDelegation').modal('show');
}

function showAdminDelegation() {
    $('#mDelegation').modal({ 'backdrop': 'static' });
    $('#mDelegation').one('show.bs.modal', function () {
        $('#fDelegation').attr("src", "../admin/admin_user_delegation.aspx");
    });
    $('#mDelegation').modal('show');
}

function closeDelegation() {
    $('#mDelegation').modal('hide');
}

function showLoginAs() {
    $('#mLoginAs').modal({ 'backdrop': 'static' });
    $('#mLoginAs').one('show.bs.modal', function () {
        $('#fLoginAs').attr("src", "../admin/user_login_as.aspx");
    });
    $('#mLoginAs').modal('show');
}

function closeLoginAs() {
    $('#mLoginAs').modal('hide');
}

function closeEditModal(blnRefresh) {
    $("#fModalUpdate").attr('src', 'about:blank');
    $("#mModalUpdate").modal("hide");
    if (blnRefresh) {
        location.href = location.href;
    }
}

function select2Setup() {
    $(".select2-selection").on("focus", function () {
        $(this).parent().parent().prev().select2("open");
    });

    $('.select2-selection').keydown((ev) => {
        if (ev.which < 32)
            return;

        var target = jQuery(ev.target).closest('.select2-container');
        if (!target.length)
            return;

        target = target.prev();
        target.select2('open');

        var search = target.data('select2').dropdown.$search ||
            target.data('select2').selection.$search;

        search.focus();
    });
}

function disableForm(frmName, blnAllowNotes) {
    $("#" + frmName).find(':input').not(".Search").addClass("readonly").prop('disabled', true).attr('placeholder', '');

    // Make sure we show the seach field - the 'not' doesn't appear to be working
    $("input[type='hidden']").removeClass("readonly").removeAttr('disabled');
   
    $(".ImageAdd").hide();
    $("#btnCancel, #btnClose, .close").removeAttr("disabled");
    $("#chkShowInactiveProjects").removeAttr("disabled");

    $("select").each(function () {
        var val = $(this).val();
        szID = $(this).prop("id");
        szVal = $("#" + szID + " option:selected").text();
        szClass = $(this).prop("class");
        if (szClass.indexOf("doNotDisable") == -1) {
            szStyle = $(this).attr("style");
            if ($("#" + szID).val() == "" || $("#" + szID).val() == "-1")
                szVal = "";

            $(this).replaceWith("<div id='" + szID + "' class='" + szClass + "' style='" + szStyle + "'>" + szVal + "</div>");
        }
    });

    $("textarea").not(".Search").each(function () {
        szID = $(this).prop("id");
        if (szID == "txtNotes" && blnAllowNotes) {
            ;
        } else {
            $(this).replaceWith("<div id='" + szID + "' class='Entry EntryPos' >" + $(this).val() + "</div>");
        }
    });
}


function editTimesheet(UserID, CycleID) {
    $('#mTimesheet').on('show.bs.modal', function () {
        params = "?IsPopup=true&blnReadOnly=" + blnReadOnly + "&UserID=" + UserID;
        if (CycleID != "")
            params += "&CycleID=" + CycleID;
        $('#fTimesheet').attr("src", "payroll_update.aspx" + params + "&ts=" + Date.now());
    });
    $('#mTimesheet').on('hidden.bs.modal', function () {
        $('#fTimesheet')[0].contentWindow.removeValidation();
    });

    $('#mTimesheet').modal('show');
    return false;
}

function createButtons(){
    $("input[type=button]").each(function () {
        $(this).addClass('btn Button');
    })
}
function showModal(szPage, szParams, intHeight, intWidth) {
    return window.showModalDialog("../main/dialog_frame.aspx?szTargetPage=" + szPage + szParams, "Test", "dialogHeight: " + intHeight + "px; dialogWidth: " + intWidth + "px; edge: Raised; center: Yes; help: Yes; resizable: Yes; status: yes;");
}

function RTrim(str) {
    var whitespace = new String(" \t\n\r");
    var s = new String(str);
    if (whitespace.indexOf(s.charAt(s.length - 1)) != -1) {
        var i = s.length - 1;
        while (i >= 0 && whitespace.indexOf(s.charAt(i)) != -1)
            i--;
        s = s.substring(0, i + 1);
    }
    return s;
}

function addFormValidation(szFormName) {
    /*$("input, select, textarea").not("#txtSearch").change(function () {
        $(this).addClass("FieldChanged");
        blnIsDirty = true;
    });*/

    $(':input[required=""],:input[required]').each(function (i) {
        szID = $(this).attr('id');
        $(this).after("<span id='" + szID + "Marker' style='color: red; float: left;; font-size: 8px; padding-top: 4px'>*</span>");
    });

    $(window).on('unload', function (e) {
        removeValidation();
    });

    return $("#" + szFormName).validVal({
        'form': {
            'onInvalid': function ($form, language) {
                // attach an error to form
                $(this).attr('error', 'yes');
                $("#btnUpdate").attr("disabled", "disabled");
            }
        },
        'fields': {
            'onValid': function ($form, language) {
                $("#btnUpdate").removeAttr("disabled");
                $(this).add($(this).parent()).removeClass('invalid');
            },
            'onInvalid': function ($form, language) {
                $(this).add($(this).parent()).addClass('invalid');
                $("#btnUpdate").attr("disabled", "disabled");
            }
        },
        customValidations: {
            "required2": function (val) {
                if (val.length < 2) {
                    return false;
                } else {
                    return true;
                }
            }
        }
    });
}

function removeValidation() {
    $("form").trigger("destroy.vv");
    window.onbeforeunload = null;
}

var szDataUpdateName = "";
function refreshDataArea(szName, intObjectID) {
    szDataUpdateName = szName;
    callWebMethod("../web_services/ws_Paymaker.asmx", "refresh" + szName, ["ItemID", intObjectID], refreshDataAreaSuccess);
}

function refreshDataAreaSuccess(szHTML) {
    $("#d" + szDataUpdateName).html(szHTML);
}

function resizeFrameToContent(obj) {
    intHeight = obj.contentWindow.document.body.scrollHeight;
    obj.height = (intHeight) + "px";
    intMaxHeight = clientHeight() - 60;
    //$(".modal-dialog,.modal-body").css("height", intHeight + 'px');
    //alert($(".modal-dialog").css("height"))
    $(".overlay-iframe").css("height", intHeight);
    $(".overlay-iframe").css("max-height", intMaxHeight).css("overflow", "auto");
}

function clientHeight() {
    var myWidth = 0, myHeight = 0;
    if (typeof (window.innerWidth) == 'number') {
        // Non-IE browsers
        // RAID 5984 - iPad layout issues under investigation...
        if (navigator.userAgent.indexOf("iPad") != -1) {
            window.innerWidth = 980;  // RAID 5984 - This seems to be the largest and stable width for an iPad...
        }
        myWidth = window.innerWidth;
        myHeight = window.innerHeight;
    } else if (document.documentElement && (document.documentElement.clientWidth || document.documentElement.clientHeight)) {
        //IE 6+ in 'standards compliant mode'
        myWidth = document.documentElement.clientWidth;
        myHeight = document.documentElement.clientHeight;
    } else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
        //IE 4 compatible
        myWidth = document.body.clientWidth;
        myHeight = document.body.clientHeight;
    }
    return myHeight;
}



function createCalendar(ControlID, blnHideIcon) {
    szDateFormat = "M d, yy";
    o = null;
    if (blnHideIcon) {
       o = $("#" + ControlID).datepicker({
            changeMonth: true,
            changeYear: true,
            showButtonPanel: true,
            dateFormat: szDateFormat,
            showAnim: 'slide',
            showOptions: { distance: 1, direction: 'up' }
        })
    } else {
        o =$("#" + ControlID).datepicker({
            showOn: 'both',
            buttonImage: '../sys_images/calendar.gif',
            buttonImageOnly: true,
            changeMonth: true,
            changeYear: true,
            showButtonPanel: true,
            dateFormat: szDateFormat,
            showAnim: 'slide',
            showOptions: { distance: 1, direction: 'up' }
        })
    }
    return o;
}

function createDataTable(szGrid, blnUseSorting, blnUseFiltering, intHeight, blnScrollCollapse, UseKeys) {
    if (null === UseKeys)
        UseKeys = false;
    if ($("#" + szGrid + " tr").length > 1) {
        var options = {
            "bPaginate": false,
            "bLengthChange": false,
            "bFilter": blnUseFiltering,
            "bSort": blnUseSorting,
            aaSorting: [],
            "bInfo": false,
            "keys": UseKeys,
            "bAutoWidth": false,
            "bScrollCollapse": true
        }

        if (intHeight && intHeight > 0) {
            options["sScrollY"] = intHeight + "px";
            if (blnScrollCollapse != null)
                options["bScrollCollapse"] = blnScrollCollapse;
            else
                options["bScrollCollapse"] = true;
        }

        // Special treatment for date sorting plugin
        // Looking for particular class names of column heading (th)
        var columnDefs = [];
        $("#" + szGrid + " th").each(function (index) {
            if ($(this).attr("class") == null) {
                columnDefs[index] = null;
            } else if ($(this).attr("class").toLowerCase().indexOf("dt-date") >= 0) {
                columnDefs[index] = { "sType": "date" };
            } else {
                columnDefs[index] = null;
            }
        });
        options["columns"] = columnDefs;
        var oTable = $('#' + szGrid).dataTable(options);

        setTimeout(function () {
            oTable.fnAdjustColumnSizing(true);
        }, 100);
        if(!blnUseFiltering)
            $(".dataTables_filter, .dataTables_info").hide();
        return oTable;
    }
}

function Trim(str) {
    return RTrim(LTrim(str));
}

function addZebra(szID) {
    //Add zebra striping
    $("#" + szID).find("tr:gt(0)").mouseover(function () { $(this).addClass("selectedRow"); }).mouseout(function () { $(this).removeClass("selectedRow"); });
}

function setSelectedIDs(lstSourceID, HDFieldID) {
    var szIDs = "";
    var oSelList = getElement(lstSourceID);
    for (var x = 0; x < oSelList.options.length; x++) {
        szIDs = append(szIDs, oSelList.options[x].value, ",");
    }
    var oHiddenIDs = getElement(HDFieldID);
    if (oHiddenIDs)
        oHiddenIDs.value = szIDs;
}

function showWait(szMsg) {
    if (szMsg == null)
        szMsg = "Please wait...";
    $.blockUI.defaults.css.top = '50%';
    $.blockUI({ message: '<span class="Normal" style="z-index: 60000; position: relative"><img src="../sys_images/loading_ani.gif" style="margin-right: 10px; vertical-align: middle" />' + szMsg + '</span>' });
}

function isVisible(szControlID) {
    return $("#" + szControlID).is(":visible");
}

function isRequiredFieldNotValid(szControlID, intLength){
    if(!isVisible(szControlID))
        return false;
    else
        return String($("#" + szControlID).val()).length == 0;
}

function isRequiredListNotValid(szControlID, InvalidValue){
    if(!isVisible(szControlID))
        return false;
    else {
        szValue = $("#" + szControlID).val();
        return String(szValue).length == 0 || szValue == InvalidValue;
    }
}

function doSessionRefresh() {
    alert("The server code has been updated. Please login again.");
    document.location.href = "../login.aspx";
}

// Appends the value to the passes in string, utilizing the split character if the string has data in it.
function append(szOriginal, szValue, szSplitChar) {
    if (szOriginal == "")
        return szValue;
    else
        return szOriginal + szSplitChar + szValue;
}

function escapeHTML(str) {
    var div = document.createElement('div');
    var text = document.createTextNode(str);
    div.appendChild(text);
    return div.innerHTML;
}

function clientWidth() {
    var myWidth = 0, myHeight = 0;
    if (typeof (window.innerWidth) == 'number') {
        //Non-IE
        myWidth = window.innerWidth;
        myHeight = window.innerHeight;
    } else if (document.documentElement && (document.documentElement.clientWidth || document.documentElement.clientHeight)) {
        //IE 6+ in 'standards compliant mode'
        myWidth = document.documentElement.clientWidth;
        myHeight = document.documentElement.clientHeight;
    } else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
        //IE 4 compatible
        myWidth = document.body.clientWidth;
        myHeight = document.body.clientHeight;
    }
    return myWidth;
}

function format_number(total, DecimalPlaces) {
    // First verify incoming value is a number
    if (isNaN(total))
        return "0.00";

    // Second round incoming value to correct number of decimal places
    var RoundedTotal = total * Math.pow(10, DecimalPlaces);
    RoundedTotal = Math.round(RoundedTotal);
    RoundedTotal = RoundedTotal / Math.pow(10, DecimalPlaces);

    // Third pad with 0's if necessary the number to a string
    var Totalstring = RoundedTotal.toString(); // Convert to a string
    var DecimalPoint = Totalstring.indexOf("."); // Look for decimal point
    if (DecimalPoint == -1) {     // No decimal so we need to pad all decimal places with 0's - if any
        currentDecimals = 0;
        // Add a decimal point if DecimalPlaces is GT 0
        Totalstring += DecimalPlaces > 0 ? "." : "";
    }
    else {     // There is already a decimal so we only need to pad remaining decimal places with 0's
        currentDecimals = Totalstring.length - DecimalPoint - 1;
    }
    // Determine how many decimal places need to be padded with 0's
    var Pad = DecimalPlaces - currentDecimals;
    if (Pad > 0) {
        for (var count = 1; count <= Pad; count++)
            Totalstring += "0";
    }
    // Return formatted value
    return Totalstring;
}

function dirtyPage(obj) {
    obj.className += " EntryChanged";
    document.frmMain.HDIsPageDirty.value = "true";
    document.frmMain.hfDoneDirty.value = "dirty";
    oUpdate = getElement("btnUpdate");
    if (oUpdate)
        oUpdate.disabled = false;
}

function doPageRefresh(intTimeRemaining) {
    intFreq = 1000;
    if (isNaN(intTimeRemaining))
        return true;
    else {
        window.status = '';
        if (intTimeRemaining < 10000)
            window.status = 'Refreshing in ' + parseInt(intTimeRemaining / 1000) + ' seconds...';
        if (intTimeRemaining < 1000)
            document.location.reload();
    }
    setTimeout('doPageRefresh(' + (intTimeRemaining - intFreq) + ' )', intFreq);
    return true;
}

function validVal(event, keyRE) {
    if ((typeof (event.keyCode) != 'undefined' && event.keyCode > 0 && String.fromCharCode(event.keyCode).search(keyRE) != (-1)) ||
        (typeof (event.charCode) != 'undefined' && event.charCode > 0 && String.fromCharCode(event.charCode).search(keyRE) != (-1)) ||
        (typeof (event.charCode) != 'undefined' && event.charCode != event.keyCode && typeof (event.keyCode) != 'undefined' && event.keyCode.toString().search(/^(8|9|13|45|46|35|36|37|39)$/) != (-1)) ||
        (typeof (event.charCode) != 'undefined' && event.charCode == event.keyCode && typeof (event.keyCode) != 'undefined' && event.keyCode.toString().search(/^(8|9|13)$/) != (-1))) {
        return true;
    } else {
        return false;
    }
}

function isNumeric(x, szAlertMsg) {
    if (!(/^\d+$/.test(x.value))) {
        alert(szAlertMsg);
        x.focus();
        return false;
    }
    return true;
}

// Checks if a given value is in a comma-separated list of values
function inCommaSeparatedString(szValueToFind, szCommaSeparatedString) {
    szCommaSeparatedString = "," + szCommaSeparatedString + ",";
    szValueToFind = "," + szValueToFind + ",";
    var intIndex = szCommaSeparatedString.indexOf(szValueToFind);
    return intIndex > -1;
}

function addValidation() {
    $(".numbersOnly").off("keypress").on('keypress', function (event) {
        // Backspace, tab, enter, end, home, left, right
        var controlKeys = [8, 9, 13, 37, 39, 45, 46];
        var isControlKey = controlKeys.join(",").match(new RegExp(event.which));
        if (9 == event.which)
            return;
        //Allowing only one decimal
        szVal = new String($(this).val());
        if (190 == event.which){
            if (szVal.indexOf(".") > -1) {
                event.preventDefault();
                return;
            } else {
                return; //Allow the decimal
            }
        }

        if (!event.which || (event.which == 45 || 110 == event.which || isValidNumberKey(event.which)) || isControlKey != null) {
            if (isTextSelected(document.getElementById($(this).attr("id")))) {
                $(this).val('');
                $(this).trigger('change');
            }
            return;
        } else {
            event.preventDefault();
        }
    });

    $(".percent").off("keypress").on('keypress', function (event) {
        // Backspace, tab, enter, end, home, left, right
        var controlKeys = [8, 9, 13, 37, 39, 45, 46, 190];
        var isControlKey = controlKeys.join(",").match(new RegExp(event.which));
        szVal = new String($(this).val());

        if (9 == event.which)
            return;
        //Allowing only one decimal
        if ((46 == event.which || 110 == event.which) && szVal.indexOf(".") > -1) {
            event.preventDefault();
        }
        //Allowing only 2 digits after decimal point
        if (szVal.indexOf(".") > -1 && szVal.split(".")[1].length > 1) {
            event.preventDefault();
        }

        if (!event.which || (isValidNumberKey(event.which)) || isControlKey != null) {
            if (isTextSelected(document.getElementById($(this).attr("id")))) {
                $(this).val('');
                $(this).trigger('change');
            }
            //Check that the user is not allowed to enter a value greater than 100%
            var fulltext = szVal + getCharFromKeyCode(event.which);
            if (parseInt(fulltext, 10) > 100) {
                event.preventDefault();
            }
            return;
        } else {
            event.preventDefault();
        }
    });
}

function getCharFromKeyCode(intKey) {
    if (intKey >= 96 && intKey <= 106)
        intKey -= 48;

    return String.fromCharCode(intKey);
}

function isValidNumberKey(intKey) {
    return ((intKey >= 48 && intKey <= 57) || (intKey >= 96 && intKey <= 105));
}
//Find if the text in a textbox is selected?
function isTextSelected(input) {
    if ($(input).val() == '')
        return false;

    if (typeof input.selectionStart == "number") {
        return input.selectionStart == 0 && input.selectionEnd == input.value.length;
    } else if (typeof document.selection != "undefined") {
        input.focus();
        return document.selection.createRange().text == input.value;
    }
}

function buttonWait(btnName) {
    $('#' + btnName).addClass('ButtonWait');
}

szPageTitleSpacer = '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;';