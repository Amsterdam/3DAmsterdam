mergeInto(LibraryManager.library, {
    /*
    Change the browser URL from Unity
     */
    SetHash: function (newHash) {
        var hash = UTF8ToString(newHash);
        window.location.hash = hash;
    }
});