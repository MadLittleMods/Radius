"use strict";

define(['module', 'require', 'jquery', 'jquery-utility', 'jquery-validate', 'hbs!./server-host-config-map-option.partial'], function(module, require, $, $utility, $validate, mapOptionPartial) {


	// Host Server button
	// this actually spins up the server
	function createServer() {

		console.log(".button-start-server clicked");

		var form = $('.server-creation-form')[0];

		var server_obj = new Object();

		server_obj.server_name = $(form).find('#server-name-input-id').val();
		server_obj.lan =  $(form).find('#server-lan-input-id').is(':checked') ? "true" : "false";
		server_obj.max_players = $(form).find('#max-players-input-id').val();
		server_obj.server_description = $(form).find('#server-description-input-id').val();
		server_obj.server_pw = $(form).find('#server-pw-input-id').val();
		server_obj.server_map = $(form).find('#server-map-input-id').val();
		server_obj.server_gametype = $(form).find('#server-gametype-input-id').val();

		engine.call('GUIStartServer', server_obj).then(function() {
			console.log("Called GUIStartServer()");
		});
	};

	function refreshLevelList()
	{

		engine.call('GUIGetLevelList').then(function() {
			var levelList = arguments[0];
			console.log(levelList);
			var context = {
				'levels': levelList
			};

			$('#server-map-input-id').empty();
			$('#server-map-input-id').html(mapOptionPartial(context));

		});
	}


	function respondToNoInternetServerRegisterAttempt()
	{
		$('.server-creation-form input[name="server-lan"]').after('<div class="form-message">* You must have internet to register online. Try a LAN game.</div>')
	}

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				
				console.log('host config dialog rendered');


				// Binds the respondToNoInternetServerRegisterAttempt (js) function to this call in C#:
				// m_View.View.TriggerEvent("respondToNoInternetServerRegisterAttempt");
				engine.off('respondToNoInternetServerRegisterAttempt', respondToNoInternetServerRegisterAttempt, module.id);
				engine.on('respondToNoInternetServerRegisterAttempt', respondToNoInternetServerRegisterAttempt, module.id);



				$('.server-creation-form').validate({
					rules: {
						'server-name': {
							required: true,
							minlength: 3
						},
						'server-lan': {

						},
						'max-players': {
							required: true
						},
						'server-description': {

						},
						'server-pw': {

						},
						'server-map': {
							required: true
						},
						'server-gametype': {
							required: true
						}
					},
					submitHandler: function(form) {
						console.log('submitting host config form');
						createServer();
					}
				});


			});

		}
	};

});

