"use strict";

define(['module', 'require', 'jquery', 'jquery-utility'], function(module, require, $, $utility) {

	var $gameTimeBox;
	var countDownIntervalId;


	function secondsToTime(secs)
	{
		var hours = Math.floor(secs / (60 * 60));

		var divisor_for_minutes = secs % (60 * 60);
		var minutes = Math.floor(divisor_for_minutes / 60);
	 
		var divisor_for_seconds = divisor_for_minutes % 60;
		var seconds = Math.ceil(divisor_for_seconds);

		var obj = {
			"h": hours,
			"m": minutes,
			"s": seconds
		};
		return obj;
	}

	function updateGameTime(time)
	{
		console.log("Updating game time: " + time);
		var fTime = secondsToTime(time);
		$gameTimeBox.attr('data-time-seconds', (time-1) > 0 ? (time-1) : 0);
		$gameTimeBox.attr('data-time', (time > 0) ? ((fTime.h > 0 ? fTime.h + ":" : "") + fTime.m + ":" + fTime.s) : "inf");

		$gameTimeBox.css('z-index', 1); // Cause a repaint. For some reason the pseudo element is not working without a repaint
	}

	function requestGameTime()
	{
		engine.call('GUIGetGameTime').then(function(time) {
			updateGameTime(time);
		});
	}

	function startCountDownGameTime()
	{
		// Every Second
		countDownIntervalId = setInterval(function() {
			// Subtract a second from the time
			// Keep it above 0
			var attrTime = parseFloat($gameTimeBox.attr('data-time-seconds'));
			updateGameTime(attrTime);

		}, 1000);
	}

	function stopCountDownGameTime()
	{
		clearInterval(countDownIntervalId);
	}

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				$gameTimeBox = $(restrictTo).filterFind('.game-time-box');

				requestGameTime();
				startCountDownGameTime();

				// Binds the updateGameTimeBox (js) function to this call in C#:
				// m_View.View.TriggerEvent("updateGameTimeBox", float time);
				engine.off('updateGameTimeBox', updateGameTime, module.id);
				engine.on('updateGameTimeBox', updateGameTime, module.id);

				// Binds the startCountDownGameTime (js) function to this call in C#:
				// m_View.View.TriggerEvent("startCountDownGameTime");
				engine.off('startCountDownGameTime', startCountDownGameTime, module.id);
				engine.on('startCountDownGameTime', startCountDownGameTime, module.id);

				// Binds the stopCountDownGameTime (js) function to this call in C#:
				// m_View.View.TriggerEvent("stopCountDownGameTime");
				engine.off('stopCountDownGameTime', stopCountDownGameTime, module.id);
				engine.on('stopCountDownGameTime', stopCountDownGameTime, module.id);


			});

		}
	};

});