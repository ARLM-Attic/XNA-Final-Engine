
#region Using directives
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace XNAFinalEngine.SkinImporter
{

    #region Importer

    class SkinXmlDocument : XmlDocument { }

    [ContentImporter(".xml", DisplayName = "Skin - XNA Final Engine")]
    class SkinImporter : ContentImporter<SkinXmlDocument>
    {

        public override SkinXmlDocument Import(string filename, ContentImporterContext context)
        {
            SkinXmlDocument doc = new SkinXmlDocument();
            doc.Load(filename);

            return doc;
        } // Import

    } // SkinImporter

    #endregion

    #region Writer

    [ContentTypeWriter]
    class SkinWriter : ContentTypeWriter<SkinXmlDocument>
    {

        protected override void Write(ContentWriter output, SkinXmlDocument value)
        {
            output.Write(value.InnerXml);
        } // Write

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(SkinXmlDocument).AssemblyQualifiedName;
        } // GetRuntimeType

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "XNAFinalEngine.UI.SkinReader, XNAFinalEngine";
        } // GetRuntimeReader

    } // SkinWriter

    #endregion

} // XNAFinalEngine.SkinImporter