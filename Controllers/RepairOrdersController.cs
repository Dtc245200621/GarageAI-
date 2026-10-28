
using GarageAI.Data;
using GarageAI.Services;
using GarageAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
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
    [HttpGet]
    public async Task<IActionResult> Services(int id)
    {
        var order = await _db.RepairOrders
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (order == null)
            return NotFound();

        var services = await _db.RepairOrderServices
            .Include(x => x.GarageService)
            .Where(x => x.RepairOrderId == id)
            .ToListAsync();

        ViewBag.Order = order;
        ViewBag.AvailableServices = await _db.GarageServices
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(services);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddService(
        int repairOrderId, int garageServiceId)
    {
        var order = await _db.RepairOrders
            .FindAsync(repairOrderId);

        if (order == null)
            return NotFound();

        if (order.Status == "DaHuy" ||
            order.Status == "HoanThanh")
        {
            TempData["Error"] =
                "Không thể thêm dịch vụ vào phiếu đã đóng.";
            return RedirectToAction(nameof(Services),
                new { id = repairOrderId });
        }

        var service = await _db.GarageServices
            .FindAsync(garageServiceId);

        if (service == null)
        {
            TempData["Error"] = "Dịch vụ không tồn tại.";
            return RedirectToAction(nameof(Services),
                new { id = repairOrderId });
        }

        bool exists = await _db.RepairOrderServices
            .AnyAsync(x =>
                x.RepairOrderId == repairOrderId &&
                x.GarageServiceId == garageServiceId);

        if (exists)
        {
            TempData["Error"] =
                "Dịch vụ này đã được thêm vào phiếu.";
            return RedirectToAction(nameof(Services),
                new { id = repairOrderId });
        }

        var item = new RepairOrderService
        {
            RepairOrderId = repairOrderId,
            GarageServiceId = garageServiceId,
            UnitPrice = service.Price
        };

        _db.RepairOrderServices.Add(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thêm dịch vụ thành công.";

        return RedirectToAction(nameof(Services),
            new { id = repairOrderId });
    }
    [HttpGet]
    public async Task<IActionResult> Parts(int id)
    {
        var order = await _db.RepairOrders
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (order == null)
            return NotFound();

        var parts = await _db.RepairOrderParts
            .Include(x => x.Part)
            .Where(x => x.RepairOrderId == id)
            .ToListAsync();

        ViewBag.Order = order;

        ViewBag.AvailableParts = await _db.Parts
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(parts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(
        int repairOrderId, int partId, int quantity)
    {
        IActionResult Back() => RedirectToAction(
            nameof(Parts), new { id = repairOrderId });

        if (quantity <= 0)
        {
            TempData["Error"] = "Số lượng phải lớn hơn 0.";
            return Back();
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var order = await _db.RepairOrders
            .FindAsync(repairOrderId);

        if (order == null)
            return NotFound();

        if (order.Status == "DaHuy" ||
            order.Status == "HoanThanh")
        {
            TempData["Error"] =
                "Không thể thêm phụ tùng vào phiếu đã đóng.";
            return Back();
        }

        var part = await _db.Parts.FindAsync(partId);

        if (part == null)
        {
            TempData["Error"] = "Phụ tùng không tồn tại.";
            return Back();
        }

        if (part.StockQuantity < quantity)
        {
            TempData["Error"] =
                "Số lượng phụ tùng vượt quá tồn kho.";
            return Back();
        }

        var existing = await _db.RepairOrderParts
            .FirstOrDefaultAsync(x =>
                x.RepairOrderId == repairOrderId &&
                x.PartId == partId);

        if (existing == null)
        {
            _db.RepairOrderParts.Add(new RepairOrderPart
            {
                RepairOrderId = repairOrderId,
                PartId = partId,
                Quantity = quantity,
                UnitPrice = part.Price
            });
        }
        else
        {
            existing.Quantity += quantity;
        }

        part.StockQuantity -= quantity;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Success"] = "Thêm phụ tùng thành công.";
        return Back();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePart(int id)
    {
        var item = await _db.RepairOrderParts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        int repairOrderId = item.RepairOrderId;

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var order = await _db.RepairOrders
            .FindAsync(repairOrderId);

        if (order == null)
            return NotFound();

        if (order.Status == "HoanThanh" ||
            order.Status == "DaHuy")
        {
            TempData["Error"] =
                "Không thể xóa phụ tùng khỏi phiếu đã đóng.";

            return RedirectToAction(nameof(Parts),
                new { id = repairOrderId });
        }

        var part = await _db.Parts.FindAsync(item.PartId);

        if (part == null)
            return NotFound();

        part.StockQuantity += item.Quantity;

        _db.RepairOrderParts.Remove(item);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Success"] =
            "Xóa phụ tùng và hoàn trả tồn kho thành công.";

        return RedirectToAction(nameof(Parts),
            new { id = repairOrderId });
    }


    [HttpGet]
    public async Task<IActionResult> TotalCost(int id)
    {
        var order = await _db.RepairOrders
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (order == null)
            return NotFound();

        // Lấy dữ liệu dịch vụ từ SQLite
        var services = await _db.RepairOrderServices
            .Where(x => x.RepairOrderId == id)
            .ToListAsync();

        // Lấy dữ liệu phụ tùng từ SQLite
        var parts = await _db.RepairOrderParts
            .Where(x => x.RepairOrderId == id)
            .ToListAsync();

        // Tính tổng bằng C# sau khi lấy dữ liệu
        decimal serviceTotal = services.Sum(x => x.UnitPrice);

        decimal partTotal = parts.Sum(
            x => x.Quantity * x.UnitPrice);

        decimal grandTotal = serviceTotal + partTotal;

        ViewBag.Order = order;
        ViewBag.ServiceTotal = serviceTotal;
        ViewBag.PartTotal = partTotal;
        ViewBag.GrandTotal = grandTotal;

        return View();
    }



}
