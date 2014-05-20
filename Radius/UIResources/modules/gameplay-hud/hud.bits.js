"use strict";

define([
	'module',
	'renderer',
	'require',
	'jquery', 
	'jquery-utility', 
	'jquery-color'
], function(
	module,
	renderer,
	require,
	$, 
	$utility,
	$color
) {



	function updateTerritoryProgress(territoryData)
	{
		// Change the liquid width to represent the progress
		$('.territory-progress-liquid').css('width', (territoryData.Progress*100).clamp(0, 100) + '%');
	}

	function updateTerritoryColor(territoryColor)
	{
		/*
		var color255 = {
			r: Math.round(territoryColor.r*255),
			g: Math.round(territoryColor.g*255),
			b: Math.round(territoryColor.b*255),
			a: territoryColor.a
		};
		*/
		var color255 = UnityUtilities.Color1ToColor255(territoryColor);
		//var color255 = territoryColor;

		var originalColor = $.Color(color255.r, color255.g, color255.b, color255.a);
		var liquidHolderColor = originalColor.saturation(originalColor.saturation() != 0 ? .39 : 0).lightness(.69);
		var liquidColor = originalColor.saturation(originalColor.saturation() != 0 ? .66 : 0).lightness(.70);
		var liquidBorderColor = originalColor.saturation(originalColor.saturation() != 0 ? .92 : 0).lightness(.28);

		//console.log("updating liquid color: " + $.Color(color255.r, color255.g, color255.b, color255.a).toRgbaString());

		$('.territory-progress').css({
			'background-color': liquidHolderColor.toRgbaString(),
			'border-color': liquidBorderColor.toRgbaString()
		});

		$('.territory-progress-liquid').css({
			'background-color': liquidColor.toRgbaString(),
			'border-color': liquidBorderColor.toRgbaString()
		});

	}

	function showTerritoryProgress(territoryData)
	{
		console.log("Showing GUI Progress");
		updateTerritoryProgress(territoryData);
		updateTerritoryColor(territoryData.Color);
		$('.territory-progress').show();
	}
	function hideTerritoryProgress()
	{
		console.log("Hiding GUI Progress");
		$('.territory-progress').hide();
	}

	function updateTerritory(territoryData)
	{
		updateTerritoryProgress(territoryData);
		updateTerritoryColor(territoryData.Color);
	}



	function updateScoreBoxMax(maxValue)
	{
		// Update all score boxs with the new max
		$('.score-box').attr('data-score-max', maxValue);
	}

	function updateScoreBoxColor(pData)
	{
		var color255 = UnityUtilities.Color1ToColor255(pData.TeamColor);

		$('.score-box.player-' + pData.guid).css({
			'background-color': function(index, value) {
				var alpha = alphaFromRGBA(value);
				console.log("background color alpha: " + alpha);
				return $.Color(color255.r, color255.g, color255.b, alpha).lightness(.39).toRgbaString();
			}
		});
	}

	function updateScoreBoxWidth(guid)
	{
		$('.score-box.player-' + guid + ' .score-liquid').css({
			'width':  function(index, value) {
				var asdf = ($(this).closest('.score-box.player-' + guid).attr('data-score')/$(this).closest('.score-box.player-' + guid).attr('data-score-max')*100).clamp(0, 100) + '%';
				console.log(asdf);
				return asdf;
			}
		});
	}

	function updateScoreBox(sData)
	{
		//console.log(sData);
		var scoreBox = $('.score-box.player-' + sData.guid);
		// If the scorebox already exists
		if(scoreBox.length > 0)
		{
			console.log("Updating Score Box");
			// Just Update the score
			$(scoreBox).attr('data-score', sData.value);
			$(scoreBox).css('z-index', $(scoreBox).css('z-index')); // Cause a repaint. For some reason the pseudo element is not working without a repaint

			// Update ScoreBox liquid width
			updateScoreBoxWidth(sData.guid);
		}
		else
		{
			console.log("Adding Score Box");

			// Otherwise we need to add the box
			var scoreBoxContext = {
				me: sData.IsMine,
				guid: sData.guid,
				score: sData.value,
			};

			renderer.append('./modules/score-box.module', scoreBoxContext, {}, $('.score-holder'), function(attachReturn) {
				// Update ScoreBox liquid width
				updateScoreBoxWidth(sData.guid);

				// Change the scorebox color
				var playerData;
				engine.call('GUIGetPlayerData', sData.guid).then(function(playerData) {
					//console.log(playerData);
					
					updateScoreBoxColor(playerData);
				});
			}, require);


		}
	}




	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function(){

				// Binds the updateTerritoryProgress (js) function to this call in C#:
				// m_View.View.TriggerEvent("updateTerritoryProgress", TerritoryData);
				engine.off('territoryUpdated', updateTerritory, module.id);
				engine.on('territoryUpdated', updateTerritory, module.id);

				// Binds the showTerritoryProgress (js) function to this call in C#:
				// m_View.View.TriggerEvent("showTerritoryProgress", TerritoryData);
				engine.off('showTerritoryProgress', showTerritoryProgress, module.id);
				engine.on('showTerritoryProgress', showTerritoryProgress, module.id);

				// Binds the hideTerritoryProgress (js) function to this call in C#:
				// m_View.View.TriggerEvent("hideTerritoryProgress");
				engine.off('hideTerritoryProgress', hideTerritoryProgress, module.id);
				engine.on('hideTerritoryProgress', hideTerritoryProgress, module.id);


				// Binds the updateScoreBox (js) function to this call in C#:
				// m_View.View.TriggerEvent("updateScoreBox", ScoreData);
				engine.off('updateScoreBox', updateScoreBox, module.id);
				engine.on('updateScoreBox', updateScoreBox, module.id);

				// Binds the updateScoreBoxMax (js) function to this call in C#:
				// m_View.View.TriggerEvent("updateScoreBoxMax", float max);
				engine.off('updateScoreBoxMax', updateScoreBoxMax, module.id);
				engine.on('updateScoreBoxMax', updateScoreBoxMax, module.id);


				// Binds the lobbyUpdatePlayer (js) function to this call in C#:
				// m_View.View.TriggerEvent("hudUpdatePlayer", PlayerData);
				engine.off('hudUpdatePlayer', updateScoreBoxColor, module.id);
				engine.on('hudUpdatePlayer', updateScoreBoxColor, module.id);


				hideTerritoryProgress();

				
				// Add the game time box to the screen
				renderer.prepend('./modules/game-time-box.module', {}, {}, $(restrictTo).filterFind('.score-holder'), function(attachReturn) {
					console.log('game time box template rendered');
				}, require);

			});

		}
	};

});