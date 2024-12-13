namespace CnvrsTextTool
{
    public static class DisplayMessage
    {
        public static void AboutTool()
        {
            string about = "This is a tool to work with text stored in cnvrs-text files (Sonic Frontiers, Shadow Generations, etc.)\n\n" +
                "- Usage -\nDrag and drop a cnvrs-text file to the executable to extract text to a json file.\n" +
                "The only important data in the created json is text strings. Everything else is added mainly for context.\n" +
                "Edit text in that json and then drag and drop it to the executable\n" +
                "to modify the corresponding cnvrs-text file data (it must also be in the same folder).\n" +
                "The modified file will be saved in the \"New files\" folder.\n\n" +
                "- CMD usage -\nCnvrsTextTool filename\n\n" +
                "The tool appends new text to the end of the source (cnvrs-text) file\n" +
                "and rewrites text pointers, making its size slightly bigger.\n" +
                "It doesn't modify any other data so the speaker info is preserved.\n\n" +                
                "This solution is basically a proof of concept.\n" +
                "But it allows text modding without losing speaker names in Shadow Generations\n" +
                "or losing static event animations in Sonic Frontiers.\n";
            Console.WriteLine(about);
            Wait();
        }

        public static void TooManyArguments()
        {
            Console.WriteLine("Too many arguments.\n");
        }

        public static void FileNotFound(string file)
        {
            Console.WriteLine($"File {file} not found.\n");
            Wait();
        }

        public static void CorrespondingFileNotFound(string file)
        {
            Console.WriteLine($"Corresponding file {file} not found.\n");
            Wait();
        }

        public static void WrongExtension()
        {
            Console.WriteLine("The file extension is not supported.\n");
            Wait();
        }

        public static void FileModified(string file)
        {
            Console.WriteLine($"File {file} has been successfully modified and saved in the \"New files\" directory!\n");
            Wait();
        }

        public static void TextExtracted(string file)
        {
            Console.WriteLine($"Text has been extracted to {file}!\n");
            Wait();
        }

        private static void Wait()
        {
            Console.WriteLine("Press Enter to exit");
            while (true)
            {
                var keyPressed = Console.ReadKey(true).Key;
                if (keyPressed == ConsoleKey.Enter) break;
            }
        }
    }
}
