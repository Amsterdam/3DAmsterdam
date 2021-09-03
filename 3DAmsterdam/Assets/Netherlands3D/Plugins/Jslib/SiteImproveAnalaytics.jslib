mergeInto(LibraryManager.library, {
	/*
	Send an event to SiteImprove Analytics
	*/
	PushEvent: function(category,action,label) {
		var categoryString = Pointer_stringify(category);
		var actionString = Pointer_stringify(action);
		var labelString = Pointer_stringify(label);
		
		if(labelString != ""){
			_sz.push(['event', categoryString, actionString, labelString]);
		}
		else
		{
			_sz.push(['event', categoryString, actionString]);
		}
	}	
});