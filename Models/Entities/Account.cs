using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SheridanBankingTeamProject.Models.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public double Balance {get; set;}


        // Foreign key
        public Guid UserId { get; set; }
        public User User { get; set; }
        
        public List<Transaction> Transactions {get; set;}
    }
}