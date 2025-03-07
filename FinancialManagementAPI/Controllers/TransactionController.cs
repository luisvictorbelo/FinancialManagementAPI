using System.Security.Claims;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Enum;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("transactions")]
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

        [HttpGet]
        public async Task<ActionResult> GetUserTransactions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] TypeTransaction? type,
            [FromQuery] string? category)
        {   
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");

            var query =  _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => EF.Functions.Like(t.Category.ToLower(), $"%{category}"));

            var transactions = await query
                .Select(t => new
                {
                t.Id,
                AccountName = t.Account.Name,
                t.Type,
                t.Amount,
                t.Category,
                t.Date
                })
                .ToListAsync();
            
            return Ok(transactions);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromBody] int accountId) {
            
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");

            
            var account = await _context.Accounts.FirstOrDefaultAsync(account => account.UserId == userId && account.Id == accountId);

            if (account == null)
                return NotFound();

            var transaction = await _context.Transactions.FirstOrDefaultAsync(transaction => transaction.AccountId == account.Id);
            
            if (transaction == null)
                return NotFound();
            
            _context.Transactions.Remove(transaction);

            await _context.SaveChangesAsync();

            return NoContent();
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