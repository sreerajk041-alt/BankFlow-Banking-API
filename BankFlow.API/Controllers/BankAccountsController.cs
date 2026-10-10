
using BankFlow.Data;
using BankFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BankFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountsController : ControllerBase
    {
        private readonly BankFlowDbContext _context;

        private const decimal MaxBalance =
            9999999999999999.99m;

        public BankAccountsController(BankFlowDbContext context)
        {
            _context = context;
        }

        // GET: api/BankAccounts
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _context.BankAccounts
                .AsNoTracking()
                .ToListAsync();

            return Ok(accounts);
        }

        // GET: api/BankAccounts/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var account = await _context.BankAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
                return NotFound("Account not found.");

            return Ok(account);
        }

        // POST: api/BankAccounts
        [HttpPost]
        public async Task<IActionResult> CreateAccount(
            [FromBody] CreateBankAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber) ||
                string.IsNullOrWhiteSpace(request.AccountHolderName))
            {
                return BadRequest("Account details are required.");
            }

            var accountNumber = request.AccountNumber.Trim();

            bool exists = await _context.BankAccounts
                .AnyAsync(a => a.AccountNumber == accountNumber);

            if (exists)
                return Conflict("Account number already exists.");

            var account = new BankAccount
            {
                AccountNumber = accountNumber,
                AccountHolderName =
                    request.AccountHolderName.Trim(),
                Balance = 0m,
                CreatedAt = DateTime.UtcNow
            };

            _context.BankAccounts.Add(account);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Unable to create account.");
            }

            return CreatedAtAction(
                nameof(GetAccount),
                new { id = account.Id },
                account);
        }

        // POST: api/BankAccounts/1/deposit
        [HttpPost("{id:int}/deposit")]
        public async Task<IActionResult> Deposit(
            int id,
            [FromBody] DepositRequest request)
        {
            if (request.Amount <= 0 ||
                request.Amount > MaxBalance)
            {
                return BadRequest("Invalid deposit amount.");
            }

            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
                return NotFound("Bank account not found.");

            if (account.Balance > MaxBalance - request.Amount)
                return BadRequest("Deposit exceeds balance limit.");

            account.Balance += request.Amount;

            _context.BankTransactions.Add(new BankTransaction
            {
                ToAccountId = id,
                TransactionType = "Deposit",
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                // Both changes are saved atomically.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(
                    "Account balance changed. Please retry.");
            }

            return Ok(new
            {
                Message = "Deposit successful",
                AccountNumber = account.AccountNumber,
                DepositedAmount = request.Amount,
                NewBalance = account.Balance
            });
        }

        // POST: api/BankAccounts/1/withdraw
        [HttpPost("{id:int}/withdraw")]
        public async Task<IActionResult> Withdraw(
            int id,
            [FromBody] WithdrawalRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Invalid withdrawal amount.");

            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
                return NotFound("Bank account not found.");

            if (account.Balance < request.Amount)
                return BadRequest("Insufficient balance.");

            account.Balance -= request.Amount;

            _context.BankTransactions.Add(new BankTransaction
            {
                FromAccountId = id,
                TransactionType = "Withdrawal",
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(
                    "Account balance changed. Please retry.");
            }

            return Ok(new
            {
                Message = "Withdrawal successful",
                AccountNumber = account.AccountNumber,
                WithdrawnAmount = request.Amount,
                NewBalance = account.Balance
            });
        }

        // POST: api/BankAccounts/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(
            [FromBody] TransferRequest request)
        {
            if (request.Amount <= 0 ||
                request.Amount > MaxBalance)
                return BadRequest("Invalid transfer amount.");

            if (request.FromAccountId <= 0 ||
                request.ToAccountId <= 0)
                return BadRequest("Invalid account ID.");

            if (request.FromAccountId == request.ToAccountId)
                return BadRequest(
                    "Cannot transfer to the same account.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var accounts = await _context.BankAccounts
                    .Where(a =>
                        a.Id == request.FromAccountId ||
                        a.Id == request.ToAccountId)
                    .ToListAsync();

                var sender = accounts.FirstOrDefault(
                    a => a.Id == request.FromAccountId);

                var receiver = accounts.FirstOrDefault(
                    a => a.Id == request.ToAccountId);

                if (sender == null || receiver == null)
                    return NotFound(
                        "One or both accounts not found.");

                if (sender.Balance < request.Amount)
                    return BadRequest("Insufficient balance.");

                if (receiver.Balance >
                    MaxBalance - request.Amount)
                    return BadRequest(
                        "Receiver balance limit exceeded.");

                sender.Balance -= request.Amount;
                receiver.Balance += request.Amount;

                _context.BankTransactions.Add(
                    new BankTransaction
                    {
                        FromAccountId = sender.Id,
                        ToAccountId = receiver.Id,
                        TransactionType = "Transfer",
                        Amount = request.Amount,
                        CreatedAt = DateTime.UtcNow
                    });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Transfer successful",
                    FromAccount = sender.AccountNumber,
                    ToAccount = receiver.AccountNumber,
                    TransferredAmount = request.Amount,
                    SenderBalance = sender.Balance,
                    ReceiverBalance = receiver.Balance
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                return Conflict(
                    "Account balance changed. Please retry.");
            }
        }

        // GET: api/BankAccounts/1/transactions
        [HttpGet("{id:int}/transactions")]
        public async Task<IActionResult> GetTransactionHistory(
            int id)
        {
            bool exists = await _context.BankAccounts
                .AnyAsync(a => a.Id == id);

            if (!exists)
                return NotFound("Bank account not found.");

            var transactions = await _context.BankTransactions
                .AsNoTracking()
                .Where(t =>
                    t.FromAccountId == id ||
                    t.ToAccountId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return Ok(transactions);
        }
    }

    public class CreateBankAccountRequest
    {
        [Required]
        [StringLength(30)]
        public string AccountNumber { get; set; }
            = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccountHolderName { get; set; }
            = string.Empty;
    }
}
