mergeInto(LibraryManager.library, {
    InitializeIndexedDB: function (str) {
        window.databaseName = Pointer_stringify(str);

        console.log("Database name: " + window.databaseName);

        window.selectedFiles = [];
        window.filesToSave = 0;
        window.counter = 0;
        window.databaseConnection = null;

        window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB;
        window.IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction;
        window.dbVersion = 21;

        //Inject our required html input fields
        window.InjectHiddenFileInput = function InjectHiddenFileInput(type, acceptedExtentions, multiFileSelect) {
            var newInput = document.createElement("input");
            newInput.id = type + '-input';
            newInput.type = 'file';
            newInput.accept = acceptedExtentions;
			newInput.multiple = multiFileSelect;
            newInput.onchange = function () {
                window.ReadFiles(this.files);
            };
            newInput.style.cssText = 'display:none; cursor:pointer; opacity: 0; position: fixed; bottom: 0; left: 0; z-index: 2; width: 0px; height: 0px;';
            document.body.appendChild(newInput);
        };
        window.InjectHiddenFileInput('obj', '.obj,.mtl', true);
        window.InjectHiddenFileInput('csv', '.csv,.tsv', false);
        window.InjectHiddenFileInput('fzp', '.fzp', false);
        window.InjectHiddenFileInput('geojson', '.json,.geojson', false);

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
                window.ConnectToDatabaseAndReadFiles(SelectedFiles);
                unityInstance.SendMessage('UserFileUploads', 'FileCount', SelectedFiles.length);
            } else {
                alert("Bestanden inladen wordt helaas niet ondersteund door deze browser.");
            };
        };

        window.ConnectToDatabaseAndReadFiles = function ConnectToDatabase(SelectedFiles) {
            //Connect to database
			window.filesToSave = SelectedFiles.length;
			
            var dbConnectionRequest = window.indexedDB.open("/idbfs", window.dbVersion);
            dbConnectionRequest.onsuccess = function () {
                console.log("connected to database");
                window.databaseConnection = dbConnectionRequest.result;
                for (var i = 0; i < SelectedFiles.length; i++) {
                    window.ReadFile(SelectedFiles[i])
                };
            }
            dbConnectionRequest.onerror = function () {
                alert("Kan geen verbinding maken met de indexedDatabase");
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
            var newIndexedFilePath = window.databaseName + "/" + filename;
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
        var fileName = Pointer_stringify(filePath);
        var url = Pointer_stringify(targetURL);

        var dbConnectionRequest = window.indexedDB.open("/idbfs", window.dbVersion);
        dbConnectionRequest.onsuccess = function () {
            console.log("Connected to database");
            window.databaseConnection = dbConnectionRequest.result;

            var transaction = window.databaseConnection.transaction(["FILE_DATA"], "readonly");
            var indexedFilePath = window.databaseName + "/" + fileName;
            console.log("Uploading from IndexedDB file: " + indexedFilePath);

            var dbRequest = transaction.objectStore("FILE_DATA").get(indexedFilePath);
            dbRequest.onsuccess = function (e) {
                var record = e.target.result;
                var xhr = new XMLHttpRequest;
                xhr.open("PUT", url, false);
                xhr.send(record.contents);
                window.databaseConnection.close();
                unityInstance.SendMessage('Share', 'IndexedDBUploadCompleted');
            };
            dbRequest.onerror = function () {
                window.databaseConnection.close();
                unityInstance.SendMessage('Share', 'IndexedDBUploadFailed', filename);
            };
        }
        dbConnectionRequest.onerror = function () {
            alert("Kan geen verbinding maken met de indexedDatabase");
        }
    },
    SyncFilesFromIndexedDB: function () {
        FS.syncfs(true, function (err) {
            console.log(err);
            SendMessage('UserFileUploads', 'IndexedDBUpdated');
        });
    },
    SyncFilesToIndexedDB: function () {
        FS.syncfs(false, function (err) {
            console.log(err);
            SendMessage('Share', 'IndexedDBSyncCompleted');
        });
    },
    ClearFileInputFields: function () {
        window.ClearInputs();
    }
});