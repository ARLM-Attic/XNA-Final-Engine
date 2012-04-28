
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
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.UserInterface
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
                    //if (s.Name.ToLower() == index.ToLower()) // Not need to produce so much garbage unnecessary.
                    if (s.Name == index)
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
                    //if (s.Name.ToLower() == index.ToLower())
                    if (s.Name == index)
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

        /// <summary>
        /// Name.
        /// </summary>
        public string Name;

        #endregion

        #region Constructors

        public SkinBase() { }

        public SkinBase(SkinBase source)
        {
            if (source != null)
            {
                Name = source.Name;
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

        /// <summary>
        /// Associated font.
        /// </summary>
        public SkinFont Font;

        /// <summary>
        /// Offset from the left.
        /// </summary>
        public int OffsetX;

        /// <summary>
        /// Offset from the bottom.
        /// </summary>
        public int OffsetY;

        /// <summary>
        /// Text aligment.
        /// </summary>
        public Alignment Alignment;

        /// <summary>
        /// Colors when enabled, hovered, pressed, focused, and disabled.
        /// </summary>
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

        /// <summary>
        /// Asset.
        /// </summary>
        public Font Font;

        /// <summary>
        /// Asset filename.
        /// </summary>
        public string Filename;

        #endregion

        #region Properties
        
        public int Height
        {
            get
            {
                if (Font != null)
                {
                    return (int)Font.MeasureString("AaYy").Y;
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
                Font = source.Font;
                Filename = source.Filename;
            }
        } // SkinFont

        #endregion

    } // SkinFont

    #endregion

    #region SkinImage

    public class SkinImage : SkinBase
    {

        #region Variables

        /// <summary>
        /// Asset.
        /// </summary>
        public Texture Texture;

        /// <summary>
        /// Asset filename.
        /// </summary>
        public string Filename;

        #endregion

        #region Constructors
        
        public SkinImage() { }

        public SkinImage(SkinImage source) : base(source)
        {
            Texture = source.Texture;
            Filename = source.Filename;
        } // SkinImage

        #endregion

    } // SkinImage

    #endregion

    #region SkinCursor

    #if (WINDOWS)

        public class SkinCursor : SkinBase
        {

            /// <summary>
            /// Asset.
            /// </summary>
            public Cursor Cursor;

            /// <summary>
            /// Asset filename.
            /// </summary>
            public string Filename;
            
        } // SkinCursor

    #endif

    #endregion

    #region SkinControl

    public class SkinControlInformation : SkinBase
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

        public SkinControlInformation() { }

        public SkinControlInformation(SkinControlInformation source) : base(source)
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

    } // SkinControl

    #endregion

    #region SkinAtribute

    public class SkinAttribute : SkinBase
    {

        #region Variables
        
        /// <summary>
        /// Value.
        /// </summary>
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
    /// The main task is to load the skin information and skin resources.
    /// </summary>
    public static class Skin
    {

        #region Variables

        /// <summary>
        /// Skin XML document.
        /// </summary>
        private static Document skinDescription;

        /// <summary>
        /// Skin content manager.
        /// </summary>
        private static ContentManager skinContentManager;
        
        #endregion

        #region Properties
       
        /// <summary>
        /// Skin information for controls.
        /// </summary>
        public static SkinList<SkinControlInformation> Controls { get; private set; }

        /// <summary>
        /// Skin information for fonts.
        /// </summary>
        public static SkinList<SkinFont> Fonts { get; private set; }

        #if (WINDOWS)
            /// <summary>
            /// Skin information for cursors.
            /// </summary>
            public static SkinList<SkinCursor> Cursors { get; private set; }
        #endif

        /// <summary>
        /// Skin information for images.
        /// </summary>
        public static SkinList<SkinImage> Images { get; private set; }

        #endregion

        #region Load Skin

        /// <summary>
        /// Manage the skin content (mouse cursors, elements' images, fonts, and skin's parameters)
        /// </summary>
        public static void LoadSkin(string skinName)
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;

            #region Unload previous skin
            
            Controls = new SkinList<SkinControlInformation>();
            Fonts = new SkinList<SkinFont>();
            Images = new SkinList<SkinImage>();
            #if (WINDOWS)
                Cursors = new SkinList<SkinCursor>();
            #endif

            if (skinContentManager == null)
                skinContentManager = new ContentManager("Skin Content Manager", true);
            else
                skinContentManager.Unload();
            ContentManager.CurrentContentManager = skinContentManager;

            #endregion

            #region Load Description File

            string fullPath = "Skin" + "\\" + skinName + "\\Description";
            skinDescription = new Document(fullPath);
            
            // Read XML data.
            if (skinDescription.Resource.Element("Skin") != null)
            {
                try
                {
                    LoadImagesDescription();
                    LoadFontsDescription();
                    #if (WINDOWS)
                        LoadCursorsDescription();
                    #endif
                    LoadControlsDescription();
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to load skin: " + skinName + ".\n\n" + e.Message);
                }
            }
            else
            {
                throw new Exception("Failed to load skin: " + skinName + ". Skin tag doesn't exist.");
            }

            #endregion

            #region Load Resources

            try
            {
                foreach (SkinFont skinFont in Fonts)
                {
                    skinFont.Font = new Font(skinFont.Filename);
                }
                #if (WINDOWS)
                    foreach (SkinCursor skinCursor in Cursors)
                    {
                        skinCursor.Cursor = new Assets.Cursor(skinName + "\\" + skinCursor.Filename);
                    }
                #endif
                foreach (SkinImage skinImage in Images)
                {
                    skinImage.Texture = new Texture("Skin\\" + skinName + "\\" + skinImage.Filename);
                }
                foreach (SkinControlInformation skinControl in Controls)
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
                        skinLayer.Text.Font = skinLayer.Text.Name != null ? Fonts[skinLayer.Text.Name] : Fonts[0];
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load skin: " + skinName + ".\n\n" + e.Message);
            }

            #endregion
            
            // Restore user content manager.
            ContentManager.CurrentContentManager = userContentManager;

        } // LoadSkin

        #endregion

        #region Load Controls

        /// <summary>
        /// Load the skin information of every control.
        /// </summary>
        private static void LoadControlsDescription()
        {
            if (skinDescription.Resource.Element("Skin").Element("Controls") == null)
                return;

            foreach (XElement control in skinDescription.Resource.Descendants("Control"))
            {
                SkinControlInformation skinControl;
                // Create skin control
                string parent = ReadAttribute(control, "Inherits", null, false);
                bool inherit = false;
                if (parent != null) // If there is a parent then it loads the information from it.
                {
                    skinControl = new SkinControlInformation(Controls[parent]);
                    inherit = true;
                }
                else
                    skinControl = new SkinControlInformation();

                // Load general information
                ReadAttribute(ref skinControl.Name, inherit, control, "Name", null, true);

                ReadAttribute(ref skinControl.DefaultSize.Width,    inherit, control.Element("DefaultSize"),   "Width",  0, false);
                ReadAttribute(ref skinControl.DefaultSize.Height,   inherit, control.Element("DefaultSize"),   "Height", 0, false);

                ReadAttribute(ref skinControl.MinimumSize.Width,    inherit, control.Element("MinimumSize"),   "Width",  0, false);
                ReadAttribute(ref skinControl.MinimumSize.Height,   inherit, control.Element("MinimumSize"),   "Height", 0, false);

                ReadAttribute(ref skinControl.OriginMargins.Left,   inherit, control.Element("OriginMargins"), "Left",   0, false);
                ReadAttribute(ref skinControl.OriginMargins.Top,    inherit, control.Element("OriginMargins"), "Top",    0, false);
                ReadAttribute(ref skinControl.OriginMargins.Right,  inherit, control.Element("OriginMargins"), "Right",  0, false);
                ReadAttribute(ref skinControl.OriginMargins.Bottom, inherit, control.Element("OriginMargins"), "Bottom", 0, false);

                ReadAttribute(ref skinControl.ClientMargins.Left,   inherit, control.Element("ClientMargins"), "Left",   0, false);
                ReadAttribute(ref skinControl.ClientMargins.Top,    inherit, control.Element("ClientMargins"), "Top",    0, false);
                ReadAttribute(ref skinControl.ClientMargins.Right,  inherit, control.Element("ClientMargins"), "Right",  0, false);
                ReadAttribute(ref skinControl.ClientMargins.Bottom, inherit, control.Element("ClientMargins"), "Bottom", 0, false);

                ReadAttribute(ref skinControl.ResizerSize, inherit, control.Element("ResizerSize"), "Value", 0, false);
                // Load control's layers
                if (control.Element("Layers") != null)
                {
                    foreach (var layer in control.Element("Layers").Elements())
                    {
                        if (layer.Name == "Layer")
                        {
                            LoadLayer(skinControl, layer);
                        }
                    }
                }
                Controls.Add(skinControl);
            }
        } // LoadControls

        #endregion

        #region Load Layers

        /// <summary>
        /// Load layers information.
        /// </summary>
        private static void LoadLayer(SkinControlInformation skinControl, XElement layerNode)
        {
            string name = ReadAttribute(layerNode, "Name", null, true);
            bool over = ReadAttribute(layerNode, "Override", false, false);
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

            ReadAttribute(ref skinLayer.Name,       inherent, layerNode, "Name", null, true);
            ReadAttribute(ref skinLayer.Image.Name, inherent, layerNode, "Image", "Control", false);
            ReadAttribute(ref skinLayer.Width,      inherent, layerNode, "Width",  0, false);
            ReadAttribute(ref skinLayer.Height,     inherent, layerNode, "Height", 0, false);

            string layerAlignment = skinLayer.Alignment.ToString();
            ReadAttribute(ref layerAlignment, inherent, layerNode, "Alignment", "MiddleCenter", false);
            skinLayer.Alignment = (Alignment)Enum.Parse(typeof(Alignment), layerAlignment, true);

            ReadAttribute(ref skinLayer.OffsetX,               inherent, layerNode, "OffsetX", 0, false);
            ReadAttribute(ref skinLayer.OffsetY,               inherent, layerNode, "OffsetY", 0, false);

            ReadAttribute(ref skinLayer.SizingMargins.Left,    inherent, layerNode.Element("SizingMargins"), "Left",   0, false);
            ReadAttribute(ref skinLayer.SizingMargins.Top,     inherent, layerNode.Element("SizingMargins"), "Top",    0, false);
            ReadAttribute(ref skinLayer.SizingMargins.Right,   inherent, layerNode.Element("SizingMargins"), "Right",  0, false);
            ReadAttribute(ref skinLayer.SizingMargins.Bottom,  inherent, layerNode.Element("SizingMargins"), "Bottom", 0, false);

            ReadAttribute(ref skinLayer.ContentMargins.Left,   inherent, layerNode.Element("ContentMargins"), "Left",   0, false);
            ReadAttribute(ref skinLayer.ContentMargins.Top,    inherent, layerNode.Element("ContentMargins"), "Top",    0, false);
            ReadAttribute(ref skinLayer.ContentMargins.Right,  inherent, layerNode.Element("ContentMargins"), "Right",  0, false);
            ReadAttribute(ref skinLayer.ContentMargins.Bottom, inherent, layerNode.Element("ContentMargins"), "Bottom", 0, false);

            #region States

            if (layerNode.Element("States") != null)
            {
                ReadAttribute(ref skinLayer.States.Enabled.Index, inherent, layerNode.Element("States").Element("Enabled"), "Index", 0, false);
                int di = skinLayer.States.Enabled.Index;
                ReadAttribute(ref skinLayer.States.Hovered.Index, inherent, layerNode.Element("States").Element("Hovered"), "Index", di, false);
                ReadAttribute(ref skinLayer.States.Pressed.Index, inherent, layerNode.Element("States").Element("Pressed"), "Index", di, false);
                ReadAttribute(ref skinLayer.States.Focused.Index, inherent, layerNode.Element("States").Element("Focused"), "Index", di, false);
                ReadAttribute(ref skinLayer.States.Disabled.Index, inherent, layerNode.Element("States").Element("Disabled"), "Index", di, false);

                ReadAttribute(ref skinLayer.States.Enabled.Color, inherent, layerNode.Element("States").Element("Enabled"), "Color", Color.White, false);
                Color dc = skinLayer.States.Enabled.Color;
                ReadAttribute(ref skinLayer.States.Hovered.Color, inherent, layerNode.Element("States").Element("Hovered"), "Color", dc, false);
                ReadAttribute(ref skinLayer.States.Pressed.Color, inherent, layerNode.Element("States").Element("Pressed"), "Color", dc, false);
                ReadAttribute(ref skinLayer.States.Focused.Color, inherent, layerNode.Element("States").Element("Focused"), "Color", dc, false);
                ReadAttribute(ref skinLayer.States.Disabled.Color, inherent, layerNode.Element("States").Element("Disabled"), "Color", dc, false);

                ReadAttribute(ref skinLayer.States.Enabled.Overlay, inherent, layerNode.Element("States").Element("Enabled"), "Overlay", false, false);
                bool dv = skinLayer.States.Enabled.Overlay;
                ReadAttribute(ref skinLayer.States.Hovered.Overlay, inherent, layerNode.Element("States").Element("Hovered"), "Overlay", dv, false);
                ReadAttribute(ref skinLayer.States.Pressed.Overlay, inherent, layerNode.Element("States").Element("Pressed"), "Overlay", dv, false);
                ReadAttribute(ref skinLayer.States.Focused.Overlay, inherent, layerNode.Element("States").Element("Focused"), "Overlay", dv, false);
                ReadAttribute(ref skinLayer.States.Disabled.Overlay, inherent, layerNode.Element("States").Element("Disabled"), "Overlay", dv, false);
            }

            #endregion

            #region Overlays

            if (layerNode.Element("Overlays") != null)
            {
                ReadAttribute(ref skinLayer.Overlays.Enabled.Index, inherent, layerNode.Element("Overlays").Element("Enabled"), "Index", 0, false);
                int di = skinLayer.Overlays.Enabled.Index;
                ReadAttribute(ref skinLayer.Overlays.Hovered.Index, inherent, layerNode.Element("Overlays").Element("Hovered"), "Index", di, false);
                ReadAttribute(ref skinLayer.Overlays.Pressed.Index, inherent, layerNode.Element("Overlays").Element("Pressed"), "Index", di, false);
                ReadAttribute(ref skinLayer.Overlays.Focused.Index, inherent, layerNode.Element("Overlays").Element("Focused"), "Index", di, false);
                ReadAttribute(ref skinLayer.Overlays.Disabled.Index, inherent, layerNode.Element("Overlays").Element("Disabled"), "Index", di, false);

                ReadAttribute(ref skinLayer.Overlays.Enabled.Color, inherent, layerNode.Element("Overlays").Element("Enabled"), "Color", Color.White, false);
                Color dc = skinLayer.Overlays.Enabled.Color;
                ReadAttribute(ref skinLayer.Overlays.Hovered.Color, inherent, layerNode.Element("Overlays").Element("Hovered"), "Color", dc, false);
                ReadAttribute(ref skinLayer.Overlays.Pressed.Color, inherent, layerNode.Element("Overlays").Element("Pressed"), "Color", dc, false);
                ReadAttribute(ref skinLayer.Overlays.Focused.Color, inherent, layerNode.Element("Overlays").Element("Focused"), "Color", dc, false);
                ReadAttribute(ref skinLayer.Overlays.Disabled.Color, inherent, layerNode.Element("Overlays").Element("Disabled"), "Color", dc, false);
            }

            #endregion

            #region Text

            if (layerNode.Element("Text") != null)
            {
                ReadAttribute(ref skinLayer.Text.Name, inherent, layerNode.Element("Text"), "Font", null, true);
                ReadAttribute(ref skinLayer.Text.OffsetX, inherent,layerNode.Element("Text"), "OffsetX", 0, false);
                ReadAttribute(ref skinLayer.Text.OffsetY, inherent, layerNode.Element("Text"), "OffsetY", 0, false);

                layerAlignment = skinLayer.Text.Alignment.ToString();
                ReadAttribute(ref layerAlignment, inherent, layerNode.Element("Text"), "Alignment", "MiddleCenter", false);
                skinLayer.Text.Alignment = (Alignment)Enum.Parse(typeof(Alignment), layerAlignment, true);

                LoadColors(inherent, layerNode.Element("Text"), ref skinLayer.Text.Colors);
            }

            #endregion

            #region Attributes

            if (layerNode.Element("Attributes") != null)
            {
                foreach (var attribute in layerNode.Element("Attributes").Elements())
                {
                    if (attribute.Name == "Attribute")
                    {
                        LoadLayerAttribute(skinLayer, attribute);
                    }
                }
            }

            #endregion

            if (!inherent)
                skinControl.Layers.Add(skinLayer);
        } // LoadLayer

        #region Load Colors

        private static void LoadColors(bool inherited, XElement e, ref SkinStates<Color> colors)
        {
            if (e != null)
            {
                ReadAttribute(ref colors.Enabled,  inherited, e.Element("Colors").Element("Enabled"),  "Color", Color.White,    false);
                ReadAttribute(ref colors.Hovered,  inherited, e.Element("Colors").Element("Hovered"),  "Color", colors.Enabled, false);
                ReadAttribute(ref colors.Pressed,  inherited, e.Element("Colors").Element("Pressed"),  "Color", colors.Enabled, false);
                ReadAttribute(ref colors.Focused,  inherited, e.Element("Colors").Element("Focused"),  "Color", colors.Enabled, false);
                ReadAttribute(ref colors.Disabled, inherited, e.Element("Colors").Element("Disabled"), "Color", colors.Enabled, false);
            }
        } // LoadColors

        #endregion

        #region Load Layer Attributes

        /// <summary>
        /// Load Layer Attributes
        /// </summary>
        private static void LoadLayerAttribute(SkinLayer skinLayer, XElement e)
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

        } // LoadLayerAttribute

        #endregion

        #endregion

        #region Load Fonts

        /// <summary>
        /// Load fonts information.
        /// </summary>
        private static void LoadFontsDescription()
        {
            if (skinDescription.Resource.Element("Skin").Element("Fonts") == null)
                return;

            foreach (var font in skinDescription.Resource.Element("Skin").Element("Fonts").Elements())
            {
                SkinFont skinFont = new SkinFont
                {
                    Name = ReadAttribute(font, "Name", null, true),
                    Filename = ReadAttribute(font, "Asset", null, true)
                };
                Fonts.Add(skinFont);
            }
        } // LoadFonts

        #endregion

        #region Load Cursors

        #if (WINDOWS)
            /// <summary>
            /// Load cursors information.
            /// </summary>
            private static void LoadCursorsDescription()
            {
                if (skinDescription.Resource.Element("Skin").Element("Cursors") == null)
                    return;

                foreach (var cursor in skinDescription.Resource.Element("Skin").Element("Cursors").Elements())
                {
                    SkinCursor skinCursor = new SkinCursor
                    {
                        Name = ReadAttribute(cursor, "Name", null, true),
                        Filename = ReadAttribute(cursor, "Asset", null, true)
                    };
                    Cursors.Add(skinCursor);
                }
            } // LoadCursors
        #endif

        #endregion

        #region Load Images

        /// <summary>
        /// Load images information.
        /// </summary>
        private static void LoadImagesDescription()
        {
            if (skinDescription.Resource.Element("Skin").Element("Images") == null)
                return;

            foreach (var image in skinDescription.Resource.Element("Skin").Element("Images").Elements())
            {
                SkinImage skinImage = new SkinImage
                {
                    Name = ReadAttribute(image, "Name", null, true),
                    Filename = ReadAttribute(image, "Asset", null, true)
                };
                Images.Add(skinImage);
            }
        } // LoadImages

        #endregion

        #region Read Attribute

        private static string ReadAttribute(XElement element, string attributeName, string defval, bool needed)
        {
            if (element != null && element.Attribute(attributeName) != null)
            {
                return element.Attribute(attributeName).Value;
            }
            if (needed)
            {
                throw new Exception("Missing required attribute \"" + attributeName + "\" in the skin file.");
            }
            return defval;
        } // ReadAttribute

        private static void ReadAttribute(ref string retval, bool inherited, XElement element, string attributeName, string defaultValue, bool needed)
        {
            if (element != null && element.Attribute(attributeName) != null)
            {
                retval = element.Attribute(attributeName).Value;
            }
            else if (inherited)
            {
                // Do nothing, the parent has the attribute.
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

        private static void ReadAttribute(ref int retval, bool inherited, XElement element, string attrib, int defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = int.Parse(tmp);
        } // ReadAttributeInt

        private static bool ReadAttribute(XElement element, string attrib, bool defval, bool needed)
        {
            return bool.Parse(ReadAttribute(element, attrib, defval.ToString(), needed));
        } // ReadAttributeBool

        private static void ReadAttribute(ref bool retval, bool inherited, XElement element, string attrib, bool defval, bool needed)
        {
            string tmp = retval.ToString();
            ReadAttribute(ref tmp, inherited, element, attrib, defval.ToString(), needed);
            retval = bool.Parse(tmp);
        } // ReadAttributeBool

        private static string ColorToString(Color c)
        {
            return string.Format("{0};{1};{2};{3}", c.R, c.G, c.B, c.A);
        } // ColorToString

        private static void ReadAttribute(ref Color retval, bool inherited, XElement element, string attrib, Color defval, bool needed)
        {
            string tmp = ColorToString(retval);
            ReadAttribute(ref tmp, inherited, element, attrib, ColorToString(defval), needed);
            retval = Utilities.ParseColor(tmp);
        } // ReadAttributeColor

        #endregion

    } // Skin

    #endregion

} // XNAFinalEngine.UserInterface
