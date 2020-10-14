mergeInto(LibraryManager.library, {

	/*
	This method locks the cursor for you, since sometimes doing it through Unity just doesn't work
	*/
	LockCursorInternal: function() {
		 document.getElementById("#canvas").requestPointerLock();
         console.log("In cursor lock internal js function");
	}
});