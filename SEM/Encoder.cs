using System.Collections.Generic;
using System.IO;

namespace CompressionMethods
{
    class Encoder {
        private const ushort MAGIC_BYTES = 12345;
        public void encode(string sourcePath, string outPath)
        {
            byte[] sourceBytes = FileReader.readSourceFile(sourcePath);
            if (sourceBytes != null)
            {
                Tree tree = new Tree(sourceBytes);
                BitWriter encodedData = new BitWriter();
                Dictionary<byte, CodeNode> alphabet = tree.generateAlphabet();

                foreach (byte b in sourceBytes)
                    encodedData.addBits(alphabet[b]);
                FileWriter.writeFile(outPath, this.composeData(encodedData.getBytes(), tree.serialize()), 0);
            }
        }
        private byte[] composeData (byte[] data, byte[] serializedTree)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(MAGIC_BYTES);
                    bw.Write(data.Length);
                    bw.Write(data);
                    bw.Write(serializedTree);
                    bw.Flush();
                }
                return ms.ToArray();
            }
        }
    }
    class Decoder
    {
        private const ushort MAGIC_BYTES = 12345;
        public void decode(string sourcePath, string outPath)
        {
            using (MemoryStream ms = new MemoryStream(FileReader.readSourceFile(sourcePath)))
            {
                if (ms.Length == 0)
                    throw new System.Exception("Filesize equals 0");
                using (BinaryReader br = new BinaryReader(ms))
                { 
                    if (br.ReadUInt16() != MAGIC_BYTES)
                        throw new System.Exception("Incorrect fileType");
                    int dataSize = br.ReadInt32();
                    byte[] compressedData = br.ReadBytes(dataSize);
                    byte[] serializedTree = br.ReadBytes((int)ms.Length - dataSize - sizeof(int) - sizeof(ushort));
                    Tree tree = new Tree();
                    tree.deserialize(serializedTree);
                    BitReader decodedData = new BitReader(compressedData);
                    FileWriter.writeFile(outPath, decodedData.readAll(tree), 0);
                }
            }
        }
    }
}
