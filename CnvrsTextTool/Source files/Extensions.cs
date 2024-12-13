using System.Text;

namespace CnvrsTextTool
{
    public static class Extensions
    {
        // BinaryReader
        
        public static T ReadAt<T>(this BinaryReader reader, long position, Func<BinaryReader, T> func)
        {
            var origPosition = reader.BaseStream.Position;

            if (origPosition != position)
            {
                reader.BaseStream.Position = position;
            }

            T value;

            try
            {
                value = func(reader);
            }
            finally
            {
                reader.BaseStream.Position = origPosition;
            }

            return value;
        }

        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            List<byte> textbytes = new List<byte>();

            while (true)
            {
                byte b = reader.ReadByte();
                if (b == 0) break;

                textbytes.Add(b);
            }

            return encoding.GetString(textbytes.ToArray());
        }
        
        public static string ReadUnicodeString(this BinaryReader reader, long count)
        {
            List<byte> textbytes = new List<byte>();

            for (int i = 0; i < count; i++)
            {
                byte a = reader.ReadByte();
                byte b = reader.ReadByte();
                textbytes.Add(a);
                textbytes.Add(b);
            }

            return Encoding.Unicode.GetString(textbytes.ToArray());
        }


        // BinaryWriter

        public static void WriteAt(this BinaryWriter writer, long position, Action<BinaryWriter> func)
        {
            var origPosition = writer.BaseStream.Position;

            if (origPosition != position)
            {
                writer.BaseStream.Position = position;
            }

            try
            {
                func(writer);
            }
            finally
            {
                writer.BaseStream.Position = origPosition;
            }
        }
    }
}
