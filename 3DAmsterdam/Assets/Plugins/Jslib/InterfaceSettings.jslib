mergeInto(LibraryManager.library, {
	/*
	If the user changed the UI DPI in Unity, this method can be used to scale HTML elements with it.
	*/
	ChangeInterfaceScale: function(scale) {
		document.getElementById("sharedUrl").style.transform = "scale("+ scale + ","+ scale + ")";
		document.getElementById("sharedUrl").style.marginTop = (50*scale)+"px";
	}
});