
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Audio
{

    /// <summary>
    /// All the music is managed here.
    /// We can also pause, stop, change song (next or previous or a select song) and we can on or off shuffle.
    /// 
    /// Because a content processor bug the song information (name, artist) can't be extracted. 
    /// Because of that it uses the file name to extract this information.
    /// 
    /// IMPORTANT!
    /// 
    /// File name format: index - Artist - song name, where index is the song number in the list.
    /// </summary>
    public static class MusicManager
    {

        #region Variables
                
        // Music Volume. Range between 0.0f to 1.0f.        
        private static float volume = 0.8f;
                
        // List of song files.
        private static FileInfo[] songsFileInformation;
                
        // Current music file index from the musicFiles array.        
        private static int currentMusicFileIndex;
                
        // Last music file index from the musicFiles array, it's needed by the random player.        
        private static int lastPlayedMusicFileIndex;
                
        // Music content manager.        
        private static Assets.ContentManager musicContentManager;
                
        // Random generation for shuffle.
        // If you use Random class but keep newly constructing it with 'new Random()' in a tight loop, you'll get the same values for a while.
        // This is because the initial state of the Random class comes from the system clock, but a tight loop doesn't let the clock's value change.        
        private static Random random = new Random();

        #endregion

        #region Properties
        
        /// <summary>
        ///  A list with the songs' information.
        /// </summary>
        private static FileInfo[] SongsFileInformation { get { return songsFileInformation; } }

        /// <summary>
        /// Music Volume. Range between 0.0f to 1.0f.
        /// </summary>
        public static float Volume
        {
            get { return volume; }
            set { volume = value; }
        } // MusicVolume

        /// <summary>
        /// Current song.
        /// </summary>
        public static XNAFinalEngine.Assets.Song CurrentSong { get; private set; }

        /// <summary>
        /// Shuffle on or off.
        /// </summary>
        public static bool Shuffle { get; set; }

        /// <summary>
        /// Is the music active?
        /// </summary>
        public static bool IsPlaying { get; private set; }
        /*
        /// <summary>
        /// Song's name (currently playing).
        /// public static String SongName { get { return currentSong.Album.Name; } } doesn't work. It's a content processor compilation bug.
        /// </summary>
        public static String CurrentSongName
        { 
            get
            {
                if (currentSong != null)
                {
                    return currentSongFilename.Split('-')[2];
                }
                return null;
            }
        } // CurrentSongName

        /// <summary>
        /// Song's artist (currenty playing).
        /// public static String SongArtist { get { return currentSong.Artist.Name; } } doesn't work. It's a content processor compilation bug.
        /// </summary>
        public static String CurrentSongArtistName
        {
            get
            {
                if (currentSongFilename != null)
                {
                    return currentSongFilename.Split('-')[1];
                }
                return null;
            }
        } // CurrentSongArtistName
        */
        #endregion

        #region Initialize

        /// <summary>
        /// Search the song files available and loads the first that it found.
        /// </summary>
        public static void Initialize()
        {
            // Search the song files //
            DirectoryInfo musicDirectory = new DirectoryInfo(Assets.ContentManager.GameDataDirectory + "Music");
            try
            {
                songsFileInformation = musicDirectory.GetFiles("*.xnb");
            }
            catch (DirectoryNotFoundException)
            {
                // Do nothing.
                songsFileInformation = new FileInfo[0];
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Music Manager: Unable to retrieve music information from files.");
            }
            // This class creates a list of only a single song at a time. Because of this the list can’t repeat itself.
            MediaPlayer.IsRepeating = false;
            MediaPlayer.IsVisualizationEnabled = false;
            IsPlaying = false;
            Shuffle = false;
        } // Initialize

        #endregion

        #region Load Song

        /// <summary>
        /// Load song.
        /// </summary>
        private static void LoadSong(string _currentSongFilename)
        {
            try
            {
                Assets.ContentManager userContentManager = Assets.ContentManager.CurrentContentManager;
                
                if (musicContentManager != null)
                    musicContentManager.Unload();
                else
                    // Creates a new content manager and load the new song.
                    musicContentManager = new Assets.ContentManager();
                Assets.ContentManager.CurrentContentManager = musicContentManager;
                CurrentSong = new Assets.Song(_currentSongFilename);

                Assets.ContentManager.CurrentContentManager = userContentManager;               
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Music Manager: Failed to load song: " + _currentSongFilename, e);
            }
        } // LoadSong

        #endregion

        #region Play Pause Stop
                
        /// <summary>
        /// Play the song from the start.
        /// </summary>
        public static void Play(int index = 0)
        {
            if (songsFileInformation.Length > 0)
            {
                lastPlayedMusicFileIndex = currentMusicFileIndex;
                currentMusicFileIndex = index;
                LoadSong(songsFileInformation[currentMusicFileIndex].Name.Substring(0, songsFileInformation[currentMusicFileIndex].Name.Length - 4));
                IsPlaying = true;
                try
                {
                    MediaPlayer.Play(CurrentSong.Resource);
                }
                catch (Exception)
                {
                    IsPlaying = false;
                }
            }
        } // Play

        /// <summary>
        /// Stop the music
        /// </summary>
        public static void Stop()
        {
            MediaPlayer.Stop();
            IsPlaying = false;
        } // Stop
                
        /// <summary>
        /// Pause the music
        /// </summary>
        public static void Pause()
        {
            MediaPlayer.Pause();
            IsPlaying = false;
        } // Pause

        /// <summary>
        /// Resume the music.
        /// </summary>
        public static void Resume()
        {
            MediaPlayer.Resume();            
        } // Resume

        /// <summary>
        /// Next Song
        /// </summary>
        public static void Next()
        {
            if (songsFileInformation.Length > 0)
            {
                if (Shuffle)
                {
                    int nextIndex = random.Next(songsFileInformation.Length);
                    while (nextIndex == lastPlayedMusicFileIndex || nextIndex == currentMusicFileIndex)
                    {
                        nextIndex++;
                        if (nextIndex == songsFileInformation.Length) nextIndex = 0;
                    }
                    Play(nextIndex);
                }
                else
                {
                    if (currentMusicFileIndex + 1 > songsFileInformation.Length - 1)
                    {
                        Play(0);
                    }
                    else
                        Play(currentMusicFileIndex + 1);
                }
            }
        } // Next

        /// <summary>
        /// Previous Song
        /// </summary>
        public static void Previous()
        {
            if (songsFileInformation.Length > 0)
            {
                if (currentMusicFileIndex - 1 < 0)
                {
                    Play(songsFileInformation.Length - 1);
                }
                else
                    Play(currentMusicFileIndex - 1);
            }
        } // Previous

        #endregion

        #region Update

        /// <summary>
        /// Update music volume and change automatically to the next song when is needed, freeing resources if that was the case.
        /// </summary>
        public static void Update()
        {       
            if (songsFileInformation.Length > 0)
            {
                if (IsPlaying)
                {
                    MediaPlayer.Volume = Volume;
                    if (MediaPlayer.State == MediaState.Stopped && IsPlaying)
                    {
                        // The song is over; it's time to load the next.
                        Next();
                    }
                }
            }
        } // Update

        #endregion

    } // MusicManager
} // XNA2FinalEngine.Audio
