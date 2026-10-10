
using System;

namespace BankFlow.Models
{
    public class BankTransaction
    {
        public int Id { get; set; }

        public int? FromAccountId { get; set; }

        public int? ToAccountId { get; set; }

        public string TransactionType { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
