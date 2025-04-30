using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.ViewModel;
using System.Globalization;

namespace OnlineShop.Services
{
    public interface IAdService
    {
        AdvertisementVM GetAdInfomationForPosition(string position, int blogId);
    }
    public class AdService : IAdService
    {
        private readonly ApplicationDbContext _context;
        public AdService(ApplicationDbContext context)
        {
            _context = context;
        }
        public AdvertisementVM? GetAdInfomationForPosition(string position, int blogId)
        {
            var msg = GetAd(position, blogId, out Advertisement? advertisement);
            if (msg.Length > 0) return null;
            msg = GetAdTemplate(advertisement, out AdTemplate? adTemplate);
            if (msg.Length > 0) return null;
            /* msg = GetAdTemplateType(adTemplate, out AdTemplateType? adTemplateType);
             if (msg.Length > 0) return null;
             msg = GetAdCategories(advertisement, out List<CategoryProduct>? adCategories);
             if (msg.Length > 0) return null;*/
            msg = GetAdPosition(position, out AdPosition? adPosition);
            if (msg.Length > 0) return null;
            return new AdvertisementVM
            {
                advertisement = advertisement,
                adTemplate = adTemplate,
                /*  adTemplateType = adTemplateType,
                  AdCategories = adCategories,*/
                adPosition = adPosition
            };



        }

        private string GetAdTemplate(Advertisement? advertisement, out AdTemplate? adTemplate)
        {
            adTemplate = null;
            if (advertisement == null) return "ad is null";
            var template = _context.AdTemplates.Where(t => t.Id == advertisement.AdTemplateId).FirstOrDefault();
            if (template == null) return "template is not found";
            adTemplate = template;
            return "";
        }

        private string GetAd(string position, int blogId, out Advertisement? advertisement)
        {
            advertisement = null;
            if (string.IsNullOrEmpty(position)) return "position  is not found";
            var msg = GetAdPosition(position, out AdPosition? adpos);
            if (msg.Length > 0) return msg;
            var adWithPosition = (
                from adPlacement in _context.AdPlacements
                join adPosition in _context.AdPositions on adPlacement.PositionId equals adPosition.Id
                join ad in _context.Advertisements on adPlacement.AdId equals ad.Id
                where adPlacement.BlogId == blogId && adPlacement.PositionId == adpos.Id
                select ad
                ).FirstOrDefault();
            if (adWithPosition == null) return "ad not found";
            advertisement = adWithPosition;
            return "";
        }
        private string GetAdTemplateType(AdTemplate? adTemplate, out AdTemplateType? adTemplateType)
        {
            adTemplateType = null;
            if (adTemplate == null) return "adTemplate is null";
            var templateType = _context.AdTemplateTypes.Where(at => at.Id == adTemplate.TypeId).FirstOrDefault();
            if (templateType == null) return "templateType is not found";
            adTemplateType = templateType;
            return "";

        }
        private string GetAdCategories(Advertisement? advertisement, out List<CategoryProduct>? adCategories)
        {
            adCategories = null;
            if (advertisement == null) return "ad is null";
            var categorieIds = _context.AdCategories.Where(ac => ac.AdId == advertisement.Id).Select(ac => ac.CategoryId).ToList();
            if (categorieIds.Count == 0) return "adCategories is not found";
            var categories = _context.CategoryProducts.Where(c => categorieIds.Contains(c.CategoryProductID)).ToList();
            if (categories == null || categories.Count == 0) return "categories is not found";
            adCategories = categories;
            return "";
        }
        private string GetAdPosition(string position, out AdPosition? adPosition)
        {
            adPosition = null;
            if (string.IsNullOrEmpty(position)) return "position is null";
            var pos = _context.AdPositions.Where(p => p.Position == position).FirstOrDefault();
            if (pos == null) return "position is not found";
            adPosition = pos;
            return "";

        }

    }
}
