(function ($) {
    var PackageModel = Backbone.Model.extend({
        defaults: {
            id: 'package.id',
            version: '1.0.0.0',
            something: 'something'
        }
    });

    function makePackage(id) {
        var package = new PackageModel();
        package.set({ id: id });
        return package;
    }

    /* MODELS */
    var Agent = Backbone.Model.extend({
        defaults: {
            id: 'agent.hostname.',
            tags: 'the, agent, tags',
            packages: [makePackage('package.1'), makePackage('package.2'), makePackage('package.3')]
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
                packages: this.model.get('packages')
            };
            var template = _.template($("#agent-row-template").html(), viewModel);
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
            'click button#add': 'addItem'
        },

        initialize: function () {
            _.bindAll(this, 'render');

            this.collection = new AgentList();
            this.collection.bind('add', this.appendItem);
            this.collection.bind('change', this.render);
            this.collection.bind('destroy', this.render);
            this.collection.fetch({ add: true });

            this.counter = 0;
            this.render();
        },

        render: function () {
            var self = this;
            $(this.el).html('');
            $(this.el).append("<input name='hostname' id='new-agent-hostname' type='textbox' />");
            $(this.el).append("<button id='add'>Add Agent</button>");
            $(this.el).append("<ul></ul>");
            _(this.collection.models).each(function (item) {
                self.appendItem(item);
            }, this);
        },

        addItem: function () {
            this.counter++;

            var item = new Agent();
            item.set({
                id: $('#new-agent-hostname').val()
            });
            item.save();
            this.collection.add(item);
            this.render();
        },

        appendItem: function (item) {
            var agentView = new AgentView({
                model: item
            });
            $('ul', this.el).append(agentView.render().el);
        }
    });

    var listView = new ListView();
})(jQuery);

$(document).ready(function () {
});