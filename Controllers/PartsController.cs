
using GarageAI.Data;
using GarageAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class PartsController : Controller
{
    private readonly GarageDbContext _db;

    public PartsController(GarageDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Parts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p =>
                p.Code.Contains(search) ||
                p.Name.Contains(search));
        }

        ViewBag.Search = search;

        return View(await query
            .OrderBy(p => p.Name)
            .ToListAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Part());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Part part)
    {
        if (await _db.Parts.AnyAsync(p => p.Code == part.Code))
        {
            ModelState.AddModelError(
                nameof(part.Code), "Mã phụ tùng đã tồn tại.");
        }

        if (!ModelState.IsValid)
            return View(part);

        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thêm phụ tùng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound();

        return View(part);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Part input)
    {
        if (id != input.Id) return BadRequest();

        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound();

        if (await _db.Parts.AnyAsync(p =>
            p.Code == input.Code && p.Id != id))
        {
            ModelState.AddModelError(
                nameof(input.Code), "Mã phụ tùng đã tồn tại.");
        }

        if (!ModelState.IsValid)
            return View(input);

        part.Code = input.Code;
        part.Name = input.Name;
        part.Price = input.Price;
        part.StockQuantity = input.StockQuantity;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Cập nhật phụ tùng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound();

        _db.Parts.Remove(part);

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xóa phụ tùng thành công.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] =
                "Không thể xóa phụ tùng đang được sử dụng.";
        }

        return RedirectToAction(nameof(Index));
    }
}
