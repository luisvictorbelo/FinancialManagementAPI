using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/account")]
    public class AccountController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateAccount([FromBody] AccountDto dto)
        {

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");
            
            var user = await _context.Users.FindAsync(userId);
            if(user == null)
                return NotFound("Usuário não encontrado");
            
            var account = new Account
            {
                Name = dto.Name,
                Balance = dto.Balance,
                UserId = userId,
            };
            
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Created("", new { account.Id, account.Name, account.Balance, account.UserId });
        }
    }
}