using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BugDetectorGP.Scans
{
    public class Scan
    {
        protected string folderPath;

        public Scan(string folderPath)
        {
            this.folderPath = folderPath;
        }

        public async Task<string> AddNewFile(string fileName, string content)
        {
            try
            {
                string filePath = Path.Combine(folderPath, fileName);

                File.WriteAllText(filePath, content);
                return ("File added successfully.");
            }
            catch (IOException ex)
            {
                return ($"Error occurred: {ex.Message}");
            }
        }

        public async Task<string> RemoveFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(folderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return ("File removed successfully.");
                }
                else
                {
                    return ("File does not exist.");
                }
            }
            catch (IOException ex)
            {
                return ($"Error occurred: {ex.Message}");
            }
        }

        public async Task<List<List<string>>> _Scan (string targit)
        {
            var files = Directory.GetFiles(folderPath);
            var result = "";
            foreach (var file in files)
            {
                string command = "bash " +file + " " + targit;

                try
                {
                    using (var process = new Process())
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"{command}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        process.StartInfo = startInfo;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();

                        process.WaitForExit();

                        result += output;
                    }
                }
                catch (Exception ex)
                {
                    result += $"Error: {ex.Message}";
                }
                
            }

            List<List<string>> _output = new List<List<string>>();
            
            for (int i = 0; i < result.Length; i++)
            {
                string title = "", details = "", output = "";

                while ( i < result.Length && result[i] == '#') { i++; continue; }
                while ( i < result.Length && result[i] != '#') { title += result[i]; i++; continue; }
                while ( i < result.Length && result[i] == '#') { i++; continue; }
                while ( i < result.Length && result[i] != '#') { details += result[i]; i++; continue; }
                while ( i < result.Length && result[i] == '#') { i++; continue; }
                while ( i < result.Length && result[i] != '#') { output += result[i]; i++; continue; }

                _output.Add(new List<string> { title, details, output });
            }
            
            return _output;
        }
        
    }
}
