namespace CapstoneCC.Models
{
    public class SalesTransaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string CashierName { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }

    }
}
