mergeInto(LibraryManager.library, {
    /*
    Change the browser URL from Unity
     */
    SetHash: function (newHash) {
        var hash = Pointer_stringify(newHash);
        window.location.hash = hash;
    }
});