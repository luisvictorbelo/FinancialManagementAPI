using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialManagementAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public required Account Account { get; set; }
        public required string Type { get; set; } // "income" ou "expense" -> talvez mudar para um enum
        public decimal Amount { get; set; }

        public required string Category { get; set; }

        public DateTime Date { get; set; }
    }
}