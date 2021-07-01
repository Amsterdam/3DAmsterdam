mergeInto(LibraryManager.library, {
	DownloadFile : function(array, size, fileNamePtr)
	{
    	var fileName = UTF8ToString(fileNamePtr);
 
    	var bytes = new Uint8Array(size);
    	for (var i = 0; i < size; i++)
    	{
    	   bytes[i] = HEAPU8[array + i];
		}
 
    	var blob = new Blob([bytes]);
    	var link = document.createElement('a');
    	link.href = window.URL.createObjectURL(blob);
    	link.download = fileName;
 
		var event = document.createEvent("MouseEvents");
    	event.initMouseEvent("click");
    	link.dispatchEvent(event);
		window.URL.revokeObjectURL(link.href);
		
		//hide loadingscreen
		unityInstance.SendMessage("LoadingScreen","Hide");
	},
	DownloadFileAsZip : function(array, size, fileNamePtr)
	{
    	var fileName = UTF8ToString(fileNamePtr);
 
    	var bytes = new Uint8Array(size);
    	for (var i = 0; i < size; i++)
    	{
    	   bytes[i] = HEAPU8[array + i];
		}
 
    	var zip = new JSZip();
		zip.file("LICENCE.txt", "License placeholder\n");
		zip.file(fileName, bytes, {binary: true});
		zip.generateAsync({type:"blob"})
		.then(function(content) {
			// see FileSaver.js
			saveAs(content, fileName + ".zip");
			
			//hide loadingscreen
			unityInstance.SendMessage("LoadingScreen","Hide");
		});
	}
});