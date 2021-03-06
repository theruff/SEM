using System;
using System.Collections.Generic;
using System.IO;

namespace CompressionMethods
{
    class Leave : IComparable<Leave>
    {
        private uint frequency;
        private byte character;
        private ushort leftChild;
        private ushort rightChild;
        public Leave(uint _freq, byte _character, ushort _leftChild, ushort _rightChild)
        {
            frequency = _freq;
            character = _character;
            leftChild = _leftChild;
            rightChild = _rightChild;
        }
        public Leave(byte _character, ushort _leftChild, ushort _rightChild) : this(0, _character, _leftChild, _rightChild) { }
        public Leave(uint _frequency, byte _character) : this(_frequency, _character, 0, 0) { }
        public Leave(uint _frequency, ushort _leftChild, ushort _rightChild) : this(_frequency, (byte)'~', _leftChild, _rightChild) { }
        public uint Frequency { get => frequency; }
        public ushort LeftChild { get => leftChild; }
        public ushort RightChild { get => rightChild; }
        public byte Character { get => character; }
        public bool isSymbol() => leftChild == rightChild ? true : false;
        public int CompareTo(Leave other)
        {
            if (this.frequency < other.frequency)
                return -1;
            else if (this.frequency > other.frequency)
                return 1;
            else
                return 0;
        }
        public override string ToString()
        {
            if (leftChild == 0 && rightChild == 0)
            {
                if (character < 32)
                    return "'" + character + "'-" + frequency.ToString() + "_L:" + leftChild.ToString() + "*R:" + rightChild.ToString();
                else
                    return (char)character + "-" + frequency.ToString() + "_L:" + leftChild.ToString() + "*R:" + rightChild.ToString();
            }
            else
                return "!0!-" + frequency.ToString() + "_L:" + leftChild.ToString() + "*R:" + rightChild.ToString();
        }
    }
    class Tree
    {
        private List<Leave> tree;
        private ushort pointer = 0;
        private Dictionary<byte, CodeNode> alphabet;
        internal List<Leave> leaves { get => tree; }
        public Tree()
        {
            tree = new List<Leave>();
        }
        public Tree(byte[] bytes)
        {
            tree = new List<Leave>();
            alphabet = new Dictionary<byte, CodeNode>();
            treePrefill(bytes);
            makeTree();
        }
        public void treePrefill(byte[] bytes)
        {
            uint[] probabilityTable = new uint[256];
            foreach (byte b in bytes)
                probabilityTable[b] += 1;

            for (int i = 0; i < probabilityTable.Length; i++)
                if (probabilityTable[i] > 0)
                    tree.Add(new Leave(probabilityTable[i], (byte)i));
            tree.Sort();
        }
        private void makeTree()
        {
            if (tree.Count == 1)
                tree.Add(new Leave(tree[0].Frequency, (byte)(byte.MaxValue - (int)tree[0].Character)));
            while (pointer + 1 < tree.Count)
            {
                insertLeave(new Leave(tree[pointer].Frequency + tree[pointer + 1].Frequency, pointer, (ushort)(pointer + 1)));
                pointer += 2;
            }
        }
        private void insertLeave(Leave leave)
        {
            for (int i = pointer; i < tree.Count; i++)
                if (leave.Frequency < tree[i].Frequency)
                {
                    tree.Insert(i, leave);
                    return;
                }
            tree.Add(leave);
        }
        public Dictionary<byte, CodeNode> generateAlphabet()
        {
            treeBypass((ushort)(tree.Count - 1), new CodeNode());
            return alphabet;
        }
        private void treeBypass(ushort leaveId, CodeNode huffCode)
        {
            if (tree[leaveId].isSymbol())
                alphabet.Add(tree[leaveId].Character, huffCode);
            else
            {
                treeBypass(tree[leaveId].LeftChild, addCodeBit(huffCode, 0));
                treeBypass(tree[leaveId].RightChild, addCodeBit(huffCode, 1));
            }
        }
        private CodeNode addCodeBit(CodeNode code, byte bit)
        {
            code.lenght++;
            code.code = (code.code << 1) | bit;
            return code;
        }
        public byte[] serialize()
        {
            byte indexPower = getIndexPower(tree.Count - 1);
            BitWriter bw = new BitWriter(indexPower);

            for (short i = 0; i < tree.Count; i++)
            {
                bw.addBits(tree[i].Character);
                bw.addBits(tree[i].LeftChild, indexPower);
                bw.addBits(tree[i].RightChild, indexPower);
            }
            return bw.getBytes();
        }
        public void deserialize(byte[] serializedTree)
        {
            BitReader br = new BitReader(serializedTree);
            int leavesCount = (serializedTree.Length - 1) * 8 / (8 + br.DefaultSize * 2);

            for (short i = 0; i < leavesCount; i++)
                tree.Add(new Leave(br.readByte(8), br.readShort(), br.readShort()));
        }
        public void print()
        {
            Console.WriteLine("Tree: {0}", tree.Count);
            string s = "";
            for (int i = 0; i < tree.Count; i++)
                s += tree[i] + " ";
            Console.WriteLine(s);
        }
        private byte getIndexPower(int s)
        {
            for (byte i = 15; i >= 0; i--)
                if ((s & (1 << i)) > 0)
                    return (byte)(i + 1);
            return 0;
        }
    }
    class Huffman : ICodec
    {
        BitWriter encodedData;
        public Huffman()
        {
            encodedData = new BitWriter();
        }
        public byte[] encode(byte[] bytes)
        {
            Tree tree = new Tree(bytes);
            Dictionary<byte, CodeNode> alphabet = tree.generateAlphabet();

            foreach (byte b in bytes)
                encodedData.addBits(alphabet[b]);
            return composeData(encodedData.getBytes(), tree.serialize());
        }
        public byte[] decode(byte[] bytes)
        {
            Tree tree = new Tree();
            MemoryStream ms = new MemoryStream(bytes);
            BinaryReader br = new BinaryReader(ms);

            int dataSize = br.ReadInt32();
            byte[] compressedData = br.ReadBytes(dataSize);
            byte[] serializedTree = br.ReadBytes((int)ms.Length - dataSize - sizeof(int));

            tree.deserialize(serializedTree);
            BitReader decodedData = new BitReader(compressedData);
            ms.Dispose();
            br.Dispose();
            return decodedData.readAll(tree);
        }
        private byte[] composeData(byte[] data, byte[] serializedTree)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(data.Length);
            bw.Write(data);
            bw.Write(serializedTree);
            bw.Flush();
            
            return ms.ToArray();
        }
    }
}