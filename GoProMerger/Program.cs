using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GoProMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = args[0];
            var files = Directory.EnumerateFiles(dir, "*.mp4");
            var listFilePath = Path.Combine(dir, "list.txt");
            var concatFilePath = Path.Combine(dir, "concat.bat");

            var index = 1;
            foreach (var file in files.ToList())
            {
                var startFileMatch = Regex.Match(file, "GOPR(\\d+)\\.MP4", RegexOptions.IgnoreCase);
                if (startFileMatch.Success)
                {
                    var startNumber = startFileMatch.Groups[1];
                    var fileSequence = new List<string> { file }.Concat(files
                        .Where(name => Regex.IsMatch(name, "GP\\d+" + startNumber + ".MP4", RegexOptions.IgnoreCase))
                        .OrderBy(n => n));

                    File.WriteAllLines(
                        listFilePath,
                        fileSequence.Select(name => "file " + Path.GetFileName(name)));

                    //File.WriteAllText(concatFilePath, $"ffmpeg -y -f concat -i list.txt -filter:v \"hflip, vflip\" -codec:a copy output{index++}.mp4");
                    File.WriteAllText(concatFilePath, $"ffmpeg -y -f concat -i list.txt -c copy output{index++}.mp4");
                    var processStartInfo = new ProcessStartInfo(concatFilePath);
                    processStartInfo.WorkingDirectory = dir;
                    var process = Process.Start(processStartInfo);
                    process.WaitForExit();
                }
            }
        }
    }
}
