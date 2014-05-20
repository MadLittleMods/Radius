"use strict";

define(['jquery', 'jquery-utility', 'jquery-ui'], function($, $utility, $ui) {

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				$(restrictTo).filterFind('.dialog-exit').on('click', function() {
					console.log("exit dialog");

					var $item = $(restrictTo).filterFind('.dialog-box:not(.remove-item)');
					$item.addClass('remove-item');
					$item.on('animationend webkitAnimationEnd oAnimationEnd MSAnimationEnd', function() {
						$(restrictTo).remove();
					});
				});

				$(restrictTo).filterFind('.draggable').draggable();
			});

		}
	};

});