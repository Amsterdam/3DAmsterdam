mergeInto(LibraryManager.library, {
	AutoCursorLock: function () {	
        window.OnCursorStateChanged = function OnCursorStateChanged()
		{
			var unityCanvas = document.getElementById("unity-canvas");
			if (document.pointerLockElement != unityCanvas && document.mozPointerLockElement != unityCanvas) {
				unityInstance.SendMessage("CameraModeChanger", "GodViewMode");
			}
		}
		//Cursor locking for First Person camera mode
		if ("onpointerlockchange" in document) {
			document.addEventListener('pointerlockchange', window.OnCursorStateChanged, false);
			console.log(document.pointerLockElement);
		} 
		else if ("onmozpointerlockchange" in document) {
			document.addEventListener('mozpointerlockchange', window.OnCursorStateChanged, false);
			console.log(document.mozPointerLockElement);
		}
    },
    /*
    This method locks the cursor for you, since sometimes doing it through Unity just doesn't work
     */
    LockCursorInternal: function () {
        document.getElementById("unity-canvas").requestPointerLock();
    }
});