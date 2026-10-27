
using GarageAI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Controllers;

public class AppointmentsController : Controller
{
    private readonly GarageDbContext _db;

    public AppointmentsController(GarageDbContext db)
    {
        _db = db;
    }


    public async Task<IActionResult> Index(
        string? search,
        string? status,
        DateTime? date)
    {
        var query = _db.Appointments
            .Include(a => a.Vehicle)
            .ThenInclude(v => v!.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(a =>
                a.Vehicle != null &&
                (a.Vehicle.LicensePlate.Contains(search) ||
                 (a.Vehicle.Customer != null &&
                  a.Vehicle.Customer.FullName.Contains(search))));
        }

        var validStatuses = new[]
        {
        "ChoXacNhan",
        "DaXacNhan",
        "DaHuy",
        "HoanThanh"
    };

        if (!string.IsNullOrWhiteSpace(status) &&
            validStatuses.Contains(status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);

            query = query.Where(a =>
                a.AppointmentDate >= start &&
                a.AppointmentDate < end);
        }

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Date = date?.ToString("yyyy-MM-dd");
        ViewBag.Success = TempData["Success"];

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }


    private async Task LoadVehicles()
    {
        var vehicles = await _db.Vehicles
            .OrderBy(v => v.LicensePlate)
            .ToListAsync();

        ViewBag.Vehicles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            vehicles, "Id", "LicensePlate");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadVehicles();
        return View(new GarageAI.Models.Appointment
        {
            AppointmentDate = DateTime.Now.AddDays(1),
            Status = "ChoXacNhan"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        GarageAI.Models.Appointment appointment)
    {
        ModelState.Remove(nameof(GarageAI.Models.Appointment.Vehicle));

        if (appointment.AppointmentDate <= DateTime.Now)
        {
            ModelState.AddModelError(
                nameof(appointment.AppointmentDate),
                "Ngày hẹn phải ở tương lai");
        }

        if (!await _db.Vehicles.AnyAsync(
            v => v.Id == appointment.VehicleId))
        {
            ModelState.AddModelError(
                nameof(appointment.VehicleId),
                "Xe không tồn tại");
        }

        appointment.Status = "ChoXacNhan";

        if (!ModelState.IsValid)
        {
            await LoadVehicles();
            return View(appointment);
        }

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đặt lịch hẹn thành công";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status == "DaHuy" ||
            appointment.Status == "HoanThanh")
            return BadRequest("Không thể sửa lịch hẹn đã kết thúc.");

        await LoadVehicles();
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, GarageAI.Models.Appointment input)
    {
        if (id != input.Id)
            return BadRequest();

        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status == "DaHuy" ||
            appointment.Status == "HoanThanh")
            return BadRequest("Không thể sửa lịch hẹn đã kết thúc.");

        ModelState.Remove(nameof(GarageAI.Models.Appointment.Vehicle));

        if (input.AppointmentDate <= DateTime.Now)
            ModelState.AddModelError(
                nameof(input.AppointmentDate),
                "Ngày hẹn phải ở tương lai");

        if (!await _db.Vehicles.AnyAsync(
            v => v.Id == input.VehicleId))
            ModelState.AddModelError(
                nameof(input.VehicleId),
                "Xe không tồn tại");

        if (!ModelState.IsValid)
        {
            await LoadVehicles();
            return View(input);
        }

        appointment.VehicleId = input.VehicleId;
        appointment.AppointmentDate = input.AppointmentDate;
        appointment.Description = input.Description;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Cập nhật lịch hẹn thành công";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status != "ChoXacNhan")
        {
            TempData["Error"] =
                "Chỉ có thể xác nhận lịch đang chờ";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = "DaXacNhan";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Xác nhận lịch hẹn thành công";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status == "DaHuy" ||
            appointment.Status == "HoanThanh")
        {
            TempData["Error"] =
                "Không thể hủy lịch hẹn đã kết thúc";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = "DaHuy";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã hủy lịch hẹn";
        return RedirectToAction(nameof(Index));
    }
}
