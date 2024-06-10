using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class FreeScanResult
    {
        public List<FreeReportDto> result { get; set; }
    }
}