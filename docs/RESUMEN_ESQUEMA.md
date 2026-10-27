# Resumen del esquema

> Este resumen conserva observaciones de la revisión inicial. Para el estado actual de las migraciones y la tabla de autenticación, consulta [Base de datos y migraciones](BASE_DATOS.md). El script de creación actual incluye `FechaAplicacion` y `CostoUnitario` en Ajustes.

## Fuentes revisadas

- `database/01_Crear_Base_Datos.sql` define la base y el esquema siguiente.
- `database/02_Datos_Prueba.sql` contiene instrucciones `INSERT` para estas tablas.
- `docs/prototipos/0. Diseño Oficial.png` no existe con ese nombre exacto. Se encontró localmente `docs/prototipos/Diseno_Oficial.png`; su equivalencia con el archivo solicitado no está confirmada y la imagen se excluye del repositorio público.
- `docs/COMANDOS_RAPIDOS.md` existe.

Los scripts se leyeron, no se ejecutaron ni modificaron.

## Base de datos

`SupermercadosGuate`, creada tras `USE master`; el script cambia luego a ese contexto con `USE SupermercadosGuate`.

## Tablas, columnas y claves

Las PK indicadas son `INT IDENTITY(1,1)`. Las columnas se declaran `NOT NULL`, excepto las marcadas `NULL`. Tipos, longitudes y nulabilidad se transcriben del script.

1. **Tbl_Sucursales** — PK `CodigoSucursal`; `Nombre VARCHAR(100)`, `Direccion VARCHAR(200)`, `Municipio VARCHAR(100)`, `Departamento VARCHAR(100)`, `Telefono VARCHAR(20)`, `FechaApertura DATE`, `Estado VARCHAR(20)`. `Nombre` UNIQUE.
2. **Tbl_Categorias** — PK `CodigoCategoria`; `Nombre VARCHAR(100)`, `Descripcion VARCHAR(200) NULL`, `Estado VARCHAR(20)`. `Nombre` UNIQUE.
3. **Tbl_Proveedores** — PK `CodigoProveedor`; `NIT VARCHAR(20)`, `Nombre VARCHAR(150)`, `Contacto VARCHAR(150)`, `Municipio VARCHAR(100)`, `Departamento VARCHAR(100)`, `Telefono VARCHAR(20)`, `Correo VARCHAR(100) NULL`, `DiasCredito INT`, `FechaRegistro DATE`, `Estado VARCHAR(20)`. `NIT` UNIQUE.
4. **Tbl_Clientes** — PK `CodigoCliente`; `NIT VARCHAR(20)`, `Nombres VARCHAR(100)`, `Apellidos VARCHAR(100)`, `Municipio VARCHAR(100)`, `Departamento VARCHAR(100)`, `Telefono VARCHAR(20) NULL`, `Correo VARCHAR(100) NULL`, `FechaRegistro DATE`, `Estado VARCHAR(20)`. `NIT` UNIQUE.
5. **Tbl_Empleados** — PK `CodigoEmpleado`; `CodigoSucursal INT`, `DPI VARCHAR(13)`, `Nombres VARCHAR(100)`, `Apellidos VARCHAR(100)`, `Puesto VARCHAR(100)`, `FechaNacimiento DATE`, `FechaContratacion DATE`, `Telefono VARCHAR(20) NULL`, `Correo VARCHAR(100) NULL`, `Salario DECIMAL(12,2)`, `Estado VARCHAR(20)`. FK `CodigoSucursal` → `Tbl_Sucursales(CodigoSucursal)`; UNIQUE `(CodigoEmpleado, CodigoSucursal)` y `DPI`.
6. **Tbl_Productos** — PK `CodigoProducto`; `CodigoCategoria INT`, `CodigoBarra VARCHAR(30)`, `Nombre VARCHAR(150)`, `Marca VARCHAR(100)`, `Descripcion VARCHAR(200) NULL`, `UnidadMedida VARCHAR(30)`, `PrecioVenta DECIMAL(12,2)`, `FechaRegistro DATE`, `Estado VARCHAR(20)`. FK `CodigoCategoria` → `Tbl_Categorias(CodigoCategoria)`; `CodigoBarra` UNIQUE.
7. **Tbl_ProductoProveedor** — PK `CodigoProductoProveedor`; `CodigoProducto INT`, `CodigoProveedor INT`, `CodigoExterno VARCHAR(50)`, `CostoReferencia DECIMAL(12,2)`, `PlazoEntrega INT`, `FechaRegistro DATE`, `Estado VARCHAR(20)`. FKs a `Tbl_Productos(CodigoProducto)` y `Tbl_Proveedores(CodigoProveedor)`; UNIQUE `(CodigoProducto, CodigoProveedor)`.
8. **Tbl_Inventario** — PK `CodigoInventario`; `CodigoSucursal INT`, `CodigoProducto INT`, `Existencia DECIMAL(12,2)`, `ExistenciaMinima DECIMAL(12,2)`, `CostoPromedio DECIMAL(12,2)`, `FechaUltimoIngreso DATE NULL`, `FechaUltimoConteo DATE NULL`. FKs a sucursal y producto; UNIQUE `(CodigoSucursal, CodigoProducto)`.
9. **Tbl_Compras** — PK `CodigoCompra`; `CodigoSucursal INT`, `CodigoProveedor INT`, `CodigoEmpleado INT`, `NumeroCompra VARCHAR(30)`, `FechaCompra DATE`, `FechaEntregaProgramada DATE`, `FechaRecepcion DATE NULL`, `Estado VARCHAR(20)`, `Observaciones VARCHAR(300) NULL`. FKs a proveedor, sucursal y `(CodigoEmpleado, CodigoSucursal)` de empleados; `NumeroCompra` UNIQUE.
10. **Tbl_ComprasDetalle** — PK `CodigoDetalleCompra`; `CodigoCompra INT`, `CodigoProducto INT`, `Cantidad DECIMAL(12,2)`, `CostoUnitario DECIMAL(12,2)`, `Descuento DECIMAL(12,2) DEFAULT 0`, `Observaciones VARCHAR(300) NULL`. FKs a compra y producto; UNIQUE `(CodigoCompra, CodigoProducto)`.
11. **Tbl_Ventas** — PK `CodigoVenta`; `CodigoCliente INT`, `CodigoSucursal INT`, `CodigoEmpleado INT`, `NumeroVenta VARCHAR(30)`, `FechaVenta DATE`, `Estado VARCHAR(20)`, `Observaciones VARCHAR(300) NULL`. FKs a cliente, sucursal y `(CodigoEmpleado, CodigoSucursal)` de empleados; `NumeroVenta` UNIQUE.
12. **Tbl_VentasDetalle** — PK `CodigoDetalleVenta`; `CodigoVenta INT`, `CodigoProducto INT`, `Cantidad DECIMAL(12,2)`, `PrecioUnitario DECIMAL(12,2)`, `Descuento DECIMAL(12,2) DEFAULT 0`, `CostoUnitarioHistorico DECIMAL(12,2)`. FKs a venta y producto; UNIQUE `(CodigoVenta, CodigoProducto)`.
13. **Tbl_Pagos** — PK `CodigoPago`; `CodigoVenta INT`, `Comprobante VARCHAR(50)`, `FechaPago DATE`, `MetodoPago VARCHAR(50)`, `Monto DECIMAL(12,2)`, `Comision DECIMAL(12,2) DEFAULT 0`, `Autorizacion VARCHAR(50) NULL`, `Estado VARCHAR(20)`. FK a venta; `Comprobante` UNIQUE.
14. **Tbl_AjustesInventario** — PK `CodigoAjuste`; `CodigoSucursal INT`, `CodigoEmpleado INT`, `NumeroAjuste VARCHAR(30)`, `FechaAjuste DATE`, `FechaAplicacion DATE NULL`, `Motivo VARCHAR(200)`, `Estado VARCHAR(20)`, `Observaciones VARCHAR(300) NULL`. FKs a sucursal y `(CodigoEmpleado, CodigoSucursal)` de empleados; `NumeroAjuste` UNIQUE.
15. **Tbl_AjustesInventarioDetalle** — PK `CodigoDetalleAjuste`; `CodigoAjuste INT`, `CodigoProducto INT`, `Cantidad DECIMAL(12,2)`, `CostoUnitario DECIMAL(12,2) NULL`. FKs a ajuste y producto; UNIQUE `(CodigoAjuste, CodigoProducto)`.

## Relaciones y dependencias

- Sucursal 1:N empleados, inventario, compras, ventas y ajustes.
- Categoría 1:N productos. Proveedores N:M productos mediante `Tbl_ProductoProveedor`.
- Producto 1:N inventario, detalles de compra, detalles de venta y detalles de ajuste.
- Proveedor 1:N compras; cliente 1:N ventas.
- Empleado 1:N compras, ventas y ajustes. Las FK compuestas `(CodigoEmpleado, CodigoSucursal)` requieren que coincida la sucursal del empleado con la del documento.
- Compra 1:N detalles de compra. Venta 1:N detalles de venta y pagos. Ajuste 1:N detalles de ajuste.
- Orden reflejado por el script: sucursales, categorías, proveedores y clientes son raíces; luego empleados/productos; después asociaciones, encabezados transaccionales y detalles dependientes.

## Restricciones declaradas

- Estados: sucursal, categoría, proveedor, cliente, empleado, producto y producto-proveedor aceptan `Activo`/`Inactivo`; compras `Borrador`/`Recibida`/`Anulada`; ventas `Borrador`/`Confirmada`/`Anulada`; pagos `Aplicado`/`Anulado`; ajustes `Borrador`/`Aplicado`/`Anulado`.
- Checks de no negatividad: días de crédito, plazo, existencias, existencia mínima, costo promedio y comisión. Checks de positividad: salario, precio de venta, costo de referencia, cantidades/costos/precios de detalles de compra y venta y monto de pago. La cantidad de ajuste debe ser distinta de cero.
- Descuento de detalle debe estar entre cero y cantidad multiplicada por costo/precio unitario.
- Fecha programada y fecha de recepción (si existe) deben ser iguales o posteriores a fecha de compra.

## Errores y ambigüedades

- Los comentarios iniciales de ambos SQL dicen que falta incorporar esquema/datos, aunque el primero crea base y tablas y el segundo inserta datos.
- El nombre pedido para la imagen no coincide con el archivo encontrado (`Diseno_Oficial.png`). Confirmar equivalencia antes de tratarlo como oficial.
- `CREATE DATABASE SupermercadosGuate` no verifica existencia; una segunda ejecución falla si la base ya existe. El script de datos no es idempotente y presupone base vacía con identidades comenzando en 1 y el orden esperado; repetir los inserts puede colisionar con UNIQUE/FK.
- El esquema no muestra actualización automática de `Tbl_Inventario` por compras, ventas o ajustes; tampoco limita fecha de recepción a la programada ni relaciona pagos con el total de venta. Definir si son reglas de aplicación o requisitos de esquema.
- `CodigoCliente` en ventas es obligatorio (el ejemplo modela consumidor final como fila de cliente). `Descuento` representa importe total de línea según el CHECK. `CodigoExterno` no es UNIQUE y números de compra/venta y comprobantes son únicos globalmente. Confirmar estas decisiones.
- No se especifica `ON DELETE`/`ON UPDATE`; rige el comportamiento predeterminado de SQL Server. La lectura no valida sintaxis/ejecución contra una instancia.

No se ejecutó SQL. Se requiere confirmación explícita antes de ejecutar cualquiera de los scripts.
