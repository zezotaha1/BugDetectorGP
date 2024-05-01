using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebScanController : ControllerBase
    {

        [HttpPost("URL")]
        public ActionResult Scan(string url)
        {
            string command = "bash free.sh " + url;
            try
            {
                using (var process = new Process())
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{command}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.StartInfo = startInfo;
                    process.Start();

                    var output = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();

                    return Ok(output);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("error" + ex.Message);
            }
        }
        
    }
}
