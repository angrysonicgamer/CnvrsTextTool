namespace CnvrsTextTool
{
    public static class Program
    {
        private static void SetAppTitle()
        {
            Console.Title = "Cnvrs-text tool";
        }


        public static void Main(string[] args)
        {
            SetAppTitle();            

            if (args.Length == 0)
            {
                DisplayMessage.AboutTool();
                return;
            }

            if (args.Length > 1)
            {
                DisplayMessage.TooManyArguments();
                DisplayMessage.AboutTool();
                return;
            }

            string sourceFile = args[0];
            string fileExtension = Path.GetExtension(sourceFile).ToLower();

            if (!File.Exists(sourceFile))
            {
                DisplayMessage.FileNotFound(sourceFile);
                return;
            }

            // Main actions

            if (fileExtension == ".cnvrs-text")
            {
                JsonFile.Create(CnvrsTextFile.ReadText(sourceFile));
            }
            else if (fileExtension == ".json")
            {
                CnvrsTextFile.Write(JsonFile.Read(sourceFile));
            }
            else
            {
                DisplayMessage.WrongExtension();
            }
        }
    }
}
