using System;
using System.IO;

namespace CompressionMethods
{
    class FileReader
    {
        public static byte[] readFile (string filePath, int offset)
        {
            try
            {
                using (FileStream fs = new FileStream (filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length - offset];
                    byte[] preamble = new byte[offset];
                    if (offset != 0)
                    {
                        fs.Read(preamble, 0, preamble.Length);
                        if (preamble[0] != Common.MAGIC_BYTES >> 8 || preamble[1] != (Common.MAGIC_BYTES & 0xff))
                            throw new System.Exception("Incorrect fileType");
                    }
                    fs.Read (buffer, 0, buffer.Length);
                    if (buffer.Length == 0)
                        throw new System.Exception("Input file is empty");
                    return buffer;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error opening source file; {0}",e.Message);
                return null;
            }
        }
    }
    class FileWriter
    {
        public static void writeFile (string filePath, byte[] bytes, int offset)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    if (offset != 0)
                        fs.Write(new byte[2] { (byte)(Common.MAGIC_BYTES >> 8), (byte)(Common.MAGIC_BYTES & 0xff) }, 0, 2);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing down output file; {0}", e.Message);
            }
        }
    }
}