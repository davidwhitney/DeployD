$(document).ready(function () {
    $('#deployDialog').hide();
    $('.cancel').click(function () {
        $(this).parent().hide();
    });

    $('.package').click(function () {
        $('#deployDialog').show();
        $('#packageIdSelect').val($(this).text());
    });
});