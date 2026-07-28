using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TransferPlatform.Api.Services.Interfaces;
using TransferPlatform.Data.Database;
using TransferPlatform.Data.Database.Queries;
using TransferPlatform.Data.Entities;
using TransferPlatform.Data.Models;
using TransferPlatform.src.TransferPlatform.Api.DTOs;

namespace TransferPlatform.Api.Services.Implementations;

public class TransferService : ITransferService
{
    private readonly AppDbContext _db;
    private readonly IDatabase _redis;

    public TransferService(AppDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis.GetDatabase();
    }

    public async Task<ApiResponse<bool>> ProcessTransferAsync(TransferRequest request)
    {
        ApiResponse<bool> response = new();
        var lockKey = $"lock:account:{request.FromAccountId}";
        var lockValue = Guid.NewGuid().ToString();

        // check request id doesn't exist
        var existing = await _db.LedgerEntries.AnyAsync(x => x.RequestId == request.RequestId);
        if (existing)
        {
            response.Message = "Duplicate request id";
            response.Status = false;
            response.Code = "409";
            return response;
        }


        // Lock expires automatically if the process crashes.
        var lockTaken = await _redis.LockTakeAsync(
            lockKey,
            lockValue,
            TimeSpan.FromSeconds(10));

        if (!lockTaken)
        {
            response.Message = "Another transfer is already being processed for this account.";
            response.Status = false;
            response.Code = "409";
            return response;
        }

        if (request.FromAccountId == request.ToAccountId)
        {
            response.Message = "Cannot transfer to the same account.";
            response.Status = false;
            response.Code = "400";
            return response;
        }

        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            Guid firstLockId;
            Guid secondLockId;

            if (request.FromAccountId.CompareTo(request.ToAccountId) < 0)
            {
                firstLockId = request.FromAccountId;
                secondLockId = request.ToAccountId;
            }
            else
            {
                firstLockId = request.ToAccountId;
                secondLockId = request.FromAccountId;
            }

            // Lock firstAccount row
            var firstAccount = await _db.Accounts
                .FromSqlRaw(SelectAccountForUpdate.PrepareQuery(_db), firstLockId)
                .SingleOrDefaultAsync();

            if (firstAccount == null)
            {
                Console.WriteLine($"First Account {firstLockId} not found");
                response.Message = "Account not found";
                response.Status = false;
                response.Code = "404";
                return response;
            }


            // Lock secondAccount row
            var secondAccount = await _db.Accounts
                .FromSqlRaw(SelectAccountForUpdate.PrepareQuery(_db), secondLockId)
                .SingleOrDefaultAsync();

            if (secondAccount == null)
            {
                Console.WriteLine($"Second Account {secondLockId} not found");
                response.Message = "Account not found";
                response.Status = false;
                response.Code = "404";
                return response;
            }

            var sender = firstAccount.Id == request.FromAccountId ? firstAccount : secondAccount;

            var receiver = firstAccount.Id == request.ToAccountId ? firstAccount : secondAccount;


            if (sender.Balance < request.Amount)
            {
                response.Message = "Insufficient funds";
                response.Status = false;
                response.Code = "400";
                return response;
            }

            sender.Balance -= request.Amount;
            receiver.Balance += request.Amount;

            _db.LedgerEntries.Add(new LedgerEntry
            {
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = request.Amount,
                RequestId = request.RequestId,
                Currency = string.IsNullOrEmpty(request.Currency) ? "NGN" : request.Currency
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            response.Message = "Transfer processed successfully";
            response.Status = true;
            response.Code = "200";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            response.Message = "An error occurred, Please try again later";
            response.Status = false;
            response.Code = "500";
        }
        finally
        {
            await _redis.LockReleaseAsync(lockKey, lockValue);
        }

        return response;
    }

    public async Task<ApiResponse<bool>> FundAccount(FundRequest request)
    {
        // can be used to put in the "initial" funds
        ApiResponse<bool> response = new();
        var accountId = request.AccountId;
        var lockKey = $"lock:account:{accountId}";
        var lockValue = Guid.NewGuid().ToString();

        // check request id doesn't exist
        var existing = await _db.LedgerEntries.AnyAsync(x => x.RequestId == request.RequestId);
        if (existing)
        {
            response.Message = "Duplicate request id";
            response.Status = false;
            response.Code = "409";
            return response;
        }

        var lockTaken = await _redis.LockTakeAsync(
            lockKey,
            lockValue,
            TimeSpan.FromSeconds(10));

        if (!lockTaken)
        {
            response.Message = "Another transfer is already being processed for this account.";
            response.Status = false;
            response.Code = "500";
            return response;
        }

        if (accountId == Guid.Empty)
        {
            response.Message = "AccountId cannot be empty";
            response.Status = false;
            response.Code = "400";
            return response;
        }

        try
        {
            await using var tx =
                await _db.Database.BeginTransactionAsync();

            // Lock row
            var receiver = await _db.Accounts
                .FromSqlRaw(SelectAccountForUpdate.PrepareQuery(_db), accountId)
                .SingleOrDefaultAsync();

            if (receiver == null)
            {
                response.Message = "Receiver Account not found";
                response.Status = false;
                response.Code = "404";
                return response;
            }


            receiver.Balance += request.Amount;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            response.Data = true;
            response.Message = "Account funded successfully";
            response.Status = true;
            response.Code = "200";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            response.Message = ex.Message;
            response.Status = false;
            response.Code = "500";
        }
        finally
        {
            await _redis.LockReleaseAsync(lockKey, lockValue);
        }

        return response;
    }

    public async Task<ApiResponse<List<LedgerResponse>>> GetAccountLedgerAsync(Guid accountId)
    {
        ApiResponse<List<LedgerResponse>> response = new();
        try
        {
            if (accountId == Guid.Empty)
            {
                response.Message = "AccountId cannot be empty";
                response.Status = false;
                response.Code = "400";
                return response;
            }
            // basic fetch all. currently no pagination support
            var records = await _db.LedgerEntries
                .AsNoTracking()
                .Where(x =>
                    x.FromAccountId == accountId ||
                    x.ToAccountId == accountId)
                .OrderByDescending(x => x.CreatedUtc)
                .Select(x => new LedgerResponse
                {
                    Id = x.Id,
                    FromAccountId = x.FromAccountId,
                    ToAccountId = x.ToAccountId,
                    Amount = x.Amount,
                    CreatedUtc = x.CreatedUtc,
                    Currency = x.Currency,
                    RequestId = x.RequestId
                })
                .ToListAsync();

            response.Data = records;
            response.Message = "Ledger fetched successfully";
            response.Status = true;
            response.Code = "200";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            response.Message = ex.Message;
            response.Status = false;
            response.Code = "500";
            response.Data = new List<LedgerResponse>();
        }

        return response;
    }
}