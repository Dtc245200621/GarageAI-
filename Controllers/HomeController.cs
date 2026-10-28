
using GarageAI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class HomeController : Controller
{
    private readonly GarageDbContext _db;

    public HomeController(GarageDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalCustomers =
            await _db.Customers.CountAsync();

        ViewBag.TotalVehicles =
            await _db.Vehicles.CountAsync();

        ViewBag.TotalAppointments =
            await _db.Appointments.CountAsync();

        ViewBag.TotalRepairOrders =
            await _db.RepairOrders.CountAsync();

        ViewBag.RepairingOrders =
            await _db.RepairOrders.CountAsync(
                r => r.Status == "DangSuaChua");

        ViewBag.CompletedOrders =
            await _db.RepairOrders.CountAsync(
                r => r.Status == "HoanThanh");

        return View();
    }
}
