# API Pagos de Servicios - ASP.NET Core

## 🚀 Estado: FUNCIONANDO

**URL Base:** `http://localhost:5178`  
**Swagger:** `http://localhost:5178/swagger`  
**BD:** Supabase PostgreSQL

---

## 📋 ENDPOINTS

### **1. Registrar Pago**
```http
POST /api/pagos/registrar
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "tipoServicio": "agua",
  "monto": 45.50,
  "referencia": "C-123456-ABC",
  "fechaVencimiento": "2026-04-30T23:59:59Z",
  "observaciones": "Código cliente: 123456"
}
```

**Servicios válidos:** agua, luz, telefonía, internet, otros

---

### **2. Obtener Pagos del Usuario**
```http
GET /api/pagos/{userId}
```

**Retorna:** Todos los pagos ordenados por fecha (descendente)

---

### **3. Obtener Pagos Pendientes**
```http
GET /api/pagos/pendientes/{userId}
```

**Retorna:** Solo pagos con estado "pendiente" ordenados por fecha de vencimiento

---

### **4. Marcar Pago como Pagado**
```http
PATCH /api/pagos/marcar-pagado/{pagoId}
```

**Actualiza:** estado → "pagado" + fecha_pago

---

### **5. Eliminar Pago Pendiente**
```http
DELETE /api/pagos/{pagoId}
```

**Restricción:** Solo se pueden eliminar pagos en estado "pendiente"

---

## 🔧 ARQUITECTURA (4 Capas)

```
Controllers/
  └── PagosController.cs          (5 endpoints HTTP)
       ↓
Services/
  └── PagosService.cs             (Validaciones + lógica)
       ↓
Repositories/
  └── PagosRepository.cs          (REST API → Supabase)
       ↓
Models/
  └── PagosModels.cs              (DTOs + validaciones)
```

---

## 📊 BASE DE DATOS (Supabase)

**Tabla:** `pagos`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| id | UUID | Identificador único |
| user_id | UUID | Usuario propietario |
| tipo_servicio | TEXT | agua\|luz\|telefonía\|internet\|otros |
| monto | DECIMAL | Cantidad a pagar |
| referencia | TEXT | Ref. del servicio |
| estado | TEXT | pendiente\|pagado |
| fecha_vencimiento | TIMESTAMP | Límite de pago |
| fecha_pago | TIMESTAMP | Cuándo se pagó |
| observaciones | TEXT | Notas |
| created_at | TIMESTAMP | Fecha creación |

---

## 🔐 Seguridad (RLS)

- ✅ Cada usuario solo ve sus pagos  
- ✅ Solo auth users pueden insertar
- ✅ Eliminación solo en estado "pendiente"
- ✅ Validación en API + BD

---

## 📝 Documentación Completa

Ver: [PAGOS_API_DOCUMENTATION.md](PAGOS_API_DOCUMENTATION.md)

---

## ✅ Setup

1. **Crear tabla en Supabase:**
   ```bash
   Ejecutar contenido de: SETUP_SUPABASE.sql
   ```

2. **Ejecutar API:**
   ```bash
   dotnet run
   ```

3. **Probar endpoints:**
   ```bash
   Ver ejemplos en PAGOS_API_DOCUMENTATION.md
   ```

---

**Última actualización:** 10 de Abril 2026
- **n** = Plazo (meses)

---

## 📦 ALMACENAMIENTO

🔴 **Actual:** En memoria (se pierde al reiniciar)  
✅ **Próximo:** Conectar Supabase PostgreSQL

---

## 🔌 CONECTAR SUPABASE (PRÓXIMO PASO)

1. Obtener credenciales Supabase (URL + KEY)
2. Actualizar `appsettings.json`:
   ```json
   "Supabase": {
     "Url": "https://tu-proyecto.supabase.co",
     "Key": "tu-anon-key"
   }
   ```
3. Instalar cliente: `dotnet add package supabase-csharp`
4. Actualizar `PrestamosRepository.cs` con queries Supabase

---

## ✅ CHECKLIST - COMPLETADO

- ✅ Create proyecto .NET 10.0
- ✅ 5 capas (Models, Repository, Service, Controller)
- ✅ Fórmula de amortización francesa
- ✅ 4 endpoints funcionando
- ✅ Validaciones con DataAnnotations
- ✅ CORS habilitado
- ✅ Swagger documentation
- ✅ Almacenamiento en memoria

---

## 🔴 PRÓXIMOS PASOS

1. ✅ Backend API corriendo
2. ⬜ Conectar Supabase PostgreSQL
3. ⬜ Frontend conectado a API
4. ⬜ Deploy a Azure/Railway

---

**Última actualización:** 10 Abril 2026
