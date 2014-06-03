define(['require', 'hbs!./map-item/map-item', 'css!./map-item/map-item'], function(require, tmpl, css) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['./map-item/map-item.bits'], function(mapBox) {
				mapBox.bind(elements);
			});
			
		}
	};
});