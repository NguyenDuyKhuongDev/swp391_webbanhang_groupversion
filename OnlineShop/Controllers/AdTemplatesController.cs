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
    public class AdTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdTemplates
        public async Task<IActionResult> Index()
        {
            var adTemplates = await _context.AdTemplates.Include(a => a.AdTemplatePositions).ToListAsync();
            ViewData["AdPositions"] = await _context.AdPositions.ToListAsync();
            ViewData["TemplateTypes"] = await _context.AdTemplateTypes.ToListAsync();
            return _context.AdTemplates != null ?
            View(adTemplates) :
            Problem("Entity set 'ApplicationDbContext.AdTemplates'  is null.");
        }

        // GET: AdTemplates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AdTemplates == null)
            {
                return NotFound();
            }

            var adTemplate = await _context.AdTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adTemplate == null)
            {
                return NotFound();
            }

            return View(adTemplate);
        }

        // GET: AdTemplates/Create
        public  IActionResult Create()
        {
            ViewData["AdPositions"] =  _context.AdPositions.ToList();
            ViewData["TemplateTypes"] =  _context.AdTemplateTypes.ToList();
            return View();
        }

        // POST: AdTemplates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,HtmlTemplate,PreviewImageUrl,IsActive")] AdTemplate adTemplate, List<int>? SelectedPositionIds,string TypeId)
        {
            if (ModelState.IsValid)
            {
                if (SelectedPositionIds == null || SelectedPositionIds.Count == 0)
                {
                    TempData["Error"] = "Must select at least one position";
                    return RedirectToAction(nameof(Index));
                }

                adTemplate.CreatedAt = DateTime.Now;
                adTemplate.TypeId = int.Parse(TypeId);
                _context.Add(adTemplate);
                await _context.SaveChangesAsync();

                foreach (var positionId in SelectedPositionIds)
                {
                    _context.AdTemplatePositions.Add(new AdTemplatePosition
                    {
                        PositionId = positionId,
                        AdTemplateId = adTemplate.Id
                    });
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdPositions"] = _context.AdPositions.ToList();
            ViewData["TemplateTypes"] = _context.AdTemplateTypes.ToList();
            return View(adTemplate);
        }

        // GET: AdTemplates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AdTemplates == null)
            {
                return NotFound();
            }
            ViewData["AdTemplatePositions"]  =await _context.AdTemplatePositions.Where(a => a.AdTemplateId == id).Select(a => a.PositionId).ToListAsync();
            ViewData["AdPositions"] = _context.AdPositions.ToList();
            ViewData["TemplateTypes"] = _context.AdTemplateTypes.ToList();
           

            var adTemplate = await _context.AdTemplates.FindAsync(id);
            if (adTemplate == null)
            {
                return NotFound();
            }
            return View(adTemplate);
        }

        // POST: AdTemplates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,HtmlTemplate,PreviewImageUrl,IsActive")] AdTemplate adTemplate, List<int> SelectedPositionIds,string TypeId)
        {
            if (id != adTemplate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
               
                try
                {
                    _context.AdTemplatePositions.RemoveRange(_context.AdTemplatePositions.Where(a => a.AdTemplateId == id));

                    _context.AdTemplatePositions.AddRange(SelectedPositionIds.Select(positionId =>
                    new AdTemplatePosition
                    {
                        PositionId = positionId,
                        AdTemplateId = id
                    }
                    ));
                    adTemplate.TypeId = int.Parse(TypeId);
                    _context.Update(adTemplate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdTemplateExists(adTemplate.Id))
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
            return View(adTemplate);
        }

        // GET: AdTemplates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AdTemplates == null)
            {
                return NotFound();
            }

            var adTemplate = await _context.AdTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adTemplate == null)
            {
                return NotFound();
            }

            return View(adTemplate);
        }

        // POST: AdTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AdTemplates == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AdTemplates'  is null.");
            }
            var adTemplate = await _context.AdTemplates.FindAsync(id);
            if (adTemplate != null)
            {
                _context.AdTemplates.Remove(adTemplate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdTemplateExists(int id)
        {
            return (_context.AdTemplates?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
