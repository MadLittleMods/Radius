define(['require', 'hbs!./profile-box/profile-box'], function(require, tmpl) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['./profile-box/profile.bits'], function(profileBox) {
				profileBox.bind(elements);
			});
			
		}
	};
});