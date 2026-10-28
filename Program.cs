
using GarageAI.Data;
using Microsoft.EntityFrameworkCore;
using GarageAI.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<GarageAI.Services.GarageAiService>(
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(120);
    });


builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GarageDbContext>(options =>
    options.UseSqlite("Data Source=garage.db"));
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<GarageDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Employee"))
    {
        var roleResult = await roleManager.CreateAsync(
            new IdentityRole("Employee"));

        if (!roleResult.Succeeded)
            throw new Exception("Không thể tạo vai trò Employee.");
    }

    const string employeeEmail = "employee@garage.local";

    var employee = await userManager.FindByEmailAsync(employeeEmail);

    if (employee == null)
    {
        employee = new ApplicationUser
        {
            UserName = employeeEmail,
            Email = employeeEmail,
            FullName = "Nhân viên Garage",
            EmailConfirmed = true
        };

        var password = builder.Configuration["Seed:EmployeePassword"];

        if (string.IsNullOrWhiteSpace(password))
            throw new Exception("Chưa cấu hình Seed:EmployeePassword.");

        var result = await userManager.CreateAsync(employee, password);

        if (!result.Succeeded)
            throw new Exception(string.Join("; ",
                result.Errors.Select(e => e.Description)));
    }

    if (!await userManager.IsInRoleAsync(employee, "Employee"))
    {
        var result = await userManager.AddToRoleAsync(employee, "Employee");

        if (!result.Succeeded)
            throw new Exception(string.Join("; ",
                result.Errors.Select(e => e.Description)));
    }
}

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
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Employee" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Không thể tạo vai trò {role}: " +
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }
}
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    var adminEmail =
        Environment.GetEnvironmentVariable("GARAGE_ADMIN_EMAIL");

    var adminPassword =
        Environment.GetEnvironmentVariable("GARAGE_ADMIN_PASSWORD");

    if (!string.IsNullOrWhiteSpace(adminEmail) &&
        !string.IsNullOrWhiteSpace(adminPassword))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Quản trị viên",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                admin, adminPassword);

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
