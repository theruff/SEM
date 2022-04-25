using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CompressionMethods
{
    class BitWriter
    {
        private int bitPtr;
        private int bytePtr;
        private List<byte> byteArray;

        public int BitPtr { get => bitPtr; }
        public List<byte> ByteArray { get => byteArray; }

        public BitWriter() : this(0) { }
        public BitWriter(byte preamble)
        {
            bitPtr = 0;
            bytePtr = 1;
            byteArray = new List<byte> { };
            byteArray.Add((byte)(preamble << 3));
            byteArray.Add(0);
        }
        private byte getBit(char c)
        {
            if (c == 48)
                return 0;
            else if (c == 49)
                return 1;
            else
                throw new Exception("Wrong number");
        }
        public byte getByte(string s)
        {
            byte b = 0;
            for (int i = s.Length - 1, j = 0; i >= 0; i--, j++)
                b |= (byte)(getBit(s[i]) << j);
            return b;
        }
        public void addBits(string s)
        {
            string sub = "";
            //int bitCount = 0;
            for (int i = 0; i < s.Length; i++)
            {
                sub += s[i];
                if (sub.Length == 8)
                {
                    addBits(getByte(sub), (byte)sub.Length);
                    sub = "";
                }
            }
            if (sub.Length > 0)
                addBits(getByte(sub), (byte)sub.Length);
        }
        public void addBits(byte b) => addBits(b, 8);
        public void addBits(byte b, byte size)
        {
            checkCompatible(size, sizeof(byte));
            if (bitPtr + size > sizeof(byte) * 8)
            {
                byteArray.Add(0);
                int nextByteBitPtr = size - (sizeof(byte) * 8 - bitPtr);
                byteArray[bytePtr] = (byte)(byteArray[bytePtr] | (byte)(b >> nextByteBitPtr));
                int mask = (2 << nextByteBitPtr) - 1;
                byteArray[bytePtr + 1] = (byte)(
                    (b & mask) << (sizeof(byte) * 8 - nextByteBitPtr)
                    );
                bitPtr = nextByteBitPtr;
                bytePtr++;
            }
            else
            {
                byteArray[bytePtr] = (byte)(byteArray[bytePtr] | (byte)(b << sizeof(byte) * 8 - size - bitPtr));
                bitPtr += size;
            }
        }
        public void addBits(ushort s, byte size)
        {
            if (size <= 8)
                addBits((byte)s, size);
            else
            {
                checkCompatible(size, sizeof(ushort));
                addBits((byte)(s >> 8), (byte)(size - 8));
                addBits((byte)(s & 0xff), 8);
            }
        }
        public void toFile(string address)
        {
            FileStream fs = File.OpenWrite(address);

            byte[] outBytes = new byte[byteArray.Count];

            fs.Write(genHeader(), 0, 12);
            fs.Write(byteArray.ToArray(), 0, byteArray.Count);
            fs.Close();
        }
        public void finalize() => byteArray[0] |= (byte)bitPtr;
        public byte[] genHeader()
        {
            const short magicBytes = 12345;
            long bitTreeLength = long.MaxValue;
            long bitDataLength = bytePtr * 8 + bitPtr;
            int headLength = sizeof(short) + 2 * sizeof(long);

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(magicBytes);
                    bw.Write(bitTreeLength);
                    bw.Write(bitDataLength);
                }
                return ms.ToArray();
            }

        }
        private void checkCompatible(byte size, int power)
        {
            if (size > power* 8)
                throw new Exception("Power is lower than digits");
        }
        public override string ToString()
        {
            string result = "";
            foreach (byte b in byteArray)
            {
                for (int i = 7; i >= 0; i--)
                    result += 1 & (b >> i);
                result += " | ";
            }
            return result;
        }
    }
    class BitReader
    {
        private int bitPtr;
        private int bytePtr;
        private byte[] byteArray;

        public BitReader(byte[] inputBytes, int offset)
        {
            byteArray = inputBytes;
        }
    }
}