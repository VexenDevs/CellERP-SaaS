QUIERO QUE CONSTRUYAS ESTE PROYECTO COMPLETO. NO QUIERO SOLO UNA EXPLICACIÓN, UN MOCKUP NI UNA LISTA DE TAREAS. QUIERO QUE EMPIECES A DESARROLLAR UNA PRIMERA VERSIÓN FUNCIONAL, EJECUTABLE Y VISUALMENTE UTILIZABLE DEL SISTEMA.

IMPORTANTE:

\- Actúa como arquitecto de software, desarrollador backend senior, frontend senior, DBA, diseñador UX/UI y especialista en SaaS.

\- Toma decisiones técnicas razonables sin preguntarme cosas innecesarias.

\- Si algo todavía no está definido, implementa una solución preparada para ampliarse posteriormente.

\- NO construyas una aplicación genérica de administración.

\- El producto debe estar pensado específicamente para tiendas de celulares, accesorios y servicio técnico.

\- Quiero poder ejecutar el proyecto localmente y desplegarlo en Railway para hacer pruebas.

\- Quiero ver una interfaz funcional desde el primer avance.

\- Entrega código real y funcional, no pseudocódigo.

\- No dejes funciones principales como "TODO".

\- Si una integración externa todavía no puede implementarse completamente, crea una arquitectura preparada y un mock funcional para poder probarla.

\==================================================

1\. CONCEPTO DEL PRODUCTO

\==================================================

Estoy creando un ERP SaaS web para negocios de:

\- Venta de celulares nuevos.

\- Venta de celulares usados/de segunda.

\- Venta de accesorios.

\- Servicio técnico de celulares.

\- Reparación de computadores y otros dispositivos.

\- Venta de repuestos.

\- Control de técnicos.

\- Control de vendedores/asesores.

\- Control de proveedores.

\- Cuentas por pagar.

\- Control de caja.

\- Inventario.

\- Comisiones.

\- Ventas financiadas mediante plataformas externas.

\- Garantías.

\- Portal de consulta para clientes.

\- Notificaciones.

\- Soporte.

Debe ser una plataforma SaaS multiempresa/multitienda.

MI EMPRESA será el administrador global del SaaS.

Cada tienda cliente debe tener sus datos completamente aislados de las demás.

\==================================================

2\. ARQUITECTURA GENERAL

\==================================================

Construye una aplicación web moderna y responsive.

Tecnologías preferidas:

BACKEND:

- [ASP.NET](https://www.google.com/url?q=http://asp.net\&sa=D\&source=editors\&ust=1788472259772919\&usg=AOvVaw3auP-XBELI9ywmrBe0xrSH) Core / C#

\- API REST

\- Entity Framework Core

\- PostgreSQL

FRONTEND:

\- React

\- TypeScript

\- Vite

\- Una librería de componentes ligera y personalizable, evitando diseños excesivamente redondeados.

AUTENTICACIÓN:

\- JWT o una solución segura equivalente.

\- Roles y permisos.

\- Multi-tenant.

INFRAESTRUCTURA:

\- Railway para las pruebas iniciales.

\- PostgreSQL de Railway inicialmente.

\- Variables de entorno mediante .env.

\- Preparar Dockerfile/Docker Compose si es conveniente.

El proyecto debe quedar preparado para poder cambiar posteriormente PostgreSQL de pruebas por una base de datos de producción sin tener que reescribir el sistema.

\==================================================

3\. MULTI-TENANT / SAAS

\==================================================

La plataforma tendrá dos niveles principales:

A) SUPERADMIN DEL SOFTWARE

Soy el propietario de la plataforma.

Necesito un panel separado donde pueda:

\- Crear tiendas.

\- Editar tiendas.

\- Activar/desactivar tiendas.

\- Ver estado de licencia.

\- Definir fecha de vencimiento.

\- Definir plan.

\- Activar módulos individualmente.

\- Desactivar módulos.

\- Ver usuarios de cada tienda.

\- Ver información general de cada tienda.

\- Ver estadísticas generales del SaaS.

B) TIENDAS CLIENTES

Cada tienda tendrá su propio entorno.

Una tienda tendrá:

\- Nombre comercial.

\- Logo.

\- NIT.

\- Dirección.

\- Teléfono.

\- Email.

\- Ciudad.

\- Configuración regional.

\- Moneda.

\- Idioma.

\- Zona horaria.

\- Configuración de módulos.

\- Configuración de notificaciones.

NINGUNA tienda debe poder consultar información de otra tienda.

Diseña la base de datos desde el principio teniendo en cuenta TenantId/StoreId.

\==================================================

4\. USUARIO MAESTRO PARA PRUEBAS

\==================================================

Quiero que el proyecto venga preparado con un usuario administrador inicial para poder entrar inmediatamente.

Credenciales iniciales de desarrollo:

Usuario:

admin

Contraseña:

admin123

IMPORTANTE:

Estas credenciales son SOLO para desarrollo/pruebas.

Implementa un seed inicial que cree:

Tienda de prueba:

"Demo Store"

Usuario:

admin

Contraseña:

admin123

Rol:

SUPERADMIN o ADMINISTRADOR GLOBAL según la arquitectura elegida.

Además crea un usuario administrador de la tienda Demo Store para probar la separación entre SuperAdmin y usuarios de tienda.

NO almacenes contraseñas en texto plano.

\==================================================

5\. SISTEMA DE ROLES Y PERMISOS

\==================================================

No quiero depender únicamente de roles rígidos.

Implementa roles + permisos.

Ejemplos:

SUPERADMIN

\- Gestionar tiendas.

\- Gestionar licencias.

\- Gestionar módulos.

\- Gestionar planes.

\- Ver estadísticas globales.

OWNER / DUEÑO

\- Acceso completo a su tienda.

\- Inventario.

\- Ventas.

\- Caja.

\- Proveedores.

\- Servicio técnico.

\- Reportes.

\- Usuarios.

\- Configuración.

\- Notificaciones.

ADMINISTRADOR

\- Similar al dueño pero limitado según permisos.

VENDEDOR / ASESOR

\- Ventas.

\- Clientes.

\- Consulta de inventario.

\- Ventas financiadas.

\- Consulta de sus comisiones.

TÉCNICO

\- Reparaciones asignadas.

\- Cambios de estado.

\- Repuestos utilizados.

\- Historial técnico.

RECEPCIÓN

\- Crear órdenes de servicio.

\- Consultar clientes.

\- Entregar equipos.

\- Consultar estados.

Los permisos deben poder ampliarse posteriormente.

\==================================================

6\. DISEÑO VISUAL

\==================================================

ESTO ES MUY IMPORTANTE.

NO quiero el típico diseño generado por IA:

\- No botones gigantes.

\- No exceso de bordes redondeados.

\- No tarjetas enormes.

\- No sombras exageradas.

\- No sidebar gigante.

\- No aspecto de dashboard genérico de startup.

Quiero:

\- Minimalista.

\- Profesional.

\- Sobrio.

\- Moderno.

\- Rápido.

\- Limpio.

\- Inspirado en software empresarial moderno y aplicaciones de escritorio clásicas bien diseñadas.

QUIERO MENÚ SUPERIOR.

NO quiero un menú lateral permanente.

La navegación principal debe estar en una barra superior.

Ejemplo:

INICIO

TIENDA

SERVICIO TÉCNICO

INVENTARIO

CLIENTES

PROVEEDORES

CAJA

REPORTES

A la derecha:

BUSCADOR GLOBAL

NOTIFICACIONES

IDIOMA

TEMA

PERFIL

Los submenús pueden aparecer mediante dropdowns.

Debe funcionar perfectamente en:

\- PC.

\- Laptop.

\- Tablet.

\- Android.

\- iPhone.

En pantallas pequeñas, el menú superior debe adaptarse sin romper la interfaz.

\==================================================

7\. TEMAS

\==================================================

Implementar:

\- Tema claro.

\- Tema oscuro.

La preferencia debe guardarse por usuario.

Preparar arquitectura para futuros temas.

El color principal/acento puede ser configurable posteriormente por tienda.

\==================================================

8\. IDIOMAS

\==================================================

Implementar internacionalización desde el comienzo.

Idiomas iniciales:

\- Español.

\- Inglés.

\- Portugués.

No escribas textos directamente por todo el código.

Utiliza archivos de traducción.

Preparar arquitectura para agregar idiomas posteriormente.

\==================================================

9\. DASHBOARD

\==================================================

El inicio debe tener un dashboard personalizable.

QUIERO WIDGETS.

El usuario debe poder:

\- Mostrar/ocultar widgets.

\- Mover widgets.

\- Cambiar tamaño.

\- Tener widgets pequeños.

\- Tener widgets medianos.

\- Tener widgets grandes.

\- Widgets de una columna.

\- Widgets de dos columnas.

\- Widgets adaptables/responsive.

Guardar la configuración del dashboard por usuario.

Ejemplos de widgets:

\- Ventas del día.

\- Ventas del mes.

\- Caja actual.

\- Productos con stock bajo.

\- Reparaciones pendientes.

\- Reparaciones listas.

\- Reparaciones atrasadas.

\- Cuentas por cobrar.

\- Cuentas por pagar.

\- Proveedores pendientes.

\- Comisiones pendientes.

\- Ventas financiadas pendientes de pago.

\- Últimas ventas.

\- Últimas reparaciones.

\==================================================

10\. BUSCADOR GLOBAL

\==================================================

Quiero un buscador pequeño y discreto en la barra superior.

Debe poder buscar transversalmente.

Ejemplos:

\- Cliente por nombre.

\- Cliente por teléfono.

\- IMEI.

\- Producto.

\- Código de producto.

\- Número de factura.

\- Orden de reparación.

\- Proveedor.

\- Venta.

\- Número de serie.

Los resultados deben indicar a qué módulo pertenecen.

Ejemplo:

"356xxxxxxxxxxxxx"

Resultado:

TELÉFONO

iPhone 15 Pro 256GB

IMEI: ...

Estado: Vendido

Y permitir abrir directamente la ficha.

\==================================================

11\. MÓDULO TIENDA

\==================================================

Separar conceptualmente el módulo TIENDA del módulo SERVICIO TÉCNICO.

El módulo tienda manejará:

\- Productos.

\- Celulares.

\- Accesorios.

\- Ventas.

\- Facturación.

\- Inventario.

\- Clientes.

\- Proveedores.

\- Caja.

\- Comisiones.

\- Ventas financiadas.

\==================================================

12\. PRODUCTOS

\==================================================

Debe existir un catálogo general.

Tipos:

\- Celular.

\- Accesorio.

\- Repuesto.

\- Otro.

\==================================================

13\. CELULARES

\==================================================

Los celulares deben tener un manejo especial.

Campos mínimos:

\- Marca.

\- Modelo.

\- Variante.

\- Capacidad.

\- RAM.

\- Color.

\- IMEI 1.

\- IMEI 2 cuando aplique.

\- Número de serie cuando aplique.

\- Estado: Nuevo / Usado.

\- Condición.

\- Costo de compra.

\- Precio.

\- Proveedor.

\- Fecha de compra.

\- Factura de compra.

\- Garantía.

\- Observaciones.

\- Estado del inventario.

Cada teléfono debe poder rastrearse por IMEI.

QUIERO PODER BUSCAR UN IMEI Y OBTENER:

\- Proveedor.

\- Factura de compra.

\- Fecha de ingreso.

\- Costo.

\- Estado.

\- Cliente al que se vendió.

\- Fecha de venta.

\- Valor de venta.

\- Asesor que realizó la venta.

\- Plataforma de financiación si aplica.

\- Historial.

Esto debe funcionar aunque el teléfono haya sido vendido hace mucho tiempo.

\==================================================

14\. ACCESORIOS

\==================================================

Los accesorios tendrán:

\- Nombre.

\- SKU.

\- Código de barras.

\- Categoría.

\- Marca.

\- Descripción.

\- Costo.

\- Precio según lista.

\- Stock.

\- Stock mínimo.

\- Proveedor.

\- Ubicación.

\- Garantía.

\==================================================

15\. LISTAS DE PRECIOS

\==================================================

NO quiero tener solamente:

\- Precio mayorista.

\- Precio minorista.

QUIERO LISTAS DE PRECIOS TOTALMENTE PERSONALIZABLES.

El administrador puede crear:

\- Cliente final.

\- Mayorista.

\- Revendedor.

\- Aliado.

\- Rebuscador.

\- Tienda.

\- Etc.

Puede crear 3, 4, 5 o más listas.

Cada lista debe tener:

\- Nombre.

\- Descripción.

\- Estado.

\- Productos/precios.

El nombre debe ser editable.

Un cliente puede tener asignada una lista de precios.

Durante una venta se debe poder seleccionar la lista de precios.

NO se debe calcular automáticamente una comisión o margen para el revendedor.

El negocio simplemente le vende al revendedor al precio establecido y el revendedor decide a cuánto venderlo.

\==================================================

16\. INGRESO DE INVENTARIO

\==================================================

Necesito dos formas.

A) INGRESO PRODUCTO POR PRODUCTO

Ejemplo:

Compré un solo iPhone.

Registrar:

\- Producto.

\- IMEI.

\- Proveedor.

\- Costo.

\- Fecha.

\- Estado de pago.

B) INGRESO MEDIANTE FACTURA DE PROVEEDOR

Ejemplo:

Compré 30 accesorios a un proveedor.

Crear:

FACTURA DE COMPRA

\- Número de factura.

\- Proveedor.

\- Fecha.

\- Fecha de vencimiento.

\- Total.

\- Forma de pago.

\- Contado / Crédito.

\- Estado.

\- Productos.

Y dentro:

Producto 1 x 10

Producto 2 x 5

Producto 3 x 15

El inventario debe aumentar automáticamente.

\==================================================

17\. CUENTAS POR PAGAR / PROVEEDORES

\==================================================

Si una compra es a crédito:

Debe crearse automáticamente una cuenta por pagar.

Quiero poder ver:

\- Proveedor.

\- Facturas pendientes.

\- Total adeudado.

\- Abonos.

\- Saldo.

\- Vencimientos.

\- Historial de pagos.

No quiero registrar manualmente la deuda dos veces.

La cuenta por pagar debe estar vinculada con la factura original.

También debe existir historial de movimientos.

\==================================================

18\. MOVIMIENTOS DE INVENTARIO

\==================================================

Registrar:

\- Compra.

\- Venta.

\- Devolución.

\- Traslado.

\- Ajuste.

\- Pérdida.

\- Daño.

\- Uso en reparación.

\- Salida manual autorizada.

Cada movimiento debe tener:

\- Usuario.

\- Fecha/hora.

\- Producto.

\- Cantidad.

\- Motivo.

\- Referencia.

\==================================================

19\. VENTAS

\==================================================

Crear módulo POS.

Debe permitir:

\- Buscar producto.

\- Escanear código de barras.

\- Buscar IMEI.

\- Agregar productos.

\- Seleccionar cliente.

\- Seleccionar lista de precios.

\- Descuentos.

\- Métodos de pago.

\- Venta contado.

\- Venta mediante plataforma de financiación.

Para celulares:

Mostrar claramente:

\- IMEI.

\- Marca.

\- Modelo.

\- Estado nuevo/usado.

\- Precio.

\==================================================

20\. VENTAS DE CELULARES FINANCIADAS

\==================================================

IMPORTANTE:

NO gestionamos directamente las cuotas del cliente.

El crédito lo maneja una plataforma financiera externa.

El sistema solo debe controlar nuestra operación con esa plataforma.

Registrar:

\- Cliente.

\- Equipo.

\- IMEI.

\- Valor de venta.

\- Asesor.

\- Plataforma.

\- Fecha.

\- Número de operación/referencia.

\- Comisión del asesor.

\- Valor que debe pagar la plataforma.

\- Estado:

  - Pendiente.

  - Pagado.

  - Parcial.

  - En revisión.

  - Cancelado.

Debe existir control de cuánto nos debe cada plataforma.

Ejemplo:

PLATAFORMA A

5 ventas pendientes

Total pendiente: $8.500.000

PLATAFORMA B

2 ventas pendientes

Total pendiente: $3.200.000

\==================================================

21\. COMISIONES DE ASESORES

\==================================================

Registrar automáticamente las comisiones según las reglas configuradas.

Debe poder configurarse:

\- Comisión por venta.

\- Comisión por tipo de producto.

\- Comisión por operación financiada.

\- Comisión fija o porcentaje.

Pero el sistema debe ser flexible.

Mostrar:

\- Ventas realizadas por asesor.

\- Comisiones generadas.

\- Comisiones pagadas.

\- Comisiones pendientes.

\- Historial.

\==================================================

22\. CAJA

\==================================================

Crear un módulo de caja completo.

Apertura:

\- Usuario.

\- Fecha.

\- Hora.

\- Base inicial.

Movimientos:

ENTRADAS:

\- Venta.

\- Abono.

\- Otros ingresos.

SALIDAS:

\- Pago proveedor.

\- Retiro.

\- Gasto.

\- Otros.

Métodos:

\- Efectivo.

\- Transferencia.

\- Tarjeta.

\- Nequi.

\- Daviplata.

\- Otros configurables.

Cierre:

\- Total esperado.

\- Total contado.

\- Diferencia.

\- Observaciones.

\- Usuario responsable.

\- Fecha/hora.

Permitir:

\- Cuadre diario.

\- Historial de cierres.

\- Reportes semanales.

\- Reportes mensuales.

\==================================================

23\. SERVICIO TÉCNICO

\==================================================

Este debe ser un módulo separado visual y funcionalmente del módulo tienda.

Debe manejar:

\- Clientes.

\- Dispositivos.

\- Órdenes de reparación.

\- Técnicos.

\- Repuestos.

\- Estados.

\- Diagnóstico.

\- Presupuestos.

\- Garantías.

\- Entrega.

Una tienda puede contratar solamente el módulo Servicio Técnico.

Otra puede tener:

Servicio Técnico + Venta de equipos.

Otra:

Tienda completa + Servicio Técnico.

\==================================================

24\. ORDEN DE REPARACIÓN

\==================================================

Crear una orden con:

\- Número de orden automático.

\- Cliente.

\- Teléfono del cliente.

\- Dispositivo.

\- Marca.

\- Modelo.

\- IMEI/serial.

\- Estado físico.

\- Accesorios entregados.

\- Daño reportado.

\- Diagnóstico.

\- Técnico.

\- Repuestos.

\- Mano de obra.

\- Precio.

\- Anticipo.

\- Saldo.

\- Fecha de ingreso.

\- Fecha estimada.

\- Fecha de entrega.

\- Garantía.

\- Observaciones.

\- Fotografías si se desea.

Estados configurables, inicialmente:

\- Recibido.

\- En diagnóstico.

\- Esperando aprobación.

\- Esperando repuesto.

\- En reparación.

\- Reparado.

\- Listo para entregar.

\- Entregado.

\- No reparado.

\- Cancelado.

\==================================================

25\. TÉCNICOS

\==================================================

Crear perfil de técnico.

Mostrar:

\- Reparaciones asignadas.

\- Reparaciones completadas.

\- Reparaciones pendientes.

\- Ingresos generados.

\- Comisiones.

\- Préstamos.

\- Anticipos.

\- Historial.

\==================================================

26\. PRÉSTAMOS A TÉCNICOS

\==================================================

Registrar:

\- Técnico.

\- Valor.

\- Fecha.

\- Motivo.

\- Cuotas si aplica.

\- Abonos.

\- Saldo.

\- Estado.

Debe quedar separado del módulo de reparaciones pero relacionado con el técnico.

\==================================================

27\. REPUESTOS

\==================================================

Controlar:

\- Inventario.

\- Proveedor.

\- Costo.

\- Stock.

\- Stock mínimo.

\- Uso en reparación.

\- Salida.

\- Garantía.

Cuando un repuesto se utiliza en una reparación:

Debe descontarse automáticamente del inventario.

Debe quedar relacionado con la orden.

\==================================================

28\. GARANTÍA

\==================================================

Crear control de garantías.

Para ventas:

\- Producto.

\- Fecha de venta.

\- Cliente.

\- IMEI/serial.

\- Duración.

\- Fecha de vencimiento.

Para reparaciones:

\- Orden.

\- Trabajo realizado.

\- Repuestos.

\- Fecha de entrega.

\- Fecha de vencimiento.

\==================================================

29\. PORTAL DEL CLIENTE

\==================================================

Crear una vista pública/segura donde el cliente pueda consultar el estado de una reparación.

Puede acceder mediante:

\- Número de orden + código de seguridad.

Mostrar:

\- Número de orden.

\- Dispositivo.

\- Estado.

\- Fecha de ingreso.

\- Fecha estimada.

\- Progreso.

\- Observaciones públicas.

\- Si está listo para recoger.

NO mostrar:

\- Costos internos.

\- Margen.

\- Costos de repuestos.

\- Información de otros clientes.

\==================================================

30\. NOTIFICACIONES

\==================================================

Sistema interno de notificaciones.

Eventos configurables:

\- Nueva venta.

\- Venta de celular.

\- Venta financiada.

\- Plataforma pendiente de pago.

\- Caja cerrada.

\- Diferencia de caja.

\- Stock bajo.

\- Nueva reparación.

\- Reparación lista.

\- Reparación atrasada.

\- Cuenta por pagar próxima a vencer.

\- Cuenta por pagar vencida.

\- Comisión generada.

\- Etc.

\==================================================

31\. TELEGRAM

\==================================================

Quiero integración con un bot de Telegram.

El dueño/admin debe poder conectar su cuenta de Telegram desde la plataforma.

NO quiero tener que configurar manualmente código para cada tienda.

Crear una sección:

CONFIGURACIÓN

→ NOTIFICACIONES

→ TELEGRAM

Permitir:

\- Conectar Telegram.

\- Desconectar Telegram.

\- Probar conexión.

Y una lista de eventos con switches:

[ON] Ventas

[ON] Ventas de celulares

[OFF] Accesorios

[ON] Stock bajo

[ON] Reparaciones listas

[OFF] Reparaciones nuevas

[ON] Cierres de caja

[ON] Diferencias de caja

[ON] Plataformas pendientes

[OFF] Comisiones

etc.

El usuario decide qué quiere recibir.

También preparar un resumen diario configurable.

Ejemplo:

RESUMEN DEL DÍA

Ventas: $4.500.000

Reparaciones: 8

Reparaciones listas: 3

Caja: $2.100.000

Stock bajo: 5 productos

Plataformas pendientes: $3.500.000

Para pruebas iniciales, si no se puede conectar Telegram inmediatamente, crea una interfaz/mock de eventos que permita verificar que el sistema genera correcta