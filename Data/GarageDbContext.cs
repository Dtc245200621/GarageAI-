
using GarageAI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GarageAI.Data;

public class GarageDbContext(
    DbContextOptions<GarageDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<RepairOrderService> RepairOrderServices
    => Set<RepairOrderService>();

    public DbSet<RepairOrderPart> RepairOrderParts
        => Set<RepairOrderPart>();
    public DbSet<GarageService> GarageServices => Set<GarageService>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.LicensePlate)
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Vehicle)
            .WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.VehicleId);

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.AppointmentDate);

        modelBuilder.Entity<RepairOrder>()
            .HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GarageService>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Part>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Part>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<RepairOrder>()
            .HasIndex(r => r.VehicleId);
        modelBuilder.Entity<RepairOrderService>()
    .HasOne(x => x.RepairOrder)
    .WithMany()
    .HasForeignKey(x => x.RepairOrderId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrderService>()
            .HasOne(x => x.GarageService)
            .WithMany()
            .HasForeignKey(x => x.GarageServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrderPart>()
            .HasOne(x => x.RepairOrder)
            .WithMany()
            .HasForeignKey(x => x.RepairOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrderPart>()
            .HasOne(x => x.Part)
            .WithMany()
            .HasForeignKey(x => x.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
    .HasOne(i => i.RepairOrder)
    .WithMany()
    .HasForeignKey(i => i.RepairOrderId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.RepairOrderId)
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);
    }
}
