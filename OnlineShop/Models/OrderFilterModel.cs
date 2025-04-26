namespace OnlineShop.Models
{
    public class OrderFilterModel : PagingBaseModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? OrderId { get; set; }
        public string? CustomerSearch { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; } // "asc" or "desc"
    }
}