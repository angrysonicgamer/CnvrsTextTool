using System.Text;

namespace CnvrsTextTool
{
    public static class CnvrsTextFile
    {
        private static readonly long fileSizePosition = 0x8;
        private static readonly long dataSegmentLengthPosition = 0x14;
        private static readonly long entriesCountPosition = 0x42;
        private static readonly long languagePtrPosition = 0x50;
        private static readonly long entriesBeginPosition = 0x60;

        public static JsonContents ReadText(string sourceFile)
        {
            var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(sourceFile)));

            string filename = Path.GetFileName(sourceFile);
            var entries = new List<CnvrsTextEntry>();

            short entriesCount = reader.ReadAt(entriesCountPosition, x => x.ReadInt16());
            long languageOffset = reader.ReadAt(languagePtrPosition, x => x.ReadInt64()) + Pointer.OffsetDifference;
            string language = reader.ReadAt(languageOffset, x => x.ReadCString(Encoding.UTF8));

            for (int i = 0; i < entriesCount; i++)
            {
                reader.SetPosition(entriesBeginPosition + i * CnvrsTextEntry.Size);
                var entry = new CnvrsTextEntry();
                entry.Read(reader);
                entries.Add(entry);
            }

            reader.Dispose();
            return new JsonContents(filename, language, entries);            
        }

        public static void Write(JsonContents data)
        {
            if (!File.Exists(data.Filename))
            {
                DisplayMessage.CorrespondingFileNotFound(data.Filename);
                return;
            }

            string destinationFolder = "New files";
            string outputFile = $"{destinationFolder}\\{data.Filename}";
            Directory.CreateDirectory(destinationFolder);

            var source = File.ReadAllBytes(data.Filename);
            var reader = new BinaryReader(new MemoryStream(source));

            int fileSizeExcludingNewText = reader.ReadAt(dataSegmentLengthPosition, x => x.ReadInt32()) + 0x10;         // DATA segment takes an entire file except for the first 16 bytes (BINA210L segment)
            var sourceWithoutNewText = source.ToList();
            sourceWithoutNewText.RemoveRange(fileSizeExcludingNewText, source.Length - fileSizeExcludingNewText);       // removing NEWTEXT segment if a file already contains one
           
            var writer = new BinaryWriter(File.Create(outputFile));
            writer.Write(sourceWithoutNewText.ToArray());
            writer.Write(new byte[(16 - sourceWithoutNewText.Count % 16) % 16]);                                        // the NEWTEXT segment will begin at nearest offset that is multiple of 16 (0x10)
            writer.Write(Encoding.UTF8.GetBytes("NEWTEXT"));
            writer.Write((byte)0);

            int entryNum = 0;            
            foreach (var entry in data.TextEntries)
            {
                writer.SetPosition(entriesBeginPosition + entryNum * CnvrsTextEntry.Size + sizeof(long) * 3);
                entry.WriteText(writer);
                entryNum++;
            }

            writer.WriteAt(fileSizePosition, x => x.Write((int)writer.BaseStream.Length));

            reader.Dispose();
            writer.Dispose();
            DisplayMessage.FileSaved(data.Filename);
        }
    }
}
