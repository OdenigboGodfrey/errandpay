using System.ComponentModel.DataAnnotations;

namespace TransferPlatform.Data.Models
{
    public class TransferRequest
    {
        [Required]
        public Guid FromAccountId { get; set; }
        [Required]
        public Guid ToAccountId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Currency { get; set; } = "NGN";
        [Required]
        public string RequestId { get; set; } = default!;
    }

    public class FundRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Currency { get; set; } = "NGN";
        [Required]
        public string RequestId { get; set; } = default!;
    }

    public class LedgerResponse
    {
        public Guid Id { get; set; }
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
    }
}