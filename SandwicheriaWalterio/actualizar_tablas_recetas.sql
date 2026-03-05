-- =============================================
-- SCRIPT DE ACTUALIZACIÓN DE BASE DE DATOS
-- Ejecutar en pgAdmin sobre la base de datos sandwicheria_local
-- =============================================

-- 1. Agregar columnas de stock a la tabla Recetas
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Recetas' AND column_name = 'StockActual') THEN
        ALTER TABLE "Recetas" ADD COLUMN "StockActual" INTEGER NOT NULL DEFAULT 0;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Recetas' AND column_name = 'StockMinimo') THEN
        ALTER TABLE "Recetas" ADD COLUMN "StockMinimo" INTEGER NOT NULL DEFAULT 5;
    END IF;
END $$;

-- 2. Agregar columna NombreReceta a DetalleVentas
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DetalleVentas' AND column_name = 'NombreReceta') THEN
        ALTER TABLE "DetalleVentas" ADD COLUMN "NombreReceta" VARCHAR(100) NULL;
    END IF;
END $$;

-- 3. Hacer ProductoID nullable en DetalleVentas (para permitir recetas sin producto)
ALTER TABLE "DetalleVentas" ALTER COLUMN "ProductoID" DROP NOT NULL;

-- 4. IMPORTANTE: Cambiar StockActual y StockMinimo de Productos a DECIMAL
-- Esto permite almacenar fracciones como 2.7 Kg, 0.5 Litros, etc.
ALTER TABLE "Productos" ALTER COLUMN "StockActual" TYPE DECIMAL(18,3);
ALTER TABLE "Productos" ALTER COLUMN "StockMinimo" TYPE DECIMAL(18,3);

-- 5. Eliminar la restricción de foreign key si causa problemas
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_DetalleVentas_Productos_ProductoID' 
        AND table_name = 'DetalleVentas'
    ) THEN
        ALTER TABLE "DetalleVentas" DROP CONSTRAINT "FK_DetalleVentas_Productos_ProductoID";
    END IF;
END $$;

-- Verificar cambios
SELECT 'Columnas en Recetas:' AS info;
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'Recetas' AND column_name IN ('StockActual', 'StockMinimo');

SELECT 'Columnas en DetalleVentas:' AS info;
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'DetalleVentas' AND column_name IN ('NombreReceta', 'ProductoID');

SELECT 'Columnas en Productos:' AS info;
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'Productos' AND column_name IN ('StockActual', 'StockMinimo');

-- Mensaje de éxito
SELECT '✅ Actualización completada exitosamente' AS resultado;
