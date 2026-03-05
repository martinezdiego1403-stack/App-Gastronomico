# 🧪 TESTS DE PRUEBA - Sandwichería Walterio
## Sistema POS con PostgreSQL

---

## 📋 MÓDULO 1: AUTENTICACIÓN Y CAJA

### Test 1.1: Inicio de sesión
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Abrir la aplicación | Aparece pantalla de login |
| 2 | Ingresar usuario válido y contraseña | Accede al sistema |
| 3 | Ingresar usuario inválido | Muestra mensaje de error |
| 4 | Dejar campos vacíos y presionar login | Muestra validación |

### Test 1.2: Apertura de Caja
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en "Abrir Caja" | Aparece diálogo pidiendo nombre de usuario |
| 2 | Ingresar nombre de usuario válido | ✅ Caja se abre (sin pedir monto inicial) |
| 3 | Verificar barra superior | Muestra "Caja #X abierta" |
| 4 | Intentar abrir otra caja | No permite (ya hay una abierta) |

### Test 1.3: Cierre de Caja
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en "Cerrar Caja" | Aparece ventana de cierre |
| 2 | Verificar totales | Muestra total ventas y monto esperado |
| 3 | Ingresar monto contado igual al esperado | Muestra "✓ Sin diferencias" (NO "caja cuadrada") |
| 4 | Ingresar monto diferente | Muestra sobrante o faltante |
| 5 | Confirmar cierre | Envía resumen por WhatsApp (sin "monto inicial" ni "caja cuadrada") |

---

## 📋 MÓDULO 2: MENÚ (Productos para venta)

### Test 2.1: Ver listado de productos
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a módulo "Menú" | Muestra lista de productos del menú |
| 2 | Verificar que NO aparezcan productos de Mercadería | Solo productos con categoría tipo "Menu" |
| 3 | Verificar formato de stock | Stock muestra "50" (NO "50.000") |

### Test 2.2: Crear producto en Menú
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en "➕ Producto" | Abre formulario de nuevo producto |
| 2 | Completar datos y guardar | Producto aparece en Menú |
| 3 | Verificar en Mercadería | El producto NO aparece en Mercadería |
| 4 | Crear nueva categoría | La categoría se crea con TipoCategoria="Menu" |

### Test 2.3: Editar producto
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en ✏️ de un producto | Abre formulario con datos cargados |
| 2 | Modificar precio y stock | Cambios se guardan correctamente |
| 3 | Verificar en lista | Los cambios se reflejan inmediatamente |
| 4 | NO se duplica el producto | Sigue siendo un único registro |

### Test 2.4: Filtros y búsqueda
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Escribir en buscador | Filtra por nombre en tiempo real |
| 2 | Seleccionar categoría | Filtra por categoría seleccionada |
| 3 | Limpiar filtros | Muestra todos los productos |

---

## 📋 MÓDULO 3: MERCADERÍA (Insumos/Ingredientes)

### Test 3.1: Ver listado de mercadería
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a módulo "Mercadería" | Muestra lista de mercadería |
| 2 | Verificar que NO aparezcan productos de Menú | Solo productos con categoría tipo "Mercaderia" |
| 3 | Verificar formato de stock | Stock muestra "60" (NO "60.000") |

### Test 3.2: Crear mercadería
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en "➕ Nueva Mercadería" | Abre formulario |
| 2 | Completar: Muzzarella, 3 Kg, precio $5000 | Mercadería se guarda |
| 3 | Verificar en Menú | La mercadería NO aparece en Menú |
| 4 | Crear nueva categoría | La categoría se crea con TipoCategoria="Mercaderia" |

### Test 3.3: Editar mercadería
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en ✏️ de una mercadería | Abre formulario con datos |
| 2 | Cambiar Stock Actual de 60 a 80 | Se guarda correctamente |
| 3 | Verificar en lista | Muestra "80" (NO "80.000" ni "0.000") |
| 4 | NO se duplica el producto | Sigue siendo un único registro |

---

## 📋 MÓDULO 4: RECETAS

### Test 4.1: Crear receta
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a Recetas → Nueva Receta | Abre formulario |
| 2 | Nombre: "Pizza Común", Precio: $8000 | Datos básicos |
| 3 | Agregar ingrediente: 1 Unidad de Prepizza | Se agrega a la lista |
| 4 | Agregar ingrediente: 300 Gramo de Muzzarella | Se agrega a la lista |
| 5 | Guardar receta | Receta se crea exitosamente |
| 6 | Verificar en Menú | La receta aparece disponible para vender |

### Test 4.2: Ver ingredientes de receta
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en botón morado (ver ingredientes) | Muestra ventana de ingredientes |
| 2 | Verificar formato | Muestra "1 Unidad de Prepizza" (NO "1,000 unidad") |
| 3 | Verificar stock | Muestra "Stock disponible: 59" (NO "59,000") |

### Test 4.3: Editar receta
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Click en ✏️ de una receta | Abre formulario con ingredientes cargados |
| 2 | Modificar cantidad de un ingrediente | Se actualiza |
| 3 | Agregar nuevo ingrediente | Se agrega sin error |
| 4 | Guardar | ✅ NO muestra error de Entity Framework |

---

## 📋 MÓDULO 5: PUNTO DE VENTA

### Test 5.1: Vender producto simple
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a "Ventas" | Muestra productos y recetas disponibles |
| 2 | Click en una Coca Cola | Se agrega al carrito |
| 3 | Verificar total | Muestra precio correcto |
| 4 | Seleccionar método: Efectivo | Se marca efectivo |
| 5 | Click en "Cobrar" | ✅ Venta se registra sin errores |

### Test 5.2: Vender receta (conversión de unidades)
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Stock inicial Muzzarella: 3 Kg | Verificar en Mercadería |
| 2 | Vender 1 Pizza Común | Receta necesita 300g de Muzzarella |
| 3 | Confirmar venta | ✅ Venta exitosa (sin error de UnidadMedidaService) |
| 4 | Verificar stock Muzzarella | Stock: 2.7 Kg (se descontó 0.3 Kg = 300g) |
| 5 | Verificar stock Prepizza | Stock: 59 (se descontó 1 unidad) |
| 6 | Verificar stock Receta | Stock de la receta disminuyó en 1 |

### Test 5.3: Vender múltiples recetas
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Agregar 2 Pizzas Comunes al carrito | Cantidad: 2 |
| 2 | Cobrar | Venta exitosa |
| 3 | Verificar descuento | Muzzarella: -600g (0.6 Kg), Prepizza: -2 unidades |

### Test 5.4: Cálculo de vuelto
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Total de venta: $8000 | Verificar total |
| 2 | Ingresar monto recibido: $10000 | Calcular vuelto |
| 3 | Verificar vuelto | Muestra "$2000" |

---

## 📋 MÓDULO 6: REPORTES

### Test 6.1: Generar reporte de ventas
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a "Reportes" | Muestra opciones de reportes |
| 2 | Seleccionar rango de fechas | Filtra ventas |
| 3 | Exportar a Excel | Genera archivo .xlsx |

### Test 6.2: Reporte de stock bajo
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ver productos con stock bajo | Lista productos debajo del mínimo |
| 2 | Verificar alertas | Productos en rojo si stock ≤ mínimo |

---

## 📋 MÓDULO 7: WHATSAPP

### Test 7.1: Configuración
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a configuración WhatsApp | Muestra formulario |
| 2 | Ingresar número y habilitar | Se guarda configuración |

### Test 7.2: Envío automático al cerrar caja
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Cerrar caja con WhatsApp habilitado | Se envía mensaje |
| 2 | Verificar mensaje | NO contiene "monto inicial" ni "caja cuadrada" |
| 3 | Verificar contenido | Contiene: cantidad ventas, total, monto contado |

---

## 📋 MÓDULO 8: USUARIOS

### Test 8.1: Crear usuario
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a "Usuarios" → Nuevo | Abre formulario |
| 2 | Completar datos | Usuario se crea |
| 3 | Asignar rol | Rol se guarda correctamente |

### Test 8.2: Cambiar contraseña
| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Seleccionar usuario → Cambiar contraseña | Abre diálogo |
| 2 | Ingresar nueva contraseña | Se actualiza |
| 3 | Login con nueva contraseña | Acceso exitoso |

---

## 🔴 CASOS DE ERROR A VERIFICAR

| # | Escenario | Resultado Esperado |
|---|-----------|-------------------|
| 1 | Vender sin caja abierta | Mensaje "Debe abrir caja" |
| 2 | Editar producto y guardar | NO se duplica el producto |
| 3 | Editar receta con ingredientes | NO error de Entity Framework |
| 4 | Cobrar venta | NO error de UnidadMedidaService |
| 5 | Stock de mercadería en lista | NO muestra "0.000" después de editar |
| 6 | Stock en Menú | Muestra "50" no "50.000" |
| 7 | Ingredientes de receta | Muestra "300 Gramo" no "300,000 gramos" |

---

## ✅ CHECKLIST FINAL

- [ ] Login funciona correctamente
- [ ] Apertura de caja sin monto inicial
- [ ] Productos de Menú NO aparecen en Mercadería
- [ ] Productos de Mercadería NO aparecen en Menú
- [ ] Editar producto NO lo duplica
- [ ] Editar receta NO da error de Entity Framework
- [ ] Vender receta NO da error de UnidadMedidaService
- [ ] Conversión de unidades funciona (300g → 0.3Kg)
- [ ] Stock se muestra sin decimales innecesarios
- [ ] Cierre de caja NO dice "caja cuadrada"
- [ ] Mensaje WhatsApp NO incluye "monto inicial"

---

*Documento generado el 12/02/2026 para Sandwichería Walterio v2.0*
