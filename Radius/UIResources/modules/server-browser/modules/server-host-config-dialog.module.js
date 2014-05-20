define(['require', 'handlebars', 'jquery', 'module-dialog', 'hbs!./server-host-config-dialog/server-host-config-dialog'], function(require, handlebars, $, dialogModule, tmpl) {
	//console.log(tmpl({}));

	

	return {
		// We are basically extending the module-dialog
		template: function(context) {
			var dialogContext = {
				title: 'Host Configuration', 
				dClass: 'host-config-dialog',
				body: tmpl(context)
			};
			return dialogModule.template($.extend({}, context, dialogContext));
		},
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods

			// Make sure we call this as well
			dialogModule.attach(options, elements);

			require(['./server-host-config-dialog/server-host-config-dialog.bits'], function(dialog) {
				dialog.bind(elements);
			});
			
		}
	};
});