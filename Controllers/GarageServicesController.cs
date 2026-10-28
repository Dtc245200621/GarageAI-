
using GarageAI.Data;
using GarageAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class GarageServicesController : Controller
{
    private readonly GarageDbContext _db;

    public GarageServicesController(GarageDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.GarageServices.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(s =>
                s.Name.Contains(search));
        }

        ViewBag.Search = search;

        return View(await query
            .OrderBy(s => s.Name)
            .ToListAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new GarageService());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Description,Price")] GarageService service)
    {
        if (!ModelState.IsValid)
            return View(service);

        _db.GarageServices.Add(service);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thêm dịch vụ thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await _db.GarageServices.FindAsync(id);

        if (service == null)
            return NotFound();

        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Name,Description,Price")] GarageService input)
    {
        if (!ModelState.IsValid)
            return View(input);

        var service = await _db.GarageServices.FindAsync(id);

        if (service == null)
            return NotFound();

        service.Name = input.Name;
        service.Description = input.Description;
        service.Price = input.Price;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Cập nhật dịch vụ thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.GarageServices.FindAsync(id);

        if (service == null)
            return NotFound();

        _db.GarageServices.Remove(service);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Xóa dịch vụ thành công.";
        return RedirectToAction(nameof(Index));
    }
}
