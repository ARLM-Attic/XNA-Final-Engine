
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngine.UI
{

    public class LayoutXmlDocument : XmlDocument { }
    public class SkinXmlDocument : XmlDocument { }

    #region Skin Reader

    /// <summary>
    /// Skin reader.
    /// </summary>
    public class SkinReader : ContentTypeReader<SkinXmlDocument>
    {

        protected override SkinXmlDocument Read(ContentReader input, SkinXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                SkinXmlDocument doc = new SkinXmlDocument();
                doc.LoadXml(input.ReadString());
                return doc;
            }
            else
            {
                existingInstance.LoadXml(input.ReadString());
            }

            return existingInstance;
        } // Read

    } // SkinReader

    #endregion

    #region Layout Reader

    /// <summary>
    /// UI layout reader.
    /// </summary>
    public class LayoutReader : ContentTypeReader<LayoutXmlDocument>
    {
        
        protected override LayoutXmlDocument Read(ContentReader input, LayoutXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                LayoutXmlDocument doc = new LayoutXmlDocument();
                doc.LoadXml(input.ReadString());
                return doc;
            }
            else
            {
                existingInstance.LoadXml(input.ReadString());
            }

            return existingInstance;
        } // Read

    } // LayoutReader

    #endregion

    #region Cursor Reader

    /// <summary>
    /// Cursor reader.
    /// </summary>
    public class CursorReader : ContentTypeReader<Cursor>
    {

        protected override Cursor Read(ContentReader input, Cursor existingInstance)
        {
            if (existingInstance == null)
            {
                int count = input.ReadInt32();
                byte[] data = input.ReadBytes(count);

                string path = Path.GetTempFileName();
                File.WriteAllBytes(path, data);

                IntPtr handle = LoadCursor(path);
                Cursor cur = new Cursor(handle);
                File.Delete(path);

                return cur;
            }

            return existingInstance;
        } // Read

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImage(IntPtr instance, string fileName, uint type, int width, int height, uint load);

        private static IntPtr LoadCursor(string fileName)
        {
            return LoadImage(IntPtr.Zero, fileName, 2, 0, 0, 0x0010);
        } // LoadCursor

    } // CursorReader

    #endregion

} // XNAFinalEngine.UI

