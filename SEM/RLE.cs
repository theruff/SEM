using System.Collections.Generic;

namespace CompressionMethods
{
    internal class RLE: ICodec
    {
        List<byte> encodedBytes;
        private const byte MARKER_BYTE = 0xff;
        private const byte MARKER_SHORT = 0xfd;
        private const byte MARKER_INT = 0xfe;
        public RLE ()
        {
            encodedBytes = new List<byte>();
        }
        public byte[] encode(byte[] bytes)
        {   
            for (int i = 0; i < bytes.Length;)
            {
                if (i + 1 == bytes.Length)
                {
                    encodedBytes.Add(bytes[i]);
                    break;
                }
                else
                {
                    if (bytes[i] == bytes[i + 1])
                        i += startRle(bytes, i);
                    else if (bytes[i] == MARKER_BYTE)
                        i += escaping();
                    else
                    {
                        encodedBytes.Add(bytes[i]);
                        i++;
                    }
                }
            }
            return encodedBytes.ToArray();
        }
        private int startRle(byte[] bytes, int index)
        {
            int startIndex = index;
            while (index + 2 < bytes.Length)
            {
                if (bytes[index + 1] == bytes[index + 2])
                    index++;
                else
                    break;
            }
            if (index - startIndex == 0)
            {
                if (bytes[index] == MARKER_BYTE)
                    return genRleCode(MARKER_BYTE, 2);
                else
                    return repeatByte(bytes[index]);
            }
            else
                return genRleCode(bytes[index], index - startIndex + 2);
        }
        private int genRleCode(byte b, int quant)
        {
            encodedBytes.Add(MARKER_BYTE);
            if (quant < 253)
                encodedBytes.Add((byte)quant);
            else if (quant < ushort.MaxValue)
            {
                encodedBytes.Add(MARKER_SHORT);
                toBytes(quant, sizeof(ushort));
            }
            else
            {
                encodedBytes.Add(MARKER_INT);
                toBytes(quant, sizeof(int));
            }
            encodedBytes.Add(b);
            return quant;
        }
        private int escaping()
        {
            encodedBytes.AddRange(new byte[] { MARKER_BYTE, MARKER_BYTE });
            return 1;
        }
        private int deEscaping()
        {
            encodedBytes.Add(MARKER_BYTE);
            return 2;
        }
        private int repeatByte(byte b)
        {
            encodedBytes.AddRange(new byte[] { b, b });
            return 2;
        }
        private void toBytes(int quant, int size)
        {
            for (int i = size; i > 0; i--)
                encodedBytes.Add((byte)((quant >> 8 * (i - 1)) & 0xff));
        }

        public byte[] decode(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length;)
            {
                if (bytes[i] == MARKER_BYTE)
                    i += startDeRle(bytes, i);
                else
                {
                    encodedBytes.Add(bytes[i]);
                    i++;
                }
            }
            return encodedBytes.ToArray();
        }
        private int startDeRle(byte[] bytes, int index)
        {
            //int count = 0;
            switch (bytes[++index])
            {
                case MARKER_BYTE:
                    return deEscaping();
                case MARKER_SHORT:
                    return longRle(bytes, index, sizeof(short));
                case MARKER_INT:
                    return longRle(bytes, index, sizeof(int));
                default:
                    return decodeRle(bytes[index + 1], bytes[index], 3);
            }
        }
        private int decodeRle(byte b, int count, int addCount)
        {
            for (int i = 0; i < count; i++)
                encodedBytes.Add(b);
            return addCount;
        }
        private int longRle(byte[] bytes, int index, int size)
        {
            int count = 0;
            for (int i = 0; i < size; i++)
                count |= bytes[index + i + 1] << (size - i - 1) * 8;
            return decodeRle(bytes[index + size + 1], count, size + 3);
        }
    }
}