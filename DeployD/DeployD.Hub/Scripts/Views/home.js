var _agents;
var _packages;
$(document).ready(function () {
    $('#deployDialog').hide();

    LoadAgents();
});

function LoadAgents() {
    $.getJSON('/api/agent/list',
        function (response, status) {
            _agents = response;
            LoadPackages();
        });
}

function LoadPackages() {
    $.getJSON('/api/package/list',
        function (response, status) {
            _packages = response;
            BuildGrid(_agents, _packages);
        });
}

function resetInstallationForm() {
    $('input:checked[type=checkbox][name=hostname]').attr('checked', false);
    $('#packageIdSelect :nth-child(1)').attr('selected', 'selected');
    $('#versionSelect :nth-child(1)').attr('selected', 'selected');
}

function BuildGrid(agents, packages) {
    var grid = $('#grid');

    grid.html('');

    var table = $('<table></table>');
    var header = $('<tr></tr>');
    header.append($('<td>Packages</td>'));
    for(var i=0;i<agents.length;i++) {
        header.append($('<td>' + agents[i].hostname + '</td>'));
    }

    table.append(header);

    for (var i = 0; i < packages.length;i++) {
        var row = $('<tr></tr>');
        row.append($('<td><a href="#" class="package">' + packages[i].id + '</a></td>'));
        for (var j = 0; j < agents.length; j++) {
            if (!agents[j].packages) {
                row.append($('<td>n/i</td>'));
            } else {
                var has = false;
                for(var k=0;k<agents[j].packages.length;k++) {
                    if (agents[j].packages[k].id==packages[i].id) {
                        if (!agents[j].packages[k].installed) {
                            row.append($('<td>n/i</td>'));
                        } else {
                            row.append($('<td>'+agents[j].packages[k].installedVersion+'</td>'));
                        }
                        has = true;
                        break;
                    }
                }
                if (!has) {
                    row.append($('<td>n/i</td>'));
                }
            }
        }
        table.append(row);
    };

    grid.html(table);

    BindEvents();
}

function BindEvents() {
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

    $('#registerAgentForm').submit(function () {
        $.post('/api/agent/register',
            'hostname=' + $('#registerAgentForm input[name=hostname]').val(),
            function () {
                $('#registerAgentForm input[name=hostname]').val('');
                LoadAgents();
            });
        return false;
    });
}