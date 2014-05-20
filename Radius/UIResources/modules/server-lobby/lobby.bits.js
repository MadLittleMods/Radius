"use strict";

define([
	'module',
	'require',
	'renderer',
	'jquery', 
	'jquery-utility', 
	'jquery-spectrum', 
	'jquery-color',
	'handlebars'
], function(
	module,
	require,
	renderer,
	$, 
	$utility, 
	$spectrum, 
	$color,
	Handlebars
) {
		

	// Update the users color in the list
	function updateUserColor(pData) {
		function colorSameAlpha(value) {
			//console.log('value: ' + value);
			var alpha = alphaFromRGBA(value);
			//console.log(pData.TeamColor);
			var color255 = UnityUtilities.Color1ToColor255(pData.TeamColor);
			//var color255 = pData.TeamColor;
			color255.a = alpha;
			//console.log(color255);
			//console.log("user color: " + $.Color(color255.r, color255.g, color255.b, color255.a).toRgbaString());
			return $.Color(color255.r, color255.g, color255.b, color255.a).toRgbaString();
		}

		$('.gamertag-box.player-' + pData.guid).css({
			'background-color': function(index, value) {
				return colorSameAlpha(value);
			},
			'border-top-color': function(index, value) {
				return colorSameAlpha(value);
			},
			'border-bottom-color': function(index, value) {
				return colorSameAlpha(value);
			},
			'border-right-color': function(index, value) {
				return colorSameAlpha(value);
			},
			'border-left-color': function(index, value) {
				return colorSameAlpha(value);
			}
		});
	}

	function updateUserGamertag(pData) {
		$('.gamertag-box.player-' + pData.guid).html(pData.Gamertag);
	}

	// Add player to the lobby list
	function lobbyAddPlayer(pData, elementAppendTo)
	{
		// elementAppendTo: optional

		console.log("Adding player to gui: " + pData.guid);

		//console.log(pData);
		// setup the template context
		var playerContext = {
			me: pData.IsMine,
			guid: pData.guid,
			gamertag: pData.Gamertag,
			team: pData.TeamString
		};

		

		// If we are using the optional `elementAppendTo` parameter then use it, otherwise default
		var otherPlayerOnTeamElement = ($(elementAppendTo).length > 0) ? elementAppendTo : $('.gamertag-container .gamertag-box.' + pData.TeamString).last();
		// If there is already someone on the same team as the player we are adding
		// Add them right after that person so that we can group the teams
		if($(otherPlayerOnTeamElement).length > 0)
		{
			renderer.after('./modules/gamertag-box.module', playerContext, {}, otherPlayerOnTeamElement, function(attachReturn) {
				// ...
				updateUserColor(pData);
			}, require);
		}
		// Otherwise we just place the player at the end
		else
		{
			renderer.append('./modules/gamertag-box.module', playerContext, {}, $('.gamertag-container'), function(attachReturn) {
				// ...
				updateUserColor(pData);
			}, require);
		}

		// Now update the party size
		updatePartySize();
		
	}


	// Update player in the list
	function lobbyUpdatePlayer(pData)
	{
		console.log("Updating player: " + pData.guid);
		//console.log(pData);

		var playerBox = $('.gamertag-box.player-' + pData.guid);
		// If the scorebox already exists
		if(playerBox.length > 0)
		{
			// Change the isMine status
			if(pData.IsMine)
				$(playerBox).addClass('me');
			else
				$(playerBox).removeClass('me');

			updateUserColor(pData);
			updateUserGamertag(pData);
		}
		else
		{
			lobbyAddPlayer(pData);
		}


	}

	// Remove player from lobby list (maybe they left...)
	function lobbyRemovePlayer(guid)
	{
		console.log("Removing player from gui: " + guid);
		$('.gamertag-box.player-' + guid).remove();

		// Now update the party size
		updatePartySize();
	}


	function updatePartySize()
	{
		engine.call('GUIGetPartySize').then(function(partyDetails) {
			$('.party-size').html(partyDetails.connectedPlayers + "/" + partyDetails.playerLimit)
		});
	}

	function respondToStartGame()
	{
		console.log('responding to start game');

		renderer.replace('module-hud', {}, {}, $('.hud-holder'), function(attachReturn) {
			console.log('hud template rendered');
		});

		window.lockUI = false;
		window.hideUI(true);

		// Change the start/end button
		updateGameStatus(true);
	}

	function respondToEndGame()
	{
		window.lockUI = true;
		window.showUI(true);

		// Cleanup the hud
		//requirejs.undef('module-hud'); // Doesn't clean up dependencies
		$('.hud-holder').empty();

		// Change the start/end button
		updateGameStatus(false);
	}

	function updateGameStatus(gameStarted)
	{
		if(gameStarted)
		{
			// If the game has started
			// Change the start game button to end game
			$('.lobby-module .button-start-game').removeClass('button-start-game').addClass('button-end-game').html('End Game');
		}
		else
		{
			// If the game has ended or not started
			// Change the end game button to start game
			$('.lobby-module .button-end-game').removeClass('button-end-game').addClass('button-start-game').html('Start Game');
		}
	}

	function updatePlayerList()
	{
		// Populate the list with whoever is already in the player list when you join
		$('.gamertag-container').empty();
		engine.call('GUIAddAllToPlayerList');
	}



	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);

			$(document).ready(function() {

				// Binds the lobbyAddPlayer (js) function to this call in C#:
				// m_View.View.TriggerEvent("lobbyAddPlayer", PlayerData);
				//var contextBindingKey = (new Date).getTime();
				//engine.off('lobbyAddPlayer', lobbyAddPlayer, module.id);
				//engine.on('lobbyAddPlayer', lobbyAddPlayer, module.id);

				// Binds the lobbyUpdatePlayer (js) function to this call in C#:
				// m_View.View.TriggerEvent("lobbyUpdatePlayer", PlayerData);
				engine.off('lobbyUpdatePlayer', lobbyUpdatePlayer, module.id);
				engine.on('lobbyUpdatePlayer', lobbyUpdatePlayer, module.id);

				// Binds the lobbyRemovePlayer (js) function to this call in C#:
				// m_View.View.TriggerEvent("lobbyRemovePlayer", guid);
				engine.off('lobbyRemovePlayer', lobbyRemovePlayer, module.id);
				engine.on('lobbyRemovePlayer', lobbyRemovePlayer, module.id);

				// Binds the respondToStartGame (js) function to this call in C#:
				// m_View.View.TriggerEvent("respondToStartGame");
				engine.off('respondToStartGame', respondToStartGame, module.id);
				engine.on('respondToStartGame', respondToStartGame, module.id);

				// Binds the respondToEndGame (js) function to this call in C#:
				// m_View.View.TriggerEvent("respondToEndGame");
				engine.off('respondToEndGame', respondToEndGame, module.id);
				engine.on('respondToEndGame', respondToEndGame, module.id);



				// Binds the updatePlayerList (js) function to this call in C#:
				// m_View.View.TriggerEvent("updatePlayerList");
				engine.off('updatePlayerList', updatePlayerList, module.id);
				engine.on('updatePlayerList', updatePlayerList, module.id);




				// We need to populate the list with whoever is already in the player list when you join
				updatePlayerList();

				// Also initialize the party size
				updatePartySize();



				$('.lobby-module.isServer').on('click', '.button-start-game', function() {
					console.log('button start-game clicked');
					engine.call('GUIStartGame');
				});

				$('.lobby-module.isServer').on('click', '.button-end-game', function() {
					console.log('button end-game clicked');
					engine.call('GUIEndGame');
				});


				$('.lobby-module.isServer').on('click', '.button-change-map', function() {
					// Remove the boxes so we don't have multiple
					$('.map-content-box').remove();

					renderer.before('./modules/map-box.module', {}, {}, $('.lobby-buttons-content-box'), function(attachReturn) {
						// ...
					}, require);
				});

			});
		}

	};

});
