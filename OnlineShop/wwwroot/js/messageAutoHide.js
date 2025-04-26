
$(document).ready(function () {
    // Find status message with ID "statusMessage"
    if ($("#statusMessage").length > 0) {
        // Wait for specified duration 
        setTimeout(function () {
            // Fade out slowly then remove from DOM
            $("#statusMessage").fadeOut('slow', function () {
                $(this).remove();
            });
        }, 2000);
    }
});