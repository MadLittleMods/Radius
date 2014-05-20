define(function() {
	// via: http://stackoverflow.com/a/18026063/796832
	return function(options) {
		var context = {};
		var mergeContext = function(obj) {
			for(var k in obj)context[k]=obj[k];
		};
		mergeContext(this);
		mergeContext(options.hash);
		return options.fn(context);
	}
});