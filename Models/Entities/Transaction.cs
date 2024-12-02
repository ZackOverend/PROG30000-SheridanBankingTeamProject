

namespace SheridanBankingTeamProject.Models.Entities
{
    public class Transaction
    {
        // Transaction ID
        public Guid Id { get; set; }
        // ID of Sender
        public Guid Sender {get; set;}
        // ID of Receiver
        public Guid Receiver {get; set;}
        // Transaction Amount
        public double Amount {get; set;}

        // Delivery type (Cheque, ATM, Transfer, Business)
        public String Message {get; set;}

        // Date of creation 
        public DateTime Date { get; set; } = DateTime.Now;

        // Foreign key
        public Guid AccountId { get; set; }
        public Account Account { get; set; }


    }
}