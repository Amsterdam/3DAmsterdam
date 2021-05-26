mergeInto(LibraryManager.library, {
	/*
	Read uploaded OBJ data from the string value, and return the buffer.
	*/
	FetchVissimData: function() {
		var bufferSize = lengthBytesUTF8(vissimData) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(vissimData, buffer, bufferSize);
		
		vissimData = "";
		
		return buffer;
	},
});