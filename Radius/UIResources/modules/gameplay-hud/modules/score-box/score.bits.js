"use strict";

define(['require', 'jquery', 'jquery-utility', 'css!score-box'], function(require, $, $utility, scorecss) {


	

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				function requestScoreMax()
				{
					engine.call('GUIGetScoreMax').then(function(scoreMax) {
						$(restrictTo).filterFind('.score-box').attr('data-score-max', scoreMax);
					});
				}
							
				requestScoreMax();

				/*
				$(restrictTo).filterFind('.score-box.me').on('click', function() {

				});
				*/


			});

		}
	};

});