using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class SourceCodeScanResult
    {
        public List<SourceCodeReport> result { get; set; }
    }
}