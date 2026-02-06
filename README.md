# Sistema de Gestión de Permisos

Challenge técnico desarrollado para N5 - Posición Sr Full Stack Developer.

Este proyecto implementa una Web API RESTful para la gestión de permisos de empleados, con una arquitectura basada en CQRS, Repository Pattern y Unit of Work. La solución incluye integración con Elasticsearch para indexación de datos y Apache Kafka para mensajería asíncrona.

---

## Stack Tecnológico

**Backend:**
- .NET 9 / ASP.NET Core
- Entity Framework Core 9
- SQL Server 2022
- MediatR (implementación de CQRS)
- Elasticsearch 8.11
- Apache Kafka 7.6
- xUnit (testing)

**Frontend:**
- React 18 con TypeScript
- Material-UI (MUI)
- Axios
- React Hook Form + Yup
- Vite

**Infraestructura:**
- Docker Compose

---

## Arquitectura

El backend está organizado en capas siguiendo Clean Architecture:

```
Domain          → Entidades del negocio y contratos (interfaces)
Infrastructure  → Implementaciones de persistencia, Elasticsearch y Kafka
Application     → Lógica de negocio (Commands, Queries, Handlers)
API             → Controllers y configuración de la aplicación
```

### Patrones Implementados

**CQRS (Command Query Responsibility Segregation):**
- Separación clara entre operaciones de escritura (Commands) y lectura (Queries)
- Cada handler tiene una única responsabilidad

**Repository Pattern + Unit of Work:**
- Abstracción del acceso a datos
- Transacciones coordinadas entre repositorios

**Dependency Injection:**
- Todas las dependencias se inyectan mediante el contenedor de .NET

---

## Funcionalidades

### Endpoints de la API

**POST /api/permissions** - Crear nuevo permiso
```json
{
  "nombreEmpleado": "Juan Carlos",
  "apellidoEmpleado": "Pérez García",
  "tipoPermiso": 1,
  "fechaPermiso": "2026-03-15T10:00:00"
}
```

**PUT /api/permissions/{id}** - Modificar permiso existente
```json
{
  "nombreEmpleado": "Juan Carlos",
  "apellidoEmpleado": "Pérez García",
  "tipoPermiso": 2,
  "fechaPermiso": "2026-03-20T10:00:00"
}
```

**GET /api/permissions** - Obtener todos los permisos
```json
[
  {
    "id": 1,
    "nombreEmpleado": "Juan Carlos",
    "apellidoEmpleado": "Pérez García",
    "tipoPermiso": 1,
    "tipoPermisoDescripcion": "Vacaciones",
    "fechaPermiso": "2026-03-15T10:00:00"
  }
]
```

### Tipos de Permisos

1. Vacaciones
2. Licencia médica
3. Permiso personal
4. Día libre
5. Trabajo remoto

### Frontend

**Panel de Filtros:**
- Búsqueda por nombre o apellido
- Filtro por tipo de permiso
- Filtro por rango de fechas
- Aplicación en tiempo real

**Gestión de Permisos:**
- Modal unificado para crear y editar
- Validación de formularios con Yup
- Manejo robusto de errores con retry automático
- Error Boundary para evitar crashes

---

## Prerequisitos

- Docker y Docker Compose
- .NET SDK 9.0 o superior
- Node.js 18+ y npm
- Git

---

## Instalación y Ejecución

### 1. Clonar el repositorio

```bash
git clone <repository-url>
cd n5_test
```

### 2. Levantar servicios de infraestructura

```bash
docker-compose up -d
```

Esto levanta:
- SQL Server (puerto 1433)
- Elasticsearch (puerto 9200)
- Kafka (puerto 9092)
- Zookeeper (puerto 2181)

**Verificar que estén corriendo:**
```bash
docker ps
```

### 3. Configurar y ejecutar el Backend

```bash
cd backend/N5.Permissions
dotnet restore
```

**Aplicar migraciones a la base de datos:**
```bash
cd src/N5.Permissions.API
dotnet ef database update --project ../N5.Permissions.Infrastructure
```

**Ejecutar la API:**
```bash
dotnet run
```

La API estará disponible en `http://localhost:5057`

Swagger UI: `http://localhost:5057/swagger`

### 4. Ejecutar el Frontend

En otra terminal:

```bash
cd frontend/n5-permissions-app
npm install
npm run dev
```

La aplicación estará disponible en `http://localhost:5173`

---

## Testing

### Tests Unitarios

Verifican la lógica de negocio en los handlers:

```bash
cd backend/N5.Permissions
dotnet test tests/N5.Permissions.UnitTests
```

Cobertura:
- RequestPermissionCommandHandler
- ModifyPermissionCommandHandler
- GetPermissionsQueryHandler

### Tests de Integración

Verifican el comportamiento end-to-end de los endpoints:

```bash
dotnet test tests/N5.Permissions.IntegrationTests
```

---

## Decisiones Técnicas

### ¿Por qué CQRS en una API simple?

Aunque para 3 endpoints podría considerarse over-engineering, implementé CQRS para demostrar conocimiento de patrones enterprise. En un sistema real que escale, esta separación facilita:
- Optimización independiente de lecturas y escrituras
- Testing más granular
- Mayor mantenibilidad

### ¿Por qué SQL Server + Elasticsearch?

SQL Server es la fuente de verdad (ACID, transacciones, relaciones). Elasticsearch funciona como capa de indexación optimizada para búsquedas. Este patrón es común en arquitecturas donde se necesita búsqueda rápida sin sacrificar integridad transaccional.

### ¿Para qué sirve Kafka?

Kafka se utiliza para publicar eventos de cada operación (request, modify, get). Esto permite:
- Auditoría de operaciones
- Comunicación asíncrona con otros servicios
- Event sourcing si se requiere en el futuro
- Replay de eventos para analytics o compliance

### Validaciones en el Frontend

- **Modo Crear:** Valida que la fecha sea futura
- **Modo Editar:** No valida fecha futura (permite corregir permisos históricos)
- Validación de campos obligatorios y longitud mínima

---

## Estructura de la Base de Datos

**Tabla: Permissions**
```sql
Id               INT PRIMARY KEY IDENTITY
NombreEmpleado   NVARCHAR(100)
ApellidoEmpleado NVARCHAR(100)
TipoPermiso      INT (FK a PermissionTypes)
FechaPermiso     DATETIME
```

**Tabla: PermissionTypes**
```sql
Id          INT PRIMARY KEY IDENTITY
Descripcion NVARCHAR(50)
```

---

## Configuración

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=N5PermissionsDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
  },
  "Elasticsearch": {
    "Url": "http://localhost:9200",
    "IndexName": "permissions"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "Topic": "permissions-operations"
  }
}
```

### Frontend (permissionService.ts)

```typescript
const API_BASE_URL = 'http://localhost:5057/api';
```

---

## Troubleshooting

### La API no conecta a SQL Server

Verificar que el contenedor esté corriendo:
```bash
docker ps | grep sqlserver
```

Si no está, levantar con:
```bash
docker-compose up -d sqlserver
```

### Elasticsearch retorna errores

Elasticsearch puede tardar unos segundos en inicializarse. Verificar:
```bash
curl http://localhost:9200
```

### "Código de error: 5" en el frontend

Indica que el frontend no puede conectarse a la API. Verificar:
1. Que la API esté corriendo en el puerto 5057
2. Que no haya problemas de CORS
3. Si aparece el error, usar el botón "Reintentar" en lugar de recargar

---

## Notas Adicionales

- La base de datos se crea automáticamente al ejecutar las migraciones
- Los tipos de permisos se seedean automáticamente en la primera ejecución
- Elasticsearch se configura en modo single-node para desarrollo
- Kafka requiere Zookeeper para coordinar el cluster

---

## Capturas de Pantalla

El frontend incluye:
- Logo de N5 integrado en el header
- Panel de filtros lateral con búsqueda en tiempo real
- Tabla de permisos con acciones de edición
- Modal unificado para crear y editar
- Manejo de errores con reintentos automáticos

---

## Autor

Lucas Brieva
Challenge técnico para N5
Febrero 2026
