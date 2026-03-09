-- ============================================
-- SCRIPT DE MIGRACION: sandwicheria_local -> sandwicheria_saas
-- Solo datos ACTIVOS y limpios
-- TenantId: 901073df6c83
-- ============================================

-- 1. Limpiar datos de prueba anteriores (mantener usuario diego)
DELETE FROM "DetalleVentas";
DELETE FROM "Ventas";
DELETE FROM "Cajas";
DELETE FROM "IngredientesReceta";
DELETE FROM "Recetas";
DELETE FROM "MovimientosStock";
DELETE FROM "Productos";
DELETE FROM "Categorias";

-- 2. Resetear secuencias (para que los IDs empiecen desde 1)
ALTER SEQUENCE "Categorias_CategoriaID_seq" RESTART WITH 1;
ALTER SEQUENCE "Productos_ProductoID_seq" RESTART WITH 1;
ALTER SEQUENCE "Recetas_RecetaID_seq" RESTART WITH 1;
ALTER SEQUENCE "IngredientesReceta_IngredienteRecetaID_seq" RESTART WITH 1;

-- ============================================
-- 3. CATEGORIAS (solo activas, sin "Ambos")
-- ============================================
-- Menu (6 categorias)
INSERT INTO "Categorias" ("Nombre", "TipoCategoria", "Activo", "CantidadDescuento", "TenantId") VALUES
('Sandwiches',    'Menu', true, 1, '901073df6c83'),
('Pizzas',        'Menu', true, 1, '901073df6c83'),
('Empanadas',     'Menu', true, 1, '901073df6c83'),
('Salchipapas',   'Menu', true, 1, '901073df6c83'),
('Panchos',       'Menu', true, 1, '901073df6c83'),
('Promos',        'Menu', true, 1, '901073df6c83'),
('Docena',        'Menu', true, 1, '901073df6c83'),
('Bebida',        'Menu', true, 1, '901073df6c83');

-- Mercaderia (7 categorias)
INSERT INTO "Categorias" ("Nombre", "TipoCategoria", "Activo", "CantidadDescuento", "TenantId") VALUES
('Insumos Sandwiches',  'Mercaderia', true, 1, '901073df6c83'),
('Insumos Pizzas',      'Mercaderia', true, 1, '901073df6c83'),
('Insumos Empanadas',   'Mercaderia', true, 1, '901073df6c83'),
('Insumos Salchipapas', 'Mercaderia', true, 1, '901073df6c83'),
('Insumos Panchos',     'Mercaderia', true, 1, '901073df6c83'),
('Insumos Docena',      'Mercaderia', true, 1, '901073df6c83'),
('Mercaderia General',  'Mercaderia', true, 1, '901073df6c83');

-- IDs resultantes:
-- 1=Sandwiches, 2=Pizzas, 3=Empanadas, 4=Salchipapas, 5=Panchos, 6=Promos, 7=Docena, 8=Bebida
-- 9=Ins.Sandwiches, 10=Ins.Pizzas, 11=Ins.Empanadas, 12=Ins.Salchipapas, 13=Ins.Panchos, 14=Ins.Docena, 15=Merc.General

-- ============================================
-- 4. PRODUCTOS MENU (solo activos)
-- ============================================
INSERT INTO "Productos" ("CategoriaID", "Nombre", "Precio", "StockActual", "StockMinimo", "UnidadMedida", "Activo", "FechaCreacion", "TenantId") VALUES
-- Bebidas (CategoriaID=8)
(8, 'COCA COLA 500ml',  3000.00, 49, 10, 'Unidad', true, NOW(), '901073df6c83'),
(8, 'Sprite',           3000.00, 49, 10, 'Unidad', true, NOW(), '901073df6c83'),
(8, 'COCA 500ml',       4500.00, 50, 10, 'Unidad', true, NOW(), '901073df6c83');

-- ============================================
-- 5. PRODUCTOS MERCADERIA (solo activos)
-- ============================================
INSERT INTO "Productos" ("CategoriaID", "Nombre", "Precio", "StockActual", "StockMinimo", "UnidadMedida", "Activo", "FechaCreacion", "TenantId") VALUES
-- Insumos Sandwiches (CategoriaID=9)
(9, 'Pan alpargata',  5600.00, 14, 5, 'Unidad', true, NOW(), '901073df6c83'),
(9, 'Picana',         9000.00, 12, 3, 'Kg',     true, NOW(), '901073df6c83'),

-- Insumos Pizzas (CategoriaID=10)
(10, 'Muzarella',  650.00, 0,  2, 'Kg',     true, NOW(), '901073df6c83'),
(10, 'Prepizza',   650.00, 46, 10, 'Unidad', true, NOW(), '901073df6c83'),

-- Insumos Panchos (CategoriaID=13)
(13, 'Pan de Viena',  560.00, 56, 10, 'Unidad', true, NOW(), '901073df6c83'),
(13, 'Salchichas',    500.00, 56, 10, 'Unidad', true, NOW(), '901073df6c83');

-- IDs de productos resultantes:
-- 1=COCA COLA 500ml, 2=Sprite, 3=COCA 500ml
-- 4=Pan alpargata, 5=Picana, 6=Muzarella, 7=Prepizza, 8=Pan de Viena, 9=Salchichas

-- ============================================
-- 6. RECETAS (solo activas)
-- ============================================
INSERT INTO "Recetas" ("Nombre", "CategoriaID", "Precio", "StockActual", "StockMinimo", "Activo", "FechaCreacion", "TenantId") VALUES
('Empanadas',       3,  560.00, 57, 10, true, NOW(), '901073df6c83'),  -- RecetaID=1
('Pizza comun',     2, 5600.00, 27,  5, true, NOW(), '901073df6c83'),  -- RecetaID=2
('Pizza huev0',     2, 5600.00, 31,  5, true, NOW(), '901073df6c83'),  -- RecetaID=3
('Sandwich Lomito', 1, 6500.00, 14,  3, true, NOW(), '901073df6c83'),  -- RecetaID=4
('Super Pancho',    5, 3500.00, 58, 10, true, NOW(), '901073df6c83'),  -- RecetaID=5
('Media Pizza',     2,  111.00,  0,  5, true, NOW(), '901073df6c83');  -- RecetaID=6

-- ============================================
-- 7. INGREDIENTES DE RECETAS
-- ============================================
INSERT INTO "IngredientesReceta" ("RecetaID", "ProductoMercaderiaID", "Cantidad", "UnidadMedida", "TenantId") VALUES
-- Empanadas (RecetaID=1): no tiene ingredientes de mercaderia activos disponibles
-- (las tapas de empanadas estan inactivas en tu BD local)

-- Pizza comun (RecetaID=2): Muzarella 300g + Prepizza 1u
(2, 6, 300, 'Gramo',  '901073df6c83'),
(2, 7,   1, 'Unidad', '901073df6c83'),

-- Pizza huev0 (RecetaID=3): Muzarella 300g + Prepizza 1u
(3, 6, 300, 'Gramo',  '901073df6c83'),
(3, 7,   1, 'Unidad', '901073df6c83'),

-- Sandwich Lomito (RecetaID=4): Pan alpargata 1u + Picana 500g
(4, 4, 1,   'Unidad', '901073df6c83'),
(4, 5, 500, 'Gramo',  '901073df6c83'),

-- Super Pancho (RecetaID=5): Pan de Viena 2u + Salchichas 2u
(5, 8, 2, 'Unidad', '901073df6c83'),
(5, 9, 2, 'Unidad', '901073df6c83'),

-- Media Pizza (RecetaID=6): Prepizza 5u
(6, 7, 5, 'Unidad', '901073df6c83');

-- ============================================
-- FIN DE LA MIGRACION
-- ============================================
