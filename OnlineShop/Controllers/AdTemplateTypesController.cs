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
    public class AdTemplateTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdTemplateTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdTemplateTypes
        public async Task<IActionResult> Index()
        {
              return _context.AdTemplateTypes != null ? 
                          View(await _context.AdTemplateTypes.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AdTemplateTypes'  is null.");
        }

        // GET: AdTemplateTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AdTemplateTypes == null)
            {
                return NotFound();
            }

            var adTemplateType = await _context.AdTemplateTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adTemplateType == null)
            {
                return NotFound();
            }

            return View(adTemplateType);
        }

        // GET: AdTemplateTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdTemplateTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Name")] AdTemplateType adTemplateType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(adTemplateType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(adTemplateType);
        }

        // GET: AdTemplateTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AdTemplateTypes == null)
            {
                return NotFound();
            }

            var adTemplateType = await _context.AdTemplateTypes.FindAsync(id);
            if (adTemplateType == null)
            {
                return NotFound();
            }
            return View(adTemplateType);
        }

        // POST: AdTemplateTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Name")] AdTemplateType adTemplateType)
        {
            if (id != adTemplateType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adTemplateType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdTemplateTypeExists(adTemplateType.Id))
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
            return View(adTemplateType);
        }

        // GET: AdTemplateTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AdTemplateTypes == null)
            {
                return NotFound();
            }

            var adTemplateType = await _context.AdTemplateTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adTemplateType == null)
            {
                return NotFound();
            }

            return View(adTemplateType);
        }

        // POST: AdTemplateTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AdTemplateTypes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AdTemplateTypes'  is null.");
            }
            var adTemplateType = await _context.AdTemplateTypes.FindAsync(id);
            if (adTemplateType != null)
            {
                _context.AdTemplateTypes.Remove(adTemplateType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdTemplateTypeExists(int id)
        {
          return (_context.AdTemplateTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
