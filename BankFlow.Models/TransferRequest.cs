
using System.ComponentModel.DataAnnotations;

namespace BankFlow.Models
{
    public class TransferRequest
    {
        [Range(1, int.MaxValue)]
        public int FromAccountId { get; set; }

        [Range(1, int.MaxValue)]
        public int ToAccountId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999.99")]
        public decimal Amount { get; set; }
    }
}
