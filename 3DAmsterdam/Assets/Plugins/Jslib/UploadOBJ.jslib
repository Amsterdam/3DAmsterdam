mergeInto(LibraryManager.library, {
	UploadButtonCSSDisplay: function(display) {
		document.getElementById("objUploadClickRegisterArea").style.display = Pointer_stringify(display);
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