using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BugDetectorGP.Scans
{
    public class PremiumNetworkScan : Scan
    {
        public PremiumNetworkScan(string folderPath) : base(folderPath)
        {
            string executablePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string directoryPath = Path.GetDirectoryName(executablePath);
            folderPath = Path.Combine(directoryPath, "BugDetectorGP", "Scans", "PremiumNetworkScan");
        }
    }
}
