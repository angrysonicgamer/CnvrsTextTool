using System.Text.Json.Serialization;

namespace CnvrsTextTool
{
    public class CnvrsTextEntry
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string? Speaker { get; set; }
        public string Text { get; set; }
        public static int Size => 48;

        [JsonConstructor]
        public CnvrsTextEntry() { }

        public CnvrsTextEntry(long id, string name, string? speaker, string text)
        {
            ID = id;
            Name = name;
            Speaker = speaker;
            Text = text;
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
