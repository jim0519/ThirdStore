$(document).ready(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });

    $('form').submit(function () {
        $(this).find("input[type='submit']").each(function () {
            $(this).attr('disabled', 'disabled');
        });
    });
});