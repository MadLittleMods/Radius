"use strict";

define([
	'module',
	'jquery',
	'jquery-utility', 
	'renderer',
	'require',
	'webfont'
], function(
	module,
	$,
	$utility,
	renderer,
	require,
	webfont
) {

	// Whether you toggle, show, hide the ui menu
	window.lockUI = true;

	window.toggleUI =  function(overrideLock) {
		if(!window.lockUI || overrideLock)
		{
			$('.main-ui-holder').toggle(200); // toggle the menus
			$('.hud-holder').toggle(200); // toggle the hud
		}
	}
	window.showUI =  function(overrideLock) {
		if(!window.lockUI || overrideLock)
		{
			$('.main-ui-holder').show(200); // show the menus
			$('.hud-holder').hide(200); //  hide the hud
		}
	}
	window.hideUI = function(overrideLock) {
		if(!window.lockUI || overrideLock)
		{
			$('.main-ui-holder').hide(200); // hide the menus
			$('.hud-holder').show(200); //show the hud
		}
	}

	window.switchTabs = function(newTab)
	{
		$('.main-nav .main-nav-item').removeClass('active');
		$('.main-ui-layout-body').removeClass('active');

		// You can pass in a class name
		if(typeof newTab == "string" || newTab instanceof String)
		{
			// Set the navbar active
			$('.main-nav .main-nav-item.' + newTab).addClass('active');
			// Show the layout body
			$('.main-ui-layout-body.' + $('.main-nav .main-nav-item.' + newTab).attr('data-connected-body')).addClass('active');
		}
		// Or a jquery object
		else
		{
			// Set the navbar active
			$(newTab).addClass('active');
			// Show the layout body
			$('.main-ui-layout-body.' + $(newTab).attr('data-connected-body')).addClass('active');
		}
	}

	window.clearTab = function(tabId)
	{
		// You can pass in a class name
		if(typeof tabId == "string" || tabId instanceof String)
		{
			// Remove the tab
			$('.main-nav .main-nav-item.' + tabId).remove();
			// Clear out the layout body
			$('.main-ui-layout-body.' + $('.main-nav .main-nav-item.' + tabId).attr('data-connected-body')).empty();
		}
	}


	return {

		bind: function(restrictTo) {
			// Set restricTo default
			restrictTo = typeof restrictTo !== 'undefined' ? restrictTo : $(document);


			$(document).ready(function() {
				// Binds the toggleUI (js) function to this call in C#:
				// m_View.View.TriggerEvent("toggleUI");
				engine.off('toggleUI', toggleUI, module.id);
				engine.on('toggleUI', toggleUI, module.id);


				// Get classes from fonts
				// using webfontloader: https://github.com/typekit/webfontloader
				WebFont.load({
					custom: {
						families: ['Mission Gothic', 'Ubuntu']
					}
				});

				
				// Make the nav bar switch tabs on click
				$(document).on('click', '.main-nav .main-nav-item', function() {
					switchTabs(this);
				});

				$(document).on('click', '.main-nav .main-nav-item.quit', function() {
					// Show the quit dialog
					var dialogTemplateHTML = '<div class="box-button half-button button-quit">Quit Game</div>';
					var dialogContext = {
						title: 'Quit Game',
						body: dialogTemplateHTML
					};
					renderer.append('module-dialog', dialogContext, {}, $('body'), function(attachReturn) {
						$(document).on('click', '.button-quit', function() {
							console.log('Quit button clicked');
							engine.call('QuitGame');
						});
					}, require);
				});
			});

		}
	};

});