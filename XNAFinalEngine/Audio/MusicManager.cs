
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
using XNAFinalEngine.Assets;
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
    /// I research a little how some popular engines managed music. For example, in Cry Engine 3 the music system is very powerful but at the same time very complex.
    /// What I try to accomplish instead is a music system that works in the majority of the scenarios
    /// but in a simple way and allowing the user to implement the missing parts via scripts or logic code.
    /// 
    /// How it works…
    /// You load the songs into the music manager’s playlist using the LoadSongs methods.
    /// Moreover, you could indicate if the assets are loaded immediately or if they are loading in real time. 
    /// Then you play the song via their index or the filename. You can also stop, pause, resume, call the next song or the previous, and loop a song.
    /// The songs are loaded in the current content manager and the music manager stores the content manager used to load them
    /// so that the user could remove songs of the playlist indicating a content manager.
    /// 
    /// Because a content processor bug the song information (name, artist) can't be extracted.
    /// Because of that it uses the file name to extract this information.
    /// Filename format: index - artist - song name, where index is the song number in the list.
    /// However the filename format: "artist - song name" works.
    /// 
    /// If you need a more memory efficient music manager you could try streaming wav files.
    /// http://msdn.microsoft.com/en-us/library/ff827591.aspx
    /// </remarks>
    public static class MusicManager
    {

        #region Structs

        /// <summary>
        /// When a song is loaded it has an order in the playlist and it’s identified by its filename.
        /// The song’s resource could be loaded in real time or in the load stage.
        /// Finally the content manager that loads the song resource is stored so that the song’s dispose could be done easily.
        /// </summary>
        public struct LoadedSong
        {
            public string Filename;
            public Song Asset;
            public ContentManager ContentManager;
        } // LoadedSong

        #endregion

        #region Variables

        // The playlist.
        private static LoadedSong[] playlist = new LoadedSong[0];

        // Music Volume. Range between 0.0f to 1.0f.        
        private static float masterVolume = 0.8f;

        // Song Volume. Range between 0.0f to 1.0f.        
        private static float songVolume = 1.0f;
        
        // Current index from the song's array.        
        private static int currentIndex;
                
        // Last index from the song's array, it's needed by the random player.
        private static int lastPlayedIndex;
                
        // Random generation for shuffle.
        // If you use Random class but keep newly constructing it with 'new Random()' in a tight loop, you'll get the same values for a while.
        // This is because the initial state of the Random class comes from the system clock, but a tight loop doesn't let the clock's value change.        
        private static readonly Random random = new Random();

        // A content manager to store temporal songs that are loaded in real time.
        private static ContentManager musicContentManager;

        #endregion

        #region Properties

        #region Volume

        /// <summary>
        /// Music Volume.
        /// </summary>
        /// <value>Volume, ranging from 0.0f (silence) to 1.0f (full volume).</value>
        /// <remarks>
        /// This value is meant for the user and the song volume is meant for the developer.
        /// The objective is to have an easy way to change the song volume (for example: fade) without losing the master volume.
        /// </remarks>
        public static float MasterVolume
        {
            get { return masterVolume; }
            set
            {
                masterVolume = value;
                if (masterVolume < 0)
                    masterVolume = 0;
                if (masterVolume > 1)
                    masterVolume = 1;
            }
        } // MusicVolume

        /// <summary>
        /// Song Volume.
        /// </summary>
        /// <value>Volume, ranging from 0.0f (silence) to 1.0f (full volume).</value>
        /// <remarks>
        /// This value is meant for the developer and the master volume is meant for the user.
        /// The objective is to have an easy way to change the song volume (for example: fade) without losing the master volume.
        /// </remarks>
        public static float SongVolume
        {
            get { return songVolume; }
            set
            {
                songVolume = value;
                if (songVolume < 0)
                    songVolume = 0;
                if (songVolume > 1)
                    songVolume = 1;
            }
        } // SongVolume

        #endregion
        
        /// <summary>
        ///  A list with all songs' filenames on the music directory.
        /// </summary>
        public static string[] SongsFilename { get; private set; }

        /// <summary>
        /// Playlist. Just for reading.
        /// </summary>
        public static LoadedSong[] Playlist { get { return playlist; } }
        
        /// <summary>
        /// Current song.
        /// </summary>
        public static Song CurrentSong { get; private set; }

        /// <summary>
        /// Current song filename.
        /// </summary>
        public static string CurrentSongFilename { get; private set; }

        /// <summary>
        /// Shuffle on or off.
        /// </summary>
        public static bool Shuffle { get; set; }

        /// <summary>
        /// Is the music active?
        /// </summary>
        public static bool IsPlaying { get; private set; }

        /// <summary>
        /// True if the song loops.
        /// </summary>
        public static bool Loop { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void SongEventHandler();

        /// <summary>
        /// Raised when the game object's is enabled or disabled.
        /// </summary>
        public static event SongEventHandler SongEnded;

        /// <summary>
        /// Raised when the game object's is enabled or disabled.
        /// </summary>
        public static event SongEventHandler SongStarted;

        #endregion

        #region Initialize

        /// <summary>
        /// Search the song files available and loads the first that it found.
        /// </summary>
        internal static void Initialize()
        {
            // Search the song files //
            DirectoryInfo musicDirectory = new DirectoryInfo(ContentManager.GameDataDirectory + "Music");
            try
            {
                FileInfo[] songsFileInformation = musicDirectory.GetFiles("*.xnb");
                SongsFilename = new string[songsFileInformation.Length];
                for (int i = 0; i < songsFileInformation.Length; i++)
                {
                    FileInfo songFileInformation = songsFileInformation[i];
                    SongsFilename[i] = songFileInformation.Name.Substring(0, songFileInformation.Name.Length - 4);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Do nothing.
                SongsFilename = new string[0];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Music Manager: Unable to retrieve music information from files.", e);
            }
            // This class creates a list of only a single song at a time. Because of this the list can’t repeat itself.
            MediaPlayer.IsRepeating = false;
            MediaPlayer.IsVisualizationEnabled = false;
            IsPlaying = false;
            Shuffle = false;
        } // Initialize

        #endregion

        #region Load Songs

        /// <summary>
        /// Load all songs of the music directoy into the playlist.
        /// </summary>
        /// <remarks>
        /// If you don’t want to waste valuable space you can load the songs one at a time but the songs will be loaded in game,
        /// that means a noticeable slow down every time a song is loaded.
        /// However, the music system allows you to load a chunk of songs at the same time. But be careful of the memory allocation. 
        /// This is the default behavior and it is controled with the preload value.
        /// </remarks>
        public static void LoadAllSong(bool preload = true)
        {
            playlist = new LoadedSong[SongsFilename.Length];
            int i = 0;
            foreach (var songFilename in SongsFilename)
            {
                try
                {
                    playlist[i] = new LoadedSong { Filename = songFilename, Asset = preload ? new Song(songFilename) : null, ContentManager = ContentManager.CurrentContentManager };
                    i++;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Music Manager: Failed to load song: " + songFilename, e);
                }
            }
        } // LoadAllSong

        /// <summary>
        /// Load a list of songs into the playlist.
        /// </summary>
        /// <remarks>
        /// If you don’t want to waste valuable space you can load the songs one at a time but the songs will be loaded in game,
        /// that means a noticeable slow down every time a song is loaded.
        /// However, the music system allows you to load a chunk of songs at the same time. But be careful of the memory allocation. 
        /// This is the default behavior and it is controled with the preload value.
        /// </remarks>
        public static void LoadSongs(string[] songsFilename, bool preload = true)
        {
            if (songsFilename == null)
                throw new ArgumentNullException("songsFilename");
            
            List<LoadedSong> playlistTemp = new List<LoadedSong>();
            foreach (var songFilename in songsFilename)
            {
                try
                {
                    bool songExist = false;
                    for (int j = 0; j < playlist.Length; j++)
                    {
                        songExist = songExist || playlist[j].Filename == songFilename;
                    }
                    if (!songExist)
                        playlistTemp.Add(new LoadedSong { Filename = songFilename, Asset = preload ? new Song(songFilename) : null, ContentManager = ContentManager.CurrentContentManager });
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Music Manager: Failed to load song: " + songFilename, e);
                }
            }
            LoadedSong[] newPlayList = new LoadedSong[playlist.Length + playlistTemp.Count];
            playlist.CopyTo(newPlayList, 0);
            playlistTemp.CopyTo(newPlayList, playlist.Length);
            playlist = newPlayList;
        } // LoadAllSong

        /// <summary>
        /// Load song in a temporal content manager.
        /// </summary>
        private static Song LoadSongInTemporalContentManager(string _currentSongFilename)
        {
            try
            {
                // Save the current content manager.
                ContentManager userContentManager = ContentManager.CurrentContentManager;
                
                if (musicContentManager != null)
                    musicContentManager.Unload();
                else
                    // Creates a new content manager and load the new song.
                    musicContentManager = new ContentManager();

                ContentManager.CurrentContentManager = musicContentManager;
                Song song = new Song(_currentSongFilename);

                // Restore the user content manager.
                ContentManager.CurrentContentManager = userContentManager;

                return song;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Music Manager: Failed to load song: " + _currentSongFilename, e);
            }
        } // LoadSongInTemporalContentManager

        #endregion

        #region Remove Songs

        /// <summary>
        /// Clear the playlist.
        /// </summary>
        public static void RemoveAllSong()
        {
            Stop();
            playlist = new LoadedSong[0];
        } // RemoveAllSong

        /// <summary>
        /// Remove songs from the playlist.
        /// </summary>
        public static void RemoveSongs(string[] songsFilename)
        {
            // First we count how much elements the new playlist will have.
            int elementCount = 0;
            for (int i = 0; i < playlist.Length; i++)
            {
                for (int j = 0; j < songsFilename.Length; j++)
                {
                    if (playlist[i].Filename == songsFilename[j])
                    {
                        // If we want to remove the current song we have to stop it first.
                        if (currentIndex == i)
                            Stop();
                    }
                    else
                    {
                        elementCount++;
                    }
                }
            }
            // Then we create the new playlist.
            LoadedSong[] newPlaylist = new LoadedSong[elementCount];
            int k = 0;
            for (int i = 0; i < playlist.Length; i++)
            {
                for (int j = 0; j < songsFilename.Length; j++)
                {
                    if (playlist[i].Filename != songsFilename[j])
                    {
                        newPlaylist[k] = playlist[i];
                        k++;
                    }
                }
            }
            playlist = newPlaylist;
        } // RemoveSongs

        /// <summary>
        /// Remove songs from the playlist.
        /// </summary>
        /// <remarks>
        /// If a content manager is unload and this content manager was used to load songs then this method is very useful for remove all the songs loaded in there.
        /// </remarks>
        public static void RemoveSongs(ContentManager contentManager)
        {
            // First we count how much elements the new playlist will have.
            int elementCount = 0;
            for (int i = 0; i < playlist.Length; i++)
            {
                if (playlist[i].ContentManager == contentManager)
                {
                    // If we want to remove the current song we have to stop it first.
                    if (currentIndex == i)
                        Stop();
                }
                else
                {
                    elementCount++;
                }
            }
            // Then we create the new playlist.
            LoadedSong[] newPlaylist = new LoadedSong[elementCount];
            int k = 0;
            for (int i = 0; i < playlist.Length; i++)
            {
                if (playlist[i].ContentManager != contentManager)
                {
                    newPlaylist[k] = playlist[i];
                    k++;
                }
            }
            playlist = newPlaylist;
        } // RemoveSongs

        #endregion

        #region Play Pause Stop Next and Previous

        /// <summary>
        /// Reproduces a song from the start.
        /// </summary>
        /// <param name="filename">Song's filename.</param>
        /// <param name="loop">True if the song loops.</param>
        public static void Play(string filename, bool loop = false)
        {
            // If there are not songs or the loaded songs are empty
            if (playlist.Length == 0)
                throw new InvalidOperationException("Music Manager: The playlist is empty.");
            for (int i = 0; i < playlist.Length; i++)
            {
                if (playlist[i].Filename == filename)
                {
                    Play(i + 1, loop);
                    return;
                }
            }
            throw new InvalidOperationException("Music Manager: The song is not present in the playlist.");
        } // Play

        /// <summary>
        /// Reproduces a song from the start.
        /// </summary>
        /// <param name="index">The values ranges between 1 to playlist lenght.</param>
        /// <param name="loop">True if the song loops.</param>
        public static void Play(int index = 1, bool loop = false)
        {
            // If there are not songs or the loaded songs are empty
            if (playlist.Length == 0)
                throw new InvalidOperationException("Music Manager: The playlist is empty.");
            // Check index
            if (index > playlist.Length || index <= 0)
                throw new ArgumentOutOfRangeException("index");

            int arrayIndex = index - 1;
            lastPlayedIndex = currentIndex;
            currentIndex = arrayIndex;

            if (playlist[arrayIndex].Asset != null)
                CurrentSong = playlist[arrayIndex].Asset;
            else
            {
                CurrentSong = LoadSongInTemporalContentManager(playlist[arrayIndex].Filename);
                CurrentSongFilename = playlist[arrayIndex].Filename;
            }

            Loop = loop;
            IsPlaying = true;
            try
            {
                MediaPlayer.Play(CurrentSong.Resource);
            }
            catch (Exception)
            {
                IsPlaying = false;
            }
            // Raise event.
            if (SongStarted != null)
                SongStarted();
        } // Play

        /// <summary>
        /// Stop the music
        /// </summary>
        public static void Stop()
        {
            MediaPlayer.Stop();
            IsPlaying = false;
            if (playlist[currentIndex].Asset == null)
            {
                musicContentManager.Unload();
            }
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
            IsPlaying = true;
        } // Resume

        /// <summary>
        /// Next Song.
        /// </summary>
        public static void Next()
        {
            if (playlist.Length > 0)
            {
                if (Shuffle && playlist.Length > 2)
                {
                    // Generate a random number
                    int nextIndex = random.Next(playlist.Length - 1);
                    // If is the same as the last played index or the current index then we take the next value.
                    while (nextIndex == lastPlayedIndex || nextIndex == currentIndex)
                    {
                        nextIndex++;
                        if (nextIndex == playlist.Length) 
                            nextIndex = 0;
                    }
                    Play(nextIndex + 1); // Changes the range from (0, playlist.Lenght - 1) to (1, playlist.Lenght)
                }
                else
                {
                    if (currentIndex + 1 >= playlist.Length)
                    {
                        Play(1);
                    }
                    else
                        Play(currentIndex + 2);
                }
            }
        } // Next

        /// <summary>
        /// Previous Song.
        /// </summary>
        public static void Previous()
        {
            if (playlist.Length > 0)
            {
                if (currentIndex == 0)
                {
                    Play(playlist.Length);
                }
                else
                    Play(currentIndex);
            }
        } // Previous

        #endregion

        #region Playlist Order

        /// <summary>
        /// Swap playlist elements.
        /// </summary>
        /// <param name="index1">The index of the first element.</param>
        /// <param name="index2">The index of the second element.</param>
        public static void SwapPlaylistElements(int index1, int index2)
        {
            LoadedSong temp = playlist[index1 - 1];
            playlist[index1 - 1] = playlist[index2 - 1];
            playlist[index2 - 1] = temp;
        } // SwapPlaylistElements

        /// <summary>
        /// Move a playlist element to a new position.
        /// </summary>
        /// <param name="oldindex">The index of the old position.</param>
        /// <param name="newIndex">The index of the new position.</param>
        public static void MovePlaylistElement(int oldindex, int newIndex)
        {
            // TODO!!!
        } // MovePlaylistElement

        #endregion

        #region Update

        /// <summary>
        /// Update music volume and change automatically to the next song when is needed, freeing resources if that was the case.
        /// </summary>
        public static void Update()
        {       
            if (playlist.Length > 0)
            {
                if (IsPlaying)
                {
                    MediaPlayer.Volume = MasterVolume * SongVolume;
                    if (!Loop)
                    {
                        // If the song is over then we raise the events.
                        if (MediaPlayer.State == MediaState.Stopped && IsPlaying)
                        {
                            if (playlist[currentIndex].Asset == null)
                            {
                                musicContentManager.Unload();
                            }
                            // Raise event
                            if (SongEnded != null)
                                SongEnded();
                        }
                        // Maybe the events changes the state of the player.
                        if (MediaPlayer.State == MediaState.Stopped && IsPlaying)
                        {
                            // The song is over; it's time to load the next.
                            Next();
                        }
                    }
                }
            }
        } // Update

        #endregion

    } // MusicManager
} // XNA2FinalEngine.Audio
