
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

namespace XNAFinalEngine.EngineCore
{
    /// <summary>
    /// Draw a series of graph with statistics like frames per second, draw calls, etc.
    /// </summary>
    /// <remarks>
    /// It is inspired in the Xen Debug Graphs but it was written from scratch.
    /// </remarks>
    public class ScriptStatisticsDrawer : Script
    {

        #region Variables

        private const int // Common Parameters.
                          linesWidth = 5,
                          elementsHeight = 100,
                          positionX = 10,
                          positionY = 10,
                          // Frames per second parameters.
                          framesPerSecondNumberOfElements = 30,
                          framesPerSecondBadThreshold = 20,
                          framesPerSecondGoodThreshold = 40,
                          // Draw Calls
                          drawCallsNumberOfElements = 50,
                          drawCallsBadThreshold = 1000,
                          drawCallsGoodThreshold = 300,
                          // Triangles Drawn
                          trianglesDrawnNumberOfElements = 50,
                          trianglesDrawnBadThreshold = 3000000,
                          trianglesDrawnGoodThreshold = 1000000,
                          // Managed Memory Used
                          managedMemoryUsedNumberOfElements = 50,
                          managedMemoryUsedBadThreshold = 50000000,
                          managedMemoryUsedGoodThreshold = 10000000,
                          // Garbage Collections
                          garbageCollectionsNumberOfElements = 30;

        // I want to implement an auto scale feature, to make the code more readable and easy I store the values here.
        // I can improve the performance using a circular array but here the performance is not critical.
        private int[] framesPerSecondValues = new int[framesPerSecondNumberOfElements],
                      drawCallsValues       = new int[drawCallsNumberOfElements],
                      trianglesDrawnValues  = new int[trianglesDrawnNumberOfElements],
                      managedMemoryUsedValues = new int[trianglesDrawnNumberOfElements],
                      garbageCollectionsValues = new int[garbageCollectionsNumberOfElements];

        // I use a chronometer to update the frames per second graph one time for second.
        private static Chronometer chronometer;

        // The component used to render the graphs.
        private static GameObject2D framesPerSecondLines, framesPerSecondBackground, framesPerSecondText,
                                    drawCallsLines, drawCallsBackground, drawCallsText,
                                    trianglesDrawnLines, trianglesDrawnBackground, trianglesDrawnText,
                                    managedMemoryUsedLines, managedMemoryUsedBackground, managedMemoryUsedText,
                                    garbageCollectionsLines, garbageCollectionsBackground, garbageCollectionsText,
                                    userInterfaceGarbageBackground, userInterfaceGarbageText;

        #endregion

        #region Load

        /// <summary>
        /// Check that it works in 3D space and a camera component exists.
        /// </summary>
        public override void Load()
        {
            Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.6f);

            #region Frames Per Second

            // Black background
            Rectangle screenRect = new Rectangle(positionX - 5, positionY - 5, linesWidth * framesPerSecondNumberOfElements + 10, elementsHeight + 25);
            framesPerSecondBackground = new GameObject2D();
            framesPerSecondBackground.AddComponent<LineRenderer>();
            framesPerSecondBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            framesPerSecondBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            framesPerSecondBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, 0), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, 0), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, 0), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, 0), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, 0), backgroundColor);
            framesPerSecondBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, 0), backgroundColor);
            
            // Graph
            framesPerSecondLines = new GameObject2D();
            framesPerSecondLines.AddComponent<LineRenderer>();
            framesPerSecondLines.LineRenderer.Vertices = new VertexPositionColor[framesPerSecondNumberOfElements * 2 + 6];
            for (int i = 0; i < framesPerSecondNumberOfElements - 1; i++)
            {
                framesPerSecondLines.LineRenderer.Vertices[i * 2]     = new VertexPositionColor(new Vector3(positionX + i * linesWidth, positionY + elementsHeight, 0), Color.Red);
                framesPerSecondLines.LineRenderer.Vertices[i * 2 + 1] = new VertexPositionColor(new Vector3(positionX + (i + 1) * linesWidth, positionY + elementsHeight, 0), Color.Red);
            }
            // White lines
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 - 2] = new VertexPositionColor(new Vector3(positionX, positionY, 0), new Color(1f, 1f, 1f, 0));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 - 1] = new VertexPositionColor(new Vector3(positionX, positionY + elementsHeight + 1, 0), Color.White);
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2]     = new VertexPositionColor(new Vector3(positionX, positionY + elementsHeight + 1, 0), Color.White);
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 1] = new VertexPositionColor(new Vector3(positionX + linesWidth * framesPerSecondNumberOfElements, positionY + elementsHeight + 1, 0), new Color(1f, 1f, 1f, 0));
            // Bad and Good Threshold Lines
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 2] = new VertexPositionColor(new Vector3(positionX, positionY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 3] = new VertexPositionColor(new Vector3(positionX + linesWidth * framesPerSecondNumberOfElements, positionY + elementsHeight - framesPerSecondBadThreshold, 0), new Color(1, 0, 0, 0.1f));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 4] = new VertexPositionColor(new Vector3(positionX, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 5] = new VertexPositionColor(new Vector3(positionX + linesWidth * framesPerSecondNumberOfElements, positionY + elementsHeight - framesPerSecondGoodThreshold, 0), new Color(1, 1, 0, 0.1f));
            // I use a chronometer to update the frames per second graph one time for second.
            chronometer = new Chronometer(Chronometer.TimeSpaceEnum.FrameTime);
            chronometer.Start();
            // Frames Per Second Text
            framesPerSecondText = new GameObject2D();
            framesPerSecondText.AddComponent<HudText>();
            framesPerSecondText.HudText.Text.Append("Frames Per Second ");
            framesPerSecondText.Transform.LocalPosition = new Vector3(positionX, positionY + elementsHeight + 5, 0);

            #endregion
            
            #region Draw Calls

            int drawCallsPositionX = positionX + linesWidth * framesPerSecondNumberOfElements + 5 + 10;

            // Black background
            screenRect = new Rectangle(drawCallsPositionX - 5, positionY - 5, linesWidth * drawCallsNumberOfElements + 10, elementsHeight + 25);
            drawCallsBackground = new GameObject2D();
            drawCallsBackground.AddComponent<LineRenderer>();
            drawCallsBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            drawCallsBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            drawCallsBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, -0.1f), backgroundColor);
            drawCallsBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            drawCallsBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            drawCallsBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            drawCallsBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            drawCallsBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            
            // Graph
            drawCallsLines = new GameObject2D();
            drawCallsLines.AddComponent<LineRenderer>();
            drawCallsLines.LineRenderer.Vertices = new VertexPositionColor[drawCallsNumberOfElements * 2 + 6];
            for (int i = 0; i < drawCallsNumberOfElements - 1; i++)
            {
                drawCallsLines.LineRenderer.Vertices[i * 2]     = new VertexPositionColor(new Vector3(drawCallsPositionX + i * linesWidth, positionY + elementsHeight, 0), Color.Green);
                drawCallsLines.LineRenderer.Vertices[i * 2 + 1] = new VertexPositionColor(new Vector3(drawCallsPositionX + (i + 1) * linesWidth, positionY + elementsHeight, 0), Color.Green);
            }
            // White lines
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 - 2] = new VertexPositionColor(new Vector3(drawCallsPositionX, positionY, 0), new Color(1f, 1f, 1f, 0));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 - 1] = new VertexPositionColor(new Vector3(drawCallsPositionX, positionY + elementsHeight + 1, 0), Color.White);
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2]     = new VertexPositionColor(new Vector3(drawCallsPositionX, positionY + elementsHeight + 1, 0), Color.White);
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 1] = new VertexPositionColor(new Vector3(drawCallsPositionX + linesWidth * drawCallsNumberOfElements, positionY + elementsHeight + 1, 0), new Color(1f, 1f, 1f, 0));
            // Bad and Good Threshold Lines
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 2] = new VertexPositionColor(new Vector3(drawCallsPositionX, positionY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 3] = new VertexPositionColor(new Vector3(drawCallsPositionX + linesWidth * drawCallsNumberOfElements, positionY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 4] = new VertexPositionColor(new Vector3(drawCallsPositionX, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 5] = new VertexPositionColor(new Vector3(drawCallsPositionX + linesWidth * drawCallsNumberOfElements, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            // Text
            drawCallsText = new GameObject2D();
            drawCallsText.AddComponent<HudText>();
            drawCallsText.HudText.Text.Append("Average Draw Calls ");
            drawCallsText.Transform.LocalPosition = new Vector3(drawCallsPositionX, positionY + elementsHeight + 5, 0);
            
            #endregion

            #region Triangles Drawn

            int trianglesDrawnPositionX = drawCallsPositionX + linesWidth * drawCallsNumberOfElements + 5 + 10;

            // Black background
            screenRect = new Rectangle(trianglesDrawnPositionX - 5, positionY - 5, linesWidth * trianglesDrawnNumberOfElements + 10, elementsHeight + 25);
            trianglesDrawnBackground = new GameObject2D();
            trianglesDrawnBackground.AddComponent<LineRenderer>();
            trianglesDrawnBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            trianglesDrawnBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            trianglesDrawnBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, -0.1f), backgroundColor);
            trianglesDrawnBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            trianglesDrawnBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            trianglesDrawnBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            trianglesDrawnBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            trianglesDrawnBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);

            // Graph
            trianglesDrawnLines = new GameObject2D();
            trianglesDrawnLines.AddComponent<LineRenderer>();
            trianglesDrawnLines.LineRenderer.Vertices = new VertexPositionColor[trianglesDrawnNumberOfElements * 2 + 6];
            for (int i = 0; i < trianglesDrawnNumberOfElements - 1; i++)
            {
                trianglesDrawnLines.LineRenderer.Vertices[i * 2]     = new VertexPositionColor(new Vector3(trianglesDrawnPositionX + i * linesWidth, positionY + elementsHeight, 0), Color.Red);
                trianglesDrawnLines.LineRenderer.Vertices[i * 2 + 1] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX + (i + 1) * linesWidth, positionY + elementsHeight, 0), Color.Red);
            }
            // White lines
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 - 2] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX, positionY, 0), new Color(1f, 1f, 1f, 0));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 - 1] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX, positionY + elementsHeight + 1, 0), Color.White);
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2]     = new VertexPositionColor(new Vector3(trianglesDrawnPositionX, positionY + elementsHeight + 1, 0), Color.White);
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 1] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX + linesWidth * trianglesDrawnNumberOfElements, positionY + elementsHeight + 1, 0), new Color(1f, 1f, 1f, 0));
            // Bad and Good Threshold Lines
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 2] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX, positionY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 3] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX + linesWidth * trianglesDrawnNumberOfElements, positionY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 4] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 5] = new VertexPositionColor(new Vector3(trianglesDrawnPositionX + linesWidth * trianglesDrawnNumberOfElements, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            // Text
            trianglesDrawnText = new GameObject2D();
            trianglesDrawnText.AddComponent<HudText>();
            trianglesDrawnText.HudText.Text.Append("Average Triangles Drawn ");
            trianglesDrawnText.Transform.LocalPosition = new Vector3(trianglesDrawnPositionX, positionY + elementsHeight + 5, 0);

            #endregion

            #region Managed Memory Used

            int managedMemoryUsedPositionX = trianglesDrawnPositionX + linesWidth * trianglesDrawnNumberOfElements + 5 + 10;

            // Black background
            screenRect = new Rectangle(managedMemoryUsedPositionX - 5, positionY - 5, linesWidth * managedMemoryUsedNumberOfElements + 10, elementsHeight + 25);
            managedMemoryUsedBackground = new GameObject2D();
            managedMemoryUsedBackground.AddComponent<LineRenderer>();
            managedMemoryUsedBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            managedMemoryUsedBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            managedMemoryUsedBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, -0.1f), backgroundColor);
            managedMemoryUsedBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            managedMemoryUsedBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            managedMemoryUsedBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            managedMemoryUsedBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            managedMemoryUsedBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);

            // Graph
            managedMemoryUsedLines = new GameObject2D();
            managedMemoryUsedLines.AddComponent<LineRenderer>();
            managedMemoryUsedLines.LineRenderer.Vertices = new VertexPositionColor[managedMemoryUsedNumberOfElements * 2 + 6];
            for (int i = 0; i < managedMemoryUsedNumberOfElements - 1; i++)
            {
                managedMemoryUsedLines.LineRenderer.Vertices[i * 2] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX + i * linesWidth, positionY + elementsHeight, 0), new Color(100, 255, 100));
                managedMemoryUsedLines.LineRenderer.Vertices[i * 2 + 1] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX + (i + 1) * linesWidth, positionY + elementsHeight, 0), new Color(100, 255, 100));
            }
            // White lines
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 - 2] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX, positionY, 0), new Color(1f, 1f, 1f, 0));
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 - 1] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX, positionY + elementsHeight + 1, 0), Color.White);
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2]     = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX, positionY + elementsHeight + 1, 0), Color.White);
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 1] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX + linesWidth * managedMemoryUsedNumberOfElements, positionY + elementsHeight + 1, 0), new Color(1f, 1f, 1f, 0));
            // Bad and Good Threshold Lines
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 2] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX, positionY + elementsHeight - 20, 0), new Color(1, 0, 0, 0.1f));
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 3] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX + linesWidth * managedMemoryUsedNumberOfElements, positionY + elementsHeight - -20, 0), new Color(1, 0, 0, 0.1f));
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 4] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 5] = new VertexPositionColor(new Vector3(managedMemoryUsedPositionX + linesWidth * managedMemoryUsedNumberOfElements, positionY + elementsHeight - 40, 0), new Color(1, 1, 0, 0.1f));
            // Text
            managedMemoryUsedText = new GameObject2D();
            managedMemoryUsedText.AddComponent<HudText>();
            managedMemoryUsedText.HudText.Text.Append("Managed Memory Used ");
            managedMemoryUsedText.Transform.LocalPosition = new Vector3(managedMemoryUsedPositionX, positionY + elementsHeight + 5, 0);

            #endregion

            #region Garbage Collections

            int garbageCollectionsPositionX = managedMemoryUsedPositionX + linesWidth * managedMemoryUsedNumberOfElements + 5 + 10;

            // Black background
            screenRect = new Rectangle(garbageCollectionsPositionX - 5, positionY - 5, linesWidth * garbageCollectionsNumberOfElements + 10, elementsHeight + 25);
            garbageCollectionsBackground = new GameObject2D();
            garbageCollectionsBackground.AddComponent<LineRenderer>();
            garbageCollectionsBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            garbageCollectionsBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            garbageCollectionsBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, -0.1f), backgroundColor);
            garbageCollectionsBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            garbageCollectionsBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            garbageCollectionsBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            garbageCollectionsBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            garbageCollectionsBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);

            // Graph
            garbageCollectionsLines = new GameObject2D();
            garbageCollectionsLines.AddComponent<LineRenderer>();
            garbageCollectionsLines.LineRenderer.Vertices = new VertexPositionColor[garbageCollectionsNumberOfElements * 2 + 2];
            for (int i = 0; i < garbageCollectionsNumberOfElements - 1; i++)
            {
                garbageCollectionsLines.LineRenderer.Vertices[i * 2]     = new VertexPositionColor(new Vector3(garbageCollectionsPositionX + i * linesWidth, positionY + elementsHeight, 0), new Color(100, 255, 100));
                garbageCollectionsLines.LineRenderer.Vertices[i * 2 + 1] = new VertexPositionColor(new Vector3(garbageCollectionsPositionX + (i + 1) * linesWidth, positionY + elementsHeight, 0), new Color(100, 255, 100));
            }
            // White lines
            garbageCollectionsLines.LineRenderer.Vertices[garbageCollectionsNumberOfElements * 2 - 2] = new VertexPositionColor(new Vector3(garbageCollectionsPositionX, positionY, 0), new Color(1f, 1f, 1f, 0));
            garbageCollectionsLines.LineRenderer.Vertices[garbageCollectionsNumberOfElements * 2 - 1] = new VertexPositionColor(new Vector3(garbageCollectionsPositionX, positionY + elementsHeight + 1, 0), Color.White);
            garbageCollectionsLines.LineRenderer.Vertices[garbageCollectionsNumberOfElements * 2]     = new VertexPositionColor(new Vector3(garbageCollectionsPositionX, positionY + elementsHeight + 1, 0), Color.White);
            garbageCollectionsLines.LineRenderer.Vertices[garbageCollectionsNumberOfElements * 2 + 1] = new VertexPositionColor(new Vector3(garbageCollectionsPositionX + linesWidth * garbageCollectionsNumberOfElements, positionY + elementsHeight + 1, 0), new Color(1f, 1f, 1f, 0));
            // Text
            garbageCollectionsText = new GameObject2D();
            garbageCollectionsText.AddComponent<HudText>();
            garbageCollectionsText.HudText.Text.Append("Garbage Collections ");
            garbageCollectionsText.Transform.LocalPosition = new Vector3(garbageCollectionsPositionX, positionY + elementsHeight + 5, 0);

            #endregion

            #region User Interface Text

            // Black background
            screenRect = new Rectangle(positionX - 5, positionY + elementsHeight + 25, 520, 15);
            userInterfaceGarbageBackground = new GameObject2D();
            userInterfaceGarbageBackground.AddComponent<LineRenderer>();
            userInterfaceGarbageBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            userInterfaceGarbageBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            userInterfaceGarbageBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, -0.1f), backgroundColor);
            userInterfaceGarbageBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            userInterfaceGarbageBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            userInterfaceGarbageBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, -0.1f), backgroundColor);
            userInterfaceGarbageBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            userInterfaceGarbageBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, -0.1f), backgroundColor);
            
            userInterfaceGarbageText = new GameObject2D();
            userInterfaceGarbageText.AddComponent<HudText>();
            userInterfaceGarbageText.HudText.Text.Append("The User Interface generates a lot of garbage collections. Do not use it in game time.");
            userInterfaceGarbageText.HudText.Color = Color.Gray;
            userInterfaceGarbageText.Transform.LocalPosition = new Vector3(positionX, positionY + elementsHeight + 25, 0);

            #endregion

        } // Load

        #endregion

        #region Render

        /// <summary>
        /// Tasks executed during the first stage of the scene render.
        /// </summary>
        public override void PreRenderUpdate()
        {
            // Some graphs are updated one time for second.
            if (chronometer.ElapsedTime > 1)
            {
                chronometer.Reset();

                #region Frames Per Second
                
                // Update values and find max value
                int framesPerSecondMaxValue = 0; // To scale the graph
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
                    UpdateLines(i, 0, framesPerSecondLines.LineRenderer, ref framesPerSecondValues, framesPerSecondMaxValue,
                                framesPerSecondBadThreshold, framesPerSecondGoodThreshold, Color.Red, Color.Yellow, new Color(100, 255, 100));
                    UpdateLines(i, 1, framesPerSecondLines.LineRenderer, ref framesPerSecondValues, framesPerSecondMaxValue,
                                framesPerSecondBadThreshold, framesPerSecondGoodThreshold, Color.Red, Color.Yellow, new Color(100, 255, 100));
                }
                // Update Text
                framesPerSecondText.HudText.Text.Length = 18;
                framesPerSecondText.HudText.Text.AppendWithoutGarbage(Time.FramesPerSecond);
                // Threshold Lines
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 2].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondBadThreshold / framesPerSecondMaxValue));
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 3].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondBadThreshold / framesPerSecondMaxValue));
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 4].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondGoodThreshold / framesPerSecondMaxValue));
                framesPerSecondLines.LineRenderer.Vertices[framesPerSecondNumberOfElements * 2 + 5].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)framesPerSecondGoodThreshold / framesPerSecondMaxValue));

                #endregion

                #region Managed Memory Used

                // Update values and find max value
                int managedMemoryUsedMaxValue = 0; // To scale the graph
                for (int i = 0; i < managedMemoryUsedNumberOfElements - 1; i++)
                {
                    managedMemoryUsedValues[i] = managedMemoryUsedValues[i + 1];
                    if (managedMemoryUsedValues[i] > managedMemoryUsedMaxValue)
                        managedMemoryUsedMaxValue = managedMemoryUsedValues[i];
                }
                managedMemoryUsedValues[managedMemoryUsedNumberOfElements - 1] = (int)Statistics.ManagedMemoryUsed;
                if (managedMemoryUsedValues[managedMemoryUsedNumberOfElements - 1] > managedMemoryUsedMaxValue)
                    managedMemoryUsedMaxValue = managedMemoryUsedValues[managedMemoryUsedNumberOfElements - 1];
                // Update graph
                for (int i = 0; i < managedMemoryUsedNumberOfElements - 1; i++)
                {
                    UpdateLines(i, 0, managedMemoryUsedLines.LineRenderer, ref managedMemoryUsedValues, managedMemoryUsedMaxValue,
                                managedMemoryUsedGoodThreshold, managedMemoryUsedBadThreshold, new Color(100, 255, 100), Color.Yellow, Color.Red);
                    UpdateLines(i, 1, managedMemoryUsedLines.LineRenderer, ref managedMemoryUsedValues, managedMemoryUsedMaxValue,
                                managedMemoryUsedGoodThreshold, managedMemoryUsedBadThreshold, new Color(100, 255, 100), Color.Yellow, Color.Red);
                }
                // Update Text
                managedMemoryUsedText.HudText.Text.Length = 20;
                managedMemoryUsedText.HudText.Text.AppendWithoutGarbage((int)Statistics.ManagedMemoryUsed, true);
                managedMemoryUsedText.HudText.Text.Append(" bytes");
                // Threshold Lines
                managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 2].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)managedMemoryUsedBadThreshold / framesPerSecondMaxValue));
                managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 3].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)managedMemoryUsedBadThreshold / framesPerSecondMaxValue));
                managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 4].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)managedMemoryUsedGoodThreshold / framesPerSecondMaxValue));
                managedMemoryUsedLines.LineRenderer.Vertices[managedMemoryUsedNumberOfElements * 2 + 5].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)managedMemoryUsedGoodThreshold / framesPerSecondMaxValue));

                #endregion

                #region Garbage Collections
                
                // Update values and find max value
                int garbageCollectionsMaxValue = 0; // To scale the graph
                for (int i = 0; i < garbageCollectionsNumberOfElements - 1; i++)
                {
                    garbageCollectionsValues[i] = garbageCollectionsValues[i + 1];
                    if (garbageCollectionsValues[i] > garbageCollectionsMaxValue)
                        garbageCollectionsMaxValue = garbageCollectionsValues[i];
                }
                garbageCollectionsValues[garbageCollectionsNumberOfElements - 1] = Statistics.GarbageCollections;
                if (garbageCollectionsValues[garbageCollectionsNumberOfElements - 1] > garbageCollectionsMaxValue)
                    garbageCollectionsMaxValue = garbageCollectionsValues[garbageCollectionsNumberOfElements - 1];
                // Update graph
                for (int i = 0; i < garbageCollectionsNumberOfElements - 1; i++)
                {
                    UpdateLines(i, 0, garbageCollectionsLines.LineRenderer, ref garbageCollectionsValues, garbageCollectionsMaxValue,
                                1, 2, new Color(100, 255, 100), Color.Red, Color.Red);
                    UpdateLines(i, 1, garbageCollectionsLines.LineRenderer, ref garbageCollectionsValues, garbageCollectionsMaxValue,
                                1, 2, new Color(100, 255, 100), Color.Red, Color.Red);
                }
                // Update Text
                garbageCollectionsText.HudText.Text.Length = 20;
                garbageCollectionsText.HudText.Text.AppendWithoutGarbage(Statistics.GarbageCollections, true);

                #endregion

            }
        } // PreRenderUpdate

        /// <summary>
        /// Tasks executed during the last stage of the scene render.
        /// </summary>
        public override void PostRenderUpdate()
        {

            #region Draw Calls

            // Update values and find max value
            int drawCallsMaxValue = 0; // To scale the graph
            for (int i = 0; i < drawCallsNumberOfElements - 1; i++)
            {
                drawCallsValues[i] = drawCallsValues[i + 1];
                if (drawCallsValues[i] > drawCallsMaxValue)
                    drawCallsMaxValue = drawCallsValues[i];
            }
            drawCallsValues[drawCallsNumberOfElements - 1] = Statistics.DrawCalls;
            if (drawCallsValues[drawCallsNumberOfElements - 1] > drawCallsMaxValue)
                drawCallsMaxValue = drawCallsValues[drawCallsNumberOfElements - 1];
            // Update graph
            for (int i = 0; i < drawCallsNumberOfElements - 1; i++)
            {
                UpdateLines(i, 0, drawCallsLines.LineRenderer, ref drawCallsValues, drawCallsMaxValue,
                            drawCallsGoodThreshold, drawCallsBadThreshold, new Color(100, 255, 100), Color.Yellow, Color.Red);
                UpdateLines(i, 1, drawCallsLines.LineRenderer, ref drawCallsValues, drawCallsMaxValue,
                            drawCallsGoodThreshold, drawCallsBadThreshold, new Color(100, 255, 100), Color.Yellow, Color.Red);
            }
            // Update Text
            drawCallsText.HudText.Text.Length = 19;
            drawCallsText.HudText.Text.AppendWithoutGarbage(Statistics.AverageDrawCalls);
            // Threshold Lines
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 2].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)drawCallsBadThreshold / drawCallsMaxValue));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 3].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)drawCallsBadThreshold / drawCallsMaxValue));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 4].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)drawCallsGoodThreshold / drawCallsMaxValue));
            drawCallsLines.LineRenderer.Vertices[drawCallsNumberOfElements * 2 + 5].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)drawCallsGoodThreshold / drawCallsMaxValue));

            #endregion

            #region Triangles Drawn

            // Update values and find max value
            int trianglesDrawnMaxValue = 0; // To scale the graph
            for (int i = 0; i < trianglesDrawnNumberOfElements - 1; i++)
            {
                trianglesDrawnValues[i] = trianglesDrawnValues[i + 1];
                if (trianglesDrawnValues[i] > trianglesDrawnMaxValue)
                    trianglesDrawnMaxValue = trianglesDrawnValues[i];
            }
            trianglesDrawnValues[trianglesDrawnNumberOfElements - 1] = Statistics.TrianglesDrawn;
            if (trianglesDrawnValues[trianglesDrawnNumberOfElements - 1] > trianglesDrawnMaxValue)
                trianglesDrawnMaxValue = trianglesDrawnValues[trianglesDrawnNumberOfElements - 1];
            // Update graph
            for (int i = 0; i < trianglesDrawnNumberOfElements - 1; i++)
            {
                UpdateLines(i, 0, trianglesDrawnLines.LineRenderer, ref trianglesDrawnValues, trianglesDrawnMaxValue,
                            trianglesDrawnGoodThreshold, trianglesDrawnBadThreshold, new Color(100, 255, 100), Color.Yellow, Color.Red);
                UpdateLines(i, 1, trianglesDrawnLines.LineRenderer, ref trianglesDrawnValues, trianglesDrawnMaxValue,
                            trianglesDrawnGoodThreshold, trianglesDrawnBadThreshold, new Color(100, 255, 100), Color.Yellow, Color.Red);
            }
            // Update Text
            trianglesDrawnText.HudText.Text.Length = 24;
            trianglesDrawnText.HudText.Text.AppendWithoutGarbage(Statistics.AverageTrianglesDrawn, true);
            // Threshold Lines
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 2].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)trianglesDrawnBadThreshold  / trianglesDrawnMaxValue));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 3].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)trianglesDrawnBadThreshold  / trianglesDrawnMaxValue));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 4].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)trianglesDrawnGoodThreshold / trianglesDrawnMaxValue));
            trianglesDrawnLines.LineRenderer.Vertices[trianglesDrawnNumberOfElements * 2 + 5].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)trianglesDrawnGoodThreshold / trianglesDrawnMaxValue));

            #endregion
            
        } // PostRenderUpdate

        #endregion

        #region Update Lines

        /// <summary>
        /// Updates the lines with the new values and also calculates their colors.
        /// </summary>
        /// <param name="index">The element or line segment to update.</param>
        /// <param name="offset">A line is form by two points. This indicates if it is the first point or the last.</param>
        /// <param name="maxValue">The maximum value is used to scale the graph.</param>
        private static void UpdateLines(int index, int offset, LineRenderer linesRenderer, ref int[] values, int maxValue,
                                        int lowerThreshold, int upperThreshold,
                                        Color lowerThresholdColor, Color mediumThresholdColor, Color upperThresholdColor)
        {
            if (maxValue == 0)
                maxValue = 1; // To avoid divisions by 0.
            linesRenderer.Vertices[index * 2 + offset].Position.Y = positionY + elementsHeight - (int)(elementsHeight * ((float)values[index + offset] / maxValue));
            if (values[index + offset] < lowerThreshold)
                linesRenderer.Vertices[index * 2 + offset].Color = lowerThresholdColor;
            else if (values[index + offset] > upperThreshold)
                linesRenderer.Vertices[index * 2 + offset].Color = upperThresholdColor;
            else
            {
                if (values[index + offset] < upperThreshold - ((upperThreshold - lowerThreshold) / 2))
                {
                    // Mix red with yellow
                    linesRenderer.Vertices[index * 2 + offset].Color = 
                        Color.Lerp(lowerThresholdColor, mediumThresholdColor, (values[index + offset] - lowerThreshold) / (float)((upperThreshold - lowerThreshold) / 2));
                }
                else
                {
                    // Mix green with yellow
                    linesRenderer.Vertices[index * 2 + offset].Color = 
                        Color.Lerp(upperThresholdColor, mediumThresholdColor, (upperThreshold - values[index + offset]) / (float)((upperThreshold - lowerThreshold) / 2));
                }
            }
        } // UpdateLines

        #endregion

    } // ScriptStatisticsDrawer
} // XNAFinalEngine.Editor

