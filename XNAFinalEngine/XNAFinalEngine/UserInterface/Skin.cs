
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Graphics.Texture;

#endregion

namespace XNAFinalEngine.UI
{

    #region Structs

    /// <summary>
    /// Skin element states.
    /// </summary>
    public struct SkinStates<T>
    {
        public T Enabled;
        public T Hovered;
        public T Pressed;
        public T Focused;
        public T Disabled;

        public SkinStates(T enabled, T hovered, T pressed, T focused, T disabled)
        {
            Enabled = enabled;
            Hovered = hovered;
            Pressed = pressed;
            Focused = focused;
            Disabled = disabled;
        } // SkinStates

    } // SkinStates

    /// <summary>
    /// Layers states
    /// </summary>
    public struct LayerStates
    {
        public int Index;
        public Color Color;
        public bool Overlay;
    } // LayerStates

    /// <summary>
    /// Layer Overlays
    /// </summary>
    public struct LayerOverlays
    {
        public int Index;
        public Color Color;
    } // LayerOverlays

    #endregion

    #region SkinList

    public class SkinList<T> : List<T>
    {

        #region Indexers

        public T this[string index]
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    SkinBase s = (SkinBase)(object)this[i];
                    if (s.Name.ToLower() == index.ToLower())
                    {
                        return this[i];
                    }
                }
                return default(T);
            }

            set
            {
                for (int i = 0; i < Count; i++)
                {
                    SkinBase s = (SkinBase)(object)this[i];
                    if (s.Name.ToLower() == index.ToLower())
                    {
                        this[i] = value;
                    }
                }
            }
        } // this

        #endregion

        #region Constructors

        public SkinList() { }

        public SkinList(SkinList<T> source)
        {
            foreach (T t1 in source)
            {
                Type[] t = new Type[1];
                t[0] = typeof(T);

                object[] p = new object[1];
                p[0] = t1;

                Add((T)t[0].GetConstructor(t).Invoke(p));
            }
        } // SkinList

        #endregion

    } // SkinList

    #endregion

    #region SkinBase

    public class SkinBase
    {

        #region Variables

        public string Name;
        public bool Archive;

        #endregion

        #region Constructors

        public SkinBase()
        {
            Archive = false;
        } // SkinBase

        public SkinBase(SkinBase source)
        {
            if (source != null)
            {
                Name = source.Name;
                Archive = source.Archive;
            }
        } // SkinBase

        #endregion

    } // SkinBase

    #endregion

    #region SkinLayer

    public class SkinLayer : SkinBase
    {

        #region Variables

        public SkinImage Image = new SkinImage();
        public int Width;
        public int Height;
        public int OffsetX;
        public int OffsetY;
        public Alignment Alignment;
        public Margins SizingMargins;
        public Margins ContentMargins;
        public SkinStates<LayerStates> States;
        public SkinStates<LayerOverlays> Overlays;
        public SkinText Text = new SkinText();
        public SkinList<SkinAttribute> Attributes = new SkinList<SkinAttribute>();

        #endregion

        #region Constructors

        public SkinLayer()
        {
            States.Enabled.Color = Color.White;
            States.Pressed.Color = Color.White;
            States.Focused.Color = Color.White;
            States.Hovered.Color = Color.White;
            States.Disabled.Color = Color.White;

            Overlays.Enabled.Color = Color.White;
            Overlays.Pressed.Color = Color.White;
            Overlays.Focused.Color = Color.White;
            Overlays.Hovered.Color = Color.White;
            Overlays.Disabled.Color = Color.White;
        } // SkinLayer

        public SkinLayer(SkinLayer source) : base(source)
        {
            if (source != null)
            {
                Image = new SkinImage(source.Image);
                Width = source.Width;
                Height = source.Height;
                OffsetX = source.OffsetX;
                OffsetY = source.OffsetY;
                Alignment = source.Alignment;
                SizingMargins = source.SizingMargins;
                ContentMargins = source.ContentMargins;
                States = source.States;
                Overlays = source.Overlays;
                Text = new SkinText(source.Text);
                Attributes = new SkinList<SkinAttribute>(source.Attributes);
            }
            else
            {
                throw new Exception("Parameter for SkinLayer copy constructor cannot be null.");
            }
        } // SkinLayer

        #endregion

    } // SkinLayer

    #endregion

    #region SkinText

    public class SkinText : SkinBase
    {

        #region Variables

        public SkinFont Font;
        public int OffsetX;
        public int OffsetY;
        public Alignment Alignment;
        public SkinStates<Color> Colors;

        #endregion

        #region Constructors

        public SkinText()
        {
            Colors.Enabled = Color.White;
            Colors.Pressed = Color.White;
            Colors.Focused = Color.White;
            Colors.Hovered = Color.White;
            Colors.Disabled = Color.White;
        } // SkinText

        public SkinText(SkinText source) : base(source)
        {
            if (source != null)
            {
                Font = new SkinFont(source.Font);
                OffsetX = source.OffsetX;
                OffsetY = source.OffsetY;
                Alignment = source.Alignment;
                Colors = source.Colors;
            }
        } // SkinText

        #endregion

    } // SkinText

    #endregion

    #region SkinFont

    public class SkinFont : SkinBase
    {

        #region Variables

        public SpriteFont Resource;
        public string Asset;

        #endregion

        #region Properties
        
        public int Height
        {
            get
            {
                if (Resource != null)
                {
                    return (int)Resource.MeasureString("AaYy").Y;
                }
                return 0;
            }
        } // Height

        #endregion

        #region Constructors

        public SkinFont() { }

        public SkinFont(SkinFont source) : base(source)
        {
            if (source != null)
            {
                Resource = source.Resource;
                Asset = source.Asset;
            }
        } // SkinFont

        #endregion

    } // SkinFont

    #endregion

    #region SkinImage

    public class SkinImage : SkinBase
    {

        #region Variables

        public Texture Texture;
        public string Asset;

        #endregion

        #region Constructors
        
        public SkinImage() { }

        public SkinImage(SkinImage source) : base(source)
        {
            Texture = source.Texture;
            Asset = source.Asset;
        } // SkinImage

        #endregion

    } // SkinImage

    #endregion

    #region SkinCursor

    public class SkinCursor : SkinBase
    {

        #region Variables

        public Cursor Resource;
        public string Asset;

        #endregion

        #region Constructors

        public SkinCursor() { }
        
        public SkinCursor(SkinCursor source) : base(source)
        {
            Resource = source.Resource;

            Asset = source.Asset;
        } // SkinCursor

        #endregion

    } // SkinCursor

    #endregion

    #region SkinControl

    public class SkinControl : SkinBase
    {

        #region Variables

        public Size DefaultSize;
        public int ResizerSize;
        public Size MinimumSize;
        public Margins OriginMargins;
        public Margins ClientMargins;
        public SkinList<SkinLayer> Layers = new SkinList<SkinLayer>();
        public SkinList<SkinAttribute> Attributes = new SkinList<SkinAttribute>();

        #endregion

        #region Constructors

        public SkinControl() { }

        public SkinControl(SkinControl source) : base(source)
        {
            DefaultSize = source.DefaultSize;
            MinimumSize = source.MinimumSize;
            OriginMargins = source.OriginMargins;
            ClientMargins = source.ClientMargins;
            ResizerSize = source.ResizerSize;
            Layers = new SkinList<SkinLayer>(source.Layers);
            Attributes = new SkinList<SkinAttribute>(source.Attributes);
        } // SkinControl

        #endregion
    }

    #endregion

    #region SkinAtribute

    public class SkinAttribute : SkinBase
    {

        #region Variables
        
        public string Value;
        
        #endregion

        #region Contructors

        public SkinAttribute() { }

        public SkinAttribute(SkinAttribute source) : base(source)
        {
            Value = source.Value;
        } // SkinAttribute

        #endregion

    } // SkinAttribute

    #endregion

    #region Skin

    /// <summary>
    /// Manage the skin content (mouse cursors, elements' images, fonts, and skin's parameters)
    /// </summary>
    public class Skin : Disposable
    {

        #region Variables

        /// <summary>
        /// Skin xml document.
        /// </summary>
        private SkinXmlDocument doc;

        /// <summary>
        /// Skin content manager.
        /// </summary>
        private readonly ContentManager skinContent;

        #endregion

        #region Properties
       
        /// <summary>
        /// Skin information for controls.
        /// </summary>
        public SkinList<SkinControl>   Controls   { get; private set; }

        /// <summary>
        /// Skin information for fonts.
        /// </summary>
        public SkinList<SkinFont>      Fonts      { get; private set; }

        /// <summary>
        /// Skin information for cursors.
        /// </summary>
        public SkinList<SkinCursor>    Cursors    { get; private set; }

        /// <summary>
        /// Skin information for images.
        /// </summary>
        public SkinList<SkinImage>     Images     { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Manage the skin content (mouse cursors, elements' images, fonts, and skin's parameters)
        /// </summary>
        public Skin(string skinFilename)
        {
            skinContent = new ContentManager(EngineManager.Services);
            doc = new SkinXmlDocument();
            Controls = new SkinList<SkinControl>();
            Fonts = new SkinList<SkinFont>();
            Images = new SkinList<SkinImage>();
            Cursors = new SkinList<SkinCursor>();

            LoadSkin(skinFilename);
        } // Skin

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            //skinContent.Unload(); // ContentManager.Dispose do this.
            skinContent.Dispose();
        } // DisposeUnmanagedResources

        #endregion

        #region Load Skin

        private void LoadSkin(string skinFilename)
        {
            string fullFilename = Directories.SkinDirectory + "\\" + skinFilename;

            #region Load file

            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load skin: File " + fullFilename + " does not exists!");
            } // if (File.Exists)

            try
            {
                doc = skinContent.Load<SkinXmlDocument>(fullFilename);
            }
            catch
            {
                throw new Exception("Failed to load skin: " + skinFilename);
            }

            #endregion

            #region Load information

            XmlElement e = doc["Skin"];
            if (e != null)
            {
                LoadImages();
                LoadFonts();
                LoadCursors();
                LoadControls();
            }
            else
            {
                throw new Exception("Failed to load skin: " + skinFilename + ". Skin tag doesn't exist.");
            }

            #endregion

            #region Load Resources

            foreach (SkinFont skinFont in Fonts)
            {
                string asset = Directories.SkinDirectory +  "\\Fonts\\" + skinFont.Asset;
                (skinFont.Resource) = skinContent.Load<SpriteFont>(asset);
            }

            foreach (SkinCursor skinCursor in Cursors)
            {
                string asset = Directories.SkinDirectory + "\\Cursors\\" + skinCursor.Asset;
                skinCursor.Resource = skinContent.Load<Cursor>(asset);
            }

            foreach (SkinImage skinImage in Images)
            {
                skinImage.Texture = new Texture("Skin\\" + skinImage.Asset);
            }

            foreach (SkinControl skinControl in Controls)
            {
                foreach (SkinLayer skinLayer in skinControl.Layers)
                {
                    if (skinLayer.Image.Name != null)
                    {
                        skinLayer.Image = Images[skinLayer.Image.Name];
                    }
                    else
                    {
                        skinLayer.Image = Images[0];
                    }

                    if (skinLayer.Text.Name != null)
                    {
                        skinLayer.Text.Font = Fonts[skinLayer.Text.Name];
                    }
                    else
                    {
                        skinLayer.Text.Font = Fonts[0];
                    }
                }
            }

            #endregion

        } // LoadSkin

        #endregion

        #region Load Controls

        /// <summary>
        /// Load the skin information of every control
        /// </summary>
        private void LoadControls()
        {
            if (doc["Skin"]["Controls"] == null)
                return;

            XmlNodeList nodeList = doc["Skin"]["Controls"].GetElementsByTagName("Control");

            foreach (XmlElement node in nodeList)
            {
                SkinControl skinControl;
                // Create skin control
                string parent = ReadAttribute(node, "Inherits", null, false);
                bool inherit = false;
                if (parent != null) // If there is a parent then it loads the information from it.
                {
                    skinControl = new SkinControl(Controls[parent]);
                    inherit = true;
                }
                else
                {
                    skinControl = new SkinControl();
                }
                // Load general information
                ReadAttribute(ref skinControl.Name, inherit, node, "Name", null, true);

                ReadAttributeInt(ref skinControl.DefaultSize.Width,    inherit, node["DefaultSize"],   "Width",  0, false);
                ReadAttributeInt(ref skinControl.DefaultSize.Height,   inherit, node["DefaultSize"],   "Height", 0, false);

                ReadAttributeInt(ref skinControl.MinimumSize.Width,    inherit, node["MinimumSize"],   "Width",  0, false);
                ReadAttributeInt(ref skinControl.MinimumSize.Height,   inherit, node["MinimumSize"],   "Height", 0, false);

                ReadAttributeInt(ref skinControl.OriginMargins.Left,   inherit, node["OriginMargins"], "Left",   0, false);
                ReadAttributeInt(ref skinControl.OriginMargins.Top,    inherit, node["OriginMargins"], "Top",    0, false);
                ReadAttributeInt(ref skinControl.OriginMargins.Right,  inherit, node["OriginMargins"], "Right",  0, false);
                ReadAttributeInt(ref skinControl.OriginMargins.Bottom, inherit, node["OriginMargins"], "Bottom", 0, false);

                ReadAttributeInt(ref skinControl.ClientMargins.Left,   inherit, node["ClientMargins"], "Left",   0, false);
                ReadAttributeInt(ref skinControl.ClientMargins.Top,    inherit, node["ClientMargins"], "Top",    0, false);
                ReadAttributeInt(ref skinControl.ClientMargins.Right,  inherit, node["ClientMargins"], "Right",  0, false);
                ReadAttributeInt(ref skinControl.ClientMargins.Bottom, inherit, node["ClientMargins"], "Bottom", 0, false);

                ReadAttributeInt(ref skinControl.ResizerSize, inherit, node["ResizerSize"], "Value", 0, false);
                // Load layers
                if (node["Layers"] != null)
                {
                    XmlNodeList layersNodesList = node["Layers"].GetElementsByTagName("Layer");
                    if (layersNodesList.Count > 0)
                    {
                        LoadLayers(skinControl, layersNodesList);
                    }
                }
                Controls.Add(skinControl);
            }
        } // LoadControls

        #endregion

        #region Load Layers

        /// <summary>
        /// Load layers information
        /// </summary>
        private static void LoadLayers(SkinControl skinControl, XmlNodeList nodeList)
        {
            foreach (XmlElement node in nodeList)
            {
                string name = ReadAttribute(node, "Name", null, true);
                bool over = ReadAttributeBool(node, "Override", false, false);
                SkinLayer skinLayer = skinControl.Layers[name];
                
                bool inherent = true;
                if (skinLayer == null)
                {
                    skinLayer = new SkinLayer();
                    inherent = false;
                }

                if (inherent && over)
                {
                    skinLayer = new SkinLayer();
                    skinControl.Layers[name] = skinLayer;
                }

                ReadAttribute(   ref skinLayer.Name,       inherent, node, "Name", null, true);
                ReadAttribute(   ref skinLayer.Image.Name, inherent, node, "Image", "Control", false);
                ReadAttributeInt(ref skinLayer.Width,      inherent, node, "Width",  0, false);
                ReadAttributeInt(ref skinLayer.Height,     inherent, node, "Height", 0, false);

                string layerAlignment = skinLayer.Alignment.ToString();
                ReadAttribute(ref layerAlignment, inherent, node, "Alignment", "MiddleCenter", false);
                skinLayer.Alignment = (Alignment)Enum.Parse(typeof(Alignment), layerAlignment, true);

                ReadAttributeInt(ref skinLayer.OffsetX,               inherent, node, "OffsetX", 0, false);
                ReadAttributeInt(ref skinLayer.OffsetY,               inherent, node, "OffsetY", 0, false);

                ReadAttributeInt(ref skinLayer.SizingMargins.Left,    inherent, node["SizingMargins"], "Left",   0, false);
                ReadAttributeInt(ref skinLayer.SizingMargins.Top,     inherent, node["SizingMargins"], "Top",    0, false);
                ReadAttributeInt(ref skinLayer.SizingMargins.Right,   inherent, node["SizingMargins"], "Right",  0, false);
                ReadAttributeInt(ref skinLayer.SizingMargins.Bottom,  inherent, node["SizingMargins"], "Bottom", 0, false);

                ReadAttributeInt(ref skinLayer.ContentMargins.Left,   inherent, node["ContentMargins"], "Left",   0, false);
                ReadAttributeInt(ref skinLayer.ContentMargins.Top,    inherent, node["ContentMargins"], "Top",    0, false);
                ReadAttributeInt(ref skinLayer.ContentMargins.Right,  inherent, node["ContentMargins"], "Right",  0, false);
                ReadAttributeInt(ref skinLayer.ContentMargins.Bottom, inherent, node["ContentMargins"], "Bottom", 0, false);

                if (node["States"] != null)
                {
                    ReadAttributeInt(ref skinLayer.States.Enabled.Index, inherent, node["States"]["Enabled"], "Index", 0, false);
                    int di = skinLayer.States.Enabled.Index;
                    ReadAttributeInt(ref skinLayer.States.Hovered.Index, inherent, node["States"]["Hovered"], "Index", di, false);
                    ReadAttributeInt(ref skinLayer.States.Pressed.Index, inherent, node["States"]["Pressed"], "Index", di, false);
                    ReadAttributeInt(ref skinLayer.States.Focused.Index, inherent, node["States"]["Focused"], "Index", di, false);
                    ReadAttributeInt(ref skinLayer.States.Disabled.Index, inherent, node["States"]["Disabled"], "Index", di, false);

                    ReadAttributeColor(ref skinLayer.States.Enabled.Color, inherent, node["States"]["Enabled"], "Color", Color.White, false);
                    Color dc = skinLayer.States.Enabled.Color;
                    ReadAttributeColor(ref skinLayer.States.Hovered.Color, inherent, node["States"]["Hovered"], "Color", dc, false);
                    ReadAttributeColor(ref skinLayer.States.Pressed.Color, inherent, node["States"]["Pressed"], "Color", dc, false);
                    ReadAttributeColor(ref skinLayer.States.Focused.Color, inherent, node["States"]["Focused"], "Color", dc, false);
                    ReadAttributeColor(ref skinLayer.States.Disabled.Color, inherent, node["States"]["Disabled"], "Color", dc, false);

                    ReadAttributeBool(ref skinLayer.States.Enabled.Overlay, inherent, node["States"]["Enabled"], "Overlay", false, false);
                    bool dv = skinLayer.States.Enabled.Overlay;
                    ReadAttributeBool(ref skinLayer.States.Hovered.Overlay, inherent, node["States"]["Hovered"], "Overlay", dv, false);
                    ReadAttributeBool(ref skinLayer.States.Pressed.Overlay, inherent, node["States"]["Pressed"], "Overlay", dv, false);
                    ReadAttributeBool(ref skinLayer.States.Focused.Overlay, inherent, node["States"]["Focused"], "Overlay", dv, false);
                    ReadAttributeBool(ref skinLayer.States.Disabled.Overlay, inherent, node["States"]["Disabled"], "Overlay", dv, false);
                }

                if (node["Overlays"] != null)
                {
                    ReadAttributeInt(ref skinLayer.Overlays.Enabled.Index, inherent, node["Overlays"]["Enabled"], "Index", 0, false);
                    int di = skinLayer.Overlays.Enabled.Index;
                    ReadAttributeInt(ref skinLayer.Overlays.Hovered.Index, inherent, node["Overlays"]["Hovered"], "Index", di, false);
                    ReadAttributeInt(ref skinLayer.Overlays.Pressed.Index, inherent, node["Overlays"]["Pressed"], "Index", di, false);
                    ReadAttributeInt(ref skinLayer.Overlays.Focused.Index, inherent, node["Overlays"]["Focused"], "Index", di, false);
                    ReadAttributeInt(ref skinLayer.Overlays.Disabled.Index, inherent, node["Overlays"]["Disabled"], "Index", di, false);

                    ReadAttributeColor(ref skinLayer.Overlays.Enabled.Color, inherent, node["Overlays"]["Enabled"], "Color", Color.White, false);
                    Color dc = skinLayer.Overlays.Enabled.Color;
                    ReadAttributeColor(ref skinLayer.Overlays.Hovered.Color, inherent, node["Overlays"]["Hovered"], "Color", dc, false);
                    ReadAttributeColor(ref skinLayer.Overlays.Pressed.Color, inherent, node["Overlays"]["Pressed"], "Color", dc, false);
                    ReadAttributeColor(ref skinLayer.Overlays.Focused.Color, inherent, node["Overlays"]["Focused"], "Color", dc, false);
                    ReadAttributeColor(ref skinLayer.Overlays.Disabled.Color, inherent, node["Overlays"]["Disabled"], "Color", dc, false);
                }

                if (node["Text"] != null)
                {
                    ReadAttribute(ref skinLayer.Text.Name, inherent, node["Text"], "Font", null, true);
                    ReadAttributeInt(ref skinLayer.Text.OffsetX, inherent, node["Text"], "OffsetX", 0, false);
                    ReadAttributeInt(ref skinLayer.Text.OffsetY, inherent, node["Text"], "OffsetY", 0, false);

                    layerAlignment = skinLayer.Text.Alignment.ToString();
                    ReadAttribute(ref layerAlignment, inherent, node["Text"], "Alignment", "MiddleCenter", false);
                    skinLayer.Text.Alignment = (Alignment)Enum.Parse(typeof(Alignment), layerAlignment, true);

                    LoadColors(inherent, node["Text"], ref skinLayer.Text.Colors);
                }
                if (node["Attributes"] != null)
                {
                    XmlNodeList attributesNodeList = node["Attributes"].GetElementsByTagName("Attribute");
                    if (attributesNodeList.Count > 0)
                    {
                        LoadLayerAttributes(skinLayer, attributesNodeList);
                    }
                }
                if (!inherent)
                    skinControl.Layers.Add(skinLayer);
            }
        } // LoadLayers

        #region Load Colors

        private static void LoadColors(bool inherited, XmlElement e, ref SkinStates<Color> colors)
        {
            if (e != null)
            {
                ReadAttributeColor(ref colors.Enabled,  inherited, e["Colors"]["Enabled"],  "Color", Color.White,    false);
                ReadAttributeColor(ref colors.Hovered,  inherited, e["Colors"]["Hovered"],  "Color", colors.Enabled, false);
                ReadAttributeColor(ref colors.Pressed,  inherited, e["Colors"]["Pressed"],  "Color", colors.Enabled, false);
                ReadAttributeColor(ref colors.Focused,  inherited, e["Colors"]["Focused"],  "Color", colors.Enabled, false);
                ReadAttributeColor(ref colors.Disabled, inherited, e["Colors"]["Disabled"], "Color", colors.Enabled, false);
            }
        } // LoadColors

        #endregion

        #region Load Layer Attributes

        /// <summary>
        /// Load Layer Attributes
        /// </summary>
        private static void LoadLayerAttributes(SkinLayer skinLayer, XmlNodeList nodeList)
        {
            foreach (XmlElement e in nodeList)
            {
                string name = ReadAttribute(e, "Name", null, true);
                SkinAttribute skinAttribute = skinLayer.Attributes[name];
                bool inherent = true;

                if (skinAttribute == null)
                {
                    skinAttribute = new SkinAttribute();
                    inherent = false;
                }

                skinAttribute.Name = name;
                ReadAttribute(ref skinAttribute.Value, inherent, e, "Value", null, true);

                if (!inherent) 
                    skinLayer.Attributes.Add(skinAttribute);
            }
        } // LoadLayerAttributes

        #endregion

        #endregion

        #region Load Fonts

        /// <summary>
        /// Load the fonts information
        /// </summary>
        private void LoadFonts()
        {
            if (doc["Skin"]["Fonts"] == null)
                return;

            XmlNodeList nodeList = doc["Skin"]["Fonts"].GetElementsByTagName("Font");
            if (nodeList.Count > 0)
            {
                foreach (XmlElement node in nodeList)
                {
                    SkinFont skinFont = new SkinFont
                    {
                        Name  = ReadAttribute(node, "Name", null, true),
                        Asset = ReadAttribute(node, "Asset", null, true)
                    };
                    Fonts.Add(skinFont);
                }
            }
        } // LoadFonts

        #endregion

        #region Load Cursors

        /// <summary>
        /// Load the cursors information
        /// </summary>
        private void LoadCursors()
        {
            if (doc["Skin"]["Cursors"] == null)
                return;

            XmlNodeList nodeList = doc["Skin"]["Cursors"].GetElementsByTagName("Cursor");
            if (nodeList.Count > 0)
            {
                foreach (XmlElement node in nodeList)
                {
                    SkinCursor skinCursor = new SkinCursor
                    {
                        Name = ReadAttribute(node, "Name", null, true),
                        Asset = ReadAttribute(node, "Asset", null, true)
                    };
                    Cursors.Add(skinCursor);
                }
            }
        } // LoadCursors

        #endregion

        #region Load Images

        /// <summary>
        /// Load the images information
        /// </summary>
        private void LoadImages()
        {
            if (doc["Skin"]["Images"] == null) return;
            XmlNodeList nodeList = doc["Skin"]["Images"].GetElementsByTagName("Image");
            if (nodeList.Count > 0)
            {
                foreach (XmlElement node in nodeList)
                {
                    SkinImage skinImage = new SkinImage
                    {
                        Name = ReadAttribute(node, "Name", null, true),
                        Asset = ReadAttribute(node, "Asset", null, true)
                    };
                    Images.Add(skinImage);
                }
            }
        } // LoadImages

        #endregion

        #region Read Attribute

        private static string ReadAttribute(XmlElement element, string attrib, string defval, bool needed)
        {
            if (element != null && element.HasAttribute(attrib))
            {
                return element.Attributes[attrib].Value;
            }
            if (needed)
            {
                throw new Exception("Missing required attribute \"" + attrib + "\" in the skin file.");
            }
            return defval;
        } // ReadAttribute

        private static void ReadAttribute(ref string retval, bool inherited, XmlElement node, string attributeName, string defaultValue, bool needed)
        {
            if (node != null && node.HasAttribute(attributeName))
            {
                retval = node.Attributes[attributeName].Value;
            }
            else if (inherited)
            {
                // Do nothing, the parent has the attribute
            }
            else if (needed)
            {
                throw new Exception("Missing required attribute \"" + attributeName + "\" in the skin file.");
            }
            else
            {
                retval = defaultValue;
            }
        } // ReadAttribute

        private static void ReadAttributeInt(ref int retval, bool inherited, XmlElement element, string attrib, int defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = int.Parse(tmp);
        } // ReadAttributeInt

        private static bool ReadAttributeBool(XmlElement element, string attrib, bool defval, bool needed)
        {
            return bool.Parse(ReadAttribute(element, attrib, defval.ToString(), needed));
        } // ReadAttributeBool

        private static void ReadAttributeBool(ref bool retval, bool inherited, XmlElement element, string attrib, bool defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = bool.Parse(tmp);
        } // ReadAttributeBool

        private static string ColorToString(Color c)
        {
            return string.Format("{0};{1};{2};{3}", c.R, c.G, c.B, c.A);
        } // ColorToString

        private static void ReadAttributeColor(ref Color retval, bool inherited, XmlElement element, string attrib, Color defval, bool needed)
        {
            string tmp = ColorToString(retval);
            ReadAttribute(ref tmp, inherited, element, attrib, ColorToString(defval), needed);
            retval = Utilities.ParseColor(tmp);
        } // ReadAttributeColor

        #endregion

    } // Skin

    #endregion

} // XNAFinalEngine.UI
