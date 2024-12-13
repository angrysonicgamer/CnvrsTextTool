using System.Globalization;
using System.Text;

namespace CnvrsTextTool
{
    public static class TextAttributesHandler
    {
        private static readonly string colorAttribute = "<color:";
        private static readonly string colorEndTag = "</color>";
        private static readonly string varAttribute = "<var:";
        private static readonly string imageAttribute = "<image:";
        private static readonly string endTag = " />";


        public static string GetPresentableString(string rawText)
        {
            var builder = new StringBuilder();
            
            for (int i = 0; i < rawText.Length; i++)
            {
                char c = rawText[i];
                
                // Raw text example with multiple attributes: \uE1E0\uFFC8\uC8E6SpeakerColor\u0000\uE141caption_1\u0000something\uE010   \uE141caption_2\u0000
                // Not very readable, right?
                // The following code is basically taken from Puyo Text Editor source code but without xml stuff
                // I'm just creating more presentable strings for json using kinda xml-like style

                switch (c & 0xF00F)
                {
                    case 0xE000: // color
                        if (c == 0xE010) // color end tag
                        {
                            builder.Append(colorEndTag);
                        }
                        else
                        {
                            i++;
                            uint argbColor = (uint)(rawText[i] << 16 | (rawText[i + 1]));
                            i += 2;
                            int colorNameLength = GetNameLength(c) - 2;
                            string colorName = new string(rawText.ToCharArray(), i, colorNameLength);
                            builder.Append($"{colorAttribute}{colorName}|{argbColor.ToString("X8")}>");     // example with color end tag: <color:SpeakerColor|FFC8C8E6>something</color>
                            i += colorNameLength;
                        }
                        break;

                    case 0xE001: // variable
                        i++;
                        int varNameLength = GetNameLength(c);
                        string varName = new string(rawText.ToCharArray(), i, varNameLength);
                        builder.Append($"{varAttribute}{varName}{endTag}");                                 // example: <var:caption_1 />
                        i += varNameLength;
                        break;

                    case 0xE005: // image
                        i++;
                        int imageNameLength = GetNameLength(c);
                        string imageName = new string(rawText.ToCharArray(), i, imageNameLength);
                        builder.Append($"{imageAttribute}{imageName}{endTag}");                             // example: <image:button_playerboost />
                        i += imageNameLength;
                        break;

                    default:
                        builder.Append(c);
                        break;
                }                
            }

            // And so, that raw string becomes this: <color:SpeakerColor|FFC8C8E6><var:caption_1 />something</color>   <var:caption_2 />
            // Much easier to read

            return builder.ToString();
        }

        public static string GetRawString(string presentableText)
        {                      
            string text = presentableText;
            
            while (text.Contains(colorAttribute))
            {
                int begin = text.IndexOf(colorAttribute);
                int end = text.IndexOf(colorEndTag) + colorEndTag.Length;
                string textWithColor = text.Substring(begin, end - begin);                                                      // example: <color:SpeakerColor|FFC8C8E6>something</color>
                string firstConversion = textWithColor.Replace(colorAttribute, "").Replace(colorEndTag, "\uE010");              // example: SpeakerColor|FFC8C8E6>something\uE010
                int separatorIndex = firstConversion.IndexOf("|");
                string colorName = firstConversion.Substring(0, separatorIndex);                                                // example: SpeakerColor
                ushort colorByte1 = ushort.Parse(firstConversion.Substring(separatorIndex + 1, 4), NumberStyles.HexNumber);     // example: FFC8
                ushort colorByte2 = ushort.Parse(firstConversion.Substring(separatorIndex + 5, 4), NumberStyles.HexNumber);     // example: C8E6
                string colorParamsOnly = firstConversion.Substring(0, firstConversion.IndexOf(">") + 1);                        // example: SpeakerColor|FFC8C8E6>
                char colorTagValue = (char)(0xE000 | SetNameLength(colorName.Length + 2));
                string raw = $"{colorTagValue}{(char)colorByte1}{(char)colorByte2}{colorName}\u0000";                           // example: \uE1E0\uFFC8\uC8E6SpeakerColor\u0000
                firstConversion = firstConversion.Replace(colorParamsOnly, raw);
                text = text.Replace(textWithColor, firstConversion);                                                            // example: \uE1E0\uFFC8\uC8E6SpeakerColor\u0000something\uE010
            }

            while (text.Contains(varAttribute))
            {
                int begin = text.IndexOf(varAttribute);
                int end = text.IndexOf(endTag) + endTag.Length;
                string textWithVariable = text.Substring(begin, end - begin);                                                   // example: <var:caption_1 />
                string varName = textWithVariable.Replace(varAttribute, "").Replace(endTag, "");                                // example: caption_1
                char varTagValue = (char)(0xE001 | SetNameLength(varName.Length));
                string raw = $"{varTagValue}{varName}\u0000";                                                                   // example: \uE141caption_1\u0000
                text = text.Replace(textWithVariable, raw);
            }

            while (text.Contains(imageAttribute))
            {
                int begin = text.IndexOf(imageAttribute);
                int end = text.IndexOf(endTag) + endTag.Length;
                string textWithImage = text.Substring(begin, end - begin);                                                      // example: <image:button_playerboost />
                string imageName = textWithImage.Replace(imageAttribute, "").Replace(endTag, "");                               // example: button_playerboost
                char imageTagValue = (char)(0xE005 | SetNameLength(imageName.Length));
                string raw = $"{imageTagValue}{imageName}\u0000";                                                               // example: \uE265button_playerboost\u0000
                text = text.Replace(textWithImage, raw);
            }

            return text;
        }


        // These are taken straight from Puyo Text Editor source code, idk what this code means lmao
        private static ushort GetNameLength(ushort value) => (ushort)((((value & 0x0FF0) >> 4) / 2) - 1);
        private static ushort SetNameLength(int value) => (ushort)((((value + 1) * 2) << 4) & 0x0FF0);
    }
}
