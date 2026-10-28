
using GarageAI.Data;
using GarageAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class InvoicesController : Controller
{
    private readonly GarageDbContext _db;

    public InvoicesController(GarageDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var invoices = await _db.Invoices
            .Include(i => i.RepairOrder)
            .ThenInclude(r => r!.Vehicle)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(invoices);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int repairOrderId)
    {
        var order = await _db.RepairOrders
            .FindAsync(repairOrderId);

        if (order == null)
            return NotFound();

        if (order.Status != "HoanThanh")
        {
            TempData["Error"] = "Phiếu chưa hoàn thành.";
            return RedirectToAction("Index", "RepairOrders");
        }

        var existing = await _db.Invoices
            .FirstOrDefaultAsync(i =>
                i.RepairOrderId == repairOrderId);

        if (existing != null)
            return RedirectToAction(nameof(Index));

        var services = await _db.RepairOrderServices
            .Where(x => x.RepairOrderId == repairOrderId)
            .Select(x => x.UnitPrice)
            .ToListAsync();

        var parts = await _db.RepairOrderParts
            .Where(x => x.RepairOrderId == repairOrderId)
            .Select(x => new { x.Quantity, x.UnitPrice })
            .ToListAsync();

        decimal total = services.Sum() +
            parts.Sum(x => x.Quantity * x.UnitPrice);

        _db.Invoices.Add(new Invoice
        {
            RepairOrderId = repairOrderId,
            TotalAmount = total,
            CreatedAt = DateTime.Now,
            PaymentStatus = "ChuaThanhToan"
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Lập hóa đơn thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id)
    {
        var invoice = await _db.Invoices.FindAsync(id);

        if (invoice == null)
            return NotFound();

        if (invoice.PaymentStatus == "DaThanhToan")
            return RedirectToAction(nameof(Index));

        invoice.PaymentStatus = "DaThanhToan";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thanh toán thành công.";
        return RedirectToAction(nameof(Index));
    }
}
