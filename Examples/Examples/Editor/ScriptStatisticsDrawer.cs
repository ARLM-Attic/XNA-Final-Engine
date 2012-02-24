
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Draw a series of graph with statistics like frames per second, draw calls, etc.
    /// </summary>
    public class ScriptStatisticsDrawer : Script
    {

        #region Variables

        private const int // Common Parameters.
                          linesWidth = 5,
                          elementsHeight = 100,
                          // Frames per second parameters.
                          framesPerSecondNumberOfElements = 30,
                          framesPerSecondX = 20,
                          framesPerSecondY = 10,
                          framesPerSecondBadThreshold = 20,
                          framesPerSecondGoodThreshold = 40;

        // I want to implement an auto scale feature, to make the code more readable and easy I store the values here.
        // I can improve the performance using a circular array but here the performance is not critical.
        private readonly int[] framesPerSecondValues = new int[framesPerSecondNumberOfElements];

        // I use a chronometer to update the frames per second graph one time for second.
        private static Chronometer framesPerSecondChronometer;

        // The component used to render the graphs.
        private static GameObject2D framesPerSecondLines, framesPerSecondBackground, framesPerSecondText;

        #endregion

        #region Load

        /// <summary>
        /// Check that it works in 3D space and a camera component exists.
        /// </summary>
        public override void Load()
        {

            #region Frames Per Second

            // Black background
            Rectangle screenRect = new Rectangle(framesPerSecondX - 5, framesPerSecondY - 5, linesWidth * framesPerSecondNumberOfElements + 10, elementsHeight + 25);
            framesPerSecondBackground = new GameObject2D();
            framesPerSecondBackground.AddComponent<LineRenderer>();
            framesPerSecondBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            framesPerSecondBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.6f);
            framesPerSecondBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, -0.1f), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            
            // Graph
            framesPerSecondLines = new GameObject2D();
            framesPerSecondLines.AddComponent<LineRenderer>();
            framesPerSecondLines.LineRenderer.Vertices = new VertexPositionColor[framesPerSecondNumberOfElements * 2 + 6];
            for (int i = 0; i < framesPerSecondNumberOfElements - 1; i++)
            {
                framesPerSecondLines.LineRenderer.Vertices[i * 2]     = new VertexPositionColor(new Vector3(framesPerSecondX + i * linesWidth, framesPerSecondY + elementsHeight, 0), Color.Red);
                framesPerSecondLines.LineRenderer.Vertices[i * 2 + 1] = new VertexPositionColor(new Vector3(framesPerSecondX + (i + 1) * linesWidth, framesPerSecondY + elementsHeight, 0), Color.Red);
            }
            // White lines
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 - 2] = new VertexPositionColor(new Vector3(framesPerSecondX, framesPerSecondY, 0), new Color(1f, 1f, 1f, 0));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 - 1] = new VertexPositionColor(new Vector3(framesPerSecondX, framesPerSecondY + elementsHeight + 1, 0), Color.White);
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2]     = new VertexPositionColor(new Vector3(framesPerSecondX, framesPerSecondY + elementsHeight + 1, 0), Color.White);
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 1] = new VertexPositionColor(new Vector3(framesPerSecondX + linesWidth * framesPerSecondNumberOfElements, framesPerSecondY + elementsHeight + 1, 0), new Color(1f, 1f, 1f, 0));
            // Bad and Good Threshold Lines
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 2] = new VertexPositionColor(new Vector3(framesPerSecondX, framesPerSecondY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 3] = new VertexPositionColor(new Vector3(framesPerSecondX + linesWidth * framesPerSecondNumberOfElements, framesPerSecondY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 4] = new VertexPositionColor(new Vector3(framesPerSecondX, framesPerSecondY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 5] = new VertexPositionColor(new Vector3(framesPerSecondX + linesWidth * framesPerSecondNumberOfElements, framesPerSecondY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            // I use a chronometer to update the frames per second graph one time for second.
            framesPerSecondChronometer = new Chronometer(Chronometer.TimeSpaceEnum.FrameTime);
            framesPerSecondChronometer.Start();
            // Frames Per Second Text
            framesPerSecondText = new GameObject2D();
            framesPerSecondText.AddComponent<HudText>();
            framesPerSecondText.HudText.Text.Append("Frames Per Second ");
            framesPerSecondText.Transform.LocalPosition = new Vector3(framesPerSecondX + 10, framesPerSecondY + elementsHeight + 5, 0);

            #endregion
            
        } // Load

        #endregion

        #region Render

        /// <summary>
        /// Tasks executed during the first stage of the scene render.
        /// </summary>
        public override void PreRenderUpdate()
        {
            // The frames per second graph is updated one time for second.
            if (framesPerSecondChronometer.ElapsedTime > 1)
            {
                framesPerSecondChronometer.Reset();
                // Update values and find max value
                int framesPerSecondMaxValue = 0; // The scale of the graph will be framesPerSecondMaxValue.
                for (int i = 0; i < framesPerSecondNumberOfElements - 1; i++)
                {
                    framesPerSecondValues[i] = framesPerSecondValues[i + 1];
                    if (framesPerSecondValues[i] > framesPerSecondMaxValue)
                        framesPerSecondMaxValue = framesPerSecondValues[i];
                }
                framesPerSecondValues[framesPerSecondNumberOfElements - 1] = Time.FramesPerSecond;
                if (framesPerSecondValues[framesPerSecondNumberOfElements - 1] > framesPerSecondMaxValue)
                    framesPerSecondMaxValue = framesPerSecondValues[framesPerSecondNumberOfElements - 1];
                // Update graph
                for (int i = 0; i < framesPerSecondNumberOfElements - 1; i++)
                {
                    UpdateFramesPerSecondLines(i, 0, framesPerSecondMaxValue);
                    UpdateFramesPerSecondLines(i, 1, framesPerSecondMaxValue);
                }
                framesPerSecondText.HudText.Text.Length = 18;
                framesPerSecondText.HudText.Text.AppendWithoutGarbage(Time.FramesPerSecond);
                // Threshold Lines
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 2].Position.Y = framesPerSecondY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondBadThreshold / framesPerSecondMaxValue));
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 3].Position.Y = framesPerSecondY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondBadThreshold / framesPerSecondMaxValue));
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 4].Position.Y = framesPerSecondY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondGoodThreshold / framesPerSecondMaxValue));
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 5].Position.Y = framesPerSecondY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondGoodThreshold / framesPerSecondMaxValue));
            }
        } // PreRenderUpdate

        #endregion

        #region Update Frames Per Second Lines

        /// <summary>
        /// Updates the lines with the new values and also calculates their colors.
        /// </summary>
        /// <param name="index">The element or line segment to update.</param>
        /// <param name="offset">A line is form by two points. This indicates if it is the first point or the last.</param>
        /// <param name="framesPerSecondMaxValue">The maximum value is used to scale the graph.</param>
        private void UpdateFramesPerSecondLines(int index, int offset, int framesPerSecondMaxValue)
        {
            framesPerSecondLines.LineRenderer.Vertices[index * 2 + offset].Position.Y = framesPerSecondY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondValues[index + offset] / framesPerSecondMaxValue));
            if (framesPerSecondValues[index + offset] < framesPerSecondBadThreshold)
                framesPerSecondLines.LineRenderer.Vertices[index * 2 + offset].Color = Color.Red;
            else if (framesPerSecondValues[index + offset] > framesPerSecondGoodThreshold)
                framesPerSecondLines.LineRenderer.Vertices[index * 2 + offset].Color = new Color(100, 255, 100);
            else
            {
                if (framesPerSecondValues[index + offset] < framesPerSecondGoodThreshold - ((framesPerSecondGoodThreshold - framesPerSecondBadThreshold) / 2))
                {
                    // Mix red with yellow
                    framesPerSecondLines.LineRenderer.Vertices[index * 2 + offset].Color = Color.Lerp(Color.Red, Color.Yellow, (framesPerSecondValues[index + offset] - framesPerSecondBadThreshold) / (float)((framesPerSecondGoodThreshold - framesPerSecondBadThreshold) / 2));
                }
                else
                {
                    // Mix green with yellow
                    framesPerSecondLines.LineRenderer.Vertices[index * 2 + offset].Color = Color.Lerp(new Color(100, 255, 100), Color.Yellow, (framesPerSecondGoodThreshold - framesPerSecondValues[index + offset]) / (float)((framesPerSecondGoodThreshold - framesPerSecondBadThreshold) / 2));
                }
            }
        } // UpdateFramesPerSecondLines

        #endregion

    } // ScriptStatisticsDrawer
} // XNAFinalEngine.Editor

