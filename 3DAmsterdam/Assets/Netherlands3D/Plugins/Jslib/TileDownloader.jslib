mergeInto(LibraryManager.library, {
    DownloadFileData: function (filenameptr) {
        var filename = Pointer_stringify(filenameptr);
        var xhr = new XMLHttpRequest();
        (window.httprequest = window.httprequest ? window.httprequest : {})[filename] = xhr;
        xhr.open('GET', filename, true);
        xhr.responseType = 'arraybuffer';
        xhr.onload = function (e) {
            if (this.status == 200) {
                (window.filedata = window.filedata ? window.filedata : {})[filename] = this.response;
                unityInstance.SendMessage("JavascriptDownloader", "FileDownloaded", filename);
				console.log("Done downloading");
            }
        };

        xhr.send();
    },
    AbortDownloadProgress: function (filenameptr) {
        var filename = Pointer_stringify(filenameptr);
        window.httprequest[filename].abort();
    },
    GetFileDataLength: function (filenameptr) {
        var filename = Pointer_stringify(filenameptr);
        console.log("GetFileDataLength");
        console.log(filename);
        console.log(window);
        console.log(window.filedata);
        return window.filedata[filename].byteLength;
    },
    GetFileData: function (filenameptr) {
        var filename = Pointer_stringify(filenameptr);
        var filedata = window.filedata[filename];
        var ptr = (window.fileptr = window.fileptr ? window.fileptr : {})[filename] = _malloc(filedata.byteLength);
        var dataHeap = new Uint8Array(HEAPU8.buffer, ptr, filedata.byteLength);
        dataHeap.set(new Uint8Array(filedata));
        return ptr;
    },
    FreeFileData: function (filenameptr) {
        var filename = Pointer_stringify(filenameptr);
        _free(window.fileptr[filename]);
        delete window.fileptr[filename];
        delete window.filedata[filename];
		console.log("Cleared download from memory");
    }
});