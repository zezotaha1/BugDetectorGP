using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class ScanResult
    {
        public List<ReportDto> result { get; set; }
    }
}