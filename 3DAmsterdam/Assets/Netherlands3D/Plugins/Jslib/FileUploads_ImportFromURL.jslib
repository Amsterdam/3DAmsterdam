mergeInto(LibraryManager.library, {
    //Addition to Netherlands3D FileUploads.jslib
    //This allows the importing of a large file from an url, and streamreading 
    //it in Unity from the IndexedDB without having to land in the unity heap memory
    ImportFromURL: function (url, filename) {
        var fileUrl = UTF8ToString(url);	
        var fileName = UTF8ToString(filename);
		
        // Create a new XHR object to download the file
        const xhr = new XMLHttpRequest();
        xhr.open('GET', fileUrl, true);
        xhr.responseType = 'arraybuffer';

        // Event handler for successful file download
        xhr.onload = function() {
            if (xhr.status === 200) {     
                const arrayBuffer = xhr.response;
                const uint8Array = new Uint8Array(arrayBuffer);

                //Use same method that is used with local file imports
                //This way we trigger native parsers
                unityInstance.SendMessage('UserFileUploads', 'FileCount', 1);
				
				var dbConnectionRequest = window.indexedDB.open("/idbfs", window.dbVersion);
							
				dbConnectionRequest.onsuccess = function () {
					window.databaseConnection = dbConnectionRequest.result;			
					window.SaveData(uint8Array, fileName);	
				}
				dbConnectionRequest.onerror = function () {
					console.error("Could not save file");
				}
                console.log('Saving file from url:', url);
            } 
            else {
                console.error('Failed to download file:', xhr.status);
            }
        };

        // Event handler for file download errors
        xhr.onerror = function() {
            console.error('Error downloading file:', xhr.statusText);
        };

        // Send the request to download the file
        xhr.send();
    }
});