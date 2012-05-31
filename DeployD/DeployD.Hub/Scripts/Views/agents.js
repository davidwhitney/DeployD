var agentsList = null;
$(function () {
    var Agent = Backbone.Model.extend({ urlRoot: '/api/agent' });
    var AgentList = Backbone.Collection.extend({ model: Agent, url: '/api/agent?includeUnapproved=true' });

    var AgentView = Backbone.View.extend({
        tagName: 'li',
        render: function () {
            this.$el.html('agent');
        }
    });

    var AgentsListView = Backbone.View.extend({
        el: $('#app'),
        template: $('#agent-list-template').html(),
        events: {
            'click a.approve-agent': 'approveAgent'
        },
        initialize: function () {
            _.bindAll(this, 'render');

            this.agents = new AgentList();
            this.agents.bind('change', this.render);
            this.agents.bind('add', this.render);
            this.agents.bind('remove', this.render);
            this.agents.bind('destroy', this.render);
            this.refresh();

        },
        refresh: function () {
            this.agents.fetch({ success: this.render });
        },
        render: function () {
            this.$el.html(_.template(this.template, this));
            return this;
        },
        approveAgent: function (event) {
            var agentHostname = $(event.target).attr('data-id');
            $.post('/api/' + agentHostname + '/approve')
                .success(function () {
                    agentsList.refresh();
                });
        }
    });

    agentsList = new AgentsListView();
    var AgentsRouter = Backbone.Router.extend({
        routes: {
            "agents/:hostname": "manageAgent",
            "*actions": "defaultRoute" // matches http://example.com/#anything-here
        },
        manageAgent: function (actions) {

        },
        defaultRoute: function (actions) {
        }
    });

    var router = new AgentsRouter;
    Backbone.history.start();
});