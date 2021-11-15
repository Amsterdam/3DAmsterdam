mergeInto(LibraryManager.library, {
    InitializeIndexedDB: function (str) {
        window.databaseName = Pointer_stringify(str);
        window.selectedFiles = [];
        window.filesToSave = 0;
        window.counter = 0;
        window.databaseConnection = null;

        //Inject our required html input fields
        window.InjectHiddenFileInput = function InjectHiddenFileInput(type, acceptedExtentions) {
            var newInput = document.createElement("input");
            newInput.id = type + '-input';
            newInput.type = 'file';
            newInput.accept = acceptedExtentions;
            newInput.onchange = function () {
                window.ReadFiles(this.files);
            };
            newInput.style.cssText = 'display:none; position: fixed; bottom: 0; left: 0; z-index: 2; width: 0px; height: 0px;';
            document.body.appendChild(newInput);
        };
        window.InjectHiddenFileInput('obj', '.obj,.mtl');
        window.InjectHiddenFileInput('csv', '.csv,.tsv');
        window.InjectHiddenFileInput('fzp', '.fzp');
        window.InjectHiddenFileInput('geojson', '.json,.geojson');

        //Support for dragging dropping files on browser window
        document.addEventListener("dragover", function (event) {
            event.preventDefault();
        });

        document.addEventListener("drop", function (event) {
            console.log("File dropped");
            event.stopPropagation();
            event.preventDefault();
            // tell Unity how many files to expect
            window.ReadFiles(event.dataTransfer.files);
        });
		
		window.FileSaved = function FileSaved() {
			filesToSave = filesToSave - 1;
			if (filesToSave == 0) {
				window.databaseConnection.close();
			}
		};

        window.ClearInputs = function ClearInputs() {
            var inputs = document.getElementsByTagName('input');
            for (i = 0; i < inputs.length; ++i) {
                inputs[i].value = '';
            }
        };

        window.ReadFiles = function ReadFiles(SelectedFiles) {
            if (window.File && window.FileReader && window.FileList && window.Blob) {
                window.ConnectToDatabase(SelectedFiles);
                unityInstance.SendMessage('UserFileUploads', 'FileCount', SelectedFiles.length);
            } else {
                alert("Bestanden inladen wordt helaas niet ondersteund door deze browser.");
            };
        };

        window.ConnectToDatabase = function ConnectToDatabase(SelectedFiles) {
            //Connect to database
            window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB;
            IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction;
            dbVersion = 21;
			window.filesToSave = SelectedFiles.length;
			
            var dbRequest = window.indexedDB.open("/idbfs", dbVersion);
            dbRequest.onsuccess = function () {
                console.log("connected to database");
                window.databaseConnection = dbRequest.result;
                for (var i = 0; i < SelectedFiles.length; i++) {
                    window.ReadFile(SelectedFiles[i])
                };
                dbRequest.onerror = function () {
                    alert("kan geen verbinding maken met de indexedDatabase");
                }
            }
        };

        window.ReadFile = function ReadFile(file) {
            window.filereader = new FileReader();
            window.filereader.onload = function (e) {
                var datastring = e.target.result;
                window.SaveData(datastring, file.name);
                window.counter = counter + 1;
            };
            window.filereader.readAsText(file);
        };

        window.SaveData = function SaveData(datastring, filename) {
            var data = {
                timestamp: "timestamp",
                mode: 33206,
                contents: "contents"
            };
            data.timestamp = new Date();
            data.contents = new TextEncoder("utf-8").encode(datastring);
            var transaction = window.databaseConnection.transaction(["FILE_DATA"], "readwrite");
			var newIndexedFilePath = databaseName + "/" + filename;
            var dbRequest = transaction.objectStore("FILE_DATA").put(data, newIndexedFilePath);
            console.log("Saving file: " + newIndexedFilePath);
            dbRequest.onsuccess = function () {
                unityInstance.SendMessage('UserFileUploads', 'LoadFile', filename);
                console.log("File saved: " + newIndexedFilePath);
                window.FileSaved();
            };
            dbRequest.onerror = function () {
                unityInstance.SendMessage('UserFileUploads', 'LoadFileError', filename);
                alert("Could not save: " + newIndexedFilePath);
                window.FileSaved();
            };
        };
    },
    UploadFromIndexedDB: function (filePath, targetURL) {
        FS.syncfs(false, function (err) { });

        var transaction = window.databaseConnection.transaction(["FILE_DATA"], "readonly");
        var indexedFilePath = databaseName + "/" + filename;
        var dbRequest = transaction.objectStore("FILE_DATA").get(indexedFilePath);
        console.log("Reading IndexedDB file: " + newIndexedFilePath);
        dbRequest.onsuccess = function (e) {
            let record = e.target.result;
            var xhr = new XMLHttpRequest;
            xhr.open("PUT", stagedUpload.targetURL, false);
            xhr.send(record.data);
            unityInstance.SendMessage('ShareDialog', 'IndexedDBUploadCompleted');	
        };
        dbRequest.onerror = function () {
            unityInstance.SendMessage('ShareDialog', 'IndexedDBUploadFailed', filename);
        };	
    },
    SyncFilesFromIndexedDB: function () {
        FS.syncfs(true, function (err) {
            SendMessage('UserFileUploads', 'IndexedDBUpdated');
        });
    },
    SyncFilesToIndexedDB: function () {
        FS.syncfs(false, function (err) {});
    },
    ClearFileInputFields: function () {
        window.ClearInputs();
    }
});