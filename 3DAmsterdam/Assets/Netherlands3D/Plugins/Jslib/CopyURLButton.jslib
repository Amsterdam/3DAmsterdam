mergeInto(LibraryManager.library, {
    CreateCopyPasteButton: function () {	
		window.preventNativeCopyEvents = true;
		window.CopySharedURL = function CopySharedURL() {
		  //copy text from out hidden textfield
		  var copySharedURLButton  = document.getElementById("copy-input");
		  copySharedURLButton.focus();
		  copySharedURLButton.select();
		  
		  window.preventNativeCopyEvents = false;
		  document.execCommand('copy');
		  
		  window.preventNativeCopyEvents = true;
		  
		  //feedback animation in unity
		  unityInstance.SendMessage("SharedURL", "CopiedText");
		  
		  console.log("Copied the url to clipboard: " + sharedUrlText);
		}
	
		var newInput = document.createElement("input");
		newInput.id = 'copy-input';
		newInput.type = 'text';
		newInput.onclick = function () {
			window.CopySharedURL();
		};
		newInput.style.cssText = 'display:none; opacity: 0; position: fixed; bottom: 0; left: 0; z-index: 2; width: 0px; height: 0px;';
		document.body.appendChild(newInput);
    },
});