using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NavtelecomProtocol
{
    internal static class BinaryUtilities
    {
        public static byte GetXorSum(byte[] buffer) => GetXorSum(buffer, 0, buffer.Length);

        public static byte GetXorSum(byte[] buffer, int offset, int length)
        {
            byte result = 0;

            for (var i = offset; i < length; ++i)
                result ^= buffer[i];

            return result;
        }

        public static byte GetXorSum(IEnumerable<byte> bytes)
        {
            return bytes.Aggregate<byte, byte>(0, (current, b) => (byte)(current ^ b));
        }

        public static bool ReadStringAndCompare(BinaryReader br, string value)
        {
            var chars = br.ReadChars(value.Length);

            return chars.SequenceEqual(value);
        }

        public static byte[] StringToBytes(string value) => value.Select(x => (byte)x).ToArray();

        public static int GetByteCountFromBitCount(int bitCount) => (bitCount >> 3) + ((bitCount & 7) == 0 ? 0 : 1);

        public static byte GetCrc8(IEnumerable<byte> buffer)
        {
            byte crc = 0xFF;
            foreach (var b in buffer)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    crc = (crc & 0x80) != 0 ? (byte)((crc << 1) ^ 0x31) : (byte)(crc << 1);
                }
            }
            return crc;
        }

        public static bool IsEven(int value) => (value & 1) == 0;
    }
}