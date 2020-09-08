mergeInto(LibraryManager.library, {
	/*
	This method draws a hidden div, at the same location of our Unity canvas button.
	This way we can directly trigger the file upload dialog. 
	Other indirect ways are blocked because of browser security.
	Canvas clicks caught by Unity are 'too late' for the browser to detect as a legitimate user action.
	*/
	UploadButtonCSSDisplay: function(display) {
		document.getElementById("objUploadClickRegisterArea").style.display = Pointer_stringify(display);
	},
	/*
	Read uploaded OBJ data from the string value, and return the buffer.
	*/
	FetchOBJData: function() {
		var bufferSize = lengthBytesUTF8(objData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(objData, buffer, bufferSize);
		return buffer;
	},
	/*
	Read uploaded OBJ-MTL data from the string value, and return the buffer.
	*/
	FetchMTLData: function() {
		var bufferSize = lengthBytesUTF8(mtlData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(mtlData, buffer, bufferSize);
		return buffer;
	}	
});