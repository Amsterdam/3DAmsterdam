mergeInto(LibraryManager.library, {
	/*
	Clicks inside Unity canvas are caught 'too late' and
	are not legitimate actions in most browsers.
	Therefore any new windows/tabs we want to open, are blocked.
	We call this method from within Unity at the moment the pointer is down.
	This methods rewrites the window level pointer up event, so when the mouse
	button is released, the legitimate event will open a new window/tab
	*/
	OpenURLInNewWindow: function(openUrl) {
		var url = Pointer_stringify(openUrl);
        document.onmouseup = function()
        {
        	window.open(url);
			//Clear the event, so it can only happen once
        	document.onmouseup = null;
        }
	}	
});