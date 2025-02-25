using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    public class TransactionController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto) {
            var account = await _context.Accounts.FindAsync(dto.AccountId);
            if (account == null) return NotFound("Conta não encontrada");
            if (dto.Type == 0 && account.Balance > dto.Amount) {
                account.Balance -= dto.Amount;
            }
            else {
                account.Balance += dto.Amount;
            }

            var transaction = new Transaction {
                AccountId = dto.AccountId,
                Account = account,
                Type = dto.Type,
                Amount = dto.Amount,
                Category = dto.Category,
                Date = dto.Date
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Created("", new { transaction.Account, transaction.Type, transaction.Amount, transaction.Category, transaction.Date});
        }
    }
}