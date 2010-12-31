
#region Using directives
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace XNAFinalEngine.SkinImporter
{

    #region Importer

    public class LayoutXmlDocument : XmlDocument { }

    [ContentImporter(".xml", DisplayName = "Layout - XNA Final Engine")]
    class LayoutImporter : ContentImporter<LayoutXmlDocument>
    {

        public override LayoutXmlDocument Import(string filename, ContentImporterContext context)
        {
            LayoutXmlDocument doc = new LayoutXmlDocument();
            doc.Load(filename);

            return doc;
        } // Import

    } // LayoutImporter

    #endregion

    #region Writer

    [ContentTypeWriter]
    class LayoutWriter : ContentTypeWriter<LayoutXmlDocument>
    {

        protected override void Write(ContentWriter output, LayoutXmlDocument value)
        {
            output.Write(value.InnerXml);
        } // Writer

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(LayoutXmlDocument).AssemblyQualifiedName;
        } // GetRuntimeType
  
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "XNAFinalEngine.UI.LayoutReader, XNAFinalEngine";
        } // GetRuntimeReader

    } // LayoutWriter

    #endregion

} // XNAFinalEngine.SkinImporter