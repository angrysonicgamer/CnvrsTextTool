using System.Text;

namespace CnvrsTextTool
{
    public static class Extensions
    {
        // BinaryReader
        
        public static void SetPosition(this BinaryReader reader, long position)
        {
            reader.BaseStream.Position = position;
        }
        
        public static T ReadAt<T>(this BinaryReader reader, long position, Func<BinaryReader, T> func)
        {
            var origPosition = reader.BaseStream.Position;

            if (origPosition != position)
            {
                reader.SetPosition(position);
            }

            T value;

            try
            {
                value = func(reader);
            }
            finally
            {
                reader.SetPosition(origPosition);
            }

            return value;
        }

        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            var textbytes = new List<byte>();

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
            var textbytes = new List<byte>();

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

        public static void SetPosition(this BinaryWriter writer, long position)
        {
            writer.BaseStream.Position = position;
        }

        public static void WriteAt(this BinaryWriter writer, long position, Action<BinaryWriter> func)
        {
            var origPosition = writer.BaseStream.Position;

            if (origPosition != position)
            {
                writer.SetPosition(position);
            }

            try
            {
                func(writer);
            }
            finally
            {
                writer.SetPosition(origPosition);
            }
        }
    }
}
