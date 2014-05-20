"use strict";

define([
	'module',
	'require',
	'renderer',
	'jquery', 
	'jquery-utility', 
	'jquery-validate',
	'hbs!./server-list-row.partial', 
	'hbs!modules/server-browser/server-pw-dialog-content.partial',
	'hbs!modules/main-ui/main-nav-item.partial'
], function(
	module, 
	require,
	renderer,
	$, 
	$utility, 
	$validate,
	serverListRowTemplate, 
	pwDialogContent,
	mainNavItemTemplate
) {

	

	function newServerBrowserMessage(message)
	{
		$('.notication-block').html(message);
	}

	// Generate a pw dialog box
	function makePWDialogBox(guid)
	{
		/* */
		var dialogTemplateHTML = pwDialogContent({ 
			'guid': guid 
		});
		var dialogContext = {
			//title: 'asdf',  
			dClass: 'pw-dialog',
			body: dialogTemplateHTML
		};
		renderer.append('module-dialog', dialogContext, {}, $('body'), function(attachReturn) {
			console.log('pw dialog rendered');
		});
		/* */
	}

	// Generate a pw dialog box
	function makeHostConfigDialogBox()
	{
		engine.call('GUIGetLevelList').then(function(levelList) {
			console.log(levelList);
			var dialogContext = {
				'levels': levelList
			};

			/* */
			renderer.append('./modules/server-host-config-dialog.module', dialogContext, {}, $('body'), function(attachReturn) {
				console.log('rendered host config dialog');
			}, require);
			/* */
		});
	}

	// Call this function to refresh the server list
	function refreshServerList()
	{
		console.log('Refreshing Server list');
		engine.call('GUIRefreshServerList').then(function() {
			console.log("Called GUIRefreshServerList()");
		});
	}

	function updateServerList() {
		
		$('.server-list .server-list-body').empty();

		var serverData = arguments[0];
		var numAppendedServers = 0;

		if(serverData.length > 0) {
			for (var i = 0; i < serverData.length; i++) 
			{
				//console.log(serverData[i]);
				if(serverData[i].status.AsString.toLowerCase() == "open") {
					//$('#server-list').append('<tr><td>' + arguments[0][i].gameName + '</td><td>' + arguments[0][i].connectedPlayers + '/' + arguments[0][i].playerLimit + '</td></tr>')
					var serverContext = {
						guid: serverData[i].guid,
						gameName: serverData[i].serverName,
						description: serverData[i].description,
						isLan: serverData[i].isLan,
						pwProtected: serverData[i].pwProtected,
						connectedPlayers: serverData[i].connectedPlayers,
						playerLimit: serverData[i].playerLimit,
						map: serverData[i].map,
						gameType: serverData[i].gameType
					};
					//console.log(serverContext);
					$('.server-list .server-list-body').append(serverListRowTemplate(serverContext));
					numAppendedServers++;
				}
			}
		}
		
		if(numAppendedServers <= 0)
			$('.server-list .server-list-body').append('<div class="server-row"><div class="server-row-item">No servers available...</div></div>');
	}


	
	function respondToServerRegisterAttempt(pass, message)
	{
		newServerBrowserMessage(message); // Always show the return message

		// If the server is registered then we should hide the menus
		if(pass)
		{
			$('.host-config-dialog').remove(); // Remove the start server dialog 


			// Add tab
			$(mainNavItemTemplate({title: "Lobby", tabId: "server-lobby" })).insertAfter('.main-nav .main-nav-item.server-browser');
			// Switch to that tab
			switchTabs('server-lobby');
			// Render content in that tab
			renderer.replace('module-lobby', { isServer: true }, {}, $('.main-ui-layout-body.server-lobby'), function(attachReturn) {
				console.log('lobby template rendered');
			});
			//$('.main-ui-layout-body.server-browser').hide();
			//$('.main-ui-layout-body.server-lobby').show();
			//hideUI(); // hide the menus

			refreshServerList(); // Also refresh the server list to add the server we just created.
		}
	}


	


	function respondToServerConnectSuccess()
	{
		// If we connect successfully
		// Close the pw dialog
		$('.pw-dialog').remove();

		// Add tab
		$(mainNavItemTemplate({title: "Lobby", tabId: "server-lobby" })).insertAfter('.main-nav .main-nav-item.server-browser');
		// Switch to that tab
		switchTabs('server-lobby');
		// Render content in that tab
		renderer.replace('module-lobby', { isClient: true }, {}, $('.main-ui-layout-body.server-lobby'), function(attachReturn) {
			console.log('lobby template rendered');
		});
	}


	function updateConnectedStatus(connected)
	{
		if(connected)
		{
			console.log('Changed connect button to disconnect button');
			$('.server-browser .button-connect').removeClass('button-connect').addClass('button-disconnect').html('Disconnect');
		}
		else
		{
			console.log('Changed disconnect button to connect button');
			$('.server-browser .button-disconnect').addClass('button-connect').removeClass('button-disconnect').html('Connect');
		}
		
	}

	function respondToServerDisconnect()
	{
		// Remove that tab from the nav bar
		// And clear out that layout body
		console.log('respondToServerDisconnect() called');
		window.clearTab('server-lobby');
		window.switchTabs('server-browser');
	}


	/* ---------------------------------------------------------------------------------------------------------------
	 * ---------------------------------------------------------------------------------------------------------------
	 * --------------------------------------------------------------------------------------------------------------- */

	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);


			$(document).ready(function() {
				
				// Binds the newServerBrowserMessage (js) function to this call in C#:
				// m_View.View.TriggerEvent("newServerBrowserMessage", string message);
				engine.off('newServerBrowserMessage', newServerBrowserMessage, this);
				engine.on('newServerBrowserMessage', newServerBrowserMessage, this);

				// Binds the updateServerList (js) function to this call in C#:
				// m_View.View.TriggerEvent("updateServerList", HostData...);
				engine.off('updateServerList', updateServerList, this);
				engine.on('updateServerList', updateServerList, this);

				// Binds the respondToServerRegisterAttempt (js) function to this call in C#:
				// m_View.View.TriggerEvent("respondToServerRegisterAttempt", bool pass, string message);
				engine.off('respondToServerRegisterAttempt', respondToServerRegisterAttempt, this);
				engine.on('respondToServerRegisterAttempt', respondToServerRegisterAttempt, this);

				// Binds the respondToServerConnectSuccess (js) function to this call in C#:
				// m_View.View.TriggerEvent("respondToServerConnectSuccess");
				engine.off('respondToServerConnectSuccess', respondToServerConnectSuccess, this);
				engine.on('respondToServerConnectSuccess', respondToServerConnectSuccess, this);

				// Binds the updateConnectedStatus (js) function to this call in C#:
				// m_View.View.TriggerEvent("updateConnectedStatus", bool connected);
				engine.off('updateConnectedStatus', updateConnectedStatus, this);
				engine.on('updateConnectedStatus', updateConnectedStatus, this);

				// Binds the respondToServerDisconnect (js) function to this call in C#:
				// m_View.View.TriggerEvent("respondToServerDisconnect");
				engine.off('respondToServerDisconnect', respondToServerDisconnect, this);
				engine.on('respondToServerDisconnect', respondToServerDisconnect, this);

				// Binds the testTrigger (js) function to this call in C#:
				// m_View.View.TriggerEvent("testTrigger");
				function testTriggerLog() { console.log("testTrigger event fired."); }
				engine.off('testTrigger', testTriggerLog, this);
				engine.on('testTrigger', testTriggerLog, this);

				




				// Refresh server list button
				$(restrictTo).filterFind('.button-refresh-server-list').on('click', function() {
					console.log('.button-refresh-server-list clicked');
					refreshServerList();
				});



				// Add the ability to select server
				$(restrictTo).filterFind('.server-list-body').on('click', '.server-row', function() {
					if($(this).hasClass('selected'))
						$(this).toggleClass('selected');
					else
					{
						$('.server-list-body .server-row').removeClass('selected');

						$(this).addClass('selected');
					}
				});

				// Connect to Selected server
				$('body').on('click', '.button-connect', function(e) {
					console.log(".button-connect clicked");

					// If the connect button has a guid attribute on it,
					// then we will honor that
					if($(this).attr('data-guid'))
					{
						var guid = $(this).attr('data-guid');

						// If the pw attribute is set then use it otherwise
						// we look for the pw-input attribute which points to an input element, otherwise
						// we leave the password blank
						var pw = $(this).attr('data-pw') ? $(this).attr('data-pw') : ($(this).attr('data-pw-input') ? $($(this).attr('data-pw-input')).val() : '');
						console.log('password: ' + pw);
						engine.call('GUIConnect', guid, pw);
					}
					// Otherwise we connect to the one selected in the list
					else
					{
						var server = $('.server-list .selected').first();
						if(server.length)
						{
							guid = server.attr('data-guid');
							if(guid)
							{
								// If pw protected, bring up the dialog
								if(server.attr('data-pw-protected') != "false")
								{
									makePWDialogBox(server.attr('data-guid'));
								}
								else
								{
									engine.call('GUIConnect', guid, "");
								}
							}
						}

					}


					e.preventDefault();
					
				});


				// Disconnect from connected server
				$('body').on('click', '.button-disconnect', function(e) {
					console.log(".button-disconnect clicked");

					engine.call('GUIDisconnect');
				});


				// Start Server button
				// This only brings up the dialog
				$(restrictTo).filterFind('.button-host-server').on('click', function() {
					console.log(".button-host-server clicked");
					makeHostConfigDialogBox();
				});


			});


		}
	};

});