using System.Security.Claims;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    [Authorize]
    public class TransactionController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
        {

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");

            var account = await _context.Accounts.FindAsync(dto.AccountId);
            if (account == null) return NotFound("Conta não encontrada");

            if (account.UserId != userId) return Forbid("Usuário não autorizado para acessar esta conta");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool isValid = ProcessTransaction(account, dto);
                if (!isValid)
                    return BadRequest("Saldo insuficiente");

                _context.Accounts.Update(account);

                var newTransaction = new Transaction
                {
                    AccountId = dto.AccountId,
                    Type = dto.Type,
                    Amount = dto.Amount,
                    Category = dto.Category,
                    Date = dto.Date
                };

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(CreateTransaction), new
                {
                    newTransaction.Type,
                    newTransaction.Amount,
                    newTransaction.Category,
                    newTransaction.Date
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Erro ao processar a transação");
            }
        }

        private static bool ProcessTransaction(Account account, TransactionDto dto)
        {
            if (dto.Type == 0)
            {
                if (account.Balance < dto.Amount)
                    return false;
                
                account.Balance -= dto.Amount;
            }
            else
            {
                account.Balance += dto.Amount;
            }

            return true;
        }
    }
}