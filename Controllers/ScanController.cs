﻿using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Scans;
using BugDetectorGP.Services;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<ScanResult> FreeWebScan(WebScan model)
        {
            return new ScanResult()
            { result = await _FreeWebScan._Scan(model.url) };
        }


        [HttpPost("FreeNetworkScan")]

        public async Task<ScanResult> FreeNetworkScan(WebScan model)
        {
            return new ScanResult()
            { result = await _FreeNetworkScan._Scan(model.url) };
        }


        [HttpPost("PremiumWebScan")]

        public async Task<ScanResult> PremiumWebScan(WebScan model)
        {
            return new ScanResult()
            { result = await _PremiumWebScan._Scan(model.url) };
        }


        [HttpPost("PremiumNetworkScan")]

        public async Task<ScanResult> PremiumNetworkScan(WebScan model)
        {
            return new ScanResult()
            { result = await _PremiumNetworkScan._Scan(model.url) };
        }
        

    
    }
}
