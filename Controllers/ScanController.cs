using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.user;
using BugDetectorGP.Scans;
using BugDetectorGP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScanController : ControllerBase
    {
        private readonly string WebScanPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/WebScan"));
        private readonly string NetworkScanPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/NetworkScan"));
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserInfo> _userManager;
        private readonly TaskSchedulerService _taskSchedulerService;

        public ScanController(ApplicationDbContext context, UserManager<UserInfo> userManager, TaskSchedulerService taskSchedulerService)
        {
            _context = context;
            _userManager = userManager;
            _taskSchedulerService = taskSchedulerService;
        }

        [HttpPost("FreeWebScan")]
        public async Task<IActionResult> FreeWebScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Execute("bash " + WebScanPath + "/domain_free.sh " + model.url);
            return await ScanResult(result, model.url, "FreeWebScan");
        }

        [HttpPost("PremiumWebScan")]
        public async Task<IActionResult> PremiumWebScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
                return BadRequest("You must be logged in or your username is incorrect");
            var rolse = await _userManager.GetRolesAsync(findUser);
            if (rolse.Contains("PremiumUser")==false)
            {
                return BadRequest("you must be subscribe");
            }

            var result = await Execute("bash " + WebScanPath + "/domain_premium.sh " + model.url);
            return await ScanResult(result, model.url, "PremiumWebScan");
        }

        [HttpPost("FreeNetworkScan")]
        public async Task<IActionResult> FreeNetworkScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Execute("bash " + NetworkScanPath + "/free.sh " + model.url);
            return await ScanResult(result, model.url, "FreeNetworkScan");
        }

        [HttpPost("PremiumNetworkScan")]
        public async Task<IActionResult> PremiumNetworkScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
                return BadRequest("You must be logged in or your username is incorrect");
            var rolse = await _userManager.GetRolesAsync(findUser);
            if (rolse.Contains("PremiumUser") == false)
            {
                return BadRequest("you must be subscribe");
            }

            var result = await Execute("bash " + NetworkScanPath + "/premium.sh " + model.url);
            return await ScanResult(result, model.url, "PremiumNetworkScan");
        }

        [HttpPost("ScheduleFreeWebScan")]
        public async Task<IActionResult> ScheduleFreeWebScan(ScheduleScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.date < DateTime.Now.ToLocalTime())
            {
                return BadRequest("The specified time is in the past.");
            }

            var WebScan = new WebScan() { url = model.url };
            _taskSchedulerService.ScheduleTask(model.date, () => FreeWebScan(WebScan));
            return Ok("Free Web Scan scheduled successfully.");
        }

        [HttpPost("SchedulePremiumWebScan")]
        public async Task<IActionResult> SchedulePremiumWebScan(ScheduleScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if(model.date < DateTime.Now.ToLocalTime())
            {
                return BadRequest("The specified time is in the past.");
            }

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
                return BadRequest("You must be logged in or your username is incorrect");
            var rolse = await _userManager.GetRolesAsync(findUser);
            if (rolse.Contains("PremiumUser") == false)
            {
                return BadRequest("you must be subscribe");
            }

            var WebScan = new WebScan() { url = model.url };
            _taskSchedulerService.ScheduleTask(model.date, () => PremiumWebScan(WebScan));
            return Ok("Premium Web Scan scheduled successfully.");
        }

        [HttpPost("ScheduleFreeNetworkScan")]
        public async Task<IActionResult> ScheduleFreeNetworkScan(ScheduleScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.date < DateTime.Now.ToLocalTime())
            {
                return BadRequest("The specified time is in the past.");
            }

            var WebScan =new WebScan() { url = model.url };
            _taskSchedulerService.ScheduleTask(model.date, () => FreeNetworkScan(WebScan));
            return Ok("Free Network Scan scheduled successfully.");
        }

        [HttpPost("SchedulePremiumNetworkScan")]
        public async Task<IActionResult> SchedulePremiumNetworkScan(ScheduleScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.date < DateTime.Now.ToLocalTime())
            {
                return BadRequest("The specified time is in the past.");
            }

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
                return BadRequest("You must be logged in or your username is incorrect");
            var rolse = await _userManager.GetRolesAsync(findUser);
            if (rolse.Contains("PremiumUser") == false)
            {
                return BadRequest("you must be subscribe");
            }

            var WebScan = new WebScan() { url = model.url };
            _taskSchedulerService.ScheduleTask(model.date, () => PremiumNetworkScan(WebScan));
            return Ok("Premium Network Scan scheduled successfully.");
        }


        [HttpPost("ReturnReportsForUser")]
        public async Task<IActionResult> ReturnReportsForUser()
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
                return BadRequest("You must be logged in or your username is incorrect");

            var reportList = _context.Reports.Where(R => R.UserId == findUser.Id).ToList();
            var returnReports = reportList.Select(report => new ReportsInfoDTO
            {
                ReportId = report.ReportId,
                Targit = report.Target,
                Type = report.Type,
                DateTime = report.PublicationDate,
            }).ToList();

            return Ok(returnReports);
        }



        [HttpPost("ReturnOneReport")]
        public async Task<IActionResult> ReturnOneReport(ReportIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
                return BadRequest("You must be logged in or your username is incorrect");

            var report = await _context.Reports.SingleOrDefaultAsync(R => R.ReportId == model.Id);

            if (report == null)
                return BadRequest("The report was not found");

            if (report.UserId != findUser.Id)
                return BadRequest("You are not allowed to see this report!");

            return Ok(GenerateReport(report.Result, report.Type));
        }

        private async Task<bool> SaveReport(string result, string target, string type)
        {
            var report = new Reports
            {
                Result = result,
                Target = target,
                Type = type,
                UserId = await _userManager.FindByNameAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).ContinueWith(t => t.Result.Id),
                PublicationDate = DateTime.Now.ToLocalTime()
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<IActionResult> ScanResult(string result, string target, string type)
        {
            if (result.Contains("Error"))
                return BadRequest(result);

            if (!await SaveReport(result, target, type))
                return BadRequest("You must be logged in");
            
            
            return Ok(GenerateReport(result,type));
        }

        private async Task<dynamic> GenerateReport(string result, string type)
        {
            dynamic report;
            switch (type)
            {
                case "FreeWebScan":
                    report = await ReturnReports.FreeReport(result);
                    break;
                case "SourceCodeScan":
                    report = await ReturnReports.SourceCodeScanResult(result);
                    break;
                default:
                    report = await ReturnReports.PremiumReport(result);
                    break;
            }
            return report;
        }

        private async Task<string> Execute(string command)
        {
            var result = "";

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

                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    int errorIndex = output.IndexOf("Error");
                    if (errorIndex >= 0)
                    {
                        result = output.Substring(errorIndex);
                        return result;
                    }

                    result += output;
                }
            }
            catch (Exception ex)
            {
                result += $"Error: {ex.Message}";
            }

            return result;
        }
        
    }
}
