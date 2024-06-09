using BugDetectorGP.Dto;
using BugDetectorGP.Scans.SourceCode.Anylazer;
using GBugDetectorGP.Scans.SourceCode.output;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugDetectorGP.Scans.SourceCode.output
{
    public class OutputGenerator
    {
        public List<csScanResult> SolutionScanResults { get; set; } = new List<csScanResult>();

        public OutputGenerator(List<csScanResult> SolutionScanResults)
        {
            this.SolutionScanResults = SolutionScanResults;
            CreateOutPut();
        }


        // Edit this function to getApi mn chatGpt to know which files 
        public void CreateOutPut()
        {
            CreateHtmlOutputAsync();
        }

        public async Task<SourceCodeScanResult> CreateHtmlOutputAsync()
        {
            int methodCounter = 1;
            SolutionScanResults.Reverse();
            var ScanResultDTO = new SourceCodeScanResult() {result=new List<SourceCodeReport> { } };
            foreach (var methodDeclaration in SolutionScanResults)
            {
                // Create an HTML file for the method
                if (methodDeclaration.HasSQLInjection == true)
                {

                  
                    string htmlContent = methodDeclaration.MethodName;
                    htmlContent += '\n';
                    htmlContent += methodDeclaration.MethodBody;
                    FinalResult final = new FinalResult(htmlContent);
                    var results = final.GetResults();
                    var _SourceCodeReport = new SourceCodeReport()
                    {
                        InjectedFunction = results[0],
                        MitigationFunction = results[1],
                        Explanation = results[2]
                    };
                    ScanResultDTO.result.Add(_SourceCodeReport);


                    methodCounter++;
                }
            }
            return ScanResultDTO;
        }

        // Method to read the content of an HTML file
        private string ReadHtmlFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
