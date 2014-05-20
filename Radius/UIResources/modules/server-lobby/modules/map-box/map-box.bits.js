"use strict";

define(['require', 'jquery', 'jquery-utility', 'hbs!./map-item.partial', ], function(require, $, $utility, mapItemTmpl) {


	function getLevels()
	{
		engine.call('GUIGetLevelList').then(function(levelList) {
			console.log(levelList);

			var levelContext = {
				'levels': levelList
			};

			$('.map-content-box .map-holder').empty();
			$('.map-content-box .map-holder').append(mapItemTmpl(levelContext));
		});
	}

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function() {
				
				getLevels();


				// Bind the map close button
				$(restrictTo).filterFind('.button-maps-close').on('click', function() {
					var $item = $(restrictTo).filterFind('.main-content-box:not(.remove-item)');
					$item.addClass('remove-item');
					$item.on('animationend webkitAnimationEnd oAnimationEnd MSAnimationEnd', function() {
						$(restrictTo).remove();
					});
				});


			});

		}
	};

});