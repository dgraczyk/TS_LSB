using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace steganografia_LSB
{
    public class LSB
    {
        public static Bitmap Encode(string text, Bitmap bitmap)
        {
            var encode = BitHelper.GetBytes(text);
            int i = 0;
            int j = 8;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    var R = (j == 8) ? pixel.R & 254 : pixel.R & 254 | +BitHelper.GetBit(encode[i], --j);

                    var G = pixel.G & 254 | +BitHelper.GetBit(encode[i], --j);

                    var B = pixel.B & 254 | +BitHelper.GetBit(encode[i], --j);
                    
                    bitmap.SetPixel(x,y, Color.FromArgb(R,G,B));

                    if (j == 0)
                    {
                        j = 8;
                        i++;
                    }

                    if (i == encode.Length)
                        break;
                }

                if (i == encode.Length)
                    break;
            }
            
            return bitmap;
        }

        public static string Decode(Bitmap bitmap)
        {
            var information = new StringBuilder();
            bool isMsg = false;
            int textLength = 0;

            var tmp = new StringBuilder();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    
                    tmp.Append(pixel.R%2);

                    tmp.Append(pixel.G%2);

                    tmp.Append(pixel.B % 2);

                    if (tmp.Length == 9)
                    {
                        try
                        {
                            information.Append(BitHelper.GetString(new[] { Convert.ToByte(tmp.ToString(), 2) }));

                            if (!isMsg && Convert.ToByte(tmp.ToString(), 2) == 32)
                            {
                                isMsg = true;
                                textLength = Int32.Parse(information.ToString());
                                information.Clear();
                            }
                            tmp.Clear();
                        }
                        catch (Exception)
                        {
                            // Break on excpetion
                            isMsg = true;
                            information.Length = textLength;

                            information.Clear();
                            information.Append("Error during decoding. Be sure that you have correct image");
                        }
                    }

                    if (isMsg && information.Length >= textLength)
                        break;
                }

                if (isMsg && information.Length >= textLength)
                    break;

            }

            return information.ToString();
        }
     }
}
