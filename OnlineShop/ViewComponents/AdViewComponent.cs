using Microsoft.AspNetCore.Mvc;
using OnlineShop.Services;
using OnlineShop.ViewModel;

namespace OnlineShop.ViewComponents
{
    public class AdViewComponent : ViewComponent
    {
        private IAdService _adService;

        public AdViewComponent(IAdService adService)
        {
            _adService = adService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string position, int blogId)
        {
            var adVM = _adService.GetAdInfomationForPosition(position, blogId);
            if (adVM == null) return Content("");
            var content = RenderBanerType(adVM);

            return View("AdRenderHTML", content);
        }

        public string RenderBanerType(AdvertisementVM adVM)
        {
            string adtemplate = adVM.adTemplate.HtmlTemplate;
            var replacement = new Dictionary<string, string> {
             { "{{width}}", adVM.adPosition.Width.ToString() },
            { "{{height}}", adVM.adPosition.Height.ToString() },
            { "{{imageUrl}}", adVM.advertisement.ImageUrl ?? "" },
            { "{{productUrl}}", adVM.advertisement.LinkUrl ?? "#" }
            };

            foreach (var item in replacement)
            {
                adtemplate = adtemplate.Replace(item.Key, item.Value);
            }
            return adtemplate;
        }
    }


}
