
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
using XNAFinalEngine.Graphics;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Spherical Harmonics for RGB colors.
    /// A spherical harmonic approximates a spherical function using only a few values.
    /// They are great to store low frequency ambient colors and are very fast.
    /// </summary>
    public class SphericalHarmonicL2 : SphericalHarmonic
    {

        #region Variables

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        private Vector3 c0 = Vector3.Zero,
                        c1 = Vector3.Zero,
                        c2 = Vector3.Zero,
                        c3 = Vector3.Zero,
                        c4 = Vector3.Zero,
                        c5 = Vector3.Zero,
                        c6 = Vector3.Zero,
                        c7 = Vector3.Zero,
                        c8 = Vector3.Zero;

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

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C4 { get { return c4; } set { c4 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C5 { get { return c5; } set { c5 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C6 { get { return c6; } set { c6 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C7 { get { return c7; } set { c7 = value; } }

        /// <summary>
        /// Spherical Harmonic RGB coefficient.
        /// </summary>
        public Vector3 C8 { get { return c8; } set { c8 = value; } }
        
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
            c4 = new Vector3(red, green, blue);
            c5 = new Vector3(red, green, blue);
            c6 = new Vector3(red, green, blue);
            c7 = new Vector3(red, green, blue);
            c8 = new Vector3(red, green, blue);
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

            // The Quadratic terms, L_{2m} -2 <= m <= 2

            // First, L_{2-2}, L_{2-1}, L_{21} corresponding to xy,yz,xz
            c = 1.092548f;
            c4 += rgb * (c * direction.X * direction.Y) * weight; // Y_{2-2} = 1.092548 xy
            c5 += rgb * (c * direction.Y * direction.Z) * weight; // Y_{2-1} = 1.092548 yz
            c7 += rgb * (c * direction.X * direction.Z) * weight; // Y_{21}  = 1.092548 xz

            // L_{20}.  Note that Y_{20} = 0.315392 (3z^2 - 1)
            c = 0.315392f;
            c6 += rgb * (c * (3 * direction.Z * direction.Z - 1)) * weight;

            // L_{22}.  Note that Y_{22} = 0.546274 (x^2 - y^2)
            c = 0.546274f;
            c8 += rgb * (c * (direction.X * direction.X - direction.Y * direction.Y)) * weight;
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
            const float f2 = 0.937500143128f;
            const float f3 = 0.234375035782f;

            // Axis constants for SH input
            float const1 = (f1 * (x));
            float const2 = (f1 * (y));
            float const3 = (f1 * (z));
            float const4 = (f2 * (x * y));
            float const5 = (f2 * (y * z));
            float const6 = (f2 * (x * z));
            float const7 = (f3 * (z * z - (1.0f / 3.0f)));
            float const8 = (f3 * (x * x - y * y));

            // Red channel
            c0.X += r * f0;
            c1.X += r * const1;
            c2.X += r * const2;
            c3.X += r * const3;
            c4.X += r * const4;
            c5.X += r * const5;
            c6.X += r * const6;
            c7.X += r * const7;
            c8.X += r * const8;
            // Green channel
            c0.Y += g * f0;
            c1.Y += g * const1;
            c2.Y += g * const2;
            c3.Y += g * const3;
            c4.Y += g * const4;
            c5.Y += g * const5;
            c6.Y += g * const6;
            c7.Y += g * const7;
            c8.Y += g * const8;
            // Blue channel
            c0.Z += b * f0;
            c1.Z += b * const1;
            c2.Z += b * const2;
            c3.Z += b * const3;
            c4.Z += b * const4;
            c5.Z += b * const5;
            c6.Z += b * const6;
            c7.Z += b * const7;
            c8.Z += b * const8;

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

            float xy = x * y;
            float yz = y * z;
            float xz = x * z;

            float zz3 = (z * z) - (1.0f / 3.0f);
            float xxyy = (x * x) - (y * y);
            
            float r = c0.X +
                        c1.X * x +
                        c2.X * y +
                        c3.X * z +
                        c4.X * xy +
                        c5.X * yz +
                        c6.X * xz +
                        c7.X * zz3 +
                        c8.X * xxyy;

            float g = c0.Y +
                        c1.Y * x +
                        c2.Y * y +
                        c3.Y * z +
                        c4.Y * xy +
                        c5.Y * yz +
                        c6.Y * xz +
                        c7.Y * zz3 +
                        c8.Y * xxyy;

            float b = c0.Z +
                        c1.Z * x +
                        c2.Z * y +
                        c3.Z * z +
                        c4.Z * xy +
                        c5.Z * yz +
                        c6.Z * xz +
                        c7.Z * zz3 +
                        c8.Z * xxyy;

            return new Vector3(r, g, b);
        } // SampleDirection

        #endregion

        #region Add

        /// <summary>
        /// Add two spherical harmonics together.
        /// </summary>
        public static SphericalHarmonicL2 Add(SphericalHarmonicL2 x, SphericalHarmonicL2 y)
        {
            return new SphericalHarmonicL2
            {
                weighting = x.weighting + y.weighting,
                c0 = { X = x.c0.X + y.c0.X, Y = x.c0.Y + y.c0.Y, Z = x.c0.Z + y.c0.Z },
                c1 = { X = x.c1.X + y.c1.X, Y = x.c1.Y + y.c1.Y, Z = x.c1.Z + y.c1.Z },
                c2 = { X = x.c2.X + y.c2.X, Y = x.c2.Y + y.c2.Y, Z = x.c2.Z + y.c2.Z },
                c3 = { X = x.c3.X + y.c3.X, Y = x.c3.Y + y.c3.Y, Z = x.c3.Z + y.c3.Z },
                c4 = { X = x.c4.X + y.c4.X, Y = x.c4.Y + y.c4.Y, Z = x.c4.Z + y.c4.Z },
                c5 = { X = x.c5.X + y.c5.X, Y = x.c5.Y + y.c5.Y, Z = x.c5.Z + y.c5.Z },
                c6 = { X = x.c6.X + y.c6.X, Y = x.c6.Y + y.c6.Y, Z = x.c6.Z + y.c6.Z },
                c7 = { X = x.c7.X + y.c7.X, Y = x.c7.Y + y.c7.Y, Z = x.c7.Z + y.c7.Z },
                c8 = { X = x.c8.X + y.c8.X, Y = x.c8.Y + y.c8.Y, Z = x.c8.Z + y.c8.Z }
            };
        } // Add

        /// <summary>
        /// Add two spherical harmonics together
        /// </summary>
        public static SphericalHarmonicL2 operator +(SphericalHarmonicL2 x, SphericalHarmonicL2 y)
        {
            return new SphericalHarmonicL2
            {
                weighting = x.weighting + y.weighting,
                c0 = { X = x.c0.X + y.c0.X, Y = x.c0.Y + y.c0.Y, Z = x.c0.Z + y.c0.Z },
                c1 = { X = x.c1.X + y.c1.X, Y = x.c1.Y + y.c1.Y, Z = x.c1.Z + y.c1.Z },
                c2 = { X = x.c2.X + y.c2.X, Y = x.c2.Y + y.c2.Y, Z = x.c2.Z + y.c2.Z },
                c3 = { X = x.c3.X + y.c3.X, Y = x.c3.Y + y.c3.Y, Z = x.c3.Z + y.c3.Z },
                c4 = { X = x.c4.X + y.c4.X, Y = x.c4.Y + y.c4.Y, Z = x.c4.Z + y.c4.Z },
                c5 = { X = x.c5.X + y.c5.X, Y = x.c5.Y + y.c5.Y, Z = x.c5.Z + y.c5.Z },
                c6 = { X = x.c6.X + y.c6.X, Y = x.c6.Y + y.c6.Y, Z = x.c6.Z + y.c6.Z },
                c7 = { X = x.c7.X + y.c7.X, Y = x.c7.Y + y.c7.Y, Z = x.c7.Z + y.c7.Z },
                c8 = { X = x.c8.X + y.c8.X, Y = x.c8.Y + y.c8.Y, Z = x.c8.Z + y.c8.Z }
            };
        } // Add

        #endregion

        #region Multiply by a scalar

        /// <summary>
        /// Multiply a spherical harmonic by a constant scale factor
        /// </summary>
        public static SphericalHarmonicL2 Multiply(SphericalHarmonicL2 x, float scale)
        {
            return new SphericalHarmonicL2
            {
                weighting = x.weighting * scale,
                c0 = { X = x.c0.X * scale, Y = x.c0.Y * scale, Z = x.c0.Z * scale },
                c1 = { X = x.c1.X * scale, Y = x.c1.Y * scale, Z = x.c1.Z * scale },
                c2 = { X = x.c2.X * scale, Y = x.c2.Y * scale, Z = x.c2.Z * scale },
                c3 = { X = x.c3.X * scale, Y = x.c3.Y * scale, Z = x.c3.Z * scale },
                c4 = { X = x.c4.X * scale, Y = x.c4.Y * scale, Z = x.c4.Z * scale },
                c5 = { X = x.c5.X * scale, Y = x.c5.Y * scale, Z = x.c5.Z * scale },
                c6 = { X = x.c6.X * scale, Y = x.c6.Y * scale, Z = x.c6.Z * scale },
                c7 = { X = x.c7.X * scale, Y = x.c7.Y * scale, Z = x.c7.Z * scale },
                c8 = { X = x.c8.X * scale, Y = x.c8.Y * scale, Z = x.c8.Z * scale }
            };
        } // Multiply

        /// <summary>
        /// Multiply a spherical harmonic by a constant scale factor.
        /// </summary>
        public static SphericalHarmonicL2 operator *(SphericalHarmonicL2 x, float scale)
        {
            return new SphericalHarmonicL2
            {
                weighting = x.weighting * scale,
                c0 = { X = x.c0.X * scale, Y = x.c0.Y * scale, Z = x.c0.Z * scale },
                c1 = { X = x.c1.X * scale, Y = x.c1.Y * scale, Z = x.c1.Z * scale },
                c2 = { X = x.c2.X * scale, Y = x.c2.Y * scale, Z = x.c2.Z * scale },
                c3 = { X = x.c3.X * scale, Y = x.c3.Y * scale, Z = x.c3.Z * scale },
                c4 = { X = x.c4.X * scale, Y = x.c4.Y * scale, Z = x.c4.Z * scale },
                c5 = { X = x.c5.X * scale, Y = x.c5.Y * scale, Z = x.c5.Z * scale },
                c6 = { X = x.c6.X * scale, Y = x.c6.Y * scale, Z = x.c6.Z * scale },
                c7 = { X = x.c7.X * scale, Y = x.c7.Y * scale, Z = x.c7.Z * scale },
                c8 = { X = x.c8.X * scale, Y = x.c8.Y * scale, Z = x.c8.Z * scale }
            };
        } // Multiply

        #endregion

        #region Lerp

        /// <summary>
        /// Linear interpolate (Lerp) between two spherical harmonics based on a interpolation factor.
        /// </summary>
        /// <param name="factor">Determines the interpolation point. When factor is 1.0, the output will be x, when factor is 0.0, the output will be y</param>
        public static SphericalHarmonicL2 Lerp(ref SphericalHarmonicL2 x, ref SphericalHarmonicL2 y, float factor)
        {
            float xs = factor;
            float ys = 1.0f - factor;
            return new SphericalHarmonicL2
            {
                weighting = x.weighting * xs + y.weighting * ys,
                c0 = { X = xs * x.c0.X + ys * y.c0.X, Y = xs * x.c0.Y + ys * y.c0.Y, Z = xs * x.c0.Z + ys * y.c0.Z },
                c1 = { X = xs * x.c1.X + ys * y.c1.X, Y = xs * x.c1.Y + ys * y.c1.Y, Z = xs * x.c1.Z + ys * y.c1.Z },
                c2 = { X = xs * x.c2.X + ys * y.c2.X, Y = xs * x.c2.Y + ys * y.c2.Y, Z = xs * x.c2.Z + ys * y.c2.Z },
                c3 = { X = xs * x.c3.X + ys * y.c3.X, Y = xs * x.c3.Y + ys * y.c3.Y, Z = xs * x.c3.Z + ys * y.c3.Z },
                c4 = { X = xs * x.c4.X + ys * y.c4.X, Y = xs * x.c4.Y + ys * y.c4.Y, Z = xs * x.c4.Z + ys * y.c4.Z },
                c5 = { X = xs * x.c5.X + ys * y.c5.X, Y = xs * x.c5.Y + ys * y.c5.Y, Z = xs * x.c5.Z + ys * y.c5.Z },
                c6 = { X = xs * x.c6.X + ys * y.c6.X, Y = xs * x.c6.Y + ys * y.c6.Y, Z = xs * x.c6.Z + ys * y.c6.Z },
                c7 = { X = xs * x.c7.X + ys * y.c7.X, Y = xs * x.c7.Y + ys * y.c7.Y, Z = xs * x.c7.Z + ys * y.c7.Z },
                c8 = { X = xs * x.c8.X + ys * y.c8.X, Y = xs * x.c8.Y + ys * y.c8.Y, Z = xs * x.c8.Z + ys * y.c8.Z }
            };
        } // Lerp

        #endregion

        #region Generate Spherical Harmonic from CubeMap

        /// <summary>
        /// Generate a spherical harmonic from the faces of a cubemap, treating each pixel as a light source and averaging the result.
        /// </summary>
        public static SphericalHarmonicL2 GenerateSphericalHarmonicFromCubeMap(TextureCube cubeMap)
        {
            SphericalHarmonicL2 sh = new SphericalHarmonicL2();

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
                cubeMap.Resource.GetData(faceId, colorArray);

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
        private SphericalHarmonicL2 GetWeightedAverageLightInputFromSphere()
        {
            // Average out the entire spherical harmonic.
            // The 4 is because the SH lighting input is being sampled over a cosine weighted hemisphere.
            // The hemisphere halves the divider, the cosine weighting halves it again.
            if (weighting > 0)
                return this * (4.0f / weighting);
            return this;
        } // GetWeightedAverageLightInputFromSphere

        private static SphericalHarmonicL2 ExtractSphericalHarmonicForCubeFace(Matrix faceTransform, Color[] colorDataRgb, int faceSize, bool isRgbm, float rgbmMaxRange)
        {
            SphericalHarmonicL2 sh = new SphericalHarmonicL2();

            // For each pixel in the face, generate it's SH contribution.
            // Treat each pixel in the cube as a light source, which gets added to the SH.
            // This is used to generate an indirect lighting SH for the scene.

            float directionStep = 2.0f / (faceSize - 1.0f);
            int pixelIndex = 0;

            float dirY = 1.0f;
            for (int y = 0; y < faceSize; y++)
            {
                SphericalHarmonicL2 lineSh = new SphericalHarmonicL2();
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

        #region Get Coeficients

        /// <summary>
        /// Spherical Harmonic RGB coefficients.
        /// </summary>
        public void GetCoeficients(Vector3[] coeficients)
        {
            if (coeficients.Length != 9)
                throw new ArgumentOutOfRangeException("coeficients");
            coeficients[0] = c0;
            coeficients[1] = c1;
            coeficients[2] = c2;
            coeficients[3] = c3;
            coeficients[4] = c4;
            coeficients[5] = c5;
            coeficients[6] = c6;
            coeficients[7] = c7;
            coeficients[8] = c8;
        } // GetCoeficients

        #endregion

    } // SphericalHarmonicL2
} // XNAFinalEngine.Assets
