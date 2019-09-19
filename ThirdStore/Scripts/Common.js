function display_kendoui_grid_error(e) {
    if (e.errors) {
        if ((typeof e.errors) == 'string') {
            //single error
            //display the message
            alert(e.errors);
        } else {
            //array of errors
            //source: http://docs.kendoui.com/getting-started/using-kendo-with/aspnet-mvc/helpers/grid/faq#how-do-i-display-model-state-errors?
            var message = "The following errors have occurred:";
            //create a message containing all errors.
            $.each(e.errors, function (key, value) {
                if (value.errors) {
                    message += "\n";
                    message += value.errors.join("\n");
                }
            });
            //display the message
            alert(message);
        }
    } else {
        alert('Error happened');
    }
}