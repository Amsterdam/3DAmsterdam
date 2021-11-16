mergeInto(LibraryManager.library, {
    /*
    This method draws a hidden div, at the same location of our Unity canvas button.
    This way we can,for example, directly trigger the file upload dialog.
    Other indirect ways are blocked because of browser security.
    Canvas clicks caught by Unity are 'too late' for the browser to detect as a legitimate user action.
     */
    DisplayDOMObjectWithID: function (id, display, x, y, width, height, offsetX, offsetY) {
        var idString = Pointer_stringify(id);
        var targetDomObject = document.getElementById(idString);
		var viewCanvas = document.getElementsByTagName("canvas")[0];
		
		var heightScreenPercentage = viewCanvas.clientHeight / window.innerHeight * 100;
		var widthScreenPercentage = viewCanvas.clientWidth / window.innerWidth * 100;

		var roundedOffsetX = Math.round(offsetX);
		var roundedOffsetY = Math.round(offsetY);
		
        if (targetDomObject) {
            targetDomObject.style.display = Pointer_stringify(display);
            targetDomObject.style.margin = "0px 0px calc(" + ((y + height) * heightScreenPercentage) + "vh - " + roundedOffsetY + "px) calc(" + (x * widthScreenPercentage) + "vw + " + roundedOffsetX + "px)";
            targetDomObject.style.width = (width * widthScreenPercentage) + "vw";
            targetDomObject.style.height = (height * heightScreenPercentage) + "vh";
        } else {
            console.log("Interface.jslib->DisplayDOMObjectWithID: Cant find DOM object with id: " + idString + ".");
        }
    },
    /*
    If the user changed the UI DPI in Unity, this method can be used to scale HTML elements with it.
     */
    ChangeInterfaceScale: function (scale) {
        //For example the fontsize of myDivname:
        //document.getElementById("myDivName").style.fontSize = scale + "em";
    }
});