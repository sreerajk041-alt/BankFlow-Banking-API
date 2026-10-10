using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace BankFlow.Models
{

    public class WithdrawalRequest
    {
        [Range(typeof(decimal), "0.01",
            "79228162514264337593543950335")]
        public decimal Amount { get; set; }
    }
}
