namespace TransferPlatform.Data.Entities;

public class LedgerEntry
{
    public Guid Id { get; set; }

    public Guid FromAccountId { get; set; }

    public Guid ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;
    public string RequestId { get; set; } = default!;

    public DateTime CreatedUtc { get; set; }

    public Account? FromAccount { get; set; }

    public Account? ToAccount { get; set; }
}
