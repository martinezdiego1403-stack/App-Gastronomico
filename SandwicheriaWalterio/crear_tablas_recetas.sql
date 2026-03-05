-- =====================================================
-- SCRIPT: Crear tablas de Recetas e Ingredientes
-- Para: PostgreSQL (sandwicheria_local)
-- Ejecutar en: pgAdmin
-- =====================================================

-- Tabla de Recetas
CREATE TABLE IF NOT EXISTS "Recetas" (
    "RecetaID" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100) NOT NULL,
    "Descripcion" VARCHAR(500) NULL,
    "CategoriaID" INTEGER NOT NULL,
    "Precio" DECIMAL(18,2) NOT NULL,
    "CodigoBarras" VARCHAR(50) NULL,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT "FK_Recetas_Categorias" FOREIGN KEY ("CategoriaID")
        REFERENCES "Categorias" ("CategoriaID") ON DELETE RESTRICT
);

-- Índices para Recetas
CREATE INDEX IF NOT EXISTS "IX_Recetas_Nombre" ON "Recetas" ("Nombre");
CREATE INDEX IF NOT EXISTS "IX_Recetas_CodigoBarras" ON "Recetas" ("CodigoBarras");
CREATE INDEX IF NOT EXISTS "IX_Recetas_CategoriaID" ON "Recetas" ("CategoriaID");

-- Tabla de Ingredientes de Receta
CREATE TABLE IF NOT EXISTS "IngredientesReceta" (
    "IngredienteRecetaID" SERIAL PRIMARY KEY,
    "RecetaID" INTEGER NOT NULL,
    "ProductoMercaderiaID" INTEGER NOT NULL,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "UnidadMedida" VARCHAR(20) NOT NULL DEFAULT 'unidad',
    
    CONSTRAINT "FK_IngredientesReceta_Recetas" FOREIGN KEY ("RecetaID")
        REFERENCES "Recetas" ("RecetaID") ON DELETE CASCADE,
    CONSTRAINT "FK_IngredientesReceta_Productos" FOREIGN KEY ("ProductoMercaderiaID")
        REFERENCES "Productos" ("ProductoID") ON DELETE RESTRICT
);

-- Índices para IngredientesReceta
CREATE INDEX IF NOT EXISTS "IX_IngredientesReceta_RecetaID" ON "IngredientesReceta" ("RecetaID");
CREATE INDEX IF NOT EXISTS "IX_IngredientesReceta_ProductoMercaderiaID" ON "IngredientesReceta" ("ProductoMercaderiaID");

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
-- Ejecuta estas consultas para verificar que las tablas se crearon correctamente:

-- SELECT * FROM "Recetas";
-- SELECT * FROM "IngredientesReceta";

-- =====================================================
-- EJEMPLO: Crear una receta de prueba
-- =====================================================
-- 
-- 1. Primero, crear la receta:
-- INSERT INTO "Recetas" ("Nombre", "Descripcion", "CategoriaID", "Precio", "Activo")
-- VALUES ('Pizza Muzzarella Grande', 'Pizza con muzzarella', 1, 1800, TRUE);
--
-- 2. Luego, agregar ingredientes (ajustar los IDs según tu base de datos):
-- INSERT INTO "IngredientesReceta" ("RecetaID", "ProductoMercaderiaID", "Cantidad", "UnidadMedida")
-- VALUES (1, 1, 1, 'unidad');  -- 1 pre-pizza
--
-- INSERT INTO "IngredientesReceta" ("RecetaID", "ProductoMercaderiaID", "Cantidad", "UnidadMedida")
-- VALUES (1, 2, 200, 'gramos'); -- 200g muzzarella
