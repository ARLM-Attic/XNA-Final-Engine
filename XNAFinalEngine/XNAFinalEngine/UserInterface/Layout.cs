
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
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.UI
{

    /// <summary>
    /// Layout loader.
    /// We can load a complete UI layout from a XML file.
    /// </summary>
    public static class Layout
    {

        #region Load

        /// <summary>
        /// Load layout file.
        /// </summary>
        public static Container Load(string layoutFilename)
        {
            Container mainContainer = null;
            LayoutXmlDocument doc;
            ContentManager content = new ContentManager(EngineManager.Services);

            try
            {
                string fullFilename = Directories.UILayoutDirectory + "\\" + layoutFilename;

                if (File.Exists(fullFilename + ".xnb") == false)
                {
                    throw new Exception("Failed to load layout: File " + fullFilename + " does not exists!");
                } // if (File.Exists)

                try
                {
                    doc = content.Load<LayoutXmlDocument>(fullFilename);
                }
                catch
                {
                    throw new Exception("Failed to load layout: " + layoutFilename);
                }

                if (doc["Layout"]["Controls"] != null && doc["Layout"]["Controls"].HasChildNodes)
                {
                    XmlNode node = doc["Layout"]["Controls"].GetElementsByTagName("Control").Item(0);
                    string className = node.Attributes["Class"].Value;
                    Type type = Type.GetType("XNAFinalEngine.UI." + className);
                    if (type == null)
                    {
                        throw new Exception("Failed to load layout: Control doesn't exist");
                    }
                    mainContainer = (Container)LoadControl(node, type, null);
                }
            }
            finally
            {
                content.Dispose();
            }

            return mainContainer;
        } // Load

        #endregion

        #region Load Control

        /// <summary>
        /// Load a control from the xml layout file.
        /// </summary>
        private static Control LoadControl(XmlNode node, Type type, Control parent)
        {
            // Create control instance (it doesn't know the type in compiler time)
            Control control = (Control)Activator.CreateInstance(type);
            
            if (parent != null)
                control.Parent = parent;
            
            control.Name = node.Attributes["Name"].Value;
            // Load control's parameters
            if (node["Properties"] != null && node["Properties"].HasChildNodes)
            {
                LoadProperties(node["Properties"].GetElementsByTagName("Property"), control);
            }
            // Load control's childrens
            if (node["Controls"] != null && node["Controls"].HasChildNodes)
            {
                foreach (XmlElement xmlElement in node["Controls"].GetElementsByTagName("Control"))
                {
                    string className = xmlElement.Attributes["Class"].Value;
                    Type typeNewControl = Type.GetType("XNAFinalEngine.UI." + className);
                    if (typeNewControl == null)
                    {
                        throw new Exception("Failed to load layout: Control doesn't exist");
                    }
                    LoadControl(xmlElement, typeNewControl, control);
                }
            }
            return control;
        } // LoadControl

        /// <summary>
        /// Load the properties of a control.
        /// </summary>
        private static void LoadProperties(XmlNodeList node, Control control)
        {
            foreach (XmlElement e in node)
            {
                string name = e.Attributes["Name"].Value;
                string val = e.Attributes["Value"].Value;

                PropertyInfo i = control.GetType().GetProperty(name);

                if (i == null)
                {
                    throw new Exception(string.Format("Failed to load layout: Parameter name {0} doesn't exist in control {1}", name, control.GetType().FullName));
                }
                try
                {
                    i.SetValue(control, Convert.ChangeType(val, i.PropertyType, null), null);
                }
                catch
                {
                    throw new Exception(string.Format("Failed to load layout: Parameter name {0} doesn't has a correct value", name));
                }
            }
        } // LoadProperties

        #endregion

    } // Layout
} // XNAFinalEngine.UI