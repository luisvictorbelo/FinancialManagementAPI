using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialManagementAPI.Models;

namespace FinancialManagementAPI.DTOs
{
    public class AccountDto()
    {
        public required string Name { get; set; }
        public decimal Balance {get; set; }

    }
}