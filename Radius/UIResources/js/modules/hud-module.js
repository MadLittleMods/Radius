define(['module-main', 'hbs!modules/gameplay-hud/hud', 'modules/gameplay-hud/hud.bits', 'css!modules/gameplay-hud/hud', 'css!modules/gameplay-hud/territory-progress'], function(mainModule, tmpl, hudScript, css2, css3) {
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			hudScript.bind(elements);
		}
	};
});