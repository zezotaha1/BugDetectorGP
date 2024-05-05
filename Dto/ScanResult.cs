using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class ScanResult
    {
        public List<List<string>> result { get; set; }
    }
}