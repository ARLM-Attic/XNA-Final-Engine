
#region License
// Created by Michael Brown (the creator of Real1ty Engine)
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Win32;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// Change the XNA profile capabilities.
    /// You can expand reach with some features from Hi-Def.
    /// Or you can reduce Hi-Def to work more like Reach.
    /// http://forums.create.msdn.com/forums/p/55788/339828.aspx
    /// </summary>
    internal static class GraphicsProfileExtender
    {

        #region Reduce Hi-Def Mode

        /// <summary>
        /// Reduce Hi Def feature set.
        /// </summary>
        public static void ReduceHiDefMode()
        {
            #if WINDOWS
                // Read informtion
                RegistryKey registryEntry = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("XNA").OpenSubKey("Game Studio").OpenSubKey("v4.0");
                Assembly xnaAssemly = Assembly.LoadFrom(((string)registryEntry.GetValue("InstallPath")) + "/References/Windows/x86/Microsoft.Xna.Framework.Graphics.dll");
                Type profileCapabilities = xnaAssemly.GetType("Microsoft.Xna.Framework.Graphics.ProfileCapabilities");
                MethodInfo getIntanceInfo = profileCapabilities.GetMethod("GetInstance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                Object objProfileReach = getIntanceInfo.Invoke(profileCapabilities, new object[] { GraphicsProfile.Reach });
                Object objProfileHidef = getIntanceInfo.Invoke(profileCapabilities, new object[] { GraphicsProfile.HiDef });

                // Copy all reach values to hi-def profile.
                foreach (FieldInfo methinfo in objProfileReach.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!methinfo.FieldType.IsClass)
                        objProfileHidef.GetType().GetField(methinfo.Name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(objProfileHidef, methinfo.GetValue(objProfileReach));
                }
              
                // Restore Profile name
                FieldInfo fieldObj = objProfileHidef.GetType().GetField("Profile", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldObj.SetValue(objProfileHidef, GraphicsProfile.HiDef);
                // Restore some other values
                ChangeValue(objProfileHidef, "SeparateAlphaBlend", true);
                ChangeValue(objProfileHidef, "MinMaxSrcDestBlend", true);
                ChangeValue(objProfileHidef, "MaxPrimitiveCount", 1048575);
                ChangeValue(objProfileHidef, "MaxIndexBufferSize", 67108863);
                ChangeValue(objProfileHidef, "MaxVertexBufferSize", 67108863);
                ChangeValue(objProfileHidef, "PixelShaderVersion", (uint)768);
                ChangeValue(objProfileHidef, "VertexShaderVersion", (uint)768);
                ChangeValue(objProfileHidef, "MaxCubeSize", 1024);
                ChangeValue(objProfileHidef, "OcclusionQuery", true);
                ChangeValue(objProfileHidef, "MaxRenderTargets", 4);
                ChangeValue(objProfileHidef, "IndexElementSize32", true);
                ChangeValue(objProfileHidef, "MaxVertexSamplers", 4);

                #region Non Pow 2 Conditions
                /*
                ChangeValue(objProfileHidef, "NonPow2Unconditional", true);
                ChangeValue(objProfileHidef, "NonPow2Cube", true);
                ChangeValue(objProfileHidef, "NonPow2Volume", true);
                */
                #endregion

                #region Validate Formats

                // FixValidTextureFormats(objProfileHidef);
                
                FieldInfo fieldValidate = objProfileReach.GetType().GetField("ValidCubeFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                List<SurfaceFormat> validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileHidef);

                fieldValidate = objProfileHidef.GetType().GetField("InvalidBlendFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileHidef);

                fieldValidate = objProfileHidef.GetType().GetField("InvalidFilterFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileHidef);

                fieldValidate = objProfileHidef.GetType().GetField("ValidDepthFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                List<DepthFormat> validateDepthFormats = (List<DepthFormat>)fieldValidate.GetValue(objProfileHidef);
            
                fieldValidate = objProfileHidef.GetType().GetField("ValidVertexFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                List<VertexElementFormat> validateVertexFormats = (List<VertexElementFormat>)fieldValidate.GetValue(objProfileHidef);
            
                fieldValidate = objProfileHidef.GetType().GetField("ValidVertexTextureFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileHidef);
                validateFormats.Clear();
                validateFormats.AddRange((List<SurfaceFormat>)fieldValidate.GetValue(objProfileReach));

                fieldValidate = objProfileHidef.GetType().GetField("ValidVolumeFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileHidef);
                validateFormats.Clear();
                validateFormats.AddRange((List<SurfaceFormat>)fieldValidate.GetValue(objProfileReach));

                #endregion
                
            #endif
        } // ReduceHiDefMode

        #endregion

        #region Expand Reach Mode

        /// <summary>
        /// Enables Hi-Def features on Reach devices.
        /// </summary>
        public static void ExpandReachMode()
        {
            #if WINDOWS
                // Read informtion
                RegistryKey registryEntry = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("XNA").OpenSubKey("Game Studio").OpenSubKey("v4.0");
                Assembly xnaAssemly = Assembly.LoadFrom(((string)registryEntry.GetValue("InstallPath")) + "/References/Windows/x86/Microsoft.Xna.Framework.Graphics.dll");
                Type profileCapabilities = xnaAssemly.GetType("Microsoft.Xna.Framework.Graphics.ProfileCapabilities");
                MethodInfo getIntanceInfo = profileCapabilities.GetMethod("GetInstance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                Object objProfileReach = getIntanceInfo.Invoke(profileCapabilities, new object[] { GraphicsProfile.Reach });

                ChangeValue(objProfileReach, "SeparateAlphaBlend", true);
                ChangeValue(objProfileReach, "MaxRenderTargets", 4);
                ChangeValue(objProfileReach, "MaxTextureSize", 4096);
                ChangeValue(objProfileReach, "MaxCubeSize", 4096);
                ChangeValue(objProfileReach, "MaxVertexSamplers", 4);
       
                FieldInfo fieldValidate = objProfileReach.GetType().GetField("ValidTextureFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                List<SurfaceFormat> validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileReach);
                validateFormats.Add(SurfaceFormat.Alpha8);
                validateFormats.Add(SurfaceFormat.Rgba1010102);
                validateFormats.Add(SurfaceFormat.Rg32);
                validateFormats.Add(SurfaceFormat.Rgba64);
                validateFormats.Add(SurfaceFormat.Single);
                validateFormats.Remove(SurfaceFormat.Vector2);
                validateFormats.Remove(SurfaceFormat.Vector4);
                validateFormats.Add(SurfaceFormat.Rgba64);
                validateFormats.Remove(SurfaceFormat.HalfSingle);
                validateFormats.Remove(SurfaceFormat.HalfVector2);
                validateFormats.Remove(SurfaceFormat.HalfVector4);
             
                //Validate TextureFormat 
                ExpandTextureFormats(objProfileReach);
            #endif
        } // ExpandReachMode
        
        #region Expand Texture Formats

        internal static void ExpandTextureFormats(Object objProfileReach)
        {
            FieldInfo fieldValidate = objProfileReach.GetType().GetField("ValidTextureFormats", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            List<SurfaceFormat> validateFormats = (List<SurfaceFormat>)fieldValidate.GetValue(objProfileReach);
            validateFormats.Add(SurfaceFormat.Alpha8);
            validateFormats.Add(SurfaceFormat.Rgba1010102);
            validateFormats.Add(SurfaceFormat.Rg32);
            validateFormats.Add(SurfaceFormat.Rgba64);
            validateFormats.Add(SurfaceFormat.Single);
            validateFormats.Add(SurfaceFormat.Vector2);
            validateFormats.Add(SurfaceFormat.Vector4);
            validateFormats.Add(SurfaceFormat.HalfSingle);
            validateFormats.Add(SurfaceFormat.HalfVector2);
            validateFormats.Add(SurfaceFormat.HalfVector4);
        } // ExpandTextureFormats

        #endregion

        #endregion

        #region Change Value

        private static void ChangeValue(Object objProfile, string name, int value)
        {
            FieldInfo fieldObj = objProfile.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldObj.SetValue(objProfile, value);
        } // ChangeMaxTextureSize

        private static void ChangeValue(Object objProfile, string name, bool value)
        {
            FieldInfo fieldObj = objProfile.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldObj.SetValue(objProfile, value);
        } // ChangeMaxTextureSize

        private static void ChangeValue(Object objProfile, string name, uint value)
        {
            FieldInfo fieldObj = objProfile.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldObj.SetValue(objProfile, value);
        } // ChangeMaxTextureSize

        #endregion

        #region Extend HiDef Support
        
        /// <summary>
        /// Check if there is an available HiDef device available.
        /// If not, extend the HiDef profile to cover the missing parts.
        /// </summary>
        public static void ExtendHiDefSupport()
        {
            #if WINDOWS
                bool supported = false;
                foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
                {
                    if (adapter.IsProfileSupported(GraphicsProfile.HiDef))
                    {
                        supported = true;
                        break;
                    }
                }
                // If there is an Hi Def addapter we do nothing
                if (!supported)
                {
                    try
                    {
                        //Changes Xna Hi-Def Profile to fit Graphics Card
                        ReduceHiDefMode();
                    }
                    catch { }

                    if (!GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef))
                    {
                        throw new Exception("Engine Manager: Couldn't find a suitable Graphics Adapter.");
                    }
                }
            #endif
        } // ExtendHiDefSupport

        #endregion

    } // GraphicsProfileExtender
} // XNAFinalEngine.EngineCore