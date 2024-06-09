using BugDetectorGP.Dto;
using BugDetectorGP.Scans;
using BugDetectorGP.Scans.SourceCode.input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SourceCodeScanController : ControllerBase
    {
        private string GithubRepos = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/SourceCode/GithubRepos"));

        [HttpPost("SourceCodeScan")]
        public async Task<IActionResult> SourceCodeScan(WebScan model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsGitHubRepositoryUrl(model.url))
            {
                return BadRequest("This URL is not a GitHub Repository URL!");
            }

            // Create a unique folder path
            string uniqueFolderName = GenerateUniqueFolderName();
            string repositoryPath = Path.Combine(GithubRepos, uniqueFolderName);

            // Ensure the folder exists
            CreateFolder(repositoryPath);

            // Clone the repository asynchronously
            bool cloneSuccess = await CloneGitHubRepositoryAsync(model.url, repositoryPath);
            if (!cloneSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cloning the repository.");
            }

            var result = new CreateInstanceOfAnylazer(repositoryPath);

            return Ok(result.output);
        }

        static bool IsGitHubRepositoryUrl(string url)
        {
            // Define a regular expression for matching GitHub repository URLs
            string pattern = @"^https:\/\/github\.com\/[^\/]+\/[^\/]+\/?$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(url);
        }

        static void CreateFolder(string path)
        {
            try
            {
                // Create the directory if it doesn't already exist
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        static string GenerateUniqueFolderName()
        {
            return Guid.NewGuid().ToString();
        }

        static async Task<bool> CloneGitHubRepositoryAsync(string repositoryUrl, string localPath)
        {
            string gitCloneCommand = $"git clone {repositoryUrl} {localPath}";

            return await Task.Run(() =>
            {
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/C {gitCloneCommand}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    Console.WriteLine(output);
                    Console.WriteLine(error);

                    return process.ExitCode == 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred while cloning the repository: {0}", e.ToString());
                    return false;
                }
            });
        }
    }
}
