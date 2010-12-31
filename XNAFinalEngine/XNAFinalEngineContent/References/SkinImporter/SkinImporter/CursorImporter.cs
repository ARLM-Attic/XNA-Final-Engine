
#region Using directives
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.IO;
using System;
#endregion

namespace XNAFinalEngine.SkinImporter
{

    #region Importer

    public class CursorFile
    {
        public byte[] Data = null;
    } // CursorFile

    [ContentImporter(".cur", DisplayName = "Cursor - XNA Final Engine")]
    class CursorImporter : ContentImporter<CursorFile>
    {
        public override CursorFile Import(string filename, ContentImporterContext context)
        {
            CursorFile cur = new CursorFile();
            cur.Data = File.ReadAllBytes(filename);
            return cur;
        } // Import
    } // CursorImporter

    #endregion

    #region Writer

    [ContentTypeWriter]
    class CursorWriter : ContentTypeWriter<CursorFile>
    {

        protected override void Write(ContentWriter output, CursorFile value)
        {
            output.Write(value.Data.Length);
            output.Write(value.Data);
        } // Write

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(CursorFile).AssemblyQualifiedName;
        } // GetRuntimeType

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "XNAFinalEngine.UI.CursorReader, XNAFinalEngine";
        } // GetRuntimeReader
    }

    #endregion

} // XNAFinalEngine.SkinImporter