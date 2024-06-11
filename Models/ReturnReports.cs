using BugDetectorGP.Dto;

namespace BugDetectorGP.Models
{
    public class ReturnReports
    {
        public async static Task<PremiumScanResult> FreeReport(string ReportResult)
        {
            var result = new List<PremiumReportDto>();

            for (int i = 0; i < ReportResult.Length; i++)
            {
                string title = "", details = "", output = "";

                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { title += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { output += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { details += ReportResult[i]; i++; continue; }

                result.Add(new PremiumReportDto
                {
                    title = title,
                    details = details,
                    output = output,
                    mitigation = "mitigation for Premium"
                });
            }

            return new PremiumScanResult() { result = result };
        }
        public async static Task<PremiumScanResult> PremiumReport(string ReportResult)
        {
            var result = new List<PremiumReportDto>();

            for (int i = 0; i < ReportResult.Length; i++)
            {
                string title = "", details = "", output = "", mitigation="";

                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { title += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { output += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { details += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '√') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '√') { mitigation += ReportResult[i]; i++; continue; }

                result.Add(new PremiumReportDto
                {
                    title = title,
                    details = details,
                    output = output,
                    mitigation = mitigation
                });
            }

            return new PremiumScanResult() { result = result };
        }

        public async static Task<SourceCodeScanResult> SourceCodeScanResult(string ReportResult)
        {
            var result = new List<SourceCodeReport>();

            for (int i = 0; i < ReportResult.Length; i++)
            {
                string filepath = "", InjectedFunction = "", MitigationFunction = "", Explanation = "";

                while (i < ReportResult.Length && ReportResult[i] == '#') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '#') { filepath += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '#') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '#') { InjectedFunction += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '#') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '#') { MitigationFunction += ReportResult[i]; i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] == '#') { i++; continue; }
                while (i < ReportResult.Length && ReportResult[i] != '#') { Explanation += ReportResult[i]; i++; continue; }

                result.Add(new SourceCodeReport
                {
                    filepath = filepath,
                    InjectedFunction =InjectedFunction,
                    MitigationFunction =MitigationFunction,
                    Explanation = Explanation
                });
            }

            return new SourceCodeScanResult() { result = result };
        }
    }
}
