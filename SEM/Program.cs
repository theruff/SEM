using System.Collections.Generic;
using System.IO;

namespace CompressionMethods
{
    class Program
    {   
        class Encoder {
            private Tree tree;
            private Dictionary<byte, string> alphabet;
            byte[] inBytes;
            public Encoder(Tree _tree, byte[] _inBytes)
            {
                alphabet = _tree.getAlphabet();
                tree = _tree;
                inBytes = _inBytes;
            }
            public void Encode (string filePath)
            {
                BitWriter br = new BitWriter();
                foreach (byte b in inBytes)
                    br.addBits(alphabet[b]);
                br.toFile(filePath);
            }
        }
        static void Main(string[] args)
        {
            string fphamlet = "C:\\TestData\\hamlet.txt"; // file
            //string fphamlet = "C:\\Users\\ruff\\Desktop\\BMPs\\custom.bmp";
            FileStream fs = File.OpenRead(fphamlet);
            byte[] hamletBytes = new byte[fs.Length];
            fs.Read(hamletBytes, 0, hamletBytes.Length);
            fs.Close();

            Tree tr = new Tree(hamletBytes);
            tr.makeTree();
            tr.serialize();
            Encoder enc = new Encoder(tr, hamletBytes);
            enc.Encode("C:\\TestData\\out.txt");
        }
    }
}
