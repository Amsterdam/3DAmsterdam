using UnityEngine;
using System.Runtime.InteropServices;

namespace Amsterdam3D.JavascriptConnection
{
	public class JavascriptMethodCaller
	{
		[DllImport("__Internal")]
		private static extern void DisplayDOMObjectWithID(string id = "htmlID", string display = "none", int x = 0, int y = 0, int width = 0, int height = 0);

		[DllImport("__Internal")]
		private static extern string FetchOBJData();

		[DllImport("__Internal")]
		private static extern string FetchMTLData();

		[DllImport("__Internal")]
		private static extern string DisplayUniqueShareURL(string uniqueToken = "");

		[DllImport("__Internal")]
		private static extern string HideUniqueShareURL();

		[DllImport("__Internal")]
		private static extern string SetCSSCursor(string cursorName = "pointer");

		[DllImport("__Internal")]
		private static extern string OpenURLInNewWindow(string openUrl = "https://");

		/// <summary>
		/// Some interface items are drawn as HTML DOM elements on top of the Unity3D canvas.
		/// This methods scales those elements with the Unity canvas.
		/// </summary>
		/// <param name="scale">The new multiplier value for the UI scale</param>
		/// <returns></returns>
		[DllImport("__Internal")]
		private static extern string ChangeInterfaceScale(float scale);

		/// <summary>
		/// This methods activates the html hitarea for the file upload button.
		/// The user will click the hidden file input dialog in the index.html template file that is drawn on top of our WebGL canvas.
		/// This overcomes the problem with browser security where a click is required to open a file upload dialog.
		/// Faking a click through JavaScript is not allowed.
		/// </summary>
		/// <param name="display">Sets the hitarea CSS of the input HTML node to inline, or none</param>
		public static void DisplayWithID(string id, bool display, int x = 0, int y = 0, int width = 0, int height = 0 )
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			 DisplayDOMObjectWithID(id,(display) ? "inline" : "none", x, y, width, height);
#endif
		}

		public static void ShowUniqueShareToken(bool show, string uniqueToken = "")
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			 if(show){
				DisplayUniqueShareURL(uniqueToken);
			 }
			 else {
				HideUniqueShareURL();
			}
#endif
		}

		public static string FetchOBJDataAsString()
		{
			return FetchOBJData();
		}
		public static string FetchMTLDataAsString()
		{
			return FetchMTLData();
		}

		public static void SetInterfaceScale(float scale)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			ChangeInterfaceScale(scale);
#endif
		}

		public static void ChangeCursor(string cursorName)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
				SetCSSCursor(cursorName);
#endif
		}

		public static void OpenURL(string url)
		{
#if UNITY_EDITOR
			Application.OpenURL(url);
#elif UNITY_WEBGL && !UNITY_EDITOR
			OpenURLInNewWindow(url);
#endif
		}
	}
}