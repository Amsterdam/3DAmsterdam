mergeInto(LibraryManager.library, {
	DisplayUniqueShareURL: function(uniqueToken) {
		document.getElementById("sharedUrl").style.display = 'inline';
		document.getElementById("sharedUrl").innerHTML = window.location.href + "?view=" + Pointer_stringify(uniqueToken);
	},
	HideUniqueShareURL: function() {
		document.getElementById("sharedUrl").style.display = 'none';
		document.getElementById("sharedUrl").innerHTML = "";
	}
});