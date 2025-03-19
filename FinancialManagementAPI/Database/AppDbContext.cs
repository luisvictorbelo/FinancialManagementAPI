using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FinancialManagementAPI.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public required DbSet<User> Users { get; set; }
        public required DbSet<Account> Accounts { get; set; }
        public required DbSet<Transaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}