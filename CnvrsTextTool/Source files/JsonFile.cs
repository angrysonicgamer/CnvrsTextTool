using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CnvrsTextTool
{
    public static class JsonFile
    {
        public static JsonContents Read(string filename)
        {
            var json = JsonNode.Parse(File.ReadAllText(filename));
            return JsonSerializer.Deserialize<JsonContents>(json);
        }
        
        public static void Create(JsonContents fileContents)
        {
            string jsonFile = $"{fileContents.Filename}.json";

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            string json = JsonSerializer.Serialize(fileContents, options);
            File.WriteAllText(jsonFile, json);
            DisplayMessage.TextExtracted(jsonFile);
        }
    }
}
