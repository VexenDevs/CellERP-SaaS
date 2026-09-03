namespace CellErp.Api;

public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Store
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? LogoUrl { get; set; }
    public string? Nit { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string Currency { get; set; } = "COP";
    public string Language { get; set; } = "es";
    public string TimeZone { get; set; } = "America/Bogota";
    public string Plan { get; set; } = "Full";
    public string ModulesCsv { get; set; } = "store,service,inventory,sales,cash,reports,notifications";
    public string AccentColor { get; set; } = "#d97706";
    public bool IsActive { get; set; } = true;
    public DateTime? LicenseExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? StoreId { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = Roles.Admin;
    public string PermissionsCsv { get; set; } = "";
    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "es";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Customer : TenantEntity
{
    public string Name { get; set; } = "";
    public string? Document { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PriceListId { get; set; }
    public string? Notes { get; set; }
}

public sealed class Supplier : TenantEntity
{
    public string Name { get; set; } = "";
    public string? Nit { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public sealed class Product : TenantEntity
{
    public string Type { get; set; } = "Accessory";
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
    public decimal MinStock { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Location { get; set; }
    public int WarrantyDays { get; set; }
    public bool Active { get; set; } = true;
}

public sealed class PhoneUnit : TenantEntity
{
    public Guid ProductId { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public string? Variant { get; set; }
    public string? Capacity { get; set; }
    public string? Ram { get; set; }
    public string? Color { get; set; }
    public string Imei1 { get; set; } = "";
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string ConditionType { get; set; } = "New";
    public string? ConditionNotes { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalePrice { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public string? PurchaseInvoiceNumber { get; set; }
    public int WarrantyDays { get; set; }
    public string InventoryStatus { get; set; } = "Available";
    public Guid? SoldToCustomerId { get; set; }
    public Guid? SoldByUserId { get; set; }
    public Guid? SaleId { get; set; }
    public DateTime? SoldAt { get; set; }
    public decimal? SoldPrice { get; set; }
    public string? Notes { get; set; }
}

public sealed class PriceList : TenantEntity
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool Active { get; set; } = true;
}

public sealed class PriceListItem : TenantEntity
{
    public Guid PriceListId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
}

public sealed class PurchaseInvoice : TenantEntity
{
    public string InvoiceNumber { get; set; } = "";
    public Guid SupplierId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string PaymentTerms { get; set; } = "Cash";
    public string Status { get; set; } = "Paid";
}

public sealed class PurchaseInvoiceLine : TenantEntity
{
    public Guid PurchaseInvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class AccountPayable : TenantEntity
{
    public Guid SupplierId { get; set; }
    public Guid PurchaseInvoiceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Pending";
}

public sealed class SupplierPayment : TenantEntity
{
    public Guid AccountPayableId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public string? Reference { get; set; }
    public Guid UserId { get; set; }
}

public sealed class InventoryMovement : TenantEntity
{
    public Guid ProductId { get; set; }
    public string Type { get; set; } = "Adjustment";
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = "";
    public string? Reference { get; set; }
    public Guid UserId { get; set; }
}

public sealed class Sale : TenantEntity
{
    public string Number { get; set; } = "";
    public Guid? CustomerId { get; set; }
    public Guid UserId { get; set; }
    public Guid? PriceListId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public bool IsFinanced { get; set; }
    public Guid? FinancingPlatformId { get; set; }
    public string? FinancingReference { get; set; }
    public string Status { get; set; } = "Completed";
}

public sealed class SaleLine : TenantEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? PhoneUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class FinancingPlatform : TenantEntity
{
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
}

public sealed class FinancedSale : TenantEntity
{
    public Guid SaleId { get; set; }
    public Guid PlatformId { get; set; }
    public string Reference { get; set; } = "";
    public decimal ExpectedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "Pending";
}

public sealed class Commission : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? RepairOrderId { get; set; }
    public decimal Amount { get; set; }
    public string Rule { get; set; } = "Fixed";
    public string Status { get; set; } = "Pending";
    public DateTime? PaidAt { get; set; }
}

public sealed class CashSession : TenantEntity
{
    public Guid UserId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public decimal OpeningBase { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal? ExpectedTotal { get; set; }
    public decimal? CountedTotal { get; set; }
    public decimal? Difference { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Open";
}

public sealed class CashMovement : TenantEntity
{
    public Guid CashSessionId { get; set; }
    public Guid UserId { get; set; }
    public string Direction { get; set; } = "In";
    public string Type { get; set; } = "Other";
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public sealed class Technician : TenantEntity
{
    public Guid? UserId { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public decimal CommissionPercent { get; set; }
    public bool Active { get; set; } = true;
}

public sealed class TechnicianLoan : TenantEntity
{
    public Guid TechnicianId { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public string Reason { get; set; } = "";
    public int? Installments { get; set; }
    public string Status { get; set; } = "Open";
}

public sealed class RepairOrder : TenantEntity
{
    public string Number { get; set; } = "";
    public string SecurityCode { get; set; } = "";
    public Guid CustomerId { get; set; }
    public string CustomerPhone { get; set; } = "";
    public string Device { get; set; } = "";
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ImeiOrSerial { get; set; }
    public string? PhysicalState { get; set; }
    public string? AccessoriesReceived { get; set; }
    public string ReportedDamage { get; set; } = "";
    public string? Diagnosis { get; set; }
    public Guid? TechnicianId { get; set; }
    public decimal LaborCost { get; set; }
    public decimal Price { get; set; }
    public decimal Advance { get; set; }
    public decimal Balance { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EstimatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int WarrantyDays { get; set; }
    public string? Notes { get; set; }
    public string? PublicNotes { get; set; }
    public string Status { get; set; } = "Received";
}

public sealed class RepairPartUsage : TenantEntity
{
    public Guid RepairOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class Warranty : TenantEntity
{
    public string Type { get; set; } = "Sale";
    public Guid? SaleId { get; set; }
    public Guid? RepairOrderId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? ImeiOrSerial { get; set; }
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string? Notes { get; set; }
}

public sealed class Notification : TenantEntity
{
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = "General";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
}

public sealed class NotificationPreference : TenantEntity
{
    public Guid UserId { get; set; }
    public bool TelegramConnected { get; set; }
    public string? TelegramChatId { get; set; }
    public string EventsCsv { get; set; } = "sale,phone_sale,low_stock,repair_ready,cash_close,cash_difference,financing_pending";
    public bool DailySummary { get; set; }
    public string DailySummaryTime { get; set; } = "20:00";
}

public sealed class DashboardLayout : TenantEntity
{
    public Guid UserId { get; set; }
    public string Json { get; set; } = "[]";
}

public static class Roles
{
    public const string SuperAdmin = "SUPERADMIN";
    public const string Owner = "OWNER";
    public const string Admin = "ADMINISTRATOR";
    public const string Seller = "SELLER";
    public const string Technician = "TECHNICIAN";
    public const string Reception = "RECEPTION";
}

public static class Permissions
{
    public const string ManageStores = "stores.manage";
    public const string ManageUsers = "users.manage";
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";
    public const string SalesRead = "sales.read";
    public const string SalesWrite = "sales.write";
    public const string CashRead = "cash.read";
    public const string CashWrite = "cash.write";
    public const string RepairsRead = "repairs.read";
    public const string RepairsWrite = "repairs.write";
    public const string SuppliersRead = "suppliers.read";
    public const string SuppliersWrite = "suppliers.write";
    public const string CustomersRead = "customers.read";
    public const string CustomersWrite = "customers.write";
    public const string ReportsRead = "reports.read";
    public const string SettingsWrite = "settings.write";

    public static readonly string[] AllStore =
    [
        ManageUsers, InventoryRead, InventoryWrite, SalesRead, SalesWrite, CashRead, CashWrite,
        RepairsRead, RepairsWrite, SuppliersRead, SuppliersWrite, CustomersRead, CustomersWrite,
        ReportsRead, SettingsWrite
    ];
}
