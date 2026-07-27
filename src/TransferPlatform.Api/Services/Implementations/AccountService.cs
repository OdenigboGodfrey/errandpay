using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TransferPlatform.Api.Services.Interfaces;
using TransferPlatform.Data.Database;
using TransferPlatform.Data.Entities;
using TransferPlatform.Data.Models;
using TransferPlatform.src.TransferPlatform.Api.DTOs;

namespace TransferPlatform.Api.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;

    public AccountService(AppDbContext db)
    {
        _db = db;
    }


    public async Task<ApiResponse<AccountResponse?>> CreateAsync(CreateAccountRequest request)
    {
        ApiResponse<AccountResponse?> response = new();
        try
        {
            if (request.OwnerName == null)
            {
                response.Message = "Account Owner Name cannot be empty";
                response.Status = false;
                response.Code = "400";
                return response;
            }

            var account = new Account
            {
                Id = Guid.NewGuid(),
                OwnerName = request.OwnerName,
                Balance = 0,
                Number = GenerateAccountNumber(),
            };


            _db.Accounts.Add(account);

            await _db.SaveChangesAsync();


            var record = new AccountResponse
            {
                Id = account.Id,
                OwnerName = account.OwnerName,
                Balance = account.Balance,
                Number = account.Number,
                CreatedUtc = DateTime.UtcNow,
            };

            response.Message = "Account created successfully";
            response.Status = true;
            response.Code = "201";
            response.Data = record;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            response.Message = ex.Message;
            response.Status = false;
            response.Code = "500";
        }

        return response;
    }

    public async Task<ApiResponse<AccountResponse?>> GetAsync(string accountNo)
    {
        ApiResponse<AccountResponse?> response = new();
        try
        {
            if (accountNo == null)
            {
                response.Message = "Account Number cannot be empty";
                response.Status = false;
                response.Code = "400";
                return response;
            }

            var account = await _db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Number == accountNo);


            if (account == null)
            {
                response.Message = "Account not found";
                response.Status = false;
                response.Code = "404";
                return response;
            }


            var record = new AccountResponse
            {
                Id = account.Id,
                OwnerName = account.OwnerName,
                Balance = account.Balance,
                Number = account.Number
            };

            response.Message = "Account found successfully";
            response.Status = true;
            response.Code = "200";
            response.Data = record;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            response.Message = ex.Message;
            response.Status = false;
            response.Code = "500";
        }

        return response;
    }

    public async Task<ApiResponse<List<AccountResponse>>> GetAllAsync()
    {
        ApiResponse<List<AccountResponse>> response = new();
        try
        {
            var account = _db.Accounts.AsNoTracking().OrderByDescending(x => x.CreatedUtc);


            var record = account.Select(x => new AccountResponse
            {
                Id = x.Id,
                OwnerName = x.OwnerName,
                Balance = x.Balance,
                Number = x.Number,
                CreatedUtc = x.CreatedUtc
            }).ToList();

            response.Message = "Accounts found successfully";
            response.Status = true;
            response.Code = "200";
            response.Data = record;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            response.Message = ex.Message;
            response.Status = false;
            response.Code = "500";
            response.Data = new List<AccountResponse>();
        }

        return response;
    }


    private static string GenerateAccountNumber()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);

        uint value = BitConverter.ToUInt32(bytes);

        // 0 - 9,999,999,999
        ulong accountNumber = value % 10_000_000_000UL;

        return accountNumber.ToString("D10");
    }
}
