
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
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Song = XNAFinalEngine.Assets.Song;
#endregion

namespace XNAFinalEngine.Audio
{

    /// <summary>
    /// All the music is managed here.
    /// We can play, pause, stop, and change song (next, previous, or a user selected).
    /// There is also a shuffle option.
    /// </summary>
    /// <remarks>
    /// Because a content processor bug the song information (name, artist) can't be extracted. 
    /// Because of that it uses the file name to extract this information. 
    /// Filename format: index - artist - song name, where index is the song number in the list.
    /// However the filename format: "artist - song name" works.
    /// </remarks>
    public static class MusicManager
    {

        #region Variables
                
        // Music Volume. Range between 0.0f to 1.0f.        
        private static float volume = 0.8f;

        // Music directory's Song's filenames.
        private static string[] songsFilename;
        
        // The loaded songs.
        private static Dictionary<string, Song> loadedSongs;
                
        // Current index from the song's array.        
        private static int currentIndex;
                
        // Last index from the song's array, it's needed by the random player.
        private static int lastPlayedIndex;
                
        // Music content manager.        
        private static Assets.ContentManager musicContentManager;
                
        // Random generation for shuffle.
        // If you use Random class but keep newly constructing it with 'new Random()' in a tight loop, you'll get the same values for a while.
        // This is because the initial state of the Random class comes from the system clock, but a tight loop doesn't let the clock's value change.        
        private static readonly Random random = new Random();

        #endregion

        #region Properties
        
        /// <summary>
        ///  A list with the songs' filenames of the music directory.
        /// </summary>
        public static string[] SongsFilename { get { return songsFilename; } }

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
        public static Song CurrentSong { get; private set; }

        /// <summary>
        /// Shuffle on or off.
        /// </summary>
        public static bool Shuffle { get; set; }

        /// <summary>
        /// Is the music active?
        /// </summary>
        public static bool IsPlaying { get; private set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Search the song files available and loads the first that it found.
        /// </summary>
        internal static void Initialize()
        {
            // Search the song files //
            DirectoryInfo musicDirectory = new DirectoryInfo(Assets.ContentManager.GameDataDirectory + "Music");
            try
            {
                FileInfo[] songsFileInformation = musicDirectory.GetFiles("*.xnb");
                songsFilename = new string[songsFileInformation.Length];
                for (int i = 0; i < songsFileInformation.Length; i++)
                {
                    songsFilename[i] = songsFileInformation[i].Name.Substring(0, songsFileInformation[i].Name.Length - 4);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Do nothing.
                songsFilename = new string[0];
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
        /// Load a all songs of the music directoy. The songs previously loaded are deleted.
        /// </summary>
        /// <remarks>
        /// If you don’t want to waste valuable space you can load the songs one at a time but the songs will be loaded in game,
        /// that means a noticeable slow down every time a song is loaded. This is the default behavior.
        /// However, the music system allows you to load a chunk of songs at the same time. But be careful of the memory allocation. 
        /// </remarks>
        public static void LoadAllSong()
        {
            // Save the current content manager.
            Assets.ContentManager userContentManager = Assets.ContentManager.CurrentContentManager;
            // Prepare the music content manager.
            if (musicContentManager != null)
                musicContentManager.Unload();
            else
                musicContentManager = new Assets.ContentManager();

            loadedSongs = new Dictionary<string, Song>(songsFilename.Length);
            for (int i = 0; i < songsFilename.Length; i++)
            {
                try
                {
                    loadedSongs[songsFilename[i]] = new Song(songsFilename[i]);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Music Manager: Failed to load song: " + songsFilename[i], e);
                }
            }
            // Restore the user content manager.
            Assets.ContentManager.CurrentContentManager = userContentManager;
        } // LoadAllSong

        /// <summary>
        /// Load a list of songs. The songs previously loaded are deleted.
        /// </summary>
        /// <remarks>
        /// If you don’t want to waste valuable space you can load the songs one at a time but the songs will be loaded in game,
        /// that means a noticeable slow down every time a song is loaded. This is the default behavior.
        /// However, the music system allows you to load a chunk of songs at the same time. But be careful of the memory allocation. 
        /// </remarks>
        public static void LoadSongs(string[] songs)
        {
            if (songs == null)
                throw new ArgumentNullException("songs");

            // Save the current content manager.
            Assets.ContentManager userContentManager = Assets.ContentManager.CurrentContentManager;
            // Prepare the music content manager.
            if (musicContentManager != null)
                musicContentManager.Unload();
            else
                musicContentManager = new Assets.ContentManager();

            loadedSongs = new Dictionary<string, Song>(songs.Length);
            for (int i = 0; i < songs.Length; i++)
            {
                try
                {
                    loadedSongs[songs[i]] = new Song(songs[i]);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Music Manager: Failed to load song: " + songs[i], e);
                }
            }
            // Restore the user content manager.
            Assets.ContentManager.CurrentContentManager = userContentManager;
        } // LoadAllSong

        /// <summary>
        /// Load songs one at time.
        /// </summary>
        /// <remarks>
        /// If you don’t want to waste valuable space you can load the songs one at a time but the songs will be loaded in game,
        /// that means a noticeable slow down every time a song is loaded. This is the default behavior.
        /// However, the music system allows you to load a chunk of songs at the same time. But be careful of the memory allocation. 
        /// </remarks>
        public static void LoadSongsOneAtTime()
        {
            loadedSongs = null;
        } // LoadSongsOneAtTime

        /// <summary>
        /// Load song.
        /// </summary>
        private static Song LoadSong(string _currentSongFilename)
        {
            try
            {
                // Save the current content manager.
                Assets.ContentManager userContentManager = Assets.ContentManager.CurrentContentManager;
                
                if (musicContentManager != null)
                    musicContentManager.Unload();
                else
                    // Creates a new content manager and load the new song.
                    musicContentManager = new Assets.ContentManager();
                Assets.ContentManager.CurrentContentManager = musicContentManager;
                Song song = new Song(_currentSongFilename);

                // Restore the user content manager.
                Assets.ContentManager.CurrentContentManager = userContentManager;

                return song;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Music Manager: Failed to load song: " + _currentSongFilename, e);
            }
        } // LoadSong

        #endregion

        #region Play Pause Stop

        public static void Play(string name)
        {
            
        }
                
        /// <summary>
        /// Play the song from the start.
        /// </summary>
        public static void Play(int index = 0)
        {
            // If there are not songs or the loaded songs are empty
            if (songsFilename.Length == 0 || (loadedSongs != null && loadedSongs.Count == 0))
                return;
            
            // Check index
            if (loadedSongs != null && loadedSongs.Count <= index)
                throw new ArgumentOutOfRangeException("index");
            if (songsFilename != null && songsFilename.Length <= index)
                throw new ArgumentOutOfRangeException("index");

            lastPlayedIndex = currentIndex;
            currentIndex = index;

            if (loadedSongs != null)
                CurrentSong = LoadSong(loadedSongs[index]);
            else
                CurrentSong = LoadSong(songsFilename[index]);
            
            IsPlaying = true;
            try
            {
                MediaPlayer.Play(CurrentSong.Resource);
            }
            catch (Exception)
            {
                IsPlaying = false;
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
                    while (nextIndex == lastPlayedIndex || nextIndex == currentIndex)
                    {
                        nextIndex++;
                        if (nextIndex == songsFileInformation.Length) nextIndex = 0;
                    }
                    Play(nextIndex);
                }
                else
                {
                    if (currentIndex + 1 > songsFileInformation.Length - 1)
                    {
                        Play(0);
                    }
                    else
                        Play(currentIndex + 1);
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
                if (currentIndex - 1 < 0)
                {
                    Play(songsFileInformation.Length - 1);
                }
                else
                    Play(currentIndex - 1);
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
