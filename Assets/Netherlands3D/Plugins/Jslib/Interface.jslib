mergeInto(LibraryManager.library, {
	/*
	This method draws a hidden div, at the same location of our Unity canvas button.
	This way we can,for example, directly trigger the file upload dialog. 
	Other indirect ways are blocked because of browser security.
	Canvas clicks caught by Unity are 'too late' for the browser to detect as a legitimate user action.
	*/
	DisplayDOMObjectWithID: function(id,display,x,y, width, height) {
		var idString = Pointer_stringify(id);
		document.getElementById(idString).style.display = Pointer_stringify(display);
		document.getElementById(idString).style.margin = "0px 0px " + ((y + height) *100) + "vh " + (x*100) + "vw";
		document.getElementById(idString).style.width = (width*100) + "vw";
		document.getElementById(idString).style.height = (height*100) + "vh";
	},
	/*
	If the user changed the UI DPI in Unity, this method can be used to scale HTML elements with it.
	*/
	ChangeInterfaceScale: function(scale) {
		//For example the fontsize of myDivname:
		//document.getElementById("myDivName").style.fontSize = scale + "em";
	}
});