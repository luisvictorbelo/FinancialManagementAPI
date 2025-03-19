using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialManagementAPI.Models
{
    public class Account
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Balance {get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public List<Transaction> Transactions { get; set; } = [];
    }
}