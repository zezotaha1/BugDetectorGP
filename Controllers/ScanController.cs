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
    //[Authorize]
    public class ScanController : ControllerBase
    {
        private Scan _FreeWebScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/FreeWebScan")));
        private Scan _PremiumWebScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans", "PremiumWebScan")));
        private Scan _FreeNetworkScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans", "FreeNetworkScan")));
        private Scan _PremiumNetworkScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans", "PremiumNetworkScan")));
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

            var result = await _FreeWebScan._Scan(model.url);
            //await SaveReport(result, model.url, "WebScan");
            return Ok(new ScanResult()
            {
                result = await Scan.ReturnWebOrNetworkReport(result)
            });
        }


        [HttpPost("FreeNetworkScan")]

        public async Task<IActionResult> FreeNetworkScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _FreeNetworkScan._Scan(model.url);

            //await SaveReport(result, model.url, "NetworkScan");
            return Ok(new ScanResult()
            {
                result = await Scan.ReturnWebOrNetworkReport(result)
            });
        }


        [HttpPost("PremiumWebScan")]

        public async Task<IActionResult> PremiumWebScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _PremiumWebScan._Scan(model.url);

            //await SaveReport(result, model.url, "WebScan");

            return Ok(new ScanResult()
            {
                result = await Scan.ReturnWebOrNetworkReport(result)
            });
        }


        [HttpPost("PremiumNetworkScan")]

        public async Task<IActionResult> PremiumNetworkScan(WebScan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _PremiumNetworkScan._Scan(model.url);

            //await SaveReport(result, model.url, "NetworkScan");
            return Ok(new ScanResult()
            {
                result = await Scan.ReturnWebOrNetworkReport(result)
            });
        }

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
            var user = await _userManager.FindByEmailAsync(userName);
            report.UserId = user.Id;
            report.PublicationDate = DateTime.Now.ToLocalTime();
            _Context.Reports.Add(report);
            _Context.SaveChanges();
            return true;
        }

    }
}
