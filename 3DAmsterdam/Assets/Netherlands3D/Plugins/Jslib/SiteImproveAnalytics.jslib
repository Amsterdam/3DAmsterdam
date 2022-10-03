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
        var categoryString = UTF8ToString(category);
        var actionString = UTF8ToString(action);
        var labelString = UTF8ToString(label);

        if (labelString != "" && typeof _sz !== 'undefined') {
            _sz.push(['event', categoryString, actionString, labelString]);
        } 
		else if(typeof _sz !== 'undefined'){
            _sz.push(['event', categoryString, actionString]);
        }
    }
});