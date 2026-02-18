using Microsoft.AspNetCore.Identity;
using StockSystem.Domain.Entities;

namespace StockSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        var roles = new[] { "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var adminEmail = "admin@stocksystem.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed categories
        if (!context.Categories.Any())
        {
            var categories = new[]
            {
                new Category { Name = "Hand Tools", Description = "Manual tools including hammers, screwdrivers, wrenches" },
                new Category { Name = "Power Tools", Description = "Electric and battery-powered tools" },
                new Category { Name = "Plumbing", Description = "Pipes, fittings, valves, and plumbing supplies" },
                new Category { Name = "Electrical", Description = "Wires, switches, outlets, and electrical components" },
                new Category { Name = "Paint & Supplies", Description = "Paints, brushes, rollers, and painting accessories" },
                new Category { Name = "Hardware", Description = "Nuts, bolts, screws, nails, and fasteners" },
                new Category { Name = "Safety Equipment", Description = "Protective gear and safety supplies" },
                new Category { Name = "Garden & Outdoor", Description = "Gardening tools and outdoor equipment" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed suppliers
        if (!context.Suppliers.Any())
        {
            var suppliers = new[]
            {
                new Supplier 
                { 
                    Name = "ToolPro Distributors", 
                    ContactName = "John Smith", 
                    Email = "john@toolpro.com", 
                    Phone = "555-0101", 
                    Address = "123 Industrial Ave, Chicago, IL" 
                },
                new Supplier 
                { 
                    Name = "BuildRight Supply Co", 
                    ContactName = "Sarah Johnson", 
                    Email = "sarah@buildright.com", 
                    Phone = "555-0102", 
                    Address = "456 Commerce St, Detroit, MI" 
                },
                new Supplier 
                { 
                    Name = "Hardware Wholesale Inc", 
                    ContactName = "Mike Brown", 
                    Email = "mike@hwwholesale.com", 
                    Phone = "555-0103", 
                    Address = "789 Market Blvd, Cleveland, OH" 
                }
            };

            context.Suppliers.AddRange(suppliers);
            await context.SaveChangesAsync();
        }

        // Seed products
        if (!context.Products.Any())
        {
            var products = new[]
            {
                new Product { Name = "Claw Hammer 16oz", Description = "Professional grade claw hammer with fiberglass handle", SKU = "HT-HAM-001", Price = 24.99m, Quantity = 50, CategoryId = 1, SupplierId = 1, MinStockLevel = 10 },
                new Product { Name = "Phillips Screwdriver Set", Description = "6-piece Phillips screwdriver set with magnetic tips", SKU = "HT-SCR-001", Price = 19.99m, Quantity = 35, CategoryId = 1, SupplierId = 1, MinStockLevel = 15 },
                new Product { Name = "Adjustable Wrench 10\"", Description = "Chrome vanadium adjustable wrench", SKU = "HT-WRN-001", Price = 15.99m, Quantity = 8, CategoryId = 1, SupplierId = 1, MinStockLevel = 10 },
                new Product { Name = "Cordless Drill 20V", Description = "20V lithium-ion cordless drill with 2 batteries", SKU = "PT-DRL-001", Price = 149.99m, Quantity = 20, CategoryId = 2, SupplierId = 2, MinStockLevel = 5 },
                new Product { Name = "Circular Saw 7-1/4\"", Description = "15-amp circular saw with laser guide", SKU = "PT-SAW-001", Price = 89.99m, Quantity = 12, CategoryId = 2, SupplierId = 2, MinStockLevel = 5 },
                new Product { Name = "Angle Grinder 4-1/2\"", Description = "7-amp angle grinder with paddle switch", SKU = "PT-GRN-001", Price = 59.99m, Quantity = 3, CategoryId = 2, SupplierId = 2, MinStockLevel = 5 },
                new Product { Name = "PVC Pipe 1\" x 10ft", Description = "Schedule 40 PVC pipe for plumbing", SKU = "PL-PIP-001", Price = 4.99m, Quantity = 100, CategoryId = 3, SupplierId = 3, MinStockLevel = 50 },
                new Product { Name = "Copper Elbow 1/2\"", Description = "90-degree copper elbow fitting", SKU = "PL-FIT-001", Price = 2.49m, Quantity = 200, CategoryId = 3, SupplierId = 3, MinStockLevel = 100 },
                new Product { Name = "Electrical Wire 14/2", Description = "14-gauge 2-conductor Romex wire, 250ft", SKU = "EL-WIR-001", Price = 89.99m, Quantity = 25, CategoryId = 4, SupplierId = 3, MinStockLevel = 10 },
                new Product { Name = "Outlet Receptacle", Description = "15-amp duplex outlet, white", SKU = "EL-OUT-001", Price = 1.99m, Quantity = 150, CategoryId = 4, SupplierId = 3, MinStockLevel = 75 },
                new Product { Name = "Interior Paint - White", Description = "Premium interior latex paint, 1 gallon", SKU = "PA-INT-001", Price = 34.99m, Quantity = 40, CategoryId = 5, SupplierId = 2, MinStockLevel = 20 },
                new Product { Name = "Paint Roller Kit", Description = "9\" roller with tray and extension pole", SKU = "PA-ROL-001", Price = 12.99m, Quantity = 5, CategoryId = 5, SupplierId = 2, MinStockLevel = 10 },
                new Product { Name = "Hex Bolt Assortment", Description = "200-piece hex bolt and nut assortment", SKU = "HW-BLT-001", Price = 29.99m, Quantity = 30, CategoryId = 6, SupplierId = 1, MinStockLevel = 15 },
                new Product { Name = "Wood Screws #8 x 2\"", Description = "Box of 100 wood screws", SKU = "HW-SCW-001", Price = 8.99m, Quantity = 75, CategoryId = 6, SupplierId = 1, MinStockLevel = 40 },
                new Product { Name = "Safety Glasses", Description = "ANSI-rated safety glasses, clear lens", SKU = "SF-GLS-001", Price = 9.99m, Quantity = 60, CategoryId = 7, SupplierId = 2, MinStockLevel = 25 },
                new Product { Name = "Work Gloves - Leather", Description = "Premium leather work gloves, large", SKU = "SF-GLV-001", Price = 19.99m, Quantity = 45, CategoryId = 7, SupplierId = 2, MinStockLevel = 20 },
                new Product { Name = "Garden Hose 50ft", Description = "Heavy-duty rubber garden hose", SKU = "GD-HOS-001", Price = 39.99m, Quantity = 18, CategoryId = 8, SupplierId = 3, MinStockLevel = 8 },
                new Product { Name = "Pruning Shears", Description = "Bypass pruning shears with ergonomic grip", SKU = "GD-PRN-001", Price = 14.99m, Quantity = 22, CategoryId = 8, SupplierId = 3, MinStockLevel = 10 }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
