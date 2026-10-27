/*
Proyecto: Supermercado Multisucursal - Base de Datos I

Datos de prueba para una instalación académica nueva.
Nombres personales, contactos, correos example.com, DPI, NIT y teléfonos de prueba:
no corresponden a datos de contacto reales.
No ejecutar sobre una base existente.
*/


USE SupermercadosGuate;
GO


-- SUCURSALES

INSERT INTO Tbl_Sucursales
(Nombre, Direccion, Municipio, Departamento, Telefono, FechaApertura, Estado)
VALUES
('Sucursal Zona 1', '6a Avenida 10-25 Zona 1', 'Guatemala', 'Guatemala', '0000-0001', '2018-03-15', 'Activo'),
('Sucursal Mixco', 'Calzada Roosevelt 15-30', 'Mixco', 'Guatemala', '0000-0002', '2019-06-20', 'Activo'),
('Sucursal Villa Nueva', '5a Avenida 3-45 Zona 1', 'Villa Nueva', 'Guatemala', '0000-0003', '2020-02-10', 'Activo'),
('Sucursal Cuilapa', '1a Calle 4-20 Zona 2', 'Cuilapa', 'Santa Rosa', '0000-0004', '2021-08-05', 'Activo'),
('Sucursal Quetzaltenango', '12 Avenida 4-35 Zona 1', 'Quetzaltenango', 'Quetzaltenango', '0000-0005', '2022-04-18', 'Activo');
GO


-- CATEGORIAS

INSERT INTO Tbl_Categorias
(Nombre, Descripcion, Estado)
VALUES
('Bebidas', 'Bebidas gaseosas, jugos y agua', 'Activo'),
('Lacteos', 'Leche, queso y productos derivados', 'Activo'),
('Granos Basicos', 'Arroz, frijol y otros granos', 'Activo'),
('Limpieza', 'Productos para limpieza del hogar', 'Activo'),
('Higiene Personal', 'Productos de cuidado personal', 'Activo'),
('Snacks', 'Galletas, papalinas y productos similares', 'Activo');
GO


-- PROVEEDORES

INSERT INTO Tbl_Proveedores
(NIT, Nombre, Contacto, Municipio, Departamento, Telefono, Correo, DiasCredito, FechaRegistro, Estado)
VALUES
('PRUEBA-P-01', 'Distribuidora Central, S.A.', 'Contacto Prueba01', 'Guatemala', 'Guatemala', '0000-0006', 'proveedor01@example.com', 30, '2021-01-10', 'Activo'),
('PRUEBA-P-02', 'Alimentos de Guatemala, S.A.', 'Contacto Prueba02', 'Mixco', 'Guatemala', '0000-0007', 'proveedor02@example.com', 30, '2021-02-15', 'Activo'),
('PRUEBA-P-03', 'Lacteos del Valle, S.A.', 'Contacto Prueba03', 'Villa Nueva', 'Guatemala', '0000-0008', 'proveedor03@example.com', 15, '2021-03-20', 'Activo'),
('PRUEBA-P-04', 'Productos Limpios, S.A.', 'Contacto Prueba04', 'Escuintla', 'Escuintla', '0000-0009', 'proveedor04@example.com', 30, '2022-01-12', 'Activo'),
('PRUEBA-P-05', 'Cuidado Personal GT, S.A.', 'Contacto Prueba05', 'Guatemala', 'Guatemala', '0000-0010', NULL, 20, '2022-05-05', 'Activo'),
('PRUEBA-P-06', 'Comercial Chapina, S.A.', 'Contacto Prueba06', 'Quetzaltenango', 'Quetzaltenango', '0000-0011', 'proveedor05@example.com', 45, '2023-02-01', 'Activo');
GO


-- CLIENTES

INSERT INTO Tbl_Clientes
(NIT, Nombres, Apellidos, Municipio, Departamento, Telefono, Correo, FechaRegistro, Estado)
VALUES
('CF', 'Consumidor', 'Final', 'Guatemala', 'Guatemala', NULL, NULL, '2020-01-01', 'Activo'),
('PRUEBA-C-01', 'Cliente', 'Prueba01', 'Guatemala', 'Guatemala', '0000-0012', 'cliente01@example.com', '2022-03-15', 'Activo'),
('PRUEBA-C-02', 'Cliente', 'Prueba02', 'Mixco', 'Guatemala', '0000-0013', NULL, '2023-07-20', 'Activo'),
('PRUEBA-C-03', 'Cliente', 'Prueba03', 'Quetzaltenango', 'Quetzaltenango', '0000-0014', 'cliente02@example.com', '2024-02-10', 'Activo'),
('PRUEBA-C-04', 'Cliente', 'Prueba04', 'Cuilapa', 'Santa Rosa', '0000-0015', 'cliente03@example.com', '2024-11-05', 'Activo'),
('PRUEBA-C-05', 'Cliente', 'Prueba05', 'Quetzaltenango', 'Quetzaltenango', '0000-0016', NULL, '2025-06-30', 'Activo'),
('PRUEBA-C-06', 'Cliente', 'Prueba06', 'Escuintla', 'Escuintla', '0000-0017', 'cliente04@example.com', '2025-09-18', 'Activo'),
('PRUEBA-C-07', 'Cliente', 'Prueba07', 'Antigua Guatemala', 'Sacatepequez', '0000-0018', 'cliente05@example.com', '2025-12-01', 'Activo'),
('PRUEBA-C-08', 'Cliente', 'Prueba08', 'Guatemala', 'Guatemala', '0000-0019', NULL, '2026-01-10', 'Activo'),
('PRUEBA-C-09', 'Cliente', 'Prueba09', 'Mixco', 'Guatemala', '0000-0020', 'cliente06@example.com', '2026-05-20', 'Activo'),
('PRUEBA-C-10', 'Cliente', 'Prueba10', 'Villa Nueva', 'Guatemala', '0000-0021', 'cliente07@example.com', '2026-06-15', 'Activo'),
('PRUEBA-C-11', 'Cliente', 'Prueba11', 'Quetzaltenango', 'Quetzaltenango', '0000-0022', NULL, '2026-07-10', 'Activo'),
('PRUEBA-C-12', 'Cliente', 'Prueba12', 'Guatemala', 'Guatemala', '0000-0023', 'cliente08@example.com', '2023-05-12', 'Inactivo'),
('PRUEBA-C-13', 'Cliente', 'Prueba13', 'Quetzaltenango', 'Quetzaltenango', '0000-0024', 'cliente09@example.com', '2025-08-14', 'Activo');
GO


-- EMPLEADOS

INSERT INTO Tbl_Empleados
(CodigoSucursal, DPI, Nombres, Apellidos, Puesto, FechaNacimiento, FechaContratacion, Telefono, Correo, Salario, Estado)
VALUES
(1, '0000000000001', 'Empleado', 'Prueba01', 'Administrador', '1990-05-10', '2022-01-15', '0000-0025', 'empleado01@example.com', 6500.00, 'Activo'),
(1, '0000000000002', 'Empleado', 'Prueba02', 'Cajero', '1997-08-21', '2023-03-10', '0000-0026', 'empleado02@example.com', 4200.00, 'Activo'),
(2, '0000000000003', 'Empleado', 'Prueba03', 'Administrador', '1995-04-12', '2023-07-01', '0000-0027', 'empleado03@example.com', 6200.00, 'Activo'),
(2, '0000000000004', 'Empleado', 'Prueba04', 'Cajero', '1998-11-30', '2024-02-05', '0000-0028', 'empleado04@example.com', 4100.00, 'Activo'),
(3, '0000000000005', 'Empleado', 'Prueba05', 'Administrador', '1989-02-18', '2021-09-20', '0000-0029', 'empleado05@example.com', 6600.00, 'Activo'),
(3, '0000000000006', 'Empleado', 'Prueba06', 'Cajero', '1999-07-14', '2024-04-12', '0000-0030', 'empleado06@example.com', 4050.00, 'Activo'),
(4, '0000000000007', 'Empleado', 'Prueba07', 'Administrador', '1991-09-25', '2022-06-15', '0000-0031', 'empleado07@example.com', 6300.00, 'Activo'),
(4, '0000000000008', 'Empleado', 'Prueba08', 'Cajero', '2000-01-08', '2025-01-10', '0000-0032', 'empleado08@example.com', 4000.00, 'Activo'),
(5, '0000000000009', 'Empleado', 'Prueba09', 'Administrador', '1988-12-11', '2022-11-01', '0000-0033', 'empleado09@example.com', 6700.00, 'Activo'),
(5, '0000000000010', 'Empleado', 'Prueba10', 'Cajero', '1998-03-19', '2024-08-05', '0000-0034', 'empleado10@example.com', 4150.00, 'Activo');
GO


-- PRODUCTOS

INSERT INTO Tbl_Productos
(CodigoCategoria, CodigoBarra, Nombre, Marca, Descripcion, UnidadMedida, PrecioVenta, FechaRegistro, Estado)
VALUES
(1, '740100000001', 'Agua Pura 600 ml', 'Aqua', 'Botella de agua pura', 'Unidad', 5.00, '2021-01-10', 'Activo'),
(1, '740100000002', 'Gaseosa Cola 2 Litros', 'Cola GT', 'Gaseosa sabor cola', 'Unidad', 18.00, '2021-01-10', 'Activo'),
(1, '740100000003', 'Jugo de Naranja 1 Litro', 'Frutal', 'Jugo de naranja', 'Unidad', 22.00, '2021-02-15', 'Activo'),
(2, '740100000004', 'Leche Entera 1 Litro', 'Valle', 'Leche entera UHT', 'Unidad', 15.00, '2021-03-20', 'Activo'),
(2, '740100000005', 'Queso Fresco 1 Libra', 'Valle', 'Queso fresco empacado', 'Libra', 32.00, '2021-03-20', 'Activo'),
(3, '740100000006', 'Arroz Blanco 1 Libra', 'Buen Grano', 'Arroz blanco empacado', 'Libra', 8.00, '2021-04-12', 'Activo'),
(3, '740100000007', 'Frijol Negro 1 Libra', 'Buen Grano', 'Frijol negro empacado', 'Libra', 10.00, '2021-04-12', 'Activo'),
(4, '740100000008', 'Detergente 1 kg', 'Limpio', 'Detergente en polvo', 'Unidad', 28.00, '2022-01-12', 'Activo'),
(4, '740100000009', 'Cloro 1 Galon', 'Limpio', 'Cloro para limpieza', 'Unidad', 22.00, '2022-01-12', 'Activo'),
(5, '740100000010', 'Shampoo 400 ml', 'Fresh', 'Shampoo para cabello', 'Unidad', 30.00, '2022-05-05', 'Activo'),
(5, '740100000011', 'Jabon de Bano Pack 3', 'Fresh', 'Paquete de tres jabones', 'Paquete', 18.00, '2022-05-05', 'Activo'),
(6, '740100000012', 'Galletas Surtidas', 'Delicia', 'Paquete de galletas surtidas', 'Unidad', 12.00, '2023-02-01', 'Activo'),
(6, '740100000013', 'Papalinas 150 g', 'Delicia', 'Bolsa de papalinas', 'Unidad', 10.00, '2023-02-01', 'Activo');
GO


-- PRODUCTO PROVEEDOR

INSERT INTO Tbl_ProductoProveedor
(CodigoProducto, CodigoProveedor, CodigoExterno, CostoReferencia, PlazoEntrega, FechaRegistro, Estado)
VALUES
(1, 1, 'DC-AGUA-01', 3.00, 2, '2021-01-10', 'Activo'),
(2, 1, 'DC-COLA-02', 12.00, 2, '2021-01-10', 'Activo'),
(3, 1, 'DC-JUGO-03', 15.00, 3, '2021-02-15', 'Activo'),
(4, 3, 'LV-LECHE-01', 11.00, 2, '2021-03-20', 'Activo'),
(5, 3, 'LV-QUESO-02', 24.00, 2, '2021-03-20', 'Activo'),
(6, 2, 'AG-ARROZ-01', 5.50, 4, '2021-04-12', 'Activo'),
(7, 2, 'AG-FRIJOL-02', 7.00, 4, '2021-04-12', 'Activo'),
(8, 4, 'PL-DETER-01', 20.00, 3, '2022-01-12', 'Activo'),
(9, 4, 'PL-CLORO-02', 15.00, 3, '2022-01-12', 'Activo'),
(10, 5, 'CP-SHAMP-01', 22.00, 3, '2022-05-05', 'Activo'),
(11, 5, 'CP-JABON-02', 12.00, 3, '2022-05-05', 'Activo'),
(12, 2, 'AG-GALLE-03', 8.00, 4, '2023-02-01', 'Activo'),
(13, 2, 'AG-PAPA-04', 6.00, 4, '2023-02-01', 'Activo'),
(2, 6, 'CC-COLA-11', 12.50, 5, '2023-05-10', 'Activo'),
(6, 6, 'CC-ARROZ-12', 5.75, 5, '2023-05-10', 'Activo'),
(12, 6, 'CC-GALLE-13', 8.25, 5, '2023-05-10', 'Activo');
GO


-- INVENTARIO

INSERT INTO Tbl_Inventario
(CodigoSucursal, CodigoProducto, Existencia, ExistenciaMinima, CostoPromedio, FechaUltimoIngreso, FechaUltimoConteo)
VALUES
(1, 1, 150, 30, 3.00, '2026-09-01', '2026-09-28'),
(1, 2, 120, 25, 12.00, '2026-09-01', '2026-09-28'),
(1, 3, 100, 20, 15.00, '2026-09-01', '2026-09-28'),
(1, 4, 90, 20, 11.00, '2026-08-25', '2026-09-28'),
(1, 5, 75, 15, 24.00, '2026-08-25', '2026-09-28'),
(1, 6, 140, 30, 5.50, '2026-09-05', '2026-09-28'),
(1, 7, 110, 25, 7.00, '2026-09-05', '2026-09-28'),
(1, 8, 80, 20, 20.00, '2026-09-08', '2026-09-28'),
(1, 9, 65, 15, 15.00, '2026-09-08', '2026-09-28'),
(1, 10, 70, 15, 22.00, '2026-09-10', '2026-09-28'),
(1, 11, 95, 20, 12.00, '2026-09-10', '2026-09-28'),
(1, 12, 130, 25, 8.00, '2026-09-12', '2026-09-28'),
(1, 13, 125, 25, 6.00, '2026-09-12', '2026-09-28'),

(2, 1, 100, 25, 3.00, '2026-09-02', '2026-09-27'),
(2, 2, 95, 20, 12.10, '2026-09-02', '2026-09-27'),
(2, 3, 85, 20, 15.00, '2026-08-26', '2026-09-27'),
(2, 4, 110, 25, 11.00, '2026-08-26', '2026-09-27'),
(2, 5, 60, 15, 24.00, '2026-08-26', '2026-09-27'),
(2, 6, 135, 30, 5.60, '2026-09-03', '2026-09-27'),
(2, 7, 120, 25, 7.00, '2026-09-03', '2026-09-27'),
(2, 8, 70, 20, 20.00, '2026-09-06', '2026-09-27'),
(2, 9, 55, 15, 15.00, '2026-09-06', '2026-09-27'),
(2, 10, 75, 15, 22.00, '2026-09-09', '2026-09-27'),
(2, 11, 90, 20, 12.00, '2026-09-09', '2026-09-27'),
(2, 12, 115, 25, 8.10, '2026-09-11', '2026-09-27'),
(2, 13, 105, 25, 6.00, '2026-09-11', '2026-09-27'),

(3, 1, 90, 20, 3.00, '2026-09-01', '2026-09-26'),
(3, 2, 80, 20, 12.00, '2026-09-01', '2026-09-26'),
(3, 3, 75, 15, 15.00, '2026-08-24', '2026-09-26'),
(3, 4, 85, 20, 11.00, '2026-08-24', '2026-09-26'),
(3, 5, 50, 12, 24.00, '2026-08-24', '2026-09-26'),
(3, 6, 100, 25, 5.50, '2026-09-04', '2026-09-26'),
(3, 7, 95, 20, 7.00, '2026-09-04', '2026-09-26'),
(3, 8, 65, 15, 20.00, '2026-09-07', '2026-09-26'),
(3, 9, 60, 15, 15.00, '2026-09-07', '2026-09-26'),
(3, 10, 60, 15, 22.00, '2026-09-09', '2026-09-26'),
(3, 11, 70, 15, 12.00, '2026-09-09', '2026-09-26'),
(3, 12, 100, 20, 8.00, '2026-09-11', '2026-09-26'),
(3, 13, 90, 20, 6.00, '2026-09-11', '2026-09-26'),

(4, 1, 70, 20, 3.00, '2026-08-28', '2026-09-25'),
(4, 2, 65, 15, 12.00, '2026-08-28', '2026-09-25'),
(4, 3, 60, 15, 15.00, '2026-08-28', '2026-09-25'),
(4, 4, 75, 20, 11.00, '2026-08-30', '2026-09-25'),
(4, 5, 45, 10, 24.00, '2026-08-30', '2026-09-25'),
(4, 6, 85, 20, 5.50, '2026-09-02', '2026-09-25'),
(4, 7, 80, 20, 7.00, '2026-09-02', '2026-09-25'),
(4, 8, 50, 15, 20.00, '2026-09-05', '2026-09-25'),
(4, 9, 45, 12, 15.00, '2026-09-05', '2026-09-25'),
(4, 10, 55, 12, 22.00, '2026-09-08', '2026-09-25'),
(4, 11, 60, 15, 12.00, '2026-09-08', '2026-09-25'),
(4, 12, 80, 20, 8.00, '2026-09-10', '2026-09-25'),
(4, 13, 70, 18, 6.00, '2026-09-10', '2026-09-25'),

(5, 1, 85, 20, 3.00, '2026-08-29', '2026-09-24'),
(5, 2, 70, 18, 12.00, '2026-08-29', '2026-09-24'),
(5, 3, 65, 15, 15.00, '2026-08-29', '2026-09-24'),
(5, 4, 80, 20, 11.00, '2026-08-31', '2026-09-24'),
(5, 5, 55, 12, 24.00, '2026-08-31', '2026-09-24'),
(5, 6, 90, 20, 5.50, '2026-09-03', '2026-09-24'),
(5, 7, 85, 20, 7.00, '2026-09-03', '2026-09-24'),
(5, 8, 55, 15, 20.00, '2026-09-06', '2026-09-24'),
(5, 9, 50, 12, 15.00, '2026-09-06', '2026-09-24'),
(5, 10, 65, 15, 22.00, '2026-09-09', '2026-09-24'),
(5, 11, 75, 18, 12.00, '2026-09-09', '2026-09-24'),
(5, 12, 95, 20, 8.00, '2026-09-11', '2026-09-24'),
(5, 13, 90, 20, 6.00, '2026-09-11', '2026-09-24');
GO


-- COMPRAS

INSERT INTO Tbl_Compras
(CodigoSucursal, CodigoProveedor, CodigoEmpleado, NumeroCompra, FechaCompra, FechaEntregaProgramada, FechaRecepcion, Estado, Observaciones)
VALUES
(1, 1, 1, 'FC-1001', '2026-01-05', '2026-01-07', '2026-01-07', 'Recibida', NULL),
(1, 3, 1, 'FC-1002', '2026-01-20', '2026-01-22', '2026-01-22', 'Recibida', NULL),
(2, 2, 3, 'FC-1003', '2026-02-10', '2026-02-14', '2026-02-12', 'Recibida', NULL),
(3, 4, 5, 'FC-1004', '2026-03-05', '2026-03-08', '2026-03-07', 'Recibida', NULL),
(4, 5, 7, 'FC-1005', '2026-04-10', '2026-04-13', '2026-04-12', 'Recibida', NULL),
(5, 2, 9, 'FC-1006', '2026-05-10', '2026-05-14', '2026-05-13', 'Recibida', NULL),
(2, 6, 3, 'FC-1007', '2026-08-01', '2026-08-06', '2026-08-05', 'Recibida', 'Compra con proveedor alterno'),
(4, 4, 7, 'FC-1008', '2026-10-01', '2026-10-04', NULL, 'Borrador', 'Pendiente de recibir'),
(5, 1, 9, 'FC-1009', '2026-06-20', '2026-06-23', NULL, 'Anulada', 'Pedido cancelado');
GO


-- DETALLE DE COMPRAS

INSERT INTO Tbl_ComprasDetalle
(CodigoCompra, CodigoProducto, Cantidad, CostoUnitario, Descuento, Observaciones)
VALUES
(1, 1, 100, 3.00, 0.00, NULL),
(1, 2, 80, 12.00, 20.00, NULL),
(1, 3, 60, 15.00, 0.00, NULL),

(2, 4, 80, 11.00, 0.00, NULL),
(2, 5, 50, 24.00, 25.00, NULL),

(3, 6, 120, 5.50, 0.00, NULL),
(3, 7, 100, 7.00, 0.00, NULL),
(3, 12, 80, 8.00, 10.00, NULL),

(4, 8, 70, 20.00, 0.00, NULL),
(4, 9, 60, 15.00, 0.00, NULL),

(5, 10, 60, 22.00, 20.00, NULL),
(5, 11, 80, 12.00, 0.00, NULL),

(6, 6, 90, 5.50, 0.00, NULL),
(6, 7, 80, 7.00, 0.00, NULL),
(6, 13, 100, 6.00, 0.00, NULL),

(7, 2, 70, 12.50, 15.00, NULL),
(7, 6, 100, 5.75, 0.00, NULL),
(7, 12, 90, 8.25, 10.00, NULL),

(8, 8, 30, 20.00, 0.00, 'Pendiente'),

(9, 1, 50, 3.00, 0.00, 'Compra anulada');
GO


-- VENTAS

INSERT INTO Tbl_Ventas
(CodigoCliente, CodigoSucursal, CodigoEmpleado, NumeroVenta, FechaVenta, Estado, Observaciones)
VALUES
(3, 1, 1, 'FV-2001', '2026-01-15', 'Confirmada', NULL),
(7, 2, 3, 'FV-2002', '2026-01-28', 'Confirmada', NULL),
(2, 3, 5, 'FV-2003', '2026-02-05', 'Confirmada', NULL),
(2, 1, 2, 'FV-2004', '2026-02-18', 'Confirmada', NULL),
(5, 4, 7, 'FV-2005', '2026-03-10', 'Confirmada', NULL),
(5, 1, 1, 'FV-2006', '2026-03-22', 'Confirmada', NULL),
(3, 2, 4, 'FV-2007', '2026-04-12', 'Confirmada', NULL),
(1, 5, 9, 'FV-2008', '2026-04-25', 'Confirmada', NULL),
(8, 1, 2, 'FV-2009', '2026-05-08', 'Confirmada', NULL),
(1, 3, 6, 'FV-2010', '2026-05-20', 'Confirmada', NULL),
(4, 2, 3, 'FV-2011', '2026-06-15', 'Confirmada', NULL),
(4, 3, 5, 'FV-2012', '2026-06-28', 'Confirmada', NULL),
(5, 1, 1, 'FV-2013', '2026-07-05', 'Confirmada', NULL),
(2, 2, 4, 'FV-2014', '2026-07-18', 'Confirmada', NULL),
(1, 4, 8, 'FV-2015', '2026-07-25', 'Confirmada', NULL),
(14, 5, 10, 'FV-2016', '2026-08-03', 'Confirmada', NULL),
(2, 1, 2, 'FV-2017', '2026-08-14', 'Confirmada', NULL),
(5, 3, 6, 'FV-2018', '2026-08-21', 'Confirmada', NULL),
(2, 1, 1, 'FV-2019', '2026-09-05', 'Confirmada', 'Pago parcial'),
(4, 1, 2, 'FV-2020', '2026-09-12', 'Confirmada', NULL),
(7, 2, 3, 'FV-2021', '2026-09-18', 'Confirmada', 'Pago parcial'),
(11, 3, 5, 'FV-2022', '2026-09-21', 'Confirmada', 'Pago parcial'),
(1, 4, 7, 'FV-2023', '2026-09-25', 'Confirmada', 'Pago parcial'),
(10, 4, 8, 'FV-2024', '2026-10-02', 'Borrador', 'Venta de practica sin confirmar'),
(14, 5, 9, 'FV-2025', '2026-09-10', 'Anulada', 'Venta anulada');
GO


-- DETALLE DE VENTAS

INSERT INTO Tbl_VentasDetalle
(CodigoVenta, CodigoProducto, Cantidad, PrecioUnitario, Descuento, CostoUnitarioHistorico)
VALUES
(1, 4, 10, 15.00, 0.00, 11.00),
(2, 2, 5, 18.00, 0.00, 12.00),
(3, 5, 5, 32.00, 0.00, 24.00),
(4, 6, 10, 8.00, 0.00, 5.50),
(5, 11, 5, 18.00, 0.00, 12.00),

(6, 3, 10, 22.00, 0.00, 15.00),
(6, 1, 4, 5.00, 0.00, 3.00),

(7, 10, 10, 30.00, 0.00, 22.00),
(8, 11, 10, 18.00, 0.00, 12.00),

(9, 5, 10, 32.00, 0.00, 24.00),
(9, 8, 5, 28.00, 10.00, 20.00),

(10, 3, 10, 22.00, 10.00, 15.00),

(11, 10, 10, 30.00, 0.00, 22.00),
(11, 5, 10, 32.00, 0.00, 24.00),

(12, 8, 10, 28.00, 30.00, 20.00),

(13, 2, 10, 18.00, 0.00, 12.00),
(13, 12, 2, 12.00, 4.00, 8.00),

(14, 8, 5, 28.00, 0.00, 20.00),
(15, 12, 10, 12.00, 0.00, 8.00),
(16, 9, 10, 22.00, 0.00, 15.00),

(17, 3, 5, 22.00, 0.00, 15.00),
(17, 1, 4, 5.00, 0.00, 3.00),

(18, 7, 10, 10.00, 0.00, 7.00),
(18, 6, 10, 8.00, 0.00, 5.50),
(18, 13, 3, 10.00, 0.00, 6.00),

(19, 5, 10, 32.00, 0.00, 24.00),
(19, 8, 10, 28.00, 0.00, 20.00),
(19, 10, 10, 30.00, 20.00, 22.00),

(20, 2, 10, 18.00, 0.00, 12.00),
(20, 3, 10, 22.00, 0.00, 15.00),
(20, 12, 10, 12.00, 0.00, 8.00),

(21, 4, 10, 15.00, 0.00, 11.00),
(21, 5, 5, 32.00, 0.00, 24.00),
(21, 13, 5, 10.00, 0.00, 6.00),

(22, 10, 5, 30.00, 0.00, 22.00),
(22, 8, 5, 28.00, 20.00, 20.00),

(23, 11, 10, 18.00, 0.00, 12.00),

(24, 8, 2, 28.00, 0.00, 20.00),

(25, 7, 10, 10.00, 0.00, 7.00);
GO


-- PAGOS

INSERT INTO Tbl_Pagos
(CodigoVenta, Comprobante, FechaPago, MetodoPago, Monto, Comision, Autorizacion, Estado)
VALUES
(1, 'rec-0001', '2026-01-15', 'Efectivo', 150.00, 0.00, NULL, 'Aplicado'),
(2, 'rec-0002', '2026-01-28', 'Tarjeta', 90.00, 2.00, 'AUT-1001', 'Aplicado'),

(3, 'rec-0003', '2026-02-05', 'Tarjeta', 100.00, 2.00, 'AUT-1002', 'Aplicado'),
(3, 'rec-0004', '2026-02-06', 'Efectivo', 60.00, 0.00, NULL, 'Aplicado'),

(4, 'rec-0005', '2026-02-18', 'Efectivo', 80.00, 0.00, NULL, 'Aplicado'),
(5, 'rec-0006', '2026-03-10', 'Efectivo', 90.00, 0.00, NULL, 'Aplicado'),
(6, 'rec-0007', '2026-03-22', 'Tarjeta', 240.00, 5.00, 'AUT-1003', 'Aplicado'),
(7, 'rec-0008', '2026-04-12', 'Transferencia', 300.00, 0.00, 'TR-2001', 'Aplicado'),
(8, 'rec-0009', '2026-04-25', 'Efectivo', 180.00, 0.00, NULL, 'Aplicado'),

(9, 'rec-0010', '2026-05-08', 'Tarjeta', 300.00, 6.00, 'AUT-1004', 'Aplicado'),
(9, 'rec-0011', '2026-05-08', 'Efectivo', 150.00, 0.00, NULL, 'Aplicado'),

(10, 'rec-0012', '2026-05-20', 'Efectivo', 210.00, 0.00, NULL, 'Aplicado'),

(11, 'rec-0013', '2026-06-15', 'Tarjeta', 400.00, 8.00, 'AUT-1005', 'Aplicado'),
(11, 'rec-0014', '2026-06-16', 'Transferencia', 220.00, 0.00, 'TR-2002', 'Aplicado'),

(12, 'rec-0015', '2026-06-28', 'Efectivo', 200.00, 0.00, NULL, 'Aplicado'),

(13, 'rec-0016', '2026-07-05', 'Efectivo', 100.00, 0.00, NULL, 'Aplicado'),
(13, 'rec-0017', '2026-07-05', 'Tarjeta', 100.00, 2.00, 'AUT-1006', 'Aplicado'),

(14, 'rec-0018', '2026-07-18', 'Efectivo', 60.00, 0.00, NULL, 'Aplicado'),
(14, 'rec-0019', '2026-07-18', 'Tarjeta', 80.00, 1.50, 'AUT-1007', 'Aplicado'),

(15, 'rec-0020', '2026-07-25', 'Efectivo', 120.00, 0.00, NULL, 'Aplicado'),
(16, 'rec-0021', '2026-08-03', 'Tarjeta', 220.00, 4.00, 'AUT-1008', 'Aplicado'),
(17, 'rec-0022', '2026-08-14', 'Efectivo', 130.00, 0.00, NULL, 'Aplicado'),

(18, 'rec-0023', '2026-08-21', 'Transferencia', 100.00, 0.00, 'TR-2003', 'Aplicado'),
(18, 'rec-0024', '2026-08-21', 'Efectivo', 110.00, 0.00, NULL, 'Aplicado'),

(19, 'rec-0025', '2026-09-05', 'Tarjeta', 500.00, 10.00, 'AUT-1009', 'Aplicado'),
(19, 'rec-0026', '2026-09-06', 'Efectivo', 200.00, 0.00, NULL, 'Aplicado'),

(20, 'rec-0027', '2026-09-12', 'Efectivo', 100.00, 0.00, NULL, 'Aplicado'),
(20, 'rec-0028', '2026-09-12', 'Tarjeta', 200.00, 4.00, 'AUT-1010', 'Aplicado'),
(20, 'rec-0029', '2026-09-13', 'Transferencia', 220.00, 0.00, 'TR-2004', 'Aplicado'),

(21, 'rec-0030', '2026-09-18', 'Tarjeta', 200.00, 4.00, 'AUT-1011', 'Aplicado'),
(21, 'rec-0031', '2026-09-18', 'Efectivo', 100.00, 0.00, NULL, 'Aplicado'),

(22, 'rec-0032', '2026-09-21', 'Efectivo', 100.00, 0.00, NULL, 'Aplicado'),
(22, 'rec-0033', '2026-09-22', 'Tarjeta', 70.00, 1.00, 'AUT-1012', 'Aplicado'),

(23, 'rec-0034', '2026-09-25', 'Efectivo', 60.00, 0.00, NULL, 'Aplicado'),

(25, 'rec-0035', '2026-09-10', 'Tarjeta', 100.00, 2.00, 'AUT-9999', 'Anulado');
GO


-- AJUSTES DE INVENTARIO

INSERT INTO Tbl_AjustesInventario
(CodigoSucursal, CodigoEmpleado, NumeroAjuste, FechaAjuste, Motivo, Estado, Observaciones)
VALUES
(1, 1, 'AJ-3001', '2026-03-01', 'Conteo fisico', 'Aplicado', 'Se encontraron unidades adicionales'),
(1, 1, 'AJ-3002', '2026-04-01', 'Producto danado', 'Aplicado', NULL),
(2, 3, 'AJ-3003', '2026-06-05', 'Correccion de inventario', 'Aplicado', NULL),
(3, 5, 'AJ-3004', '2026-08-10', 'Producto vencido', 'Aplicado', NULL),
(4, 7, 'AJ-3005', '2026-09-18', 'Producto danado', 'Aplicado', NULL),
(4, 7, 'AJ-3006', '2026-10-03', 'Revision pendiente', 'Borrador', NULL),
(5, 9, 'AJ-3007', '2026-09-01', 'Ajuste cancelado', 'Anulado', NULL);
GO


-- DETALLE DE AJUSTES

INSERT INTO Tbl_AjustesInventarioDetalle
(CodigoAjuste, CodigoProducto, Cantidad)
VALUES
(1, 1, 10),
(2, 4, -3),
(3, 6, 5),
(4, 8, -2),
(5, 11, -4),
(6, 8, 10),
(7, 13, -5);
GO
