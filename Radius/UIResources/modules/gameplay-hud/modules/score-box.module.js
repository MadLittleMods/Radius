define(['require', 'hbs!./score-box/score-box', 'css!./score-box/score-box'], function(require, tmpl, css1) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['./score-box/score.bits'], function(scoreBox) {
				scoreBox.bind(elements);
			});
			
		}
	};
});