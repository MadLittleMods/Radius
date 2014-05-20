"use strict";



jQuery.fn.filterFind = function(selector) { 
	return this.find('*')         // Take the current selection and find all descendants,
			   .addBack()         // add the original selection back to the set 
			   .filter(selector); // and filter by the selector.
};

/**
 * Returns a number whose value is limited to the given range.
 *
 * Example: limit the output of this computation to between 0 and 255
 * (x * 255).clamp(0, 255)
 *
 * @param {Number} min The lower boundary of the output range
 * @param {Number} max The upper boundary of the output range
 * @returns A number in the range [min, max]
 * @type Number
 */
Number.prototype.clamp = function(min, max) {
	return Math.min(Math.max(this, min), max);
};



function alphaFromRGBA(color) {
	//console.log('value: ' + value);
	var matches = color.match(/^rgba\([0-9]{1,3},\s?[0-9]{1,3},\s?[0-9]{1,3},\s?([0-9]*.?[0-9]*)\)$/i);
	var alpha = matches[1] ? matches[1] : 1;
	return alpha;
}


var UnityUtilities = {
	UnEscapeURL: function(parameter) {
		return decodeURIComponent((parameter +'').replace(/\+/g, '%20'));
	},
	Color1ToColor255: function(color1) {
		return {
			r: Math.round(color1.r*255).clamp(0, 255),
			g: Math.round(color1.g*255).clamp(0, 255),
			b: Math.round(color1.b*255).clamp(0, 255),
			a: color1.a.clamp(0, 1)
		};
	}
}