/**********************************************************
 * Date sorting plugin for DataTables library v0.1
 *
 * To-do: Handle am/pm designator.
 *
 * Date: 21 Jan 2014
 *********************************************************/

// dd/mm/yyyy
jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "date-ddmmyyyy-pre": function (a) {
        var dateParts;

        // Remove leading and trailing space
        a = $.trim(a.replace(/&nbsp;/g, " "));
        // Blank string
        if (a.length == 0) return 0;

        // Get date parts
        a = a.replace(/\s*[,|-|:|\s]\s*/g, "/");
        dateParts = a.split('/');
        if (dateParts.length < 3) {
            // Unexpected format
            return 0;
        } else {
            switch (dateParts.length) {
                case 5:
                    // ddmmyyyyhhmm -> yyyymmddhhmmss
                    return 1 * (
                        dateParts[2]
                        + convertMonth(dateParts[1])
                        + addLeadingZero(dateParts[0])
                        + addLeadingZero(dateParts[3])
                        + addLeadingZero(dateParts[4])
                        + "00");
                case 6:
                    // ddmmyyyyhhmmss -> yyyymmddhhmmss
                    return 1 * (
                        dateParts[2]
                        + convertMonth(dateParts[1])
                        + addLeadingZero(dateParts[0])
                        + addLeadingZero(dateParts[3])
                        + addLeadingZero(dateParts[4])
                        + addLeadingZero(dateParts[5]));
                default:
                    // ddmmyyyy -> yyyymmddhhmmss
                    return 1 * (
                        dateParts[2]
                        + convertMonth(dateParts[1])
                        + addLeadingZero(dateParts[0])
                        + "000000");
            }
        }
    },

    "date-ddmmyyyy-asc": function (a, b) {
        return a - b;
    },

    "date-ddmmyyyy-desc": function (a, b) {
        return b - a;
    }
});

// mmddyyyy
jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "date-mmddyyyy-pre": function (a) {
        var dateParts;

        // Remove leading and trailing space
        a = $.trim(a.replace(/&nbsp;/g, " "));
        // Blank string
        if (a.length == 0) return 0;

        // Get date parts
        a = a.replace(/\s*[,|-|:|\s]\s*/g, "/");
        dateParts = a.split('/');
        if (dateParts.length < 3) {
            // Unexpected format
            return 0;
        } else {
            switch (dateParts.length) {
                case 5:
                    // mmddyyyyhhmm -> yyyymmddhhmmss
                    return 1 * (
                        dateParts[2]
                        + convertMonth(dateParts[0])
                        + addLeadingZero(dateParts[1])
                        + addLeadingZero(dateParts[3])
                        + addLeadingZero(dateParts[4])
                        + "00");
                case 6:
                    // mmddyyyyhhmmss -> yyyymmddhhmmss
                    return 1 * (
                        dateParts[2]
                        + convertMonth(dateParts[0])
                        + addLeadingZero(dateParts[1])
                        + addLeadingZero(dateParts[3])
                        + addLeadingZero(dateParts[4])
                        + addLeadingZero(dateParts[5]));
                default:
                    // mmddyyyy -> yyyymmddhhmmss
                    return 1 * (
                        dateParts[2]
                        + convertMonth(dateParts[0])
                        + addLeadingZero(dateParts[1])
                        + "000000");
            }
        }
    },

    "date-mmddyyyy-asc": function (a, b) {
        return a - b;
    },

    "date-mmddyyyy-desc": function (a, b) {
        return b - a;
    }
});

function convertMonth(month) {
    switch (month.toLowerCase()) {
        case "jan": return "01";
        case "january": return "01";
        case "feb": return "02";
        case "february": return "02";
        case "mar", "march": return "03";
        case "mar": return "03";
        case "march": return "03";
        case "apr": return "04";
        case "april": return "04";
        case "may": return "05";
        case "jun": return "06";
        case "june": return "06";
        case "jul": return "07";
        case "july": return "07";
        case "aug": return "08";
        case "august": return "08";
        case "sep": return "09";
        case "sept": return "09";
        case "september": return "09";
        case "oct": return "10";
        case "october": return "10";
        case "nov": return "11";
        case "november": return "11";
        case "dec": return "12";
        case "december": return "12";
        default: break;
    }
    return addLeadingZero(month);
}

function addLeadingZero(input) {
    if (isNaN(input)) return input;
    if (parseInt(input) <= 0 || parseInt(input) >= 10) return input;
    return "0" + parseInt(input);
}