mergeInto(LibraryManager.library, {
	SetCSSCursor: function(cursorName) {
		document.getElementById("gameContainer").style.cursor = Pointer_stringify(cursorName);
	}
});