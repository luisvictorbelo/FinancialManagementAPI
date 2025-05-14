using System.Security.Claims;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Enum;
using FinancialManagementAPI.Extensions;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("transaction")]
    [Authorize]
    public class TransactionController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
        {

            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var account = await _context.Accounts.FindAsync(dto.AccountId);
            if (account == null) return NotFound("Conta não encontrada");

            if (account.UserId != userId) return Forbid("Usuário não autorizado para acessar esta conta");

            bool isValid = ProcessTransactionOnCreate(account, dto);
            if (!isValid)
                return BadRequest("Erro! Tente novamente.");

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

            return CreatedAtAction(nameof(CreateTransaction), new
            {
                newTransaction.Type,
                newTransaction.Amount,
                newTransaction.Category,
                newTransaction.Date
            });
        }

        [HttpGet("{accountId}/transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromRoute] int accountId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] TypeTransaction? type,
            [FromQuery] string? category)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var account = await _context.Accounts.FindAsync(accountId);

            if (account == null)
                return NotFound("Conta não encontrada.");

            if (account.UserId != userId)
                return Forbid("Você não tem permissão para acessar as transações desta conta.");

            var query = _context.Transactions
               .Where(t => t.AccountId == accountId)
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
                    t.Type,
                    t.Amount,
                    t.Category,
                    t.Date
                }).ToListAsync();

            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById([FromRoute] int id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var transaction = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            return NotFound("Transação não encontrada.");

            if (transaction.Account?.UserId != userId)
            return Forbid("Você não tem permissão para acessar esta transação.");

            return Ok(new
            {
            transaction.Id,
            transaction.AccountId,
            transaction.Type,
            transaction.Amount,
            transaction.Category,
            transaction.Date
            });
        }



        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTransactionById([FromRoute] int id)
        {

            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");


            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound("Transação não encontrada.");

            if (transaction.Account?.UserId != userId)
                return Forbid("Você não tem permissão para deletar esta transação.");

            var account = await _context.Accounts.FindAsync(transaction.AccountId);
            if (account == null) return NotFound("Conta não encontrada");


            bool isValid = ProcessTransactionOnDelete(account, transaction);
            if (!isValid)
                return BadRequest("Houve um erro na sua operação.");

            _context.Accounts.Update(account);

            _context.Transactions.Remove(transaction);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateTransaction([FromRoute] int id, [FromBody] TransactionDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");


            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound("Transação não encontrada.");

            if (transaction.Account?.UserId != userId)
                return Forbid("Você não tem permissão para atualizar esta transação.");

            var newTransaction = new Transaction
            {
                AccountId = dto.AccountId,
                Type = dto.Type,
                Amount = dto.Amount,
                Category = dto.Category,
                Date = dto.Date
            };

            transaction.AccountId = newTransaction.AccountId;
            transaction.Type = newTransaction.Type;
            transaction.Amount = newTransaction.Amount;
            transaction.Category = newTransaction.Category;
            transaction.Date = newTransaction.Date;

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                transaction.Type,
                transaction.Amount,
                transaction.Category,
                transaction.Date
            });
        }


        private static bool ProcessTransactionOnCreate(Account account, TransactionDto dto)
        {
            if (dto.Type == TypeTransaction.Expense)
            {
                account.Balance -= dto.Amount;
                return true;
            }
            else if (dto.Type == TypeTransaction.Income)
            {
                account.Balance += dto.Amount;
                return true;
            }
            return false;
        }

        private static bool ProcessTransactionOnDelete(Account account, Transaction transaction)
        {
            if (transaction.Type == TypeTransaction.Expense)
            {
                account.Balance += transaction.Amount;
                return true;
            }
            else if (transaction.Type == TypeTransaction.Income)
            {
                account.Balance -= transaction.Amount;
                return true;
            }
            return false;
        }
    }
}