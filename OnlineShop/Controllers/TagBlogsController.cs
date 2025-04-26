using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class TagBlogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagBlogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TagBlogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TagBlogs.Include(t => t.Blog).Include(t => t.Tag);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TagBlogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TagBlogs == null)
            {
                return NotFound();
            }

            var tagBlog = await _context.TagBlogs
                .Include(t => t.Blog)
                .Include(t => t.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagBlog == null)
            {
                return NotFound();
            }

            return View(tagBlog);
        }

        // GET: TagBlogs/Create
        public IActionResult Create()
        {
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Title");
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Name");
            return View();
        }

        // POST: TagBlogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TagId,BlogId,CreatedAt")] TagBlog tagBlog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tagBlog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Id", tagBlog.BlogId);
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagBlog.TagId);
            return View(tagBlog);
        }

        // GET: TagBlogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TagBlogs == null)
            {
                return NotFound();
            }

            var tagBlog = await _context.TagBlogs.FindAsync(id);
            if (tagBlog == null)
            {
                return NotFound();
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Id", tagBlog.BlogId);
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagBlog.TagId);
            return View(tagBlog);
        }

        // POST: TagBlogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TagId,BlogId,CreatedAt")] TagBlog tagBlog)
        {
            if (id != tagBlog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tagBlog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagBlogExists(tagBlog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Id", tagBlog.BlogId);
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagBlog.TagId);
            return View(tagBlog);
        }

        // GET: TagBlogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TagBlogs == null)
            {
                return NotFound();
            }

            var tagBlog = await _context.TagBlogs
                .Include(t => t.Blog)
                .Include(t => t.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagBlog == null)
            {
                return NotFound();
            }

            return View(tagBlog);
        }

        // POST: TagBlogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TagBlogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TagBlogs'  is null.");
            }
            var tagBlog = await _context.TagBlogs.FindAsync(id);
            if (tagBlog != null)
            {
                _context.TagBlogs.Remove(tagBlog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagBlogExists(int id)
        {
          return (_context.TagBlogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
