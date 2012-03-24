var _appTemplate;
var _agentTemplate;
var _taskTemplate;
var _packageTemplate;
var _updateInterval = 6000;

(function ($) {
    var PackageModel = Backbone.Model.extend({
        defaults: {
            id: '',
            version: '',
            something: ''
        }
    });

    /* MODELS */
    var Agent = Backbone.Model.extend({
        defaults: {
            id: '',
            tags: '',
            packages: [],
            currentTasks: [],
            environment: 'unknown',
            contacted: false
        },
        urlRoot: 'api/agent'
    });

    var AgentList = Backbone.Collection.extend({
        model: Agent,
        url: 'api/agent'
    });

    /* VIEWS */
    var AgentView = Backbone.View.extend({
        tagName: 'li',
        selected: false,
        events: {
            'click a.unregister-agent': 'unregister'
        },
        initialize: function () {
            _.bindAll(this, 'render');

            this.model.bind('change', this.render);
            this.model.bind('destroy', this.render);
            this.model.bind('add', this.render);
        },
        render: function () {
            var viewModel = {
                hostname: this.model.get('id'),
                tags: this.model.get('tags'),
                packages: this.model.get('packages'),
                currentTasks: this.model.get('currentTasks'),
                availableVersions: this.model.get('availableVersions'),
                environment: this.model.get('environment'),
                selected: this.selected,
                contacted: this.model.get('contacted')
            };
            var template = _.template(_agentTemplate, viewModel);
            $(this.el).html(template);
            return this;
        },
        unregister: function () {
            this.model.destroy();
        }
    });

    var ListView = Backbone.View.extend({
        el: $('body'),

        events: {
            'click button#add': 'addItem',
            'click button#updateSelected': 'updateSelected'
        },

        initialize: function () {
            _.bindAll(this, 'render');

            this.collection = new AgentList();
            this.collection.bind('add', this.appendItem);
            this.collection.bind('change', this.render);
            this.collection.bind('destroy', this.render);
            this.collection.fetch({ add: true });

            $(this.el).append("<input name='hostname' id='new-agent-hostname' type='textbox' />");
            $(this.el).append("<button id='add'>Add Agent</button>");

            $(this.el).append('<p>Update selected to: <select id="allVersions" /> <button id="updateSelected">Update Selected</button></p>');
            $(this.el).append("<ul id='agents'></ul>");

            this.render();

        },

        render: function () {
            var self = this;
            //$('ul#agents', this.el).html('');

            var versionList = new Array();
            _(this.collection.models).each(function (item) {
                self.updateOrAppend(item);

                var agentVersions = item.get('availableVersions');

                _(agentVersions).each(function(version) {
                    if (versionList.indexOf(version)==-1)
                        versionList.push(version);
                });
                
            }, this);

            $('select#allVersions').html('');
            _(versionList).each(function(version) {
                $('select#allVersions').append('<option>' + version + '</option>');
            });
            


        },

        addItem: function () {

            var item = new Agent();
            item.set({
                id: $('#new-agent-hostname').val()
            });
            item.save();
            this.collection.add(item);
            this.render();
        },
        
        updateSelected: function () {
            var version = $('#allVersions').val();
            var selectedAgents = $('input:checked[type=checkbox][name="select-agent"]');
            var hostnames = '';
            _(selectedAgents).each(function(input) {
                hostnames += '&agentHostnames=' + $(input).attr('data-value');
            });
            $.post('/api/agent/updateall',
                'version=' + version + hostnames,
            function () {
                listView.collection.fetch();
                listView.render();
            });
        },

        updateOrAppend: function (item) {
            var agentView = new AgentView({
                model: item,
                selected: false
            });

            var agentElement = $('ul#agents li#' + item.id, this.el);
            
            if (agentElement.length==0) {
                $('ul#agents', this.el).append(agentView.render().el);
            } else {
                var selected = $('input:checked', agentElement);
                if(selected.length > 0) {
                    agentView.selected = true;
                }
                $(agentElement).replaceWith(agentView.render().el);
            }

            
        }
    });

    var listView = new ListView();
    listView.collection.fetch();
    listView.render({add:true});

    setInterval(function () { 
        listView.collection.fetch();
        listView.render(); }, _updateInterval);
    
})(jQuery);

$(document).ready(function () {
    _appTemplate = $('#app-template').html();
    _agentTemplate = $('#agent-row-template').html();
    _taskTemplate = $('#task-template').html();
    _packageTemplate = $('#package-cell-template').html();
    
});