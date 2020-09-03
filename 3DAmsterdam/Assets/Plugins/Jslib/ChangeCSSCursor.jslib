mergeInto(LibraryManager.library, {
	SetCSSCursor: function(cursorName) {
		document.getElementById("#canvas").style.cursor = Pointer_stringify(cursorName);
	}
});