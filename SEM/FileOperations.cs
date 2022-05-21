using System;
using System.IO;
//using System.Collections.Generic;

namespace CompressionMethods
{
    class FileReader
    {
        public static byte[] readSourceFile (string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream (filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read (buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error oprning source file; {0}",e.Message);
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
                    fs.Write(bytes, offset, bytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing down output file; {0}", e.Message);
            }
        }
    }
}