namespace OnlineShop.ViewModel
{
    public class AdvertisementVM
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? AdHtmlContent { get; set; }

        public string? ImageUrl { get; set; }

        public string? LinkUrl { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? AdTemplateId { get; set; }

        public bool? IsCustomHtml { get; set; }
    }
}
