
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransferPlatform.Api.Helpers;
using TransferPlatform.Api.Services.Implementations;
using TransferPlatform.Api.Services.Interfaces;
using TransferPlatform.Data.Models;
using TransferPlatform.src.TransferPlatform.Api.DTOs;

namespace TransferPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TransferPlatformController : ControllerBase
{
    private readonly ITransferService _service;
    private readonly ICacheWrapper _memoryCache;

    public TransferPlatformController(
        ITransferService transactionService, ICacheWrapper memoryCache)
    {
        _service = transactionService;
        _memoryCache = memoryCache;
    }

    [Authorize]
    [HttpPost("/webhook/transactions")]
    public async Task<IActionResult> PostTransaction([FromBody] TransferRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _service.ProcessTransferAsync(request);

            if (!response.Status)
            {
                return ResponseHelper.BuildResponse<bool>(response.Code, false, response.Message, false);
            }
            return ResponseHelper.BuildResponse<bool>("200", true, "Transfer processed successfully", true);
        }
        catch (Exception ex)
        {
            return ResponseHelper.BuildResponse<string>("500", false, ex.Message, null);
        }
    }

    [Authorize]
    [HttpPost("/fund")]
    public async Task<IActionResult> Fund([FromBody] FundRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _service.FundAccount(request);

            if (!response.Status)
            {
                return ResponseHelper.BuildResponse<bool>(response.Code, false, response.Message, false);
            }
            return ResponseHelper.BuildResponse<bool>("200", true, "Transfer processed successfully", true);
        }
        catch (Exception ex)
        {
            return ResponseHelper.BuildResponse<string>("500", false, ex.Message, null);
        }
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get(Guid accountId)
    {
        var ledger = await _service.GetAccountLedgerAsync(accountId);

        if (!ledger.Status)
        {
            return ResponseHelper.BuildResponse<List<LedgerResponse>>(ledger.Code, false,ledger.Message, null);
        }

        return ResponseHelper.BuildResponse<List<LedgerResponse>>("200", true, "Ledger fetched successfully", ledger.Data);

    }

}