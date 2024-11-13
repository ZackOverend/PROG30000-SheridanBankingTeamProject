using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SheridanBankingTeamProject.Models.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public int AccountNumber { get; set; }
        public double Balance {get; set;}
    }
}