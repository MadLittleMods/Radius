define(['hbs!modules/main-ui/main', 'css!../css/base', 'css!../css/icon-font', 'css!modules/main-ui/main-ui'], function(tmpl, basecss, iconcss, maincss) {
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['modules/main-ui/main.bits'], function(mainmodule) {
				mainmodule.bind(elements);
			});
		}
	};
});