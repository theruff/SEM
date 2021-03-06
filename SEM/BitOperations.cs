using System;
using System.Collections.Generic;

namespace CompressionMethods
{
    class BitWriter
    {
        private int bitPtr;
        private int bytePtr;
        private List<byte> byteArray;

        public int BitPtr { get => bitPtr; }
        public List<byte> ByteArray { get => byteArray; }
        public int BytePtr { get => bytePtr; }
        public BitWriter() : this(0) { }
        public BitWriter(byte preamble)
        {
            bitPtr = 0;
            bytePtr = 1;
            byteArray = new List<byte> { (byte)(preamble << 3), 0 };
        }
        public void addBits(CodeNode c) {
            if (c.lenght > 0)
                addBits(c.code, c.lenght);
        }
        public void addBits(byte b) => addBits(b, 8);
        public void addBits(byte b, byte size)
        {
            checkCompatible(size, sizeof(byte));
            if (bitPtr + size >= sizeof(byte) * 8)
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
        public void addBits(uint s, byte size)
        {
            if (size <= 8)
                addBits((byte)s, size);
            else if (size > 8 && size <= 16)
                addBits((ushort)s, size);
            else
            {
                checkCompatible(size, sizeof(uint));
                addBits((ushort)(s >> 16), (byte)(size - 16));
                addBits((ushort)(s & ushort.MaxValue), 16);
            }
        }
        public byte[] getBytes()
        {
            finalize();
            return byteArray.ToArray();
        }
        private void finalize() => byteArray[0] |= (byte)bitPtr;
        private void checkCompatible(byte size, int power)
        {
            if (size > power * 8)
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
        private int lastBits;
        private byte defaultSize;
        private byte[] byteArray;
        private List<byte> decodedBytes;
        private int count;

        public BitReader(byte[] inputBytes)
        {
            byteArray = inputBytes;
            decodedBytes = new List<byte>();
            defaultSize = (byte)(byteArray[0] >> 3);
            lastBits = byteArray[0] & 0x07;
            bytePtr = 1;
            bitPtr = 0;
        }
        public uint this[int index] {
            get {
                if (index > Count - 1)
                    throw new IndexOutOfRangeException();
                int tmpBit = bitPtr;
                int tmpByte = bytePtr;
                bitPtr = (index * defaultSize % 8);
                
                bytePtr = (((defaultSize * index) + 1) / 8) + 1;
                if (bitPtr == 7)
                    bytePtr--;
                uint ret = readInt();
                this.bytePtr = tmpByte;
                this.bitPtr = tmpBit;
                return ret;
            } 
        }

        public byte DefaultSize { get => defaultSize; }
        public int Count { get => (byteArray.Length - 1) * 8 / defaultSize; }
        public bool continueReading() => (bytePtr + 1 == byteArray.Length && bitPtr == lastBits) ? false : true;
        
        public ushort readShort() => readShort(defaultSize);
        public uint readInt() => readInt(defaultSize);
        public byte readByte(byte size)
        {
            int mask = (0xff >> bitPtr);
            if (bitPtr + size >= sizeof(byte) * 8)
            {
                bitPtr = size - (sizeof(byte) * 8 - bitPtr);
                byte higherBits = (byte)((byteArray[bytePtr] & mask) << bitPtr);
                bytePtr++;
                return (byte)((byteArray[bytePtr] >> (sizeof(byte) * 8 - bitPtr)) | higherBits);
            }
            else
            {
                int oddBits = sizeof(byte) * 8 - size - bitPtr;
                bitPtr += size;
                return (byte)((byteArray[bytePtr] & mask) >> oddBits);
            }
        }
        public ushort readShort(byte size)
        {
            if (size <= 8)
                return readByte(size);
            else
                return (ushort)(readByte(8) << (size - 8) | readByte((byte)(size - 8)));
        }
        public uint readInt(byte size)
        {
            if (size <= 8)
                return readByte(size);
            else if (size <= 16)
                return readShort(size);
            else
                return (uint)(readShort(16) << (size - 16) | readShort((byte)(size - 16)));
        }
        public byte[] readAll(Tree tree)
        {
            int leavesIndex = 0;
            
            while (bytePtr != byteArray.Length)
            {
                if (bytePtr + 1 == byteArray.Length)
                    if (bitPtr == lastBits)
                        break;
                leavesIndex = tree.leaves.Count - 1;
                while (!tree.leaves[leavesIndex].isSymbol())
                {
                    if (readByte(1) == 0)
                        leavesIndex = tree.leaves[leavesIndex].LeftChild;
                    else
                        leavesIndex = tree.leaves[leavesIndex].RightChild;
                }
                decodedBytes.Add(tree.leaves[leavesIndex].Character);
            }       
            return decodedBytes.ToArray();
        }
        public byte[] readAll(LZW lzw)
        {
            uint tmp;
            while (bytePtr != byteArray.Length)
            {
                if (bytePtr + 1 == byteArray.Length)
                    if (bitPtr == lastBits)
                        break;
                tmp = readInt();
                if (tmp <= byte.MaxValue)
                {

                }
            }
            return decodedBytes.ToArray();
        }
        public override string ToString()
        {
            string result = "";
            if (defaultSize > 0)
            {
                for (int i = 0; i < Count; i++)
                    result += this[i].ToString() + " | ";
            }
            /*
            else
            {
                foreach (byte b in byteArray)
                {
                    for (int i = 7; i >= 0; i--)
                        result += 1 & (b >> i);
                    result += " | ";
                }
            }
            */
            return result;
        }
    }
}