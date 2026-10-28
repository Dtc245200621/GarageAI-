using GarageAI.Data;
using GarageAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GarageAI.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class CustomersController(GarageDbContext db) : Controller {
 public async Task<IActionResult> Index(string? q) {
  var customers = db.Customers.AsNoTracking().AsQueryable();
  if (!string.IsNullOrWhiteSpace(q)) customers = customers.Where(x => x.FullName.Contains(q) || x.Phone.Contains(q));
  ViewBag.Query = q;
  return View(await customers.OrderByDescending(x => x.Id).ToListAsync());
 }
 [HttpPost][ValidateAntiForgeryToken]
 public async Task<IActionResult> Create(Customer customer) {
  if (!ModelState.IsValid) { TempData["Error"]="Dữ liệu không hợp lệ, hãy kiểm tra họ tên, số điện thoại và email."; return RedirectToAction(nameof(Index)); }
  db.Customers.Add(customer); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
 }
 [HttpPost][ValidateAntiForgeryToken]
 public async Task<IActionResult> Delete(int id) {
  var customer = await db.Customers.FindAsync(id);
  if (customer != null) { db.Customers.Remove(customer); await db.SaveChangesAsync(); }
  return RedirectToAction(nameof(Index));
 }
}
