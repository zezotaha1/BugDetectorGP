using BugDetectorGP.Models;
using BugDetectorGP.Scans;
using BugDetectorGP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ScanController : ControllerBase
    {
        private Scan _FreeWebScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans/FreeWebScan")));
        private Scan _PremiumWebScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans", "PremiumWebScan")));
        private Scan _FreeNetworkScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans", "FreeNetworkScan")));
        private Scan _PremiumNetworkScan = new Scan(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Scans", "PremiumNetworkScan")));

        
        [HttpPost("FreeWebScan")]

        public async Task<IActionResult> FreeWebScan(string Url)
        {
            var result = _FreeWebScan._Scan(Url);
            return Ok(result);
        }


        [HttpPost("FreeNetworkScan")]

        public async Task<IActionResult> FreeNetworkScan(string IP)
        {
            var result = _FreeNetworkScan._Scan(IP);
            return Ok(result);
        }


        [HttpPost("PremiumWebScan")]

        public async Task<IActionResult> PremiumWebScan(string Url)
        {
            var result = _PremiumWebScan._Scan(Url);
            return Ok(result);
        }


        [HttpPost("PremiumNetworkScan")]

        public async Task<IActionResult> PremiumNetworkScan(string IP)
        {
            var result = _PremiumNetworkScan._Scan(IP);
            return Ok(result);
        }
        

    
    }
}
