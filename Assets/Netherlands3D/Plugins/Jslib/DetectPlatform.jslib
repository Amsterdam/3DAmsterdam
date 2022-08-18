mergeInto(LibraryManager.library, {
	/*
	Returns if we are on a mobile device (allowing us do do specific settings wihtin Unity)
	*/
	IsMobile: function() {
		return UnityLoader.SystemInfo.mobile;
	}
});