# CellERP SaaS — v0.1.0

Primera versión funcional de un ERP SaaS multiempresa para tiendas de celulares, accesorios y servicio técnico.

## Qué incluye esta entrega

- **ASP.NET Core 8 / C# + REST API + Entity Framework Core + PostgreSQL**.
- **React + TypeScript + Vite** con UI responsive y navegación superior.
- Autenticación JWT con contraseñas **PBKDF2-SHA256** (no se guardan en texto plano).
- Aislamiento multi-tenant por `StoreId` mediante filtros globales de EF Core.
- SuperAdmin SaaS y usuarios por tienda con roles + permisos ampliables.
- Tema claro/oscuro e internacionalización inicial **ES / EN / PT**.
- Dashboard con widgets persistidos por usuario, reordenables y con tamaños 1x/2x.
- Buscador global de clientes, productos, IMEI/serial, reparaciones, proveedores y ventas.
- Productos, celulares/IMEI, clientes, proveedores y listas de precios personalizables.
- Ingreso de inventario mediante factura; aumenta stock automáticamente.
- Compras a crédito que generan automáticamente cuentas por pagar vinculadas.
- Historial de movimientos de inventario.
- POS de ventas con descuentos, métodos de pago y financiación externa.
- Seguimiento de ventas financiadas, pagos de plataformas y comisiones.
- Caja: apertura, movimientos, cierre, total esperado y diferencia.
- Servicio técnico: órdenes, estados configurables, técnicos y consumo de repuestos.
- Préstamos a técnicos y garantías en API.
- Portal público seguro de consulta de reparación por orden + código.
- Notificaciones internas y configuración de eventos.
- **Telegram mock funcional**: genera eventos reales dentro del sistema y deja preparada la persistencia para conectar el bot real.
- Docker Compose para pruebas locales, Dockerfile único para Railway y CI de GitHub Actions.

La especificación original completa está preservada en `docs/SPEC.md`.

## Credenciales de desarrollo

| Contexto | Usuario | Contraseña |
|---|---|---|
| SuperAdmin SaaS | `admin` | `admin123` |
| Administrador de Demo Store | `demo` | `demo123` |

Estas credenciales son únicamente para desarrollo. El seed crea los hashes PBKDF2 al inicializar una base de datos vacía.

## Ejecutar con Docker (recomendado)

1. Copia `.env.example` a `.env` y cambia `POSTGRES_PASSWORD` y `JWT_SECRET`.
2. Ejecuta:

```bash
docker compose up --build
```

3. Abre `http://localhost:8080`.
4. Swagger: `http://localhost:8080/swagger`.
5. Portal de cliente: `http://localhost:8080/repair-status`.
   - Demo: orden `REP-000001`, código `4812`.

## Ejecutar frontend y backend por separado

### PostgreSQL

Crea una base `cellerp` y define una cadena de conexión compatible con Npgsql en `ConnectionStrings__Default` o `DATABASE_URL`.

### Backend

Requiere .NET 8 SDK.

```bash
cd backend/CellErp.Api
dotnet restore
dotnet run
```

La API escucha en `http://localhost:8080` salvo que se defina `PORT`.

### Frontend

Requiere Node 20+.

```bash
cd frontend
npm install
npm run dev
```

Vite abre `http://localhost:5173` y redirige `/api` al backend local.

## Railway

1. Crea un proyecto nuevo en Railway.
2. Agrega PostgreSQL.
3. Sube este repositorio a GitHub y conéctalo a Railway.
4. Railway detectará el `Dockerfile` mediante `railway.toml`.
5. Define:
   - `DATABASE_URL`: puedes usar la variable del Postgres de Railway. El backend acepta tanto URL `postgresql://...` como cadena Npgsql.
   - `JWT_SECRET`: secreto aleatorio largo.
6. El health check es `/health`.

El contenedor compila el frontend y lo copia a `wwwroot`; en producción hay un solo servicio web.

## Multi-tenant

Las entidades operativas heredan de `TenantEntity` y contienen `StoreId`. `AppDbContext` aplica filtros globales basados en el claim `store_id` del JWT. Un usuario normal no recibe resultados de otra tienda aunque modifique IDs en el cliente. Las acciones de SuperAdmin usan rutas separadas bajo `/api/superadmin`.

## Flujo de inventario y contabilidad operativa

- **Compra:** crea factura + líneas + movimiento(s) de inventario y aumenta stock.
- **Compra a crédito:** además crea una cuenta por pagar vinculada a la factura.
- **Pago a proveedor:** reduce saldo y, si existe caja abierta, registra salida de caja.
- **Venta:** descuenta stock, crea movimientos y registra entrada de caja si no es financiada.
- **Venta financiada:** crea una cuenta operativa contra la plataforma externa; no administra cuotas del cliente.
- **Repuesto en reparación:** descuenta inventario y queda vinculado a la orden.

## Estructura

```text
.
├── backend/CellErp.Api/     ASP.NET Core + EF Core + PostgreSQL
├── frontend/                React + TypeScript + Vite
├── docs/SPEC.md             Requisitos originales
├── Dockerfile               Build único frontend + backend
├── docker-compose.yml       App + PostgreSQL local
├── railway.toml             Railway
└── .github/workflows/ci.yml GitHub Actions
```

## Alcance de v0.1.0

Esta entrega prioriza una **vertical completa y ejecutable** de los procesos centrales y deja modelados los módulos secundarios. No pretende ser todavía una versión fiscal/legal lista para facturación electrónica en producción ni incluye el transporte real de Telegram, almacenamiento de fotografías o integraciones financieras de terceros. Esas piezas están desacopladas del núcleo para poder incorporarlas sin rehacer multi-tenant, inventario, ventas o servicio técnico.

Antes de una producción comercial se recomienda reemplazar `EnsureCreated` por migraciones EF versionadas, configurar backups, observabilidad, rate limiting, recuperación de contraseña/2FA, auditoría inmutable y los requisitos tributarios del país objetivo.
