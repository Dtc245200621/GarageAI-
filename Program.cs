
using GarageAI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<GarageAI.Services.GarageAiService>(
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(120);
    });


builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GarageDbContext>(options =>
    options.UseSqlite("Data Source=garage.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<GarageDbContext>();

    db.Database.EnsureCreated();

    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Vehicles" (
            "Id" INTEGER NOT NULL
                CONSTRAINT "PK_Vehicles" PRIMARY KEY AUTOINCREMENT,
            "LicensePlate" TEXT NOT NULL,
            "Brand" TEXT NOT NULL,
            "Model" TEXT NOT NULL,
            "Year" INTEGER NULL,
            "CustomerId" INTEGER NOT NULL,
            CONSTRAINT "FK_Vehicles_Customers_CustomerId"
                FOREIGN KEY ("CustomerId")
                REFERENCES "Customers" ("Id")
                ON DELETE RESTRICT
        );
        """);

    db.Database.ExecuteSqlRaw("""
    CREATE TABLE IF NOT EXISTS "RepairOrders" (
        "Id" INTEGER NOT NULL
            CONSTRAINT "PK_RepairOrders"
            PRIMARY KEY AUTOINCREMENT,

        "VehicleId" INTEGER NOT NULL,

        "CreatedAt" TEXT NOT NULL,

        "Description" TEXT NOT NULL,

        "TechnicianName" TEXT NOT NULL,

        "Status" TEXT NOT NULL,

        "RepairNotes" TEXT NOT NULL,

        CONSTRAINT "FK_RepairOrders_Vehicles_VehicleId"
            FOREIGN KEY ("VehicleId")
            REFERENCES "Vehicles" ("Id")
            ON DELETE RESTRICT
    );
    """);

    db.Database.ExecuteSqlRaw("""
    CREATE INDEX IF NOT EXISTS
    "IX_RepairOrders_VehicleId"
    ON "RepairOrders" ("VehicleId");
    """);


    db.Database.ExecuteSqlRaw("""
        CREATE UNIQUE INDEX IF NOT EXISTS
        "IX_Vehicles_LicensePlate"
        ON "Vehicles" ("LicensePlate");
        """);

    db.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS
        "IX_Vehicles_CustomerId"
        ON "Vehicles" ("CustomerId");
        """);

    db.Database.ExecuteSqlRaw("""
    CREATE TABLE IF NOT EXISTS "Appointments" (
        "Id" INTEGER NOT NULL
            CONSTRAINT "PK_Appointments"
            PRIMARY KEY AUTOINCREMENT,

        "VehicleId" INTEGER NOT NULL,

        "AppointmentDate" TEXT NOT NULL,

        "Description" TEXT NOT NULL,

        "Status" TEXT NOT NULL,

        CONSTRAINT "FK_Appointments_Vehicles_VehicleId"
            FOREIGN KEY ("VehicleId")
            REFERENCES "Vehicles" ("Id")
            ON DELETE RESTRICT
    );
    """);

    db.Database.ExecuteSqlRaw("""
    CREATE INDEX IF NOT EXISTS
    "IX_Appointments_VehicleId"
    ON "Appointments" ("VehicleId");
    """);

    db.Database.ExecuteSqlRaw("""
    CREATE INDEX IF NOT EXISTS
    "IX_Appointments_AppointmentDate"
    ON "Appointments" ("AppointmentDate");
    """);
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
