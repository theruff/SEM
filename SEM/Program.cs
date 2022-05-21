using System;
using System.Collections.Generic;
using System.IO;

namespace CompressionMethods
{
    class Program
    {   
        static void Main(string[] args)
        {
            string sourcePath = "C:\\TestData\\hamlet5.txt"; // file
            //string sourcePath = "C:\\TestData\\1.txt";
            //string sourcePath = "C:\\TestData\\DJI_0009_Trim.mp4";
            //string sourcePath = "C:\\TestData\\notepad.exe";
            string destPath = "C:\\TestData\\out.haf";
            
            if (args.Length != 0)
            {
                sourcePath = args[0];
                destPath = args[1];
            }

            Encoder encoder = new Encoder();
            encoder.encode(sourcePath, destPath);
            Decoder decoder = new Decoder();
            decoder.decode(destPath, "C:\\TestData\\out.txt");
        }
    }
}
