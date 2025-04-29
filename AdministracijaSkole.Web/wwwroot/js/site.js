$(document).ready(function () {
    $(function () {
        $(".inp-datepicker").each(function (idx, el) {
            var fmt = $(el).data('format');
            $(el).datepicker({
                format: fmt
            });
        });
    });
});