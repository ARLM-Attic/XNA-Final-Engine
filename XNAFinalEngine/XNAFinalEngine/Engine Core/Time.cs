
#region License
/*
-----------------------------------------------------------------------------------------------------------------------------------------------
Authors: Schneider, José Ignacio (jis@cs.uns.edu.ar) under XNA Final Engine license - New BSD License (BSD)
 *       Schefer, Gustavo Martin (gusschefer@hotmail.com) under Microsoft Permissive License (Ms-PL)
-----------------------------------------------------------------------------------------------------------------------------------------------
*/
#endregion

#region Using directives
using System.Linq;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The interface to get time information from XNA Final Engine.
    /// </summary>
    public static class Time
    {

        #region Variables

        /// <summary>
        /// Is the game paused?
        /// </summary>
        private static bool paused;

        /// <summary>
        /// Delta times.
        /// </summary>
        private static float gameDeltaTime, frameTime;

        /// <summary>
        /// The scale at which the time is passing. This can be used for slow motion effects.
        /// </summary>
        private static float timeScale = 1;

        /// <summary>
        /// For more accurate frames per second calculations, just count for one second, then frameCountLastSecond is updated.
        /// Start with 1 to help some tests avoid the devide through zero problem.
        /// </summary>
        private static int frameCountThisSecond,
                           totalFrameCount,
                           frameCountLastSecond = 1;

        /// <summary>
        /// It counts the elapsed time from the last frame per second update. When it reaches 1 second the frames per second value is updated.
        /// </summary>
        private static float framePerSecondTimeHelper;

        /// <summary>
        /// Previous frame time values. Used for smooth frame time calculations.
        /// </summary>
        private static readonly float[] previousFrameTime = new float[] { 0, 0 };

        #endregion

        #region Properties

        /// <summary>
        /// The amount of time in seconds since the application started.
        /// The system starts counting from the first update. 
        /// Time scale does not affect this value.
        /// </summary>
        public static float ApplicationTime { get; private set; }

        /// <summary>
        /// The time in seconds it took to complete the last rendered frame.
        /// </summary>
        public static float FrameTime
        {
            get { return frameTime; }
            internal set
            {
                frameTime = !paused ? value * TimeScale : 0;
                
                // Update smooth frame time.
                SmoothFrameTime = (previousFrameTime[0] + previousFrameTime[1] + frameTime) / previousFrameTime.Length + 1;
                previousFrameTime[1] = previousFrameTime[0];
                previousFrameTime[0] = frameTime;

                // Update timer with the real elapsed time (not scaled)
                framePerSecondTimeHelper += value;
                // Update frame count.
                frameCountThisSecond++;
                totalFrameCount++;
                // One second elapsed?
                if (framePerSecondTimeHelper > 1)
                {
                    // Calculate frames per second
                    frameCountLastSecond = (int)(frameCountThisSecond / (framePerSecondTimeHelper));
                    // Reset startSecondTick and repaintCountSecond
                    framePerSecondTimeHelper = 0;
                    frameCountThisSecond = 0;
                }                
            }
        } // FrameTime

        /// <summary>
        /// Smooth Frame Time limits the effect of sudden fluctuations in frame time.
        /// The values are averaged over several frames.
        /// </summary>
        public static float SmoothFrameTime { get; private set; }

        /// <summary>
        /// The interval in seconds at which physics and other fixed frame rate updates.
        /// </summary>
        public static float GameDeltaTime
        {
            get { return gameDeltaTime; }
            internal set
            {
                if (!paused)
                {
                    gameDeltaTime = value * TimeScale;
                    GameTotalTime += gameDeltaTime;
                }
                else
                    gameDeltaTime = 0;
                // Update application time with the real elapsed time (not scaled)
                ApplicationTime += value;
            }
        } // GameDeltaTime

        /// <summary>
        /// Total played time.
        /// Time scale and pause affect this counter. Moreover the counter could be reset.
        /// </summary>
        public static float GameTotalTime { get; private set; }

        /// <summary>
        /// The scale at which the time is passing. This can be used for slow motion effects.
        /// Except for ApplicationTime, TimeScale affects all the time and delta time measuring variables of the Time class.
        /// </summary>
        public static float TimeScale
        {
            get { return timeScale; }
            set { timeScale = value; }
        } // TimeScale

        /// <summary>
        /// Frames per second.
        /// </summary>
        public static int FramesPerSecond { get { return frameCountLastSecond; } }

        /// <summary>
        /// Total frames count.
        /// </summary>
        public static int TotalFramesCount { get { return totalFrameCount; } }

        #endregion

        #region Reset Game Time

        /// <summary>
        /// Reset game total time.
        /// </summary>
        public static void ResetGameTotalTime()
        {
            GameTotalTime = 0;
        } // ResetGameTime

        #endregion

        #region Reset Total Frame Count

        /// <summary>
        /// Reset Total Frame Count.
        /// </summary>
        public static void ResetTotalFrameCount()
        {
            totalFrameCount = 0;
        } // ResetTotalFrameCount

        #endregion

        #region Pause and Resume

        /// <summary>
        /// Pause Game Time.
        /// Important: this value does not affect the time scale.
        /// </summary>
        public static void PauseGame()
        {
            paused = true;
        } // PauseGame

        /// <summary>
        /// Resume Game Time.
        /// Important: this value does not affect the time scale.
        /// </summary>
        public static void ResumeGame()
        {
            paused = false;
        } // ResumeGame

        #endregion

    } // Time
} // XNAFinalEngine.EngineCore