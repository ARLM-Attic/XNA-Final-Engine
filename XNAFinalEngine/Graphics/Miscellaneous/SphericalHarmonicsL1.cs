
#region License
/*

 From Xen: Graphics API for XNA 
 License: Microsoft Public License (Ms-PL)
 
 Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

*/
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Spherical Harmonics for RGB colors.
    /// A spherical harmonic approximates a spherical function using only a few values.
    /// They are great to store low frequency ambient colors and are very fast.
    /// </summary>
    public class SphericalHarmonicL1 : SphericalHarmonic
    {
        
        #region Variables

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        private Vector3 c0 = Vector3.Zero, c1 = Vector3.Zero, c2 = Vector3.Zero, c3 = Vector3.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C0 { get { return c0; } set { c0 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C1 { get { return c1; } set { c1 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C2 { get { return c2; } set { c2 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C3 { get { return c3; } set { c3 = value; } }
        
        #endregion

        #region Fill constants

        /// <summary>
        /// Fill the constants with a color.
        /// </summary>
        public override void Fill(float red, float green, float blue)
        {
            c0 = new Vector3(red, green, blue);
            c1 = new Vector3(red, green, blue);
            c2 = new Vector3(red, green, blue);
            c3 = new Vector3(red, green, blue);
        } // Fill

        #endregion
        
        #region Add Light

        /// <summary>
        /// Add light from a given direction to the SH function. See the class remarks for further details on how an SH can be used to approximate lighting.
        /// Input light inputRGB will be multiplied by weight, and weight will be added to weighting.
        /// </summary>
        /// <param name="rgb">Input light intensity in RGB (usually gamma space) format for the given direction</param>
        /// <param name="direction">direction of the incoming light</param>
        /// <param name="weight">Weighting for this light, usually 1.0f. Use this value, and weighting if averaging a large number of lighting samples (eg, when converting a cube map to an SH)</param>
        public override void AddLight(Vector3 rgb, Vector3 direction, float weight = 1.0f)
        {
            direction = Vector3.Normalize(direction);

            // Slightly different result than the Xen method. The Xen method requires less shader operations.
            #region Ravi Ramamoorthi method

            /*
            // L_{00}.  Note that Y_{00} = 0.282095
            float c = 0.282095f;
            c0 += rgb * c * weight;

            // L_{1m}. -1 <= m <= 1.  The linear terms
            c = 0.488603f;
            c1 += rgb * (c * direction.Y) * weight;   // Y_{1-1} = 0.488603 y
            c2 += rgb * (c * direction.Z) * weight;   // Y_{10}  = 0.488603 z
            c3 += rgb * (c * direction.X) * weight;   // Y_{11}  = 0.488603 x
            */

            #endregion

            #region Xen method

            float x = direction.X;
            float y = direction.Y;
            float z = direction.Z;

            float r = rgb.X * weight;
            float g = rgb.Y * weight;
            float b = rgb.Z * weight;

            // Spherical Harmonic constants
            const float f0 = 0.25f;
            const float f1 = 0.5f;

            // Axis constants for SH input
            float const1 = (f1 * (x));
            float const2 = (f1 * (y));
            float const3 = (f1 * (z));

            // Red channel
            c0.X += r * f0;
            c1.X += r * const1;
            c2.X += r * const2;
            c3.X += r * const3;
            // Green channel
            c0.Y += g * f0;
            c1.Y += g * const1;
            c2.Y += g * const2;
            c3.Y += g * const3;
            // Blue channel
            c0.Z += b * f0;
            c1.Z += b * const1;
            c2.Z += b * const2;
            c3.Z += b * const3;

            #endregion

            // Store the accumulated weighting, useful if averaging the light input is desired.
            weighting += weight;
        } // AddLight

        #endregion

        #region Sample Direction

        /// <summary>
        /// Sample the spherical harmonic in the given direction.
        /// </summary>
        public override Vector3 SampleDirection(Vector3 direction)
        {
            direction = Vector3.Normalize(direction);
            float x = direction.X;
            float y = direction.Y;
            float z = direction.Z;

            float r = c0.X +
                      c1.X * x +
                      c2.X * y +
                      c3.X * z;

            float g = c0.Y +
                      c1.Y * x +
                      c2.Y * y +
                      c3.Y * z;

            float b = c0.Z +
                      c1.Z * x +
                      c2.Z * y +
                      c3.Z * z;

            return new Vector3(r, g, b);
        } // SampleDirection

        #endregion

        #region Generate Spherical Harmonic from CubeMap

        /// <summary>
        /// Generate a spherical harmonic from the faces of a cubemap, treating each pixel as a light source and averaging the result.
        /// </summary>
        public static SphericalHarmonicL1 GenerateSphericalHarmonicFromCubeMap(TextureCube cubeMap)
        {
            SphericalHarmonicL1 sh = new SphericalHarmonicL1();

            // Extract the 6 faces of the cubemap.
            for (int face = 0; face < 6; face++)
            {
                CubeMapFace faceId = (CubeMapFace)face;

                // Get the transformation for this face,
                Matrix cubeFaceMatrix;
                switch (faceId)
                {
                    case CubeMapFace.PositiveX:
                        cubeFaceMatrix = Matrix.CreateLookAt(Vector3.Zero, new Vector3(1, 0, 0), new Vector3(0, 1, 0));
                        break;
                    case CubeMapFace.NegativeX:
                        cubeFaceMatrix = Matrix.CreateLookAt(Vector3.Zero, new Vector3(-1, 0, 0), new Vector3(0, 1, 0));
                        break;
                    case CubeMapFace.PositiveY:
                        cubeFaceMatrix = Matrix.CreateLookAt(Vector3.Zero, new Vector3(0, 1, 0), new Vector3(0, 0, 1));
                        break;
                    case CubeMapFace.NegativeY:
                        cubeFaceMatrix = Matrix.CreateLookAt(Vector3.Zero, new Vector3(0, -1, 0), new Vector3(0, 0, -1));
                        break;
                    case CubeMapFace.PositiveZ:
                        cubeFaceMatrix = Matrix.CreateLookAt(Vector3.Zero, new Vector3(0, 0, -1), new Vector3(0, 1, 0));
                        break;
                    case CubeMapFace.NegativeZ:
                        cubeFaceMatrix = Matrix.CreateLookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Color[] colorArray = new Color[cubeMap.Size * cubeMap.Size];
                cubeMap.Resource.GetData<Color>(faceId, colorArray);

                // Extract the spherical harmonic for this face and accumulate it.
                sh += ExtractSphericalHarmonicForCubeFace(cubeFaceMatrix, colorArray, cubeMap.Size, cubeMap.IsRgbm, cubeMap.RgbmMaxRange);
            }

            //average out over the sphere
            return sh.GetWeightedAverageLightInputFromSphere();
        } // GenerateSphericalHarmonicFromCubeMap

        /// <summary>
        /// Use this function after accumulating multiple lights with the AddLight method.
        /// This method averages all the calls to AddLight() based on the accumulated weighting, assuming they were light input over a sphere.
        /// For example, if generating a spherical harmonic from a cube map, treat each pixel as a light source by calling AddLight(),
        /// then call this method to get the average for entire the sphere.
        /// </summary>
        private SphericalHarmonicL1 GetWeightedAverageLightInputFromSphere()
        {
            // Average out the entire spherical harmonic.
            // The 4 is because the SH lighting input is being sampled over a cosine weighted hemisphere.
            // The hemisphere halves the divider, the cosine weighting halves it again.
            if (weighting > 0)
                return this * (4.0f / weighting);
            return this;
        } // GetWeightedAverageLightInputFromSphere

        private static SphericalHarmonicL1 ExtractSphericalHarmonicForCubeFace(Matrix faceTransform, Color[] colorDataRgb, int faceSize, bool isRgbm, float rgbmMaxRange)
        {
            SphericalHarmonicL1 sh = new SphericalHarmonicL1();

            // For each pixel in the face, generate it's SH contribution.
            // Treat each pixel in the cube as a light source, which gets added to the SH.
            // This is used to generate an indirect lighting SH for the scene.
            
            float directionStep = 2.0f / (faceSize - 1.0f);
            int pixelIndex = 0;

            float dirY = 1.0f;
            for (int y = 0; y < faceSize; y++)
            {
                SphericalHarmonicL1 lineSh = new SphericalHarmonicL1();
                float dirX = -1.0f;

                for (int x = 0; x < faceSize; x++)
                {
                    //the direction to the pixel in the cube
                    Vector3 direction = new Vector3(dirX, dirY, 1);
                    Vector3.TransformNormal(ref direction, ref faceTransform, out direction);

                    //length of the direction vector
                    float length = direction.Length();
                    //approximate area of the pixel (pixels close to the cube edges appear smaller when projected)
                    float weight = 1.0f / length;

                    //normalise:
                    direction.X *= weight;
                    direction.Y *= weight;
                    direction.Z *= weight;

                    Vector3 rgbFloat;
                    if (isRgbm)
                    {
                        Color rgbm = colorDataRgb[pixelIndex++];
                        rgbFloat = RgbmHelper.RgbmGammaToFloatLinear(rgbm, rgbmMaxRange);
                    }
                    else
                    {
                        Color rgb = colorDataRgb[pixelIndex++];
                        rgbFloat = new Vector3(GammaLinearSpaceHelper.GammaToLinear(rgb).X, GammaLinearSpaceHelper.GammaToLinear(rgb).Y, GammaLinearSpaceHelper.GammaToLinear(rgb).Z);
                    }
                    
                    //Add it to the SH
                    lineSh.AddLight(rgbFloat, direction, weight);

                    dirX += directionStep;
                }

                //average the SH
                if (lineSh.weighting > 0)
                    lineSh *= 1 / lineSh.weighting;

                // Add the line to the full SH
                // (SH is generated line by line to ease problems with floating point accuracy loss)
                sh += lineSh;

                dirY -= directionStep;
            }

            if (sh.weighting > 0)
                sh *= 1 / sh.weighting;

            return sh;
        } // ExtractSphericalHarmonicForCubeFace

        #endregion

        #region Add

        /// <summary>
        /// Add two spherical harmonics together.
        /// </summary>
        public static SphericalHarmonicL1 Add(SphericalHarmonicL1 x, SphericalHarmonicL1 y)
        {
            return new SphericalHarmonicL1
            {
                weighting = x.weighting + y.weighting,
                c0 = { X = x.c0.X + y.c0.X, Y = x.c0.Y + y.c0.Y, Z = x.c0.Z + y.c0.Z },
                c1 = { X = x.c1.X + y.c1.X, Y = x.c1.Y + y.c1.Y, Z = x.c1.Z + y.c1.Z },
                c2 = { X = x.c2.X + y.c2.X, Y = x.c2.Y + y.c2.Y, Z = x.c2.Z + y.c2.Z },
                c3 = { X = x.c3.X + y.c3.X, Y = x.c3.Y + y.c3.Y, Z = x.c3.Z + y.c3.Z }
            };
        } // Add

        /// <summary>
        /// Add two spherical harmonics together
        /// </summary>
        public static SphericalHarmonicL1 operator +(SphericalHarmonicL1 x, SphericalHarmonicL1 y)
        {
            return new SphericalHarmonicL1
            {
                weighting = x.weighting + y.weighting,
                c0 = { X = x.c0.X + y.c0.X, Y = x.c0.Y + y.c0.Y, Z = x.c0.Z + y.c0.Z },
                c1 = { X = x.c1.X + y.c1.X, Y = x.c1.Y + y.c1.Y, Z = x.c1.Z + y.c1.Z },
                c2 = { X = x.c2.X + y.c2.X, Y = x.c2.Y + y.c2.Y, Z = x.c2.Z + y.c2.Z },
                c3 = { X = x.c3.X + y.c3.X, Y = x.c3.Y + y.c3.Y, Z = x.c3.Z + y.c3.Z }
            };
        } // Add

        #endregion

        #region Multiply by a scalar

        /// <summary>
        /// Multiply a spherical harmonic by a constant scale factor
        /// </summary>
        public static SphericalHarmonicL1 Multiply(SphericalHarmonicL1 x, float scale)
        {
            return new SphericalHarmonicL1
            {
                weighting = x.weighting * scale,
                c0 = { X = x.c0.X * scale, Y = x.c0.Y * scale, Z = x.c0.Z * scale },
                c1 = { X = x.c1.X * scale, Y = x.c1.Y * scale, Z = x.c1.Z * scale },
                c2 = { X = x.c2.X * scale, Y = x.c2.Y * scale, Z = x.c2.Z * scale },
                c3 = { X = x.c3.X * scale, Y = x.c3.Y * scale, Z = x.c3.Z * scale }
            };
        } // Multiply

        /// <summary>
        /// Multiply a spherical harmonic by a constant scale factor.
        /// </summary>
        public static SphericalHarmonicL1 operator *(SphericalHarmonicL1 x, float scale)
        {
            return new SphericalHarmonicL1
            {
                weighting = x.weighting * scale,
                c0 = { X = x.c0.X * scale, Y = x.c0.Y * scale, Z = x.c0.Z * scale },
                c1 = { X = x.c1.X * scale, Y = x.c1.Y * scale, Z = x.c1.Z * scale },
                c2 = { X = x.c2.X * scale, Y = x.c2.Y * scale, Z = x.c2.Z * scale },
                c3 = { X = x.c3.X * scale, Y = x.c3.Y * scale, Z = x.c3.Z * scale }
            };
        } // Multiply

        #endregion

        #region Lerp

        /// <summary>
        /// Linear interpolate (Lerp) between two spherical harmonics based on a interpolation factor.
        /// </summary>
        /// <param name="factor">Determines the interpolation point. When factor is 1.0, the output will be x, when factor is 0.0, the output will be y</param>
        public static SphericalHarmonicL1 Lerp(SphericalHarmonicL1 x, SphericalHarmonicL1 y, float factor)
        {
            float xs = factor;
            float ys = 1.0f - factor;
            return new SphericalHarmonicL1
            {
                weighting = x.weighting * xs + y.weighting * ys,
                c0 = { X = xs * x.c0.X + ys * y.c0.X, Y = xs * x.c0.Y + ys * y.c0.Y, Z = xs * x.c0.Z + ys * y.c0.Z },
                c1 = { X = xs * x.c1.X + ys * y.c1.X, Y = xs * x.c1.Y + ys * y.c1.Y, Z = xs * x.c1.Z + ys * y.c1.Z },
                c2 = { X = xs * x.c2.X + ys * y.c2.X, Y = xs * x.c2.Y + ys * y.c2.Y, Z = xs * x.c2.Z + ys * y.c2.Z },
                c3 = { X = xs * x.c3.X + ys * y.c3.X, Y = xs * x.c3.Y + ys * y.c3.Y, Z = xs * x.c3.Z + ys * y.c3.Z },
            };
        } // Lerp

        #endregion

        #region Get Coeficients

        /// <summary>
        /// Spherical Harmonic RGB coefficients.
        /// </summary>
        public void GetCoeficients(Vector3[] coeficients)
        {
            if (coeficients.Length != 4)
                throw new ArgumentOutOfRangeException("coeficients");
            coeficients[0] = c0;
            coeficients[1] = c1;
            coeficients[2] = c2;
            coeficients[3] = c3;
        } // GetCoeficients

        #endregion

    } // SphericalHarmonicL1
} // XNAFinalEngine.Graphics
