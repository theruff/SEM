namespace CompressionMethods
{
    class Encoder {
        public static void encode(string sourcePath, string outPath)
        {
            RLE rle = new RLE();
            LZW lzw = new LZW(9);
            Huffman huff = new Huffman();

            byte[] sourceBytes = FileReader.readFile(sourcePath, 0);
            //byte[] outBytes = lzw.encode(sourceBytes);
            byte[] outBytes = huff.encode(rle.encode(sourceBytes));
            FileWriter.writeFile(outPath, outBytes, 2);
        }
    }
    class Decoder
    {
        public static void decode(string sourcePath, string outPath)
        {
            RLE rle = new RLE();
            LZW lzw = new LZW(9);
            Huffman huff = new Huffman();

            byte[] sourceBytes = FileReader.readFile(sourcePath, 2);
            byte[] outBytes = rle.decode(huff.decode(sourceBytes));
            //byte[] outBytes = lzw.decode(sourceBytes);
            FileWriter.writeFile(outPath, outBytes, 0);
        }
    }
}