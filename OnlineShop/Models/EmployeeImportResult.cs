namespace OnlineShop.Models
{
    public class EmployeeImportResult
    {
        public int RowNumber { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, string> Errors { get; set; }
    }
}
