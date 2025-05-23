﻿using System;
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
    public class AdvertisementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdvertisementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Advertisements
        public async Task<IActionResult> Index()
        {
            ViewData["templateTypes"] = _context.AdTemplateTypes.ToList();
            ViewData["categories"] = _context.CategoryProducts.ToList();
            ViewData["Products"] = await _context.Products.ToListAsync();
            var applicationDbContext = _context.Advertisements.Include(a => a.AdTemplate);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Advertisements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Advertisements == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements
                .Include(a => a.AdTemplate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (advertisement == null)
            {
                return NotFound();
            }

            return View(advertisement);
        }

        // GET: Advertisements/Create
        public IActionResult Create()
        {
            ViewData["AdTemplateId"] = new SelectList(_context.AdTemplates, "Id", "Id");
            return View();
        }

        // POST: Advertisements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ImageUrl,LinkUrl,IsActive,StartDate,EndDate,CreatedAt,UpdatedAt,AdTemplateId")] Advertisement advertisement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(advertisement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdTemplateId"] = new SelectList(_context.AdTemplates, "Id", "Id", advertisement.AdTemplateId);
            return View(advertisement);
        }

        // GET: Advertisements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Advertisements == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return NotFound();
            }
            ViewData["AdTemplateId"] = new SelectList(_context.AdTemplates, "Id", "Id", advertisement.AdTemplateId);
            return View(advertisement);
        }

        // POST: Advertisements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImageUrl,LinkUrl,IsActive,StartDate,EndDate,CreatedAt,UpdatedAt,AdTemplateId")] Advertisement advertisement)
        {
            if (id != advertisement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(advertisement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertisementExists(advertisement.Id))
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
            ViewData["AdTemplateId"] = new SelectList(_context.AdTemplates, "Id", "Id", advertisement.AdTemplateId);
            return View(advertisement);
        }

        // GET: Advertisements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Advertisements == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements
                .Include(a => a.AdTemplate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (advertisement == null)
            {
                return NotFound();
            }

            return View(advertisement);
        }

        // POST: Advertisements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Advertisements == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Advertisements'  is null.");
            }
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement != null)
            {
                _context.Advertisements.Remove(advertisement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisementExists(int id)
        {
            return (_context.Advertisements?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
