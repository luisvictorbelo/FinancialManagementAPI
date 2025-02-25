using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialManagementAPI.Enum;
using FinancialManagementAPI.Models;

namespace FinancialManagementAPI.DTOs
{
    public class TransactionDto
    {
        public int AccountId { get; set; }
        public required TypeTransaction Type { get; set; }
        public decimal Amount { get; set; }
        public required string Category { get; set; }
        public DateTime Date { get; set; }
    }
}