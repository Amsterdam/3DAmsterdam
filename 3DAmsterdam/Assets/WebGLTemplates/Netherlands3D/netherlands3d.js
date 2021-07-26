var forceFocus = false;
var unityCanvas;
var fileReader;

var sharedUrlText = "";

var unityObjectName = "FileUploads";
var objData = "";
var mtlData = "";
var dataType = 0;

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
	
	//Support for dragging dropping files on browser window
	document.addEventListener("dragover", function (event) {
		event.preventDefault();
	});
	document.addEventListener("drop", function (event) {
		event.stopPropagation();
		event.preventDefault();

		ReadFiles(event.dataTransfer.files);
	});

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

function BuildInUnity() {
    console.log("Build model in Unity");
    unityInstance.SendMessage(unityObjectName, "LoadOBJFromJavascript");
    //Reset file selection
    document.getElementById("obj").value = "";
}
function AbortInUnity() {
    console.log("Abort import in Unity");
    unityInstance.SendMessage(unityObjectName, "AbortImport");
    document.getElementById("obj").value = "";
}
function HandleCsvInUnity() {
    unityInstance.SendMessage("CSVTab", "LoadCSVFromJavascript");
    document.getElementById("csv").value = "";
}
function HandleFzpInUnity() {
    unityInstance.SendMessage(unityObjectName, "LoadVissimFromJavascript");
    document.getElementById("fzp").value = "";
}

function ReadFiles(selectedFiles) {
    //Check browser support
    if (window.File && window.FileReader && window.FileList && window.Blob) {
        fileReader = new FileReader();
    } else {
        alert("Bestanden uploaden wordt helaas niet ondersteund door deze browser.");
    }

    //Set the obj name to display in Unity, and open progress overlay
    unityInstance.SendMessage(unityObjectName, "SetOBJFileName", selectedFiles.item(0).name);

    //If we selected at least one file
    if (selectedFiles && selectedFiles[0]) {
        //parse obj and optionaly the mtl file
        var mtlFileSelectionIndex = -1;
        var objFileSelectionIndex = -1;

        var amountOfObjs = 0;
        var amountOfMtls = 0;

        //Check what index is the obj file, and optionaly the mtl file
        for (var i = 0; i < selectedFiles.length; i++) {
            var extension = selectedFiles[i].name.split(".").pop().toLowerCase();
            if (extension == "mtl") {
                console.log("MTL file");
                mtlFileSelectionIndex = i;
                amountOfMtls++;
            } else if (extension == "obj") {
                console.log("OBJ file");
                objFileSelectionIndex = i;
                amountOfObjs++;
            }
        }

        //Selected too many files?
        if (amountOfObjs > 1 || amountOfMtls > 1) {
            AbortInUnity();
            return false;
        }

        //Check if we at least found an obj in our selection of files
        if (objFileSelectionIndex == -1) {
            AbortInUnity();
            return false;
        }

        //What to do when files are done loading
        fileReader.onload = function (e) {
            console.log("OBJ file loaded.");
            //After reading the text for the OBJ
            objData = e.target.result;
            if (mtlFileSelectionIndex > -1) {
                //Load material after loading obj (optionaly)
                fileReader.onload = function (e) {
                    console.log("MTL file loaded.");
                    mtlData = e.target.result;
                    BuildInUnity();
                };
                fileReader.readAsText(selectedFiles[mtlFileSelectionIndex]);
            } else {
                BuildInUnity();
            }
        };

        //Start loading the obj file
        if (objFileSelectionIndex != -1) {
            fileReader.readAsText(selectedFiles[objFileSelectionIndex])
        };
    }
    return true;
}

function ReadFilesCsv(selectedFiles) {

    fileReader = new FileReader();
    //If we selected at least one file
    if (selectedFiles && selectedFiles[0]) {

        //Check what index is the obj file, and optionaly the mtl file
        for (var i = 0; i < selectedFiles.length; i++) {
            var extension = selectedFiles[i].name.split(".").pop().toLowerCase();
        }

        //What to do when files are done loading
        fileReader.onload = function (e) {
            objData = e.target.result;
            HandleCsvInUnity();
        };

        fileReader.readAsText(selectedFiles[0]);

    }
    return true;
}

function ReadFilesFzp(selectedFiles) {

    fileReader = new FileReader();
    //If we selected at least one file
    if (selectedFiles && selectedFiles[0]) {

        	//What to do when files are done loading
        	fileReader.onload = function (e) {
            	objData = e.target.result;
            	HandleFzpInUnity();
        	};

        	fileReader.readAsText(selectedFiles[0]);
    }
    return true;
}