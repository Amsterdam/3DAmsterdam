mergeInto(LibraryManager.library, {
	/*
	Read uploaded OBJ data from the string value, and return the buffer.
	*/
	FetchOBJData: function() {
		var bufferSize = lengthBytesUTF8(objData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(objData, buffer, bufferSize);
		
		objData = "";
		
		return buffer;
	},
	/*
	Read uploaded OBJ-MTL data from the string value, and return the buffer.
	*/
	FetchMTLData: function() {
		var bufferSize = lengthBytesUTF8(mtlData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(mtlData, buffer, bufferSize);
		
		mtlData = "";
		
		return buffer;
	}	
});