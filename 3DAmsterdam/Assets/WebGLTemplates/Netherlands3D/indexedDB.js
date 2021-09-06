/* Copyright(C)  X Gemeente
				 X Amsterdam
				 X Economic Services Departments
Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License. You may obtain a copy of the License at:
https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions and limitations under the License.
*/
var databaseName = "retrieved from unity";
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
    if (filesToSave == 0) {
        db.close();
    }
}

function ClearInputs()
{
	inputs = document.getElementsByTagName('input');
	for (i = 0; i < inputs.length; ++i) {
		inputs[i].value = '';
	}
}

function SaveDatabaseName(dbname) {
    databaseName = dbname;
}

function ReadFiles(SelectedFiles) {
    if (window.File && window.FileReader && window.FileList && window.Blob) {
		ConnectToDatabase(SelectedFiles);   
		unityInstance.SendMessage('FileUploads', 'FileCount', SelectedFiles.length);
    } else {
        alert("Bestanden inladen wordt helaas niet ondersteund door deze browser.");
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

	let request = transaction.objectStore("FILE_DATA").put(data, databaseName + "/" + filename);
	console.log("saving file");
	request.onsuccess = function () {
		unityInstance.SendMessage('FileUploads', 'LoadFile', filename);
		console.log("file saved");
		FileSaved();
	};
	request.onerror = function () {
		unityInstance.SendMessage('FileUploads', 'LoadFileError', filename);
		alert("kan " + filename + " niet opslaan");
		FileSaved();
	};
}

function ConnectToDatabase(SelectedFiles) {
	
	//Connect to database
	window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB,
	IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction,
	dbVersion = 21;
	let request = indexedDB.open("/idbfs", dbVersion);
	request.onsuccess = function () {
		console.log("connected to database");
		db = request.result;
		for (var i = 0; i < SelectedFiles.length; i++) {
			ReadFile(SelectedFiles[i])
		};
		request.onerror = function () {
			alert("kan geen verbinding maken met de indexedDatabase");
		}
	}
}




	