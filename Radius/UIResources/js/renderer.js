
/*
 * Available functions:
 *	renderer.replace(...)
 *	renderer.append(...)
 *	renderer.before(...)
 *	renderer.after(...)
 *
 * Syntax: 
 *	renderer.replace('module-lobby', { isServer: true }, {}, $('.main-ui-layout-body.server-lobby'), function(attachReturn) {
 *		console.log('lobby template rendered');
 *	});
*/

var renderer = (function() {
	function genericRender(addTemplateAndReturnElements) {
		return function(module, context, attachOptions, element, callback, localRequire) {
			// Set require default
			// You might want to pass your own require so you know what assets that are already added
			require = typeof localRequire !== 'undefined' ? localRequire : require;

			require([module], function(cpt) {
				// Add the template html into the parent
				var elements = addTemplateAndReturnElements(cpt.template(context), element);
				//console.log(elements);

				// Call the callback
				callback(cpt.attach(attachOptions, elements));
			});
		}
	}

	return {
		replace: genericRender(function(html, element){
			return $(element).html(html).children();
		}),
		append: genericRender(function(html, element){
			return $(html).appendTo(element);
		}),
		prepend: genericRender(function(html, element){
			return $(html).prependTo(element);
		}),
		before: genericRender(function(html, element){
			return $(html).insertBefore(element);
		}),
		after: genericRender(function(html, element){
			return $(html).insertAfter(element);
		})
	}
})();



define(function(){
	return renderer;
});





/*
// `require` is optional
function render(module, context, attachOptions, parent, callback, localRequire) {
	// Set restricTo default
	require = typeof localRequire !== 'undefined' ? localRequire : require;

	require([module], function(cpt) {
		// Add the template html into the parent
		var elements = parent.html(cpt.template(context)).children();
		//console.log(elements);

		// Call the callback
		callback(cpt.attach(attachOptions, elements));
	});
}

// `require` is optional
function renderAppend(module, context, attachOptions, parent, callback, localRequire) {
	// Set restricTo default
	require = typeof localRequire !== 'undefined' ? localRequire : require;

	require([module], function(cpt) {
		// Add the template html into the parent
		var elements = $(cpt.template(context)).appendTo(parent);
		//console.log(elements);

		// Call the callback
		callback(cpt.attach(attachOptions, elements));
	});
}
*/