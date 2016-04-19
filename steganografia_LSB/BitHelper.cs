using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steganografia_LSB
{
    public class BitHelper
    {
        public static byte GetBit(byte val, int pos)
        {
            return (byte)((val & (1 << pos)) >> pos);
        }

        public static byte[] GetBytes(string val)
        {
            return Encoding.ASCII.GetBytes(val);
        }

        public static string GetString(byte[] array)
        {
            return Encoding.ASCII.GetString(array);
        }

        public static byte[] JoinArrayBytes(byte[] firts, byte[] second)
        {
            return firts.Concat(second).ToArray();
        }
    }
}
