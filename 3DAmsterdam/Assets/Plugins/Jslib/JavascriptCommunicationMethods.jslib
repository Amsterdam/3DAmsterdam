mergeInto(LibraryManager.library, {
	UploadButtonCSSDisplay: function(display) {
		document.getElementById("objUploadClickRegisterArea").style.display = Pointer_stringify(display);
	},
	DisplayUniqueShareURL: function(uniqueToken) {
		document.getElementById("sharedUrl").style.display = 'inline';
		document.getElementById("sharedUrl").innerHTML = window.location.href + "?view=" + Pointer_stringify(uniqueUrl);
	},
	HideUniqueShareURL: function() {
		document.getElementById("sharedUrl").style.display = 'none';
		document.getElementById("sharedUrl").innerHTML = "";
	},
	FetchOBJData: function() {
		var bufferSize = lengthBytesUTF8(objData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(objData, buffer, bufferSize);
		return buffer;
	},
	FetchMTLData: function() {
		var bufferSize = lengthBytesUTF8(mtlData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(mtlData, buffer, bufferSize);
		return buffer;
	}	
});