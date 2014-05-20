define(['module-main', 'hbs!modules/game-options/options', 'css!modules/game-options/options'], function(moduleMain, tmpl, css) {
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['modules/game-options/options.bits'], function(gameOpts) {
				gameOpts.bind(elements);
			});
		}
	};
});