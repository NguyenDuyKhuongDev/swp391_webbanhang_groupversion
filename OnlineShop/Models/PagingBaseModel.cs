namespace OnlineShop.Models
{
    public class PagingBaseModel
    {
        private int pageSize = 8;
        private int pageNumber = 1;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > 0 ? value : 8;
        }

        public int PageNumber
        {
            get => pageNumber;
            set => pageNumber = value > 0 ? value : 1;
        }

        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}