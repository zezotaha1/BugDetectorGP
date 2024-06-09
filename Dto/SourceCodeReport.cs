namespace BugDetectorGP.Dto
{
    public class SourceCodeReport
    {
        public string filepath { get; set; } 
        public string InjectedFunction {  get; set; }
        public string MitigationFunction { get; set; }
        public string Explanation { get; set; }

    }
}
