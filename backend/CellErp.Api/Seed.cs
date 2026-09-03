using Microsoft.EntityFrameworkCore;

namespace CellErp.Api;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Users.IgnoreQueryFilters().AnyAsync()) return;

        var store = new Store
        {
            Name = "Demo Store",
            Nit = "900000000-1",
            City = "Bogotá",
            Phone = "+57 300 000 0000",
            Email = "demo@cellerp.local",
            LicenseExpiresAt = DateTime.UtcNow.AddYears(1)
        };
        db.Stores.Add(store);

        var super = new AppUser
        {
            Username = "admin",
            DisplayName = "Super Admin",
            PasswordHash = PasswordService.Hash("admin123"),
            Role = Roles.SuperAdmin,
            PermissionsCsv = Permissions.ManageStores,
            Theme = "dark",
            Language = "es"
        };
        var owner = new AppUser
        {
            StoreId = store.Id,
            Username = "demo",
            DisplayName = "Administrador Demo Store",
            PasswordHash = PasswordService.Hash("demo123"),
            Role = Roles.Owner,
            PermissionsCsv = string.Join(',', Permissions.AllStore),
            Theme = "dark",
            Language = "es"
        };
        db.Users.AddRange(super, owner);

        var supplier = new Supplier { StoreId = store.Id, Name = "Distribuciones Móvil SAS", Nit = "901111111-2", Phone = "3001112233" };
        var customer = new Customer { StoreId = store.Id, Name = "Cliente Demo", Phone = "3005558899", Email = "cliente@example.com" };
        db.Suppliers.Add(supplier); db.Customers.Add(customer);

        var accessory = new Product { StoreId = store.Id, Type = "Accessory", Name = "Cargador USB-C 30W", Sku = "ACC-USB30", Barcode = "770000000001", Category = "Cargadores", Brand = "Genérico", Cost = 30000, Price = 59000, Stock = 12, MinStock = 4, SupplierId = supplier.Id, Location = "A-01", WarrantyDays = 90 };
        var phoneProduct = new Product { StoreId = store.Id, Type = "Phone", Name = "iPhone 15 Pro 256GB", Sku = "IPH15P256", Category = "Celulares", Brand = "Apple", Cost = 3600000, Price = 4290000, Stock = 1, MinStock = 1, SupplierId = supplier.Id, WarrantyDays = 365 };
        db.Products.AddRange(accessory, phoneProduct);

        db.PhoneUnits.Add(new PhoneUnit { StoreId = store.Id, ProductId = phoneProduct.Id, Brand = "Apple", Model = "iPhone 15 Pro", Capacity = "256GB", Ram = "8GB", Color = "Titanio natural", Imei1 = "356000000000001", ConditionType = "New", PurchaseCost = 3600000, SalePrice = 4290000, SupplierId = supplier.Id, PurchaseDate = DateTime.UtcNow.AddDays(-8), PurchaseInvoiceNumber = "DEMO-001", WarrantyDays = 365 });

        var list = new PriceList { StoreId = store.Id, Name = "Cliente final", Description = "Lista de precio predeterminada" };
        db.PriceLists.Add(list);
        db.PriceListItems.AddRange(
            new PriceListItem { StoreId = store.Id, PriceListId = list.Id, ProductId = accessory.Id, Price = accessory.Price },
            new PriceListItem { StoreId = store.Id, PriceListId = list.Id, ProductId = phoneProduct.Id, Price = phoneProduct.Price });

        db.FinancingPlatforms.AddRange(
            new FinancingPlatform { StoreId = store.Id, Name = "Plataforma A" },
            new FinancingPlatform { StoreId = store.Id, Name = "Plataforma B" });

        var tech = new Technician { StoreId = store.Id, Name = "Técnico Demo", Phone = "3002223344", CommissionPercent = 10 };
        db.Technicians.Add(tech);

        var repair = new RepairOrder
        {
            StoreId = store.Id, Number = "REP-000001", SecurityCode = "4812", CustomerId = customer.Id,
            CustomerPhone = customer.Phone ?? "", Device = "Samsung Galaxy S22", Brand = "Samsung", Model = "S22",
            ImeiOrSerial = "351000000000001", PhysicalState = "Uso normal", AccessoriesReceived = "Sin accesorios",
            ReportedDamage = "No carga", Diagnosis = "Pendiente", TechnicianId = tech.Id, Price = 120000, Balance = 120000,
            EstimatedAt = DateTime.UtcNow.AddDays(2), PublicNotes = "Equipo recibido y pendiente de diagnóstico", Status = "Received"
        };
        db.RepairOrders.Add(repair);

        db.NotificationPreferences.Add(new NotificationPreference { StoreId = store.Id, UserId = owner.Id, DailySummary = true });
        db.Notifications.Add(new Notification { StoreId = store.Id, UserId = owner.Id, EventType = "welcome", Title = "Demo Store lista", Message = "La tienda de prueba quedó inicializada correctamente." });

        await db.SaveChangesAsync();
    }
}
