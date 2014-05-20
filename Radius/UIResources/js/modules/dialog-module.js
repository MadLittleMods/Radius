define(['module-main', 'hbs!modules/generic/dialog/dialog', 'css!modules/generic/dialog/dialog'], function(moduleMain, tmpl, css) {
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['modules/generic/dialog/dialog.bits'], function(dialog) {
				dialog.bind(elements);
			});
		}
	};
});