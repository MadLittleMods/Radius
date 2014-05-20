define(['require', 'hbs!./gamertag-box/gamertag-box'], function(require, tmpl) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['./gamertag-box/gamertag.bits'], function(gamertagBox) {
				gamertagBox.bind(elements);
			});
			
		}
	};
});