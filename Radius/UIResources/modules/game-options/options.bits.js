define(['jquery', 'jquery-utility'], function($, $utility) {

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){
				// Bind the master volume controls
				$(restrictTo).filterFind('input[type="range"].master_music_volume').change(function () {
					//console.log("GUI Change master music volume: " + $(this).val());
					engine.call('GUISetMasterVolume', 'music', parseFloat($(this).val()));
				});
				$(restrictTo).filterFind('input[type="range"].master_soundeffect_volume').change(function () {
					engine.call('GUISetMasterVolume', 'soundeffect', parseFloat($(this).val()));
				});

				// Set the input to the last saved value
				engine.call('GetMasterVolume', 'Music').then(function() {
					$(restrictTo).filterFind('input[type="range"].master_music_volume').attr('value', arguments[0]);
				});
				engine.call('GetMasterVolume', 'SoundEffect').then(function() {
					$(restrictTo).filterFind('input[type="range"].master_soundeffect_volume').attr('value', arguments[0]);
				});
			});

		}
	};

});