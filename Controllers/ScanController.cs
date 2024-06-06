using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.user;
using BugDetectorGP.Scans;
using BugDetectorGP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScanController : ControllerBase
    {
        private Scan Scan = new Scan();
        private string WebScanPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/WebScan"));
        private string NetworkScanPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/NetworkScan"));
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<UserInfo> _userManager;

        public ScanController(ApplicationDbContext Context, UserManager<UserInfo> userManager)
        {
            _Context = Context;
            _userManager = userManager;
        }

        [HttpPost("FreeWebScan")]

        public async Task<IActionResult> FreeWebScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Scan._Scan( "bash "+WebScanPath + "domain_free.sh "+model.url);

            return await ScanResult(result,model.url, "WebScan");
        }

        [HttpPost("PremiumWebScan")]

        public async Task<IActionResult> PremiumWebScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Scan._Scan("Python3 " + WebScanPath + "url_free.py " + model.url);

            return await ScanResult(result, model.url, "WebScan");
        }

        /*[HttpPost("FreeNetworkScan")]

        public async Task<IActionResult> FreeNetworkScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Scan._Scan("bash " + WebScanPath + "domain_free.sh " + model.url);

            return await ScanResult(result, model.url, "NetworkScan");
        }

        [HttpPost("PremiumNetworkScan")]

        public async Task<IActionResult> PremiumNetworkScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _PremiumNetworkScan._Scan(model.url);

            return await ScanResult(result, model.url, "NetworkScan");
        }*/


        [HttpPost("ReturnReportsForUser")]
        public async Task<IActionResult> ReturnReportsForUser()
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var reportlist = _Context.Reports.ToList().Where(R=>R.UserId==findUser.Id);
            var returnReports = new List<ReportsInfoDTO>() { };
            foreach(var report in reportlist)
            {
                returnReports.Add(new ReportsInfoDTO()
                {
                    ReportId=report.ReportId,
                    Targit=report.Target,
                    Type=report.Type,
                    DateTime=report.PublicationDate,
                });
            }
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
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var report = await _Context.Reports.SingleOrDefaultAsync(R => R.ReportId == model.Id);

            if (report == null)
                return BadRequest("The Report Not Found");
            
            if (report.UserId != findUser.Id)
                return BadRequest("You are not allowed to see this Report!");
                
            return Ok(new ScanResult()
            {
                result = await Scan.ReturnWebOrNetworkReport(report.Result)
            });
        }

        private async Task<bool> SaveReport(string result ,string target,string type)
        {
            var report = new Reports();

            report.Result = result;
            report.Target = target;
            report.Type = type;

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userName == null)
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

        private async Task<IActionResult> ScanResult(string result,string target,string type)
        {
            if (result.Contains("Error"))
            {
                return BadRequest(result);
            }

            if (await SaveReport(result, target, type) == false)
            {
                return BadRequest("You mast be login");
            }

            return Ok(new ScanResult()
            {
                result = await Scan.ReturnWebOrNetworkReport(result)
            });
        }
    }
}
