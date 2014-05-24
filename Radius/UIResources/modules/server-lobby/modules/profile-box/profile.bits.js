"use strict";

define(['jquery', 'jquery-utility'], function($, $utility) {

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				// Bind the profile close button
				$(restrictTo).filterFind('.button-profile-close').on('click', function() {
					var $item = $(restrictTo).filterFind('.main-content-box:not(.remove-item)');
					$item.addClass('remove-item');
					$item.on('animationend webkitAnimationEnd oAnimationEnd MSAnimationEnd', function() {
						$(restrictTo).remove();
					});
				});
				// Make the color input
				$(restrictTo).filterFind('.profile-edit-form .gamertag-color-input').each(function() {
					var $inputThis = $(this);
					$inputThis.spectrum({
						color: (function() {
							// Make an async call to get the player color
							var guid = $(restrictTo).filterFind('.profile-edit-form .guid').val();
							engine.call('GUIGetPlayerData', guid ? guid : "").then(function(playerData) {
								// update the input
								//console.log(playerData);
								var color255 = UnityUtilities.Color1ToColor255(playerData.TeamColor);
								var color = $.Color(color255.r, color255.g, color255.b, color255.a);

								// Update the spectrum color pickers
								$inputThis.spectrum("set", color.toHexString());
							});

							// default to red while we are waiting on the async call
							return "#f00";
						})(),
						clickoutFiresChange: true,
						showButtons: false,
						showColor: false,
						change: function(color) {
							console.log(color.toRgb());
							var guid = $(restrictTo).filterFind('.profile-edit-form .guid').val();
							engine.call('GUIProfileColorChange', guid ? guid : "", color.toRgb()).then(function() {
								console.log("Called GUIProfileColorChange()");
							});
						},
						move: function(color) {
						    console.log(color.toRgb());
						}
					});
				});

				



				// Bind the gamertag/username form
				$(restrictTo).filterFind('.profile-edit-form .gamertag-input').on('input propertychange change', function(data) {
					//console.log($(this).val());
					//console.log(data);

					var guid = $(restrictTo).filterFind('.profile-edit-form .guid').val();
					var newGamertag = $(this).val();
					engine.call('GUIProfileGamertagChange', guid ? guid : "", newGamertag ? newGamertag : "").then(function() {
						console.log("Called GUIProfileGamertagChange()");
					});
				});

				
			});

		}
	};

});