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
        }

        public async Task<string> CreateOutPut(string file)
        {
            int methodCounter = 1;
            SolutionScanResults.Reverse();
            string result = "";
            foreach (var methodDeclaration in SolutionScanResults)
            {

                if (methodDeclaration.HasSQLInjection == true)
                {

                  
                    string htmlContent = methodDeclaration.MethodName;
                    htmlContent += '\n';
                    htmlContent += methodDeclaration.MethodBody;
                    FinalResult final = new FinalResult(htmlContent);
                    result += "##########" + file+ final.GetResults();
                    
                    


                    methodCounter++;
                }
            }
            return result;
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
