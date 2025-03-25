using System;
using System.Threading;
using System.Diagnostics;
using nanoFramework.Hardware.Esp32;
using System.IO;

namespace nanoFramework_FileSystem
{
    public class Program
    {
        public static void Main()
        {
            string _drive = @"I:\";
            string _path = @"data\";
            int _noOfFiles = 60;
            int _noOfRecords = 10;
            int _recordLength = 128;

            Debug.WriteLine("Hello from nanoFramework!");
         
            CreateFiles(_drive, _path, _noOfFiles, _noOfRecords, _recordLength);

            ListFiles(_drive, _path);

            LoadFile(_drive, _path, _recordLength);

            DeleteFiles(_drive, _path);

            Debug.WriteLine("Done");

            Thread.Sleep(Timeout.Infinite);

           
        }

        public static byte[] GenerateBytes(int length)
        {
            byte[] ret = new byte[length];
            var rnd = new Random();
            rnd.NextBytes(ret);
            return ret;
        }

        private static void ListFiles(string drive, string path)
        {

            PrintMemory("Start List Files");
            if (Directory.Exists(drive + path))
            {
                var sw = new Stopwatch();
                sw.Start();
                var files = Directory.GetFiles(drive + path);
                Debug.WriteLine($"No of Files in List: {files.Length}");
                foreach (var file in files)
                {
                    Debug.WriteLine("FileName: " + file);
                }
                Debug.WriteLine("List File Time: " + sw.ElapsedMilliseconds);
                sw.Stop();

            }
            PrintMemory("End List Files");
        }

        public static void CreateFiles(string drive, string path, int noOfFiles, int noOfRecords, int recordLength)
        {
            PrintMemory("Start Create Files");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var dird = Directory.CreateDirectory(drive + path);
            Debug.WriteLine($"Create Directory Time: {sw.ElapsedMilliseconds}");
          
            sw.Restart();

            for (int i = 0; i < noOfFiles; i++)
            {
                var rnd = new Random().Next();
                try
                {
                    var filename = rnd.ToString();
                    var data = GenerateBytes(recordLength);

                    using var file = new FileStream(drive + path + filename, FileMode.Create);
                    for (int y = 0; y < noOfRecords; y++)
                        {
                            file.Write(BitConverter.GetBytes((ushort)recordLength), 0, 2);
                            file.Write(data, 0, data.Length);
                        }

                    file.Close();
                    file.Dispose();
                    if (i == 0)
                    {
                        Debug.WriteLine($"First File Time: {sw.ElapsedMilliseconds}");
                    }
                    Debug.Write($"{i} ");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Create Error: " + ex.Message);
                }
       
            }
            Debug.WriteLine("\nCreate EndTime: " + sw.ElapsedMilliseconds);
               
            sw.Stop();
            PrintMemory("End Create Files");
           
        }


        private static void LoadFile(string drive, string path, int recordLength)
        {
            PrintMemory("Start Load Files");
            if (Directory.Exists(drive + path))
            {
                var sw = new Stopwatch();
                sw.Start();
                var files = Directory.GetFiles(drive + path);
                Debug.WriteLine($"Total No of Files: {files.Length}");
                foreach (var file in files)
                {
                    Debug.WriteLine("Load FileName: " + file);
                    int count = 0;
                    int datasize = 0;
                    byte[] cntbyte = new byte[2];
                    byte[] databytes = new byte[recordLength];

                    using (var file1 = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        Debug.WriteLine("Total file Length: " + file1.Length);
                        var totalLen = file1.Length; 
                        if (totalLen <= 0)
                            continue;

                         Debug.WriteLine("File Position: " + file1.Position);
                        while (file1.Position < totalLen)
                        {
                            
                            var clen = file1.Read(cntbyte, 0, 2);
                            Debug.WriteLine("Read Length: " + clen);
                            if (clen > 0)
                            {
                                Debug.WriteLine("Length: " + BitConverter.ToUInt16(cntbyte, 0));
                                var datalen = file1.Read(databytes, 0, BitConverter.ToUInt16(cntbyte, 0));
                                Debug.WriteLine($"Position: {file1.Position} DataLength: {datalen}");
                            }
                        }

                        file1.Close();

                    }
                    
                }
                Debug.WriteLine("Load File Time: " + sw.ElapsedMilliseconds);
                sw.Stop();
                PrintMemory("End Load Files");
            }

        }

        private static void DeleteFiles(string drive, string path)
        {
            PrintMemory("Start Delete Files");
           
            if (Directory.Exists(drive + path))
            {

                var files = Directory.GetFiles(drive + path);
                foreach (var file in files)
                {
                    try
                    {
                        Debug.Write($"Delete File: {file} ");
                       
                        File.Delete(file);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Delete Error: " + ex.Message);
                    }
                }
                PrintMemory("End Delete Files");
            }

        }

        public static void PrintMemory(string msg)
        {
            NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.Internal, out uint totalSize, out uint totalFreeSize, out uint largestBlock);
            Debug.WriteLine($"\n{msg}-> Internal Total Mem {totalSize} Total Free {totalFreeSize} Largest Block {largestBlock}");
            Debug.WriteLine($"nF Mem {nanoFramework.Runtime.Native.GC.Run(false)}");
            NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.SpiRam, out uint totalSize1, out uint totalFreeSize1, out uint largestBlock1);
            Debug.WriteLine($"{msg}-> SpiRam Total Mem {totalSize1} Total Free {totalFreeSize1} Largest Block {largestBlock1}\n");
        }
    }
}
