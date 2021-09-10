using Netherlands3D.Interface;
using Netherlands3D.Logging;
using Netherlands3D.ObjectInteraction;
using Netherlands3D.Rendering;
using Netherlands3D.Settings;

namespace Netherlands3D {
	/// <summary>
	/// Shortcut class for all singleton systems
	/// </summary>
	public class Systems
	{
		public static Selector Selector => Selector.Instance;
		public static Analytics Analytics => Analytics.Instance;
		public static LoadingScreen LoadingScreen => LoadingScreen.Instance;
		public static WarningDialogs WarningDialog => WarningDialogs.Instance;
		public static ApplicationSettings ApplicationSettings => ApplicationSettings.Instance;
		public static CoordinateNumbers CoordinateNumbers => CoordinateNumbers.Instance;
		public static MaterialLibrary MaterialLibrary => MaterialLibrary.Instance;
		public static VisualGrid VisualGrid => VisualGrid.Instance;
		public static RenderSettings RenderSettings => RenderSettings.Instance;

	}
}