mergeInto(LibraryManager.library, {
    Initialize: function (str) {
        window.databaseName = Pointer_stringify(str);
        window.selectedFiles = [];
        window.filesToSave = [];
        window.counter = 0;
        window.database = null;

        //Inject our required html input fields
        window.InjectHiddenFileInput = function InjectHiddenFileInput(type, acceptedExtentions) {
            var newInput = document.createElement("input");
            newInput.id = type + '-input';
            newInput.type = 'file';
            newInput.accept = acceptedExtentions;
            newInput.onchange = function () {
                window.ReadFiles(this.files)
            };
            newInput.style.cssText = 'display:none; position: fixed; bottom: 0; left: 0; z-index: 2; width: 0px; height: 0px;');
            document.body.appendChild(newInput);
        }
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

        window.ClearInputs = function ClearInputs() {
            var inputs = document.getElementsByTagName('input');
            for (i = 0; i < inputs.length; ++i) {
                inputs[i].value = '';
            }
        };

        window.ReadFiles = function ReadFiles(SelectedFiles) {
            if (window.File && window.FileReader && window.FileList && window.Blob) {
                ConnectToDatabase(SelectedFiles);
                unityInstance.SendMessage('FileUploads', 'FileCount', SelectedFiles.length);
            } else {
                alert("Bestanden inladen wordt helaas niet ondersteund door deze browser.");
            };
        };

        window.ConnectToDatabase = function ConnectToDatabase(SelectedFiles) {
            //Connect to database
            window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB,
            IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction,
            dbVersion = 21;
            let request = indexedDB.open("/idbfs", dbVersion);
            request.onsuccess = function () {
                console.log("connected to database");
                window.database = request.result;
                for (var i = 0; i < SelectedFiles.length; i++) {
                    window.ReadFile(SelectedFiles[i])
                };
                request.onerror = function () {
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
            const data = {
                timestamp: "timestamp",
                mode: 33206,
                contents: "contents"
            };
            data.timestamp = new Date();
            data.contents = new TextEncoder("utf-8").encode(datastring);
            var transaction = window.database.transaction(["FILE_DATA"], "readwrite");

            let request = transaction.objectStore("FILE_DATA").put(data, databaseName + "/" + filename);
            console.log("saving file");
            request.onsuccess = function () {
                unityInstance.SendMessage('FileUploads', 'LoadFile', filename);
                console.log("file saved");
                window.FileSaved();
            };
            request.onerror = function () {
                unityInstance.SendMessage('FileUploads', 'LoadFileError', filename);
                alert("kan " + filename + " niet opslaan");
                window.FileSaved();
            };
        };
    },
    StageIndexedDBUpload: function (filename, targetURL) {
        //stage a file for uploading
    },
    UploadFromIndexedDB: function () {
        //start uploading stages files

    },
    SyncFilesFromIndexedDB: function () {
        FS.syncfs(true, function (err) {
            SendMessage('FileUploads', 'IndexedDBUpdated');
        });
    },
    SyncFilesToIndexedDB: function () {
        FS.syncfs(false, function (err) {});
    },
    ClearFileInputFields: function () {
        window.ClearInputs();
    }
});