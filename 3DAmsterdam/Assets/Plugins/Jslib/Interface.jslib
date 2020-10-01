mergeInto(LibraryManager.library, {
	/*
	This method draws a hidden div, at the same location of our Unity canvas button.
	This way we can,for example, directly trigger the file upload dialog. 
	Other indirect ways are blocked because of browser security.
	Canvas clicks caught by Unity are 'too late' for the browser to detect as a legitimate user action.
	*/
	DisplayDOMObjectWithID: function(id,display,x,y, width, height) {
		document.getElementById(id).style.display = Pointer_stringify(display);
		document.getElementById(id).style.margin = x + "px 0px 0px" + y + "px";
		document.getElementById(id).style.width = width + "px";
		document.getElementById(id).style.height = height + "px";
	},
	/*
	If the user changed the UI DPI in Unity, this method can be used to scale HTML elements with it.
	*/
	ChangeInterfaceScale: function(scale) {
		document.getElementById("sharedUrl").style.transform = "scale("+ scale + ","+ scale + ")";
		document.getElementById("sharedUrl").style.marginTop = (50*scale)+"px";
	}
});