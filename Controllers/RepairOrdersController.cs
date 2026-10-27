
using GarageAI.Data;
using GarageAI.Services;
using GarageAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

public class RepairOrdersController : Controller
{
    private readonly GarageDbContext _db;
    private readonly GarageAiService _garageAiService;

    public RepairOrdersController(
        GarageDbContext db,
        GarageAiService garageAiService)
    {
        _db = db;
        _garageAiService = garageAiService;
    }


    public async Task<IActionResult> Index(
        string? search, string? status)
    {
        var query = _db.RepairOrders
            .Include(r => r.Vehicle)
            .ThenInclude(v => v!.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(r =>
                r.Vehicle != null &&
                (r.Vehicle.LicensePlate.Contains(search) ||
                 (r.Vehicle.Customer != null &&
                  r.Vehicle.Customer.FullName.Contains(search))));
        }

        var validStatuses = new[]
        {
        "MoiTiepNhan",
        "DangSuaChua",
        "HoanThanh",
        "DaHuy"
    };

        if (!string.IsNullOrWhiteSpace(status) &&
            validStatuses.Contains(status))
        {
            query = query.Where(r => r.Status == status);
        }

        ViewBag.Search = search;
        ViewBag.Status = status;

        var orders = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(orders);
    }


    private async Task LoadVehicles()
    {
        var vehicles = await _db.Vehicles
            .OrderBy(v => v.LicensePlate)
            .ToListAsync();

        ViewBag.Vehicles = new SelectList(
            vehicles, "Id", "LicensePlate");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadVehicles();

        return View(new RepairOrder
        {
            CreatedAt = DateTime.Now,
            Status = "MoiTiepNhan"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        RepairOrder input)
    {
        ModelState.Remove(nameof(RepairOrder.Vehicle));

        if (!await _db.Vehicles.AnyAsync(
            v => v.Id == input.VehicleId))
        {
            ModelState.AddModelError(
                nameof(input.VehicleId),
                "Xe không tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadVehicles();
            return View(input);
        }

        var order = new RepairOrder
        {
            VehicleId = input.VehicleId,
            Description = input.Description,
            TechnicianName = input.TechnicianName,
            RepairNotes = input.RepairNotes,
            CreatedAt = DateTime.Now,
            Status = "MoiTiepNhan"
        };

        _db.RepairOrders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Tạo phiếu sửa chữa thành công";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var order = await _db.RepairOrders.FindAsync(id);

        if (order == null)
            return NotFound();

        if (order.Status == "DaHuy")
            return BadRequest("Không thể sửa phiếu đã hủy.");

        await LoadVehicles();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RepairOrder input)
    {
        if (id != input.Id)
            return BadRequest();

        var order = await _db.RepairOrders.FindAsync(id);

        if (order == null)
            return NotFound();

        if (order.Status == "DaHuy")
            return BadRequest("Không thể sửa phiếu đã hủy.");

        ModelState.Remove(nameof(RepairOrder.Vehicle));

        if (!await _db.Vehicles.AnyAsync(
            v => v.Id == input.VehicleId))
        {
            ModelState.AddModelError(
                nameof(input.VehicleId),
                "Xe không tồn tại");
        }

        if (!ModelState.IsValid)
        {
            await LoadVehicles();
            return View(input);
        }

        order.VehicleId = input.VehicleId;
        order.Description = input.Description;
        order.TechnicianName = input.TechnicianName;
        order.RepairNotes = input.RepairNotes;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Cập nhật phiếu sửa chữa thành công";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        int id, string status)
    {
        var order = await _db.RepairOrders.FindAsync(id);

        if (order == null)
            return NotFound();

        bool validTransition =
            (order.Status == "MoiTiepNhan" &&
             (status == "DangSuaChua" || status == "DaHuy"))
            ||
            (order.Status == "DangSuaChua" &&
             status == "HoanThanh");

        if (!validTransition)
        {
            TempData["Error"] =
                "Không thể chuyển sang trạng thái này.";

            return RedirectToAction(nameof(Index));
        }

        order.Status = status;
        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Cập nhật trạng thái thành công";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> History(int vehicleId)
    {
        var vehicle = await _db.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle == null)
            return NotFound();

        var orders = await _db.RepairOrders
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.Vehicle = vehicle;

        return View(orders);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AiSummary(int vehicleId)
    {
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle == null)
            return NotFound();

        var orders = await _db.RepairOrders
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        if (orders.Count == 0)
        {
            TempData["AiError"] = "Xe chưa có lịch sử sửa chữa.";
            return RedirectToAction(nameof(History), new { vehicleId });
        }

        var history = string.Join("\n\n", orders.Select(r =>
            $"Ngày: {r.CreatedAt:dd/MM/yyyy}\n" +
            $"Tình trạng: {r.Description}\n" +
            $"Kỹ thuật viên: {r.TechnicianName}\n" +
            $"Ghi chú: {r.RepairNotes}\n" +
            $"Trạng thái: {r.Status}"
        ));

        var summary = await _garageAiService.SummarizeAsync(history);

        TempData["AiSummary"] = summary;

        return RedirectToAction(nameof(History), new { vehicleId });
    }

}
