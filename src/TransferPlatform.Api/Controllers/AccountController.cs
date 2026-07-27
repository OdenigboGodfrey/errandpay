using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransferPlatform.Api.Helpers;
using TransferPlatform.Api.Services;
using TransferPlatform.Api.Services.Interfaces;
using TransferPlatform.Data.Models;
using TransferPlatform.src.TransferPlatform.Api.DTOs;


namespace TransferPlatform.Api.Controllers;


[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;
    private readonly ICacheWrapper _memoryCache;


    public AccountsController(IAccountService service, ICacheWrapper memoryCache)
    {
        _service = service;
        _memoryCache = memoryCache;
    }



    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Status)
        {
            return ResponseHelper.BuildResponse<AccountResponse>(result.Code, false, result.Message, null);
        }

        return ResponseHelper.BuildResponse<AccountResponse>("201", true, "Account created successfully", result.Data);
    }



    [Authorize]
    [HttpGet("get/{id}")]
    public async Task<IActionResult> Get(string accountNo)
    {
        ApiResponse<AccountResponse> response = new();
        if (!_memoryCache.TryGetValue(accountNo, out ApiResponse<AccountResponse> accountResponse))
        {
            var account = await _service.GetAsync(accountNo);

            if (account == null)
            {
                return ResponseHelper.BuildResponse<AccountResponse>("404", false, "Account not found", null);
            }
            else
            {
                _memoryCache.Set(accountNo, account, DateTime.Now.AddMinutes(15));
                accountResponse = account;
            }
        }

        return ResponseHelper.BuildResponse<AccountResponse>("200", true, "Account found successfully", accountResponse.Data);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return ResponseHelper.BuildResponse<List<AccountResponse>>("200", true, "Request processed successfully", result.Data);
    }
}
