﻿
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Helpers
{

    /// <summary>
    /// Curve 3D. Assumes that the firts point it's allways 0.
    /// 
    /// To create a curve:
    /// 
    ///  // Create instance
    ///  * curve = new Curve3D();
    ///  
    ///  // Add points
    ///  * curve.AddPoint(..., 0);
    ///  * curve.AddPoint(...);
    ///  ...
    ///  
    ///  // Finish the curve calculating its tangents.
    ///  * curve.SetTangents();
    /// </summary>
    public class Curve3D
    {

        #region Variables

        private readonly Curve curveX = new Curve();
        private readonly Curve curveY = new Curve();
        private readonly Curve curveZ = new Curve();

        #endregion

        #region Properties

        /// <summary>
        /// The PreLoop and PostLoop types determine how the curve will interpret positions before the first key or after the last key.
        /// </summary>
        public CurveLoopType PostLoop
        {
            get { return curveX.PostLoop; }
            set
            {
                curveX.PostLoop = value;
                curveY.PostLoop = value;
                curveZ.PostLoop = value;
            }
        } // PostLoop

        /// <summary>
        /// Point count.
        /// </summary>
        public int PointCount { get { return curveX.Keys.Count; } }

        /// <summary>
        /// The PreLoop and PostLoop types determine how the curve will interpret positions before the first key or after the last key.
        /// </summary>
        public CurveLoopType PreLoop
        {
            get { return curveX.PostLoop; }
            set
            {
                curveX.PreLoop = value;
                curveY.PreLoop = value;
                curveZ.PreLoop = value;
            }
        } // PreLoop

        /// <summary>
        /// Stores the last point's time.
        /// </summary>
        public float CurveTotalTime { get; private set; }

        /// <summary>
        /// is the curve close?
        /// </summary>
        public bool IsClose { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Curve 3D. Assumes that the firts point it's allways 0.
        /// </summary>
        public Curve3D()
        {
            IsClose = false;
            CurveTotalTime = 0;
        }

        #endregion

        #region Add Point

        /// <summary>
        /// Add a point to the curve. Assumes that the firts point it's allways 0.
        /// The worldMatrix modify the point's position to the world matrix coordinate system.
        /// </summary>
        public void AddPoint(Vector3 point, float time, Matrix worldMatrix)
        {
            point = Vector3.Transform(point, worldMatrix);
            curveX.Keys.Add(new CurveKey(time, point.X));
            curveY.Keys.Add(new CurveKey(time, point.Y));
            curveZ.Keys.Add(new CurveKey(time, point.Z));
            CurveTotalTime = time;
        } // AddPoint

        /// <summary>
        /// Add a point to the curve. Assumes that the firts point it's allways 0.
        /// </summary>
        public void AddPoint(Vector3 point, float time)
        {
            AddPoint(point, time, Matrix.Identity);
        }

        #endregion

        #region Close

        /// <summary>
        /// Close the curve
        /// </summary>
        public void Close()
        {
            if (!IsClose)
            {
                float newTime = (CurveTotalTime / (PointCount - 1)) * PointCount;
                AddPoint(GetPoint(0), newTime);
                CurveTotalTime = newTime;
                IsClose = true;
            }
        } // Close

        #endregion

        #region Get point

        /// <summary>
        /// Get point
        /// </summary>
        public Vector3 GetPoint(float time)
        {
            return new Vector3(curveX.Evaluate(time), curveY.Evaluate(time), curveZ.Evaluate(time));
        } // GetPoint
        
        #endregion

        #region Set Tangents

        /// <summary>
        /// Build tangents. The tangents of the CurveKeys control the shape of the Curve.
        /// Setting the tangents of the CurveKeys to the slope between the previous and next CurveKey will give a curve that moves smoothly through each point on the curve.
        /// </summary>
        public void BuildTangents()
        {
            CurveKey prev;
            CurveKey current;
            CurveKey next;
            int prevIndex;
            int nextIndex;
            bool border; // If it's the border then the tangent calculations change.

            for (int i = 0; i < PointCount; i++)
            {
                border = false;
                prevIndex = i - 1;
                if (prevIndex < 0) // If it's the first curve key
                {
                    if (IsClose)
                    {
                        prevIndex = PointCount - 2;
                        border = true;
                    }
                    else
                        prevIndex = i;
                }

                nextIndex = i + 1;
                if (nextIndex == PointCount) // If it's the last curve key
                {
                    if (IsClose)
                    {
                        nextIndex = 1;
                        border = true;
                    }
                    else
                        nextIndex = i;
                }
                // Build the x tangent
                prev = curveX.Keys[prevIndex];
                next = curveX.Keys[nextIndex];
                current = curveX.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next, border);
                curveX.Keys[i] = current;
                // Build the y tangent
                prev = curveY.Keys[prevIndex];
                next = curveY.Keys[nextIndex];
                current = curveY.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next, border);
                curveY.Keys[i] = current;
                // Build the z tangent
                prev = curveZ.Keys[prevIndex];
                next = curveZ.Keys[nextIndex];
                current = curveZ.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next, border);
                curveZ.Keys[i] = current;
            } // for
        } // BuildTangents

        /// <summary>
        /// Set curve key tangent
        /// </summary>
        private static void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey cur, ref CurveKey next, bool border)
        {
            float dt = next.Position - prev.Position;
            float dv;

            if (border)
                dv = prev.Value - next.Value;
            else
                dv = next.Value - prev.Value;
                
            if (Math.Abs(dv) < float.Epsilon)
            {
                cur.TangentIn = 0;
                cur.TangentOut = 0;
            }
            else
            {
                // The in and out tangents should be equal to the  slope between the adjacent keys.
                cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
                cur.TangentOut = dv * (next.Position - cur.Position) / dt;
            }
        } // SetCurveKeyTangent

        #endregion

        #region Circle (static)

        /// <summary>
        /// Circle
        /// The worldMatrix modify the point's position to the world matrix coordinate system.
        /// </summary>
        public static Curve3D Circle(Matrix worldMatrix, float radius = 1, int numberOfPoints = 50)
        {
            Curve3D circle = new Curve3D();
            for (float i = 0; i < 3.1416f * 2; i = i + (3.1416f / numberOfPoints))
            {
                float x = (float)Math.Sin(i) * radius;
                float y = (float)Math.Cos(i) * radius;
                circle.AddPoint(Vector3.Transform(new Vector3(x, y, 0), worldMatrix), i);
            }
            circle.Close();
            circle.BuildTangents();
            return circle;
        } // Circle

        /// <summary>
        /// Circle
        /// </summary>
        public static Curve3D Circle(float radius = 1, int numberOfPoints = 50)
        {
            return Circle(Matrix.Identity, radius, 50);
        } // Circle

        #endregion

    } // Curve3D
} // XNAFinalEngine.Helpers