namespace TransferPlatform.Data.Models;

public class CreateAccountRequest
{
    public string OwnerName { get; set; } = string.Empty;
}

public class AccountResponse
{
    public Guid Id { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public decimal Balance { get; set; }
    public DateTime CreatedUtc { get; set; }
}
