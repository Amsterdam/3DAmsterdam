mergeInto(LibraryManager.library, {
	SyncFilesFromIndexedDB : function()
     {
        FS.syncfs(true,function (err) {
			SendMessage('FileUploads', 'IndexedDBUpdated');
        });
     },
	SyncFilesToIndexedDB : function()
     {
        FS.syncfs(false,function (err) {
		
		});
     },
	SendPersistentDataPath: function (str) {
		SaveDatabaseName(Pointer_stringify(str));
	},
	ClearFileInputFields: function ()
	{
		ClearInputs();
	}
});