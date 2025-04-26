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
    public class AdPositionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdPositionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdPositions
        public async Task<IActionResult> Index()
        {
              return _context.AdPositions != null ? 
                          View(await _context.AdPositions.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AdPositions'  is null.");
        }

        // GET: AdPositions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AdPositions == null)
            {
                return NotFound();
            }

            var adPosition = await _context.AdPositions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adPosition == null)
            {
                return NotFound();
            }

            return View(adPosition);
        }

        // GET: AdPositions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdPositions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Position,Name,Description,Width,Height,IsActive")] AdPosition adPosition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(adPosition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(adPosition);
        }

        // GET: AdPositions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AdPositions == null)
            {
                return NotFound();
            }

            var adPosition = await _context.AdPositions.FindAsync(id);
            if (adPosition == null)
            {
                return NotFound();
            }
            return View(adPosition);
        }

        // POST: AdPositions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Position,Name,Description,Width,Height,IsActive")] AdPosition adPosition)
        {
            if (id != adPosition.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adPosition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdPositionExists(adPosition.Id))
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
            return View(adPosition);
        }

        // GET: AdPositions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AdPositions == null)
            {
                return NotFound();
            }

            var adPosition = await _context.AdPositions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adPosition == null)
            {
                return NotFound();
            }

            return View(adPosition);
        }

        // POST: AdPositions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AdPositions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AdPositions'  is null.");
            }
            var adPosition = await _context.AdPositions.FindAsync(id);
            if (adPosition != null)
            {
                _context.AdPositions.Remove(adPosition);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdPositionExists(int id)
        {
          return (_context.AdPositions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
