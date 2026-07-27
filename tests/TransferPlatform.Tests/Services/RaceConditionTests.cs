using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using TransferPlatform.Api.Services.Implementations;
using TransferPlatform.Data.Database;
using TransferPlatform.Data.Entities;
using TransferPlatform.Data.Models;

public class RaceConditionTests
{
    [Fact]
    public async Task Should_Reject_When_Redis_Lock_Is_Not_Acquired()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            await pragma.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        db.Accounts.AddRange(
            new Account
            {
                Id = Guid.NewGuid(),
                OwnerName = "John",
                Number = "1234567890",
                Balance = 100
            },
            new Account
            {
                Id = Guid.NewGuid(),
                OwnerName = "Ade",
                Number = "0987654321",
                Balance = 0
            });

        await db.SaveChangesAsync();

        var redis = new Mock<IDatabase>();

        redis.Setup(x => x.LockTakeAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var multiplexer = new Mock<IConnectionMultiplexer>();

        multiplexer
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redis.Object);

        var service = new TransferService(db, multiplexer.Object);

        var sender = await db.Accounts.FirstAsync();
        var receiver = await db.Accounts.Skip(1).FirstAsync();

        var result = await service.ProcessTransferAsync(new TransferRequest
        {
            FromAccountId = sender.Id,
            ToAccountId = receiver.Id,
            Amount = 100
        });

        result.Should().NotBeNull();
        result.Status.Should().BeFalse();
        result.Code.Should().Be("409");
    }
}