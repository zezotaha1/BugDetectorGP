using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.user;
using BugDetectorGP.Scans;
using BugDetectorGP.Scans.SourceCode.input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
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
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<UserInfo> _userManager;

        public SourceCodeScanController(ApplicationDbContext Context, UserManager<UserInfo> userManager)
        {
            _Context = Context;
            _userManager = userManager;
        }

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

            // Perform the scan
            var result = new CreateInstanceOfAnylazer(repositoryPath);
            if (!await SaveReport(result.output, model.url, "SourceCodeScan"))
                return BadRequest("You must be logged in");
            // Clean up the folder after the scan
            DeleteFolder(repositoryPath);

            return Ok(ReturnReports.SourceCodeScanResult(result.output)); 
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

        static void DeleteFolder(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    Console.WriteLine("Directory deleted successfully at {0}", path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }
        private async Task<bool> SaveReport(string result, string target, string type)
        {
            var report = new Reports();

            report.Result = result;
            report.Target = target;
            report.Type = type;

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userName == null)
            {
                return false;
            }

            var user = await _userManager.FindByNameAsync(userName);
            report.UserId = user.Id;
            report.PublicationDate = DateTime.Now.ToLocalTime();
            _Context.Reports.Add(report);
            _Context.SaveChanges();
            return true;
        }
    }
}
