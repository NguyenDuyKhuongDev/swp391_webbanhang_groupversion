using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.ExtensionMethods;
using OnlineShop.Models;
using OnlineShop.ViewModel;

namespace OnlineShop.Controllers
{
    public class BlogsController : Controller
    {
        private const int PAGE_SIZE = 5;
        private const int MAX_DISPLAY_PAGES_INDEX = 5;
        private const int MAX_DISPLAY_PAGES_COMMON_PAGE = 7;
        private readonly ApplicationDbContext _context;
        private List<Blog>? sourceBlog;
        public BlogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Blogs
        public async Task<IActionResult> Index(int page = 1)
        {


            sourceBlog = await _context.Blogs.Include(b => b.Category).Include(b => b.Thumbnail).ToListAsync();
            var totalPage = (int)Math.Ceiling((double)sourceBlog.Count / PAGE_SIZE);
            var blogPaging = CommonMethods.Paging(sourceBlog, page, PAGE_SIZE);
            var blogPagingSorted = blogPaging.OrderByDescending(b => b.PublishedDate).ToList();
            ViewData["currentPage"] = page;
            ViewData["totalPage"] = totalPage;
            ViewData["maxDisplayPages"] = MAX_DISPLAY_PAGES_INDEX;
            return View(blogPagingSorted);
        }

        public async Task<IActionResult> IndexCommonBlogs(int page = 1, string CategoryId = "", string tagId = "")
        {
            ViewData["blogCategories"] = await _context.BlogCategories.ToListAsync();
            ViewData["ListTags"] = await _context.Tags.ToListAsync();
            var sourceBlog = await _context.Blogs
     .Include(b => b.Category)
     .Include(b => b.Thumbnail)
     .Include(b => b.TagBlogs)
     .Where(b => b.IsPublished)
     .ToListAsync();

            if (!string.IsNullOrEmpty(CategoryId)) sourceBlog = sourceBlog.Where(b => b.CategoryId == int.Parse(CategoryId)).ToList();
            if (!string.IsNullOrEmpty(tagId)) sourceBlog = sourceBlog.Where(b => b.TagBlogs.Any(t => t.TagId == int.Parse(tagId))).ToList();

            var totalPage = (int)Math.Ceiling((double)sourceBlog.Count / PAGE_SIZE);
            var blogPaging = CommonMethods.Paging(sourceBlog, page, PAGE_SIZE);
            var blogPagingSorted = blogPaging.OrderByDescending(b => b.PublishedDate).ToList();
            ViewData["currentPage"] = page;
            ViewData["totalPage"] = totalPage;
            ViewData["maxDisplayPages"] = MAX_DISPLAY_PAGES_COMMON_PAGE;
            ViewData["PopularBlog"] = sourceBlog.OrderByDescending(b => b.ViewCount).FirstOrDefault();
            return View("IndexCommonBlogs", blogPagingSorted);
        }

        public async Task<IActionResult> BlogPageView(int id)
        {

            ViewData["BlogTags"] = await _context.TagBlogs.Include(t => t.Tag).Where(t => t.BlogId == id).Select(t => t.Tag.Name).ToListAsync();
            var blogContext = _context.Blogs.Include(b => b.Category).Include(b => b.Thumbnail).FirstOrDefault(blog => blog.Id == id);
            ViewData["BlogAds"] = await _context.Advertisements.Include(ad=>ad.AdPlacements).Where(ad=>ad.AdPlacements.Any(ap=>ap.BlogId==id)&&ad.IsActive).ToListAsync()??new List<Advertisement>();
            string msg = await UpdateViewCount(id);
            return View("BlogPageView", blogContext);
        }

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Blogs/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name");
            ViewData["ThumbnailId"] = new SelectList(_context.Thumbnails, "Id", "ThumbnailName");
            ViewData["ListTag"] = new SelectList(_context.Tags, "Id", "Name");
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogVM blogVM, List<int> SelectedTags)
        {

            if (ModelState.IsValid)
            {
                var publishDate = (blogVM.IsPublished) ? DateTime.Now : (DateTime?)null;
                var blog = new Blog()
                {
                    Title = blogVM.Title,
                    MetaTitle = blogVM.MetaTitle,
                    MetaDescription = blogVM.MetaDescription,
                    Slug = blogVM.Slug,
                    Summary = blogVM.Summary,
                    Content = blogVM.Content,
                    PublishedDate = publishDate,
                    CategoryId = blogVM.CategoryId,
                    ThumbnailId = blogVM.ThumbnailId,
                };

                _context.Add(blog);
                await _context.SaveChangesAsync();
                var currentblogCreate = _context.Blogs.FirstOrDefault(b => b.Title == blog.Title && b.Content == blog.Content);
                if (SelectedTags != null || SelectedTags.Count > 0)
                {
                    foreach (var tag in SelectedTags)
                    {
                        _context.Add(new TagBlog()
                        {
                            BlogId = currentblogCreate.Id,
                            TagId = tag

                        });
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));

            }
            ViewData["CategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name", blogVM.CategoryId);
            ViewData["ThumbnailId"] = new SelectList(_context.Thumbnails, "Id", "ThumbnailName", blogVM.ThumbnailId);
            return View(blogVM);
        }

        // GET: Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            var blogVM = new BlogVM()
            {
                Id = blog.Id,
                ThumbnailId = blog.ThumbnailId,
                CategoryId = blog.CategoryId,
                Title = blog.Title,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                Slug = blog.Slug,
                Summary = blog.Summary,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                IsPublished = blog.IsPublished,

            };
            ViewData["CategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name", blog.CategoryId);
            ViewData["ThumbnailId"] = new SelectList(_context.Thumbnails, "Id", "ThumbnailName", blog.ThumbnailId);
            var tagOfBlog = await _context.TagBlogs
                .Include(t => t.Tag)
                .Where(t => t.BlogId == id)
                .Select(t => t.Tag).ToListAsync();

            ViewData["tagOfBlog"] = new SelectList(tagOfBlog, "Id", "Name");
            ViewData["ListTag"] = new SelectList(_context.Tags, "Id", "Name");
            return View(blogVM);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BlogVM blogVM, List<int> SelectedTags)
        {
            if (ModelState.IsValid)
            {
                var blogUpdate = await _context.Blogs.FirstAsync(blog => blog.Id == blogVM.Id);


                if (blogUpdate == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                blogUpdate.Title = blogVM.Title;
                blogUpdate.MetaTitle = blogVM.MetaTitle;
                blogUpdate.MetaDescription = blogVM.MetaDescription;
                blogUpdate.IsPublished = blogVM.IsPublished;
                if (blogVM.IsPublished) blogUpdate.PublishedDate = DateTime.Now;
                blogUpdate.Slug = blogVM.Slug;
                blogUpdate.Summary = blogVM.Summary;
                blogUpdate.Content = blogVM.Content;
                blogUpdate.ThumbnailId = blogVM.ThumbnailId;
                blogUpdate.CategoryId = blogVM.CategoryId;
                blogUpdate.AuthorId = blogVM.AuthorId;
                try
                {
                    _context.Update(blogUpdate);
                    await _context.SaveChangesAsync();

                    await UpdateTags(blogVM.Id, SelectedTags);
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    throw new DbUpdateConcurrencyException(exception.Message);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name", blogVM.CategoryId);
            ViewData["ThumbnailId"] = new SelectList(_context.Thumbnails, "Id", "ThumbnailName", blogVM.ThumbnailId);

            return View(blogVM);
        }


        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Blogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Blogs'  is null.");
            }
            var blog = await _context.Blogs.FindAsync(id);
            var tagsOfBlog = await _context.TagBlogs.Where(t => t.BlogId == id).ToListAsync();
            if (tagsOfBlog != null && tagsOfBlog.Count > 0)
            {
                foreach (var tag in tagsOfBlog)
                {
                    _context.Remove(tag);
                }
            }
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> PublishBlog(int id)
        {
            if (id == null) return RedirectToAction(nameof(Index));
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return RedirectToAction(nameof(Index));
            blog.IsPublished = !blog.IsPublished;
            blog.PublishedDate = (blog.IsPublished) ? DateTime.Now : (DateTime?)null;
            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        /*    public async Task<string> UpdateLikeCount(int id) {
                if (id == null) return "Input is null";
                var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
                if (blog == null) return "Blog is null";
                blog.LikeCount += 1;
                _context.Blogs.Update(blog);
                await _context.SaveChangesAsync();
                return "";
            }*/

        public async Task<string> UpdateViewCount(int id)
        {

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return "Blog is null";
            blog.ViewCount = (blog.ViewCount ?? 0) + 1;

            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();
            return "";
        }
        private bool BlogExists(int id)
        {
            return (_context.Blogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private async Task UpdateTags(int blogId, List<int> SelectedTags)
        {
            var existingTagIds = await _context.TagBlogs
                .Where(t => t.BlogId == blogId)
                .Select(t => t.TagId)
                .ToListAsync();

            var existingSet = new HashSet<int>(existingTagIds);
            var selectedSet = new HashSet<int>(SelectedTags);

            var tagToRemove = existingSet.Except(selectedSet);
            var tagToAdd = selectedSet.Except(existingSet);

            foreach (var tagId in tagToAdd)
            {
                _context.TagBlogs.Add(new TagBlog { BlogId = blogId, TagId = tagId });
            }
            var tagNeedRemove = _context.TagBlogs.Where(t => tagToRemove.Contains(t.TagId) && t.BlogId == blogId);
            if (tagNeedRemove != null) _context.TagBlogs.RemoveRange(tagNeedRemove);

            await _context.SaveChangesAsync();

        }
    }
}
