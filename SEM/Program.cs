using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CompressionMethods
{
    class Program
    {
        class BitWriter
        {
            private int bitPtr;
            private int bytePtr;
            private List<byte> byteArray;
            public BitWriter()
            {
                bitPtr = 0;
                bytePtr = 0;
                byteArray = new List<byte> { 0 };
            }
            public void addBits(string s)
            {
                string sub = "";
                int bitCount = 0;
                for (int i = s.Length - 1; i >= 0; i--)
                {
                    sub += s[i];
                    bitCount++;
                    if (bitCount == 8)
                    {
                        addBits(getByte(sub), sub.Length);
                        bitCount = 0;
                        sub = "";
                    }
                }
                if (sub.Length > 0)
                    addBits(getByte(sub), sub.Length);
            }
            private byte getBit (char c)
            {
                if (c == 48)
                    return 0;
                else if (c == 49)
                    return 1;
                else
                    throw new Exception("Wrong number");
            }
            public byte getByte (string s)
            {
                byte b = 0;
                for (int i = s.Length - 1, j = 0; i >= 0; i--, j++)
                    b |= (byte)(getBit(s[i]) << j);
                return b;
            }
            public void addBits(byte b, int size)
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
                    byte a = (byte)(byteArray[bytePtr] | (byte)(b << sizeof(byte) * 8 - size - bitPtr));
                    byteArray[bytePtr] = (byte)(byteArray[bytePtr] | (byte)(b << sizeof(byte) * 8 - size - bitPtr));
                    bitPtr += size;
                }
            }
            public void addBits(ushort s, int size)
            {
                checkCompatible(size, sizeof(ushort));
            }
            public void addBits(uint i, int size)
            {
                checkCompatible(size, sizeof(uint));
            }
            public void toFile (string address)
            {
                FileStream fs = File.OpenWrite(address);
                
                byte[] outBytes = new byte[byteArray.Count];
                for (int i = 0; i < byteArray.Count; i++)
                    outBytes[i] = byteArray[i];

                fs.Write(outBytes, 0, byteArray.Count);
                fs.Close();
            }
            public void getSize()
            {
                Console.WriteLine(bytePtr);
            }
            private void checkCompatible(int size, int power)
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
            private byte[] byteArray;

            public BitReader(byte[] inputBytes, int offset)
            {
                byteArray = inputBytes;
            }
        }
        [StructLayout(LayoutKind.Explicit)]
        struct FileHeader
        {
            [FieldOffset(0)] private short magicBytes;
            [FieldOffset(2)] private long bitLengthTree;
            [FieldOffset(6)] private long bitLengthData;
            [FieldOffset(10)] private short offsetData;
            public FileHeader(Tree tree)
            {
                magicBytes = 12345;
                bitLengthTree = 0;
                bitLengthData = 0;
                offsetData = 0;
            }
        }
        class TreeSerializer
        {
            private long bitLength;
            public TreeSerializer ()
            {

            }
            public void serialize(Tree tree)
            {
                int nodeIndexLength = getIndexLength(tree.getSize());


            }
            private int getIndexLength (int treeSize)
            {
                int predictLength = 1;
                int bitSize = 0;
                while (treeSize > predictLength)
                    predictLength *= 2; bitSize++;
                return bitSize;
            }
        }
        class SortedArray
        {
            private Dictionary<byte, int> sortedChars;
            private int[] probabilityTable = new int[256];

            public SortedArray(byte[] inputArray)
            {
                foreach (byte b in inputArray)
                    this.probabilityTable[b] += 1;
                sortedChars = new Dictionary<byte, int>();
            }
            public Dictionary<byte, int> SortIndexesDesc()
            {
                sortedChars.Clear();
                int maxChar = 0;
                int index = 0;
                for (int i = 0; i < probabilityTable.Length; i++)
                {
                    for (int j = 0; j < probabilityTable.Length; j++)
                    {
                        if (probabilityTable[j] > maxChar)
                        {
                            index = j;
                            maxChar = probabilityTable[j];
                        }
                    }
                    if (maxChar > 0)
                        sortedChars.Add((byte)index, maxChar);
                    else
                        return sortedChars;
                    maxChar = 0;
                    probabilityTable[index] = 0;
                }
                return sortedChars;
            }
            public Dictionary<byte, int> SortIndexesAsc()
            {
                sortedChars.Clear();
                int minFreq = int.MaxValue;
                int index = 0;
                for (int i = 0; i < probabilityTable.Length; i++)
                {
                    for (int j = 0; j < probabilityTable.Length; j++)
                    {
                        if (probabilityTable[j] < minFreq && probabilityTable[j] != 0)
                        {
                            index = j;
                            minFreq = probabilityTable[j];
                        }
                    }
                    if (minFreq != int.MaxValue)
                        sortedChars.Add((byte)index, minFreq);
                    else
                        return sortedChars;
                    probabilityTable[index] = 0;
                    minFreq = int.MaxValue;
                }
                return sortedChars;
            }
            public void getArray()
            {
                foreach (KeyValuePair<byte, int> bi in sortedChars)
                    Console.WriteLine("{0}-{1}",bi.Value,(char)bi.Key);
            }
        }
        class Encoder {
            private Dictionary<byte, string> alphabet;
            byte[] inBytes;
            public Encoder(Dictionary<byte, string> alphabet, byte[] inBytes)
            {
                this.alphabet = alphabet;
                this.inBytes = inBytes;
            }

            public void Encode (string filePath)
            {
                BitWriter br = new BitWriter();
                foreach (byte b in inBytes)
                    br.addBits(alphabet[b]);
                br.toFile(filePath);
            }
        }
        class Tree
        {
            static private int currentIndex = 1;

            private Dictionary<byte, string> alphabet;
            private Dictionary<int, TreeNode> leaves;
            private List<KeyValuePair<int, TreeNode>> tree;

            public Dictionary<byte, string> Alphabet { get => alphabet; set => alphabet = value; }

            public Tree()
            {
                leaves = new Dictionary<int, TreeNode>();
                tree = new List<KeyValuePair<int, TreeNode>>();
                Alphabet = new Dictionary<byte, string>();
            }
            public Tree(Dictionary<byte, int> dict)
            {
                tree = new List<KeyValuePair<int, TreeNode>>();
                
                foreach (KeyValuePair<byte, int> kvp in dict)
                {
                    this.addNode(new TreeNode(kvp.Value, kvp.Key));
                }
                leaves = new Dictionary<int, TreeNode>(tree);
                Alphabet = new Dictionary<byte, string>();
                while (this.getSize() != 1)
                    this.combineWeights();
                this.makeAlphabet();
            }
            public void addNode(TreeNode node)
            {
                if (node.NextIndexLeft != 0 || node.NextIndexRight != 0)
                    leaves.Add(currentIndex, node);
                for (int i = 0; i < tree.Count; i++)
                {
                    if (tree.ElementAt(i).Value.NodeWeight > node.NodeWeight)
                    {
                        tree.Insert(i, new KeyValuePair<int, TreeNode>(currentIndex, node));
                        currentIndex++;
                        return;
                    }
                }
                tree.Add(new KeyValuePair<int, TreeNode>(currentIndex, node));
                currentIndex++;
            }
            private void combineWeights ()
            {
                var leftEl = tree.ElementAt(0);
                var rightEl = tree.ElementAt(1);
                tree.RemoveAt(0);
                tree.RemoveAt(0);
                TreeNode n = new TreeNode(leftEl.Value.NodeWeight + rightEl.Value.NodeWeight, leftEl.Key, rightEl.Key);
                this.addNode(n);
            }
            public void drawTree(List<int>parentLeaveIndexes, int level)
            {
                if (parentLeaveIndexes.Count == 0)
                    parentLeaveIndexes.Add(currentIndex - 1);
                List<int> nextIndexes = new List<int>();
                string currentLevel = level + ":  ";
                foreach (int index in parentLeaveIndexes)
                {
                    if (leaves[index].NextIndexLeft != 0 )
                        nextIndexes.Add(leaves[index].NextIndexLeft);
                    if (leaves[index].NextIndexRight != 0)
                        nextIndexes.Add(leaves[index].NextIndexRight);
                    currentLevel += leaves[index].ToString() + " --- ";
                }
                Console.WriteLine(currentLevel);
                if (nextIndexes.Count == 0)
                    return;
                drawTree(nextIndexes, ++level);
            }
            private void makeAlphabet()
            {
                treeBypass(tree.FirstOrDefault().Key, "");
            }
            public void treeBypass(int nodeId, string elementCode)
            {
                if (leaves[nodeId].isSymbol())
                    Alphabet.Add(leaves[nodeId].getByte(), elementCode);
                leaves[nodeId].ElementCode = elementCode;
                if (leaves[nodeId].NextIndexLeft != 0)
                    treeBypass(leaves[nodeId].NextIndexLeft, elementCode + "0");
                if (leaves[nodeId].NextIndexRight != 0)
                    treeBypass(leaves[nodeId].NextIndexRight, elementCode + "1");
            }
            public int getSize()
            {
                return tree.Count;
            }
            public void getLeaves()
            {
                foreach (KeyValuePair<int, TreeNode> l in leaves)
                    Console.Write("{0} - {1}; ", l.Key, l.Value);
            }
            public override string ToString()
            {
                string result = "";
                foreach (KeyValuePair<int, TreeNode> n in tree)
                    result += "ID:" + n.Key + "_" + n.Value.ToString() + " - ";
                return result;
            }
        }


        class TreeNode : IComparable<TreeNode>
        {
            private static int currentIndex = 1;
            private int nodeIndex;
            private int nodeWeight;
            private int nextIndexLeft;
            private int nextIndexRight;
            private byte nodeChar;
            private string elementCode;
            private bool nonSymbolNode = true;

            public TreeNode(int nodeWeight, int nextIndexLeft, int nextIndexRight)
            {
                this.nodeWeight = nodeWeight;
                this.NextIndexLeft = nextIndexLeft;
                this.NextIndexRight = nextIndexRight;
                nodeIndex = currentIndex;
                currentIndex++;
                nodeChar = 0;
            }
            public TreeNode(int nodeWeight, byte ch)
            {
                this.nodeWeight = nodeWeight;
                nodeIndex = currentIndex;
                currentIndex++;
                nodeChar = ch;
                nonSymbolNode = false;
            }
            public int NodeWeight { get => nodeWeight; set => nodeWeight = value; }
            public int NextIndexLeft { get => nextIndexLeft; set => nextIndexLeft = value; }
            public int NextIndexRight { get => nextIndexRight; set => nextIndexRight = value; }
            public string ElementCode { get => elementCode; set => elementCode = value; }
            public bool isSymbol() { return !nonSymbolNode; }
            public byte getByte() { return nodeChar; }
            public override string ToString()
            {
                if (nonSymbolNode)
                    return "(" + nodeWeight.ToString() + ":___O___-" + nodeIndex.ToString() + "_/" + nextIndexLeft + "-" + nextIndexRight + "\\_" + ")";
                return "(" + nodeWeight.ToString() + ":" + ((char)nodeChar).ToString() + "-" + nodeIndex.ToString() + "**" + elementCode + "**)";
            }
            public int CompareTo(TreeNode other)
            {
                if (this.nodeWeight < other.nodeWeight)
                    return 1;
                else if (this.nodeWeight > other.nodeWeight)
                    return -1;
                else
                    return 0;
            }
        }

        static void Main(string[] args)
        {
            string fphamlet = "C:\\Users\\ruff\\Desktop\\BMPs\\hamlet4.txt";
            //string fphamlet = "C:\\Users\\ruff\\Desktop\\BMPs\\custom.bmp";
            FileStream fs = File.OpenRead(fphamlet);
            byte[] hamletBytes = new byte[fs.Length];
            fs.Read(hamletBytes, 0, hamletBytes.Length);
            fs.Close();

            SortedArray sa = new SortedArray(hamletBytes);
            Tree tr = new Tree(sa.SortIndexesAsc());

            tr.getLeaves();
            tr.drawTree(new List<int>(), 0);

            Encoder enc = new Encoder(tr.Alphabet, hamletBytes);
            enc.Encode("C:\\Users\\ruff\\Desktop\\BMPs\\out.txt");
        }
    }
}
