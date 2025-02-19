using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagementAPI.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public required DbSet<User> Users { get; set; }
        public required DbSet<Account> Accounts { get; set; }
        public required DbSet<Transaction> Transactions { get; set; }
    }
}