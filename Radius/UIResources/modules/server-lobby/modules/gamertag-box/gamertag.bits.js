"use strict";

define(['require', 'jquery', 'jquery-utility'], function(require, $, $utility) {

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				
				$(restrictTo).filterFind('.gamertag-box').on('click', function() {
					// Check if you are clicking your own box
					// You can only edit your own stuff
					if($(this).hasClass('me'))
					{
						console.log($(this).html() + ' clicked');

						var profileContext = {
							guid: $(this).attr('data-guid'),
							gamertag: $(this).html()
						};
						// Remove the boxes so we don't have multiple
						$('.profile-content-box').remove();

						renderer.after('../profile-box.module', profileContext, {}, $('.lobby-party-content-box'), function(attachReturn) {
							// ...
						}, require);
					}
				});


			});

		}
	};

});