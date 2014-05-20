define(['require', 'hbs!./map-box/map-box', 'css!./map-box/map-box'], function(require, tmpl, css) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['./map-box/map-box.bits'], function(mapBox) {
				mapBox.bind(elements);
			});
			
		}
	};
});