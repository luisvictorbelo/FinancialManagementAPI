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

            if (account.UserId != userId) return Forbid();

            if (dto.Type == 0 && account.Balance > dto.Amount)
            {
                account.Balance -= dto.Amount;
                // Criar método PUT AccountController
            }
            else
            {
                account.Balance += dto.Amount;
                // Criar método PUT AccountController
            }

            var transaction = new Transaction
            {
                AccountId = dto.AccountId,
                Type = dto.Type,
                Amount = dto.Amount,
                Category = dto.Category,
                Date = dto.Date
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return Created("", new { transaction.Type, transaction.Amount, transaction.Category, transaction.Date });
        }
    }
}