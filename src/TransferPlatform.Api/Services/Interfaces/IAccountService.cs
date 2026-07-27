using TransferPlatform.Data.Models;
using TransferPlatform.src.TransferPlatform.Api.DTOs;

namespace TransferPlatform.Api.Services.Interfaces;

public interface IAccountService
{
    Task<ApiResponse<AccountResponse?>> CreateAsync(CreateAccountRequest request);
    Task<ApiResponse<AccountResponse?>> GetAsync(string accountNo);
    Task<ApiResponse<List<AccountResponse>>> GetAllAsync();
}