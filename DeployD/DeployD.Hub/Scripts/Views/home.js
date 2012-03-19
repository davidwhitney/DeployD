$(document).ready(function () {
    $('#deployDialog').hide();
    $('.cancel').click(function () {
        $(this).parent().hide();
    });

    $('.package').click(function () {
        $('#deployDialog').show();
        $('#packageIdSelect').val($(this).text());
    });

    $('#startInstallationForm').submit(function () {
        $('#deployDialog').hide();

        var agents = "";
        $('input:checked[type=checkbox][name=hostname]').each(function (index, element) {
            agents += "&agents=" + $(element).val();
        });

        $.post('/api/installation/start',
            "packageId=" + $('#packageIdSelect').val()
                + "&version=" + $('#versionSelect').val()
                    + agents,
                function () {
                    resetInstallationForm();
                });

        return false;
    });
});

function resetInstallationForm() {
    $('input:checked[type=checkbox][name=hostname]').attr('checked', false);
    $('#packageIdSelect :nth-child(1)').attr('selected', 'selected');
    $('#versionSelect :nth-child(1)').attr('selected', 'selected');
}