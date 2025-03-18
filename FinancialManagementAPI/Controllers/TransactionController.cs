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
                bool isValid = ProcessTransactionOnCreate(account, dto);
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



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction([FromRoute] int id)
        {

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado.");


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