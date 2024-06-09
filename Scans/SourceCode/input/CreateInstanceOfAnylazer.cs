using BugDetectorGP.Dto;
using BugDetectorGP.Scans.SourceCode.Anylazer;
using BugDetectorGP.Scans.SourceCode.output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugDetectorGP.Scans.SourceCode.input
{
    
    public class CreateInstanceOfAnylazer
    {
        string directoryPath;
        static Dictionary<string, int> visitedPath =
                     new Dictionary<string, int>();
        public SourceCodeScanResult output;

        public CreateInstanceOfAnylazer(string FilePath) {
            this.directoryPath = FilePath;
            output = new SourceCodeScanResult() { result = new List<SourceCodeReport> { } };
            DoScanAsync(FilePath);
        }
        public async Task DoScanAsync(string directoryPath)
        {

            try
            {
                visitedPath.Add(directoryPath, 1);
                string[] files = Directory.GetFiles(directoryPath, "*.cs");

                foreach (string file in files)
                {

                    Converter getFile = new Converter(file);
                    List<csScanResult> resultList = getFile.FinalResult();
                    OutputGenerator outputGen = new OutputGenerator(resultList);
                    var x = await outputGen.CreateHtmlOutputAsync();
                    var FilePath = file;
                    int Src = FilePath.IndexOf("src");
                    if (Src >= 0)
                    {
                        Src += 4;
                        FilePath = FilePath.Substring(Src);  
                    }
                    foreach (var i in x.result)
                    {

                        i.filepath = FilePath;
                        output.result.Add(i);
                    }
                }
                // create a recursive function to call all subpaths 
                string[] subDirectories = Directory.GetDirectories(directoryPath);
                foreach (string subDirectory in subDirectories)
                {
                    if (visitedPath.ContainsKey(subDirectory) != true)
                    {
                        DoScanAsync(subDirectory);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }
    }
}
