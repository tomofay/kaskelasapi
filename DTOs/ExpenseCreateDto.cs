namespace KasKelasApi.DTOs
{
    public class ExpenseCreateDto
    {
        public int ClassId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int CreatedBy { get; set; } // Id user yang membuat
    }
}
