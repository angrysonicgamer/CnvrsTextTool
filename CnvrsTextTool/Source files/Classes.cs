using System.Text;
using System.Text.Json.Serialization;

namespace CnvrsTextTool
{
    public static class Pointer
    {
        public static int OffsetDifference => 64; // real offset = pointer + this
    }
    
    
    public class CnvrsTextEntry
    {
        public string Name { get; set; }
        public string? Speaker { get; set; }
        public string Text { get; set; }
        public static int Size => sizeof(long) * 6;

        [JsonConstructor]
        public CnvrsTextEntry() { }

        public CnvrsTextEntry(string name, string? speaker, string text)
        {
            Name = name;
            Speaker = speaker;
            Text = text;
        }


        public void Read(BinaryReader reader)
        {
            long entryNameHash = reader.ReadInt64();                                            // a hash number calculated from entry name (will ignore)
            long entryNameOffset = reader.ReadInt64() + Pointer.OffsetDifference;
            long otherInfoOffset = reader.ReadInt64();                                          // contains another offset for entry name and (in Puyo Puyo and Sonic Frontiers) font and layout info (null in Shadow Generations)
            long textOffset = reader.ReadInt64() + Pointer.OffsetDifference;
            long textLength = reader.ReadInt64();
            long puyoTextEditorIgnoresThisOffset = reader.ReadInt64();                          // Shadow Generations have speaker info and likely something else, might be null (for everything but subtitles)

            Name = reader.ReadAt(entryNameOffset, x => x.ReadCString(Encoding.UTF8));
            string rawText = reader.ReadAt(textOffset, x => x.ReadUnicodeString(textLength));
            Text = TextAttributesHandler.GetPresentableString(rawText);                         // handle attributes in more presentable way

            if (puyoTextEditorIgnoresThisOffset != 0)
            {
                reader.BaseStream.Position = puyoTextEditorIgnoresThisOffset + Pointer.OffsetDifference;

                long value1 = reader.ReadInt64();                                               // always 1 (?)
                long offset1 = reader.ReadInt64() + Pointer.OffsetDifference;                   // points to offset2
                long offset2 = reader.ReadInt64() + Pointer.OffsetDifference;                   // points to speakerOffset
                long speakerSignatureOffset = reader.ReadInt64() + Pointer.OffsetDifference;    // points to "Speaker" signature
                long value2 = reader.ReadInt64();                                               // always 3 (?)
                long speakerOffset = reader.ReadInt64() + Pointer.OffsetDifference;             // points to actual speaker code

                Speaker = reader.ReadAt(speakerOffset, x => x.ReadCString(Encoding.UTF8));
            }
        }
    }

    public class JsonContents
    {
        public string Filename { get; set; }
        public string Language { get; set; }
        public List<CnvrsTextEntry> TextEntries { get; set; }

        [JsonConstructor]
        public JsonContents() { }

        public JsonContents(string filename, string language, List<CnvrsTextEntry> textEntries)
        {
            Filename = filename;
            Language = language;
            TextEntries = textEntries;
        }
    }
}
