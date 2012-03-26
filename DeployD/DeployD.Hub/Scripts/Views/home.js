var _appTemplate;
var _agentTemplate;
var _taskTemplate;
var _packageTemplate;
var _manageAgentDialogTemplate;
var _updateAgentsTemplate;
var _updateInterval = 3000;
var listView=null;

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
            contacted: false,
            selected: false
        },
        urlRoot: 'api/agent'
    });

    var Version = Backbone.Model.extend({
        defaults: { id: '1.0.0.0' }
    });

    var AgentList = Backbone.Collection.extend({
        model: Agent,
        url: 'api/agent'
    });

    var VersionList = Backbone.Collection.extend({
       model: Version         
    });

    /* VIEWS */
    var AgentView = Backbone.View.extend({
        tagName: 'li',
        container: 'ul#agents',
        selected: false,
        events: {
            'click a.unregister-agent': 'unregister',
            'click a.manage-agent': 'manage'
        },
        initialize: function () {
            _.bindAll(this, 'render','manage','unregister');

            this.model.bind('change', this.update);
            this.model.bind('destroy', this.render);
            this.model.bind('add', this.render);
        },
        update: function (agent) {
            this.model = agent;
            this.render();
            console.log('agent updated');
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

            _(viewModel.packages).each(function(package) {
                if (package.currentTask != null) {
                    console.log(package.packageId + ' current task is ' + package.currentTask);
                }
            });

            var template = _.template(_agentTemplate, viewModel);

            this.$el = $(template);

            return this;
        },
        unregister: function () {
            this.model.destroy();
        },
        manage: function(event) {
            var id = $(event.target).attr('data-id');
            manageAgentDialog.load(id);
        }
    });

    var ManageAgentDialogView = Backbone.View.extend({
       tagName: 'div',
       id: 'manage-agent-dialog',
       events: {
           'click a.close-dialog': 'closeDialog',
           'submit form#apply-versions-form': 'startInstall'
       },
       initialize: function () {
           _.bindAll(this, 'render');
           this.model = new Agent();
           this.$el.hide();
           $('body').append(this.$el);
       },
       load: function (hostname) {
           if (this.model) {
               console.log('load ' + hostname);
               this.model.id = hostname;
               this.model.fetch({success: this.render});
           }
       },
       render: function () {
           console.log('render dialog');
            var viewModel = {
                hostname: this.model.get('id'),
                packages: this.model.get('packages')
            };
            var dialogContent = _.template(_manageAgentDialogTemplate, viewModel);
           this.$el.html(dialogContent);
           this.$el.show();
           this.delegateEvents();

       },
       closeDialog: function () {
           $("div.dialog").hide();
       },
       startInstall: function () {
           var url = $('form', this.$el).attr('action');
           var data = '';
           var selectors = $('form select', this.$el);
           $(selectors).each(function(index,element) {
            if (data.length > 0) {
                data += '&';
            }
               data += $(element).attr('name') + '=' + $(element).val();
           });
           $.post(url, data);

           return false;
       }
    });

    var UpdateAgentsView = Backbone.View.extend({
       tagName: 'div',
       id:'update-agents',
       initialize: function () {
           
       },
       render:function () {
           this.$el.html(_.template(_updateAgentsTemplate, listView.versionCollection));
           return this;
       }
    });

    var ListView = Backbone.View.extend({
        tagName: 'div',
        id:'agent-list',
        events: {
            'click button#add': 'addItem',
            'click button#updateSelected': 'updateSelected'
        },

        initialize: function () {
            _.bindAll(this, 'render', 'add', 'remove');
            var self = this;

            this.agentViews = [];

            this.versionCollection = new VersionList();

            this.collection = new AgentList();
            this.collection.bind('change', this.change);
            this.collection.bind('add', this.add);
            this.collection.bind('remove', this.remove);
            this.collection.bind('destroy', this.render);
            this.collection.fetch({ add: true, success: this.render });

            this.collection.each(function(agent) {
              self.agentViews.push(new AgentView({
                model : agent,
                tagName : 'li'
              }));
            });

            this.updateAgentsView = new UpdateAgentsView();
        },
        add: function(agent) {
            var that = this;
            var dv = new AgentView({
                model: agent,
                tagName: 'li'
            });
            this.agentViews.push(dv);
            
            if (this._rendered) {
                $(this.el).append(dv.render().el);
            }
            
            // update versions list
            _(agent.get("packages")).each(function(package) {
                _(package.availableVersions).each(function(version) {
                    if (that.versionCollection.where({id:version}).length==0) {
                        var versionModel = new Version();
                        versionModel.id = version;
                        that.versionCollection.add(versionModel);
                    }
                });
            });
        },
        remove: function (agent) {
            var viewToRemove = _(this.agentViews).select(function(cv) { return cv.model === agent; })[0];
            this.agentViews = _(this.agentViews).without(viewToRemove);
            if (this._rendered) $(viewToRemove.el).remove();
        },
        change: function (agent) {
            console.log('listView.change()');
            var viewToUpdate = _(this.agentViews).select(function(cv) { return cv.model === agent; })[0];
            if (this._rendered) viewToUpdate.render();
        },

        render: function () {
            var self = this;
            this._rendered = true;
            
            $(this.el).empty();

            $(this.el).append(_appTemplate);
            $('div#version-select',this.$el).replaceWith(this.updateAgentsView.render().$el);

            _(this.collection.models).each(function(agent) {
                console.log('collection has agent ' + agent.id);
                console.log('with packages:');
                _(agent.get('packages')).each(function (package) {
                    console.log(package.packageId);
                    if (package.currentTask != null) {
                        console.log('current task ' + package.currentTask.lastMessage);
                    }
                });
            });

            _(this.agentViews).each(function(dv) {
                console.log('agentview render()');
                var agentId = dv.model.get("id");
                var matchingAgents= self.collection.where({ id: agentId });
                if (matchingAgents.length == 1) {
                    dv.model = matchingAgents[0];
                }
                $('ul', self.el).append(dv.render().$el);
                dv.delegateEvents();
                
                agentId=manageAgentDialog.model.get("id");
                self.collection.where({ id: agentId });
                if (matchingAgents.length == 1) {
                    manageAgentDialog.model = matchingAgents[0];
                    manageAgentDialog.render();
                }
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
            var version = $('select[name=allVersions]').val();
            var selectedAgents = $('input:checked[type=checkbox][name="select-agent"]');
            var hostnames = '';
            _(selectedAgents).each(function(input) {
                hostnames += '&agentHostnames=' + $(input).attr('data-value');
            });
            $.post('/api/agent/updateall',
                'version=' + version + hostnames,
            function () {
                listView.collection.fetch({success:function (){ listView.render();}});
            });
        },

        updateViews: function (item) {

            var agentView = this.agentViews[item.id];

            if (agentView) {
                agentView.model = item;
            }
            else {
                var agentView = new AgentView({
                    model: item,
                    selected: false
                });
                this.agentViews.push(agentView);
            }
        }
    });

    listView = new ListView();

    var manageAgentDialog = new ManageAgentDialogView();

    var updateLink = $('<a>update</a>').click(function() {
         listView.collection.fetch({ success: listView.render });
    });
    $('body').append(updateLink);

    /*setInterval(function () { 
        listView.collection.fetch({ success: listView.render }); }, 
        _updateInterval);*/
    
})(jQuery);

$(document).ready(function () {
    _appTemplate = $('#app-template').html();
    _agentTemplate = $('#agent-row-template').html();
    _taskTemplate = $('#task-template').html();
    _packageTemplate = $('#package-template').html();
    _manageAgentDialogTemplate = $('#manage-agent-template').html();
    _updateAgentsTemplate = $('#update-agents-template').html();
    
    $('div#app').append(listView.el);
});