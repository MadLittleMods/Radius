// https://github.com/guybedford/hbs

define(['amd-loader', 'handlebars'], function(amdLoader, Handlebars) {

	// via: http://stackoverflow.com/a/17903018/796832
	function getUniqueValuesInArray(a) {
		return a.reduce(function(p, c) {
			if (p.indexOf(c) < 0) p.push(c);
				return p;
		}, []);
	};

	// Joins path segments.  Preserves initial "/" and resolves ".." and "."
	// Original from: https://gist.github.com/creationix/7435851
	// * heavily modified by MLM
	// Unit tests: http://jsfiddle.net/MadLittleMods/3L4J8/
	function resolvePath( /* path segments */ ) {
		// Split the inputs into a list of path commands.
		var parts = [];
		for (var i = 0, l = arguments.length; i < l; i++) {
			parts = parts.concat(arguments[i].split("/"));
		}
		// Interpret the path commands to get the new resolved path.
		var newParts = [];
		var stillDotDot = true; // Used in the first piece to determine if still using `../`. This preserves it on the first piece
		for (i = 0, l = parts.length; i < l; i++) {
			var part = parts[i];
			// Remove leading and trailing slashes
			if (!part) continue;
			// Interpret "." to pop the last segment
			// If the second to last part is a dot
			// and If there is not a trailing slash on the previous piece
			if (part === "." && i == l-2 && parts[i-1]) {
				newParts.pop();
				stillDotDot = false;
			}
			else if(part === ".") {
				stillDotDot = false;
				continue;
			}
			// Interpret ".." to pop the last segment
			// If not stillDotDot.
			else if (part === ".." && !stillDotDot) {
				newParts.pop();
			}
			else if (part === "..") {
				newParts.push(part);         
			}
			// Push new path segments.
			else {
				stillDotDot = false;
				newParts.push(part);
			}
		}
		// Preserve the initial slash if there was one.
		if (parts[0] === "") newParts.unshift("");
		// Turn back into a single string path.
		return newParts.join("/") || (newParts.length ? "/" : ".");
	}

	
	var includeHelpers = /{{@([a-zA-Z-0-9\.\/\~]+)/g;
	var includeBlockHelpers = /{{#@([a-zA-Z-0-9\.\/\~]+)/g;
	var includePartials = /{{> ([a-zA-Z-0-9\.\/\~]+)/g;

	return amdLoader('hbs', 'hbs', function(name, source, req, callback, errback, config) {

		// replace internal requires with helper form
		var sanitize = function(name) {
			return name.replace('/', '-').replace('.', '').replace('~', '_');
		}
		var helpers = [];
		// Work out the normal helpers
		source = source.replace(includeHelpers, function(match, dep) {
			helpers.push(dep);
			return '{{' + sanitize(dep);
		});
		// Work out the block helpers
		source = source.replace(includeBlockHelpers, function(match, dep) {
			var helperId = dep;
			helpers.push(helperId);
			return '{{#' + sanitize(helperId);
		});
		// Work out closing to the block helper {{/helper}}
		var includeClosingBlockHelpers = new RegExp("{{\/([a-zA-Z-0-9\.\/\~]+)","g");
		source = source.replace(includeClosingBlockHelpers, function(match, dep) {
			return '{{/' + sanitize(dep);
		});
		// require the helpers
		helpers.unshift('handlebars-runtime');

		// Grab all the partials
		// Keep in mind that this does not rule out things commented out but it doesn't hurt anything
		var partials = [];
		source = source.replace(includePartials, function(match, dep) {
			partials.push(dep);
			return '{{>' + sanitize(dep);
		});
		partials = getUniqueValuesInArray(partials);


		// return the compiled template
		if (config.isBuild) {
			var output = "define(" + JSON.stringify(helpers) + ", function(Handlebars) {\n"
				+ (helpers[1] ? "  Handlebars.registerHelper('" + sanitize(helpers[1]) + "', arguments[1]);\n" : "")
				+ "  var t = Handlebars.template(" + Handlebars.precompile(source) + "); \n"
				+ "  return function(o) { return t(o); };\n"
				+ "});"
			callback(output);
		}
		else {
			// require the helpers
			req(helpers, function() {
				// Register the helpers
				for (var i = 0; i < arguments.length; i++)
					Handlebars.registerHelper(sanitize(helpers[i]), arguments[i]);


				// Now make the path relative
				var partialReferences = [partials.length];
				for (var i in partials)
				{
					var pathToPartial = resolvePath(name, partials[i]);
					partialReferences[i] = 'hbs!' + pathToPartial;
				}

				// Require the partials
				req(partialReferences, function() {
					// Register the partials
					for (var i = 0; i < arguments.length; i++)
						Handlebars.registerPartial(sanitize(partials[i]), arguments[i]);

					// return the compiled template
					var compiled = Handlebars.compile(source);
					callback(function(o) {
						return compiled(o);
					});
				});
			});
		}
	});
});
