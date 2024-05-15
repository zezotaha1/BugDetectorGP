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
        private static ProfileDataController _ProfileData = new ProfileDataController();

        public ScanController(ApplicationDbContext Context, UserManager<UserInfo> userManager)
        {
            _Context = Context;
            _userManager = userManager;
        }

        [HttpPost("FreeWebScan")]

        public async Task<ScanResult> FreeWebScan(WebScan model)
        {
            var result = await _FreeWebScan._Scan(model.url);
            //await SaveReport(result, model.url, "WebScan");
            return new ScanResult()
            { 
                result = await Scan.ReturnWebOrNetworkReport( result) 
            };
        }


        [HttpPost("FreeNetworkScan")]

        public async Task<ScanResult> FreeNetworkScan(WebScan model)
        {
            var result = await _FreeNetworkScan._Scan(model.url);

            //await SaveReport(result, model.url, "NetworkScan");
            return new ScanResult()
            { result = await Scan.ReturnWebOrNetworkReport(result) };
        }


        [HttpPost("PremiumWebScan")]

        public async Task<ScanResult> PremiumWebScan(WebScan model)
        {
            var result = await _PremiumWebScan._Scan(model.url);

            //await SaveReport(result, model.url, "WebScan");

            return new ScanResult()
            { result = await Scan.ReturnWebOrNetworkReport(result) };
        }


        [HttpPost("PremiumNetworkScan")]

        public async Task<ScanResult> PremiumNetworkScan(WebScan model)
        {
            var result = await _PremiumNetworkScan._Scan(model.url);

            //await SaveReport(result, model.url, "NetworkScan");
            return new ScanResult()
            { result = await Scan.ReturnWebOrNetworkReport(result) };
        }


        private async Task<bool> SaveReport(string result ,string target,string type)
        {
            var report = new Reports();

            report.Result = result;
            report.Target = target;
            report.Type = type;

            var UserProfile = await _ProfileData.GetUserProfile();
            var user = await _userManager.FindByEmailAsync(UserProfile.Email);
            report.UserId = user.Id;
            _Context.Reports.Add(report);
            _Context.SaveChanges();
            return true;
        }

    }
}
