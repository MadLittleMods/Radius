"use strict";

require.config({

	paths: {
		coherent: 'coherent',

		jquery: 'jquery-1.10.2.min', 
		'jquery-ui': 'jquery-ui-1.10.3.min',
		'jquery-utility': 'jquery.utility',
		'jquery-validate': 'jquery.validate.min',
		'jquery-spectrum': 'jquery.spectrum',
		'jquery-color': 'jquery.color-2.1.2.min',

		'webfont': 'webfont',

		handlebars: 'handlebars-v1.3.0',
		'handlebars-runtime': 'handlebars.runtime-v1.3.0',
		hbs: 'require-hbs',
		css: 'require-css',

		renderer: 'renderer',
		modules: '../modules',
		helpers: 'helpers',

		'module-dialog': 'modules/dialog-module',

		'module-main': 'modules/main-module',
		'module-hud': 'modules/hud-module',
		'module-lobby': 'modules/lobby-module',
		'module-server-browser': 'modules/server-browser-module',
		'module-options': 'modules/options-module',


	},
	shim: {
		'jquery-ui': ['jquery'],
		'jquery-validate': ['jquery'],
		'jquery-spectrum': ['jquery', 'css!../css/spectrum'],

		handlebars: {
			exports: 'Handlebars'
		},
		'handlebars-runtime': {
			exports: 'Handlebars'
		}
		
	}


});

require(['jquery', 'coherent', 'renderer'], function($, coherent, renderer) {




	renderer.append('module-main', {}, {}, $('body'), function(attachReturn) {


		// 'hbs!../modules/multiplayer-lobby/lobby'
		//console.log(lobbyTemplate);
		//console.dir(lobbyTemplate);
		//var template_html = lobbyTemplate(); // get html from template with given context
		//$('.main-ui-layout-body').html(template_html); // Add the template to the page

		/* * /
		render('module-hud', {}, {}, $('.hud-holder'), function(attachReturn) {
			console.log('template rendered');
		});
		/* */

		/* * /
		var lobbyContext = {
			isServer: true, 
			players: [

			]
		};
		render('module-lobby', lobbyContext, {}, $('.main-ui-layout-body'), function(attachReturn) {
			console.log('template rendered');
		});
		/* */

		/* */
		var serverListContext = {
			servers: [
				{
					guid: 'sa1234ffds',
					gameName: "(fake) My Server",
					description: "so awesome",
					pwProtected: false,
					connectedPlayers: 7,
					playerLimit: 16,
					map: "Haven",
					gameType: "Territories"
				},
				{
					guid: 'sa1234ffds',
					gameName: "(fake) Another Server",
					description: "24/7 gameplay galore",
					pwProtected: true,
					connectedPlayers: 4,
					playerLimit: 8,
					map: "Blood Gulch",
					gameType: "Territories"
				},
				{
					guid: 'sa1234ffds',
					gameName: "(fake) My Server",
					description: "so awesome",
					pwProtected: false,
					connectedPlayers: 7,
					playerLimit: 16,
					map: "Haven",
					gameType: "Territories"
				},
				{
					guid: 'sa1234ffds',
					gameName: "(fake) Mike's killer server that is cool",
					description: "This is where you describe your special little server",
					pwProtected: false,
					connectedPlayers: 9,
					playerLimit: 32,
					map: "Midship",
					gameType: "Territories"
				}
				/* * /
				,
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) Another Server", description: "24/7 gameplay galore", pwProtected: true, connectedPlayers: 4, playerLimit: 8, map: "Blood Gulch", gameType: "Territories" },
				{ guid: 'sa1234ffds', gameName: "(fake) My Server", description: "so awesome", pwProtected: false, connectedPlayers: 7, playerLimit: 16, map: "Haven", gameType: "Territories" }
				/* */
			]
		};
		renderer.replace('module-server-browser', serverListContext, {}, $('.main-ui-layout-body.server-browser'), function(attachReturn) {
			console.log('template rendered');
		});
		/* */


		renderer.replace('module-options', {}, {}, $('.main-ui-layout-body.options'), function(attachReturn) {
			console.log('template rendered');
		});

	});
		
});