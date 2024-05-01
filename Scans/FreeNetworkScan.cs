using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BugDetectorGP.Scans
{
    public class FreeNetworkScan : Scan
    {
        public FreeNetworkScan(string folderPath) : base(folderPath)
        {
            string executablePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string directoryPath = Path.GetDirectoryName(executablePath);
            folderPath = Path.Combine(directoryPath, "BugDetectorGP", "Scans", "FreeNetworkScan");
        }
    }
}
