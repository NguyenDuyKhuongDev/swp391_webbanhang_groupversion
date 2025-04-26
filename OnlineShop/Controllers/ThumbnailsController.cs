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
    public class ThumbnailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThumbnailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Thumbnails
        public async Task<IActionResult> Index()
        {
              return _context.Thumbnails != null ? 
                          View(await _context.Thumbnails.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Thumbnails'  is null.");
        }

        // GET: Thumbnails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Thumbnails == null)
            {
                return NotFound();
            }

            var thumbnail = await _context.Thumbnails
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thumbnail == null)
            {
                return NotFound();
            }

            return View(thumbnail);
        }

        // GET: Thumbnails/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thumbnails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThumnailUrl,ThumbnailName")] Thumbnail thumbnail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thumbnail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(thumbnail);
        }

        // GET: Thumbnails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Thumbnails == null)
            {
                return NotFound();
            }

            var thumbnail = await _context.Thumbnails.FindAsync(id);
            if (thumbnail == null)
            {
                return NotFound();
            }
            return View(thumbnail);
        }

        // POST: Thumbnails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ThumnailUrl,ThumbnailName")] Thumbnail thumbnail)
        {
            if (id != thumbnail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thumbnail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThumbnailExists(thumbnail.Id))
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
            return View(thumbnail);
        }

        // GET: Thumbnails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Thumbnails == null)
            {
                return NotFound();
            }

            var thumbnail = await _context.Thumbnails
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thumbnail == null)
            {
                return NotFound();
            }

            return View(thumbnail);
        }

        // POST: Thumbnails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Thumbnails == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Thumbnails'  is null.");
            }
            var thumbnail = await _context.Thumbnails.FindAsync(id);
            if (thumbnail != null)
            {
                _context.Thumbnails.Remove(thumbnail);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ThumbnailExists(int id)
        {
          return (_context.Thumbnails?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
