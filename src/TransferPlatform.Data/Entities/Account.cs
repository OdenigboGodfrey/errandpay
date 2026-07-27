namespace TransferPlatform.Data.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public ICollection<LedgerEntry> OutgoingTransfers { get; set; }
        = new List<LedgerEntry>();
    public ICollection<LedgerEntry> IncomingTransfers { get; set; }
        = new List<LedgerEntry>();
}