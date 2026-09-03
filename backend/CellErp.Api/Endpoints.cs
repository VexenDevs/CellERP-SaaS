using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CellErp.Api;

public static class Endpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "cellerp-api", version = "0.1.0", utc = DateTime.UtcNow }));
        MapAuth(app);
        MapSuperAdmin(app);
        MapMasterData(app);
        MapInventoryAndSales(app);
        MapRepairsAndCash(app);
        MapDashboardAndSupport(app);
    }

    private static Guid StoreId(ClaimsPrincipal user)
        => Guid.Parse(user.FindFirst("store_id")?.Value ?? throw new UnauthorizedAccessException("Store context required"));

    private static async Task AddNotification(AppDbContext db, Guid storeId, Guid? userId, string type, string title, string message)
    {
        db.Notifications.Add(new Notification { StoreId = storeId, UserId = userId, EventType = type, Title = title, Message = message });
        await db.SaveChangesAsync();
    }

    private static string NextNumber(string prefix, int count) => $"{prefix}-{count + 1:000000}";

    private static void MapAuth(WebApplication app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db, JwtService jwt) =>
        {
            var user = await db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Username == req.Username);
            if (user is null || !user.IsActive || !PasswordService.Verify(req.Password, user.PasswordHash)) return Results.Unauthorized();
            if (user.StoreId.HasValue)
            {
                var store = await db.Stores.FindAsync(user.StoreId.Value);
                if (store is null || !store.IsActive || (store.LicenseExpiresAt.HasValue && store.LicenseExpiresAt.Value < DateTime.UtcNow))
                    return Results.Problem("Store disabled or license expired", statusCode: 403);
            }
            return Results.Ok(new { token = jwt.Create(user), user = new { user.Id, user.StoreId, user.Username, user.DisplayName, user.Role, user.PermissionsCsv, user.Theme, user.Language } });
        });

        app.MapGet("/api/auth/me", async (ClaimsPrincipal principal, AppDbContext db) =>
        {
            var id = principal.UserId();
            var user = await db.Users.IgnoreQueryFilters().SingleAsync(x => x.Id == id);
            Store? store = user.StoreId.HasValue ? await db.Stores.FindAsync(user.StoreId.Value) : null;
            return Results.Ok(new { user.Id, user.StoreId, user.Username, user.DisplayName, user.Role, user.PermissionsCsv, user.Theme, user.Language, store });
        }).RequireAuthorization();

        app.MapPut("/api/auth/profile", async (ProfileRequest req, ClaimsPrincipal principal, AppDbContext db) =>
        {
            var user = await db.Users.IgnoreQueryFilters().SingleAsync(x => x.Id == principal.UserId());
            user.Theme = req.Theme is "light" or "dark" ? req.Theme : user.Theme;
            user.Language = req.Language is "es" or "en" or "pt" ? req.Language : user.Language;
            await db.SaveChangesAsync();
            return Results.Ok(new { user.Theme, user.Language });
        }).RequireAuthorization();
    }

    private static void MapSuperAdmin(WebApplication app)
    {
        var group = app.MapGroup("/api/superadmin").RequireAuthorization();

        group.MapGet("/stores", async (ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.IsSuperAdmin()) return Results.Forbid();
            var stores = await db.Stores.OrderBy(x => x.Name).ToListAsync();
            var users = await db.Users.IgnoreQueryFilters().Where(x => x.StoreId != null).GroupBy(x => x.StoreId).Select(g => new { StoreId = g.Key, Count = g.Count() }).ToListAsync();
            return Results.Ok(stores.Select(s => new { s.Id, s.Name, s.Nit, s.City, s.Plan, s.ModulesCsv, s.IsActive, s.LicenseExpiresAt, Users = users.FirstOrDefault(x => x.StoreId == s.Id)?.Count ?? 0 }));
        });

        group.MapPost("/stores", async (StoreRequest req, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.IsSuperAdmin()) return Results.Forbid();
            var store = new Store { Name = req.Name, Nit = req.Nit, Address = req.Address, Phone = req.Phone, Email = req.Email, City = req.City, Currency = req.Currency, Language = req.Language, TimeZone = req.TimeZone, Plan = req.Plan, ModulesCsv = req.ModulesCsv, LicenseExpiresAt = req.LicenseExpiresAt, IsActive = req.IsActive };
            db.Stores.Add(store); await db.SaveChangesAsync(); return Results.Created($"/api/superadmin/stores/{store.Id}", store);
        });

        group.MapPut("/stores/{id:guid}", async (Guid id, StoreRequest req, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.IsSuperAdmin()) return Results.Forbid();
            var s = await db.Stores.FindAsync(id); if (s is null) return Results.NotFound();
            s.Name=req.Name; s.Nit=req.Nit; s.Address=req.Address; s.Phone=req.Phone; s.Email=req.Email; s.City=req.City; s.Currency=req.Currency; s.Language=req.Language; s.TimeZone=req.TimeZone; s.Plan=req.Plan; s.ModulesCsv=req.ModulesCsv; s.LicenseExpiresAt=req.LicenseExpiresAt; s.IsActive=req.IsActive;
            await db.SaveChangesAsync(); return Results.Ok(s);
        });

        group.MapGet("/stats", async (ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.IsSuperAdmin()) return Results.Forbid();
            var stores = await db.Stores.CountAsync();
            var active = await db.Stores.CountAsync(x => x.IsActive);
            var users = await db.Users.IgnoreQueryFilters().CountAsync();
            return Results.Ok(new { stores, activeStores = active, users, generatedAt = DateTime.UtcNow });
        });
    }

    private static void MapMasterData(WebApplication app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        api.MapGet("/users", async (ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.ManageUsers)) return Results.Forbid();
            var store = StoreId(p);
            var users = await db.Users.IgnoreQueryFilters().Where(x => x.StoreId == store).OrderBy(x => x.DisplayName).ToListAsync();
            return Results.Ok(users.Select(x => new { x.Id, x.Username, x.DisplayName, x.Role, x.PermissionsCsv, x.Theme, x.Language, x.IsActive, x.CreatedAt }));
        });
        api.MapPost("/users", async (UserRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.ManageUsers)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(r.Password) || r.Password.Length < 8) return Results.BadRequest(new { message = "Password must contain at least 8 characters" });
            if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Username == r.Username)) return Results.Conflict(new { message = "Username already exists" });
            var allowedRoles = new[] { Roles.Owner, Roles.Admin, Roles.Seller, Roles.Technician, Roles.Reception };
            if (!allowedRoles.Contains(r.Role)) return Results.BadRequest(new { message = "Invalid role" });
            var x = new AppUser { StoreId = StoreId(p), Username = r.Username.Trim(), DisplayName = r.DisplayName.Trim(), PasswordHash = PasswordService.Hash(r.Password), Role = r.Role, PermissionsCsv = r.PermissionsCsv, IsActive = r.IsActive };
            db.Users.Add(x); await db.SaveChangesAsync();
            return Results.Ok(new { x.Id, x.Username, x.DisplayName, x.Role, x.PermissionsCsv, x.IsActive });
        });

        api.MapGet("/customers", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.CustomersRead) ? Results.Ok(await db.Customers.OrderBy(x => x.Name).ToListAsync()) : Results.Forbid());
        api.MapPost("/customers", async (CustomerRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.CustomersWrite)) return Results.Forbid();
            var x = new Customer { StoreId=StoreId(p), Name=r.Name, Document=r.Document, Phone=r.Phone, Email=r.Email, PriceListId=r.PriceListId, Notes=r.Notes }; db.Customers.Add(x); await db.SaveChangesAsync(); return Results.Created($"/api/customers/{x.Id}", x);
        });
        api.MapPut("/customers/{id:guid}", async (Guid id, CustomerRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.CustomersWrite)) return Results.Forbid(); var x=await db.Customers.SingleOrDefaultAsync(x=>x.Id==id); if(x is null)return Results.NotFound(); x.Name=r.Name;x.Document=r.Document;x.Phone=r.Phone;x.Email=r.Email;x.PriceListId=r.PriceListId;x.Notes=r.Notes;x.UpdatedAt=DateTime.UtcNow;await db.SaveChangesAsync();return Results.Ok(x);
        });

        api.MapGet("/suppliers", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.SuppliersRead) ? Results.Ok(await db.Suppliers.OrderBy(x => x.Name).ToListAsync()) : Results.Forbid());
        api.MapPost("/suppliers", async (SupplierRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.SuppliersWrite)) return Results.Forbid(); var x=new Supplier{StoreId=StoreId(p),Name=r.Name,Nit=r.Nit,Phone=r.Phone,Email=r.Email,Address=r.Address,Notes=r.Notes};db.Suppliers.Add(x);await db.SaveChangesAsync();return Results.Created($"/api/suppliers/{x.Id}",x);
        });

        api.MapGet("/products", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.InventoryRead) ? Results.Ok(await db.Products.OrderBy(x=>x.Name).ToListAsync()) : Results.Forbid());
        api.MapPost("/products", async (ProductRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.InventoryWrite)) return Results.Forbid(); var x=new Product{StoreId=StoreId(p),Type=r.Type,Name=r.Name,Sku=r.Sku,Barcode=r.Barcode,Category=r.Category,Brand=r.Brand,Description=r.Description,Cost=r.Cost,Price=r.Price,Stock=r.Stock,MinStock=r.MinStock,SupplierId=r.SupplierId,Location=r.Location,WarrantyDays=r.WarrantyDays,Active=r.Active};db.Products.Add(x);await db.SaveChangesAsync();return Results.Created($"/api/products/{x.Id}",x);
        });
        api.MapPut("/products/{id:guid}", async (Guid id, ProductRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.InventoryWrite)) return Results.Forbid();var x=await db.Products.SingleOrDefaultAsync(x=>x.Id==id);if(x is null)return Results.NotFound();x.Type=r.Type;x.Name=r.Name;x.Sku=r.Sku;x.Barcode=r.Barcode;x.Category=r.Category;x.Brand=r.Brand;x.Description=r.Description;x.Cost=r.Cost;x.Price=r.Price;x.Stock=r.Stock;x.MinStock=r.MinStock;x.SupplierId=r.SupplierId;x.Location=r.Location;x.WarrantyDays=r.WarrantyDays;x.Active=r.Active;x.UpdatedAt=DateTime.UtcNow;await db.SaveChangesAsync();return Results.Ok(x);
        });

        api.MapGet("/phones", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.InventoryRead) ? Results.Ok(await db.PhoneUnits.OrderByDescending(x=>x.CreatedAt).ToListAsync()) : Results.Forbid());
        api.MapPost("/phones", async (PhoneUnitRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!p.HasPermission(Permissions.InventoryWrite)) return Results.Forbid(); var x=new PhoneUnit{StoreId=StoreId(p),ProductId=r.ProductId,Brand=r.Brand,Model=r.Model,Variant=r.Variant,Capacity=r.Capacity,Ram=r.Ram,Color=r.Color,Imei1=r.Imei1,Imei2=r.Imei2,SerialNumber=r.SerialNumber,ConditionType=r.ConditionType,ConditionNotes=r.ConditionNotes,PurchaseCost=r.PurchaseCost,SalePrice=r.SalePrice,SupplierId=r.SupplierId,PurchaseDate=r.PurchaseDate,PurchaseInvoiceNumber=r.PurchaseInvoiceNumber,WarrantyDays=r.WarrantyDays,Notes=r.Notes};db.PhoneUnits.Add(x);var product=await db.Products.SingleOrDefaultAsync(x=>x.Id==r.ProductId);if(product!=null)product.Stock+=1;await db.SaveChangesAsync();return Results.Created($"/api/phones/{x.Id}",x);
        });
        api.MapGet("/phones/imei/{imei}", async (string imei, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.InventoryRead))return Results.Forbid();var phone=await db.PhoneUnits.FirstOrDefaultAsync(x=>x.Imei1==imei||x.Imei2==imei);if(phone is null)return Results.NotFound();var supplier=phone.SupplierId.HasValue?await db.Suppliers.SingleOrDefaultAsync(x=>x.Id==phone.SupplierId.Value):null;var customer=phone.SoldToCustomerId.HasValue?await db.Customers.SingleOrDefaultAsync(x=>x.Id==phone.SoldToCustomerId.Value):null;var movements=await db.InventoryMovements.Where(x=>x.ProductId==phone.ProductId).OrderByDescending(x=>x.CreatedAt).Take(30).ToListAsync();return Results.Ok(new{phone,supplier,customer,movements});
        });

        api.MapGet("/price-lists", async (AppDbContext db) => Results.Ok(await db.PriceLists.OrderBy(x=>x.Name).ToListAsync()));
        api.MapPost("/price-lists", async (PriceListRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.SettingsWrite))return Results.Forbid();var store=StoreId(p);var list=new PriceList{StoreId=store,Name=r.Name,Description=r.Description,Active=r.Active};db.PriceLists.Add(list);foreach(var i in r.Items)db.PriceListItems.Add(new PriceListItem{StoreId=store,PriceListId=list.Id,ProductId=i.ProductId,Price=i.Price});await db.SaveChangesAsync();return Results.Ok(list);
        });
        api.MapGet("/price-lists/{id:guid}/items", async (Guid id, AppDbContext db) => Results.Ok(await db.PriceListItems.Where(x=>x.PriceListId==id).ToListAsync()));

        api.MapGet("/financing-platforms", async (AppDbContext db) => Results.Ok(await db.FinancingPlatforms.OrderBy(x=>x.Name).ToListAsync()));
        api.MapPost("/financing-platforms", async (FinancingPlatformRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.SettingsWrite))return Results.Forbid();var x=new FinancingPlatform{StoreId=StoreId(p),Name=r.Name,Active=r.Active};db.FinancingPlatforms.Add(x);await db.SaveChangesAsync();return Results.Ok(x);
        });
    }

    private static void MapInventoryAndSales(WebApplication app)
    {
        var api=app.MapGroup("/api").RequireAuthorization();

        api.MapPost("/purchases", async (PurchaseInvoiceRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.InventoryWrite))return Results.Forbid();if(r.Lines.Count==0)return Results.BadRequest(new{message="Invoice requires lines"});var store=StoreId(p);await using var tx=await db.Database.BeginTransactionAsync();var total=r.Lines.Sum(x=>x.Quantity*x.UnitCost);var inv=new PurchaseInvoice{StoreId=store,InvoiceNumber=r.InvoiceNumber,SupplierId=r.SupplierId,InvoiceDate=r.InvoiceDate,DueDate=r.DueDate,Total=total,PaymentMethod=r.PaymentMethod,PaymentTerms=r.PaymentTerms,Status=r.PaymentTerms.Equals("Credit",StringComparison.OrdinalIgnoreCase)?"Pending":"Paid"};db.PurchaseInvoices.Add(inv);foreach(var line in r.Lines){var product=await db.Products.SingleOrDefaultAsync(x=>x.Id==line.ProductId);if(product is null)return Results.BadRequest(new{message=$"Product {line.ProductId} not found"});product.Stock+=line.Quantity;product.Cost=line.UnitCost;db.PurchaseInvoiceLines.Add(new PurchaseInvoiceLine{StoreId=store,PurchaseInvoiceId=inv.Id,ProductId=line.ProductId,Quantity=line.Quantity,UnitCost=line.UnitCost,LineTotal=line.Quantity*line.UnitCost});db.InventoryMovements.Add(new InventoryMovement{StoreId=store,ProductId=line.ProductId,Type="Purchase",Quantity=line.Quantity,Reason="Supplier invoice",Reference=r.InvoiceNumber,UserId=p.UserId()});}if(inv.Status=="Pending")db.AccountsPayable.Add(new AccountPayable{StoreId=store,SupplierId=r.SupplierId,PurchaseInvoiceId=inv.Id,OriginalAmount=total,Balance=total,DueDate=r.DueDate});await db.SaveChangesAsync();await tx.CommitAsync();await AddNotification(db,store,p.UserId(),"purchase","Compra registrada",$"Factura {r.InvoiceNumber} por {total:N0}");return Results.Ok(inv);
        });
        api.MapGet("/purchases", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.InventoryRead)?Results.Ok(await db.PurchaseInvoices.OrderByDescending(x=>x.InvoiceDate).Take(100).ToListAsync()):Results.Forbid());
        api.MapGet("/accounts-payable", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.SuppliersRead)?Results.Ok(await db.AccountsPayable.OrderBy(x=>x.DueDate).ToListAsync()):Results.Forbid());
        api.MapPost("/accounts-payable/{id:guid}/payments", async (Guid id, PayablePaymentRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.SuppliersWrite))return Results.Forbid();var ap=await db.AccountsPayable.SingleOrDefaultAsync(x=>x.Id==id);if(ap is null)return Results.NotFound();if(r.Amount<=0||r.Amount>ap.Balance)return Results.BadRequest(new{message="Invalid payment amount"});ap.PaidAmount+=r.Amount;ap.Balance-=r.Amount;ap.Status=ap.Balance==0?"Paid":"Partial";db.SupplierPayments.Add(new SupplierPayment{StoreId=StoreId(p),AccountPayableId=id,Amount=r.Amount,Method=r.Method,Reference=r.Reference,UserId=p.UserId()});var session=await db.CashSessions.FirstOrDefaultAsync(x=>x.Status=="Open");if(session!=null)db.CashMovements.Add(new CashMovement{StoreId=StoreId(p),CashSessionId=session.Id,UserId=p.UserId(),Direction="Out",Type="SupplierPayment",Amount=r.Amount,Method=r.Method,Reference=r.Reference});await db.SaveChangesAsync();return Results.Ok(ap);
        });

        api.MapGet("/sales", async (ClaimsPrincipal p, AppDbContext db) => p.HasPermission(Permissions.SalesRead)?Results.Ok(await db.Sales.OrderByDescending(x=>x.CreatedAt).Take(100).ToListAsync()):Results.Forbid());
        api.MapPost("/sales", async (SaleRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.SalesWrite))return Results.Forbid();if(r.Lines.Count==0)return Results.BadRequest(new{message="Sale requires lines"});var store=StoreId(p);await using var tx=await db.Database.BeginTransactionAsync();var subtotal=r.Lines.Sum(x=>x.Quantity*x.UnitPrice);var sale=new Sale{StoreId=store,Number=NextNumber("VEN",await db.Sales.CountAsync()),CustomerId=r.CustomerId,UserId=p.UserId(),PriceListId=r.PriceListId,Subtotal=subtotal,Discount=r.Discount,Total=Math.Max(0,subtotal-r.Discount),PaymentMethod=r.PaymentMethod,IsFinanced=r.IsFinanced,FinancingPlatformId=r.FinancingPlatformId,FinancingReference=r.FinancingReference};db.Sales.Add(sale);foreach(var line in r.Lines){var product=await db.Products.SingleOrDefaultAsync(x=>x.Id==line.ProductId);if(product is null||product.Stock<line.Quantity)return Results.BadRequest(new{message=$"Insufficient stock: {line.ProductId}"});product.Stock-=line.Quantity;db.SaleLines.Add(new SaleLine{StoreId=store,SaleId=sale.Id,ProductId=line.ProductId,PhoneUnitId=line.PhoneUnitId,Quantity=line.Quantity,UnitPrice=line.UnitPrice,LineTotal=line.Quantity*line.UnitPrice});db.InventoryMovements.Add(new InventoryMovement{StoreId=store,ProductId=line.ProductId,Type="Sale",Quantity=-line.Quantity,Reason="POS sale",Reference=sale.Number,UserId=p.UserId()});if(line.PhoneUnitId.HasValue){var phone=await db.PhoneUnits.SingleOrDefaultAsync(x=>x.Id==line.PhoneUnitId.Value);if(phone is null||phone.InventoryStatus!="Available")return Results.BadRequest(new{message="Phone unit is not available"});phone.InventoryStatus="Sold";phone.SoldToCustomerId=r.CustomerId;phone.SoldByUserId=p.UserId();phone.SaleId=sale.Id;phone.SoldAt=DateTime.UtcNow;phone.SoldPrice=line.UnitPrice;}}if(r.CommissionAmount.GetValueOrDefault()>0)db.Commissions.Add(new Commission{StoreId=store,UserId=p.UserId(),SaleId=sale.Id,Amount=r.CommissionAmount!.Value,Rule="Sale"});if(r.IsFinanced){if(!r.FinancingPlatformId.HasValue||string.IsNullOrWhiteSpace(r.FinancingReference))return Results.BadRequest(new{message="Financing platform and reference required"});db.FinancedSales.Add(new FinancedSale{StoreId=store,SaleId=sale.Id,PlatformId=r.FinancingPlatformId.Value,Reference=r.FinancingReference,ExpectedAmount=r.PlatformExpectedAmount??sale.Total});}else{var session=await db.CashSessions.FirstOrDefaultAsync(x=>x.Status=="Open");if(session!=null)db.CashMovements.Add(new CashMovement{StoreId=store,CashSessionId=session.Id,UserId=p.UserId(),Direction="In",Type="Sale",Amount=sale.Total,Method=r.PaymentMethod,Reference=sale.Number});}await db.SaveChangesAsync();await tx.CommitAsync();await AddNotification(db,store,p.UserId(),r.IsFinanced?"financed_sale":"sale","Venta registrada",$"{sale.Number} · {sale.Total:N0}");return Results.Ok(sale);
        });

        api.MapGet("/financed-sales", async (AppDbContext db) => Results.Ok(await db.FinancedSales.OrderByDescending(x=>x.CreatedAt).ToListAsync()));
        api.MapPost("/financed-sales/{id:guid}/payment", async (Guid id, FinancingPaymentRequest r, ClaimsPrincipal p, AppDbContext db) =>
        {
            if(!p.HasPermission(Permissions.SalesWrite))return Results.Forbid();var fs=await db.FinancedSales.SingleOrDefaultAsync(x=>x.Id==id);if(fs is null)return Results.NotFound();fs.PaidAmount=Math.Min(fs.ExpectedAmount,fs.PaidAmount+r.Amount);fs.Status=fs.PaidAmount>=fs.ExpectedAmount?"Paid":"Partial";await db.SaveChangesAsync();return Results.Ok(fs);
        });
        api.MapGet("/commissions", async (AppDbContext db) => Results.Ok(await db.Commissions.OrderByDescending(x=>x.CreatedAt).ToListAsync()));
        api.MapGet("/inventory-movements", async (AppDbContext db) => Results.Ok(await db.InventoryMovements.OrderByDescending(x=>x.CreatedAt).Take(200).ToListAsync()));
    }

    private static void MapRepairsAndCash(WebApplication app)
    {
        var api=app.MapGroup("/api").RequireAuthorization();
        api.MapGet("/technicians", async (AppDbContext db)=>Results.Ok(await db.Technicians.OrderBy(x=>x.Name).ToListAsync()));
        api.MapPost("/technicians", async (TechnicianRequest r, ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.RepairsWrite))return Results.Forbid();var x=new Technician{StoreId=StoreId(p),Name=r.Name,Phone=r.Phone,CommissionPercent=r.CommissionPercent,Active=r.Active};db.Technicians.Add(x);await db.SaveChangesAsync();return Results.Ok(x);});
        api.MapGet("/technician-loans", async (AppDbContext db)=>Results.Ok(await db.TechnicianLoans.OrderByDescending(x=>x.CreatedAt).ToListAsync()));
        api.MapPost("/technician-loans", async (TechnicianLoanRequest r, ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.RepairsWrite))return Results.Forbid();var x=new TechnicianLoan{StoreId=StoreId(p),TechnicianId=r.TechnicianId,Amount=r.Amount,Balance=r.Amount,Reason=r.Reason,Installments=r.Installments};db.TechnicianLoans.Add(x);await db.SaveChangesAsync();return Results.Ok(x);});

        api.MapGet("/repairs", async (ClaimsPrincipal p, AppDbContext db)=>p.HasPermission(Permissions.RepairsRead)?Results.Ok(await db.RepairOrders.OrderByDescending(x=>x.ReceivedAt).ToListAsync()):Results.Forbid());
        api.MapPost("/repairs", async (RepairOrderRequest r, ClaimsPrincipal p, AppDbContext db)=>
        {
            if(!p.HasPermission(Permissions.RepairsWrite))return Results.Forbid();var store=StoreId(p);var order=new RepairOrder{StoreId=store,Number=NextNumber("REP",await db.RepairOrders.CountAsync()),SecurityCode=Random.Shared.Next(1000,9999).ToString(),CustomerId=r.CustomerId,CustomerPhone=r.CustomerPhone,Device=r.Device,Brand=r.Brand,Model=r.Model,ImeiOrSerial=r.ImeiOrSerial,PhysicalState=r.PhysicalState,AccessoriesReceived=r.AccessoriesReceived,ReportedDamage=r.ReportedDamage,Diagnosis=r.Diagnosis,TechnicianId=r.TechnicianId,LaborCost=r.LaborCost,Price=r.Price,Advance=r.Advance,Balance=Math.Max(0,r.Price-r.Advance),EstimatedAt=r.EstimatedAt,WarrantyDays=r.WarrantyDays,Notes=r.Notes,PublicNotes=r.PublicNotes,Status=r.Status};db.RepairOrders.Add(order);await db.SaveChangesAsync();await AddNotification(db,store,p.UserId(),"repair_new","Nueva reparación",$"{order.Number} · {order.Device}");return Results.Ok(order);
        });
        api.MapPut("/repairs/{id:guid}/status", async (Guid id, RepairStatusRequest r, ClaimsPrincipal p, AppDbContext db)=>
        {
            if(!p.HasPermission(Permissions.RepairsWrite))return Results.Forbid();var order=await db.RepairOrders.SingleOrDefaultAsync(x=>x.Id==id);if(order is null)return Results.NotFound();order.Status=r.Status;order.Diagnosis=r.Diagnosis??order.Diagnosis;order.PublicNotes=r.PublicNotes??order.PublicNotes;order.UpdatedAt=DateTime.UtcNow;if(r.Status=="Delivered")order.DeliveredAt=DateTime.UtcNow;await db.SaveChangesAsync();if(r.Status=="Ready")await AddNotification(db,StoreId(p),p.UserId(),"repair_ready","Reparación lista",$"{order.Number} está lista para entregar");return Results.Ok(order);
        });
        api.MapPost("/repairs/{id:guid}/parts", async (Guid id, RepairPartRequest r, ClaimsPrincipal p, AppDbContext db)=>
        {
            if(!p.HasPermission(Permissions.RepairsWrite))return Results.Forbid();var order=await db.RepairOrders.SingleOrDefaultAsync(x=>x.Id==id);var product=await db.Products.SingleOrDefaultAsync(x=>x.Id==r.ProductId);if(order is null||product is null)return Results.NotFound();if(product.Stock<r.Quantity)return Results.BadRequest(new{message="Insufficient part stock"});product.Stock-=r.Quantity;db.RepairPartUsages.Add(new RepairPartUsage{StoreId=StoreId(p),RepairOrderId=id,ProductId=r.ProductId,Quantity=r.Quantity,UnitCost=product.Cost});db.InventoryMovements.Add(new InventoryMovement{StoreId=StoreId(p),ProductId=r.ProductId,Type="RepairUse",Quantity=-r.Quantity,Reason="Used in repair",Reference=order.Number,UserId=p.UserId()});await db.SaveChangesAsync();return Results.Ok(order);
        });

        api.MapGet("/cash/current", async (ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.CashRead))return Results.Forbid();var s=await db.CashSessions.FirstOrDefaultAsync(x=>x.Status=="Open");if(s is null)return Results.Ok(new{open=false});var moves=await db.CashMovements.Where(x=>x.CashSessionId==s.Id).OrderByDescending(x=>x.CreatedAt).ToListAsync();var expected=s.OpeningBase+moves.Sum(x=>x.Direction=="In"?x.Amount:-x.Amount);return Results.Ok(new{open=true,session=s,movements=moves,expected});});
        api.MapPost("/cash/open", async (CashOpenRequest r, ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.CashWrite))return Results.Forbid();if(await db.CashSessions.AnyAsync(x=>x.Status=="Open"))return Results.Conflict(new{message="A cash session is already open"});var s=new CashSession{StoreId=StoreId(p),UserId=p.UserId(),OpeningBase=r.OpeningBase};db.CashSessions.Add(s);await db.SaveChangesAsync();return Results.Ok(s);});
        api.MapPost("/cash/movements", async (CashMovementRequest r, ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.CashWrite))return Results.Forbid();var s=await db.CashSessions.FirstOrDefaultAsync(x=>x.Status=="Open");if(s is null)return Results.BadRequest(new{message="Open cash first"});var m=new CashMovement{StoreId=StoreId(p),CashSessionId=s.Id,UserId=p.UserId(),Direction=r.Direction,Type=r.Type,Amount=r.Amount,Method=r.Method,Reference=r.Reference,Notes=r.Notes};db.CashMovements.Add(m);await db.SaveChangesAsync();return Results.Ok(m);});
        api.MapPost("/cash/close", async (CashCloseRequest r, ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.CashWrite))return Results.Forbid();var s=await db.CashSessions.FirstOrDefaultAsync(x=>x.Status=="Open");if(s is null)return Results.BadRequest(new{message="No open cash"});var moves=await db.CashMovements.Where(x=>x.CashSessionId==s.Id).ToListAsync();var expected=s.OpeningBase+moves.Sum(x=>x.Direction=="In"?x.Amount:-x.Amount);s.ExpectedTotal=expected;s.CountedTotal=r.CountedTotal;s.Difference=r.CountedTotal-expected;s.Notes=r.Notes;s.ClosedAt=DateTime.UtcNow;s.Status="Closed";await db.SaveChangesAsync();await AddNotification(db,StoreId(p),p.UserId(),Math.Abs(s.Difference.Value)>0.01m?"cash_difference":"cash_close","Caja cerrada",$"Esperado {expected:N0} · Contado {r.CountedTotal:N0} · Diferencia {s.Difference:N0}");return Results.Ok(s);});
        api.MapGet("/cash/history", async (AppDbContext db)=>Results.Ok(await db.CashSessions.OrderByDescending(x=>x.OpenedAt).Take(90).ToListAsync()));

        api.MapGet("/warranties", async (AppDbContext db)=>Results.Ok(await db.Warranties.OrderByDescending(x=>x.StartsAt).ToListAsync()));
        api.MapPost("/warranties", async (WarrantyRequest r, ClaimsPrincipal p, AppDbContext db)=>{var x=new Warranty{StoreId=StoreId(p),Type=r.Type,SaleId=r.SaleId,RepairOrderId=r.RepairOrderId,ProductId=r.ProductId,CustomerId=r.CustomerId,ImeiOrSerial=r.ImeiOrSerial,StartsAt=r.StartsAt,ExpiresAt=r.ExpiresAt,Notes=r.Notes};db.Warranties.Add(x);await db.SaveChangesAsync();return Results.Ok(x);});
    }

    private static void MapDashboardAndSupport(WebApplication app)
    {
        var api=app.MapGroup("/api").RequireAuthorization();
        api.MapGet("/dashboard", async (ClaimsPrincipal p, AppDbContext db)=>
        {
            if(p.IsSuperAdmin())return Results.Ok(new{mode="superadmin",stores=await db.Stores.CountAsync(),activeStores=await db.Stores.CountAsync(x=>x.IsActive),users=await db.Users.IgnoreQueryFilters().CountAsync()});
            var today=DateTime.UtcNow.Date;var month=new DateTime(today.Year,today.Month,1,0,0,0,DateTimeKind.Utc);var salesToday=await db.Sales.Where(x=>x.CreatedAt>=today).SumAsync(x=>(decimal?)x.Total)??0;var salesMonth=await db.Sales.Where(x=>x.CreatedAt>=month).SumAsync(x=>(decimal?)x.Total)??0;var lowStock=await db.Products.CountAsync(x=>x.Active&&x.Stock<=x.MinStock);var pendingRepairs=await db.RepairOrders.CountAsync(x=>x.Status!="Delivered"&&x.Status!="Cancelled");var readyRepairs=await db.RepairOrders.CountAsync(x=>x.Status=="Ready");var overdueRepairs=await db.RepairOrders.CountAsync(x=>x.EstimatedAt<DateTime.UtcNow&&x.Status!="Delivered"&&x.Status!="Ready"&&x.Status!="Cancelled");var payable=await db.AccountsPayable.Where(x=>x.Balance>0).SumAsync(x=>(decimal?)x.Balance)??0;var financed=await db.FinancedSales.Where(x=>x.Status!="Paid"&&x.Status!="Cancelled").SumAsync(x=>(decimal?)(x.ExpectedAmount-x.PaidAmount))??0;var commissions=await db.Commissions.Where(x=>x.Status=="Pending").SumAsync(x=>(decimal?)x.Amount)??0;var cash=await db.CashSessions.FirstOrDefaultAsync(x=>x.Status=="Open");decimal cashExpected=0;if(cash!=null){var moves=await db.CashMovements.Where(x=>x.CashSessionId==cash.Id).ToListAsync();cashExpected=cash.OpeningBase+moves.Sum(x=>x.Direction=="In"?x.Amount:-x.Amount);}var latestSales=await db.Sales.OrderByDescending(x=>x.CreatedAt).Take(5).ToListAsync();var latestRepairs=await db.RepairOrders.OrderByDescending(x=>x.CreatedAt).Take(5).ToListAsync();return Results.Ok(new{mode="store",salesToday,salesMonth,cashCurrent=cashExpected,lowStock,pendingRepairs,readyRepairs,overdueRepairs,accountsPayable=payable,financingPending=financed,commissionsPending=commissions,latestSales,latestRepairs});
        });
        api.MapGet("/dashboard/layout", async (ClaimsPrincipal p, AppDbContext db)=>{var uid=p.UserId();var x=await db.DashboardLayouts.FirstOrDefaultAsync(x=>x.UserId==uid);return Results.Ok(new{json=x?.Json??"[]"});});
        api.MapPut("/dashboard/layout", async (DashboardLayoutRequest r, ClaimsPrincipal p, AppDbContext db)=>{var uid=p.UserId();var x=await db.DashboardLayouts.FirstOrDefaultAsync(x=>x.UserId==uid);if(x is null){x=new DashboardLayout{StoreId=StoreId(p),UserId=uid,Json=r.Json};db.DashboardLayouts.Add(x);}else{x.Json=r.Json;x.UpdatedAt=DateTime.UtcNow;}await db.SaveChangesAsync();return Results.Ok(new{saved=true});});

        api.MapGet("/search", async (string q, ClaimsPrincipal p, AppDbContext db)=>
        {
            if(string.IsNullOrWhiteSpace(q))return Results.Ok(Array.Empty<object>());q=q.Trim();var results=new List<object>();results.AddRange((await db.Customers.Where(x=>EF.Functions.ILike(x.Name,$"%{q}%")||x.Phone==q).Take(5).ToListAsync()).Select(x=>(object)new{module="customers",type="CLIENT",id=x.Id,title=x.Name,subtitle=x.Phone}));results.AddRange((await db.Products.Where(x=>EF.Functions.ILike(x.Name,$"%{q}%")||x.Sku==q||x.Barcode==q).Take(5).ToListAsync()).Select(x=>(object)new{module="inventory",type="PRODUCT",id=x.Id,title=x.Name,subtitle=x.Sku}));results.AddRange((await db.PhoneUnits.Where(x=>x.Imei1==q||x.Imei2==q||x.SerialNumber==q).Take(5).ToListAsync()).Select(x=>(object)new{module="inventory",type="PHONE",id=x.Id,title=$"{x.Brand} {x.Model}",subtitle=$"IMEI {x.Imei1} · {x.InventoryStatus}"}));results.AddRange((await db.RepairOrders.Where(x=>x.Number==q||x.ImeiOrSerial==q||EF.Functions.ILike(x.Device,$"%{q}%")).Take(5).ToListAsync()).Select(x=>(object)new{module="repairs",type="REPAIR",id=x.Id,title=$"{x.Number} · {x.Device}",subtitle=x.Status}));results.AddRange((await db.Suppliers.Where(x=>EF.Functions.ILike(x.Name,$"%{q}%")||x.Nit==q).Take(5).ToListAsync()).Select(x=>(object)new{module="suppliers",type="SUPPLIER",id=x.Id,title=x.Name,subtitle=x.Nit}));results.AddRange((await db.Sales.Where(x=>x.Number==q).Take(5).ToListAsync()).Select(x=>(object)new{module="sales",type="SALE",id=x.Id,title=x.Number,subtitle=x.Total.ToString("N0")}));return Results.Ok(results.Take(20));
        });

        api.MapGet("/notifications", async (ClaimsPrincipal p, AppDbContext db)=>{var uid=p.UserId();return Results.Ok(await db.Notifications.Where(x=>x.UserId==null||x.UserId==uid).OrderByDescending(x=>x.CreatedAt).Take(50).ToListAsync());});
        api.MapPut("/notifications/{id:guid}/read", async (Guid id, AppDbContext db)=>{var n=await db.Notifications.SingleOrDefaultAsync(x=>x.Id==id);if(n is null)return Results.NotFound();n.IsRead=true;await db.SaveChangesAsync();return Results.Ok(n);});
        api.MapGet("/notification-preferences", async (ClaimsPrincipal p, AppDbContext db)=>{var uid=p.UserId();var x=await db.NotificationPreferences.FirstOrDefaultAsync(x=>x.UserId==uid);return Results.Ok(x);});
        api.MapPut("/notification-preferences", async (NotificationPreferenceRequest r, ClaimsPrincipal p, AppDbContext db)=>{if(!p.HasPermission(Permissions.SettingsWrite))return Results.Forbid();var uid=p.UserId();var x=await db.NotificationPreferences.FirstOrDefaultAsync(x=>x.UserId==uid);if(x is null){x=new NotificationPreference{StoreId=StoreId(p),UserId=uid};db.NotificationPreferences.Add(x);}x.TelegramConnected=r.TelegramConnected;x.TelegramChatId=r.TelegramChatId;x.EventsCsv=r.EventsCsv;x.DailySummary=r.DailySummary;x.DailySummaryTime=r.DailySummaryTime;await db.SaveChangesAsync();return Results.Ok(x);});
        api.MapPost("/notification-preferences/telegram/test", async (ClaimsPrincipal p, AppDbContext db)=>{var store=StoreId(p);await AddNotification(db,store,p.UserId(),"telegram_test","Telegram mock","Evento de prueba generado correctamente. La integración real puede conectarse sin cambiar el dominio de eventos.");return Results.Ok(new{deliveredTo="mock",eventGenerated=true,utc=DateTime.UtcNow});});

        app.MapGet("/api/public/repair/{number}/{code}", async (string number,string code,AppDbContext db)=>{var order=await db.RepairOrders.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Number==number&&x.SecurityCode==code);if(order is null)return Results.NotFound();return Results.Ok(new{order.Number,order.Device,order.Brand,order.Model,order.Status,order.ReceivedAt,order.EstimatedAt,order.PublicNotes,ready=order.Status=="Ready"||order.Status=="Delivered"});});
    }
}
