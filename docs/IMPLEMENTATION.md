# Matriz de implementación v0.1.0

## Funcional y visible en UI

- Login SuperAdmin / tienda.
- Panel SaaS de tiendas.
- Dashboard operativo y persistencia de widgets.
- Menú superior responsive, light/dark, ES/EN/PT.
- Buscador global.
- Productos.
- Clientes.
- Proveedores.
- Facturas de compra e inventario.
- Movimientos de inventario.
- POS / ventas.
- Ventas financiadas (registro desde POS).
- Servicio técnico y cambios de estado.
- Caja.
- Reportes operativos básicos.
- Notificaciones y configuración Telegram mock.
- Portal público de reparación.

## Funcional vía API y modelo de dominio

- Celulares individualizados por IMEI/serial e historial asociado.
- Listas de precios y precios por producto.
- Cuentas por pagar y abonos.
- Plataformas de financiación y abonos recibidos.
- Comisiones.
- Técnicos.
- Préstamos a técnicos.
- Repuestos consumidos por reparación.
- Garantías.
- Usuarios de tienda, roles y permisos ampliables.

## Preparado para siguiente iteración

- Transporte real Telegram (el dominio y preferencias ya existen; el mock valida eventos).
- Fotografías de órdenes (requiere storage S3/R2/compatible).
- Reglas avanzadas configurables de comisión.
- Reportería analítica avanzada/exportaciones.
- Facturación electrónica/fiscal por país.
- Integraciones directas con plataformas financieras.
- Migraciones EF versionadas para upgrades de bases existentes.
