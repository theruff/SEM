using System;
using System.Collections.Generic;
using System.Linq;

namespace CompressionMethods
{
    internal class LZW : ICodec
    {
        private BitWriter bw;
        private BitReader br;
        private byte[] decodedBytes;

        Dictionary<byte[], uint> lzwAlphabet;
        Dictionary<uint, byte[]> lzwReverse;

        private byte maxAlphabetPower;
        private byte alphabetPower = 9;
        byte[] sequence;
        byte[] outChars;

        private uint outValue;
        private uint seqMaxValue;
        uint lzwCode = byte.MaxValue + 1;

        public LZW(byte _maxAlphabetPower)
        {
            seqMaxValue = (uint)1 << alphabetPower;
            maxAlphabetPower = _maxAlphabetPower;
            bw = new BitWriter(_maxAlphabetPower);
            lzwAlphabet = new Dictionary<byte[], uint>(new ByteArrayComparer());
        }
        public LZW()
        {
            seqMaxValue = (uint)1 << alphabetPower;
            decodedBytes = new byte[1];
            lzwReverse = new Dictionary<uint, byte[]>();
        }
        public byte[] encode(byte[] bytes)
        {
            sequence = new byte[1] { bytes[0] };
            Console.WriteLine("{0}", bytes[0]); ///
            for (int i = 1; i < bytes.Length; i++)
            {
                //Console.WriteLine("{0}", bytes[i]); ///
                if (i % 100000 == 0)
                    Console.WriteLine("{0}", i);
                byte[] newSeq = sequence.Append(bytes[i]).ToArray();
                if (lzwAlphabet.TryGetValue(newSeq, out outValue))
                    sequence = newSeq;
                else
                {
                    tryAddAlphabet(newSeq, lzwCode++, false);
                    if (lzwAlphabet.TryGetValue(sequence, out outValue))
                        bw.addBits(outValue, alphabetPower);
                    else
                        foreach (byte b in sequence)
                            bw.addBits((uint)b, alphabetPower);

                    sequence = new byte[1] { bytes[i] };
                }
            }
            bw.addBits(sequence.Length > 1 ? lzwAlphabet.GetValueOrDefault(sequence) : (uint)sequence[0], alphabetPower);
            printAlphabet();
            return bw.getBytes();
        }
        
        public byte[] decode(byte[] bytes)
        {   
            br = new BitReader(bytes);
            maxAlphabetPower = br.DefaultSize;
            uint encodedByte;

            byte ch = (byte)br.readInt(alphabetPower);
            byte[] oldSeq = new byte[1] { ch };
            decodedBytes[0] = ch;
            Console.WriteLine("{0}", ch.ToString()); ///

            while (br.continueReading())
            {
                encodedByte = br.readInt(alphabetPower);
                //Console.WriteLine("{0}", encodedByte.ToString()); ///
                if (encodedByte == 508)
                    encodedByte *= 1;
                if (encodedByte <= byte.MaxValue)
                    sequence = new byte[1] { (byte)encodedByte };
                else
                    sequence = lzwReverse.TryGetValue(encodedByte, out outChars) ? outChars : oldSeq.Append(ch).ToArray();

                decodedBytes = decodedBytes.Concat(sequence).ToArray();
                //printBytes(sequence); ///
                ch = sequence[0];
                tryAddAlphabet(oldSeq.Append(ch).ToArray(), lzwCode++, true);
                oldSeq = sequence;
                //if (encodedByte == 508)
                //   alphabetPower += 1;
            }
            return decodedBytes;
        }
        private void tryAddAlphabet(byte[] sequence, uint code, bool reverse)
        {
            if (code < seqMaxValue - (reverse ? 2 : 0))
                addAlphabet(sequence, code, reverse);
            else
            {
                if (alphabetPower < maxAlphabetPower) /// Replace maxAlphabetPower
                {
                    alphabetPower++;
                    seqMaxValue *= 2;
                    addAlphabet(sequence, code, reverse);
                }
            }
        }
        private void addAlphabet(byte[] sequence, uint code, bool reverse)
        {
            if (reverse)
                lzwReverse.Add(code, sequence);
            else
                lzwAlphabet.Add(sequence, code);
        }
        private void printBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
                Console.WriteLine("{0}", b.ToString());
        }
        private void printAlphabet()
        {
            string s = "";
            for (int i = 0; i < lzwAlphabet.Count; i++)
            {
                foreach (byte b in lzwAlphabet.FirstOrDefault(x => x.Value == i + 256).Key)
                    s += b < 32 ? "." + b.ToString() + "." : ((char)b).ToString();
                s += ":<" + (i + 256) + ">-";
            }
            Console.WriteLine(s);
        }
    }
}