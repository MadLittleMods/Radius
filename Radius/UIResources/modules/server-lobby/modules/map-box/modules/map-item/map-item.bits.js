"use strict";

define(['require', 'jquery', 'jquery-utility'], function(require, $, $utility) {


	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function() {

				$(restrictTo).filterFind('.map-item').on('click', function() {
					if($(this).hasClass('selected'))
						$(this).toggleClass('selected');
					else
					{
						$('.map-content-box .map-holder .map-item').removeClass('selected');
						$(this).addClass('selected');
					}
				});


			});

		}
	};

});