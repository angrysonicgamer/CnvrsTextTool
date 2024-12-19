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

        public void Read(BinaryReader reader)
        {
            long entryNameHash = reader.ReadInt64();                                            // a hash number calculated from entry name (will ignore)
            long entryNameOffset = reader.ReadInt64() + Pointer.OffsetDifference;
            long fontAndLayoutInfoOffset = reader.ReadInt64();                                  // contains another offset for entry name and (in Puyo Puyo and Sonic Frontiers) font and layout info (null in Shadow Generations)
            long textOffset = reader.ReadInt64() + Pointer.OffsetDifference;
            long textLength = reader.ReadInt64();
            long speakerInfoPtr = reader.ReadInt64();                                           // Puyo Text Editor ignores this; might be null (for everything but subtitles)

            Name = reader.ReadAt(entryNameOffset, x => x.ReadCString(Encoding.UTF8));
            string rawText = reader.ReadAt(textOffset, x => x.ReadUnicodeString(textLength));
            Text = TextAttributesHandler.GetPresentableString(rawText);                         // handle attributes in more presentable way

            if (speakerInfoPtr != 0)
            {
                reader.SetPosition(speakerInfoPtr + Pointer.OffsetDifference);

                long value1 = reader.ReadInt64();                                               // always 1 (?)
                long ptr1 = reader.ReadInt64();                                                 // points to ptr2
                long ptr2 = reader.ReadInt64();                                                 // points to "Speaker" signature pointer
                long speakerSignaturePtr = reader.ReadInt64();                                  // points to "Speaker" signature
                long value2 = reader.ReadInt64();                                               // always 3 (?)
                long speakerOffset = reader.ReadInt64() + Pointer.OffsetDifference;             // points to actual speaker name

                Speaker = reader.ReadAt(speakerOffset, x => x.ReadCString(Encoding.UTF8));
            }
        }

        public void WriteText(BinaryWriter writer)
        {
            string rawText = TextAttributesHandler.GetRawString(Text);
            long textPosition = writer.BaseStream.Length;
            
            writer.Write(textPosition - Pointer.OffsetDifference);
            writer.Write((long)rawText.Length);

            writer.SetPosition(textPosition);
            writer.Write(Encoding.Unicode.GetBytes(rawText));
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
