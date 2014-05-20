define(['module-main', 'hbs!modules/server-browser/server-browser', 'css!modules/server-browser/server-browser'], function(moduleMain, tmpl, css) {
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['modules/server-browser/server.bits'], function(browser) {
				browser.bind(elements);
			});
		}
	};
});