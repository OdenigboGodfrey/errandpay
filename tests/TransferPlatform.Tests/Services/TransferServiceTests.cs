using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using TransferPlatform.Api.Services.Implementations;
using TransferPlatform.Data.Database;
using TransferPlatform.Data.Entities;
using TransferPlatform.Data.Models;
using Xunit;

public class TransferServiceTests
{
    [Fact]
    public async Task Should_Transfer_Money()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA foreign_keys = ON;";
            await command.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        await using var db = new AppDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var sender = new Account
        {
            Id = Guid.NewGuid(),
            OwnerName = "John",
            Number = "1234567890",
            Balance = 100
        };

        var receiver = new Account
        {
            Id = Guid.NewGuid(),
            OwnerName = "Ade",
            Number = "0987654321",
            Balance = 0
        };

        db.Accounts.AddRange(sender, receiver);
        await db.SaveChangesAsync();

        var redis = new Mock<IDatabase>();

        redis.Setup(x => x.LockTakeAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        redis.Setup(x => x.LockReleaseAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var multiplexer = new Mock<IConnectionMultiplexer>();

        multiplexer
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redis.Object);

        var service = new TransferService(db, multiplexer.Object);

        var result = await service.ProcessTransferAsync(new TransferRequest
        {
            FromAccountId = sender.Id,
            ToAccountId = receiver.Id,
            Amount = 25,
            RequestId = "123",
        });

        result.Status.Should().BeTrue();
        result.Code.Should().Be("200");
        sender.Balance.Should().Be(75);
        receiver.Balance.Should().Be(25);

        var ledgerCount = await db.LedgerEntries.CountAsync();
        ledgerCount.Should().Be(1);
    }
}