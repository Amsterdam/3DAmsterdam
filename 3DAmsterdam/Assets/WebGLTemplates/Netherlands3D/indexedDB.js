
var databasenaam = "nog niet ongevuld";
var db;
var selectedFiles;
var filesToSave;
var counter = 0;
//Support for dragging dropping files on browser window
document.addEventListener("dragover", function (event) {
	event.preventDefault();
});

document.addEventListener("drop", function (event) {
	console.log("file dropped");
	event.stopPropagation();
	event.preventDefault();
	// tell Unity how many files to expect
	ReadFiles(event.dataTransfer.files);

});

function FileSaved() {
	filesToSave = filesToSave - 1;
    if (filesToSave==0) {
		db.close();
		//myGameInstance.SendMessage('FileUploads', 'FileCount', SelectedFiles.length);
    }
}

function saveDatabaseName(dbname) {
	databasenaam = dbname;
}

function ReadFiles(SelectedFiles) {
	
	if (window.File && window.FileReader && window.FileList && window.Blob) {
		connectToDatabase(SelectedFiles);
		myGameInstance.SendMessage('FileUploads', 'FileCount', SelectedFiles.length);
	}
	else {
		alert("Bestanden uploaden wordt helaas niet ondersteund door deze browser.");
	};
};

function ReadFile(file) {
	filereader = new FileReader();
	filereader.onload = function (e) {
		datastring = e.target.result;
		SaveData(datastring, file.name);
		counter = counter + 1;
	};
	filereader.readAsText(file);
}

function SaveData(datastring, filename) {
	
	const data = { timestamp: "timestamp", mode: 33206, contents: "contents" };
	data.timestamp = new Date();
	data.contents = new TextEncoder("utf-8").encode(datastring);
	var transaction = db.transaction(["FILE_DATA"], "readwrite");
	transaction.oncomplete = function () {
		myGameInstance.SendMessage('FileUploads', 'LoadFile', filename);
		FileSaved();
	};
	transaction.objectStore("FILE_DATA").put(data, databasenaam + "/" + filename);
}

function connectToDatabase(SelectedFiles) {
	
	//connect tot database
	window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB,
		IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction,
		dbVersion = 21;
	var request = indexedDB.open("/idbfs", dbVersion);
	request.onsuccess = function (event) {
		db = request.result;
		for (var i = 0; i < SelectedFiles.length; i++) {
			ReadFile(SelectedFiles[i])
		};

	}
}




	