using Nito.HashAlgorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HashTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 1)
            {
                Console.WriteLine("Pass in only a single XCI file.");
                return;
            }

            // Locate files
            var xci = args[0];
            if (!xci.EndsWith(".xci"))
            {
                Console.WriteLine("Pass in only a single XCI file.");
                return;
            }

            string[] files;
            if (Path.IsPathRooted(args[0]))
                files = Directory.GetFiles(Path.GetDirectoryName(args[0]));
            else
                files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(args[0])));

            var gameName = Path.GetFileName(Regex.Match(xci, @".*?(?=\] )\]", RegexOptions.IgnoreCase).Value);

            var nullkeyarea = Path.Combine(AppContext.BaseDirectory, "nullkeyarea.bin");
            if (!File.Exists(nullkeyarea))
                nullkeyarea = Path.Combine(Directory.GetCurrentDirectory(), "nullkeyarea.bin");

            var pattern = $@"{gameName.Replace("[", "\\[").Replace("]", "\\]")} \(Initial Data\) \([0-9A-F]{{8}}\)\.bin";
            var initialArea = files.FirstOrDefault(f => Regex.IsMatch(Path.GetFileName(f), pattern));

            // Initial Area
            var xcOut = Path.Combine(Path.GetDirectoryName(xci), Path.GetFileNameWithoutExtension(xci) + ".xci.keyarealess.hashes.txt");
            var isOut = Path.Combine(Path.GetDirectoryName(initialArea), Path.GetFileNameWithoutExtension(initialArea) + ".bin.hashes.txt");
            var cmOut = Path.Combine(Path.GetDirectoryName(xci), Path.GetFileNameWithoutExtension(xci) + ".xci.keyarea.hashes.txt");

            HashFiles(new List<string> { initialArea }, isOut);
            HashFiles(new List<string> { xci }, xcOut);
            HashFiles(new List<string> { initialArea, nullkeyarea, xci }, cmOut);
        }

        /// <summary>
        /// Hashes a series of files concurrently.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="outFile"></param>
        /// <param name="BLOCK_SIZE"></param>
        static void HashFiles(List<string> files, string outFile, long BLOCK_SIZE = 0x100000)
        {
            var algos = CreateAlgorithms();

            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];

                using var br = new BinaryReader(File.OpenRead(f));

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var left = br.BaseStream.Length - br.BaseStream.Position;
                    var size = (int)Math.Min(BLOCK_SIZE, left);

                    var block = br.ReadBytes(size);

                    foreach (var a in algos)
                    {
                        if (left <= BLOCK_SIZE && i == files.Count - 1)
                            a.Value.TransformFinalBlock(block, 0, size);
                        else
                            a.Value.TransformBlock(block, 0, size, block, 0);
                    }
                }
            }

            // Output
            using var sw = new StreamWriter(File.Create(outFile), Encoding.ASCII);
            sw.WriteLine(Path.GetFileName(outFile));
            foreach (var a in algos)
                if (a.Value is CRC32 crc)
                    sw.WriteLine($"{a.Key}: {string.Concat(a.Value.Hash.Reverse().Select(x => x.ToString("X2"))).ToLower()}");
                else
                    sw.WriteLine($"{a.Key}: {string.Concat(a.Value.Hash.Select(x => x.ToString("X2"))).ToLower()}");
        }

        static Dictionary<string, HashAlgorithm> CreateAlgorithms()
        {
            return new Dictionary<string, HashAlgorithm>()
            {
                {"CRC", new CRC32(CRC32.Definition.Default) },
                {"MD5", MD5.Create() },
                {"SHA1", SHA1.Create() },
                {"SHA256", SHA256.Create() },
                {"SHA512", SHA512.Create() }
            };
        }
    }
}
