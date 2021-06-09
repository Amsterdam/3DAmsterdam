mergeInto(LibraryManager.library, {

	/*
	We use this method to make OS level cursor style changes.
	Users expect a mouse cursor to be a hand, or a text pointer etc. 
	This way we can change the canvas pointer style from within Unity scripts.
	*/
	SetCSSCursor: function(cursorName) {
		document.getElementById("unity-canvas").style.cursor = Pointer_stringify(cursorName);
	}
});