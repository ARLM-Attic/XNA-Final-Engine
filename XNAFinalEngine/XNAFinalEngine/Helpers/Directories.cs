
#region License
/*

 Based in the class Directories.cs from RacingGame.
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

namespace XNAFinalEngine.Helpers
{
	/// <summary>
	/// Helper class that stores the name of all used directories.
	/// </summary>
	class Directories
	{

		#region Game data directory

		/// <summary>
		/// We can use this to relocate the whole data of the game to another location.
		/// </summary>
		private const string GameDataDirectory = "Content\\";

		#endregion

		#region Directories

		/// <summary>
		/// Textures directory
		/// </summary>
        public static string TexturesDirectory { get { return GameDataDirectory + "Textures"; } }
		
		/// <summary>
		/// Models directory
		/// </summary>
		public static string ModelsDirectory { get { return GameDataDirectory + "Models"; } }

		/// <summary>
		/// Shaders directory
		/// </summary>
		public static string ShadersDirectory { get { return GameDataDirectory + "Shaders"; } }
        				
		/// <summary>
		/// Screenshots directory.
		/// </summary>
		public static string ScreenshotsDirectory { get { return GameDataDirectory + "Screenshots"; } }

        /// <summary>
		/// Sounds directory
		/// </summary>
		public static string SoundsDirectory { get { return GameDataDirectory + "Sounds"; } }

        /// <summary>
        /// Music directory
        /// </summary>
        public static string MusicDirectory { get { return GameDataDirectory + "Music"; } }

        /// <summary>
        /// Font directory
        /// </summary>
        public static string FontsDirectory { get { return GameDataDirectory + "Fonts"; } }

        /// <summary>
        /// Video directory
        /// </summary>
        public static string VideosDirectory { get { return GameDataDirectory + "Videos"; } }
        		
		#endregion

	} // Directories
} // XNAFinalEngine.Helpers
