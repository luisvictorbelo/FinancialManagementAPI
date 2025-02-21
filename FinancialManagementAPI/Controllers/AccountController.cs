using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateAccount([FromBody] AccountDto dto) {
            var user = await _context.Users.FindAsync(dto.UserId);
            if(user == null)
                return NotFound("Usuário não encontrado");
            
            var account = new Account {
                Name = dto.Name,
                Balance = dto.Balance,
                UserId = dto.UserId,
                User = user
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Created("", new { account.Name, account.Balance, account.UserId, account.User }); 
        }
    }
}