mergeInto(LibraryManager.library, {
    SetUniqueShareURL: function (uniqueToken) {
        sharedUrlText = window.location.href.split(/[?#]/)[0] + "?view=" + UTF8ToString(uniqueToken);

        //inject copy button if we do not have it yet
        if (!window.copySharedURLButton) {
            window.copySharedURLButton = document.createElement("input");
            window.copySharedURLButton.type = 'text';
            window.copySharedURLButton.style.cssText = 'opacity:0; cursor:pointer !important; position: fixed; bottom: 0; left: 0; z-index: 2; width: 0px; height: 0px;';
            window.copySharedURLButton.id = 'copy-input';
            window.copySharedURLButton.onclick = function () {
                window.copySharedURLButton.focus();
                window.copySharedURLButton.select();
                window.preventNativeCopyEvents = false;
                document.execCommand('copy');
				window.preventNativeCopyEvents = true;
                //feedback animation in unity
                unityInstance.SendMessage("SharedURL", "CopiedText");

                console.log("Copied the url to clipboard: " + sharedUrlText);
            };
            document.body.appendChild(window.copySharedURLButton);
        }
        window.copySharedURLButton.value = sharedUrlText;
    }
});