using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class PremiumScanResult
    {
        public List<PremiumReportDto> result { get; set; }
    }
}