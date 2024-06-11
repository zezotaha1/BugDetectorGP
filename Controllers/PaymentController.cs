using BugDetectorGP.Dto;
using BugDetectorGP.Models.user;
using BugDetectorGP.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BugDetectorGP.Models.blog;
using BugDetectorGP.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _Context;
    private readonly UserManager<UserInfo> _userManager;

    public PaymentController(IHttpClientFactory httpClientFactory, ApplicationDbContext Context, UserManager<UserInfo> userManager)
    {
        _httpClientFactory = httpClientFactory;
        _Context = Context;
        _userManager = userManager;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] TransactionRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Your profile server key");
        client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        var jsonContent = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://secure-egypt.paytabs.com/payment/request", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var transactionResponse = JsonConvert.DeserializeObject<TransactionResponse>(responseContent);
            if (transactionResponse.RedirectUrl != null)
            {
                // Redirect the customer if necessary
                return Redirect(transactionResponse.RedirectUrl);
            }

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ChangeUserRole(userName);

            return Ok(transactionResponse);
        }

        return BadRequest(responseContent);
    }
    private async Task ChangeUserRole(string userName)
    {
        var findUser = await _userManager.FindByNameAsync(userName);

        var userRole =_Context.UserRoles.FirstOrDefaultAsync(u=>u.UserId==findUser.Id);
        //_Context.UserRoles.Remove(userRole);
        
    }
}
