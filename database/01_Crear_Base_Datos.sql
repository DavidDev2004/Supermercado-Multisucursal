/*
Proyecto: Supermercado Multisucursal - Base de Datos I
Motor: SQL Server

Pendiente: incorporar el esquema aprobado por el usuario.
No se definen bases de datos, tablas, columnas, relaciones ni restricciones
sin una especificación aprobada.
*/


-- 1. Crear base de datos

USE master;
GO



CREATE DATABASE SupermercadosGuate;
GO

USE SupermercadosGuate;
GO


-- 2. Crear tablas

-- SUCURSALES
CREATE TABLE Tbl_Sucursales
(
    CodigoSucursal INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Direccion VARCHAR(200) NOT NULL,
    Municipio VARCHAR(100) NOT NULL,
    Departamento VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    FechaApertura DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Sucursales_Nombre
    UNIQUE (Nombre),

    CONSTRAINT CK_Sucursales_Estado
    CHECK (Estado IN ('Activo', 'Inactivo'))
);
GO


-- CATEGORIAS
CREATE TABLE Tbl_Categorias
(
    CodigoCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(200) NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Categorias_Nombre
    UNIQUE (Nombre),

    CONSTRAINT CK_Categorias_Estado
    CHECK (Estado IN ('Activo', 'Inactivo'))
);
GO


-- PROVEEDORES
CREATE TABLE Tbl_Proveedores
(
    CodigoProveedor INT IDENTITY(1,1) PRIMARY KEY,
    NIT VARCHAR(20) NOT NULL,
    Nombre VARCHAR(150) NOT NULL,
    Contacto VARCHAR(150) NOT NULL,
    Municipio VARCHAR(100) NOT NULL,
    Departamento VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    Correo VARCHAR(100) NULL,
    DiasCredito INT NOT NULL,
    FechaRegistro DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Proveedores_NIT
    UNIQUE (NIT),

    CONSTRAINT CK_Proveedores_DiasCredito
    CHECK (DiasCredito >= 0),

    CONSTRAINT CK_Proveedores_Estado
    CHECK (Estado IN ('Activo', 'Inactivo'))
);
GO


-- CLIENTES
CREATE TABLE Tbl_Clientes
(
    CodigoCliente INT IDENTITY(1,1) PRIMARY KEY,
    NIT VARCHAR(20) NOT NULL,
    Nombres VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL,
    Municipio VARCHAR(100) NOT NULL,
    Departamento VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20) NULL,
    Correo VARCHAR(100) NULL,
    FechaRegistro DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Clientes_NIT
    UNIQUE (NIT),

    CONSTRAINT CK_Clientes_Estado
    CHECK (Estado IN ('Activo', 'Inactivo'))
);
GO


-- EMPLEADOS
CREATE TABLE Tbl_Empleados
(
    CodigoEmpleado INT IDENTITY(1,1) PRIMARY KEY,
    CodigoSucursal INT NOT NULL,
    DPI VARCHAR(13) NOT NULL,
    Nombres VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL,
    Puesto VARCHAR(100) NOT NULL,
    FechaNacimiento DATE NOT NULL,
    FechaContratacion DATE NOT NULL,
    Telefono VARCHAR(20) NULL,
    Correo VARCHAR(100) NULL,
    Salario DECIMAL(12,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Empleados_DPI
    UNIQUE (DPI),

    CONSTRAINT UQ_Empleados_Sucursal
    UNIQUE (CodigoEmpleado, CodigoSucursal),

    CONSTRAINT CK_Empleados_Salario
    CHECK (Salario > 0),

    CONSTRAINT CK_Empleados_Estado
    CHECK (Estado IN ('Activo', 'Inactivo')),

    CONSTRAINT FK_Empleados_Sucursales
    FOREIGN KEY (CodigoSucursal)
    REFERENCES Tbl_Sucursales(CodigoSucursal)
);
GO


-- PRODUCTOS
CREATE TABLE Tbl_Productos
(
    CodigoProducto INT IDENTITY(1,1) PRIMARY KEY,
    CodigoCategoria INT NOT NULL,
    CodigoBarra VARCHAR(30) NOT NULL,
    Nombre VARCHAR(150) NOT NULL,
    Marca VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(200) NULL,
    UnidadMedida VARCHAR(30) NOT NULL,
    PrecioVenta DECIMAL(12,2) NOT NULL,
    FechaRegistro DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Productos_CodigoBarra
    UNIQUE (CodigoBarra),

    CONSTRAINT CK_Productos_PrecioVenta
    CHECK (PrecioVenta > 0),

    CONSTRAINT CK_Productos_Estado
    CHECK (Estado IN ('Activo', 'Inactivo')),

    CONSTRAINT FK_Productos_Categorias
    FOREIGN KEY (CodigoCategoria)
    REFERENCES Tbl_Categorias(CodigoCategoria)
);
GO


-- PRODUCTO PROVEEDOR
CREATE TABLE Tbl_ProductoProveedor
(
    CodigoProductoProveedor INT IDENTITY(1,1) PRIMARY KEY,
    CodigoProducto INT NOT NULL,
    CodigoProveedor INT NOT NULL,
    CodigoExterno VARCHAR(50) NOT NULL,
    CostoReferencia DECIMAL(12,2) NOT NULL,
    PlazoEntrega INT NOT NULL,
    FechaRegistro DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_ProductoProveedor
    UNIQUE (CodigoProducto, CodigoProveedor),

    CONSTRAINT CK_ProductoProveedor_Costo
    CHECK (CostoReferencia > 0),

    CONSTRAINT CK_ProductoProveedor_Plazo
    CHECK (PlazoEntrega >= 0),

    CONSTRAINT CK_ProductoProveedor_Estado
    CHECK (Estado IN ('Activo', 'Inactivo')),

    CONSTRAINT FK_ProductoProveedor_Productos
    FOREIGN KEY (CodigoProducto)
    REFERENCES Tbl_Productos(CodigoProducto),

    CONSTRAINT FK_ProductoProveedor_Proveedores
    FOREIGN KEY (CodigoProveedor)
    REFERENCES Tbl_Proveedores(CodigoProveedor)
);
GO


-- INVENTARIO
CREATE TABLE Tbl_Inventario
(
    CodigoInventario INT IDENTITY(1,1) PRIMARY KEY,
    CodigoSucursal INT NOT NULL,
    CodigoProducto INT NOT NULL,
    Existencia DECIMAL(12,2) NOT NULL,
    ExistenciaMinima DECIMAL(12,2) NOT NULL,
    CostoPromedio DECIMAL(12,2) NOT NULL,
    FechaUltimoIngreso DATE NULL,
    FechaUltimoConteo DATE NULL,

    CONSTRAINT UQ_Inventario_SucursalProducto
    UNIQUE (CodigoSucursal, CodigoProducto),

    CONSTRAINT CK_Inventario_Existencia
    CHECK (Existencia >= 0),

    CONSTRAINT CK_Inventario_ExistenciaMinima
    CHECK (ExistenciaMinima >= 0),

    CONSTRAINT CK_Inventario_CostoPromedio
    CHECK (CostoPromedio >= 0),

    CONSTRAINT FK_Inventario_Sucursales
    FOREIGN KEY (CodigoSucursal)
    REFERENCES Tbl_Sucursales(CodigoSucursal),

    CONSTRAINT FK_Inventario_Productos
    FOREIGN KEY (CodigoProducto)
    REFERENCES Tbl_Productos(CodigoProducto)
);
GO


-- COMPRAS
CREATE TABLE Tbl_Compras
(
    CodigoCompra INT IDENTITY(1,1) PRIMARY KEY,
    CodigoSucursal INT NOT NULL,
    CodigoProveedor INT NOT NULL,
    CodigoEmpleado INT NOT NULL,
    NumeroCompra VARCHAR(30) NOT NULL,
    FechaCompra DATE NOT NULL,
    FechaEntregaProgramada DATE NOT NULL,
    FechaRecepcion DATE NULL,
    Estado VARCHAR(20) NOT NULL,
    Observaciones VARCHAR(300) NULL,

    CONSTRAINT UQ_Compras_NumeroCompra
    UNIQUE (NumeroCompra),

    CONSTRAINT CK_Compras_Fechas
    CHECK (
        FechaEntregaProgramada >= FechaCompra
        AND (FechaRecepcion IS NULL OR FechaRecepcion >= FechaCompra)
    ),

    CONSTRAINT CK_Compras_Estado
    CHECK (Estado IN ('Borrador', 'Recibida', 'Anulada')),

    CONSTRAINT FK_Compras_Proveedores
    FOREIGN KEY (CodigoProveedor)
    REFERENCES Tbl_Proveedores(CodigoProveedor),

    CONSTRAINT FK_Compras_Sucursales
    FOREIGN KEY (CodigoSucursal)
    REFERENCES Tbl_Sucursales(CodigoSucursal),

    CONSTRAINT FK_Compras_EmpleadoSucursal
    FOREIGN KEY (CodigoEmpleado, CodigoSucursal)
    REFERENCES Tbl_Empleados(CodigoEmpleado, CodigoSucursal)
);
GO


-- DETALLE DE COMPRAS
CREATE TABLE Tbl_ComprasDetalle
(
    CodigoDetalleCompra INT IDENTITY(1,1) PRIMARY KEY,
    CodigoCompra INT NOT NULL,
    CodigoProducto INT NOT NULL,
    Cantidad DECIMAL(12,2) NOT NULL,
    CostoUnitario DECIMAL(12,2) NOT NULL,
    Descuento DECIMAL(12,2) NOT NULL DEFAULT 0,
    Observaciones VARCHAR(300) NULL,

    CONSTRAINT UQ_ComprasDetalle_Producto
    UNIQUE (CodigoCompra, CodigoProducto),

    CONSTRAINT CK_ComprasDetalle_Cantidad
    CHECK (Cantidad > 0),

    CONSTRAINT CK_ComprasDetalle_Costo
    CHECK (CostoUnitario > 0),

    CONSTRAINT CK_ComprasDetalle_Descuento
    CHECK (Descuento >= 0 AND Descuento <= Cantidad * CostoUnitario),

    CONSTRAINT FK_ComprasDetalle_Compras
    FOREIGN KEY (CodigoCompra)
    REFERENCES Tbl_Compras(CodigoCompra),

    CONSTRAINT FK_ComprasDetalle_Productos
    FOREIGN KEY (CodigoProducto)
    REFERENCES Tbl_Productos(CodigoProducto)
);
GO


-- VENTAS
CREATE TABLE Tbl_Ventas
(
    CodigoVenta INT IDENTITY(1,1) PRIMARY KEY,
    CodigoCliente INT NOT NULL,
    CodigoSucursal INT NOT NULL,
    CodigoEmpleado INT NOT NULL,
    NumeroVenta VARCHAR(30) NOT NULL,
    FechaVenta DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    Observaciones VARCHAR(300) NULL,

    CONSTRAINT UQ_Ventas_NumeroVenta
    UNIQUE (NumeroVenta),

    CONSTRAINT CK_Ventas_Estado
    CHECK (Estado IN ('Borrador', 'Confirmada', 'Anulada')),

    CONSTRAINT FK_Ventas_Clientes
    FOREIGN KEY (CodigoCliente)
    REFERENCES Tbl_Clientes(CodigoCliente),

    CONSTRAINT FK_Ventas_Sucursales
    FOREIGN KEY (CodigoSucursal)
    REFERENCES Tbl_Sucursales(CodigoSucursal),

    CONSTRAINT FK_Ventas_EmpleadoSucursal
    FOREIGN KEY (CodigoEmpleado, CodigoSucursal)
    REFERENCES Tbl_Empleados(CodigoEmpleado, CodigoSucursal)
);
GO


-- DETALLE DE VENTAS
CREATE TABLE Tbl_VentasDetalle
(
    CodigoDetalleVenta INT IDENTITY(1,1) PRIMARY KEY,
    CodigoVenta INT NOT NULL,
    CodigoProducto INT NOT NULL,
    Cantidad DECIMAL(12,2) NOT NULL,
    PrecioUnitario DECIMAL(12,2) NOT NULL,
    Descuento DECIMAL(12,2) NOT NULL DEFAULT 0,
    CostoUnitarioHistorico DECIMAL(12,2) NOT NULL,

    CONSTRAINT UQ_VentasDetalle_Producto
    UNIQUE (CodigoVenta, CodigoProducto),

    CONSTRAINT CK_VentasDetalle_Cantidad
    CHECK (Cantidad > 0),

    CONSTRAINT CK_VentasDetalle_Precio
    CHECK (PrecioUnitario > 0),

    CONSTRAINT CK_VentasDetalle_CostoHistorico
    CHECK (CostoUnitarioHistorico > 0),

    CONSTRAINT CK_VentasDetalle_Descuento
    CHECK (Descuento >= 0 AND Descuento <= Cantidad * PrecioUnitario),

    CONSTRAINT FK_VentasDetalle_Ventas
    FOREIGN KEY (CodigoVenta)
    REFERENCES Tbl_Ventas(CodigoVenta),

    CONSTRAINT FK_VentasDetalle_Productos
    FOREIGN KEY (CodigoProducto)
    REFERENCES Tbl_Productos(CodigoProducto)
);
GO


-- PAGOS
CREATE TABLE Tbl_Pagos
(
    CodigoPago INT IDENTITY(1,1) PRIMARY KEY,
    CodigoVenta INT NOT NULL,
    Comprobante VARCHAR(50) NOT NULL,
    FechaPago DATE NOT NULL,
    MetodoPago VARCHAR(50) NOT NULL,
    Monto DECIMAL(12,2) NOT NULL,
    Comision DECIMAL(12,2) NOT NULL DEFAULT 0,
    Autorizacion VARCHAR(50) NULL,
    Estado VARCHAR(20) NOT NULL,

    CONSTRAINT UQ_Pagos_Comprobante
    UNIQUE (Comprobante),

    CONSTRAINT CK_Pagos_Monto
    CHECK (Monto > 0),

    CONSTRAINT CK_Pagos_Comision
    CHECK (Comision >= 0 AND Comision <= Monto),

    CONSTRAINT CK_Pagos_Estado
    CHECK (Estado IN ('Aplicado', 'Anulado')),

    CONSTRAINT FK_Pagos_Ventas
    FOREIGN KEY (CodigoVenta)
    REFERENCES Tbl_Ventas(CodigoVenta)
);
GO


-- AJUSTES DE INVENTARIO
CREATE TABLE Tbl_AjustesInventario
(
    CodigoAjuste INT IDENTITY(1,1) PRIMARY KEY,
    CodigoSucursal INT NOT NULL,
    CodigoEmpleado INT NOT NULL,
    NumeroAjuste VARCHAR(30) NOT NULL,
    FechaAjuste DATE NOT NULL,
    FechaAplicacion DATE NULL,
    Motivo VARCHAR(200) NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    Observaciones VARCHAR(300) NULL,

    CONSTRAINT UQ_Ajustes_NumeroAjuste
    UNIQUE (NumeroAjuste),

    CONSTRAINT CK_Ajustes_Estado
    CHECK (Estado IN ('Borrador', 'Aplicado', 'Anulado')),

    CONSTRAINT FK_Ajustes_Sucursales
    FOREIGN KEY (CodigoSucursal)
    REFERENCES Tbl_Sucursales(CodigoSucursal),

    CONSTRAINT FK_Ajustes_EmpleadoSucursal
    FOREIGN KEY (CodigoEmpleado, CodigoSucursal)
    REFERENCES Tbl_Empleados(CodigoEmpleado, CodigoSucursal)
);
GO


-- DETALLE DE AJUSTES
CREATE TABLE Tbl_AjustesInventarioDetalle
(
    CodigoDetalleAjuste INT IDENTITY(1,1) PRIMARY KEY,
    CodigoAjuste INT NOT NULL,
    CodigoProducto INT NOT NULL,
    Cantidad DECIMAL(12,2) NOT NULL,
    CostoUnitario DECIMAL(12,2) NULL,

    CONSTRAINT UQ_AjustesDetalle_Producto
    UNIQUE (CodigoAjuste, CodigoProducto),

    CONSTRAINT CK_AjustesDetalle_Cantidad
    CHECK (Cantidad <> 0),

    CONSTRAINT CK_AjustesDetalle_CostoUnitario
    CHECK (CostoUnitario IS NULL OR CostoUnitario > 0),

    CONSTRAINT FK_AjustesDetalle_Ajustes
    FOREIGN KEY (CodigoAjuste)
    REFERENCES Tbl_AjustesInventario(CodigoAjuste),

    CONSTRAINT FK_AjustesDetalle_Productos
    FOREIGN KEY (CodigoProducto)
    REFERENCES Tbl_Productos(CodigoProducto)
);
GO
