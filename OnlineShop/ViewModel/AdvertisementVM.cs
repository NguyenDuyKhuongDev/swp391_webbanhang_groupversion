using OnlineShop.Models;

namespace OnlineShop.ViewModel
{
    public class AdvertisementVM
    {
        public Advertisement? advertisement { get; set; }
        public AdTemplate? adTemplate { get; set; }
        public AdTemplateType? adTemplateType { get; set; }
        public List<CategoryProduct>? AdCategories { get; set; }
        public AdPosition? adPosition { get; set; }

    }
}
