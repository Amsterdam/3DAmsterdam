mergeInto(LibraryManager.library, {
	SetUniqueShareURL: function(uniqueToken) {
		document.getElementById("sharedUrl").innerHTML = window.location.href.split('?')[0] + "?view=" + Pointer_stringify(uniqueToken);
	}
});