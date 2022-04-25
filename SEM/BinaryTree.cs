using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CompressionMethods
{
    class Leave : IComparable<Leave>
    {
        private uint frequency;
        private byte character;
        private ushort leftChild;
        private ushort rightChild;
        public Leave(uint _frequency, byte _character)
        {
            frequency = _frequency;
            character = _character;
            leftChild = 0;
            rightChild = 0;
        }
        public Leave(uint _frequency, ushort _leftChild, ushort _rightChild)
        {
            frequency = _frequency;
            character = (byte)'~';
            leftChild = _leftChild;
            rightChild = _rightChild;
        }
        public uint Frequency { get => frequency; }
        public ushort LeftChild { get => leftChild; }
        public ushort RightChild { get => rightChild; }
        public byte Character { get => character; }

        public bool isSymbol() => leftChild == rightChild ? true : false;
        public int CompareTo([AllowNull] Leave other)
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
                return frequency.ToString() + "-" + character + "_" + leftChild.ToString() + rightChild.ToString();
            else
                return frequency.ToString() + "-" + ":O:_" + leftChild.ToString() + rightChild.ToString(); ;
            return base.ToString();
        }
    }
    class Tree
    {
        private List<Leave> tree;
        private ushort pointer;
        private Dictionary<byte, string> alphabet;
        public Tree(byte[] bytes)
        {
            pointer = 0;
            uint[] probabilityTable = new uint[256];
            foreach (byte b in bytes)
                probabilityTable[b] += 1;
            tree = new List<Leave>();
            alphabet = new Dictionary<byte, string>();
            for (int i = 0; i < probabilityTable.Length; i++)
            {
                if (probabilityTable[i] > 0)
                    tree.Add(new Leave(probabilityTable[i], (byte)i));
            }
            tree.Sort();
        }
        public void makeTree()
        {
            while (pointer + 1 < tree.Count)
            {
                tree.Add(new Leave(tree[pointer].Frequency + tree[pointer + 1].Frequency, pointer, (ushort)(pointer + 1)));
                tree.Sort();
                pointer += 2;
            }
        }
        private void treeBypass(ushort leaveId, string elementCode)
        {
            if (tree[leaveId].isSymbol())
                alphabet.Add(tree[leaveId].Character, elementCode);
            else
            {
                treeBypass(tree[leaveId].LeftChild, elementCode + "0");
                treeBypass(tree[leaveId].RightChild, elementCode + "1");
            }
        }
        public Dictionary<byte, string> getAlphabet()
        {
            treeBypass((ushort)(tree.Count - 1), "");
            return alphabet;
        }
        public BitWriter serialize()
        {
            byte indexPower = getIndexPower(tree.Count - 1);
            BitWriter br = new BitWriter(indexPower);

            for (short i = 0; i < tree.Count; i++)
            {
                br.addBits(tree[i].Character);
                br.addBits(tree[i].LeftChild, indexPower);
                br.addBits(tree[i].RightChild, indexPower);
            }
            br.finalize();
            return br;
        }
        private byte getIndexPower(int s)
        {
            for (byte i = 15; i >= 0; i--)
            {
                if ((s & (1 << i)) > 0)
                    return (byte)(i + 1);
            }
            return 0;
        }
    }
}
