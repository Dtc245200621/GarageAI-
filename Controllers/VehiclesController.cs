
using GarageAI.Data;
using GarageAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class VehiclesController : Controller
{
    private readonly GarageDbContext _db;

    public VehiclesController(GarageDbContext db)
    {
        _db = db;
    }

    private async Task LoadCustomers()
    {
        var customers = await _db.Customers
            .OrderBy(c => c.FullName)
            .ToListAsync();

        ViewBag.Customers = new SelectList(
            customers, "Id", "FullName");
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Vehicles
            .Include(v => v.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v =>
                v.LicensePlate.Contains(search) ||
                v.Brand.Contains(search) ||
                v.Model.Contains(search));
        }

        ViewBag.Search = search;
        await LoadCustomers();

        return View(await query
            .OrderByDescending(v => v.Id)
            .ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vehicle vehicle)
    {
        ModelState.Remove(nameof(Vehicle.Customer));

        if (!await _db.Customers.AnyAsync(
            c => c.Id == vehicle.CustomerId))
        {
            ModelState.AddModelError(
                nameof(Vehicle.CustomerId),
                "Khách hàng không tồn tại");
        }

        if (await _db.Vehicles.AnyAsync(
            v => v.LicensePlate == vehicle.LicensePlate))
        {
            ModelState.AddModelError(
                nameof(Vehicle.LicensePlate),
                "Biển số đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] =
                string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        _db.Vehicles.Add(vehicle);

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Thêm xe thành công";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] =
                "Không thể lưu xe. Kiểm tra biển số và chủ xe.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();

        await LoadCustomers();
        return View(vehicle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vehicle input)
    {
        if (id != input.Id) return BadRequest();

        ModelState.Remove(nameof(Vehicle.Customer));

        if (!await _db.Customers.AnyAsync(
            c => c.Id == input.CustomerId))
        {
            ModelState.AddModelError(
                nameof(Vehicle.CustomerId),
                "Khách hàng không tồn tại");
        }

        if (await _db.Vehicles.AnyAsync(v =>
            v.LicensePlate == input.LicensePlate &&
            v.Id != id))
        {
            ModelState.AddModelError(
                nameof(Vehicle.LicensePlate),
                "Biển số đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadCustomers();
            return View(input);
        }

        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();

        vehicle.LicensePlate = input.LicensePlate;
        vehicle.Brand = input.Brand;
        vehicle.Model = input.Model;
        vehicle.Year = input.Year;
        vehicle.CustomerId = input.CustomerId;

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật xe thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("",
                "Không thể cập nhật. Kiểm tra dữ liệu xe.");
            await LoadCustomers();
            return View(input);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);

        if (vehicle == null) return NotFound();

        _db.Vehicles.Remove(vehicle);

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa xe";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] =
                "Không thể xóa xe vì có dữ liệu liên quan.";
        }

        return RedirectToAction(nameof(Index));
    }
}
