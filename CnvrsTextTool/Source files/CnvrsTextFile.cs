using System.Text;

namespace CnvrsTextTool
{
    public static class CnvrsTextFile
    {
        private static readonly int fileSizePosition = 0x8;
        private static readonly int dataSegmentLengthPosition = 0x14;
        private static readonly int entriesCountOffsetPosition = 0x42;
        private static readonly int languageOffsetPosition = 0x50;
        private static readonly int entriesBeginPosition = 0x60;
        private static readonly int offsetDifference = 64; // real offset minus value            

        public static JsonContents ReadText(string sourceFile)
        {
            var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(sourceFile)));

            string filename = Path.GetFileName(sourceFile);
            var entries = new List<CnvrsTextEntry>();

            short entriesCount = reader.ReadAt(entriesCountOffsetPosition, x => x.ReadInt16());
            long languageOffset = reader.ReadAt(languageOffsetPosition, x => x.ReadInt64()) + offsetDifference;
            string language = reader.ReadAt(languageOffset, x => x.ReadCString(Encoding.UTF8));

            for (int i = 0; i < entriesCount; i++)
            {
                reader.BaseStream.Position = entriesBeginPosition + i * CnvrsTextEntry.Size;

                long entryId = reader.ReadInt64();
                long entryNameOffset = reader.ReadInt64() + offsetDifference;
                long otherInfoOffset = reader.ReadInt64();                                  // contains another offset for entry name and (in Puyo Puyo and Sonic Frontiers) font and layout info (null in Shadow Generations)
                long textOffset = reader.ReadInt64() + offsetDifference;
                long textLength = reader.ReadInt64();
                long puyoTextEditorIgnoresThisOffset = reader.ReadInt64();                  // Shadow Generations have speaker info and likely something else, might be null (for everything but subtitles)

                string entryName = reader.ReadAt(entryNameOffset, x => x.ReadCString(Encoding.UTF8));
                string rawText = reader.ReadAt(textOffset, x => x.ReadUnicodeString(textLength));
                string text = TextAttributesHandler.GetPresentableString(rawText);          // handle attributes in more presentable way
                string? speakerCode = null;

                if (puyoTextEditorIgnoresThisOffset != 0)
                {
                    reader.BaseStream.Position = puyoTextEditorIgnoresThisOffset + offsetDifference;

                    long value1 = reader.ReadInt64();                                       // always 1 (?)
                    long offset1 = reader.ReadInt64() + offsetDifference;                   // points to offset2
                    long offset2 = reader.ReadInt64() + offsetDifference;                   // points to speakerOffset
                    long speakerOffset = reader.ReadInt64() + offsetDifference;             // points to "Speaker" signature
                    long value2 = reader.ReadInt64();                                       // always 3 (?)
                    long speakerCodeOffset = reader.ReadInt64() + offsetDifference;         // points to actual speaker code

                    speakerCode = reader.ReadAt(speakerCodeOffset, x => x.ReadCString(Encoding.UTF8));
                }

                entries.Add(new CnvrsTextEntry(entryId, entryName, speakerCode, text));
            }

            reader.Dispose();

            return new JsonContents(filename, language, entries);            
        }

        public static void Write(JsonContents cnvrsData)
        {
            if (!File.Exists(cnvrsData.Filename))
            {
                DisplayMessage.CorrespondingFileNotFound(cnvrsData.Filename);
                return;
            }
            
            var source = File.ReadAllBytes(cnvrsData.Filename);
            var reader = new BinaryReader(new MemoryStream(source));
            string newText = "NEWTEXT\0";            

            int fileSizeExcludingNewText = reader.ReadAt(dataSegmentLengthPosition, x => x.ReadInt32()) + 0x10;     // DATA segment takes an entire file except for the first 16 bytes (BINA210L segment)
            var sourceToExpand = source.ToList();
            sourceToExpand.RemoveRange(fileSizeExcludingNewText, source.Length - fileSizeExcludingNewText);         // removing NEWTEXT segment if a file already contains one
            int extraBytes = 16 - sourceToExpand.Count % 16;                                                        // the NEWTEXT segment will begin at offset that is multiple of 16 (0x10)
            sourceToExpand.AddRange(new byte[extraBytes]);                                                          // these will be filled later
            sourceToExpand.AddRange(new byte[newText.Length]);
            sourceToExpand.AddRange(new byte[CalculateTotalTextLengthInBytes(cnvrsData)]);

            var newCnvrsFileContents = sourceToExpand.ToArray();
            var writer = new BinaryWriter(new MemoryStream(newCnvrsFileContents));

            writer.WriteAt(fileSizePosition, x => x.Write(newCnvrsFileContents.Length));
            writer.WriteAt(fileSizeExcludingNewText + extraBytes, x => x.Write(Encoding.UTF8.GetBytes(newText)));
            int newTextBeginPosition = fileSizeExcludingNewText + extraBytes + newText.Length;

            for (int i = 0; i < cnvrsData.TextEntries.Count; i++)
            {
                int textOffsetPosition = entriesBeginPosition + i * CnvrsTextEntry.Size + sizeof(long) * 3;
                int textLengthPosition = textOffsetPosition + 8;
                string rawText = TextAttributesHandler.GetRawString(cnvrsData.TextEntries[i].Text);

                writer.WriteAt(textOffsetPosition, x => x.Write((long)newTextBeginPosition - offsetDifference));
                writer.WriteAt(textLengthPosition, x => x.Write((long)rawText.Length));
                writer.WriteAt(newTextBeginPosition, x => x.Write(Encoding.Unicode.GetBytes(rawText)));

                newTextBeginPosition += (cnvrsData.TextEntries[i].Text.Length + 1) * 2;
            }

            reader.Dispose();
            writer.Dispose();

            string destinationFolder = "New files";
            string outputFile = $"{destinationFolder}\\{cnvrsData.Filename}";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes(outputFile, newCnvrsFileContents);
            DisplayMessage.FileModified(cnvrsData.Filename);
        }

        private static int CalculateTotalTextLengthInBytes(JsonContents cnvrsData)
        {
            int totalBytes = 0;

            foreach (var entry in cnvrsData.TextEntries)
            {
                totalBytes += (entry.Text.Length + 1) * 2;
            }

            return totalBytes;
        }
    }
}
