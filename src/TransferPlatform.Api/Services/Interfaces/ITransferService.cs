using TransferPlatform.Data.Models;
using TransferPlatform.src.TransferPlatform.Api.DTOs;

namespace TransferPlatform.Api.Services.Interfaces;

public interface ITransferService
{
    Task<ApiResponse<bool>> ProcessTransferAsync(TransferRequest request);
    Task<ApiResponse<List<LedgerResponse>>> GetAccountLedgerAsync(Guid accountId);
    Task<ApiResponse<bool>> FundAccount(FundRequest request);
}