using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinancialManagementAPI.Database;
using FinancialManagementAPI.DTOs;
using FinancialManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagementAPI.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("create")]

        public async Task<IActionResult> CreateAccount([FromBody] AccountDto dto)
        {

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");

            var newAccount = new Account
            {
                Name = dto.Name,
                Balance = dto.Balance,
                UserId = userId,
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Created("", new { newAccount.Id, newAccount.Name, newAccount.Balance, newAccount.UserId });
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized("Usuário não autenticado");

            var accounts = await _context.Accounts
                .Where(x => x.UserId == userId)
                .Select(x => new { x.Id, x.Name, x.Balance })
                .ToListAsync();

            return Ok(accounts);
        }

        // public async Task<IActionResult> GetAccountById()
        // {
        //     if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        //         return Unauthorized("Usuário não autenticado");

        //     var accounts = await _context.Accounts
        //         .Where(x => x.UserId == userId)
        //         .Select(x => new { x.Id, x.Name, x.Balance })
        //         .ToListAsync();

        //     return Ok(accounts);
        // }
    }
}