$(document).ready(function () {
    $('#mastercheckbox').click(function () {
        //$('.checkboxGroups').attr('checked', $(this).is(':checked')).change();
        $('.checkboxGroups').each(function () {
            if ($('#mastercheckbox').is(':checked')) {
                $(this).prop('checked', true);
            }
            else {
                $(this).prop('checked', false);
            }
            $(this).change();
        });
    });

    //wire up checkboxes. 
    $('div[id$=grid]').on('change', 'input[type=checkbox][id!=mastercheckbox]', function (e) {
        //debugger;
        var $check = $(this);
        if ($check.prop('checked') == true) {
            var checked = jQuery.inArray($check.val(), selectedIds);
            if (checked == -1) {
                //add id to selectedIds.  
                selectedIds.push($check.val());
            }
        }
        else {
            var checked = jQuery.inArray($check.val(), selectedIds);
            if (checked > -1) {
                //remove id from selectedIds.  
                selectedIds = $.grep(selectedIds, function (item, index) {
                    return item != $check.val();
                });
            }
        }
        //updateMasterCheckbox();
    });

    $('#btnSearch').click(function () {
        //search
        RefreshGrid(false);

        return false;
    });

    $(this).scannerDetection({
        timeBeforeScanTest: 200, // wait for the next character for upto 200ms
        avgTimeByChar: 100, // it's not a barcode if a character takes longer than 40ms
        onComplete: function (barcode, qty) {
            var e = $.Event("keypress");
            e.which = 13; //enter keycode
            e.keyCode = 13;
            $(document).trigger(e);
            
            //jQuery.event.trigger({ type: 'keypress', which: character.charCodeAt(13) });

            //alert(barcode);
        } // main callback function
    });

});

$(document).on('keypress', function (e) {
    if (e.which == 13 && $("#btnSearch").length) {
        $("#btnSearch").click();
    }
});

function onDataBound(e) {
    $('div[id$=grid] input[type=checkbox][id!=mastercheckbox]').each(function () {
        //debugger;
        var currentId = $(this).val();
        var checked = jQuery.inArray(currentId, selectedIds);
        //set checked based on if current checkbox's value is in selectedIds.  
        $(this).prop('checked', checked > -1);
    });

    updateMasterCheckbox();
}

function updateMasterCheckbox() {
    //debugger;
    var numChkBoxes = $('input[id$=grid] input[type=checkbox][id!=mastercheckbox]').length;
    var numChkBoxesChecked = $('input[id$=grid] input[type=checkbox]:checked[id!=mastercheckbox]').length;
    //var isChecked = false;
    //if (numChkBoxes == numChkBoxesChecked && numChkBoxes > 0)
    //    isChecked=true
    $('#mastercheckbox').prop('checked', numChkBoxes == numChkBoxesChecked && numChkBoxes > 0);
    //$('#mastercheckbox').prop('checked', isChecked);
}

function RefreshGrid(isBackCurrentPage) {
    var grid = $('div[id$=grid]').data('kendoGrid');
    var currentPage = 1;
    if (isBackCurrentPage)
        currentPage = grid.dataSource.page();
    grid.dataSource.page(currentPage);
}


//$.getScript("http://cdn.kendostatic.com/2014.3.1029/js/jszip.min.js", function () { });
//$.getScript("http://cdn.kendostatic.com/2014.3.1316/js/kendo.all.min.js", function () { });
//debugger;
//var jszip = document.createElement("script");
//jszip.type = "text/javascript";
//jszip.src = "http://cdn.kendostatic.com/2014.3.1029/js/jszip.min.js";
//document.head.appendChild(jszip);
//var kendoall = document.createElement("script");
//kendoall.type = "text/javascript";
//kendoall.src = "http://cdn.kendostatic.com/2014.3.1316/js/kendo.all.min.js";
//document.head.appendChild(kendoall);