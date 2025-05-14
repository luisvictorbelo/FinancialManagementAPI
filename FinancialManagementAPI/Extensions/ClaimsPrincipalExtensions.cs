using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinancialManagementAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            if (user == null) return null;
            
            if (int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return userId;
            
            return null;
        }
    }
}