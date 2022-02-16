mergeInto(LibraryManager.library, {
	AutoPauseLogging: function () {
		/*
		Detect if browser tab/window is active (background tabs are usually throttled down)
		 */
        window.CheckVisibilityState = function CheckVisibilityState() {
			if (document.visibilityState == "hidden") {
				unityInstance.SendMessage("FpsCounter", "ActiveApplication", 0);
			} 
			else {
				unityInstance.SendMessage("FpsCounter", "ActiveApplication", 1);
			}
		}
		
		window.CheckVisibilityState();
		document.addEventListener('visibilitychange', function () {
			window.CheckVisibilityState();
		});
    },
    /*
    Send an event to SiteImprove Analytics
     */
    PushEvent: function (category, action, label) {
        var categoryString = Pointer_stringify(category);
        var actionString = Pointer_stringify(action);
        var labelString = Pointer_stringify(label);

        if (labelString != "" && typeof _sz !== 'undefined') {
            _sz.push(['event', categoryString, actionString, labelString]);
        } 
		else if(typeof _sz !== 'undefined'){
            _sz.push(['event', categoryString, actionString]);
        }
    }
});