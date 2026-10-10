
using System;

namespace BankFlow.Models
{
    public class BankAccount
    {
        public int Id { get; set; }

        public string AccountNumber { get; set; }
            = string.Empty;

        public string AccountHolderName { get; set; }
            = string.Empty;

        public decimal Balance { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();
    }
}
