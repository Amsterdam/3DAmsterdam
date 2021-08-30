var forceFocus = false;
var sharedUrlText = "";
var preventNativeCopyEvents = true;

function ListenerInit()
{
	//Cursor locking for First Person camera mode
	if ("onpointerlockchange" in document) {
		document.addEventListener('pointerlockchange', OnCursorStateChanged, false);
		console.log(document.pointerLockElement);
	} else if ("onmozpointerlockchange" in document) {
		document.addEventListener('mozpointerlockchange', OnCursorStateChanged, false);
		console.log(document.mozPointerLockElement);
	}

	//Listener and method to let Unity know our tab/application is active
	//FPS counting is not done on an inactive tab (those tabs are throttled down to 1~5fps)
	CheckVisibilityState();
	document.addEventListener('visibilitychange', function () {
		CheckVisibilityState();
	});
	
	//A user can provide a lat,lon coordinate in the url has
	//This listener catches those changes
	window.onhashchange = LocationHashChanged;
	if (window.location.hash) {
		  LocationHashChanged();
	}
}

function CheckVisibilityState() {
    if (document.visibilityState == "hidden") {
        unityInstance.SendMessage("FpsCounter", "ActiveApplication", 0);
    } else {
        unityInstance.SendMessage("FpsCounter", "ActiveApplication", 1);
    }
}


function OnCursorStateChanged() {
    if (document.pointerLockElement != canvas && document.mozPointerLockElement != canvas) {
        unityInstance.SendMessage("FirstPersonCamera", "EnableMenus");
    }
    if (forceFocus == true) {
        window.focus();
    }
}

//Method 
function LocationHashChanged() {
    unityInstance.SendMessage("CameraModeChanger", "ChangedPointFromUrl", window.location.hash.replace("#", ""));
}

function CopySharedURL() {
  //copy text from out hidden textfield
  let copySharedURLButton  = document.getElementById("copySharedURLButton");
  copySharedURLButton.focus();
  copySharedURLButton.select();
  
  preventNativeCopyEvents = false;
  document.execCommand('copy');
  
  //feedback animation in unity
  unityInstance.SendMessage("SharedURL", "CopiedText");
  
  console.log("Copied the url to clipboard: " + sharedUrlText);
}