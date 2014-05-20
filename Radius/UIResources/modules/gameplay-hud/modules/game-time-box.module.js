"use strict";

define(['require', 'hbs!./game-time-box/game-time-box', 'css!./game-time-box/game-time-box'], function(require, tmpl, css1) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['./game-time-box/game-time.bits'], function(timeBox) {
				timeBox.bind(elements);
			});
			
		}
	};
});