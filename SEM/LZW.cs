using System;
using System.Collections.Generic;
using System.Linq;

namespace CompressionMethods
{
    public class ByteArrayComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] first, byte[] second)
        {
            if (first == null || second == null)
                return first == second;
            if (ReferenceEquals(first, second))
                return true;
            if (first.Length != second.Length)
                return false;
            return first.SequenceEqual(second);
        }
        public override int GetHashCode(byte[] obj)
        {
            if (obj == null)
                throw new ArgumentNullException("One of the comparing arrays is empty");
            return obj.Length;
        }
    }
    internal class LZW : ICodec
    {
        private BitWriter bw;
        private BitReader br;
        Dictionary<byte[], int> lzwAlphabet;
        private const int defaultWindowSize = 2;
        private byte alphabetPower;
        public LZW(byte _alphabetPower)
        {
            alphabetPower = _alphabetPower;
            bw = new BitWriter(_alphabetPower);
            lzwAlphabet = new Dictionary<byte[],int>(new ByteArrayComparer());
            initAlphabet();
        }
        public LZW(BitReader _br)
        {
            br = _br;
            alphabetPower = br.DefaultSize;
            lzwAlphabet = new Dictionary<byte[], int>(new ByteArrayComparer());
            initAlphabet();
        }
        public byte[] encode(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length - 1;)
                i += scanWindow(bytes, i, defaultWindowSize);

            printAlphabet();
            return bw.ByteArray.ToArray();
        }
        private int scanWindow(byte[] bytes, int position, int windowSize)
        {
            byte[] sequence = new byte[windowSize];
            for (int i = 0; i < windowSize; i++)
                sequence[i] = bytes[position++];
            byte[] toBitStream = sequence.Take(windowSize - 1).ToArray();

            if (!lzwAlphabet.TryAdd(sequence, lzwAlphabet.Count))
                return scanWindow(bytes, position - windowSize, windowSize + 1);
            else
                bw.addBits((uint)lzwAlphabet.GetValueOrDefault(toBitStream), alphabetPower);
            return windowSize - defaultWindowSize + 1;
        }
        private void initAlphabet()
        {
            for (int i = 0; i <= byte.MaxValue; i++)
                lzwAlphabet.Add(new byte[1] { (byte)i }, i);
        }

        public byte[] decode(byte[] bytes)
        { 
            return br.readAll(this);
        }
        private void printAlphabet()
        {
            string s = "";
            for (int i = 256; i < lzwAlphabet.Count; i++)
            {
                foreach (byte b in lzwAlphabet.FirstOrDefault(x => x.Value == i).Key)
                    s += (char)b;
                s += ":" + i + "-";
            }
            Console.WriteLine(s);
        }
    }
}