using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Enum;
using FinancialManagementAPI.Extensions;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("accounts")]
    [Authorize]
    public class AccountController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateAccount([FromBody] AccountDto dto)
        {

            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");

            var newAccount = new Account
            {
                Name = dto.Name,
                Balance = dto.Balance,
                UserId = (int)userId,
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Created("", new { newAccount.Id, newAccount.Name, newAccount.Balance, newAccount.UserId });
        }

        [HttpGet()]
        public async Task<IActionResult> GetUserAccounts()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");

            var accounts = await _context.Accounts
                .Where(x => x.UserId == userId)
                .Select(x => new { x.Id, x.Name, x.Balance })
                .ToListAsync();

            return Ok(accounts);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int id)
        {

            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Usuário não autenticado.");


            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
                return NotFound("Conta não encontrada.");

            if (account.UserId != userId)
                return Forbid("Você não tem permissão para deletar esta conta.");

            _context.Accounts.Remove(account);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}