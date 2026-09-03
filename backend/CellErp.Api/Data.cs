using Microsoft.EntityFrameworkCore;

namespace CellErp.Api;

public sealed class TenantContext(IHttpContextAccessor accessor)
{
    public Guid? StoreId
    {
        get
        {
            var raw = accessor.HttpContext?.User.FindFirst("store_id")?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var raw = accessor.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, TenantContext tenant) : DbContext(options)
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PhoneUnit> PhoneUnits => Set<PhoneUnit>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();
    public DbSet<AccountPayable> AccountsPayable => Set<AccountPayable>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<FinancingPlatform> FinancingPlatforms => Set<FinancingPlatform>();
    public DbSet<FinancedSale> FinancedSales => Set<FinancedSale>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<TechnicianLoan> TechnicianLoans => Set<TechnicianLoan>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<RepairPartUsage> RepairPartUsages => Set<RepairPartUsage>();
    public DbSet<Warranty> Warranties => Set<Warranty>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DashboardLayout> DashboardLayouts => Set<DashboardLayout>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        b.Entity<Store>().HasIndex(x => x.Name);
        b.Entity<Product>().HasIndex(x => new { x.StoreId, x.Sku }).IsUnique();
        b.Entity<PhoneUnit>().HasIndex(x => new { x.StoreId, x.Imei1 }).IsUnique();
        b.Entity<PurchaseInvoice>().HasIndex(x => new { x.StoreId, x.InvoiceNumber }).IsUnique();
        b.Entity<Sale>().HasIndex(x => new { x.StoreId, x.Number }).IsUnique();
        b.Entity<RepairOrder>().HasIndex(x => new { x.StoreId, x.Number }).IsUnique();

        Filter<Customer>(b); Filter<Supplier>(b); Filter<Product>(b); Filter<PhoneUnit>(b);
        Filter<PriceList>(b); Filter<PriceListItem>(b); Filter<PurchaseInvoice>(b); Filter<PurchaseInvoiceLine>(b);
        Filter<AccountPayable>(b); Filter<SupplierPayment>(b); Filter<InventoryMovement>(b); Filter<Sale>(b);
        Filter<SaleLine>(b); Filter<FinancingPlatform>(b); Filter<FinancedSale>(b); Filter<Commission>(b);
        Filter<CashSession>(b); Filter<CashMovement>(b); Filter<Technician>(b); Filter<TechnicianLoan>(b);
        Filter<RepairOrder>(b); Filter<RepairPartUsage>(b); Filter<Warranty>(b); Filter<Notification>(b);
        Filter<NotificationPreference>(b); Filter<DashboardLayout>(b);
    }

    private void Filter<T>(ModelBuilder b) where T : TenantEntity
        => b.Entity<T>().HasQueryFilter(x => tenant.StoreId.HasValue && x.StoreId == tenant.StoreId.Value);
}

public static class ConnectionStringHelper
{
    public static string Resolve(IConfiguration cfg)
    {
        var direct = Environment.GetEnvironmentVariable("DATABASE_URL") ?? cfg.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(direct)) throw new InvalidOperationException("Database connection is missing.");
        if (!direct.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !direct.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)) return direct;

        var uri = new Uri(direct);
        var userInfo = uri.UserInfo.Split(':', 2);
        var database = uri.AbsolutePath.TrimStart('/');
        return $"Host={uri.Host};Port={uri.Port};Database={database};Username={Uri.UnescapeDataString(userInfo[0])};Password={Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? "")};SSL Mode=Require;Trust Server Certificate=true";
    }
}
