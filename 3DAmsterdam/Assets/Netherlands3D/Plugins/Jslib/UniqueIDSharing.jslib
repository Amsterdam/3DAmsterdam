mergeInto(LibraryManager.library, {
	SetUniqueShareURL: function(uniqueToken) {
		sharedUrlText = window.location.href.split('?')[0] + "?view=" + Pointer_stringify(uniqueToken);
		document.getElementById("copySharedURLButton").value = sharedUrlText;
	}
});