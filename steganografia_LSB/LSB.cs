using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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

        public static Bitmap EncodeParity(BitArray encode, Bitmap bitmap)
        {
            
            int i = 0;
            int j = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    var x1 = Convert.ToInt32(encode[j++]);
                    var x2 = Convert.ToInt32(encode[j++]);

                    var R = pixel.R;
                    var G = pixel.G;
                    var B = pixel.B;

                    if (x1 != (pixel.R % 2 ^ pixel.B % 2) && x2 == (pixel.G % 2 ^ pixel.B % 2))
                        R = (byte) (pixel.R ^ 1);

                    if (x1 == (pixel.R % 2 ^ pixel.B % 2) && x2 != (pixel.G % 2 ^ pixel.B % 2))
                        G = (byte) (pixel.G ^ 1);

                    if (x1 != (pixel.R % 2 ^ pixel.B % 2) && x2 != (pixel.G % 2 ^ pixel.B % 2))
                        B = (byte) (pixel.B ^ 1);
                    
                    bitmap.SetPixel(x, y, Color.FromArgb(R, G, B));
                    
                    if (j == encode.Length)
                        break;
                }

                if (j == encode.Length)
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

        public static string DecodeParity(Bitmap bitmap)
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

                    var x1 = (pixel.R % 2) ^ (pixel.B % 2);
                    var x2 = (pixel.G % 2) ^ (pixel.B % 2);

                    tmp.Append(x1);

                    tmp.Append(x2);
                    
                    if (tmp.Length == 40)
                    {
                        try
                        {
                            information.Append(BitHelper.GetString(new[] { Convert.ToByte(LSB.Decode1Of5(tmp.ToString()), 2) }));

                            if (!isMsg && Convert.ToByte(LSB.Decode1Of5(tmp.ToString()), 2) == 32)
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

        public static Bitmap PermutateBitmap(Bitmap bitmap, int seed)
        {
            var image = new Bitmap(bitmap.Width, bitmap.Height);
            var random = new Random(seed);
            var permutate = new List<int>() { 0 };
            
            for (int i = 1; i < bitmap.Width*bitmap.Height; i++)
            {
                int swap = random.Next(i - 1);
                permutate.Add(permutate[swap]);
                permutate[swap] = i;
            }

            for (int i = 0; i < permutate.Count; i++)
            {
                image.SetPixel(i % bitmap.Width, (int)Math.Floor((double)(i / bitmap.Width)),bitmap.GetPixel(permutate.ElementAt(i) % bitmap.Width, (int)Math.Floor((double)(permutate.ElementAt(i) / bitmap.Width))));
            }

            return image;
        }

        public static Bitmap UnpermutateBitmap(Bitmap bitmap, int seed)
        {
            var image = new Bitmap(bitmap.Width, bitmap.Height);
            var random = new Random(seed);
            var permutate = new List<int>(){0};

            for (int i = 1; i < bitmap.Width * bitmap.Height; i++)
            {
                int swap = random.Next(i - 1);
                permutate.Add(permutate[swap]);
                permutate[swap] = i;
            }

            for (int i = 0; i < permutate.Count; i++)
            {
                image.SetPixel(permutate.ElementAt(i) % bitmap.Width, (int)Math.Floor((double)(permutate.ElementAt(i) / bitmap.Width)),bitmap.GetPixel(i % bitmap.Width, (int)Math.Floor((double)(i / bitmap.Width))));
            }

            return image;
        }

        public static BitArray Encode1Of5(byte[] message)
        {
            var encode = new BitArray(message.Length * 8 * 5);
            var pos = 0;

            foreach (var t in message)
            {
                var tmp = new BitArray(BitConverter.GetBytes(t).ToArray());
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        encode.Set(pos++, tmp[j]);
                    }
                }
            }

            return encode;
        }

        public static string Decode1Of5(string message)
        {
            var tmp = new StringBuilder();
            
            for (int i = 0; i < message.Length; i += 5)
            {
                tmp.Insert(0,message.Substring(i, 5).Count(x => x == '1') > 2 ? "1" : "0");
                
            }

            return tmp.ToString();
        }

       
     }



}
