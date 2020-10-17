$(document).ready(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });

    //$('form').submit(function () {
    //    $(this).find("input[type='submit']").each(function () {
    //        //$(this).attr('disabled', 'disabled');
    //        $(this).prop('disabled', true); 
    //    });
    //});

    //$('form').submit(function () {
    //    $(this).find("input[type='submit']").each(function () {
    //        $(this).attr("submitting", "true");
    //        $(this).click(function (e) {
    //            if (e.target) {
    //                var attr = $(this).attr('submitting');
    //                if (typeof attr !== 'undefined' && attr !== false) { // If button has submitting attribute then do not submit it again.
    //                    //$(this).prop('disabled', true);
    //                    //$(this).removeAttr("submitting");
    //                    e.preventDefault();
    //                }
    //                else {
    //                    $(this).attr("submitting", "true"); // Add "submitting" attribute to prevent multiple submissions.
    //                    //e.preventDefault();
    //                }
    //            }
    //        });
    //    });
    //});

    //$("input[type='submit']").each(function () {
    //    $(this).click(function () {
    //        setTimeout(function () {
    //            //your code to be executed after 1 second
    //            debugger;
    //            $("form")[1].submit();
    //        }, 2000);
    //        return false;
    //    });
    //});

    $("form").submit(function () {
        //debugger;
        if ($(this).valid()) {
            $(this).submit(function () {
                //debugger;
                return false;
            });
            return true;
        }
        else {
            return false;
        }
    });
});