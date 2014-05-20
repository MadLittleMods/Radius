define(['module-main', 'hbs!modules/server-lobby/lobby', 'css!modules/server-lobby/lobby'], function(mainModule, tmpl, css2) {
	//console.log(tmpl({}));
	return {
		template: tmpl,
		attach: function(options, elements) {
			// You can do stuff here
			// even return some methods
			require(['modules/server-lobby/lobby.bits'], function(lobby) {
				lobby.bind(elements);
			});
			
		}
	};
});