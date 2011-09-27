
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
using System.Reflection;
using System.Xml.Linq;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Layout loader.
    /// Load a complete UI layout from a XML file.
    /// </summary>
    public static class Layout
    {

        #region Load

        /// <summary>
        /// Load layout file.
        /// </summary>
        public static Container Load(string filename)
        {
            Container mainContainer = null;
            ContentManager temporalContent = new ContentManager();
            ContentManager.CurrentContentManager = temporalContent;
            try
            {
                Document layoutDocument = new Document("Layout\\" + filename);
                try
                {
                    if (layoutDocument.XDocument.Element("Layout").Element("Controls") != null)
                    {
                        foreach (var control in layoutDocument.XDocument.Element("Layout").Element("Controls").Elements())
                        {
                            string className = control.Attribute("Class").Value;
                            Type type = Type.GetType("XNAFinalEngine.UserInterface." + className);
                            if (type == null)
                            {
                                throw new Exception("Failed to load layout: Control doesn't exist");
                            }
                            mainContainer = (Container)LoadControl(control, type, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Failed to load layout: " + filename + ".\nThere are probably syntax errors.", e);
                }
            }
            finally
            {
                temporalContent.Dispose();
            }

            return mainContainer;
        } // Load

        #endregion

        #region Load Control

        /// <summary>
        /// Load a control from the xml layout file.
        /// </summary>
        private static Control LoadControl(XElement element, Type type, Control parent)
        {
            // Create control instance (it doesn't know the type in compiler time)
            Control control = (Control)Activator.CreateInstance(type);
            
            if (parent != null)
                control.Parent = parent;

            control.Name = element.Attribute("Name").Value;
            // Load control's parameters
            if (element.Element("Properties") != null)
            {
                foreach (var property in element.Element("Properties").Elements())
                {
                    LoadProperty(property, control);
                }
            }
            // Load control's childrens
            if (element.Element("Controls") != null)
            {
                foreach (var subControl in element.Element("Controls").Elements())
                {
                    string className = subControl.Attribute("Class").Value;
                    Type typeNewControl = Type.GetType("XNAFinalEngine.UserInterface." + className);
                    if (typeNewControl == null)
                    {
                        throw new Exception("Failed to load layout: Control doesn't exist");
                    }
                    LoadControl(subControl, typeNewControl, control);
                }
            }

            return control;
        } // LoadControl

        #endregion

        #region Load Property

        /// <summary>
        /// Load the properties of a control.
        /// </summary>
        private static void LoadProperty(XElement property, Control control)
        {
            string name = property.Attribute("Name").Value;
            string val = property.Attribute("Value").Value;

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
        } // LoadProperties

        #endregion

    } // Layout
} // XNAFinalEngine.UserInterface