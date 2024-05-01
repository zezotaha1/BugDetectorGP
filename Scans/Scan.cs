using System.Collections.Generic;
using System.Diagnostics;
namespace BugDetectorGP.Scans
{
    public class Scan
    {
        string  folderPath;
        public void AddNewFile(string file)
        {
            
        }
        public void RemoveFile(string file)
        {
            
        }
        public string ExecuteCommands(string targit)
        {
            string[] files = Directory.GetFiles(folderPath);
            string result = "";
            foreach (var file in files)
            {
                string command = "bash " +file+" "+ targit;

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
                result += "\n\n\n\n\n";
            }
            return result;
        }
    }
}
