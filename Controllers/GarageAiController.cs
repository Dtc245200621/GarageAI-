
using GarageAI.Data;
using GarageAI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class GarageAiController : Controller
{
    private readonly GarageAiService _ai;
    private readonly GarageDbContext _db;

    public GarageAiController(
        GarageAiService ai,
        GarageDbContext db)
    {
        _ai = ai;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        string? problem,
        string? services,
        string? serviceName,
        string? description,
        string? actionType)
    {
        ViewBag.Problem = problem;
        ViewBag.Services = services;
        ViewBag.ServiceName = serviceName;
        ViewBag.Description = description;

        if (actionType == "quotation")
        {
            ViewBag.Result = await _ai.SuggestQuotationAsync(
                problem ?? "", services ?? "");
        }
        else if (actionType == "explain")
        {
            ViewBag.Result = await _ai.ExplainServiceAsync(
                serviceName ?? "", description ?? "");
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SummarizeHistory(int vehicleId)
    {
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle == null)
            return NotFound();

        var orders = await _db.RepairOrders
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var history = string.Join("\n\n", orders.Select(r =>
            $"Ngày: {r.CreatedAt:dd/MM/yyyy}\n" +
            $"Tình trạng xe: {r.Description}\n" +
            $"Ghi chú sửa chữa: {r.RepairNotes}\n" +
            $"Trạng thái: {r.Status}"));

        ViewBag.LicensePlate = vehicle.LicensePlate;
        ViewBag.Result = await _ai.SummarizeAsync(history);

        return View();
    }
}
